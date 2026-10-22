using FluentValidation;
using VacationHome.ProductQuote.Application.DTOs.Admin;


namespace VacationHome.ProductQuote.Application.Validators
{
    public sealed class AdminRejectionRequestValidator
        : AbstractValidator<AdminRejectionRequest>
    {
        public AdminRejectionRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MinimumLength(20)
                .MaximumLength(1000);


            RuleFor(x => x.RowVersion)
                .NotEmpty();
        }
    }
}
