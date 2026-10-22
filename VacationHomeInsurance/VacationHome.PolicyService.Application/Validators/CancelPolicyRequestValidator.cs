using FluentValidation;
using VacationHome.PolicyService.Application.DTOs.Policies;


namespace VacationHome.PolicyService.Application.Validators
{
    public sealed class CancelPolicyRequestValidator
        : AbstractValidator<CancelPolicyRequest>
    {
        public CancelPolicyRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MinimumLength(20)
                .MaximumLength(1000);
        }
    }
}
