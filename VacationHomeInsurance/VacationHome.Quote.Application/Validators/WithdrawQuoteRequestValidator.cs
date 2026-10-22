using FluentValidation;
using VacationHome.ProductQuote.Application.DTOs.Quotes;


public sealed class WithdrawQuoteRequestValidator
    : AbstractValidator<WithdrawQuoteRequest>
{
    public WithdrawQuoteRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(1000);


        RuleFor(x => x.RowVersion)
            .NotEmpty();
    }
}
