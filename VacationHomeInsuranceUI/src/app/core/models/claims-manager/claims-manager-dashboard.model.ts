import { ClaimsManagerClaimSummary } from './claims-manager-claim-summary.model';

export interface ClaimsManagerDashboard {
	totalClaims: number;
	submittedClaims: number;
	underReviewClaims: number;
	informationRequestedClaims: number;
	approvedClaims: number;
	rejectedClaims: number;
	paidClaims: number;
	closedClaims: number;
	totalClaimedAmount: number;
	totalApprovedAmount: number;
	recentClaims: ClaimsManagerClaimSummary[];
}
