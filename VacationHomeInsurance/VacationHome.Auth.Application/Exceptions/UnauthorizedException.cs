namespace VacationHome.Auth.Application.Exceptions
{
    public class UnauthorizedException
        : ApplicationExceptionBase
    {
        public UnauthorizedException(
            string message)
            : this(
                "UNAUTHORIZED",
                message)
        {
        }


        public UnauthorizedException(
            string code,
            string message)
            : base(
                code,
                message)
        {
        }
    }
}
