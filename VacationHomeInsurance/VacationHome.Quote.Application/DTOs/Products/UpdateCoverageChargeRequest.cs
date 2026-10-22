namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed class UpdateCoverageChargeRequest
    {
        public decimal CoverageCharge
        {
            get;
            init;
        }


        public string RowVersion
        {
            get;
            init;
        } = string.Empty;
    }
}
