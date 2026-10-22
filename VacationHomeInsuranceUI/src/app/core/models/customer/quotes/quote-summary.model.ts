import { AdminDecision } from './admin-decision.enum';
import { QuoteStatus } from './quote-status.enum';

export interface QuoteSummary {
	id: string;
	quoteNumber: string;
	propertyId: string;
	propertyName: string;
	productId: string;
	productName: string;
	status: QuoteStatus;
	finalAnnualPremium: number;
	generatedAtUtc: string;
	expiresAtUtc: string;
	customerAccepted: boolean;
	adminDecision: AdminDecision | null;
	paymentDeadlineUtc: string | null;
}
