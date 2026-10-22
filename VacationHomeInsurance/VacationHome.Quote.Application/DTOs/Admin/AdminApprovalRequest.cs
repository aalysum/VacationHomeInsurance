namespace VacationHome.ProductQuote.Application.DTOs.Admin
{
    public sealed class ApproveQuoteRequest
    {
        public string? Remarks { get; init; }

        public decimal? RevisedAnnualPremium { get; init; }

        public string? PriceAdjustmentReason { get; init; }

        public string RowVersion { get; init; } =
            string.Empty;
    }
}
