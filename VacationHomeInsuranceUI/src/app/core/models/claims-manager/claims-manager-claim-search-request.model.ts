import { ClaimStatus } from '../customer/claims/claim-status.enum';
import { ClaimType } from '../customer/claims/claim-type.enum';

export interface ClaimsManagerClaimSearchRequest {
	status?: ClaimStatus | null;
	claimType?: ClaimType | null;
	claimNumber?: string | null;
	customerId?: string | null;
	fromDate?: string | null;
	toDate?: string | null;
	pageNumber: number;
	pageSize: number;
}
