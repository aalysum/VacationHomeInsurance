using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.Internal;
using VacationHome.PolicyService.Application.DTOs.Policies;
using VacationHome.PolicyService.Application.Interfaces.Persistence;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Application.Services;
using VacationHome.PolicyService.Domain.Entities;
using VacationHome.PolicyService.Domain.Enums;
using Xunit;

namespace VacationHome.PolicyService.Tests;

public sealed class PolicyOverlapTests
{
    private static readonly Guid CustomerId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid PropertyId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task CheckOverlap_AllowsPropertyAndNaturalDisasterPoliciesTogether()
    {
        PoliciesService service = CreateService(
            new Policy
            {
                CustomerId = CustomerId,
                PropertyId = PropertyId,
                ProductType = ProductType.PropertyProtection,
                Status = PolicyStatus.Active
            });

        PolicyOverlapResultDto result =
            await service.CheckOverlapAsync(
                new PolicyOverlapRequestDto(
                    CustomerId,
                    PropertyId,
                    ProductType.NaturalDisasterProtection),
                CancellationToken.None);

        Assert.False(result.HasConflict);
    }

    [Fact]
    public async Task CheckOverlap_AllowsNewPropertyPolicyAfterComprehensivePolicyWasCancelled()
    {
        PoliciesService service = CreateService(
            new Policy
            {
                CustomerId = CustomerId,
                PropertyId = PropertyId,
                ProductType = ProductType.ComprehensiveProtection,
                Status = PolicyStatus.Cancelled
            });

        PolicyOverlapResultDto result =
            await service.CheckOverlapAsync(
                new PolicyOverlapRequestDto(
                    CustomerId,
                    PropertyId,
                    ProductType.PropertyProtection),
                CancellationToken.None);

        Assert.False(result.HasConflict);
    }

    [Theory]
    [InlineData(ProductType.PropertyProtection)]
    [InlineData(ProductType.NaturalDisasterProtection)]
    public async Task CheckOverlap_RejectsComprehensiveWhenOtherProtectionIsActive(
        ProductType activeProductType)
    {
        PoliciesService service = CreateService(
            new Policy
            {
                CustomerId = CustomerId,
                PropertyId = PropertyId,
                ProductType = activeProductType,
                Status = PolicyStatus.Active
            });

        PolicyOverlapResultDto result =
            await service.CheckOverlapAsync(
                new PolicyOverlapRequestDto(
                    CustomerId,
                    PropertyId,
                    ProductType.ComprehensiveProtection),
                CancellationToken.None);

        Assert.True(result.HasConflict);
        Assert.Equal("COMPREHENSIVE_POLICY_CONFLICT", result.ConflictCode);
    }

    [Fact]
    public async Task CheckOverlap_RejectsOtherProtectionWhenComprehensiveIsActive()
    {
        PoliciesService service = CreateService(
            new Policy
            {
                CustomerId = CustomerId,
                PropertyId = PropertyId,
                ProductType = ProductType.ComprehensiveProtection,
                Status = PolicyStatus.Active
            });

        PolicyOverlapResultDto result =
            await service.CheckOverlapAsync(
                new PolicyOverlapRequestDto(
                    CustomerId,
                    PropertyId,
                    ProductType.PropertyProtection),
                CancellationToken.None);

        Assert.True(result.HasConflict);
        Assert.Equal("COMPREHENSIVE_POLICY_EXISTS", result.ConflictCode);
    }

    private static PoliciesService CreateService(
        params Policy[] policies)
    {
        return new PoliciesService(
            new FakePolicyRepository(policies),
            new FakeAuditService(),
            new FakeNotificationService());
    }

    private sealed class FakePolicyRepository(
        IReadOnlyCollection<Policy> policies)
        : IPolicyRepository
    {
        public Task<List<Policy>> GetActivePoliciesAsync(
            Guid customerId,
            Guid propertyId,
            Guid? excludedQuoteId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                policies
                    .Where(policy =>
                        policy.CustomerId == customerId &&
                        policy.PropertyId == propertyId)
                    .ToList());
        }

        public Task AddAsync(
            Policy policy,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task UpdateAsync(
            Policy policy,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Policy?> GetByIdAsync(
            Guid policyId,
            bool track = false,
            CancellationToken cancellationToken = default) =>
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

        public Task SaveChangesAsync(
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakeAuditService : IAuditService
    {
        public Task LogAsync(
            string action,
            string entityType,
            string entityId,
            string details,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public Task SendAsync(
            Guid customerId,
            string title,
            string message,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
