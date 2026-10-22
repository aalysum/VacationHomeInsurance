using FluentValidation;
using VacationHome.ProductQuote.Application.DTOs.Admin;
using VacationHome.ProductQuote.Application.DTOs.Pricing;
using VacationHome.ProductQuote.Application.Services;


namespace VacationHome.ProductQuote.Application.Validation
{
    public sealed class UpdatePricingRequestValidator :
        AbstractValidator<UpdatePricingRequest>
    {
        public UpdatePricingRequestValidator()
        {
            RuleFor(x => x.BaseRate)
                .InclusiveBetween(
                    PricingAssumptions.MinimumBaseRate,
                    PricingAssumptions.MaximumBaseRate)
                .Must(value =>
                    decimal.Round(
                        value,
                        6,
                        MidpointRounding.AwayFromZero) == value)
                .WithMessage(
                    "Base rate must contain no more than six decimal places.");


            RuleFor(x => x.LowRiskMultiplier)
                .InclusiveBetween(
                    PricingAssumptions.MinimumRiskMultiplier,
                    PricingAssumptions.MaximumRiskMultiplier);


            RuleFor(x => x.MediumRiskMultiplier)
                .InclusiveBetween(
                    PricingAssumptions.MinimumRiskMultiplier,
                    PricingAssumptions.MaximumRiskMultiplier);


            RuleFor(x => x.HighRiskMultiplier)
                .InclusiveBetween(
                    PricingAssumptions.MinimumRiskMultiplier,
                    PricingAssumptions.MaximumRiskMultiplier);

            RuleFor(x => x.LowRiskMultiplier)
                .Must(value =>
                    decimal.Round(
                        value,
                        6,
                        MidpointRounding.AwayFromZero) == value)
                .WithMessage(
                    "Risk multipliers must contain no more than six decimal places.");

            RuleFor(x => x.MediumRiskMultiplier)
                .Must(value =>
                    decimal.Round(
                        value,
                        6,
                        MidpointRounding.AwayFromZero) == value)
                .WithMessage(
                    "Risk multipliers must contain no more than six decimal places.");

            RuleFor(x => x.HighRiskMultiplier)
                .Must(value =>
                    decimal.Round(
                        value,
                        6,
                        MidpointRounding.AwayFromZero) == value)
                .WithMessage(
                    "Risk multipliers must contain no more than six decimal places.");

            RuleFor(x => x)
                .Must(request =>
                    request.LowRiskMultiplier <=
                        request.MediumRiskMultiplier &&
                    request.MediumRiskMultiplier <=
                        request.HighRiskMultiplier)
                .WithMessage(
                    "Risk multipliers must increase from low to high risk.");


            RuleFor(x => x.RowVersion)
                .NotEmpty();
        }
    }
}
