namespace VacationHome.ProductQuote.Application.Exceptions
{
    public sealed class DependencyUnavailableException :
        ApplicationExceptionBase
    {
        public DependencyUnavailableException(
            string dependencyName,
            string message)
            : base(
                "DEPENDENCY_UNAVAILABLE",
                message)
        {
            DependencyName = dependencyName;
        }


        public string DependencyName { get; }
    }
}
