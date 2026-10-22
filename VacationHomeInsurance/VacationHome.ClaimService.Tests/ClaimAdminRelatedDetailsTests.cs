using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Application.DTOs.External;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.External;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Application.Services;
using VacationHome.ClaimService.Domain.Entities;
using VacationHome.ClaimService.Domain.Enums;
using Xunit;

namespace VacationHome.ClaimService.Tests;

public sealed class ClaimAdminRelatedDetailsTests
{
    [Fact]
    public async Task GetClaimAsync_ReturnsRelatedPersistedDetails()
    {
        var fixture = new Fixture(isAssignedManager: true);

        var result = await fixture.Service.GetClaimAsync(
            fixture.Claim.Id);

        Assert.Equal("Alex Customer", $"{result.Customer?.FirstName} {result.Customer?.LastName}");
        Assert.Equal("alex@example.com", result.Customer?.Email);
        Assert.Equal("555-0100", result.Customer?.PhoneNumber);
        Assert.Equal("18 Cedar Lane", result.Property?.AddressLine1);
        Assert.Equal(425000m, result.Property?.PropertyValue);
        Assert.Equal(1998, result.Property?.ConstructionYear);
        Assert.Equal("Seasonal", result.Property?.OccupancyFrequency);
        Assert.True(result.Property?.HasSecuritySystem);
        Assert.Equal("Comprehensive Protection", result.Policy?.ProductName);
        Assert.Equal("P-2026-1001", result.Policy?.PolicyNumber);
        Assert.Equal(1850m, result.Policy?.AnnualPremium);
        Assert.Single(result.Policy!.Documents);
        Assert.Equal(1, fixture.PolicyClient.GetPolicyCallCount);
        Assert.Equal(1, fixture.CustomerClient.GetCustomerCallCount);
        Assert.Equal(1, fixture.AuthClient.GetCustomerContactCallCount);
    }

    [Fact]
    public async Task UnassignedManager_CannotReadRelatedDetailsOrDownloadPolicyDocument()
    {
        var fixture = new Fixture(isAssignedManager: false);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => fixture.Service.GetClaimAsync(fixture.Claim.Id));

        await Assert.ThrowsAsync<ForbiddenException>(
            () => fixture.Service.DownloadPolicyDocumentAsync(
                fixture.Claim.Id,
                fixture.PolicyDocumentId));

        Assert.Equal(0, fixture.PolicyClient.GetPolicyCallCount);
        Assert.Equal(0, fixture.PolicyClient.DownloadCallCount);
        Assert.Equal(0, fixture.CustomerClient.GetCustomerCallCount);
        Assert.Equal(0, fixture.AuthClient.GetCustomerContactCallCount);
    }

    [Fact]
    public async Task AssignedManager_CanDownloadDocumentListedOnRelatedPolicy()
    {
        var fixture = new Fixture(isAssignedManager: true);

        var result = await fixture.Service.DownloadPolicyDocumentAsync(
            fixture.Claim.Id,
            fixture.PolicyDocumentId);

        Assert.Equal("policy.pdf", result.FileName);
        Assert.Equal(1, fixture.PolicyClient.DownloadCallCount);
    }

    [Fact]
    public async Task AssignedManager_CannotDownloadDocumentNotListedOnRelatedPolicy()
    {
        var fixture = new Fixture(isAssignedManager: true);

        await Assert.ThrowsAsync<NotFoundException>(
            () => fixture.Service.DownloadPolicyDocumentAsync(
                fixture.Claim.Id,
                Guid.NewGuid()));

        Assert.Equal(0, fixture.PolicyClient.DownloadCallCount);
    }

    private sealed class Fixture
    {
        public Fixture(bool isAssignedManager)
        {
            var repository = new TestClaimRepository(Claim);
            PolicyClient = new TestPolicyClient(Policy);
            CustomerClient = new TestCustomerClient(Customer);
            AuthClient = new TestAuthClient();

            var currentUser = new TestCurrentUser
            {
                AuthUserId = "manager-user",
                Role = "ClaimsManager",
                IsAuthenticated = true
            };

            if (isAssignedManager)
            {
                Claim.AssignedClaimsManagerId = currentUser.AuthUserId;
            }

            Service = new ClaimAdminService(
                repository,
                AuthClient,
                PolicyClient,
                CustomerClient,
                currentUser,
                new TestAuditService(),
                new TestUnitOfWork());
        }

        public readonly Guid PolicyDocumentId =
            Guid.Parse("c20ad09f-5c9e-4e49-b605-bdf891318e81");

        public Claim Claim { get; } = new()
        {
            Id = Guid.Parse("b28ac4a8-21a8-4948-812d-7c9af3bd60e0"),
            CustomerId = Guid.Parse("d21f0974-1cef-4e0c-beb8-cd26f21855a2"),
            PolicyId = Guid.Parse("1030e7c6-26ce-4b93-8ca4-9e6ad8283c5d"),
            PolicyNumber = "P-2026-1001",
            PropertyId = Guid.Parse("8e76b8a1-dfa7-4bf2-8509-8b1c570286ad"),
            ClaimNumber = "CL-1001"
        };

        public PolicyDetailsDto Policy { get; } = new()
        {
            Id = Guid.Parse("1030e7c6-26ce-4b93-8ca4-9e6ad8283c5d"),
            CustomerId = Guid.Parse("d21f0974-1cef-4e0c-beb8-cd26f21855a2"),
            PropertyId = Guid.Parse("8e76b8a1-dfa7-4bf2-8509-8b1c570286ad"),
            PolicyNumber = "P-2026-1001",
            ProductName = "Comprehensive Protection",
            PropertyName = "Cedar House",
            PropertyAddressLine1 = "18 Cedar Lane",
            PropertyCity = "Portland",
            PropertyState = "OR",
            PropertyPostalCode = "97201",
            PropertyCountry = "US",
            PropertyType = "Vacation home",
            PropertyValue = 425000m,
            ConstructionYear = 1998,
            OccupancyFrequency = "Seasonal",
            HasSecuritySystem = true,
            AnnualPremium = 1850m,
            Status = PolicyStatus.Active,
            Documents =
            [
                new PolicyDocumentMetadataDto
                {
                    Id = Guid.Parse("c20ad09f-5c9e-4e49-b605-bdf891318e81"),
                    Version = 1,
                    Status = "Available",
                    ContentType = "application/pdf",
                    FileSizeBytes = 1024
                }
            ]
        };

        public CustomerSummaryDto Customer { get; } = new()
        {
            CustomerId = Guid.Parse("d21f0974-1cef-4e0c-beb8-cd26f21855a2"),
            AuthUserId = "customer-user",
            FirstName = "Alex",
            LastName = "Customer",
            PhoneNumber = "555-0100",
            IsActive = true
        };

        public ClaimAdminService Service { get; }
        public TestPolicyClient PolicyClient { get; }
        public TestCustomerClient CustomerClient { get; }
        public TestAuthClient AuthClient { get; }
    }

    private sealed class TestClaimRepository(Claim claim) : IClaimRepository
    {
        public Task<Claim?> GetByIdAsync(Guid claimId, bool track = true, CancellationToken cancellationToken = default) =>
            Task.FromResult<Claim?>(claimId == claim.Id ? claim : null);

        public Task AddAsync(Claim value, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Claim value, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void AddStatusHistory(ClaimStatusHistory history) => throw new NotImplementedException();
        public Task<Claim?> GetByNumberAsync(string claimNumber, bool track = true, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Claim>> SearchAsync(Guid customerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<Claim>> GetOpenClaimsByPolicyAsync(Guid policyId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<decimal> GetCommittedAmountAsync(Guid policyId, Guid coverageId, Guid? excludingClaimId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void SetOriginalRowVersion(Claim value, string rowVersion) => throw new NotImplementedException();
        public Task<PagedResult<Claim>> SearchAdminAsync(ClaimStatus? status, ClaimType? claimType, Guid? customerId, string? claimNumber, DateTime? fromDate, DateTime? toDate, string? assignedClaimsManagerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class TestPolicyClient(PolicyDetailsDto policy) : IPolicyServiceClient
    {
        public int GetPolicyCallCount { get; private set; }
        public int DownloadCallCount { get; private set; }

        public Task<PolicyDetailsDto?> GetPolicyAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            GetPolicyCallCount++;
            return Task.FromResult<PolicyDetailsDto?>(policy);
        }

        public Task<PolicyDocumentContentDto?> DownloadPolicyDocumentAsync(Guid policyId, Guid documentId, CancellationToken cancellationToken = default)
        {
            DownloadCallCount++;
            return Task.FromResult<PolicyDocumentContentDto?>(
                new PolicyDocumentContentDto([], "policy.pdf", "application/pdf"));
        }
    }

    private sealed class TestCustomerClient(CustomerSummaryDto customer) : ICustomerServiceClient
    {
        public int GetCustomerCallCount { get; private set; }

        public Task<CustomerSummaryDto?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            GetCustomerCallCount++;
            return Task.FromResult<CustomerSummaryDto?>(customer);
        }

        public Task<CustomerStatusResponse?> GetCustomerStatusByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class TestAuthClient : IAuthServiceClient
    {
        public int GetCustomerContactCallCount { get; private set; }

        public Task<ActiveClaimsManagerDto?> GetActiveClaimsManagerAsync(string claimsManagerId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<InternalCustomerContactDto?> GetCustomerContactAsync(string userId, CancellationToken cancellationToken = default)
        {
            GetCustomerContactCallCount++;
            return Task.FromResult<InternalCustomerContactDto?>(
                new InternalCustomerContactDto { Email = "alex@example.com" });
        }
    }

    private sealed class TestCurrentUser : ICurrentUserService
    {
        public Guid UserId => Guid.Empty;
        public string AuthUserId { get; init; } = string.Empty;
        public string Email => string.Empty;
        public string Role { get; init; } = string.Empty;
        public bool IsAuthenticated { get; init; }
    }

    private sealed class TestAuditService : IAuditService
    {
        public Task LogAsync(string entityType, string entityId, string action, string? details = null, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }
}
