import { CustomerStatus } from './customer-status.enum';

export interface AdminCustomerDetails {
	customerId: string;
	authUserId: string;
	firstName: string;
	lastName: string;
	phoneNumber: string;
	status: CustomerStatus;
	city: string | null;
	state: string | null;
	postalCode: string | null;
	country: string | null;
	propertyCount: number;
	createdAtUtc: string;
	updatedAtUtc: string | null;
}
