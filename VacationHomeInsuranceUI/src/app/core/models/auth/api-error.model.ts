export interface ApiValidationErrors {
	[fieldName: string]: string[];
}

export interface ApiError {
	code?: string;
	message?: string;
	errorCode?: string;
	errorMessage?: string;
	title?: string;
	detail?: string;
	status?: number;
	errors?: ApiValidationErrors;
}
