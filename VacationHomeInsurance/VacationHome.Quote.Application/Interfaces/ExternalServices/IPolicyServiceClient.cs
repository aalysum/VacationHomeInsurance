using VacationHome.ProductQuote.Application.DTOs.External;


namespace VacationHome.ProductQuote.Application.Interfaces.External
{
    public interface IPolicyServiceClient
    {
        Task<PolicyOverlapResult> CheckOverlapAsync(
            PolicyOverlapRequest request,
            CancellationToken cancellationToken);


        Task<InternalPolicyDto?> GetPolicyAsync(
        Guid policyId,
        CancellationToken cancellationToken);
    }
}
