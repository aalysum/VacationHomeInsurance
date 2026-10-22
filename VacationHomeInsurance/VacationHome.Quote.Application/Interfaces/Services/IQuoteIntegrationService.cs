using VacationHome.ProductQuote.Application.DTOs.Internal;


namespace VacationHome.ProductQuote.Application
    .Interfaces.Services
{
    public interface IQuoteIntegrationService
    {
        Task<ValidatedQuoteDto> ValidateQuoteForPaymentAsync(
            ValidateQuoteForPaymentRequest request,
            CancellationToken cancellationToken);


        Task<ValidatedQuoteDto> GetValidatedQuoteAsync(
            Guid quoteId,
            Guid customerId,
            string correlationId,
            CancellationToken cancellationToken);


        Task<QuoteConversionResultDto> MarkQuoteConvertedAsync(
            Guid quoteId,
            MarkQuoteConvertedRequest request,
            CancellationToken cancellationToken);


        Task<RenewalQuoteResultDto>
            CreateRenewalQuoteAsync(
                CreateRenewalQuoteRequest request,
                CancellationToken cancellationToken);
    }
}
