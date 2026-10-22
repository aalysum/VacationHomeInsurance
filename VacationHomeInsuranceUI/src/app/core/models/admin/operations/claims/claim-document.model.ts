export interface ClaimDocument {
	id: string;
	documentType: number;
	fileName: string;
	contentType: string;
	fileSizeBytes: number;
	uploadedAtUtc: string;
}
