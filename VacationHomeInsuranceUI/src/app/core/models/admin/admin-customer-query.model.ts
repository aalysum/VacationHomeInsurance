import { CustomerStatus } from './customer-status.enum';
import { SortDirection } from './sort-direction.type';

export interface AdminCustomerQuery {
	pageNumber: number;
	pageSize: 10 | 25 | 50;
	name?: string;
	status?: CustomerStatus;
	sortBy: string;
	sortDirection: SortDirection;
}
