namespace VacationHome.Auth.Application.DTOs
{
    public class ClaimsManagerDocumentDto
    {
        public string FileName { get; set; } = string.Empty;


        public string ContentType { get; set; } = string.Empty;


        public byte[] Content { get; set; } = Array.Empty<byte>();
    }
}
