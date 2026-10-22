using VacationHome.ProductQuote.Application.DTOs.Quotes;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.Mappers
{
    public static class QuoteMapper
    {
        public static QuoteSummaryDto ToSummaryDto(
            this Quote quote)
        {
            ArgumentNullException.ThrowIfNull(quote);


            return new QuoteSummaryDto(
                quote.Id,
                quote.QuoteNumber,
                quote.PropertyId,
                quote.PropertyName,
                quote.ProductId,
                quote.ProductName,
                quote.Status,
                quote.FinalAnnualPremium,
                quote.GeneratedAtUtc,
                quote.ExpiresAtUtc,
                quote.CustomerAcceptedAtUtc.HasValue,
                quote.AdminDecision,
                quote.PaymentDeadlineUtc);
        }


        public static QuoteDetailsDto ToDetailsDto(
            this Quote quote,
            DateTime utcNow)
        {
            ArgumentNullException.ThrowIfNull(quote);


            bool paymentEligible =
                quote.Status == QuoteStatus.AdminApproved &&
                quote.AcceptedByCustomerId == quote.CustomerId &&
                quote.CustomerAcceptedAtUtc.HasValue &&
                quote.AdminDecision ==
                    AdminDecision.Approved &&
                quote.AdminDecisionAtUtc.HasValue &&
                quote.CustomerAcceptedAtUtc >
                    quote.AdminDecisionAtUtc &&
                quote.ExpiresAtUtc > utcNow &&
                (!quote.PaymentDeadlineUtc.HasValue ||
                 quote.PaymentDeadlineUtc.Value > utcNow) &&
                !quote.ConvertedPolicyId.HasValue;


            return new QuoteDetailsDto
            {
                Id = quote.Id,
                QuoteNumber = quote.QuoteNumber,
                CustomerId = quote.CustomerId,
                PropertyId = quote.PropertyId,
                ProductId = quote.ProductId,


                ProductName = quote.ProductName,
                ProductDescription =
                    quote.ProductDescription,
                ProductType = quote.ProductType,


                Status = quote.Status,


                PropertyName = quote.PropertyName,
                PropertyType = quote.PropertyType,


                AddressSummary = BuildAddressSummary(quote),


                PropertyValue = quote.PropertyValue,
                ConstructionYear =
                    quote.ConstructionYear,
                OccupancyFrequency =
                    quote.OccupancyFrequency,


                RiskScore = quote.RiskScore,
                RiskLevel = quote.RiskLevel,
                RiskExplanation =
                    quote.RiskExplanation,


                BasePremium = quote.BasePremium,
                RiskAdjustmentAmount =
                    quote.RiskAdjustmentAmount,
                MandatoryCoverageCharges =
                    quote.MandatoryCoverageCharges,
                OptionalCoverageCharges =
                    quote.OptionalCoverageCharges,
                FinalAnnualPremium =
                    quote.FinalAnnualPremium,
                EstimatedMonthlyEquivalent =
                    quote.EstimatedMonthlyEquivalent,
                PaymentFrequency =
                    quote.PaymentFrequency,


                GeneratedAtUtc = quote.GeneratedAtUtc,
                ExpiresAtUtc = quote.ExpiresAtUtc,
                PaymentDeadlineUtc =
                    quote.PaymentDeadlineUtc,


                CustomerAcceptedAtUtc =
                    quote.CustomerAcceptedAtUtc,


                AdminDecision = quote.AdminDecision,
                AdminDecisionAtUtc =
                    quote.AdminDecisionAtUtc,
                ApprovalRemarks =
                    quote.ApprovalRemarks,
                RejectionReason =
                    quote.RejectionReason,


                PaymentEligible = paymentEligible,


                Coverages = quote.Coverages
                .OrderBy(coverage => coverage.DisplayOrder)
                .ThenBy(coverage => coverage.CoverageName)
                .Select(coverage =>
                    new QuoteCoverageDto(
                        coverage.CoverageId,
                        coverage.CoverageName,
                        coverage.CoverageDescription,
                        coverage.CoverageType,
                        coverage.CoverageCharge,
                        coverage.CoverageLimitPercentage,
                        coverage.CoverageLimitAmount,
                        coverage.DisplayOrder))
                         .ToArray(),
                RowVersion = Convert.ToBase64String(
    quote.RowVersion),




            };


        }


        private static string BuildAddressSummary(
            Quote quote)
        {
            string?[] addressParts =
            {
                quote.AddressLine1,
                quote.AddressLine2,
                quote.City,
                quote.State,
                quote.PostalCode,
                quote.Country
            };


            return string.Join(
                ", ",
                addressParts.Where(part =>
                    !string.IsNullOrWhiteSpace(part)));
        }
    }
}
