namespace VacationHome.ProductQuote.Application.Exceptions
{
    public sealed class UnauthorizedException :
        ApplicationExceptionBase
    {
        public UnauthorizedException(string message)
            : base("UNAUTHORIZED", message)
        {
        }
    }
}
