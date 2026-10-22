using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Domain.Entities;
using VacationHome.ClaimService.Domain.Enums;
using Microsoft.AspNetCore.Http;




namespace VacationHome.ClaimService.Application.Services
{
    public sealed class ClaimDocumentService
        : IClaimDocumentService
    {
        private readonly IClaimRepository
            _claimRepository;
        private readonly ICustomerIdentityService
            _customerIdentityService;


        private readonly IClaimDocumentRepository
            _documentRepository;


        private readonly ICurrentUserService
            _currentUserService;


        private readonly IAuditService
            _auditService;


        private readonly IUnitOfWork
            _unitOfWork;


        public ClaimDocumentService(
            IClaimRepository claimRepository,
            IClaimDocumentRepository documentRepository,
            ICurrentUserService currentUserService,
            ICustomerIdentityService customerIdentityService,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _claimRepository =
                claimRepository;


            _documentRepository =
                documentRepository;


            _currentUserService =
                currentUserService;


            _customerIdentityService =
                customerIdentityService;


            _auditService =
                auditService;


            _unitOfWork =
                unitOfWork;
        }






        public async Task<ClaimDocumentDto>
            AddDocumentAsync(
                Guid claimId,
                ClaimDocumentType documentType,
                IFormFile file,
                CancellationToken cancellationToken = default)
        {
            if (claimId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Claim identifier is required.");
            }


            if (file is null ||
                file.Length == 0)
            {
                throw new BadRequestException(
                    "A document file is required.");
            }


            const long maximumFileSize =
                10 * 1024 * 1024;


            if (file.Length > maximumFileSize)
            {
                throw new BadRequestException(
                    "The document cannot exceed 10 MB.");
            }


            string[] permittedContentTypes =
            {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };


            if (!permittedContentTypes.Contains(
                    file.ContentType,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new BadRequestException(
                    "Only PDF, JPEG, and PNG documents are supported.");
            }


            string extension =
                Path.GetExtension(
                    file.FileName)
                .ToLowerInvariant();


            string[] permittedExtensions =
            {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    };


            if (!permittedExtensions.Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new BadRequestException(
                    "The document has an unsupported file extension.");
    }


    Claim claim =
        await _claimRepository.GetByIdAsync(
            claimId,
            true,
            cancellationToken)
        ?? throw new NotFoundException(
            "Claim not found.");


    await EnsureCustomerOwnsClaimAsync(
        claim,
        cancellationToken);


    if (claim.Status is
        ClaimStatus.Closed or
        ClaimStatus.Rejected or
        ClaimStatus.Withdrawn)
    {
        throw new BadRequestException(
            "Documents cannot be uploaded for a closed, rejected, or withdrawn Claim.");
    }


    string storedFileName =
        $"{Guid.NewGuid():N}{extension}";


    string claimDirectory =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads",
            "Claims",
            claimId.ToString("N"));


    Directory.CreateDirectory(
        claimDirectory);


    string physicalPath =
        Path.Combine(
            claimDirectory,
            storedFileName);


    string storagePath =
        Path.Combine(
            "Uploads",
            "Claims",
            claimId.ToString("N"),
            storedFileName)
        .Replace(
            '\\',
            '/');


    bool fileSaved =
        false;


    try
    {
        await using (
            FileStream stream =
                new(
                    physicalPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
        {
            await file.CopyToAsync(
                stream,
                cancellationToken);
        }


        fileSaved =
            true;


        var document =
            new ClaimDocument
            {
                Id =
                    Guid.NewGuid(),


                ClaimId =
                    claimId,


                DocumentType =
                    documentType,


                FileName =
                    Path.GetFileName(
                        file.FileName),


                ContentType =
                    file.ContentType,


                StoragePath =
                    storagePath,


                FileSizeBytes =
                    file.Length,


                UploadedAtUtc =
                    DateTime.UtcNow,


                UploadedBy =
                    _currentUserService.Email
            };


        await _documentRepository.AddAsync(
            document,
            cancellationToken);


                if (
                    claim.Status ==
                    ClaimStatus.AdditionalInformationRequested
                )
                {
                    DateTime utcNow = DateTime.UtcNow;
                    ClaimStatus previousStatus = claim.Status;
                    claim.Status =
                        ClaimStatus.UnderReview;


                    claim.LastModifiedAtUtc =
                        utcNow;
                    claim.LastModifiedBy =
                    _currentUserService.Email;

                    _claimRepository.AddStatusHistory(
                        new ClaimStatusHistory
                        {
                            Id = Guid.NewGuid(),
                            ClaimId = claim.Id,
                            PreviousStatus = previousStatus,
                            NewStatus = ClaimStatus.UnderReview,
                            Comments = "Requested documents received; claim returned to review.",
                            ChangedBy = _currentUserService.Email,
                            ChangedAtUtc = utcNow
                        });


                    await _claimRepository.UpdateAsync(
                    claim,
                    cancellationToken);
                }






                await _auditService.LogAsync(
                    nameof(Claim),
                    claim.Id.ToString(),
                    "DOCUMENT_UPLOADED",
                    document.FileName,
                    cancellationToken);


                Console.WriteLine(
                    $"Claim Status Before Save: {claim.Status}");


                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);


                return ToDocumentDto(
            document);
    }
    catch
    {
        if (
            fileSaved &&
            File.Exists(
                physicalPath))
        {
            try
            {
                File.Delete(
                    physicalPath);
            }
            catch
            {
                /*
                 * Do not hide the original upload
                 * or database exception if cleanup
                 * also fails.
                 */
            }
        }


        throw;
    }
}


        public async Task<
            IReadOnlyCollection<ClaimDocumentDto>>
            GetDocumentsAsync(
                Guid claimId,
                CancellationToken cancellationToken =
                    default)
        {
            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim not found.");


            /*
             * Claim owner, Admin, and Surveyor can view
             * the documents.
             */
            await EnsureDocumentReadAccessAsync(
                claim,
                cancellationToken);


            IReadOnlyCollection<ClaimDocument>
                documents =
                    await _documentRepository
                        .GetByClaimIdAsync(
                            claimId,
                            cancellationToken);


            return documents
                .Select(ToDocumentDto)
                .ToArray();
        }


        public async Task<ClaimDocumentDto>
            GetDocumentAsync(
                Guid claimId,
                Guid documentId,
                CancellationToken cancellationToken =
                    default)
        {
            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim not found.");


            await EnsureDocumentReadAccessAsync(
                claim,
                cancellationToken);


            ClaimDocument? document =
                await _documentRepository.GetByIdAsync(
                    documentId,
                    false,
                    cancellationToken);


            if (document is null ||
                document.ClaimId != claimId)
            {
                throw new NotFoundException(
                    "Document not found.");
            }


            return ToDocumentDto(document);
        }

        public async Task<(Stream Content, string ContentType, string FileName)>
            GetDocumentContentAsync(
                Guid claimId,
                Guid documentId,
                CancellationToken cancellationToken = default)
        {
            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim not found.");

            await EnsureDocumentReadAccessAsync(
                claim,
                cancellationToken);

            ClaimDocument? document =
                await _documentRepository.GetByIdAsync(
                    documentId,
                    false,
                    cancellationToken);

            if (document is null || document.ClaimId != claimId)
            {
                throw new NotFoundException(
                    "Document not found.");
            }

            string storageRoot =
                Path.GetFullPath(
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Uploads",
                        "Claims"));
            string physicalPath =
                Path.GetFullPath(
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        document.StoragePath));
            string relativePath =
                Path.GetRelativePath(
                    storageRoot,
                    physicalPath);

            if (
                relativePath.StartsWith("..", StringComparison.Ordinal) ||
                Path.IsPathRooted(relativePath)
            )
            {
                throw new BadRequestException(
                    "The document storage reference is invalid.");
            }

            if (!File.Exists(physicalPath))
            {
                throw new NotFoundException(
                    "The stored document file is unavailable.");
            }

            Stream content =
                new FileStream(
                    physicalPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    useAsync: true);

            return (
                content,
                document.ContentType,
                Path.GetFileName(document.FileName));
        }


        public async Task DeleteDocumentAsync(
            Guid claimId,
            Guid documentId,
            CancellationToken cancellationToken =
                default)
        {
            Claim claim =
                await _claimRepository.GetByIdAsync(
                    claimId,
                    false,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Claim not found.");


            /*
             * Only the claim-owning customer can delete a
             * document using this endpoint.
             */
            await EnsureCustomerOwnsClaimAsync(
                claim,
                cancellationToken);


            ClaimDocument? document =
                await _documentRepository.GetByIdAsync(
                    documentId,
                    true,
                    cancellationToken);


            if (document is null ||
                document.ClaimId != claimId)
            {
                throw new NotFoundException(
                    "Document not found.");
            }


            await _documentRepository.DeleteAsync(
                document,
                cancellationToken);


            await _auditService.LogAsync(
                nameof(Claim),
                claim.Id.ToString(),
                "DOCUMENT_DELETED",
                document.FileName,
                cancellationToken);


            /*
             * Save the deletion and audit log together.
             */
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        private async Task EnsureCustomerOwnsClaimAsync(
            Claim claim,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(
                    _currentUserService.Role,
                    "Customer",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenException(
                    "Only the Claim-owning Customer can perform this operation.");
            }


            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            if (claim.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "Access denied.");
            }
        }


        private async Task EnsureDocumentReadAccessAsync(
            Claim claim,
            CancellationToken cancellationToken)
        {
            bool isAdmin =
                string.Equals(
                    _currentUserService.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);


            bool isClaimsManager =
                string.Equals(
                    _currentUserService.Role,
                    "ClaimsManager",
                    StringComparison.OrdinalIgnoreCase);


            bool isSurveyor =
                string.Equals(
                    _currentUserService.Role,
                    "Surveyor",
                    StringComparison.OrdinalIgnoreCase);


            if (isAdmin || isSurveyor)
            {
                return;
            }

            if (isClaimsManager)
            {
                string authUserId =
                    _currentUserService.AuthUserId.Trim();

                if (string.IsNullOrWhiteSpace(authUserId))
                {
                    throw new UnauthorizedException(
                        "Authenticated Claims Manager identifier is missing.");
                }

                if (
                    string.IsNullOrWhiteSpace(
                        claim.AssignedClaimsManagerId) ||
                    !string.Equals(
                        claim.AssignedClaimsManagerId.Trim(),
                        authUserId,
                        StringComparison.Ordinal)
                )
                {
                    throw new ForbiddenException(
                        "Only the assigned Claims Manager can view Claim documents.");
                }

                return;
            }


            bool isCustomer =
                string.Equals(
                    _currentUserService.Role,
                    "Customer",
                    StringComparison.OrdinalIgnoreCase);


            if (!isCustomer)
            {
                throw new ForbiddenException(
                    "Access denied.");
            }


            Guid customerId =
                await _customerIdentityService
                    .GetCurrentCustomerIdAsync(
                        cancellationToken);


            if (claim.CustomerId != customerId)
            {
                throw new ForbiddenException(
                    "Access denied.");
            }
        }


        private static ClaimDocumentDto ToDocumentDto(
            ClaimDocument document)
        {
            return new ClaimDocumentDto
            {
                Id = document.Id,


                DocumentType =
                    document.DocumentType,


                FileName =
                    document.FileName,


                ContentType =
                    document.ContentType,


                FileSizeBytes =
                    document.FileSizeBytes,


                UploadedAtUtc =
                    document.UploadedAtUtc
            };
        }
    }
}
