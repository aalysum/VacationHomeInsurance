using VacationHome.PolicyService.Application.DTOs.External;


namespace VacationHome.PolicyService.Application.Interfaces.External
{
    public interface IQuoteServiceClient
    {
        Task<ApprovedQuoteDto> GetApprovedQuoteAsync(
            Guid quoteId,
            Guid customerId,
            CancellationToken cancellationToken);


        Task MarkQuoteConvertedAsync(
            Guid quoteId,
            Guid customerId,
            Guid policyId,
            Guid paymentId,
            decimal paidAmount,
            DateTime paymentSucceededAtUtc,
            string rowVersion,
            CancellationToken cancellationToken);


        Task<RenewalQuoteResultDto> CreateRenewalQuoteAsync(
            Guid policyId,
            Guid customerId,
            CancellationToken cancellationToken);
    }
}
