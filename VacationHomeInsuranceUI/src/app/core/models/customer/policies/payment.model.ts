import { PaymentStatus } from './payment-status.enum';

export interface Payment {
	id: string;
	paymentNumber: string;
	quoteId: string;
	amount: number;
	status: PaymentStatus;
	attemptNumber: number;
	processedAtUtc: string;
	last4Digits: string;
}
