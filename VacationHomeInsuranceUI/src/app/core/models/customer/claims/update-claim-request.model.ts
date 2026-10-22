import { ClaimCoverageRequest } from './claim-coverage-request.model';
import { ClaimType } from './claim-type.enum';

export interface UpdateClaimRequest {
	type: ClaimType;
	incidentDate: string;
	title: string;
	description: string;
	coverages: ClaimCoverageRequest[];
	rowVersion: string;
}
