using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Claims;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class UpdateClaimRequestValidator
        : AbstractValidator<UpdateClaimRequest>
    {
        public UpdateClaimRequestValidator()
        {
            RuleFor(request =>
                    request.Type)
                .IsInEnum();


            RuleFor(request =>
                    request.IncidentDate)
                .NotEmpty()
                .LessThanOrEqualTo(
                    DateTime.UtcNow);


            RuleFor(request =>
                    request.Title)
                .NotEmpty()
                .MaximumLength(200);


            RuleFor(request =>
                    request.Description)
                .NotEmpty()
                .MaximumLength(4000);


            RuleFor(request =>
                    request.Coverages)
                .NotNull()
                .Must(coverages =>
                    coverages.Count > 0)
                .WithMessage(
                    "At least one Policy Coverage must be selected.")


                .Must(coverages =>
                    coverages
                        .Select(
                            coverage =>
                                coverage.CoverageId)
                        .Distinct()
                        .Count() ==
                    coverages.Count)
                .WithMessage(
                    "A Policy Coverage cannot be selected more than once.");


            RuleForEach(request =>
                    request.Coverages)
                .SetValidator(
                    new ClaimCoverageRequestValidator());


            RuleFor(request =>
                    request.RowVersion)
                .NotEmpty();
        }
    }
}
