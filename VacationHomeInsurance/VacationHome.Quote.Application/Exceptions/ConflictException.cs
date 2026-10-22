namespace VacationHome.ProductQuote.Application.Exceptions
{
    public sealed class ConflictException :
        ApplicationExceptionBase
    {
        public ConflictException(
            string errorCode,
            string message)
            : base(errorCode, message)
        {
        }
    }
}
