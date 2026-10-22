using VacationHome.ProductQuote.Domain.Constants;
using VacationHome.ProductQuote.Domain.Exceptions;


namespace VacationHome.ProductQuote.Domain.ValueObjects
{
    public sealed record PropertySnapshot
    {
        private PropertySnapshot()
        {
        }


        public PropertySnapshot(
            string propertyName,
            string propertyType,
            string addressLine1,
            string? addressLine2,
            string city,
            string state,
            string postalCode,
            string country,
            decimal propertyValue,
            int constructionYear,
            string occupancyFrequency,
            bool hasSecuritySystem,
            int previousClaimsCount,
            bool hasDisasterExposure)
        {
            if (string.IsNullOrWhiteSpace(propertyName) ||
                string.IsNullOrWhiteSpace(propertyType) ||
                string.IsNullOrWhiteSpace(addressLine1) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(state) ||
                string.IsNullOrWhiteSpace(postalCode) ||
                string.IsNullOrWhiteSpace(country) ||
                string.IsNullOrWhiteSpace(occupancyFrequency))
            {
                throw new DomainRuleException(
                    DomainErrorCodes.InvalidDomainValue,
                    "Property snapshot is incomplete.");
            }


            if (propertyValue <= 0)
            {
                throw new DomainRuleException(
                    DomainErrorCodes.InvalidDomainValue,
                    "Property value must be greater than zero.");
            }


            if (constructionYear <= 0)
            {
                throw new DomainRuleException(
                    DomainErrorCodes.InvalidDomainValue,
                    "Construction year must be greater than zero.");
            }


            if (previousClaimsCount < 0)
            {
                throw new DomainRuleException(
                    DomainErrorCodes.InvalidDomainValue,
                    "Previous claims count cannot be negative.");
            }


            PropertyName = propertyName.Trim();
            PropertyType = propertyType.Trim();
            AddressLine1 = addressLine1.Trim();
            AddressLine2 = NormalizeOptional(addressLine2);
            City = city.Trim();
            State = state.Trim();
            PostalCode = postalCode.Trim();
            Country = country.Trim();
            PropertyValue = Round(propertyValue);
            ConstructionYear = constructionYear;
            OccupancyFrequency = occupancyFrequency.Trim();
            HasSecuritySystem = hasSecuritySystem;
            PreviousClaimsCount = previousClaimsCount;
            HasDisasterExposure = hasDisasterExposure;
        }


        public string PropertyName { get; private set; } =
            string.Empty;


        public string PropertyType { get; private set; } =
            string.Empty;


        public string AddressLine1 { get; private set; } =
            string.Empty;


        public string? AddressLine2 { get; private set; }


        public string City { get; private set; } =
            string.Empty;


        public string State { get; private set; } =
            string.Empty;


        public string PostalCode { get; private set; } =
            string.Empty;


        public string Country { get; private set; } =
            string.Empty;


        public decimal PropertyValue { get; private set; }


        public int ConstructionYear { get; private set; }


        public string OccupancyFrequency { get; private set; } =
            string.Empty;


        public bool HasSecuritySystem { get; private set; }


        public int PreviousClaimsCount { get; private set; }


        public bool HasDisasterExposure { get; private set; }


        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        private static decimal Round(decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}
