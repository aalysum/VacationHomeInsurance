using Microsoft.EntityFrameworkCore;
using VacationHome.Customer.Application
    .Interfaces.Repositories;
using VacationHome.Customer.Domain.Entities;
using VacationHome.Customer.Infrastructure
    .Persistence.Context;


namespace VacationHome.Customer.Infrastructure
    .Persistence.Repositories
{
    public class VacationPropertyRepository
        : IVacationPropertyRepository
    {
        private readonly CustomerDbContext _context;


        public VacationPropertyRepository(
            CustomerDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(
            VacationProperty property,
            CancellationToken cancellationToken)
        {
            await _context.VacationProperties.AddAsync(
                property,
                cancellationToken);
        }


        public async Task<VacationProperty?> GetByIdAsync(
            Guid propertyId,
            bool includeArchived,
            CancellationToken cancellationToken)
        {
            var query =
                _context.VacationProperties
                    .Include(x => x.Customer)
                    .AsQueryable();


            if (!includeArchived)
            {
                query =
                    query.Where(x => !x.IsArchived);
            }


            return await query.FirstOrDefaultAsync(
                x => x.Id == propertyId,
                cancellationToken);
        }


        public async Task<VacationProperty?>
            GetOwnedPropertyReadOnlyAsync(
                Guid customerId,
                Guid propertyId,
                bool includeArchived,
                CancellationToken cancellationToken)
        {
            var query =
                _context.VacationProperties
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.Id == propertyId &&
                            x.CustomerId == customerId);


            if (!includeArchived)
            {
                query = query.Where(
                    x => !x.IsArchived);
            }


            return await query.FirstOrDefaultAsync(
                cancellationToken);
        }


        public async Task<VacationProperty?>
            GetOwnedPropertyForUpdateAsync(
                Guid customerId,
                Guid propertyId,
                bool includeArchived,
                CancellationToken cancellationToken)
        {
            var query =
                _context.VacationProperties
                    .Where(
                        x =>
                            x.Id == propertyId &&
                            x.CustomerId == customerId);


            if (!includeArchived)
            {
                query = query.Where(
                    x => !x.IsArchived);
            }


            return await query.FirstOrDefaultAsync(
                cancellationToken);
        }


        public async Task<bool> HasDuplicatePropertyNameAsync(
            Guid customerId,
            string propertyName,
            Guid? ignorePropertyId,
            CancellationToken cancellationToken)
        {
            var normalizedName =
                propertyName?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return false;
            }

            var query =
                _context.VacationProperties
                    .AsNoTracking()
                    .Where(x =>
                        x.CustomerId == customerId &&
                        !x.IsArchived &&
                        x.PropertyName == normalizedName);

            if (ignorePropertyId.HasValue)
            {
                query = query.Where(
                    x => x.Id != ignorePropertyId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }


        public async Task<(
            IReadOnlyList<VacationProperty> Items,
            int TotalCount)>
            GetPagedByCustomerIdAsync(
                Guid customerId,
                int pageNumber,
                int pageSize,
                bool includeArchived,
                string sortBy,
                bool sortDescending,
                CancellationToken cancellationToken)
        {
            IQueryable<VacationProperty> query =
                _context.VacationProperties
                    .AsNoTracking()
                    .Where(
                        x => x.CustomerId == customerId);


            if (!includeArchived)
            {
                query = query.Where(
                    x => !x.IsArchived);
            }


            var totalCount =
                await query.CountAsync(
                    cancellationToken);


            query = ApplySorting(
                query,
                sortBy,
                sortDescending);


            var properties =
                await query
                    .Skip(
                        (pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(
                        cancellationToken);


            return (
                properties,
                totalCount);
        }


        public void SetOriginalRowVersion(
            VacationProperty property,
            byte[] rowVersion)
        {
            _context.Entry(property)
                .Property(x => x.RowVersion)
                .OriginalValue = rowVersion;
        }


        private static IQueryable<VacationProperty>
            ApplySorting(
                IQueryable<VacationProperty> query,
                string sortBy,
                bool sortDescending)
        {
            return sortBy.ToLowerInvariant() switch
            {
                "updatedatutc" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                x => x.UpdatedAtUtc)
                            .ThenByDescending(
                                x => x.Id)
                        : query
                            .OrderBy(
                                x => x.UpdatedAtUtc)
                            .ThenBy(
                                x => x.Id),


                "propertyname" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                x => x.PropertyName)
                            .ThenByDescending(
                                x => x.Id)
                        : query
                            .OrderBy(
                                x => x.PropertyName)
                            .ThenBy(
                                x => x.Id),


                "propertyvalue" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                x => x.PropertyValue)
                            .ThenByDescending(
                                x => x.Id)
                        : query
                            .OrderBy(
                                x => x.PropertyValue)
                            .ThenBy(
                                x => x.Id),


                "constructionyear" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                x => x.ConstructionYear)
                            .ThenByDescending(
                                x => x.Id)
                        : query
                            .OrderBy(
                                x => x.ConstructionYear)
                            .ThenBy(
                                x => x.Id),


                _ =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                x => x.CreatedAtUtc)
                            .ThenByDescending(
                                x => x.Id)
                        : query
                            .OrderBy(
                                x => x.CreatedAtUtc)
                            .ThenBy(
                                x => x.Id)
            };
        }
    }
}
