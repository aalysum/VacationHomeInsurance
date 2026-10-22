namespace VacationHome.ProductQuote.Domain.Exceptions
{
    public sealed class DomainRuleException : Exception
    {
        public DomainRuleException(
            string errorCode,
            string message)
            : base(message)
        {
            if (string.IsNullOrWhiteSpace(errorCode))
            {
                throw new ArgumentException(
                    "Error code is required.",
                    nameof(errorCode));
            }


            ErrorCode = errorCode.Trim();
        }


        public string ErrorCode { get; }
    }
}
