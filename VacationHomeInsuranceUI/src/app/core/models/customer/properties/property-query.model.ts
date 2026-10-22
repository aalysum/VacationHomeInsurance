export interface PropertyQuery {
	pageNumber: number;
	pageSize: number;
	includeArchived: boolean;
	sortBy: string;
	sortDirection:
		| 'asc'
		| 'desc';
}
