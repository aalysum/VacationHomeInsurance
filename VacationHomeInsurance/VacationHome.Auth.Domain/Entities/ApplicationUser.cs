using Microsoft.AspNetCore.Identity;
using VacationHome.Auth.Domain.Enums;


namespace VacationHome.Auth.Domain.Entities
{


    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;


        public string LastName { get; set; } = string.Empty;


        public UserStatus Status { get; set; } = UserStatus.Active;


        public int RefreshTokenVersion { get; set; }


        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;


        public DateTime? UpdatedAtUtc { get; set; }
    }
}
