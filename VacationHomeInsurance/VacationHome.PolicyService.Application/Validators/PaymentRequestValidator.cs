using FluentValidation;
using VacationHome.PolicyService.Application.DTOs.Payments;


namespace VacationHome.PolicyService.Application.Validators
{
    public sealed class PaymentRequestValidator
        : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator()
        {
            RuleFor(x => x.QuoteId)
                .NotEmpty();


            RuleFor(x => x.CardholderName)
                .NotEmpty()
                .MaximumLength(200);


            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .Length(16);


            RuleFor(x => x.Cvv)
                .NotEmpty()
                .Length(3);


            RuleFor(x => x.ExpiryMonth)
                .InclusiveBetween(1, 12);


            RuleFor(x => x.ExpiryYear)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Year);


            RuleFor(x => x.Amount)
                .GreaterThan(0);
        }
    }
}
