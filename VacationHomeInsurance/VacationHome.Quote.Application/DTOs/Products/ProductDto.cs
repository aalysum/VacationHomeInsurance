namespace VacationHome.ProductQuote.Application.DTOs.Products
{
    public sealed class ProductDto
    {
        public Guid Id { get; set; }


        public string Name { get; set; } = string.Empty;


        public string Description { get; set; } = string.Empty;


        public bool IsActive { get; set; }


        public List<ProductCoverageDto> Coverages { get; set; } = [];
    }
}
