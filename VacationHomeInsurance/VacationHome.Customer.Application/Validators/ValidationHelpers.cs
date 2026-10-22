namespace VacationHome.Customer.Application.Validators
{
    internal static class ValidationHelpers
    {
        public static bool ContainsNoControlCharacters(
            string? value)
        {
            return string.IsNullOrEmpty(value) ||
                   value.All(
                       character =>
                           !char.IsControl(character));
        }


        public static bool IsValidBase64(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }


            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
