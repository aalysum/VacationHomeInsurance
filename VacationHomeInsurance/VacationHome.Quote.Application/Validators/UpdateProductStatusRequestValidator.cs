using FluentValidation;
using VacationHome.ProductQuote.Application.DTOs.Products;


public sealed class UpdateProductStatusRequestValidator
    : AbstractValidator<UpdateProductStatusRequest>
{
    public UpdateProductStatusRequestValidator()
    {
        RuleFor(x => x.RowVersion)
            .NotEmpty();
    }
}
