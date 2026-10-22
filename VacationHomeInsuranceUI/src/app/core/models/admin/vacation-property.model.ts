import { OccupancyFrequency } from './occupancy-frequency.enum';
import { PropertyType } from './property-type.enum';

export interface VacationProperty {
	propertyId: string;
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
	propertyAge: number;
	occupancyFrequency: OccupancyFrequency;
	hasSecuritySystem: boolean;
	previousClaimsCount: number;
	disasterExposure: boolean;
	isArchived: boolean;
	archivedAtUtc: string | null;
	createdAtUtc: string;
	updatedAtUtc: string | null;
	rowVersion: string;
}
