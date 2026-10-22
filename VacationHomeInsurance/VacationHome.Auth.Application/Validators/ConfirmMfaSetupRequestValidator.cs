using FluentValidation;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{
    public sealed class ConfirmMfaSetupRequestValidator
    : AbstractValidator<ConfirmMfaSetupRequest>
    {
        public ConfirmMfaSetupRequestValidator()
        {
            RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^[0-9]{6}$")
            .WithMessage(
            "Authenticator code must contain six digits.");
        }
    }
}
