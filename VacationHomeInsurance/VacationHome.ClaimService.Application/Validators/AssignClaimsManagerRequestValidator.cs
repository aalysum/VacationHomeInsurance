using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Admin;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class AssignClaimsManagerRequestValidator
        : AbstractValidator<AssignClaimsManagerRequest>
    {
        public AssignClaimsManagerRequestValidator()
        {
            RuleFor(request =>
                    request.ClaimsManagerId)
                .NotEmpty()
                .MaximumLength(450);


            RuleFor(request =>
                    request.Reason)
                .MaximumLength(2000);


            RuleFor(request =>
                    request.RowVersion)
                .NotEmpty()
                .Must(BeValidBase64)
                .WithMessage(
                    "Claim RowVersion must be a valid Base64 value.");
        }


        private static bool BeValidBase64(
            string rowVersion)
        {
            if (
                string.IsNullOrWhiteSpace(
                    rowVersion)
            )
            {
                return false;
            }


            try
            {
                byte[] value =
                    Convert.FromBase64String(
                        rowVersion.Trim());


                return value.Length > 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
