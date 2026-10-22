import { ClaimStatus } from './claim-status.enum';

export interface ClaimSearchRequest {
	claimNumber?: string | null;
	status?: ClaimStatus | null;
	pageNumber: number;
	pageSize: number;
}
