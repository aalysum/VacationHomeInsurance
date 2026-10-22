using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Claims;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class
        ApproveClaimCoverageRequestValidator
        : AbstractValidator<
            ApproveClaimCoverageRequest>
    {
        public
            ApproveClaimCoverageRequestValidator()
        {
            RuleFor(request =>
                    request.CoverageId)
                .NotEmpty();


            RuleFor(request =>
                    request.ApprovedAmount)
                .GreaterThanOrEqualTo(0);
        }
    }


    public sealed class
        ApproveClaimRequestValidator
        : AbstractValidator<
            ApproveClaimRequest>
    {
        public ApproveClaimRequestValidator()
        {
            RuleFor(request =>
                    request.Coverages)
                .NotNull()
                .Must(coverages =>
                    coverages.Count > 0)
                .WithMessage(
                    "At least one Coverage approval allocation is required.")


                .Must(coverages =>
                    coverages
                        .Select(
                            coverage =>
                                coverage.CoverageId)
                        .Distinct()
                        .Count() ==
                    coverages.Count)
                .WithMessage(
                    "A Coverage cannot be included more than once.");


            RuleForEach(request =>
                    request.Coverages)
                .SetValidator(
                    new
                        ApproveClaimCoverageRequestValidator());


            RuleFor(request =>
                    request.Notes)
                .MaximumLength(2000);


            RuleFor(request =>
                    request.RowVersion)
                .NotEmpty();
        }
    }
}
