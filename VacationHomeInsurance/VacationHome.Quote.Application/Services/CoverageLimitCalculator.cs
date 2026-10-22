using VacationHome.ProductQuote.Application.Exceptions;

namespace VacationHome.ProductQuote.Application.Services
{
    public static class CoverageLimitCalculator
    {
        public static decimal Calculate(
            decimal propertyValue,
            decimal coverageLimitPercentage)
        {
            if (propertyValue <= 0 ||
                coverageLimitPercentage <= 0 ||
                coverageLimitPercentage > 100 ||
                Math.Round(
                    coverageLimitPercentage,
                    4,
                    MidpointRounding.AwayFromZero) !=
                    coverageLimitPercentage)
            {
                throw new BadRequestException(
                    "INVALID_COVERAGE_LIMIT_INPUT",
                    "Property value must be positive and the coverage-limit percentage must be between 0 and 100 with no more than four decimal places.");
            }

            return Math.Round(
                propertyValue * coverageLimitPercentage / 100m,
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}
