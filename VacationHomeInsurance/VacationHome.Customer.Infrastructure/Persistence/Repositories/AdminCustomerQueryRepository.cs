using Microsoft.EntityFrameworkCore;
using VacationHome.Customer.Application.DTOs.Admin;
using VacationHome.Customer.Application.Interfaces.Repositories;
using VacationHome.Customer.Domain.Entities;
using VacationHome.Customer.Infrastructure.Persistence.Context;


namespace VacationHome.Customer.Infrastructure.Persistence.Repositories
{
    public class AdminCustomerQueryRepository
        : IAdminCustomerQueryRepository
    {
        private readonly CustomerDbContext _context;


        public AdminCustomerQueryRepository(
            CustomerDbContext context)
        {
            _context = context;
        }


        public async Task<(
            IReadOnlyList<AdminCustomerSummaryResponse> Items,
            int TotalCount)>
            GetCustomersAsync(
                AdminCustomerQueryParameters query,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);


            IQueryable<Customers> customersQuery =
                _context.Customers
                    .AsNoTracking();


            customersQuery =
                ApplyCustomerFilters(
                    customersQuery,
                    query);


            var totalCount =
                await customersQuery.CountAsync(
                    cancellationToken);


            customersQuery =
                ApplyCustomerSorting(
                    customersQuery,
                    query.SortBy,
                    IsDescending(
                        query.SortDirection));


            var items =
                await customersQuery
                    .Skip(
                        (query.PageNumber - 1) *
                        query.PageSize)
                    .Take(query.PageSize)
                    .Select(
                        customer =>
                            new AdminCustomerSummaryResponse
                            {
                                CustomerId =
                                    customer.Id,


                                AuthUserId =
                                    customer.AuthUserId,


                                FirstName =
                                    customer.FirstName,


                                LastName =
                                    customer.LastName,


                                PhoneNumber =
                                    customer.PhoneNumber,


                                Status =
                                    customer.Status,


                                PropertyCount =
                                    customer
                                        .VacationProperties
                                        .Count(),


                                CreatedAtUtc =
                                    customer.CreatedAtUtc,


                                UpdatedAtUtc =
                                    customer.UpdatedAtUtc
                            })
                    .ToListAsync(
                        cancellationToken);


            return (
                items,
                totalCount);
        }


        public async Task<Customers?>
            GetCustomerWithProfileAsync(
                Guid customerId,
                CancellationToken cancellationToken)
        {
            return await _context.Customers
                .AsNoTracking()
                .Include(
                    customer =>
                        customer.Profile)
                .FirstOrDefaultAsync(
                    customer =>
                        customer.Id == customerId,
                    cancellationToken);
        }


        public async Task<bool> CustomerExistsAsync(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            return await _context.Customers
                .AsNoTracking()
                .AnyAsync(
                    customer =>
                        customer.Id == customerId,
                    cancellationToken);
        }


        public async Task<int> GetPropertyCountAsync(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            return await _context.VacationProperties
                .AsNoTracking()
                .CountAsync(
                    property =>
                        property.CustomerId ==
                        customerId,
                    cancellationToken);
        }


        public async Task<(
            IReadOnlyList<VacationProperty> Items,
            int TotalCount)>
            GetCustomerPropertiesAsync(
                Guid customerId,
                AdminCustomerPropertyQueryParameters query,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);


            IQueryable<VacationProperty> propertiesQuery =
                _context.VacationProperties
                    .AsNoTracking()
                    .Where(
                        property =>
                            property.CustomerId ==
                            customerId);


            if (!query.IncludeArchived)
            {
                propertiesQuery =
                    propertiesQuery.Where(
                        property =>
                            !property.IsArchived);
            }


            var totalCount =
                await propertiesQuery.CountAsync(
                    cancellationToken);


            propertiesQuery =
                ApplyPropertySorting(
                    propertiesQuery,
                    query.SortBy,
                    IsDescending(
                        query.SortDirection));


            var items =
                await propertiesQuery
                    .Skip(
                        (query.PageNumber - 1) *
                        query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync(
                        cancellationToken);


            return (
                items,
                totalCount);
        }


        private static IQueryable<Customers>
            ApplyCustomerFilters(
                IQueryable<Customers> query,
                AdminCustomerQueryParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(
                    parameters.Name))
            {
                var normalizedName =
                    parameters.Name.Trim();


                query =
                    query.Where(
                        customer =>
                            customer.FirstName.Contains(
                                normalizedName) ||
                            customer.LastName.Contains(
                                normalizedName) ||
                            (customer.FirstName +
                             " " +
                             customer.LastName)
                            .Contains(
                                normalizedName));
            }


            if (parameters.Status.HasValue)
            {
                query =
                    query.Where(
                        customer =>
                            customer.Status ==
                            parameters.Status.Value);
            }


            return query;
        }


        private static IQueryable<Customers>
            ApplyCustomerSorting(
                IQueryable<Customers> query,
                string? sortBy,
                bool sortDescending)
        {
            var normalizedSortField =
                NormalizeSortField(sortBy);


            return normalizedSortField switch
            {
                "firstname" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                customer =>
                                    customer.FirstName)
                            .ThenByDescending(
                                customer =>
                                    customer.Id)
                        : query
                            .OrderBy(
                                customer =>
                                    customer.FirstName)
                            .ThenBy(
                                customer =>
                                    customer.Id),


                "lastname" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                customer =>
                                    customer.LastName)
                            .ThenByDescending(
                                customer =>
                                    customer.Id)
                        : query
                            .OrderBy(
                                customer =>
                                    customer.LastName)
                            .ThenBy(
                                customer =>
                                    customer.Id),


                "status" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                customer =>
                                    customer.Status)
                            .ThenByDescending(
                                customer =>
                                    customer.Id)
                        : query
                            .OrderBy(
                                customer =>
                                    customer.Status)
                            .ThenBy(
                                customer =>
                                    customer.Id),


                "updatedatutc" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                customer =>
                                    customer.UpdatedAtUtc)
                            .ThenByDescending(
                                customer =>
                                    customer.Id)
                        : query
                            .OrderBy(
                                customer =>
                                    customer.UpdatedAtUtc)
                            .ThenBy(
                                customer =>
                                    customer.Id),


                _ =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                customer =>
                                    customer.CreatedAtUtc)
                            .ThenByDescending(
                                customer =>
                                    customer.Id)
                        : query
                            .OrderBy(
                                customer =>
                                    customer.CreatedAtUtc)
                            .ThenBy(
                                customer =>
                                    customer.Id)
            };
        }


        private static IQueryable<VacationProperty>
            ApplyPropertySorting(
                IQueryable<VacationProperty> query,
                string? sortBy,
                bool sortDescending)
        {
            var normalizedSortField =
                NormalizeSortField(sortBy);


            return normalizedSortField switch
            {
                "propertyname" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                property =>
                                    property.PropertyName)
                            .ThenByDescending(
                                property =>
                                    property.Id)
                        : query
                            .OrderBy(
                                property =>
                                    property.PropertyName)
                            .ThenBy(
                                property =>
                                    property.Id),


                "propertyvalue" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                property =>
                                    property.PropertyValue)
                            .ThenByDescending(
                                property =>
                                    property.Id)
                        : query
                            .OrderBy(
                                property =>
                                    property.PropertyValue)
                            .ThenBy(
                                property =>
                                    property.Id),


                "constructionyear" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                property =>
                                    property.ConstructionYear)
                            .ThenByDescending(
                                property =>
                                    property.Id)
                        : query
                            .OrderBy(
                                property =>
                                    property.ConstructionYear)
                            .ThenBy(
                                property =>
                                    property.Id),


                "updatedatutc" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                property =>
                                    property.UpdatedAtUtc)
                            .ThenByDescending(
                                property =>
                                    property.Id)
                        : query
                            .OrderBy(
                                property =>
                                    property.UpdatedAtUtc)
                            .ThenBy(
                                property =>
                                    property.Id),


                "propertytype" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                property =>
                                    property.PropertyType)
                            .ThenByDescending(
                                property =>
                                    property.Id)
                        : query
                            .OrderBy(
                                property =>
                                    property.PropertyType)
                            .ThenBy(
                                property =>
                                    property.Id),


                "isarchived" =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                property =>
                                    property.IsArchived)
                            .ThenByDescending(
                                property =>
                                    property.Id)
                        : query
                            .OrderBy(
                                property =>
                                    property.IsArchived)
                            .ThenBy(
                                property =>
                                    property.Id),


                _ =>
                    sortDescending
                        ? query
                            .OrderByDescending(
                                property =>
                                    property.CreatedAtUtc)
                            .ThenByDescending(
                                property =>
                                    property.Id)
                        : query
                            .OrderBy(
                                property =>
                                    property.CreatedAtUtc)
                            .ThenBy(
                                property =>
                                    property.Id)
            };
        }


        private static bool IsDescending(
            string? sortDirection)
        {
            return !string.Equals(
                sortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase);
        }


        private static string NormalizeSortField(
            string? sortBy)
        {
            return string.IsNullOrWhiteSpace(sortBy)
                ? "createdatutc"
                : sortBy.Trim().ToLowerInvariant();
        }
    }
}
