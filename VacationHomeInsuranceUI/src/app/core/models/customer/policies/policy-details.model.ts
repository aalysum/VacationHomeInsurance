import { ProductType } from '../quotes/product-type.enum';
import { PolicyCoverage } from './policy-coverage.model';
import { PolicyStatus } from './policy-status.enum';
import { PolicyDocument } from '../../policy-document.model';

export interface PolicyDetails {
	id: string;
	policyNumber: string;
	quoteId: string;
	customerId: string;
	propertyId: string;
	propertyName: string;
	productType: ProductType;
	productName: string;
	annualPremium: number;
	effectiveDateUtc: string;
	expiryDateUtc: string;
	status: PolicyStatus;
	coverages: PolicyCoverage[];
	documents: PolicyDocument[];
}
