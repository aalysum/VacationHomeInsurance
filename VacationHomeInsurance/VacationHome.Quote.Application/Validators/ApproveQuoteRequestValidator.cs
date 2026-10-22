using FluentValidation;
using VacationHome.ProductQuote.Application.DTOs.Admin;


public sealed class ApproveQuoteRequestValidator
    : AbstractValidator<ApproveQuoteRequest>
{
    public ApproveQuoteRequestValidator()
    {
        RuleFor(x => x.Remarks)
            .MaximumLength(1000);

        RuleFor(x => x.RevisedAnnualPremium)
            .Must(value => !value.HasValue ||
                          (value.Value > 0 &&
                           value.Value <= 9999999999999999.99m))
            .WithMessage("Revised annual premium must be positive and within the supported range.");

        RuleFor(x => x.RevisedAnnualPremium)
            .Must(value => !value.HasValue ||
                          decimal.Round(
                              value.Value,
                              2,
                              MidpointRounding.AwayFromZero) == value.Value)
            .WithMessage("Revised annual premium must have no more than two decimal places.");

        RuleFor(x => x.PriceAdjustmentReason)
            .MaximumLength(500);


        RuleFor(x => x.RowVersion)
            .NotEmpty();
    }
}
