using FluentValidation;
using VacationHome.ProductQuote.Application.DTOs.Quotes;


namespace VacationHome.ProductQuote.Application.Validation
{
    public sealed class GenerateQuoteRequestValidator :
        AbstractValidator<GenerateQuoteRequest>
    {
        public GenerateQuoteRequestValidator()
        {
            RuleFor(x => x.PropertyId)
                .NotEmpty();


            RuleFor(x => x.ProductId)
                .NotEmpty();


            RuleFor(x => x.SelectedOptionalCoverageIds)
                .NotNull();


            RuleFor(x => x.SelectedOptionalCoverageIds)
                .Must(ids =>
                    ids.Count == ids.Distinct().Count())
                .WithErrorCode(
                    "DUPLICATE_COVERAGE_SELECTION")
                .WithMessage(
                    "Optional Coverage IDs cannot be repeated.");
        }
    }
}
