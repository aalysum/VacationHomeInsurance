namespace VacationHome.Auth.Domain.Entities
{


    public class LoginAttempt
    {
        public Guid Id { get; set; }


        public string? UserId { get; set; }


        public string Email { get; set; } = string.Empty;


        public bool Success { get; set; }


        public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
