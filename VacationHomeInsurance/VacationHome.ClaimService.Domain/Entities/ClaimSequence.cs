namespace VacationHome.ClaimService.Domain.Entities
{
    public sealed class ClaimSequence
    {
        public int Id { get; set; }


        public long NextValue { get; set; }
    }
}
