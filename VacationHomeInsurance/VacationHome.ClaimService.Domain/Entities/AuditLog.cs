namespace VacationHome.ClaimService.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }


        public string EntityType { get; set; } = string.Empty;


        public string EntityId { get; set; } = string.Empty;


        public string Action { get; set; } = string.Empty;


        public string? PerformedBy { get; set; }


        public DateTime PerformedAtUtc { get; set; }


        public string? Details { get; set; }
    }
}
