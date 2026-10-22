export interface PaymentRequest {
	quoteId: string;
	cardholderName: string;
	cardNumber: string;
	expiryMonth: number;
	expiryYear: number;
	cvv: string;
	amount: number;
}
