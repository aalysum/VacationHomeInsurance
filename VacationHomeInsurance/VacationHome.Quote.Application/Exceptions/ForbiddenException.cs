namespace VacationHome.ProductQuote.Application.Exceptions
{
    public sealed class ForbiddenException :
        ApplicationExceptionBase
    {
        public ForbiddenException(string message)
            : base("FORBIDDEN", message)
        {
        }
    }
}
