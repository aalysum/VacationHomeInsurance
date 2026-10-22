export interface AuthenticatedUser {
	userId: string | null;
	email: string | null;
	roles: string[];
	expiresAtUtc: string;
}
