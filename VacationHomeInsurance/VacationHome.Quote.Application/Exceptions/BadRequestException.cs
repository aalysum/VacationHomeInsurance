namespace VacationHome.ProductQuote.Application.Exceptions
{
    public sealed class BadRequestException :
        ApplicationExceptionBase
    {
        public BadRequestException(
            string errorCode,
            string message)
            : base(errorCode, message)
        {
        }
    }
}
