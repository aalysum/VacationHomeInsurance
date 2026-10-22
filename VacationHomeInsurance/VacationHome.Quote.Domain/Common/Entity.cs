namespace VacationHome.ProductQuote.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; set; }


        public byte[] RowVersion { get; set; } =
            Array.Empty<byte>();
    }
}
