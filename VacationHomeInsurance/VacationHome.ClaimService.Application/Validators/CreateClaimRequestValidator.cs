using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class CreateClaimRequestValidator
        : AbstractValidator<CreateClaimRequest>
    {
        public CreateClaimRequestValidator()
        {
            RuleFor(request =>
                    request.PolicyId)
                .NotEmpty();


            RuleFor(request =>
                    request.PropertyId)
                .NotEmpty();


            RuleFor(request =>
                    request.Type)
                .Must(type =>
                    type.IsSelectableIncidentType())
                .WithMessage(
                    "Select a supported incident type.");


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
        }
    }


    public sealed class
        ClaimCoverageRequestValidator
        : AbstractValidator<
            ClaimCoverageRequest>
    {
        public ClaimCoverageRequestValidator()
        {
            RuleFor(request =>
                    request.CoverageId)
                .NotEmpty();


            RuleFor(request =>
                    request.RequestedAmount)
                .GreaterThan(0);
        }
    }
}
