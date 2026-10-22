namespace VacationHome.Customer.Application.Exceptions
{
    public class NotFoundException : ApplicationExceptionBase
    {
        public NotFoundException(
            string code,
            string message)
            : base(code, message)
        {
        }
    }
}
