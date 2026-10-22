using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Application.DTOs.External;

namespace VacationHome.ClaimService.Application.DTOs.Claims
{
    public sealed class ClaimCustomerDetailsDto
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
    }

    public sealed class ClaimPropertyDetailsDto
    {
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string AddressLine1 { get; init; } = string.Empty;
        public string? AddressLine2 { get; init; }
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public decimal PropertyValue { get; init; }
        public int ConstructionYear { get; init; }
        public string OccupancyFrequency { get; init; } = string.Empty;
        public bool HasSecuritySystem { get; init; }
    }

    public sealed class ClaimPolicyDetailsDto
    {
        public string PolicyNumber { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime EffectiveDateUtc { get; init; }
        public DateTime ExpiryDateUtc { get; init; }
        public decimal AnnualPremium { get; init; }
        public IReadOnlyCollection<PolicyCoverageDto> Coverages { get; init; } =
            Array.Empty<PolicyCoverageDto>();
        public IReadOnlyCollection<PolicyDocumentMetadataDto> Documents { get; init; } =
            Array.Empty<PolicyDocumentMetadataDto>();
    }
}
