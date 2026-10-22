export interface PricingConfiguration {
	id: string;
	baseRate: number;
	lowRiskMultiplier: number;
	mediumRiskMultiplier: number;
	highRiskMultiplier: number;
	isActive: boolean;
	createdAtUtc: string;
	updatedAtUtc: string | null;
	rowVersion: string;
}
