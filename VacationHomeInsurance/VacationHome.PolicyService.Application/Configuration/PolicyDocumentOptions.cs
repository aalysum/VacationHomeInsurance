namespace VacationHome.PolicyService.Application.Configuration
{
    public sealed class PolicyDocumentOptions
    {
        public const string SectionName = "PolicyDocuments";

        public string StoragePath { get; set; } =
            "App_Data/PolicyDocuments";

        public string DocumentVersionLabel { get; set; } = "1.0";

        public string ProviderName { get; set; } = string.Empty;

        public string ProviderContact { get; set; } = string.Empty;

        public string? LogoPath { get; set; }

        public string PolicyWording { get; set; } = string.Empty;

        public string CoveredEvents { get; set; } = string.Empty;

        public string Exclusions { get; set; } = string.Empty;

        public string ClaimsInstructions { get; set; } = string.Empty;

        public string CancellationTerms { get; set; } = string.Empty;

        public string RenewalTerms { get; set; } = string.Empty;
    }
}