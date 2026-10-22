namespace VacationHome.ProductQuote.Application.Exceptions
{
    public sealed class ConcurrencyConflictException
        : ApplicationExceptionBase
    {
        public ConcurrencyConflictException()
            : base(
                "CONCURRENCY_CONFLICT",
                "The record was modified by another operation. Refresh the data and try again.")
        {
        }


        public ConcurrencyConflictException(string message)
            : base(
                "CONCURRENCY_CONFLICT",
                message)
        {
        }


        public ConcurrencyConflictException(
            string message,
            Exception innerException)
            : base(
                "CONCURRENCY_CONFLICT",
                message,
                innerException)
        {
        }
    }
}
