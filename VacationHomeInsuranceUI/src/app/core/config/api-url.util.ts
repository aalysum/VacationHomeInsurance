import { environment } from '../../../environments/environment';

export function buildApiUrl(path: string): string {
  const baseUrl = environment.apiGatewayBaseUrl.replace(/\/+$/, '');
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;

  return `${baseUrl}${normalizedPath}`;
}
