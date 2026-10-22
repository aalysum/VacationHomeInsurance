using FluentValidation;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{
    public sealed class RefreshTokenRequestValidator
        : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}
