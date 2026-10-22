namespace VacationHome.Auth.Application.DTOs
{
    public sealed class StoredDocumentResponse
    {
        public string FullPath { get; set; } =
            string.Empty;


        public string DownloadFileName { get; set; } =
            string.Empty;


        public string ContentType { get; set; } =
            "application/octet-stream";
    }
}
