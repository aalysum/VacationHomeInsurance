import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest
} from '@angular/common/http';

import {
  inject
} from '@angular/core';

import {
  Router
} from '@angular/router';

import {
  catchError,
  Observable,
  switchMap,
  throwError
} from 'rxjs';

import {
  environment
} from '../../../environments/environment';

import {
  TokenStorageService
} from '../services/token-storage.service';

import {
  TokenRefreshService
} from '../services/token-refresh.service';

import {
  AuthService
} from '../services/auth.service';

import {
  GlobalNotificationService
} from '../services/global-notification.service';

import {
  parseApiError
} from '../utils/api-error.util';

import {
  AUTH_RETRY_ATTEMPTED
} from './auth-interceptor.context';

const PUBLIC_AUTH_PATHS: readonly string[] = [
  '/auth/customers/register',
  '/auth/claims-managers/register',
  '/auth/login',
  '/auth/refresh',
  '/auth/forgot-password',
  '/auth/reset-password',
  '/auth/claims-managers/application-status',
  '/auth/claims-managers/reapply',
  '/auth/mfa/verify'
];

export const authInterceptor:
  HttpInterceptorFn =
  (
    request,
    next
  ) => {
    const tokenStorage =
      inject(TokenStorageService);
    const tokenRefreshService =
      inject(TokenRefreshService);
    const authService =
      inject(AuthService);
    const notifications =
      inject(GlobalNotificationService);
    const router =
      inject(Router);

    /*
     * Never attach application tokens to unrelated
     * external URLs.
     */
    if (!isApiGatewayRequest(request.url)) {
      return next(request);
    }

    const publicAuthRequest =
      isPublicAuthenticationRequest(
        request.url
      );

    const accessToken =
      tokenStorage.getAccessToken();

    const requestWithAuthentication =
      !publicAuthRequest &&
      accessToken
        ? attachBearerToken(
            request,
            accessToken
          )
        : request;

    return next(
      requestWithAuthentication
    ).pipe(
      catchError(error => {
        if (!(error instanceof HttpErrorResponse)) {
          return throwError(
            () => error
          );
        }

        /*
         * HTTP 403 indicates a permission failure.
         * Never attempt refresh for 403 responses.
         */
        if (error.status !== 401) {
          if (
            error.status === 0 ||
            error.status === 403
          ) {
            notifications.showError(
              parseApiError(error).message
            );
          }

          return throwError(
            () => error
          );
        }

        /*
         * Public Auth requests must not trigger refresh.
         */
        if (publicAuthRequest) {
          notifications.showError(
            parseApiError(error).message
          );
          return throwError(
            () => error
          );
        }

        /*
         * The original request may only be retried once.
         */
        if (
          request.context.get(
            AUTH_RETRY_ATTEMPTED
          )
        ) {
          notifications.showError(
            parseApiError(error).message
          );
          clearSessionAndRedirect(
            authService,
            router
          );

          return throwError(
            () => error
          );
        }

        const refreshToken =
          tokenStorage.getRefreshToken();

        if (!refreshToken) {
          notifications.showError(
            parseApiError(error).message
          );
          clearSessionAndRedirect(
            authService,
            router
          );

          return throwError(
            () => error
          );
        }

        return tokenRefreshService
          .refreshSession()
          .pipe(
            switchMap(
              refreshResponse => {
                const newAccessToken =
                  refreshResponse
                    .accessToken;

                if (!newAccessToken) {
                  clearSessionAndRedirect(
                    authService,
                    router
                  );

                  return throwError(
                    () =>
                      new Error(
                        'The refreshed access token is missing.'
                      )
                  );
                }

                authService
                  .applyRefreshedSession(
                    refreshResponse
                  );

                const retryRequest =
                  request.clone({
                    setHeaders: {
                      Authorization:
                        `Bearer ${newAccessToken}`
                    },
                    context:
                      request.context.set(
                        AUTH_RETRY_ATTEMPTED,
                        true
                      )
                  });

                return next(
                  retryRequest
                );
              }
            ),

            catchError(
              refreshError => {
                notifications.showError(
                  parseApiError(refreshError).message
                );
                clearSessionAndRedirect(
                  authService,
                  router
                );

                return throwError(
                  () => refreshError
                );
              }
            )
          );

      })

    );

  };

function isApiGatewayRequest(
  url: string
): boolean {
  const baseUrl =
    environment
      .apiGatewayBaseUrl
      .replace(/\/+$/, '');

  return (
    url === baseUrl ||
    url.startsWith(
      `${baseUrl}/`
    )
  );
}

function isPublicAuthenticationRequest(
  url: string
): boolean {
  return PUBLIC_AUTH_PATHS.some(
    path =>
      getUrlPath(url) === path
  );
}

function getUrlPath(
  url: string
): string {
  try {
    return new URL(url).pathname;
  } catch {
    return url;
  }
}

function attachBearerToken<T>(
  request: HttpRequest<T>,
  accessToken: string
): HttpRequest<T> {
  return request.clone({
    setHeaders: {
      Authorization:
        `Bearer ${accessToken}`
    }
  });
}

function clearSessionAndRedirect(
  authService: AuthService,
  router: Router
): void {
  authService
    .clearSessionAfterRefreshFailure();

  const currentUrl =
    router.url;

  const returnUrl =
    isSafeReturnUrl(currentUrl)
      ? currentUrl
      : null;

  if (returnUrl) {
    void router.navigate(
      ['/auth/login'],
      {
        queryParams: {
          returnUrl
        }
      }
    );

    return;
  }

  void router.navigateByUrl(
    '/auth/login'
  );
}

function isSafeReturnUrl(
  url: string
): boolean {
  if (
    !url ||
    url === '/' ||
    url.startsWith('/auth/')
  ) {
    return false;
  }

  /*
   * Only internal Angular URLs are preserved.
   */
  return url.startsWith('/');
}
