import { CustomerStatus } from '../admin/customer-status.enum';

export interface CustomerProfile {
	customerId: string;
	firstName: string;
	lastName: string;
	phoneNumber: string;
	status: CustomerStatus;
	dateOfBirth: string | null;
	addressLine1: string | null;
	addressLine2: string | null;
	city: string | null;
	state: string | null;
	postalCode: string | null;
	country: string | null;
}
