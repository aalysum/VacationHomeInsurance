using VacationHome.ProductQuote.Domain.Common;


namespace VacationHome.ProductQuote.Domain.Entities
{
    public class Coverage : Entity
    {
        public string Name { get; set; } =
            string.Empty;


        public string Description { get; set; } =
            string.Empty;


        public bool IsActive { get; set; }


        public ICollection<ProductCoverage> ProductCoverages
        {
            get;
            set;
        } = new List<ProductCoverage>();
    }
}
