namespace VacationHome.ProductQuote.Application.DTOs.Customers
{


    public class CustomerPropertyDto
    {
        public Guid PropertyId { get; set; }


        public Guid CustomerId { get; set; }


        public decimal PropertyValue { get; set; }


        public int ConstructionYear { get; set; }


        public int PreviousClaimsCount { get; set; }


        public bool DisasterExposure { get; set; }


        public bool HasSecuritySystem { get; set; }


        public string OccupancyFrequency { get; set; }
            = string.Empty;
    }
}
