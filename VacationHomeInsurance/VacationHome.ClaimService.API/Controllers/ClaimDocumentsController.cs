using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationHome.ClaimService.API.Models;
using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Application.Interfaces.Services;


namespace VacationHome.ClaimService.API.Controllers
{
    [ApiController]
    [Route("claims/{claimId:guid}/documents")]
    [Authorize]
    public sealed class ClaimDocumentsController : ControllerBase
    {
        private readonly IClaimDocumentService _documentService;


        public ClaimDocumentsController(
            IClaimDocumentService documentService)
        {
            _documentService = documentService;
        }


        [HttpGet]
        public async Task<IActionResult> GetDocuments(
            Guid claimId)
        {
            return Ok(
                await _documentService.GetDocumentsAsync(
                    claimId));
        }


        [HttpGet("{documentId:guid}")]
        public async Task<IActionResult> GetDocument(
            Guid claimId,
            Guid documentId)
        {
            return Ok(
                await _documentService.GetDocumentAsync(
                    claimId,
                    documentId));
        }

        [HttpGet("{documentId:guid}/download")]
        public async Task<IActionResult> Download(
            Guid claimId,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            var document =
                await _documentService.GetDocumentContentAsync(
                    claimId,
                    documentId,
                    cancellationToken);

            return File(
                document.Content,
                document.ContentType,
                document.FileName);
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
            Guid claimId,
            [FromForm] UploadClaimDocumentForm request,
            CancellationToken cancellationToken)
        {
            ClaimDocumentDto result =
                await _documentService.AddDocumentAsync(
                    claimId,
                    request.DocumentType,
                    request.File,
                    cancellationToken);


            return Ok(result);
        }


        [HttpDelete("{documentId:guid}")]
        public async Task<IActionResult> Delete(
            Guid claimId,
            Guid documentId)
        {
            await _documentService.DeleteDocumentAsync(
                claimId,
                documentId);


            return NoContent();
        }




    }
}
