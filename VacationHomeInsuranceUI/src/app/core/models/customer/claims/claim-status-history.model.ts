import { ClaimStatus } from './claim-status.enum';

export interface ClaimStatusHistory {
	previousStatus: ClaimStatus;
	newStatus: ClaimStatus;
	comments: string;
	changedBy: string;
	changedAtUtc: string;
}
