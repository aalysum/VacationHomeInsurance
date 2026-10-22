using FluentValidation;
using VacationHome.ClaimService.Application.DTOs.Documents;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class UploadClaimDocumentRequestValidator
        : AbstractValidator<UploadClaimDocumentRequest>
    {
        public UploadClaimDocumentRequestValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .MaximumLength(255);


            RuleFor(x => x.ContentType)
                .NotEmpty();


            RuleFor(x => x.StoragePath)
                .NotEmpty();


            RuleFor(x => x.FileSizeBytes)
                .GreaterThan(0);
        }
    }
}
