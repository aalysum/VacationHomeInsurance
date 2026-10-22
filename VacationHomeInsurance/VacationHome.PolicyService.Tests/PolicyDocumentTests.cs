using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PdfSharp.Pdf.IO;
using VacationHome.PolicyService.API.Controllers;
using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.Configuration;
using VacationHome.PolicyService.Application.DTOs.Documents;
using VacationHome.PolicyService.Application.DTOs.Policies;
using VacationHome.PolicyService.Application.Exceptions;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;
using VacationHome.PolicyService.Infrastructure.Persistence;
using VacationHome.PolicyService.Infrastructure.Services;
using Xunit;

namespace VacationHome.PolicyService.Tests;

public sealed class PolicyDocumentTests
{
    private static readonly Guid PolicyId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid CustomerId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void PdfGenerator_RequiresInsurerAndPolicyWording()
    {
        var generator = new PolicyPdfGenerator();

        Assert.Throws<InvalidOperationException>(() =>
            generator.Generate(
                new Policy { PolicyNumber = "POL-1" },
                1,
                DateTime.UtcNow,
                new PolicyDocumentOptions()));
    }

    [Fact]
    public void PdfGenerator_CreatesPdfFromPolicyAndConfiguredTerms()
    {
        var generator = new PolicyPdfGenerator();
        var policy = new Policy
        {
            PolicyNumber = "POL-1",
            CustomerFirstName = "Jane",
            CustomerLastName = "Customer",
            PropertyName = "Lake House",
            AnnualPremium = 1200m,
            EffectiveDateUtc = DateTime.UtcNow.Date,
            ExpiryDateUtc = DateTime.UtcNow.Date.AddYears(1),
            Coverages =
            {
                new PolicyCoverage
                {
                    CoverageId = Guid.NewGuid(),
                    CoverageName = "Dwelling",
                    CoverageDescription = "Configured coverage snapshot",
                    CoverageLimitAmount = 250000m
                }
            }
        };

        byte[] pdf = generator.Generate(
            policy,
            1,
            DateTime.UtcNow,
            new PolicyDocumentOptions
            {
                ProviderName = "Configured Insurer",
                PolicyWording = "Sample test wording."
            });

        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdf));
        using var generatedDocument = PdfReader.Open(
            new MemoryStream(pdf),
            PdfDocumentOpenMode.Import);
        Assert.NotEmpty(generatedDocument.Pages);
        Assert.Equal(595, generatedDocument.Pages[0].Width.Point, precision: 1);
    }

    [Fact]
    public void PdfGenerator_PaginatesLongConfiguredPolicyWording()
    {
        var generator = new PolicyPdfGenerator();
        var policy = new Policy
        {
            PolicyNumber = "POL-PAGINATION",
            CustomerFirstName = "Jane",
            CustomerLastName = "Customer"
        };

        byte[] pdf = generator.Generate(
            policy,
            1,
            DateTime.UtcNow,
            new PolicyDocumentOptions
            {
                ProviderName = "Configured Insurer",
                PolicyWording = string.Join(
                    ' ',
                    Enumerable.Repeat("Configured policy wording.", 900))
            });

        using var generatedDocument = PdfReader.Open(
            new MemoryStream(pdf),
            PdfDocumentOpenMode.Import);
        Assert.True(generatedDocument.PageCount > 1);
    }

    [Fact]
    public async Task CustomerDocumentAccess_RejectsNonOwner()
    {
        PolicyDocumentService service = CreateService(
            new FakeDocumentRepository(),
            new FakeFileStore(),
            new FakePdfGenerator());

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GetForCustomerAsync(
                PolicyId,
                Guid.NewGuid(),
                CancellationToken.None));
    }

    [Fact]
    public async Task MissingStoredFile_MarksDocumentFailedAndReturnsNotFound()
    {
        var documents = new FakeDocumentRepository();
        var document = new PolicyDocument
        {
            Id = Guid.NewGuid(),
            PolicyId = PolicyId,
            Version = 1,
            StorageKey = "policy/missing.pdf",
            Status = PolicyDocumentGenerationStatus.Available,
            Sha256Checksum = "00",
            CreatedAtUtc = DateTime.UtcNow
        };
        documents.Documents.Add(document);

        PolicyDocumentService service = CreateService(
            documents,
            new FakeFileStore(),
            new FakePdfGenerator());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DownloadForCustomerAsync(
                PolicyId,
                document.Id,
                CustomerId,
                CancellationToken.None));

        Assert.Equal(
            PolicyDocumentGenerationStatus.Failed,
            document.Status);
        Assert.Equal("DOCUMENT_FILE_MISSING", document.FailureCode);
    }

    [Fact]
    public async Task FailedGeneration_CanBeRetriedWithoutCreatingDuplicateVersions()
    {
        var documents = new FakeDocumentRepository();
        var fileStore = new FakeFileStore();
        var generator = new FakePdfGenerator { ShouldFail = true };
        PolicyDocumentService service = CreateService(
            documents,
            fileStore,
            generator);

        await Assert.ThrowsAsync<PolicyDocumentGenerationException>(() =>
            service.EnsureLatestGeneratedAsync(
                PolicyId,
                CancellationToken.None));

        Assert.Equal(
            PolicyDocumentGenerationStatus.Failed,
            Assert.Single(documents.Documents).Status);

        generator.ShouldFail = false;
        var retried = await service.EnsureLatestGeneratedAsync(
            PolicyId,
            CancellationToken.None);
        var repeated = await service.EnsureLatestGeneratedAsync(
            PolicyId,
            CancellationToken.None);

        Assert.Equal(2, retried.Version);
        Assert.Equal(retried.Id, repeated.Id);
        Assert.Equal(2, documents.Documents.Count);
        Assert.Equal(2, generator.CallCount);
    }

    [Fact]
    public async Task ConcurrentGenerationRequests_ReuseTheSameDocument()
    {
        var documents = new FakeDocumentRepository();
        var service = CreateService(
            documents,
            new FakeFileStore(),
            new FakePdfGenerator());

        PolicyDocumentDto[] results = await Task.WhenAll(
            service.EnsureLatestGeneratedAsync(
                PolicyId,
                CancellationToken.None),
            service.EnsureLatestGeneratedAsync(
                PolicyId,
                CancellationToken.None));

        Assert.Equal(results[0].Id, results[1].Id);
        Assert.Single(documents.Documents);
    }

    [Fact]
    public void DocumentEndpoints_RequireCustomerAndAdministratorRoles()
    {
        Assert.Equal(
            "Customer",
            typeof(PoliciesController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>()
                .Single()
                .Roles);

        Assert.Equal(
            "Admin",
            typeof(AdminPoliciesController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>()
                .Single()
                .Roles);
    }

    [Fact]
    public void DatabaseModel_EnforcesOnePolicyAndSuccessfulPaymentPerQuote()
    {
        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseSqlServer("Server=localhost;Database=PolicyModelTest")
            .Options;
        using var context = new PolicyDbContext(options);

        var policyQuoteIndex = context.Model
            .FindEntityType(typeof(Policy))!
            .GetIndexes()
            .Single(index => index.Properties.Any(
                property => property.Name == nameof(Policy.QuoteId)));
        var successfulPaymentQuoteIndex = context.Model
            .FindEntityType(typeof(Payment))!
            .GetIndexes()
            .Single(index => index.GetFilter() == "[Status] = 2");

        Assert.True(policyQuoteIndex.IsUnique);
        Assert.True(successfulPaymentQuoteIndex.IsUnique);
    }

    private static PolicyDocumentService CreateService(
        FakeDocumentRepository documents,
        FakeFileStore fileStore,
        FakePdfGenerator generator)
    {
        return new PolicyDocumentService(
            new FakePolicyRepository(),
            documents,
            fileStore,
            generator,
            Options.Create(new PolicyDocumentOptions
            {
                ProviderName = "Configured Insurer",
                PolicyWording = "Sample test wording."
            }),
            NullLogger<PolicyDocumentService>.Instance);
    }

    private sealed class FakePolicyRepository : IPolicyRepository
    {
        private readonly Policy _policy = new()
        {
            Id = PolicyId,
            CustomerId = CustomerId,
            PolicyNumber = "POL-1",
            ProductName = "Vacation Home",
            EffectiveDateUtc = DateTime.UtcNow.Date,
            ExpiryDateUtc = DateTime.UtcNow.Date.AddYears(1),
            CreatedAtUtc = DateTime.UtcNow,
            Status = PolicyStatus.Active
        };

        public Task<Policy?> GetByIdAsync(
            Guid policyId,
            bool track = false,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(policyId == _policy.Id ? _policy : null);

        public Task AddAsync(Policy policy, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task UpdateAsync(Policy policy, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Policy?> GetByQuoteIdAsync(
            Guid quoteId,
            bool track = false,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Policy>> GetCustomerPoliciesAsync(
            Guid customerId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<PagedResult<Policy>> GetAdminPoliciesAsync(
            PolicyFilterRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Policy>> GetPoliciesForExpiryAsync(
            DateTime utcNow,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task SaveChangesAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<List<Policy>> GetActivePoliciesAsync(
            Guid customerId,
            Guid propertyId,
            Guid? excludedQuoteId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakeDocumentRepository : IPolicyDocumentRepository
    {
        public List<PolicyDocument> Documents { get; } = [];

        public Task<PolicyDocument?> GetByIdAsync(
            Guid policyId,
            Guid documentId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Documents.SingleOrDefault(document =>
                document.PolicyId == policyId &&
                document.Id == documentId));

        public Task<PolicyDocument?> GetLatestAsync(
            Guid policyId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Documents
                .Where(document => document.PolicyId == policyId)
                .OrderByDescending(document => document.Version)
                .FirstOrDefault());

        public Task<IReadOnlyList<PolicyDocument>> GetAllAsync(
            Guid policyId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PolicyDocument>>(
                Documents.Where(document => document.PolicyId == policyId).ToArray());

        public Task AddAsync(
            PolicyDocument document,
            CancellationToken cancellationToken)
        {
            Documents.Add(document);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            PolicyDocument document,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeFileStore : IPolicyDocumentFileStore
    {
        private readonly SemaphoreSlim _policyLock = new(1, 1);
        private readonly Dictionary<string, byte[]> _files = [];

        public async Task<IAsyncDisposable> AcquirePolicyLockAsync(
            Guid policyId,
            CancellationToken cancellationToken)
        {
            await _policyLock.WaitAsync(cancellationToken);
            await Task.Yield();
            return new LockLease(_policyLock);
        }

        public string CreateStorageKey(Guid policyId, int version) =>
            $"policy/{policyId:N}/v{version:D4}.pdf";

        public Task WriteAtomicallyAsync(
            string storageKey,
            ReadOnlyMemory<byte> content,
            CancellationToken cancellationToken)
        {
            _files.Add(storageKey, content.ToArray());
            return Task.CompletedTask;
        }

        public Task<byte[]?> ReadAllBytesAsync(
            string storageKey,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                _files.TryGetValue(storageKey, out byte[]? bytes)
                    ? bytes.ToArray()
                    : null);

        public Task<Stream?> OpenReadAsync(
            string storageKey,
            CancellationToken cancellationToken) =>
            Task.FromResult<Stream?>(
                _files.TryGetValue(storageKey, out byte[]? bytes)
                    ? new MemoryStream(bytes.ToArray(), writable: false)
                    : null);

        public Task QuarantineAsync(
            string storageKey,
            CancellationToken cancellationToken)
        {
            _files.Remove(storageKey);
            return Task.CompletedTask;
        }

        private sealed class LockLease : IAsyncDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public LockLease(SemaphoreSlim semaphore) =>
                _semaphore = semaphore;

            public ValueTask DisposeAsync()
            {
                _semaphore.Release();
                return ValueTask.CompletedTask;
            }
        }
    }

    private sealed class FakePdfGenerator : IPolicyPdfGenerator
    {
        public bool ShouldFail { get; set; }
        public int CallCount { get; private set; }

        public byte[] Generate(
            Policy policy,
            int version,
            DateTime generatedAtUtc,
            PolicyDocumentOptions options)
        {
            CallCount++;
            if (ShouldFail)
            {
                throw new IOException("Simulated PDF generation failure.");
            }

            return Encoding.ASCII.GetBytes("%PDF-1.4\npolicy test");
        }
    }
}
