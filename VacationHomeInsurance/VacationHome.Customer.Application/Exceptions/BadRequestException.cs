namespace VacationHome.Customer.Application.Exceptions
{
    public class BadRequestException : ApplicationExceptionBase
    {
        public BadRequestException(
            string code,
            string message)
            : base(code, message)
        {
        }
    }
}
