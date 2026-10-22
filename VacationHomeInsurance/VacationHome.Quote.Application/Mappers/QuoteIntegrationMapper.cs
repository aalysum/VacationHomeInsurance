using VacationHome.ProductQuote.Application.DTOs.Internal;
using VacationHome.ProductQuote.Domain.Entities;
using VacationHome.ProductQuote.Domain.Enums;


namespace VacationHome.ProductQuote.Application.Mappers
{
    public static class QuoteIntegrationMapper
    {
        public static ValidatedQuoteDto ToValidatedQuoteDto(
            this Quote quote,
            DateTime validationTimeUtc)
        {
            ArgumentNullException.ThrowIfNull(quote);


            bool customerAccepted =
                quote.AcceptedByCustomerId == quote.CustomerId &&
                quote.CustomerAcceptedAtUtc.HasValue &&
                quote.AdminDecisionAtUtc.HasValue &&
                quote.CustomerAcceptedAtUtc >
                    quote.AdminDecisionAtUtc;


            bool adminApproved =
                quote.AdminDecision ==
                    AdminDecision.Approved &&
                quote.DecidedByAdminId.HasValue &&
                quote.AdminDecisionAtUtc.HasValue;


            bool quoteWithinValidity =
                quote.ExpiresAtUtc > validationTimeUtc;


            bool paymentDeadlineValid =
                !quote.PaymentDeadlineUtc.HasValue ||
                quote.PaymentDeadlineUtc.Value >
                    validationTimeUtc;


            bool eligible =
                quote.Status ==
                    QuoteStatus.AdminApproved &&
                customerAccepted &&
                adminApproved &&
                quoteWithinValidity &&
                paymentDeadlineValid &&
                !quote.ConvertedPolicyId.HasValue;


            string? failureCode = null;
            string? failureMessage = null;


            if (!eligible)
            {
                (failureCode, failureMessage) =
                    ResolveFailure(
                        quote,
                        validationTimeUtc,
                        customerAccepted,
                        adminApproved);
            }


            return new ValidatedQuoteDto
            {
                QuoteId = quote.Id,
                QuoteNumber = quote.QuoteNumber,


                CustomerId = quote.CustomerId,


                PropertyId = quote.PropertyId,
                PropertyName = quote.PropertyName,


                ProductId = quote.ProductId,


                IsEligible = eligible,
                FailureCode = failureCode,
                FailureMessage = failureMessage,


                Status = quote.Status,
                GeneratedAtUtc = quote.GeneratedAtUtc,
                ExpiresAtUtc = quote.ExpiresAtUtc,
                PaymentDeadlineUtc =
        quote.PaymentDeadlineUtc,


                AcceptedByCustomerId =
        quote.AcceptedByCustomerId,
                CustomerAcceptedAtUtc =
        quote.CustomerAcceptedAtUtc,


                DecidedByAdminId =
        quote.DecidedByAdminId,
                AdminDecision = quote.AdminDecision,
                AdminDecisionAtUtc =
        quote.AdminDecisionAtUtc,
                ApprovalRemarks =
        quote.ApprovalRemarks,


                ProductType = quote.ProductType,
                ProductName = quote.ProductName,
                ProductDescription =
        quote.ProductDescription,


                PropertyType = quote.PropertyType,
                AddressLine1 = quote.AddressLine1,
                AddressLine2 = quote.AddressLine2,
                City = quote.City,
                State = quote.State,
                PostalCode = quote.PostalCode,
                Country = quote.Country,
                PropertyValue = quote.PropertyValue,
                ConstructionYear =
        quote.ConstructionYear,
                PropertyAge = quote.PropertyAge,
                OccupancyFrequency =
        quote.OccupancyFrequency,
                HasSecuritySystem =
        quote.HasSecuritySystem,
                PreviousClaimsCount =
        quote.PreviousClaimsCount,
                HasDisasterExposure =
        quote.HasDisasterExposure,


                RiskScore = quote.RiskScore,
                RiskLevel = quote.RiskLevel,
                RiskExplanation =
        quote.RiskExplanation,


                PricingConfigurationId =
        quote.PricingConfigurationId,
                BaseRate = quote.BaseRate,
                RiskMultiplier =
        quote.RiskMultiplier,
                BasePremium = quote.BasePremium,
                RiskAdjustedPremium =
        quote.RiskAdjustedPremium,
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


                Coverages = quote.Coverages
        .OrderBy(
            x => x.DisplayOrder)
        .Select(
            ToValidatedCoverageDto)
        .ToArray(),


                RowVersion =
        Convert.ToBase64String(
            quote.RowVersion)
            }; 
        }


        public static QuoteConversionResultDto
            ToConversionResultDto(
                this Quote quote,
                bool wasAlreadyConverted)
        {
            ArgumentNullException.ThrowIfNull(quote);


            if (!quote.ConvertedPolicyId.HasValue ||
                !quote.ConvertedAtUtc.HasValue)
            {
                throw new InvalidOperationException(
                    "The Quote does not contain completed conversion information.");
            }


            return new QuoteConversionResultDto
            {
                QuoteId = quote.Id,
                QuoteNumber = quote.QuoteNumber,
                PolicyId =
                    quote.ConvertedPolicyId.Value,
                Status = quote.Status,
                WasAlreadyConverted =
                    wasAlreadyConverted,
                ConvertedAtUtc =
                    quote.ConvertedAtUtc.Value,
                CorrelationId =
                    quote.ConversionCorrelationId ??
                    string.Empty,
                RowVersion = Convert.ToBase64String(
                    quote.RowVersion)
            };
        }


        private static ValidatedQuoteCoverageDto
            ToValidatedCoverageDto(
                QuoteCoverage coverage)
        {
            return new ValidatedQuoteCoverageDto
            {
                CoverageId = coverage.CoverageId,
                ProductCoverageId =
                    coverage.ProductCoverageId,
                CoverageName =
                    coverage.CoverageName,
                CoverageDescription =
                    coverage.CoverageDescription,
                CoverageType =
                    coverage.CoverageType,
                CoverageCharge =
                    coverage.CoverageCharge,
                CoverageLimitPercentage =
                    coverage.CoverageLimitPercentage,
                CoverageLimitAmount =
                    coverage.CoverageLimitAmount,
                DisplayOrder =
                    coverage.DisplayOrder
            };
        }


        private static (
            string FailureCode,
            string FailureMessage)
            ResolveFailure(
                Quote quote,
                DateTime validationTimeUtc,
                bool customerAccepted,
                bool adminApproved)
        {
            if (quote.Status ==
                    QuoteStatus.ConvertedToPolicy ||
                quote.ConvertedPolicyId.HasValue)
            {
                return (
                    "QUOTE_ALREADY_CONVERTED",
                    "The Quote has already been converted to a Policy.");
            }


            if (quote.Status == QuoteStatus.Expired ||
                quote.ExpiresAtUtc <= validationTimeUtc)
            {
                return (
                    "QUOTE_EXPIRED",
                    "The Quote has expired.");
            }


            if (quote.PaymentDeadlineUtc.HasValue &&
                quote.PaymentDeadlineUtc.Value <=
                    validationTimeUtc)
            {
                return (
                    "PAYMENT_DEADLINE_EXPIRED",
                    "The approved payment deadline has expired.");
            }


            if (quote.Status ==
                QuoteStatus.Invalidated)
            {
                return (
                    "QUOTE_INVALIDATED",
                    "The Quote has been invalidated.");
            }


            if (quote.Status ==
                QuoteStatus.Withdrawn)
            {
                return (
                    "QUOTE_WITHDRAWN",
                    "The Quote has been withdrawn.");
            }


            if (quote.Status ==
                    QuoteStatus.AdminRejected ||
                quote.AdminDecision ==
                    AdminDecision.Rejected)
            {
                return (
                    "QUOTE_REJECTED",
                    "The Quote was rejected by Admin.");
            }


            if (!customerAccepted)
            {
                return (
                    "QUOTE_NOT_ACCEPTED",
                    "The Quote has not been accepted by the Customer.");
            }


            if (!adminApproved)
            {
                return (
                    "QUOTE_NOT_ADMIN_APPROVED",
                    "The Quote has not been approved by Admin.");
            }


            if (quote.Status !=
                QuoteStatus.AdminApproved)
            {
                return (
                    "INVALID_QUOTE_STATUS",
                    "The Quote is not in an eligible payment status.");
            }


            return (
                "QUOTE_NOT_ELIGIBLE",
                "The Quote is not eligible for payment.");
        }
    }
}
