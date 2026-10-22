using VacationHome.Customer.Application.Common;


namespace VacationHome.Customer.Application.Exceptions
{
    public class UnauthorizedException : ApplicationExceptionBase
    {
        public UnauthorizedException(
            string message)
            : base(
                ErrorCodes.Unauthorized,
                message)
        {
        }
    }
}
