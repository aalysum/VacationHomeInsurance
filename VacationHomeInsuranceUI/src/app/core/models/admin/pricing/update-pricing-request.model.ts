export interface UpdatePricingRequest {
	baseRate: number;
	lowRiskMultiplier: number;
	mediumRiskMultiplier: number;
	highRiskMultiplier: number;
	rowVersion: string;
}
