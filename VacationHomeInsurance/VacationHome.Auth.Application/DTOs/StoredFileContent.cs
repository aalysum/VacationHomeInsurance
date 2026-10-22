namespace VacationHome.Auth.Application.DTOs
{
    public sealed class StoredFileContent
    {
        public Stream Content { get; set; } =
            Stream.Null;


        public string ContentType { get; set; } =
            "application/octet-stream";


        public string DownloadFileName { get; set; } =
            string.Empty;
    }
}
