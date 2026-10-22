using FluentValidation;
using System.Threading.Tasks;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{
    public sealed class VerifyMfaRequestValidator
    : AbstractValidator<VerifyMfaRequest>
    {
        public VerifyMfaRequestValidator()
        {
            RuleFor(x => x.MfaChallengeToken)
            .NotEmpty();


            RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^[0-9]{6}$")
            .WithMessage(
            "Authenticator code must contain six digits.");
        }
    }
}
