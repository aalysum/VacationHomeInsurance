namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed class UpdateCoverageStatusRequest
    {
        public bool IsActive { get; init; }


        public string RowVersion { get; init; } =
            string.Empty;
    }
}
