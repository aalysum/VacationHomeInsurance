import { ClaimDocumentType } from './claim-document-type.enum';

export interface ClaimDocument {
	id: string;
	documentType: ClaimDocumentType;
	fileName: string;
	contentType: string;
	fileSizeBytes: number;
	uploadedAtUtc: string;
}
