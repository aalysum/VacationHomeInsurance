using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Claims;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class MarkClaimPaidRequestValidator
        : AbstractValidator<MarkClaimPaidRequest>
    {
        public MarkClaimPaidRequestValidator()
        {
            RuleFor(x => x.PaymentReference)
                .NotEmpty()
                .MaximumLength(100);


            RuleFor(x => x.RowVersion)
                .NotEmpty();
        }
    }
}
