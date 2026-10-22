using System.Security.Cryptography;
using System.Text;


namespace VacationHome.Auth.Application.Common
{


    public static class SecurityHelper
    {
        public static string ComputeSha256(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);


            var hash = SHA256.HashData(bytes);


            return Convert.ToHexString(hash);
        }
    }
}
