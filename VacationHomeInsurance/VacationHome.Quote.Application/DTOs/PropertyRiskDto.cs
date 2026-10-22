namespace VacationHome.ProductQuote.Application.DTOs.Integrations
{
    public sealed class PropertyRiskDto
    {
        public Guid PropertyId { get; set; }


        public Guid CustomerId { get; set; }


        public string PropertyName { get; set; } = string.Empty;


        public string PropertyType { get; set; } = string.Empty;


        public decimal PropertyValue { get; set; }


        public int ConstructionYear { get; set; }


        public string OccupancyFrequency { get; set; } = string.Empty;


        public bool SecuritySystem { get; set; }


        public int PreviousClaimsCount { get; set; }


        public bool DisasterExposure { get; set; }
    }
}
