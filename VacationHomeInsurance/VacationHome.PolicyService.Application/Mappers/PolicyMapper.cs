using VacationHome.PolicyService.Application.DTOs.Payments;
using VacationHome.PolicyService.Application.DTOs.Documents;
using VacationHome.PolicyService.Application.DTOs.Policies;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Application.Mappers
{
    public static class PolicyMapper
    {
        public static PolicySummaryDto ToSummaryDto(
            Policy policy)
        {
            return new PolicySummaryDto(
                policy.Id,
                policy.PolicyNumber,


                policy.CustomerId,


                policy.PropertyId,
                policy.PropertyName,


                policy.ProductType,
                policy.ProductName,


                policy.AnnualPremium,
                policy.EffectiveDateUtc,
                policy.ExpiryDateUtc,
                policy.Status,
                policy.Documents
                    .OrderByDescending(document => document.Version)
                    .Select(ToDocumentDto)
                    .ToArray());
        }


        public static PolicyDetailsDto ToDetailsDto(
            Policy policy)
        {
            return new PolicyDetailsDto(
                policy.Id,
                policy.PolicyNumber,


                policy.QuoteId,


                policy.CustomerId,


                policy.PropertyId,
                policy.PropertyName,
                policy.PropertyAddressLine1,
                policy.PropertyAddressLine2,
                policy.PropertyCity,
                policy.PropertyState,
                policy.PropertyPostalCode,
                policy.PropertyCountry,
                policy.PropertyType,
                policy.PropertyValue,
                policy.ConstructionYear,
                policy.OccupancyFrequency,
                policy.HasSecuritySystem,


                policy.ProductType,
                policy.ProductName,


                policy.AnnualPremium,
                policy.EffectiveDateUtc,
                policy.ExpiryDateUtc,
                policy.Status,


                policy.Coverages
                    .Select(ToCoverageDto)
                    .ToList(),
                policy.Documents
                    .OrderByDescending(document => document.Version)
                    .Select(ToDocumentDto)
                    .ToArray());
        }


        public static PolicyCoverageDto ToCoverageDto(
            PolicyCoverage coverage)
        {
            return new PolicyCoverageDto(
                coverage.CoverageId,
                coverage.CoverageName,
                coverage.CoverageDescription,
                coverage.IsMandatory,
                coverage.CoverageCharge,
                coverage.CoverageLimitPercentage,
                coverage.CoverageLimitAmount);
        }


        public static PaymentDto ToDto(
            Payment payment)
        {
            return new PaymentDto(
                payment.Id,
                payment.PaymentNumber,
                payment.QuoteId,
                payment.Amount,
                payment.Status,
                payment.AttemptNumber,
                payment.ProcessedAtUtc,
                payment.Last4Digits);
        }


        public static PolicyDocumentDto ToDocumentDto(
            PolicyDocument document)
        {
            return new PolicyDocumentDto(
                document.Id,
                document.PolicyId,
                document.Version,
                document.Status.ToString(),
                document.ContentType,
                document.FileSizeBytes,
                document.Sha256Checksum,
                document.FailureCode,
                document.CreatedAtUtc,
                document.GeneratedAtUtc);
        }
    }
}
