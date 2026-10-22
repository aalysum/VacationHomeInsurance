namespace VacationHome.ProductQuote.Application.DTOs.Admin
{
    public sealed class AdminRejectionRequest
    {
        public string Reason { get; init; } =
            string.Empty;


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
