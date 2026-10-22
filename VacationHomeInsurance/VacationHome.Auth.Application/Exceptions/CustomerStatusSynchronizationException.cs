namespace VacationHome.Auth.Application.Exceptions
{
    public class CustomerStatusSynchronizationException
        : Exception
    {
        public CustomerStatusSynchronizationException(
            string message,
            int? statusCode = null)
            : base(message)
        {
            StatusCode = statusCode;
        }


        public int? StatusCode { get; }
    }
}
