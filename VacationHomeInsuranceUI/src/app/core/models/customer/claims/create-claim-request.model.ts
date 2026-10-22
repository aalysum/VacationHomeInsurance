import { ClaimCoverageRequest } from './claim-coverage-request.model';
import { ClaimType } from './claim-type.enum';

export interface CreateClaimRequest {
	policyId: string;
	propertyId: string;
	type: ClaimType;
	incidentDate: string;
	title: string;
	description: string;
	coverages: ClaimCoverageRequest[];
}
