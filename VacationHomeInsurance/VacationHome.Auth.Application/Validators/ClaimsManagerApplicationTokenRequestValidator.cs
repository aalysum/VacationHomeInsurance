using FluentValidation;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{
    public sealed class
        ClaimsManagerApplicationTokenRequestValidator
        : AbstractValidator<
            ClaimsManagerApplicationTokenRequest>
    {
        public ClaimsManagerApplicationTokenRequestValidator()
        {
            RuleFor(x => x.ApplicationToken)
                .NotEmpty();
        }
    }
}
