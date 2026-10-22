import { ProductType } from '../quotes/product-type.enum';
import { ProductCoverage } from './product-coverage.model';

export interface ProductDetails {
	id: string;
	name: string;
	description: string;
	productType: ProductType;
	isActive: boolean;
	rowVersion: string;
	coverages: ProductCoverage[];
}
