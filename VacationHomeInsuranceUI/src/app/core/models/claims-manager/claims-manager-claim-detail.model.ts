import { ClaimAssignmentHistory } from '../admin/operations/claims/claim-assignment-history.model';
import { ClaimCoverage } from '../customer/claims/claim-coverage.model';
import { ClaimDocument } from '../customer/claims/claim-document.model';
import { ClaimStatusHistory } from '../customer/claims/claim-status-history.model';
import { ClaimStatus } from '../customer/claims/claim-status.enum';
import { ClaimType } from '../customer/claims/claim-type.enum';

export interface ClaimsManagerCustomerDetails {
	firstName: string;
	lastName: string;
	email: string;
	phoneNumber: string;
}

export interface ClaimsManagerPropertyDetails {
	name: string;
	type: string;
	addressLine1: string;
	addressLine2: string | null;
	city: string;
	state: string;
	postalCode: string;
	country: string;
	propertyValue: number;
	constructionYear: number;
	occupancyFrequency: string;
	hasSecuritySystem: boolean;
}

export interface ClaimsManagerPolicyDocument {
	id: string;
	version: number;
	status: string;
	contentType: string;
	fileSizeBytes: number;
	createdAtUtc: string;
	generatedAtUtc: string | null;
}

export interface ClaimsManagerPolicyDetails {
	policyNumber: string;
	productName: string;
	status: string;
	effectiveDateUtc: string;
	expiryDateUtc: string;
	annualPremium: number;
	coverages: Array<{
		coverageId: string;
		coverageName: string;
		isMandatory: boolean;
		coverageCharge: number;
		coverageLimitPercentage: number;
		coverageLimitAmount: number;
	}>;
	documents: ClaimsManagerPolicyDocument[];
}

export interface ClaimsManagerClaimDetail {
	id: string;
	claimNumber: string;
	customerId: string;
	policyId: string;
	policyNumber: string;
	propertyId: string;
	customer: ClaimsManagerCustomerDetails | null;
	property: ClaimsManagerPropertyDetails | null;
	policy: ClaimsManagerPolicyDetails | null;
	type: ClaimType;
	status: ClaimStatus;
	incidentDate: string;
	title: string;
	description: string;
	claimedAmount: number;
	approvedAmount: number | null;
	rejectionReason: string | null;
	submittedAtUtc: string;
	coverages: ClaimCoverage[];
	documents: ClaimDocument[];
	timeline: ClaimStatusHistory[];
	rowVersion: string;
	assignedClaimsManagerId: string | null;
	assignedClaimsManagerName: string | null;
	assignedClaimsManagerEmail: string | null;
	assignedAtUtc: string | null;
	assignmentHistory: ClaimAssignmentHistory[];
}
