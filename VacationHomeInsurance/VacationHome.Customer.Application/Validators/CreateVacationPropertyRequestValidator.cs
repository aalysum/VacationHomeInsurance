using FluentValidation;
using VacationHome.Customer.Application.DTOs.Properties;


namespace VacationHome.Customer.Application.Validators
{
    public class CreateVacationPropertyRequestValidator
        : AbstractValidator<CreateVacationPropertyRequest>
    {
        private const decimal MinimumPropertyValue =
            50_000m;


        private const decimal MaximumPropertyValue =
            100_000_000m;


        private const int MinimumConstructionYear = 1800;


        public CreateVacationPropertyRequestValidator()
        {
            RuleFor(x => x.PropertyName)
                .NotEmpty()
                .MaximumLength(200)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters);


            RuleFor(x => x.PropertyType)
                .IsInEnum();


            RuleFor(x => x.AddressLine1)
                .NotEmpty()
                .MaximumLength(200)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters);


            RuleFor(x => x.AddressLine2)
                .MaximumLength(200)
                .Must(
                    ValidationHelpers
                        .ContainsNoControlCharacters)
                .When(x => x.AddressLine2 is not null);


            RuleFor(x => x.City)
                .NotEmpty()
                .MaximumLength(100);


            RuleFor(x => x.State)
                .NotEmpty()
                .MaximumLength(100);


            RuleFor(x => x.PostalCode)
                .NotEmpty()
                .MaximumLength(20);


            RuleFor(x => x.Country)
                .NotEmpty()
                .MaximumLength(100);


            RuleFor(x => x.PropertyValue)
                .InclusiveBetween(
                    MinimumPropertyValue,
                    MaximumPropertyValue)
                .WithMessage(
                    "Property value must be between 50,000 and 100,000,000.");


            RuleFor(x => x.ConstructionYear)
                .InclusiveBetween(
                    MinimumConstructionYear,
                    DateTime.UtcNow.Year)
                .WithMessage(
                    $"Construction year must be between {MinimumConstructionYear} and the current year.");


            RuleFor(x => x.OccupancyFrequency)
                .IsInEnum();


            RuleFor(x => x.PreviousClaimsCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage(
                    "Previous claims count cannot be negative.");
        }
    }
}
