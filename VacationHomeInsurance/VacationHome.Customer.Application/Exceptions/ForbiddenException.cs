namespace VacationHome.Customer.Application.Exceptions
{
    public class ForbiddenException : ApplicationExceptionBase
    {
        public ForbiddenException(
            string code,
            string message)
            : base(code, message)
        {
        }
    }
}
