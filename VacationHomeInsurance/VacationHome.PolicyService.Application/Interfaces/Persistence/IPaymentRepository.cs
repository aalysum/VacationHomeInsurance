using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.Payments;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Application.Interfaces.Persistence
{
    public interface IPaymentRepository
    {
        Task AddAsync(
            Payment payment,
            CancellationToken cancellationToken);


        Task<Payment?> GetByIdAsync(
            Guid paymentId,
            bool track = false,
            CancellationToken cancellationToken = default);


        Task<Payment?> GetSuccessfulPaymentAsync(
            Guid quoteId,
            CancellationToken cancellationToken);


        Task<int> GetAttemptCountAsync(
            Guid quoteId,
            CancellationToken cancellationToken);


        Task<IReadOnlyList<Payment>> GetCustomerPaymentsAsync(
            Guid customerId,
            CancellationToken cancellationToken);


        Task<PagedResult<Payment>> GetAdminPaymentsAsync(
            PaymentFilterRequest request,
            CancellationToken cancellationToken);


        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
