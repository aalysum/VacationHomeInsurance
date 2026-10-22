import { HttpErrorResponse } from '@angular/common/http';

import {
	ApiError,
	ApiValidationErrors
} from '../models/auth/api-error.model';

export interface ParsedApiError {
	code: string | null;
	message: string;
	fieldErrors: ApiValidationErrors;
}

export function parseApiError(
	error: unknown
): ParsedApiError {
	if (!(error instanceof HttpErrorResponse)) {
		return {
			code: null,
			message:
				'Unable to complete the request. Please try again.',
			fieldErrors: {}
		};
	}

	const response = isObject(error.error)
		? error.error as ApiError
		: null;
	const fieldErrors =
		normalizeFieldErrors(response?.errors);
	const firstValidationMessage =
		Object.values(fieldErrors)
			.flat()
			.find(message => Boolean(message));
	const backendError =
		typeof (response as any)?.error === 'string'
			? (response as any).error
			: null;
	const code =
		response?.code ??
		response?.errorCode ??
		null;
	return {
		code,
		message:
			getBusinessMessage(code) ??
			firstValidationMessage ??
			response?.message ??
			response?.errorMessage ??
			response?.detail ??
			backendError ??
			getFallbackMessage(error.status),
		fieldErrors
	};
}

function getBusinessMessage(
	code: string | null
): string | null {
	switch (code) {
		case 'COMPREHENSIVE_POLICY_CONFLICT':
		case 'COMPREHENSIVE_POLICY_EXISTS':
			return 'Property Protection and Natural Disaster Protection may coexist, but neither may coexist with Comprehensive Protection on the same property.';
		default:
			return null;
	}
}

function normalizeFieldErrors(
	errors: ApiValidationErrors | undefined
): ApiValidationErrors {
	if (!errors) {
		return {};
	}

	return Object.entries(errors).reduce(
		(
			result,
			[fieldName, messages]
		) => {
			const normalizedName =
				fieldName.length > 0
					? fieldName[0].toLowerCase() +
						fieldName.slice(1)
					: fieldName;
			result[normalizedName] =
				Array.isArray(messages)
					? messages
					: [];
			return result;
		},
		{} as ApiValidationErrors
	);
}

function getFallbackMessage(
	status: number
): string {
	switch (status) {
		case 0:
			return 'The API Gateway could not be reached. Make sure it is running.';
		case 400:
			return 'Please review the submitted information and try again.';
		case 401:
			return 'Authentication is required.';
		case 403:
			return 'You do not have permission to perform this action.';
		case 404:
			return 'The requested service was not found.';
		case 409:
			return 'The submitted information conflicts with an existing record.';
		case 500:
			return 'The request could not be completed because of a server error. Please try again later.';
		default:
			return 'Unable to complete the request. Please try again.';
	}
}

function isObject(
	value: unknown
): value is Record<string, unknown> {
	return (
		typeof value === 'object' &&
		value !== null
	);
}
