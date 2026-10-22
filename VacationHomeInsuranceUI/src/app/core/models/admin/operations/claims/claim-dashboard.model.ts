import { ClaimSummary } from './claim-summary.model';

export interface ClaimDashboard {
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
	recentClaims: ClaimSummary[];
}
