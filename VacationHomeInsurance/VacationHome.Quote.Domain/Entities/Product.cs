using VacationHome.ProductQuote.Domain.Common;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Domain.Entities
{
    public class Product : Entity
    {
        public string Name { get; set; } =
            string.Empty;


        public string Description { get; set; } =
            string.Empty;


        public ProductType ProductType { get; set; }


        public bool IsActive { get; set; }


        public ICollection<ProductCoverage> Coverages
        {
            get;
            set;
        } = new List<ProductCoverage>();
    }
}
