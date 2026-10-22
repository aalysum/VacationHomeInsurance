namespace VacationHome.ProductQuote.Application.DTOs.Quotes
{
    public sealed class WithdrawQuoteRequest
    {
        public string? Reason { get; init; }


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
