import { SortDirection } from './sort-direction.type';

export interface AdminCustomerPropertyQuery {
	pageNumber: number;
	pageSize: 10 | 25 | 50;
	includeArchived: boolean;
	sortBy: string;
	sortDirection: SortDirection;
}
