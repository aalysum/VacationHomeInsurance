import { PaymentStatus } from './payment-status.enum';

export interface PaymentResult {
	paymentId: string;
	paymentNumber: string;
	status: PaymentStatus;
	policyId: string | null;
	message: string;
}
