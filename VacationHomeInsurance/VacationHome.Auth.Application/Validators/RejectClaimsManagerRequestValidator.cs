using FluentValidation;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators {


    public sealed class RejectClaimsManagerRequestValidator
        : AbstractValidator<RejectClaimsManagerRequest>
    {
        public RejectClaimsManagerRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MinimumLength(20)
                .MaximumLength(1000);
        }
    }
}
