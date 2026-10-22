namespace VacationHome.Auth.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }


        public string UserId { get; set; } =
            string.Empty;


        public string TokenHash { get; set; } =
            string.Empty;


        public int TokenVersion { get; set; }


        public DateTime ExpiresAtUtc { get; set; }


        public DateTime CreatedAtUtc { get; set; } =
            DateTime.UtcNow;


        public DateTime? RevokedAtUtc { get; set; }


        public byte[] RowVersion { get; set; } =
            Array.Empty<byte>();


        public bool IsActive =>
            RevokedAtUtc is null &&
            ExpiresAtUtc > DateTime.UtcNow;
    }
}
