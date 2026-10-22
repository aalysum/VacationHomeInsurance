using VacationHome.ClaimService.Application.DTOs.Admin;
using VacationHome.ClaimService.Application.DTOs.Claims;
using VacationHome.ClaimService.Application.DTOs.Documents;
using VacationHome.ClaimService.Domain.Entities;


namespace VacationHome.ClaimService.Application.Mappers
{
    public static class ClaimMapper
    {
        public static ClaimDto ToDto(
    this Claim claim)
        {
            ArgumentNullException.ThrowIfNull(
                claim);


            return new ClaimDto
            {
                Id =
                    claim.Id,


                ClaimNumber =
                    claim.ClaimNumber,


                CustomerId =
                    claim.CustomerId,


                PolicyId =
                    claim.PolicyId,


                PolicyNumber =
                    claim.PolicyNumber,


                PropertyId =
                    claim.PropertyId,


                Type =
                    claim.Type,


                Status =
                    claim.Status,


                Title =
                    claim.Title,


                ClaimedAmount =
                    claim.ClaimedAmount,


                ApprovedAmount =
                    claim.ApprovedAmount,


                IncidentDate =
                    claim.IncidentDate,


                SubmittedAtUtc =
                    claim.SubmittedAtUtc,


                AssignedClaimsManagerId =
                    claim.AssignedClaimsManagerId,


                AssignedClaimsManagerName =
                    claim.AssignedClaimsManagerName,


                AssignedClaimsManagerEmail =
                    claim.AssignedClaimsManagerEmail,


                AssignedAtUtc =
                    claim.AssignedAtUtc,


                Coverages =
                    claim.Coverages
                        .OrderBy(
                            coverage =>
                                coverage.CoverageName)
                        .Select(
                            ToCoverageDto)
                        .ToArray(),


                RowVersion =
                    Convert.ToBase64String(
                        claim.RowVersion)
            };
        }


        public static ClaimSummaryDto ToSummaryDto(
     this Claim claim)
        {
            ArgumentNullException.ThrowIfNull(
                claim);


            return new ClaimSummaryDto
            {
                Id =
                    claim.Id,


                ClaimNumber =
                    claim.ClaimNumber,


                CustomerId =
                    claim.CustomerId,


                PolicyId =
                    claim.PolicyId,


                PolicyNumber =
                    claim.PolicyNumber,


                Type =
                    claim.Type,


                Status =
                    claim.Status,


                Title =
                    claim.Title,


                ClaimedAmount =
                    claim.ClaimedAmount,


                ApprovedAmount =
                    claim.ApprovedAmount,


                CoverageCount =
                    claim.Coverages.Count,


                SubmittedAtUtc =
                    claim.SubmittedAtUtc,


                AssignedClaimsManagerId =
                    claim.AssignedClaimsManagerId,


                AssignedClaimsManagerName =
                    claim.AssignedClaimsManagerName
            };
        }
        public static ClaimDetailDto ToDetailDto(
     this Claim claim)
        {
            ArgumentNullException.ThrowIfNull(
                claim);


            return new ClaimDetailDto
            {
                Id =
                    claim.Id,


                ClaimNumber =
                    claim.ClaimNumber,


                CustomerId =
                    claim.CustomerId,


                PolicyId =
                    claim.PolicyId,


                PolicyNumber =
                    claim.PolicyNumber,


                PropertyId =
                    claim.PropertyId,


                Type =
                    claim.Type,


                Status =
                    claim.Status,


                IncidentDate =
                    claim.IncidentDate,


                Title =
                    claim.Title,


                Description =
                    claim.Description,


                ClaimedAmount =
                    claim.ClaimedAmount,


                ApprovedAmount =
                    claim.ApprovedAmount,


                RejectionReason =
                    claim.RejectionReason,


                SubmittedAtUtc =
                    claim.SubmittedAtUtc,


                AssignedClaimsManagerId =
                    claim.AssignedClaimsManagerId,


                AssignedClaimsManagerName =
                    claim.AssignedClaimsManagerName,


                AssignedClaimsManagerEmail =
                    claim.AssignedClaimsManagerEmail,


                AssignedAtUtc =
                    claim.AssignedAtUtc,


                Coverages =
                    claim.Coverages
                        .OrderBy(
                            coverage =>
                                coverage.CoverageName)
                        .Select(
                            ToCoverageDto)
                        .ToArray(),


                Documents =
                    claim.Documents
                        .OrderByDescending(
                            document =>
                                document.UploadedAtUtc)
                        .Select(
                            document =>
                                new ClaimDocumentDto
                                {
                                    Id =
                                        document.Id,


                                    DocumentType =
                                        document.DocumentType,


                                    FileName =
                                        document.FileName,


                                    ContentType =
                                        document.ContentType,


                                    FileSizeBytes =
                                        document.FileSizeBytes,


                                    UploadedAtUtc =
                                        document.UploadedAtUtc
                                })
                        .ToArray(),


                Timeline =
                    claim.StatusHistory
                        .OrderBy(
                            history =>
                                history.ChangedAtUtc)
                        .Select(
                            history =>
                                new ClaimStatusHistoryDto
                                {
                                    PreviousStatus =
                                        history.PreviousStatus,


                                    NewStatus =
                                        history.NewStatus,


                                    Comments =
                                        history.Comments,


                                    ChangedBy =
                                        history.ChangedBy,


                                    ChangedAtUtc =
                                        history.ChangedAtUtc
                                })
                        .ToArray(),


               
                AssignmentHistory =
                    Array.Empty<
                        ClaimAssignmentHistoryDto>(),


                RowVersion =
                    Convert.ToBase64String(
                        claim.RowVersion)
            };
        }


        private static ClaimCoverageDto
            ToCoverageDto(
                ClaimCoverage coverage)
        {
            return new ClaimCoverageDto
            {
                Id =
                    coverage.Id,


                CoverageId =
                    coverage.CoverageId,


                CoverageName =
                    coverage.CoverageName,


                IsMandatoryCoverage =
                    coverage.IsMandatoryCoverage,


                CoverageCharge =
                    coverage.CoverageCharge,


                CoverageLimitPercentage =
                    coverage.CoverageLimitPercentage,


                CoverageLimitAmount =
                    coverage.CoverageLimitAmount,


                RequestedAmount =
                    coverage.RequestedAmount,


                ApprovedAmount =
                    coverage.ApprovedAmount
            };
        }
    }
}
