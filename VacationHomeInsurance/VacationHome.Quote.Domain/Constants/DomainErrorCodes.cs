namespace VacationHome.ProductQuote.Domain.Constants
{
    public static class DomainErrorCodes
    {
        public const string InvalidDomainValue =
            "INVALID_DOMAIN_VALUE";


        public const string InvalidQuoteStatus =
            "INVALID_QUOTE_STATUS";


        public const string QuoteExpired =
            "QUOTE_EXPIRED";


        public const string QuoteNotExpired =
            "QUOTE_NOT_EXPIRED";


        public const string QuoteAlreadyDecided =
            "QUOTE_ALREADY_DECIDED";


        public const string QuoteAlreadyConverted =
            "QUOTE_ALREADY_CONVERTED";


        public const string InvalidRejectionReason =
            "INVALID_REJECTION_REASON";


        public const string DuplicateCoverageSelection =
            "DUPLICATE_COVERAGE_SELECTION";


        public const string MandatoryCoverageCannotBeOptional =
            "MANDATORY_COVERAGE_CANNOT_BE_OPTIONAL";


        public const string OptionalCoverageInactive =
            "OPTIONAL_COVERAGE_INACTIVE";


        public const string CoverageNotAvailableForProduct =
            "COVERAGE_NOT_AVAILABLE_FOR_PRODUCT";


        public const string InvalidOptionalCoverage =
            "INVALID_OPTIONAL_COVERAGE";


        public const string DuplicateProductCoverage =
            "DUPLICATE_PRODUCT_COVERAGE";


        public const string InvalidCoverageConfiguration =
            "INVALID_COVERAGE_CONFIGURATION";


        public const string InvalidPricingConfiguration =
            "INVALID_PRICING_CONFIGURATION";


        public const string InvalidRiskConfiguration =
            "INVALID_RISK_CONFIGURATION";
    }
}
