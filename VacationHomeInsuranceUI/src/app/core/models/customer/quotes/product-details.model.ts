import { ProductCoverage } from './product-coverage.model';
import { ProductType } from './product-type.enum';

export interface ProductDetails {
	id: string;
	name: string;
	description: string;
	productType: ProductType;
	isActive: boolean;
	rowVersion: string;
	coverages: ProductCoverage[];
}
