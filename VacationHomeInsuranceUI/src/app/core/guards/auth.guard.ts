import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { catchError, map, Observable, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenRefreshService } from '../services/token-refresh.service';
import { TokenStorageService } from '../services/token-storage.service';

export const authGuard: CanActivateFn = (
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
   * The frontend already has a valid authenticated-user
   * state, so allow access to the protected route.
   */
  if (authService.isAuthenticated()) {
    return true;
  }

  /*
   * The access token may have expired while the refresh
   * token is still valid.
   */
  const refreshToken = tokenStorage.getRefreshToken();

  /*
   * Without a refresh token, the session cannot be
   * restored.
   */
  if (!refreshToken) {
    return createLoginUrlTree(router, state.url);
  }

  /*
   * Attempt to restore the authenticated session before
   * activating the protected route.
   *
   * TokenRefreshService shares one refresh operation when
   * multiple guarded requests trigger refresh concurrently.
   */
  return tokenRefreshService
    .refreshSession()
    .pipe(
      map(() => {
        /*
         * Refresh succeeded. TokenRefreshService should
         * have applied the refreshed session through
         * AuthService.
         */
        if (authService.isAuthenticated()) {
          return true;
        }

        /*
         * Do not allow route activation if the refresh
         * response did not restore authenticated state.
         */
        authService.clearSessionAfterRefreshFailure();
        return createLoginUrlTree(router, state.url);
      }),
      catchError(() => {
        /*
         * The refresh token may be invalid, expired,
         * revoked, or already used.
         *
         * Clear the local authenticated session before
         * redirecting to login.
         */
        authService.clearSessionAfterRefreshFailure();
        return of(createLoginUrlTree(router, state.url));
      })
    );
};

function createLoginUrlTree(
  router: Router,
  requestedUrl: string
): UrlTree {
  const returnUrl = isSafeInternalReturnUrl(requestedUrl)
    ? requestedUrl
    : null;

  /*
   * Do not include an unsafe or invalid return URL.
   */
  if (!returnUrl) {
    return router.parseUrl('/auth/login');
  }

  return router.createUrlTree(['/auth/login'], {
    queryParams: { returnUrl }
  });
}

function isSafeInternalReturnUrl(url: string): boolean {
  if (
    !url ||
    !url.startsWith('/') ||
    url.startsWith('//') ||
    url.includes('\\')
  ) {
    return false;
  }

  /*
   * Prevent redirect loops if a protected route somehow
   * resolves back to an Auth page.
   */
  if (url === '/auth' || url.startsWith('/auth/')) {
    return false;
  }

  return true;
}
