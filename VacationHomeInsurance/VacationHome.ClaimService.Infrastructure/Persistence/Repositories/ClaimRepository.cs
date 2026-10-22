using Microsoft.EntityFrameworkCore;
using VacationHome.ClaimService.Application.Common;
using VacationHome.ClaimService.Application.Exceptions;
using VacationHome.ClaimService.Application.Interfaces.Persistence;
using VacationHome.ClaimService.Domain.Entities;
using VacationHome.ClaimService.Domain.Enums;




namespace VacationHome.ClaimService.Infrastructure
    .Persistence.Repositories
{
    public sealed class ClaimRepository
        : IClaimRepository
    {
        private readonly ClaimDbContext
            _context;


        public ClaimRepository(
            ClaimDbContext context)
        {
            _context =
                context;
        }


        public async Task AddAsync(
            Claim claim,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                claim);


            await _context.Claims.AddAsync(
                claim,
                cancellationToken);
        }


        public Task UpdateAsync(
            Claim claim,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(
                claim);


            cancellationToken
                .ThrowIfCancellationRequested();


            var entry =
                _context.Entry(claim);

            if (entry.State == EntityState.Detached)
            {
                _context.Claims.Update(claim);
            }

            return Task.CompletedTask;
        }

        public void AddStatusHistory(
            ClaimStatusHistory history)
        {
            ArgumentNullException.ThrowIfNull(
                history);


            _context.ClaimStatusHistories.Add(
                history);
        }


        public async Task<Claim?> GetByIdAsync(
            Guid claimId,
            bool track = true,
            CancellationToken cancellationToken = default)
        {
            if (claimId == Guid.Empty)
            {
                return null;
            }


            IQueryable<Claim> query =
                BuildClaimDetailsQuery();


            if (!track)
            {
                query =
                    query.AsNoTracking();
            }


            return await query
                .SingleOrDefaultAsync(
                    claim =>
                        claim.Id ==
                        claimId,
                    cancellationToken);
        }


        public async Task<Claim?> GetByNumberAsync(
            string claimNumber,
            bool track = true,
            CancellationToken cancellationToken = default)
        {
            if (
                string.IsNullOrWhiteSpace(
                    claimNumber)
            )
            {
                return null;
            }


            string normalizedClaimNumber =
                claimNumber.Trim();


            IQueryable<Claim> query =
                BuildClaimDetailsQuery();


            if (!track)
            {
                query =
                    query.AsNoTracking();
            }


            return await query
                .SingleOrDefaultAsync(
                    claim =>
                        claim.ClaimNumber ==
                        normalizedClaimNumber,
                    cancellationToken);
        }


        public async Task<PagedResult<Claim>>
            SearchAsync(
                Guid customerId,
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default)
        {
            if (customerId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Customer identifier is required.");
            }


            ValidatePagination(
                pageNumber,
                pageSize);


            IQueryable<Claim> query =
                _context.Claims
                    .AsNoTracking()
                    .Include(
                        claim =>
                            claim.Coverages)
                    .Where(
                        claim =>
                            claim.CustomerId ==
                            customerId);


            long totalCount =
                await query.LongCountAsync(
                    cancellationToken);


            Claim[] items =
                await query
                    .OrderByDescending(
                        claim =>
                            claim.SubmittedAtUtc)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToArrayAsync(
                        cancellationToken);


            return new PagedResult<Claim>
            {
                Items =
                    items,


                PageNumber =
                    pageNumber,


                PageSize =
                    pageSize,


                TotalCount =
                    totalCount
            };
        }


        public async Task<PagedResult<Claim>>
            SearchAdminAsync(
                ClaimStatus? status,
                ClaimType? claimType,
                Guid? customerId,
                string? claimNumber,
                DateTime? fromDate,
                DateTime? toDate,
                string? assignedClaimsManagerId,
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default)
        {
            ValidatePagination(
                pageNumber,
                pageSize);


            if (
                customerId.HasValue &&
                customerId.Value ==
                    Guid.Empty
            )
            {
                throw new BadRequestException(
                    "Customer identifier is invalid.");
            }


            if (
                fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value >
                    toDate.Value
            )
            {
                throw new BadRequestException(
                    "From-date must not be later than to-date.");
            }


            IQueryable<Claim> query =
                _context.Claims
                    .AsNoTracking()
                    .Include(
                        claim =>
                            claim.Coverages);


            if (status.HasValue)
            {
                query =
                    query.Where(
                        claim =>
                            claim.Status ==
                            status.Value);
            }


            if (claimType.HasValue)
            {
                query =
                    query.Where(
                        claim =>
                            claim.Type ==
                            claimType.Value);
            }


            if (customerId.HasValue)
            {
                query =
                    query.Where(
                        claim =>
                            claim.CustomerId ==
                            customerId.Value);
            }


            if (
                !string.IsNullOrWhiteSpace(
                    claimNumber)
            )
            {
                string normalizedClaimNumber =
                    claimNumber.Trim();


                query =
                    query.Where(
                        claim =>
                            EF.Functions.Like(
                                claim.ClaimNumber,
                                $"%{normalizedClaimNumber}%"));
            }


            if (fromDate.HasValue)
            {
                query =
                    query.Where(
                        claim =>
                            claim.SubmittedAtUtc >=
                            fromDate.Value);
            }


            if (toDate.HasValue)
            {
                query =
                    query.Where(
                        claim =>
                            claim.SubmittedAtUtc <=
                            toDate.Value);
            }
            if (
    !string.IsNullOrWhiteSpace(
        assignedClaimsManagerId)
)
            {
                string normalizedClaimsManagerId =
                    assignedClaimsManagerId.Trim();


                query =
                    query.Where(
                        claim =>
                            claim.AssignedClaimsManagerId ==
                            normalizedClaimsManagerId);
            }


            long totalCount =
                await query.LongCountAsync(
                    cancellationToken);


            Claim[] items =
                await query
                    .OrderByDescending(
                        claim =>
                            claim.SubmittedAtUtc)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToArrayAsync(
                        cancellationToken);


            return new PagedResult<Claim>
            {
                Items =
                    items,


                PageNumber =
                    pageNumber,


                PageSize =
                    pageSize,


                TotalCount =
                    totalCount
            };
        }


        public async Task<
            IReadOnlyCollection<Claim>>
            GetOpenClaimsByPolicyAsync(
                Guid policyId,
                CancellationToken cancellationToken = default)
        {
            if (policyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Policy identifier is required.");
            }


            return await _context.Claims
                .AsNoTracking()
                .Include(
                    claim =>
                        claim.Coverages)
                .Where(
                    claim =>
                        claim.PolicyId ==
                            policyId &&
                        claim.Status !=
                            ClaimStatus.Closed &&
                        claim.Status !=
                            ClaimStatus.Rejected &&
                        claim.Status !=
                            ClaimStatus.Withdrawn)
                .OrderByDescending(
                    claim =>
                        claim.SubmittedAtUtc)
                .ToArrayAsync(
                    cancellationToken);
        }


        public async Task<decimal>
            GetCommittedAmountAsync(
                Guid policyId,
                Guid coverageId,
                Guid? excludingClaimId,
                CancellationToken cancellationToken = default)
        {
            if (policyId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Policy identifier is required.");
            }


            if (coverageId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Coverage identifier is required.");
            }


            IQueryable<ClaimCoverage> query =
                _context.ClaimCoverages
                    .AsNoTracking()
                    .Where(
                        claimCoverage =>
                            claimCoverage.CoverageId ==
                                coverageId &&
                            claimCoverage.Claim.PolicyId ==
                                policyId &&
                            (
                                claimCoverage.Claim.Status ==
                                    ClaimStatus.Approved ||
                                claimCoverage.Claim.Status ==
                                    ClaimStatus.Paid ||
                                claimCoverage.Claim.Status ==
                                    ClaimStatus.Closed
                            ));


            if (excludingClaimId.HasValue)
            {
                query =
                    query.Where(
                        claimCoverage =>
                            claimCoverage.ClaimId !=
                            excludingClaimId.Value);
            }


            decimal committedAmount =
                await query
                    .Select(
                        claimCoverage =>
                            claimCoverage.ApprovedAmount ??
                            0m)
                    .SumAsync(
                        cancellationToken);


            return committedAmount;
        }


        public void SetOriginalRowVersion(
            Claim claim,
            string rowVersion)
        {
            ArgumentNullException.ThrowIfNull(
                claim);


            if (
                string.IsNullOrWhiteSpace(
                    rowVersion)
            )
            {
                throw new BadRequestException(
                    "Claim RowVersion is required.");
            }


            byte[] expectedRowVersion;


            try
            {
                expectedRowVersion =
                    Convert.FromBase64String(
                        rowVersion.Trim());
            }
            catch (FormatException)
            {
                throw new BadRequestException(
                    "Claim RowVersion must be a valid Base64 value.");
            }


            if (expectedRowVersion.Length == 0)
            {
                throw new BadRequestException(
                    "Claim RowVersion cannot be empty.");
            }


            _context.Entry(
                    claim)
                .Property(
                    trackedClaim =>
                        trackedClaim.RowVersion)
                .OriginalValue =
                    expectedRowVersion;
        }


        private IQueryable<Claim>
            BuildClaimDetailsQuery()
        {
            return _context.Claims
                .AsSplitQuery()
                .Include(
                    claim =>
                        claim.Coverages)
                .Include(
                    claim =>
                        claim.Documents)
                .Include(
                    claim =>
                        claim.StatusHistory)
                .Include(
                    claim =>
                        claim.AssignmentHistory);
        }


        private static void ValidatePagination(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new BadRequestException(
                    "Page number must be greater than zero.");
            }


            if (pageSize < 1)
            {
                throw new BadRequestException(
                    "Page size must be greater than zero.");
            }


            if (pageSize > 100)
            {
                throw new BadRequestException(
                    "Page size cannot exceed 100.");
            }
        }
    }
}
