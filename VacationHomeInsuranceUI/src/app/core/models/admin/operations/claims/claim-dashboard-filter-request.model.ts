import { ClaimStatus } from './claim-status.enum';
import { ClaimType } from './claim-type.enum';

export interface ClaimDashboardFilterRequest {
	status?: ClaimStatus | null;
	claimType?: ClaimType | null;
	claimNumber?: string | null;
	customerId?: string | null;
	fromDate?: string | null;
	toDate?: string | null;
	pageNumber: number;
	pageSize: number;
}
