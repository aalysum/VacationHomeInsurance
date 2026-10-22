namespace VacationHome.Auth.Application.Exceptions
{
    public class ForbiddenException
        : ApplicationExceptionBase
    {
        public ForbiddenException(
            string message)
            : this("FORBIDDEN", message)
        {
        }


        public ForbiddenException(
            string code,
            string message)
            : base(code, message)
        {
        }
    }
}
