import { ApproveClaimCoverageRequest } from './approve-claim-coverage-request.model';

export interface ApproveClaimRequest {
	coverages: ApproveClaimCoverageRequest[];
	notes: string | null;
	rowVersion: string;
}
