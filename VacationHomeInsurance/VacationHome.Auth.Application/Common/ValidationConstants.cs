namespace VacationHome.Auth.Application.Common
{


    public static class ValidationConstants
    {
        public const string PasswordRegex =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$";
    }
}
