using FluentValidation;
using VacationHome.Auth.Application.DTOs;
using VacationHome.Auth.Application.Common;


namespace VacationHome.Auth.Application.Validators
{


    public sealed class RegisterCustomerRequestValidator
        : AbstractValidator<RegisterCustomerRequest>
    {
        public RegisterCustomerRequestValidator()
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
