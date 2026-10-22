import { ClaimAssignmentHistory } from './claim-assignment-history.model';
import { ClaimCoverage } from './claim-coverage.model';
import { ClaimDocument } from './claim-document.model';
import { ClaimStatusHistory } from './claim-status-history.model';
import { ClaimStatus } from './claim-status.enum';
import { ClaimType } from './claim-type.enum';

export interface ClaimDetails {
	id: string;
	claimNumber: string;
	customerId: string;
	policyId: string;
	policyNumber: string;
	propertyId: string;
	type: ClaimType;
	status: ClaimStatus;
	incidentDate: string;
	submittedAtUtc: string;
	title: string;
	description: string;
	claimedAmount: number;
	approvedAmount: number | null;
	rejectionReason: string | null;
	assignedClaimsManagerId: string | null;
	assignedClaimsManagerName: string | null;
	assignedClaimsManagerEmail: string | null;
	assignedAtUtc: string | null;
	coverages: ClaimCoverage[];
	documents: ClaimDocument[];
	timeline: ClaimStatusHistory[];
	assignmentHistory: ClaimAssignmentHistory[];
	rowVersion: string;
}
