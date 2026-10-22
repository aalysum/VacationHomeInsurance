using FluentValidation;
using VacationHome.ProductQuote.Application.DTOs.Products;


public sealed class UpdateCoverageLimitRequestValidator
    : AbstractValidator<UpdateCoverageLimitRequest>
{
    public UpdateCoverageLimitRequestValidator()
    {
        RuleFor(x => x.CoverageLimitPercentage)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .Must(value =>
                decimal.Round(
                    value,
                    4,
                    MidpointRounding.AwayFromZero) == value)
            .WithMessage(
                "Coverage-limit percentage must contain no more than four decimal places.");


        RuleFor(x => x.RowVersion)
            .NotEmpty();
    }
}
