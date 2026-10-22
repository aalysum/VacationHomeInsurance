import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenRefreshService } from '../services/token-refresh.service';
import { TokenStorageService } from '../services/token-storage.service';

export const customerGuard: CanActivateFn = (
  route,
  state
) => {
  const authService = inject(AuthService);
  const tokenStorage = inject(TokenStorageService);
  const tokenRefreshService = inject(TokenRefreshService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return authService.isCustomer()
      ? true
      : router.parseUrl('/access-denied');
  }

  const refreshToken = tokenStorage.getRefreshToken();

  if (!refreshToken) {
    return createLoginUrlTree(router, state.url);
  }

  return tokenRefreshService
    .refreshSession()
    .pipe(
      map(() => {
        if (!authService.isAuthenticated()) {
          return createLoginUrlTree(router, state.url);
        }

        return authService.isCustomer()
          ? true
          : router.parseUrl('/access-denied');
      }),
      catchError(() => {
        authService.clearSessionAfterRefreshFailure();
        return of(createLoginUrlTree(router, state.url));
      })
    );
};

function createLoginUrlTree(router: Router, requestedUrl: string) {
  return router.createUrlTree(['/auth/login'], {
    queryParams: {
      returnUrl: requestedUrl.startsWith('/')
        ? requestedUrl
        : '/customer/dashboard'
    }
  });
}
