namespace VacationHome.ProductQuote.Domain.Entities
{
    public class RiskConfiguration
    {
        public Guid Id { get; set; }


        public decimal MediumPropertyValueThreshold { get; set; }


        public decimal HighPropertyValueThreshold { get; set; }


        public bool IsActive { get; set; }


        public DateTime CreatedAtUtc { get; set; }


        public DateTime? UpdatedAtUtc { get; set; }


        public byte[] RowVersion { get; set; } =
            Array.Empty<byte>();
    }
}
