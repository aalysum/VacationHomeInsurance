import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { catchError, map, Observable, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenRefreshService } from '../services/token-refresh.service';
import { TokenStorageService } from '../services/token-storage.service';

export const adminGuard: CanActivateFn = (
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
   * The user already has a valid authenticated
   * frontend session.
   */
  if (authService.isAuthenticated()) {
    /*
     * Authentication alone is insufficient.
     * The user must also have the Admin role.
     */
    return authService.isAdmin()
      ? true
      : router.parseUrl('/access-denied');
  }

  /*
   * The access token may have expired while the
   * refresh token remains valid.
   */
  const refreshToken = tokenStorage.getRefreshToken();

  /*
   * Without a refresh token, the session cannot
   * be restored.
   */
  if (!refreshToken) {
    return createAdminLoginUrlTree(router, state.url);
  }

  /*
   * Restore the authenticated session before
   * evaluating the Admin role.
   */
  return tokenRefreshService
    .refreshSession()
    .pipe(
      map(() => {
        /*
         * The refresh request completed, but route
         * activation still requires authenticated state.
         */
        if (!authService.isAuthenticated()) {
          return createAdminLoginUrlTree(router, state.url);
        }

        /*
         * Allow only the Admin role.
         */
        return authService.isAdmin()
          ? true
          : router.parseUrl('/access-denied');
      }),
      catchError(() => {
        /*
         * Refresh failed. Clear every local session value
         * and send the user to Login.
         */
        authService.clearSessionAfterRefreshFailure();
        return of(createAdminLoginUrlTree(router, state.url));
      })
    );
};

function createAdminLoginUrlTree(
  router: Router,
  requestedUrl: string
): UrlTree {
  const returnUrl = isSafeInternalReturnUrl(requestedUrl)
    ? requestedUrl
    : '/admin/dashboard';

  return router.createUrlTree(
    ['/auth/login'],
    {
      queryParams: { returnUrl }
    }
  );
}

function isSafeInternalReturnUrl(url: string): boolean {
  return (
    Boolean(url) &&
    url.startsWith('/') &&
    !url.startsWith('//') &&
    !url.startsWith('/auth/')
  );
}
