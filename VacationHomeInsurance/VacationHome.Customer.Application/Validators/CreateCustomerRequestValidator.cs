using FluentValidation;
using VacationHome.Customer.Application.DTOs.Customers;


namespace VacationHome.Customer.Application.Validators
{
    public class CreateCustomerRequestValidator
        : AbstractValidator<CreateCustomerRequest>
    {
        public CreateCustomerRequestValidator()
        {
            RuleFor(x => x.AuthUserId)
                .NotEmpty()
                .MaximumLength(450);


            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .WithMessage(
                    "First name contains unsupported characters.");


            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .WithMessage(
                    "Last name contains unsupported characters.");


            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20)
                .Matches(@"^\+?[0-9\s\-()]+$")
                .WithMessage(
                    "Phone number has an invalid format.");
        }
    }
}
