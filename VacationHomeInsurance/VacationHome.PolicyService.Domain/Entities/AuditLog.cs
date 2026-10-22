namespace VacationHome.PolicyService.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }


        public Guid ActorId { get; set; }


        public string Action { get; set; } = string.Empty;


        public string EntityType { get; set; } = string.Empty;


        public string EntityId { get; set; } = string.Empty;


        public string Details { get; set; } = string.Empty;


        public DateTime CreatedAtUtc { get; set; }
    }
}
