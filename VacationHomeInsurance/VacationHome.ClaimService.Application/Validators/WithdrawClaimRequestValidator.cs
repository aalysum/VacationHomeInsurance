using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Claims;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class WithdrawClaimRequestValidator
        : AbstractValidator<WithdrawClaimRequest>
    {
        public WithdrawClaimRequestValidator()
        {
            RuleFor(x => x.RowVersion)
                .NotEmpty();


            RuleFor(x => x.Reason)
                .MaximumLength(1000);
        }
    }
}
