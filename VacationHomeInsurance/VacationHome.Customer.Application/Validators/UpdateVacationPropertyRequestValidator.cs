using FluentValidation;
using VacationHome.Customer.Application.DTOs.Properties;
using VacationHome.Customer.Domain.Constants;


namespace VacationHome.Customer.Application.Validators
{
    public class UpdateVacationPropertyRequestValidator
        : AbstractValidator<UpdateVacationPropertyRequest>
    {
        private const decimal MinimumPropertyValue =
            50_000m;


        private const decimal MaximumPropertyValue =
            10_000_000m;


        private const int MinimumConstructionYear = 1800;


        public UpdateVacationPropertyRequestValidator()
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
                    CustomerDomainConstants.MinimumPropertyValue,
                    CustomerDomainConstants.MaximumPropertyValue)
                .WithMessage(
                    $"Property value must be between " +
                    $"{CustomerDomainConstants.MinimumPropertyValue:N0} and " +
                    $"{CustomerDomainConstants.MaximumPropertyValue:N0}.");


            RuleFor(x => x.ConstructionYear)
                .InclusiveBetween(
                    CustomerDomainConstants.MinimumConstructionYear,
                    DateTime.UtcNow.Year)
                .WithMessage(
                    $"Construction year must be between " +
                    $"{CustomerDomainConstants.MinimumConstructionYear} " +
                    $"and {DateTime.UtcNow.Year}.");


            RuleFor(x => x.OccupancyFrequency)
                .IsInEnum();


            RuleFor(x => x.PreviousClaimsCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage(
                    "Previous claims count cannot be negative.");


            RuleFor(x => x.RowVersion)
                .NotEmpty()
                .Must(ValidationHelpers.IsValidBase64)
                .WithMessage(
                    "Row version must be a valid Base64 value.");
        }
    }
}
