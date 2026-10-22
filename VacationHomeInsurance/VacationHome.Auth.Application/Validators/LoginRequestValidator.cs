using FluentValidation;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{


    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();


            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
