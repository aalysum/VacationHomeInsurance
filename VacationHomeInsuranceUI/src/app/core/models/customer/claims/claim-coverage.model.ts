export interface ClaimCoverage {
	id: string;
	coverageId: string;
	coverageName: string;
	isMandatoryCoverage: boolean;
	coverageCharge: number;
	coverageLimitPercentage: number;
	coverageLimitAmount: number;
	requestedAmount: number;
	approvedAmount: number | null;
}
