import { ClaimCoverage } from './claim-coverage.model';
import { ClaimStatus } from './claim-status.enum';
import { ClaimType } from './claim-type.enum';

export interface Claim {
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
}
