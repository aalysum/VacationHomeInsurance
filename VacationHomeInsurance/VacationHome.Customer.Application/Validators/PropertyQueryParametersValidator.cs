using FluentValidation;
using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Application.DTOs.Properties;


namespace VacationHome.Customer.Application.Validators
{
    public class PropertyQueryParametersValidator
        : AbstractValidator<PropertyQueryParameters>
    {
        private static readonly string[] AllowedSortFields =
        {
            "createdAtUtc",
            "updatedAtUtc",
            "propertyName",
            "propertyValue",
            "constructionYear"
        };


        public PropertyQueryParametersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0);


            RuleFor(x => x.PageSize)
                .Must(
                    pageSize =>
                        PaginationConstants
                            .AllowedPageSizes
                            .Contains(pageSize))
                .WithMessage(
                    "Page size must be 10, 25, or 50.");


            RuleFor(x => x.SortBy)
                .Must(
                    sortBy =>
                        AllowedSortFields.Contains(
                            sortBy,
                            StringComparer
                                .OrdinalIgnoreCase))
                .WithMessage(
                    "Unsupported property sort field.");


            RuleFor(x => x.SortDirection)
                .Must(
                    direction =>
                        direction.Equals(
                            "asc",
                            StringComparison.OrdinalIgnoreCase) ||
                        direction.Equals(
                            "desc",
                            StringComparison.OrdinalIgnoreCase))
                .WithMessage(
                    "Sort direction must be asc or desc.");
        }
    }
}
