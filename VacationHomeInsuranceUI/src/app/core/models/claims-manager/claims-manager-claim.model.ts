import { ClaimCoverage } from '../customer/claims/claim-coverage.model';
import { ClaimStatus } from '../customer/claims/claim-status.enum';
import { ClaimType } from '../customer/claims/claim-type.enum';

export interface ClaimsManagerClaim {
	id: string;
	claimNumber: string;
	customerId: string;
	policyId: string;
	policyNumber: string;
	propertyId: string;
	type: ClaimType;
	status: ClaimStatus;
	title: string;
	claimedAmount: number;
	approvedAmount: number | null;
	incidentDate: string;
	submittedAtUtc: string;
	coverages: ClaimCoverage[];
	rowVersion: string;
	assignedClaimsManagerId: string | null;
	assignedClaimsManagerName: string | null;
	assignedClaimsManagerEmail: string | null;
	assignedAtUtc: string | null;
}
