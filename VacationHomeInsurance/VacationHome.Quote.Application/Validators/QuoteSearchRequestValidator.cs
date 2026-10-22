using FluentValidation;
using VacationHome.ProductQuote.Application.Common;
using VacationHome.ProductQuote.Application.DTOs.Quotes;


namespace VacationHome.ProductQuote.Application.Validation
{
    public sealed class QuoteSearchRequestValidator :
        AbstractValidator<QuoteSearchRequest>
    {
        private static readonly string[] AllowedSortFields =
        {
            "generatedAtUtc",
            "expiresAtUtc",
            "status",
            "finalAnnualPremium",
            "productName",
            "propertyName"
        };


        public QuoteSearchRequestValidator()
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
                    "Unsupported Quote sort field.");


            RuleFor(x => x)
                .Must(x =>
                    !x.GeneratedFromUtc.HasValue ||
                    !x.GeneratedToUtc.HasValue ||
                    x.GeneratedFromUtc <=
                    x.GeneratedToUtc)
                .WithMessage(
                    "Generated-from date must not be later than generated-to date.");
        }
    }
}
