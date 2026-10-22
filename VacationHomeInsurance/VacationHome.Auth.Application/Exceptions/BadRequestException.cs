namespace VacationHome.Auth.Application.Exceptions
{
    public class BadRequestException
        : ApplicationExceptionBase
    {
        public BadRequestException(
            string message)
            : this(
                "BAD_REQUEST",
                message)
        {
        }


        public BadRequestException(
            string code,
            string message)
            : base(
                code,
                message)
        {
        }
    }
}
