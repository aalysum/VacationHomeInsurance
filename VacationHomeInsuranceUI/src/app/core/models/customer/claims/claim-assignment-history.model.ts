export interface ClaimAssignmentHistory {
	id: string;
	previousClaimsManagerId: string | null;
	previousClaimsManagerName: string | null;
	previousClaimsManagerEmail: string | null;
	claimsManagerId: string;
	claimsManagerName: string;
	claimsManagerEmail: string;
	reason: string | null;
	assignedBy: string;
	assignedAtUtc: string;
}
