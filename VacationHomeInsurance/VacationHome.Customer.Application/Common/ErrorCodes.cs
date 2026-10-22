namespace VacationHome.Customer.Application.Common
{
    public static class ErrorCodes
    {
        public const string Unauthorized =
            "UNAUTHORIZED";


        public const string InvalidAuthUserId =
            "INVALID_AUTH_USER_ID";


        public const string CustomerNotFound =
            "CUSTOMER_NOT_FOUND";


        public const string CustomerInactive =
            "CUSTOMER_INACTIVE";


        public const string CustomerAlreadyExists =
            "CUSTOMER_ALREADY_EXISTS";


        public const string CustomerOnboardingConflict =
            "CUSTOMER_ONBOARDING_CONFLICT";


        public const string PropertyNotFound =
            "PROPERTY_NOT_FOUND";


        public const string PropertyArchived =
            "PROPERTY_ARCHIVED";


        public const string PropertyNameAlreadyExists =
            "PROPERTY_NAME_ALREADY_EXISTS";


        public const string PropertyAlreadyArchived =
            "PROPERTY_ALREADY_ARCHIVED";


        public const string InvalidRowVersion =
            "INVALID_ROW_VERSION";


        public const string ConcurrencyConflict =
            "CONCURRENCY_CONFLICT";


        public const string InvalidPageNumber =
            "INVALID_PAGE_NUMBER";


        public const string InvalidPageSize =
            "INVALID_PAGE_SIZE";


        public const string InvalidSortField =
            "INVALID_SORT_FIELD";
    }
}
