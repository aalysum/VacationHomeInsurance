namespace VacationHome.Auth.Application.Common
{
    public static class InputValidationHelper
    {
        public static bool HasNoControlCharacters(
            string? value)
        {
            return value is null || value.All(
                character =>
                    !char.IsControl(character));
        }


        public static string NormalizeEmail(
            string email)
        {
            return email.Trim().ToLowerInvariant();
        }


        public static string NormalizeText(
            string value)
        {
            return value.Trim();
        }
    }
}
