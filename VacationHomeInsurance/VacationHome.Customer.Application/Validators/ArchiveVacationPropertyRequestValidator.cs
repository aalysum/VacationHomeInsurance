using FluentValidation;
using VacationHome.Customer.Application.DTOs.Properties;


namespace VacationHome.Customer.Application.Validators
{
    public class ArchiveVacationPropertyRequestValidator
        : AbstractValidator<ArchiveVacationPropertyRequest>
    {
        public ArchiveVacationPropertyRequestValidator()
        {
            RuleFor(x => x.RowVersion)
                .NotEmpty()
                .Must(ValidationHelpers.IsValidBase64)
                .WithMessage(
                    "Row version must be a valid Base64 value.");
        }
    }
}
