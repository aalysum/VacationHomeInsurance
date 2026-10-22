using VacationHome.ProductQuote.Application.DTOs.Quotes;


namespace VacationHome.ProductQuote.Application.Interfaces.Services
{
    public interface IQuoteGenerationService
    {
        Task<QuoteDetailsDto> GenerateAsync(
            GenerateQuoteRequest request,
            CancellationToken cancellationToken);

        Task<QuoteDetailsDto> GenerateRenewalAsync(
            Guid policyId,
            Guid customerId,
            CancellationToken cancellationToken);
    }
}
