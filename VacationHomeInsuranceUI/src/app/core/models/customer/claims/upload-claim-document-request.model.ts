import { ClaimDocumentType } from './claim-document-type.enum';

export interface UploadClaimDocumentRequest {
	documentType: ClaimDocumentType;
	fileName: string;
	contentType: string;
	fileSizeBytes: number;
	storagePath: string;
}
