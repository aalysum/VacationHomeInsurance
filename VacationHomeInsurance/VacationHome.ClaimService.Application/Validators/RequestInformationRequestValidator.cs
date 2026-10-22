using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Claims;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class RequestInformationRequestValidator
        : AbstractValidator<RequestInformationRequest>
    {
        public RequestInformationRequestValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty()
                .MaximumLength(2000);


            RuleFor(x => x.RowVersion)
                .NotEmpty();
        }
    }
}
