import { ProductType } from '../../quotes/product-type.enum';
import { PolicyStatus } from './policy-status.enum';

export interface PolicyFilterRequest {
	status?: PolicyStatus | null;
	productName?: string | null;
	productType?: ProductType | null;
	pageNumber: number;
	pageSize: number;
}
