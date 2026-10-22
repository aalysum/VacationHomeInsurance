import {
  HttpBackend,
  HttpClient
} from '@angular/common/http';


import {
  inject,
  Injectable
} from '@angular/core';


import {
  catchError,
  finalize,
  Observable,
  shareReplay,
  tap,
  throwError
} from 'rxjs';


import {
  TokenStorageService
} from './token-storage.service';


import {
  LoginResponse,
  isCompletedAuthenticationResponse
} from '../models/auth/login-response.model';


import {
  RefreshTokenRequest
} from '../models/auth/refresh-token-request.model';


import {
  AUTH_ENDPOINTS
} from '../constants/auth-endpoints';


import {
  buildApiUrl
} from '../config/api-url.util';


@Injectable({
  providedIn: 'root'
})
export class TokenRefreshService {
  private readonly httpBackend =
    inject(HttpBackend);


  private readonly tokenStorage =
    inject(TokenStorageService);


  /*
   * HttpClient created from HttpBackend bypasses all
   * registered Angular HTTP interceptors.
   *
   * This prevents the refresh request from entering the
   * authentication interceptor and causing a refresh loop.
   */
  private readonly refreshHttp =
    new HttpClient(
      this.httpBackend
    );


  /*
   * All simultaneous 401 responses share this one active
   * refresh request.
   */
  private activeRefreshRequest:
    Observable<LoginResponse> | null =
    null;


  refreshSession():
    Observable<LoginResponse> {
    /*
     * If a refresh call is already running, return its
     * shared Observable instead of sending another request.
     */
    if (this.activeRefreshRequest) {
      return this.activeRefreshRequest;
    }


    const refreshToken =
      this.tokenStorage
        .getRefreshToken();


    if (!refreshToken) {
      return throwError(
        () =>
          new Error(
            'No refresh token is available.'
          )
      );
    }


    const request:
      RefreshTokenRequest = {
      refreshToken
    };


    const refreshRequest =
      this.refreshHttp
        .post<LoginResponse>(
          buildApiUrl(
            AUTH_ENDPOINTS.refresh
          ),
          request
        )
        .pipe(
          tap(response => {            /*
             * RefreshTokenAsync returns the same model as
             * completed authentication.
             *
             * It must contain a new access token, rotated
             * refresh token, and expiration date.
             */
            if (
              !isCompletedAuthenticationResponse(
                response
              )
            ) {
              throw new Error(
                'The refresh response is incomplete.'
              );
            }


          }),


          catchError(error => {
            /*
             * Any refresh failure invalidates the local
             * authenticated session.
             */
            return throwError(
              () => error
            );
          }),


          finalize(() => {
             /*
             * Clear the shared request reference after
             * the refresh succeeds or fails.
             */
            this.activeRefreshRequest =
              null;
          }),


          /*
           * Share the same refresh result with all
           * protected requests that failed concurrently.
           */
          shareReplay({
            bufferSize: 1,
            refCount: false
          })
        );


    this.activeRefreshRequest =
      refreshRequest;


    return refreshRequest;
  }


  isRefreshing(): boolean {
    return (
      this.activeRefreshRequest !==
      null
    );
  }
}
