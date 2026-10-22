export interface ApproveQuoteRequest {
	remarks: string | null;
	revisedAnnualPremium: number | null;
	priceAdjustmentReason: string | null;
	rowVersion: string;
}
