import { ProductType } from '../quotes/product-type.enum';
import { PolicyStatus } from './policy-status.enum';
import { PolicyDocument } from '../../policy-document.model';

export interface PolicySummary {
	id: string;
	policyNumber: string;
	propertyId: string;
	propertyName: string;
	productType: ProductType;
	productName: string;
	annualPremium: number;
	effectiveDateUtc: string;
	expiryDateUtc: string;
	status: PolicyStatus;
	documents: PolicyDocument[];
}
