export interface PolicyPagedResult<T> {
	items: T[];
	pageNumber: number;
	pageSize: number;
	totalCount: number;
}
