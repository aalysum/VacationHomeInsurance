using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Application.Interfaces.Services;
using VacationHome.ClaimService.Application.Services;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Domain.Entities;
using VacationHome.ClaimService.Domain.Enums;
using Xunit;

namespace VacationHome.ClaimService.Tests;

public sealed class ClaimDocumentAuthorizationTests
{
    private static readonly Guid ClaimId =
        Guid.Parse("d1b7bb14-15b1-4c2c-90b7-0fdcbb634f21");

    private static readonly Guid CustomerId =
        Guid.Parse("acb1141a-15d5-4eaf-a7cf-4d206d1a164d");

    [Fact]
    public async Task AssignedClaimsManagerCanReadClaimDocuments()
    {
        var service = CreateService(
            role: "ClaimsManager",
            authUserId: "manager-1",
            assignedManagerId: "manager-1");

        var documents = await service.GetDocumentsAsync(ClaimId);

        Assert.Empty(documents);
    }

    [Fact]
    public async Task UnassignedClaimsManagerCannotReadClaimDocuments()
    {
        var service = CreateService(
            role: "ClaimsManager",
            authUserId: "manager-2",
            assignedManagerId: "manager-1");

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.GetDocumentsAsync(ClaimId));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Surveyor")]
    public async Task AdminAndSurveyorDocumentReadAccessIsPreserved(string role)
    {
        var service = CreateService(
            role,
            authUserId: "user-1",
            assignedManagerId: "manager-1");

        var documents = await service.GetDocumentsAsync(ClaimId);

        Assert.Empty(documents);
    }

    [Fact]
    public async Task OnlyOwningCustomerCanReadClaimDocuments()
    {
        var ownerService = CreateService(
            role: "Customer",
            authUserId: "customer-user",
            assignedManagerId: null,
            currentCustomerId: CustomerId);

        Assert.Empty(await ownerService.GetDocumentsAsync(ClaimId));

        var otherCustomerService = CreateService(
            role: "Customer",
            authUserId: "other-customer",
            assignedManagerId: null,
            currentCustomerId: Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(
            () => otherCustomerService.GetDocumentsAsync(ClaimId));
    }

    [Fact]
    public async Task AssignedClaimsManagerCanMoveSubmittedClaimToReview()
    {
        var claim = CreateWorkflowClaim(ClaimStatus.Submitted);
        var service = CreateWorkflowService(claim, "manager-1");

        await service.MoveToReviewAsync(ClaimId, Convert.ToBase64String([1]));

        Assert.Equal(ClaimStatus.UnderReview, claim.Status);
        var history = Assert.Single(claim.StatusHistory);
        Assert.Equal(ClaimStatus.Submitted, history.PreviousStatus);
        Assert.Equal(ClaimStatus.UnderReview, history.NewStatus);
    }

    [Fact]
    public async Task UnassignedClaimsManagerCannotMoveClaimToReview()
    {
        var claim = CreateWorkflowClaim(ClaimStatus.Submitted);
        var service = CreateWorkflowService(claim, "manager-2");

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.MoveToReviewAsync(ClaimId, Convert.ToBase64String([1])));

        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.Empty(claim.StatusHistory);
    }

    [Fact]
    public async Task InformationRequestPersistsMessageInStatusHistory()
    {
        var claim = CreateWorkflowClaim(ClaimStatus.UnderReview);
        var service = CreateWorkflowService(claim, "manager-1");

        await service.RequestInformationAsync(
            ClaimId,
            new RequestInformationRequest
            {
                Message = "Please provide the repair invoice.",
                RowVersion = Convert.ToBase64String([1])
            });

        Assert.Equal(
            ClaimStatus.AdditionalInformationRequested,
            claim.Status);
        Assert.Equal(
            "Please provide the repair invoice.",
            Assert.Single(claim.StatusHistory).Comments);
    }

    private static ClaimDocumentService CreateService(
        string role,
        string authUserId,
        string? assignedManagerId,
        Guid? currentCustomerId = null)
    {
        var claim = new Claim
        {
            Id = ClaimId,
            CustomerId = CustomerId,
            AssignedClaimsManagerId = assignedManagerId
        };

        return new ClaimDocumentService(
            new FakeClaimRepository(claim),
            new FakeClaimDocumentRepository(),
            new FakeCurrentUserService(role, authUserId),
            new FakeCustomerIdentityService(currentCustomerId ?? CustomerId),
            new FakeAuditService(),
            new FakeUnitOfWork());
    }

    private static Claim CreateWorkflowClaim(ClaimStatus status) =>
        new()
        {
            Id = ClaimId,
            CustomerId = CustomerId,
            AssignedClaimsManagerId = "manager-1",
            Status = status,
            ClaimNumber = "CLM-TEST-001",
            RowVersion = [1],
            Coverages =
            [
                new ClaimCoverage
                {
                    Id = Guid.NewGuid(),
                    ClaimId = ClaimId,
                    CoverageId = Guid.NewGuid(),
                    CoverageName = "Dwelling Coverage",
                    RequestedAmount = 1000,
                    CoverageLimitAmount = 5000
                }
            ]
        };

    private static ClaimWorkflowService CreateWorkflowService(
        Claim claim,
        string authUserId) =>
        new(
            new FakeClaimRepository(claim),
            new FakeAuditService(),
            new FakeCurrentUserService("ClaimsManager", authUserId),
            new FakeUnitOfWork());

    private sealed class FakeClaimRepository(Claim claim) : IClaimRepository
    {
        public Task AddAsync(Claim item, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Claim item, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void AddStatusHistory(ClaimStatusHistory history) =>
            claim.StatusHistory.Add(history);

        public Task<Claim?> GetByIdAsync(
            Guid claimId,
            bool track = true,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Claim?>(claimId == claim.Id ? claim : null);

        public Task<Claim?> GetByNumberAsync(
            string claimNumber,
            bool track = true,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<PagedResult<Claim>> SearchAsync(
            Guid customerId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyCollection<Claim>> GetOpenClaimsByPolicyAsync(
            Guid policyId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<decimal> GetCommittedAmountAsync(
            Guid policyId,
            Guid coverageId,
            Guid? excludingClaimId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public void SetOriginalRowVersion(Claim item, string rowVersion) =>
            _ = rowVersion;

        public Task<PagedResult<Claim>> SearchAdminAsync(
            ClaimStatus? status,
            ClaimType? claimType,
            Guid? customerId,
            string? claimNumber,
            DateTime? fromDate,
            DateTime? toDate,
            string? assignedClaimsManagerId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeClaimDocumentRepository : IClaimDocumentRepository
    {
        public Task AddAsync(ClaimDocument document, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<ClaimDocument?> GetByIdAsync(
            Guid id,
            bool track = true,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyCollection<ClaimDocument>> GetByClaimIdAsync(
            Guid claimId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<ClaimDocument>>([]);

        public Task DeleteAsync(ClaimDocument document, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeCurrentUserService(
        string role,
        string authUserId) : ICurrentUserService
    {
        public Guid UserId => Guid.Empty;
        public string AuthUserId => authUserId;
        public string Email => "claims.manager@example.com";
        public string Role => role;
        public bool IsAuthenticated => true;
    }

    private sealed class FakeCustomerIdentityService(Guid customerId) : ICustomerIdentityService
    {
        public Task<Guid> GetCurrentCustomerIdAsync(CancellationToken cancellationToken) =>
            Task.FromResult(customerId);
    }

    private sealed class FakeAuditService : IAuditService
    {
        public Task LogAsync(
            string entityType,
            string entityId,
            string action,
            string? details = null,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);
    }
}
