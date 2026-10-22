namespace VacationHome.ProductQuote.Application.Exceptions
{
    public sealed class NotFoundException :
        ApplicationExceptionBase
    {
        public NotFoundException(
            string errorCode,
            string message)
            : base(errorCode, message)
        {
        }
    }
}
