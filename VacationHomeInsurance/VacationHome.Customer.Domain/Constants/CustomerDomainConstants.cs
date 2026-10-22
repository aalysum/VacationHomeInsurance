namespace VacationHome.Customer.Domain.Constants
{
    public static class CustomerDomainConstants
    {
        public const int AuthUserIdMaxLength = 450;


        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
        public const int PhoneNumberMaxLength = 20;


        public const int AddressLineMaxLength = 200;
        public const int CityMaxLength = 100;
        public const int StateMaxLength = 100;
        public const int PostalCodeMaxLength = 20;
        public const int CountryMaxLength = 100;


        public const int PropertyNameMaxLength = 200;


        public const decimal MinimumPropertyValue = 50_000m;


        public const decimal MaximumPropertyValue = 100_000_000m;


        public const int MinimumConstructionYear = 1800;


        public const int AuditEntityNameMaxLength = 100;
        public const int AuditEntityIdMaxLength = 100;
        public const int AuditActionMaxLength = 100;
        public const int AuditActorIdMaxLength = 450;
        public const int AuditActorRoleMaxLength = 100;
        public const int AuditCorrelationIdMaxLength = 100;
    }
}
