import { ClaimStatus } from '../customer/claims/claim-status.enum';
import { ClaimType } from '../customer/claims/claim-type.enum';

export interface ClaimsManagerClaimSummary {
	id: string;
	claimNumber: string;
	customerId: string;
	policyId: string;
	policyNumber: string;
	type: ClaimType;
	status: ClaimStatus;
	title: string;
	claimedAmount: number;
	approvedAmount: number | null;
	coverageCount: number;
	submittedAtUtc: string;
	assignedClaimsManagerId: string | null;
	assignedClaimsManagerName: string | null;
}
