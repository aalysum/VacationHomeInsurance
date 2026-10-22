using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using VacationHome.PolicyService.Application.Configuration;
using VacationHome.PolicyService.Application.DTOs.Documents;
using VacationHome.PolicyService.Application.Exceptions;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;


namespace VacationHome.PolicyService.Infrastructure.Services
{
    public sealed class PolicyDocumentService : IPolicyDocumentService
    {
        private const string PdfContentType = "application/pdf";
        private readonly IPolicyRepository _policyRepository;
        private readonly IPolicyDocumentRepository _documentRepository;
        private readonly IPolicyDocumentFileStore _fileStore;
        private readonly IPolicyPdfGenerator _pdfGenerator;
        private readonly PolicyDocumentOptions _options;
        private readonly ILogger<PolicyDocumentService> _logger;

        public PolicyDocumentService(
            IPolicyRepository policyRepository,
            IPolicyDocumentRepository documentRepository,
            IPolicyDocumentFileStore fileStore,
            IPolicyPdfGenerator pdfGenerator,
            Microsoft.Extensions.Options.IOptions<PolicyDocumentOptions> options,
            ILogger<PolicyDocumentService> logger)
        {
            _policyRepository = policyRepository;
            _documentRepository = documentRepository;
            _fileStore = fileStore;
            _pdfGenerator = pdfGenerator;
            _options = options.Value;
            _logger = logger;
        }

        public Task<PolicyDocumentDto> EnsureLatestGeneratedAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            return GenerateOrRecoverAsync(
                policyId,
                forceNewVersion: false,
                cancellationToken);
        }

        public Task<PolicyDocumentDto> RegenerateForAdminAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            return GenerateOrRecoverAsync(
                policyId,
                forceNewVersion: true,
                cancellationToken);
        }

        public async Task<IReadOnlyList<PolicyDocumentDto>>
            GetForCustomerAsync(
                Guid policyId,
                Guid customerId,
                CancellationToken cancellationToken)
        {
            Policy policy = await GetPolicyAsync(
                policyId,
                cancellationToken);

            if (policy.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "Access denied to this policy document.");
            }

            return (await _documentRepository.GetAllAsync(
                    policyId,
                    cancellationToken))
                .Select(ToDto)
                .ToArray();
        }

        public async Task<IReadOnlyList<PolicyDocumentDto>>
            GetForAdminAsync(
                Guid policyId,
                CancellationToken cancellationToken)
        {
            await GetPolicyAsync(policyId, cancellationToken);
            return (await _documentRepository.GetAllAsync(
                    policyId,
                    cancellationToken))
                .Select(ToDto)
                .ToArray();
        }

        public async Task<PolicyDocumentDownload?> DownloadForCustomerAsync(
            Guid policyId,
            Guid documentId,
            Guid customerId,
            CancellationToken cancellationToken)
        {
            Policy policy = await GetPolicyAsync(
                policyId,
                cancellationToken);

            if (policy.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "Access denied to this policy document.");
            }

            return await DownloadAsync(
                policy,
                documentId,
                cancellationToken);
        }

        public async Task<PolicyDocumentDownload?> DownloadForAdminAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            Policy policy = await GetPolicyAsync(
                policyId,
                cancellationToken);
            return await DownloadAsync(
                policy,
                documentId,
                cancellationToken);
        }

        public Task<PolicyDocumentDownload?> DownloadForInternalServiceAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            return DownloadForAdminAsync(
                policyId,
                documentId,
                cancellationToken);
        }

        private async Task<PolicyDocumentDto> GenerateOrRecoverAsync(
            Guid policyId,
            bool forceNewVersion,
            CancellationToken cancellationToken)
        {
            await using IAsyncDisposable policyLock =
                await _fileStore.AcquirePolicyLockAsync(
                    policyId,
                    cancellationToken);

            Policy policy = await GetPolicyAsync(
                policyId,
                cancellationToken);
            PolicyDocument? latest =
                await _documentRepository.GetLatestAsync(
                    policyId,
                    cancellationToken);

            if (latest is not null &&
                latest.Status == PolicyDocumentGenerationStatus.Available)
            {
                if (!forceNewVersion && await IsStoredDocumentValidAsync(
                        latest,
                        cancellationToken))
                {
                    return ToDto(latest);
                }

                if (forceNewVersion)
                {
                    await IsStoredDocumentValidAsync(
                        latest,
                        cancellationToken);
                }
                else
                {
                    await MarkFailedAsync(
                        latest,
                        "DOCUMENT_FILE_UNAVAILABLE",
                        cancellationToken);
                    if (!string.IsNullOrWhiteSpace(latest.StorageKey))
                    {
                        await _fileStore.QuarantineAsync(
                            latest.StorageKey,
                            cancellationToken);
                    }
                }
            }
            else if (latest is not null)
            {
                if (await TryAdoptFileAsync(
                        latest,
                        cancellationToken))
                {
                    return ToDto(latest);
                }

                await MarkFailedAsync(
                    latest,
                    "GENERATION_INCOMPLETE",
                    cancellationToken);
                if (!string.IsNullOrWhiteSpace(latest.StorageKey))
                {
                    await _fileStore.QuarantineAsync(
                        latest.StorageKey,
                        cancellationToken);
                }
            }

            int version = latest is null
                ? 1
                : checked(latest.Version + 1);
            string storageKey =
                _fileStore.CreateStorageKey(policyId, version);

            if (await TryAdoptOrphanAsync(
                    policy,
                    version,
                    storageKey,
                    cancellationToken) is { } adopted)
            {
                return adopted;
            }

            var document = new PolicyDocument
            {
                Id = Guid.NewGuid(),
                PolicyId = policyId,
                Version = version,
                StorageKey = storageKey,
                ContentType = PdfContentType,
                Status = PolicyDocumentGenerationStatus.Generating,
                CreatedAtUtc = DateTime.UtcNow
            };

            try
            {
                await _documentRepository.AddAsync(
                    document,
                    cancellationToken);
                await _documentRepository.SaveChangesAsync(
                    cancellationToken);

                byte[] pdf = _pdfGenerator.Generate(
                    policy,
                    version,
                    DateTime.UtcNow,
                    _options);
                ValidatePdf(pdf);

                await _fileStore.WriteAtomicallyAsync(
                    storageKey,
                    pdf,
                    cancellationToken);

                document.FileSizeBytes = pdf.LongLength;
                document.Sha256Checksum = ComputeChecksum(pdf);
                document.Status = PolicyDocumentGenerationStatus.Available;
                document.GeneratedAtUtc = DateTime.UtcNow;
                await _documentRepository.UpdateAsync(
                    document,
                    cancellationToken);
                await _documentRepository.SaveChangesAsync(
                    cancellationToken);

                return ToDto(document);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Policy document generation failed for policy {PolicyId}, document {DocumentId}, version {Version}",
                    policyId,
                    document.Id,
                    version);

                document.Status = PolicyDocumentGenerationStatus.Failed;
                document.FailureCode =
                    PolicyDocumentGenerationException.Code;
                try
                {
                    await _documentRepository.UpdateAsync(
                        document,
                        cancellationToken);
                    await _documentRepository.SaveChangesAsync(
                        cancellationToken);
                }
                catch (Exception persistenceException)
                {
                    _logger.LogError(
                        persistenceException,
                        "Unable to persist failed generation status for policy {PolicyId}, document {DocumentId}",
                        policyId,
                        document.Id);
                }

                throw new PolicyDocumentGenerationException(exception);
            }
        }

        private async Task<PolicyDocumentDownload?> DownloadAsync(
            Policy policy,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            PolicyDocument? document =
                await _documentRepository.GetByIdAsync(
                    policy.Id,
                    documentId,
                    cancellationToken);

            if (document is null)
            {
                return null;
            }

            if (document.Status != PolicyDocumentGenerationStatus.Available ||
                string.IsNullOrWhiteSpace(document.StorageKey))
            {
                throw new NotFoundException(
                    "Policy document is not available.");
            }

            Stream? stream;
            try
            {
                stream = await _fileStore.OpenReadAsync(
                    document.StorageKey,
                    cancellationToken);
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(
                    exception,
                    "Unable to open policy document {DocumentId} for policy {PolicyId}",
                    document.Id,
                    policy.Id);
                await MarkFailedAsync(
                    document,
                    "DOCUMENT_FILE_UNAVAILABLE",
                    cancellationToken);
                throw new NotFoundException(
                    "The policy document file is unavailable.");
            }

            if (stream is null)
            {
                await MarkFailedAsync(
                    document,
                    "DOCUMENT_FILE_MISSING",
                    cancellationToken);
                throw new NotFoundException(
                    "The policy document file is unavailable. An Administrator can retry generation.");
            }

            byte[] contents;
            try
            {
                await using (stream)
                using (var memory = new MemoryStream())
                {
                    await stream.CopyToAsync(
                        memory,
                        cancellationToken);
                    contents = memory.ToArray();
                }
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(
                    exception,
                    "Unable to read policy document {DocumentId} for policy {PolicyId}",
                    document.Id,
                    policy.Id);
                await MarkFailedAsync(
                    document,
                    "DOCUMENT_FILE_UNAVAILABLE",
                    cancellationToken);
                throw new NotFoundException(
                    "The policy document file is unavailable.");
            }

            if (!IsPdf(contents) ||
                !string.Equals(
                    ComputeChecksum(contents),
                    document.Sha256Checksum,
                    StringComparison.OrdinalIgnoreCase))
            {
                await MarkFailedAsync(
                    document,
                    "DOCUMENT_CHECKSUM_MISMATCH",
                    cancellationToken);
                throw new NotFoundException(
                    "The policy document failed its integrity check.");
            }

            return new PolicyDocumentDownload(
                new MemoryStream(contents, writable: false),
                $"Policy-{SanitizeFileSegment(policy.PolicyNumber)}_Version_{document.Version}.pdf",
                PdfContentType);
        }

        private async Task<bool> IsStoredDocumentValidAsync(
            PolicyDocument document,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(document.StorageKey))
            {
                return false;
            }

            byte[]? bytes = await _fileStore.ReadAllBytesAsync(
                document.StorageKey,
                cancellationToken);

            return bytes is not null &&
                   IsPdf(bytes) &&
                   string.Equals(
                       ComputeChecksum(bytes),
                       document.Sha256Checksum,
                       StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> TryAdoptFileAsync(
            PolicyDocument document,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(document.StorageKey))
            {
                return false;
            }

            byte[]? bytes = await _fileStore.ReadAllBytesAsync(
                document.StorageKey,
                cancellationToken);
            if (bytes is null || !IsPdf(bytes))
            {
                return false;
            }

            string checksum = ComputeChecksum(bytes);
            if (!string.IsNullOrWhiteSpace(document.Sha256Checksum) &&
                !string.Equals(
                    checksum,
                    document.Sha256Checksum,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            document.FileSizeBytes = bytes.LongLength;
            document.Sha256Checksum = checksum;
            document.ContentType = PdfContentType;
            document.Status = PolicyDocumentGenerationStatus.Available;
            document.FailureCode = null;
            document.GeneratedAtUtc ??= DateTime.UtcNow;
            await _documentRepository.UpdateAsync(
                document,
                cancellationToken);
            await _documentRepository.SaveChangesAsync(
                cancellationToken);
            return true;
        }

        private async Task<PolicyDocumentDto?> TryAdoptOrphanAsync(
            Policy policy,
            int version,
            string storageKey,
            CancellationToken cancellationToken)
        {
            byte[]? bytes = await _fileStore.ReadAllBytesAsync(
                storageKey,
                cancellationToken);
            if (bytes is null)
            {
                return null;
            }

            if (!IsPdf(bytes))
            {
                await _fileStore.QuarantineAsync(
                    storageKey,
                    cancellationToken);
                return null;
            }

            var document = new PolicyDocument
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                Version = version,
                StorageKey = storageKey,
                ContentType = PdfContentType,
                FileSizeBytes = bytes.LongLength,
                Sha256Checksum = ComputeChecksum(bytes),
                Status = PolicyDocumentGenerationStatus.Available,
                CreatedAtUtc = DateTime.UtcNow,
                GeneratedAtUtc = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(
                document,
                cancellationToken);
            await _documentRepository.SaveChangesAsync(
                cancellationToken);
            return ToDto(document);
        }

        private async Task MarkFailedAsync(
            PolicyDocument document,
            string failureCode,
            CancellationToken cancellationToken)
        {
            document.Status = PolicyDocumentGenerationStatus.Failed;
            document.FailureCode = failureCode;
            await _documentRepository.UpdateAsync(
                document,
                cancellationToken);
            await _documentRepository.SaveChangesAsync(
                cancellationToken);
        }

        private async Task<Policy> GetPolicyAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            if (policyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Policy identifier is required.");
            }

            return await _policyRepository.GetByIdAsync(
                       policyId,
                       false,
                       cancellationToken)
                   ?? throw new NotFoundException(
                       "Policy not found.");
        }

        private static PolicyDocumentDto ToDto(
            PolicyDocument document)
        {
            return new PolicyDocumentDto(
                document.Id,
                document.PolicyId,
                document.Version,
                document.Status.ToString(),
                document.ContentType,
                document.FileSizeBytes,
                document.Sha256Checksum,
                document.FailureCode,
                document.CreatedAtUtc,
                document.GeneratedAtUtc);
        }

        private static void ValidatePdf(byte[] bytes)
        {
            if (!IsPdf(bytes))
            {
                throw new InvalidDataException(
                    "The generated file is not a valid PDF document.");
            }
        }

        private static bool IsPdf(byte[] bytes)
        {
            return bytes.Length >= 5 &&
                   bytes[0] == (byte)'%' &&
                   bytes[1] == (byte)'P' &&
                   bytes[2] == (byte)'D' &&
                   bytes[3] == (byte)'F' &&
                   bytes[4] == (byte)'-';
        }

        private static string ComputeChecksum(byte[] bytes)
        {
            return Convert.ToHexString(SHA256.HashData(bytes));
        }

        private static string SanitizeFileSegment(string value)
        {
            string sanitized = new(
                value.Where(char.IsLetterOrDigit).ToArray());
            return string.IsNullOrWhiteSpace(sanitized)
                ? "Policy"
                : sanitized;
        }
    }
}