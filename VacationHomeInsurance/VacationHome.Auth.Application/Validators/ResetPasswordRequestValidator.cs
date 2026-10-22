using FluentValidation;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{


    public sealed class ResetPasswordRequestValidator
        : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();


            RuleFor(x => x.Token)
                .NotEmpty();


            RuleFor(x => x.NewPassword)
               .NotEmpty()
               .Matches(ValidationConstants.PasswordRegex);


            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.NewPassword)
                .WithMessage(
                    "New password and confirmation password must match.");
        }
    }
}
