import { CustomerStatus } from './customer-status.enum';

export interface AdminCustomerSummary {
	customerId: string;
	authUserId: string;
	firstName: string;
	lastName: string;
	phoneNumber: string;
	status: CustomerStatus;
	propertyCount: number;
	createdAtUtc: string;
	updatedAtUtc: string | null;
}
