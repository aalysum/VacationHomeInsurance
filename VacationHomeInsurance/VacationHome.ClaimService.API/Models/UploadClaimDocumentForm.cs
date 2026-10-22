using Microsoft.AspNetCore.Http;
using VacationHome.ClaimService.Domain.Enums;


namespace VacationHome.ClaimService.API.Models
{
    public sealed class UploadClaimDocumentForm
    {
        public ClaimDocumentType DocumentType
        {
            get;
            set;
        }


        public IFormFile File
        {
            get;
            set;
        } = null!;
    }
}
