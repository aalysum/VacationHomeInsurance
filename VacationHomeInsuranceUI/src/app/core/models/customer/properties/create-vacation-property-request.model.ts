import { OccupancyFrequency } from '../../admin/occupancy-frequency.enum';
import { PropertyType } from '../../admin/property-type.enum';

export interface CreateVacationPropertyRequest {
	propertyName: string;
	propertyType: PropertyType;
	addressLine1: string;
	addressLine2: string | null;
	city: string;
	state: string;
	postalCode: string;
	country: string;
	propertyValue: number;
	constructionYear: number;
	occupancyFrequency: OccupancyFrequency;
	hasSecuritySystem: boolean;
	previousClaimsCount: number;
	disasterExposure: boolean;
}
