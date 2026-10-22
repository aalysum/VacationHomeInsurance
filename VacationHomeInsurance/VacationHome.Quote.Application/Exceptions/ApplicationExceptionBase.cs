namespace VacationHome.ProductQuote.Application.Exceptions
{
    public abstract class ApplicationExceptionBase
        : Exception
    {
        protected ApplicationExceptionBase(
            string errorCode,
            string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }


        protected ApplicationExceptionBase(
            string errorCode,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }


        public string ErrorCode { get; }
    }
}
