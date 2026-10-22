using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationHome.ClaimService.Application.DTOs.Admin;


namespace VacationHome.ClaimService.Application.Validators
{
    public sealed class
        ClaimDashboardFilterRequestValidator
        : AbstractValidator<
            ClaimDashboardFilterRequest>
    {
        private static readonly int[]
            AllowedPageSizes =
            {
            10,
            20,
            25,
            50
        };


        public
            ClaimDashboardFilterRequestValidator()
        {
            RuleFor(request =>
                    request.PageNumber)
                .GreaterThan(0);


            RuleFor(request =>
                    request.PageSize)
                .Must(pageSize =>
                    AllowedPageSizes.Contains(
                        pageSize))
                .WithMessage(
                    "Page size must be 10, 20, 25, or 50.");


            RuleFor(request =>
                    request.ClaimNumber)
                .MaximumLength(100);


            RuleFor(request =>
                    request)
                .Must(request =>
                    !request.FromDate.HasValue ||
                    !request.ToDate.HasValue ||
                    request.FromDate <=
                    request.ToDate)
                .WithMessage(
                    "From-date must not be later than to-date.");
        }
    }
}
