export interface JwtPayload {
	sub?: string;
	email?: string;
	role?: string | string[];
	roles?: string | string[];
	exp?: number;
	session_version?: string;
	[claimName: string]: unknown;
}
