using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Application.Interfaces.Repositories
{
    public interface IVacationPropertyRepository
    {
        Task AddAsync(
            VacationProperty property,
            CancellationToken cancellationToken);


        Task<VacationProperty?> GetByIdAsync(
            Guid propertyId,
            bool includeArchived,
            CancellationToken cancellationToken);


        Task<(IReadOnlyList<VacationProperty> Items, int TotalCount)>
            GetPagedByCustomerIdAsync(
                Guid customerId,
                int pageNumber,
                int pageSize,
                bool includeArchived,
                string sortBy,
                bool sortDescending,
                CancellationToken cancellationToken);


        void SetOriginalRowVersion(
            VacationProperty property,
            byte[] rowVersion);


        Task<VacationProperty?> GetOwnedPropertyReadOnlyAsync(
            Guid customerId,
            Guid propertyId,
            bool includeArchived,
            CancellationToken cancellationToken);


        Task<VacationProperty?> GetOwnedPropertyForUpdateAsync(
            Guid customerId,
            Guid propertyId,
            bool includeArchived,
            CancellationToken cancellationToken);


        Task<bool> HasDuplicatePropertyNameAsync(
            Guid customerId,
            string propertyName,
            Guid? ignorePropertyId,
            CancellationToken cancellationToken);
    }
}
