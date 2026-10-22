using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Claims;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class RejectClaimRequestValidator
        : AbstractValidator<RejectClaimRequest>
    {
        public RejectClaimRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MaximumLength(1000);


            RuleFor(x => x.RowVersion)
                .NotEmpty();
        }
    }
}
