using FluentValidation;
using VacationHome.Customer.Application.DTOs.Customers;


namespace VacationHome.Customer.Application.Validators
{
    public class UpdateCustomerStatusRequestValidator
        : AbstractValidator<UpdateCustomerStatusRequest>
    {
        public UpdateCustomerStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum();


            RuleFor(x => x.Reason)
                .NotEmpty()
                .MaximumLength(500)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .WithMessage(
                    "Reason contains unsupported characters.");
        }
    }
}
