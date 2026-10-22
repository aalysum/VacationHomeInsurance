using FluentValidation;
using FluentValidation.Validators;
using VacationHome.ProductQuote.Application.DTOs.Products;


namespace VacationHome.ProductQuote.Application.Validation
{
    public sealed class UpdateProductCoverageRequestValidator :
        AbstractValidator<UpdateProductCoverageRequest>
    {
        public UpdateProductCoverageRequestValidator()
        {
            RuleFor(x => x.CoverageType)
                .IsInEnum();


            RuleFor(x => x.CoverageCharge)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(9999999999999999.99m)
                .Must(value =>
                    decimal.Round(
                        value,
                        2,
                        MidpointRounding.AwayFromZero) == value)
                .WithMessage(
                    "Annual coverage charge must contain no more than two decimal places.");


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


            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0);


            RuleFor(x => x.RowVersion)
                .NotEmpty();
        }
    }
}
