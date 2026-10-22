using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Application.Interfaces.Repositories
{
    public interface ICustomerOnboardingPersistence
    {
        Task<CustomerOnboardingSaveResult> TryCreateAsync(
            Customers customer,
            AuditLog auditLog,
            CancellationToken cancellationToken);
    }


    public enum CustomerOnboardingSaveResult
    {
        Created = 1,
        DuplicateAuthUserId = 2
    }
}
