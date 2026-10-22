using FluentValidation;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Admin;


namespace VacationHome.ProductQuote.Application.Validation
{
    public sealed class AdminQuoteSearchRequestValidator :
        AbstractValidator<AdminQuoteSearchRequest>
    {
        private static readonly string[] AllowedSortFields =
        {
            "generatedAtUtc",
            "expiresAtUtc",
            "adminDecisionAtUtc",
            "status",
            "finalAnnualPremium",
            "customerId",
            "productName"
        };


        public AdminQuoteSearchRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0);


            RuleFor(x => x.PageSize)
                .Must(x =>
                    PaginationConstants
                        .AllowedPageSizes
                        .Contains(x))
                .WithMessage(
                    "Page size must be 10, 25, or 50.");


            RuleFor(x => x.SortBy)
                .Must(x =>
                    string.IsNullOrWhiteSpace(x) ||
                    AllowedSortFields.Contains(
                        x,
                        StringComparer.OrdinalIgnoreCase))
                .WithMessage(
                    "Unsupported Admin Quote sort field.");
            RuleFor(x => x)
                .Must(x =>
                    !x.GeneratedFromUtc.HasValue ||
                    !x.GeneratedToUtc.HasValue ||
                    x.GeneratedFromUtc <= x.GeneratedToUtc)
                .WithMessage(
                    "Generated-from date must not be later than generated-to date.");


            RuleFor(x => x)
                .Must(x =>
                    !x.DecisionFromUtc.HasValue ||
                    !x.DecisionToUtc.HasValue ||
                    x.DecisionFromUtc <= x.DecisionToUtc)
                .WithMessage(
                    "Decision-from date must not be later than decision-to date.");
        }
    }
}
