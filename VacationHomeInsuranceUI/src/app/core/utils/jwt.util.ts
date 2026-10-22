import {
	JwtPayload
} from '../models/auth/jwt-payload.model';

const roleClaim =
	'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

const nameIdentifierClaim =
	'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

const emailClaim =
	'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';

export function decodeJwtPayload(
	token: string
): JwtPayload | null {
	try {
		const tokenParts =
			token.split('.');

		if (tokenParts.length !== 3) {
			return null;
		}

		const base64Url =
			tokenParts[1];
		const base64 =
			base64Url
				.replace(/-/g, '+')
				.replace(/_/g, '/');
		const normalizedBase64 =
			base64.padEnd(
				Math.ceil(
					base64.length / 4
				) * 4,
				'='
			);
		const decodedBytes =
			atob(normalizedBase64);
		const decodedText =
			decodeURIComponent(
				Array.from(decodedBytes)
					.map(character =>
						`%${character
							.charCodeAt(0)
							.toString(16)
							.padStart(2, '0')}`
					)
					.join('')
			);

		return JSON.parse(
			decodedText
		) as JwtPayload;
	} catch {
		return null;
	}
}

export function getJwtRoles(
	payload: JwtPayload
): string[] {
	const value =
		payload[roleClaim] ??
		payload.role ??
		payload.roles;

	if (Array.isArray(value)) {
		return value
			.filter(
				(role): role is string =>
					typeof role === 'string'
			)
			.map(role => role.trim())
			.filter(Boolean);
	}

	if (typeof value === 'string') {
		const role =
			value.trim();
		return role
			? [role]
			: [];
	}

	return [];
}

export function getJwtUserId(
	payload: JwtPayload
): string | null {
	const value =
		payload[nameIdentifierClaim] ??
		payload.sub;

	return typeof value === 'string'
		? value
		: null;
}

export function getJwtEmail(
	payload: JwtPayload
): string | null {
	const value =
		payload[emailClaim] ??
		payload.email;

	return typeof value === 'string'
		? value
		: null;
}
