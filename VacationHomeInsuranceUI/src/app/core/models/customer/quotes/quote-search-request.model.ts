import { AdminDecision } from './admin-decision.enum';
import { QuoteStatus } from './quote-status.enum';

export interface QuoteSearchRequest {
	pageNumber: number;
	pageSize: 10 | 25 | 50;
	propertyId: string | null;
	productId: string | null;
	status: QuoteStatus | null;
	generatedFromUtc: string | null;
	generatedToUtc: string | null;
	customerAccepted: boolean | null;
	adminDecision: AdminDecision | null;
	paymentEligible: boolean | null;
}
