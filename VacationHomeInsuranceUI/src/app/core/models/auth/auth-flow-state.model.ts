export type ClaimsManagerApplicationStatus =
	| 'PendingApproval'
	| 'Rejected'
	| 'Approved'
	| string;

export interface MfaFlowState {
	challengeToken: string;
}

export interface ClaimsManagerFlowState {
	applicationToken: string;
	status: ClaimsManagerApplicationStatus;
	rejectionReason: string | null;
}
