using FluentValidation;
using VacationHome.ProductQuote.Application.DTOs.Quotes;


public sealed class AcceptQuoteRequestValidator
    : AbstractValidator<AcceptQuoteRequest>
{
    public AcceptQuoteRequestValidator()
    {
        RuleFor(x => x.RowVersion)
            .NotEmpty();
    }
}
