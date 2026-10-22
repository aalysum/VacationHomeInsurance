using FluentValidation;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{


    public sealed class ForgotPasswordRequestValidator
        : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
