export interface LoginResponse {
	accessToken: string | null;
	refreshToken: string | null;
	expiresAtUtc: string | null;
	requiresMfa: boolean;
	mfaChallengeToken: string | null;
	requiresClaimsManagerAction: boolean;
	claimsManagerApplicationToken: string | null;
	claimsManagerApplicationStatus: string | null;
	rejectionReason: string | null;
}

export type CompletedAuthenticationResponse =
	LoginResponse & {
		accessToken: string;
		refreshToken: string;
		expiresAtUtc: string;
		requiresMfa: false;
		requiresClaimsManagerAction: false;
	};

export function isCompletedAuthenticationResponse(
	response: LoginResponse
): response is CompletedAuthenticationResponse {
	return (
		typeof response.accessToken === 'string' &&
		response.accessToken.length > 0 &&
		typeof response.refreshToken === 'string' &&
		response.refreshToken.length > 0 &&
		typeof response.expiresAtUtc === 'string' &&
		response.expiresAtUtc.length > 0 &&
		response.requiresMfa === false &&
		response.requiresClaimsManagerAction === false
	);
}
