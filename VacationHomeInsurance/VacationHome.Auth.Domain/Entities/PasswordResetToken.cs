namespace VacationHome.Auth.Domain.Entities
{


    public class PasswordResetToken
    {
        public Guid Id { get; set; }


        public string UserId { get; set; } = string.Empty;


        public string TokenHash { get; set; } = string.Empty;


        public DateTime ExpiresAtUtc { get; set; }


        public DateTime? UsedAtUtc { get; set; }


    }
}
