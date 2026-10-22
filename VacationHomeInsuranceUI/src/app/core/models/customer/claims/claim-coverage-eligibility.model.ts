export interface ClaimCoverageEligibility {
	rulesConfigured: boolean;
	message: string;
	coverages: EligibleClaimCoverage[];
}

export interface EligibleClaimCoverage {
	coverageId: string;
	coverageName: string;
	coverageLimitAmount: number;
	remainingLimitAmount: number;
}
