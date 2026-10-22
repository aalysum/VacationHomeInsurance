import { CoverageType } from './coverage-type.enum';

export interface ProductCoverage {
	id: string;
	coverageId: string;
	coverageName: string;
	coverageDescription: string;
	coverageType: CoverageType;
	coverageCharge: number;
	coverageLimitPercentage: number;
	displayOrder: number;
	isActive: boolean;
	rowVersion: string;
}
