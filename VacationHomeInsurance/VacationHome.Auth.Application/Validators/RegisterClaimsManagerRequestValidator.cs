using FluentValidation;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{


    public sealed class RegisterClaimsManagerRequestValidator
        : AbstractValidator<RegisterClaimsManagerRequest>
    {
        public RegisterClaimsManagerRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .Must(InputValidationHelper.HasNoControlCharacters)
                .WithMessage(
                    "First name contains unsupported characters.");


            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .Must(InputValidationHelper.HasNoControlCharacters)
                .WithMessage(
                    "Last name contains unsupported characters.");


            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();


            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(10);


            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0);


            RuleFor(x => x.ProfessionalSummary)
                .NotEmpty();


            RuleFor(x => x.Password)
                .NotEmpty()
                .Matches(ValidationConstants.PasswordRegex)
                .WithMessage(
                    "Password must contain uppercase, lowercase, digit and special character.");


            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage(
                    "Password and confirmation password must match.");
        }
    }
}
