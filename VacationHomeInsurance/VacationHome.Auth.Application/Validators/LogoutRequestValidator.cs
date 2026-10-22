using FluentValidation;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{
    public sealed class LogoutRequestValidator
    : AbstractValidator<LogoutRequest>
    {
        public LogoutRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
            .NotEmpty();
        }
    }
}
