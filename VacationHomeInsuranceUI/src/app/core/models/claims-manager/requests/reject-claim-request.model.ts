export interface RejectClaimRequest {
	reason: string;
	notes: string | null;
	rowVersion: string;
}
