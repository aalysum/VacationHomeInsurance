namespace VacationHome.ProductQuote.Domain.Entities
{


    public class AuditLog
    {
        public Guid Id { get; set; }


        public string EntityName { get; set; } = string.Empty;


        public string EntityId { get; set; } = string.Empty;


        public string Action { get; set; } = string.Empty;


        public string? OldValues { get; set; }


        public string? NewValues { get; set; }


        public string? PerformedByRole { get; set; }


        public string CorrelationId { get; set; }


        public DateTime PerformedAtUtc { get; set; }
            = DateTime.UtcNow;
    }
}
