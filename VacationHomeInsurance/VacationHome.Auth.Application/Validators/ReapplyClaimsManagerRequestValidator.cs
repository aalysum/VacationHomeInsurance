using FluentValidation;
using VacationHome.Auth.Application.Common;
using VacationHome.Auth.Application.DTOs;


namespace VacationHome.Auth.Application.Validators
{
    public sealed class
        ReapplyClaimsManagerRequestValidator
        : AbstractValidator<
            ReapplyClaimsManagerRequest>
    {
        public ReapplyClaimsManagerRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .Must(
                    InputValidationHelper
                        .HasNoControlCharacters)
                .WithMessage(
                    "First name contains unsupported characters.");


            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .Must(
                    InputValidationHelper
                        .HasNoControlCharacters)
                .WithMessage(
                    "Last name contains unsupported characters.");


            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(30);


            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0);


            RuleFor(x => x.ProfessionalSummary)
                .NotEmpty()
                .MaximumLength(2000)
                .Must(
                    InputValidationHelper
                        .HasNoControlCharacters)
                .WithMessage(
                    "Professional summary contains unsupported characters.");
        }
    }
}
