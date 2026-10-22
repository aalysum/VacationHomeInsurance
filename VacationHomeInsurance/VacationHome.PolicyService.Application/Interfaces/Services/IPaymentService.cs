using VacationHome.PolicyService.Application.Common;
using VacationHome.PolicyService.Application.DTOs.Payments;


namespace VacationHome.PolicyService.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentResultDto> ProcessPaymentAsync(
            Guid customerId,
            PaymentRequest request,
            CancellationToken cancellationToken);


        Task<PaymentDto> GetPaymentAsync(
            Guid paymentId,
            Guid customerId,
            bool isAdmin,
            CancellationToken cancellationToken);


        Task<IReadOnlyCollection<PaymentDto>>
            GetCustomerPaymentsAsync(
                Guid customerId,
                CancellationToken cancellationToken);


        Task<PagedResult<PaymentDto>>
            GetAdminPaymentsAsync(
                PaymentFilterRequest request,
                CancellationToken cancellationToken);
    }
}
