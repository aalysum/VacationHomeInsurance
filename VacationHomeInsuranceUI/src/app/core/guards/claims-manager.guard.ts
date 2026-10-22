import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { catchError, map, Observable, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenRefreshService } from '../services/token-refresh.service';
import { TokenStorageService } from '../services/token-storage.service';

export const claimsManagerGuard: CanActivateFn = (
  _route,
  state
): | boolean
  | UrlTree
  | Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const tokenStorage = inject(TokenStorageService);
  const tokenRefreshService = inject(TokenRefreshService);
  const router = inject(Router);

  /*
   * The frontend already has a valid authenticated
   * session.
   */
  if (authService.isAuthenticated()) {
    /*
     * Authentication alone is insufficient.
     * The user must have the ClaimsManager role.
     */
    return authService.isClaimsManager()
      ? true
      : router.parseUrl('/access-denied');
  }

  /*
   * The access token may have expired while the refresh
   * token is still available.
   */
  const refreshToken = tokenStorage.getRefreshToken();

  /*
   * Without a refresh token, the authenticated session
   * cannot be restored.
   */
  if (!refreshToken) {
    return createClaimsManagerLoginUrlTree(router, state.url);
  }

  /*
   * Restore the authenticated session before checking
   * the ClaimsManager role.
   */
  return tokenRefreshService
    .refreshSession()
    .pipe(
      map(() => {
        if (!authService.isAuthenticated()) {
          return createClaimsManagerLoginUrlTree(router, state.url);
        }

        return authService.isClaimsManager()
          ? true
          : router.parseUrl('/access-denied');
      }),
      catchError(() => {
        authService.clearSessionAfterRefreshFailure();
        return of(createClaimsManagerLoginUrlTree(router, state.url));
      })
    );
};

function createClaimsManagerLoginUrlTree(
  router: Router,
  requestedUrl: string
): UrlTree {
  const returnUrl = isSafeInternalReturnUrl(requestedUrl)
    ? requestedUrl
    : '/claims-manager/dashboard';

  return router.createUrlTree(['/auth/login'], {
    queryParams: { returnUrl }
  });
}

function isSafeInternalReturnUrl(url: string): boolean {
  return (
    Boolean(url) &&
    url.startsWith('/') &&
    !url.startsWith('//') &&
    !url.includes('\\') &&
    url !== '/auth' &&
    !url.startsWith('/auth/')
  );
}
