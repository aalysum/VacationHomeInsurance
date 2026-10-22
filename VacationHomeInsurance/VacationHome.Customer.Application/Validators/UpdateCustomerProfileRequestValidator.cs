using FluentValidation;
using VacationHome.Customer.Application.DTOs.Customers;


namespace VacationHome.Customer.Application.Validators
{
    public class UpdateCustomerProfileRequestValidator
        : AbstractValidator<UpdateCustomerProfileRequest>
    {
        public UpdateCustomerProfileRequestValidator()
        {
            RuleFor(x => x)
                .Must(HaveAtLeastOneValue)
                .WithMessage(
                    "At least one profile field must be supplied.");


            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(
                    DateTime.UtcNow.Date)
                .When(
                    x => x.DateOfBirth.HasValue)
                .WithMessage(
                    "Date of birth cannot be in the future.");


            RuleFor(x => x.AddressLine1)
                .MaximumLength(200)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .When(
                    x => x.AddressLine1 is not null);


            RuleFor(x => x.AddressLine2)
                .MaximumLength(200)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .When(
                    x => x.AddressLine2 is not null);


            RuleFor(x => x.City)
                .MaximumLength(100)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .When(
                    x => x.City is not null);


            RuleFor(x => x.State)
                .MaximumLength(100)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .When(
                    x => x.State is not null);


            RuleFor(x => x.PostalCode)
                .MaximumLength(20)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .When(
                    x => x.PostalCode is not null);


            RuleFor(x => x.Country)
                .MaximumLength(100)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .When(
                    x => x.Country is not null);
        }


        private static bool HaveAtLeastOneValue(
            UpdateCustomerProfileRequest request)
        {
            return request.DateOfBirth.HasValue ||
                   request.AddressLine1 is not null ||
                   request.AddressLine2 is not null ||
                   request.City is not null ||
                   request.State is not null ||
                   request.PostalCode is not null ||
                   request.Country is not null;
        }
    }
}
