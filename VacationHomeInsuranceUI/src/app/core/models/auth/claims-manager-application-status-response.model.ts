export interface ClaimsManagerApplicationStatusResponse {
	applicationId: string;
	status: string;
	rejectionReason: string | null;
	submittedAtUtc: string;
	approvedAtUtc: string | null;
}
