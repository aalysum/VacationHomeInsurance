namespace VacationHome.Customer.Application.Exceptions
{
    public class ConflictException : ApplicationExceptionBase
    {
        public ConflictException(
            string code,
            string message)
            : base(code, message)
        {
        }
    }
}
