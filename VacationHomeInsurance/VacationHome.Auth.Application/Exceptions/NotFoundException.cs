namespace VacationHome.Auth.Application.Exceptions
{
    public class NotFoundException
        : ApplicationExceptionBase
    {
        public NotFoundException(
            string message)
            : this(
                "NOT_FOUND",
                message)
        {
        }


        public NotFoundException(
            string code,
            string message)
            : base(
                code,
                message)
        {
        }
    }
}
