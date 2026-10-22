import { CoverageType } from './coverage-type.enum';

export interface QuoteCoverage {
	coverageId: string;
	coverageName: string;
	coverageDescription: string;
	coverageType: CoverageType;
	coverageCharge: number;
	coverageLimitPercentage: number;
	coverageLimitAmount: number;
	displayOrder: number;
}
