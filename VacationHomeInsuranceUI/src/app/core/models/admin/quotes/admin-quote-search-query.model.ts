import { QuoteStatus } from './quote-status.enum';

export interface AdminQuoteSearchQuery {
	pageNumber: number;
	pageSize: 10 | 25 | 50;
	sortBy?: string;
	sortDescending: boolean;
	customerId?: string;
	propertyId?: string;
	productId?: string;
	status?: QuoteStatus;
	generatedFromUtc?: string;
	generatedToUtc?: string;
	decisionFromUtc?: string;
	decisionToUtc?: string;
}
