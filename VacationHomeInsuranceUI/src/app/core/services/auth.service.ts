import {
  computed,
  inject,
  Injectable,
  signal
} from '@angular/core';


import {
  Router
} from '@angular/router';


import {
  finalize
} from 'rxjs';


import {
  AuthApiService
} from './auth-api.service';


import {
  TokenStorageService
} from './token-storage.service';


import {
  LoginRequest
} from '../models/auth/login-request.model';


import {
  LoginResponse
} from '../models/auth/login-response.model';


import {
  AuthenticatedUser
} from '../models/auth/authenticated-user.model';


import {
  decodeJwtPayload,
  getJwtEmail,
  getJwtRoles,
  getJwtUserId
} from '../utils/jwt.util';


import {
  parseApiError
} from '../utils/api-error.util';


import {
  ClaimsManagerApplicationStatusResponse
} from '../models/auth/claims-manager-application-status-response.model';


import {
  ReapplyClaimsManagerRequest
} from '../models/auth/reapply-claims-manager-request.model';


import {
LogoutRequest
} from '../models/auth/logout-request.model';


import {
  Observable,
  tap
} from 'rxjs';


import {
  ChangePasswordRequest
} from '../models/auth/change-password-request.model';


import {
  ConfirmMfaSetupRequest
} from '../models/auth/confirm-mfa-setup-request.model';


import {
  MfaSetupResponse
} from '../models/auth/mfa-setup-response.model';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly authApi =
    inject(AuthApiService);


  private readonly tokenStorage =
    inject(TokenStorageService);


  private readonly router =
    inject(Router);


  /*
   * Authenticated user state.
   */
  private readonly currentUserState =
    signal<AuthenticatedUser | null>(
      this.restoreCurrentUser()
    );


  /*
   * Login state.
   */
  private readonly loginLoadingState =
    signal(false);


  private readonly loginErrorState =
    signal<string | null>(null);


  /*
   * MFA verification state.
   */
  private readonly mfaVerificationLoadingState =
    signal(false);


  private readonly mfaVerificationErrorState =
    signal<string | null>(null);


  private readonly mfaChallengeExpiredState =
    signal(false);


  /*
   * Public authenticated-user Signals.
   */
  readonly currentUser =
    this.currentUserState.asReadonly();


  readonly isAuthenticated =
    computed(
      () =>
        this.currentUserState() !== null
    );


  readonly roles =
    computed(
      () =>
        this.currentUserState()?.roles ?? []
    );


  readonly isCustomer =
    computed(
      () =>
        this.hasRole('Customer')
    );


  readonly isClaimsManager =
    computed(
      () =>
        this.hasRole('ClaimsManager')
    );


  readonly isAdmin =
    computed(
      () =>
        this.hasRole('Admin')
    );


    private readonly claimsManagerStatusLoadingState =
  signal(false);


private readonly claimsManagerStatusState =
  signal<
    ClaimsManagerApplicationStatusResponse | null
  >(null);


private readonly claimsManagerStatusErrorState =
  signal<string | null>(null);


readonly claimsManagerStatusLoading =
  this.claimsManagerStatusLoadingState.asReadonly();


readonly claimsManagerApplicationStatus =
  this.claimsManagerStatusState.asReadonly();


readonly claimsManagerStatusErrorMessage =
  this.claimsManagerStatusErrorState.asReadonly();


  /*
   * Public login Signals.
   */
  readonly loginLoading =
    this.loginLoadingState.asReadonly();


  readonly loginErrorMessage =
    this.loginErrorState.asReadonly();


  /*
   * Public MFA Signals.
   */
  readonly mfaVerificationLoading =
    this.mfaVerificationLoadingState.asReadonly();


  readonly mfaVerificationErrorMessage =
    this.mfaVerificationErrorState.asReadonly();


  readonly mfaChallengeExpired =
    this.mfaChallengeExpiredState.asReadonly();


 private readonly claimsManagerReapplicationLoadingState =
  signal(false);


private readonly claimsManagerReapplicationSuccessState =
  signal<string | null>(null);


private readonly claimsManagerReapplicationErrorState =
  signal<string | null>(null);


private readonly claimsManagerReapplicationFieldErrorsState =
  signal<Record<string, string[]>>({});


readonly claimsManagerReapplicationLoading =
  this.claimsManagerReapplicationLoadingState.asReadonly();


readonly claimsManagerReapplicationSuccessMessage =
  this.claimsManagerReapplicationSuccessState.asReadonly();


readonly claimsManagerReapplicationErrorMessage =
  this.claimsManagerReapplicationErrorState.asReadonly();


readonly claimsManagerReapplicationFieldErrors =
  this.claimsManagerReapplicationFieldErrorsState.asReadonly();


  private readonly authApiService =
  inject(AuthApiService);


private readonly tokenStorageService =
  inject(TokenStorageService);






 beginMfaSetup():
  Observable<MfaSetupResponse> {
  return this.authApiService
    .beginMfaSetup();
}


confirmMfaSetup(
  request: ConfirmMfaSetupRequest
): Observable<void> {
  return this.authApiService
    .confirmMfaSetup(request)
    .pipe(
      tap(() => {
        this.clearSessionAfterRefreshFailure();
      })
    );
}


disableMfa(
  request: ConfirmMfaSetupRequest
): Observable<void> {
  return this.authApiService
    .disableMfa(request)
    .pipe(
      tap(() => {
        this.clearSessionAfterRefreshFailure();
      })
    );
} 


  changePassword(
  request: ChangePasswordRequest
): Observable<void> {
  return this.authApiService
    .changePassword(request)
    .pipe(
      tap(() => {
        this.clearSessionAfterRefreshFailure();
      })
    );
}


logout(): void {
  const refreshToken =
    this.tokenStorageService
      .getRefreshToken();


  if (!refreshToken) {
    this.completeLocalLogout();
    return;
  }


  const request: LogoutRequest = {
    refreshToken
  };


  this.authApiService
    .logout(request)
    .subscribe({
      next: () => {
        this.completeLocalLogout();
      },
      error: () => {
        this.completeLocalLogout();
      }
    });
}


private completeLocalLogout(): void {
  this.clearSessionAfterRefreshFailure();


  void this.router.navigate(
    [
      '/auth/login'
    ],
    {
      replaceUrl: true
    }
  );
}


  reapplyClaimsManager(
  request: ReapplyClaimsManagerRequest,
  verificationDocument: File
): void {
  if (this.claimsManagerReapplicationLoadingState()) {
    return;
  }


  const flow = this.tokenStorage.getClaimsManagerFlow();


  if (!flow?.applicationToken) {
    this.claimsManagerReapplicationErrorState.set(
      'Your application session is unavailable. Please sign in again.'
    );
    return;
  }


  if (flow.status.toLowerCase() !== 'rejected') {
    this.claimsManagerReapplicationErrorState.set(
      'Only a rejected Claims Manager application can be resubmitted.'
    );
    return;
  }


  


  this.claimsManagerReapplicationLoadingState.set(true);
  this.claimsManagerReapplicationSuccessState.set(null);
  this.claimsManagerReapplicationErrorState.set(null);
  this.claimsManagerReapplicationFieldErrorsState.set({});


  this.authApi
    .reapplyClaimsManager(
      flow.applicationToken,
      request,
      verificationDocument
    )
    .pipe(
      finalize(() => {
        this.claimsManagerReapplicationLoadingState.set(false);
      })
    )
    .subscribe({
      next: response => {
        this.claimsManagerReapplicationSuccessState.set(
          response.message
        );
        this.claimsManagerReapplicationErrorState.set(null);
        this.claimsManagerReapplicationFieldErrorsState.set({});


        /*
         * Preserve the restricted token but update the
         * current application state to PendingApproval.
         */
        this.tokenStorage.updateClaimsManagerFlowStatus(
          response.status,
          null
        );
      },
      error: error => {
        const parsedError = parseApiError(error);


        this.claimsManagerReapplicationErrorState.set(
          parsedError.message
        );


        this.claimsManagerReapplicationFieldErrorsState.set(
          parsedError.fieldErrors
        );


        if (
          parsedError.code ===
          'INVALID_CLAIMS_MANAGER_APPLICATION_TOKEN'
        ) {
          this.tokenStorage.clearClaimsManagerFlow();
        }
      }
    });
}




clearClaimsManagerReapplicationState(): void {
  this.claimsManagerReapplicationLoadingState.set(false);
  this.claimsManagerReapplicationSuccessState.set(null);
  this.claimsManagerReapplicationErrorState.set(null);
  this.claimsManagerReapplicationFieldErrorsState.set({});
}


clearClaimsManagerReapplicationErrors(): void {
  this.claimsManagerReapplicationErrorState.set(null);
  this.claimsManagerReapplicationFieldErrorsState.set({});
}


returnToClaimsManagerStatus(): void {
  this.clearClaimsManagerReapplicationState();


  void this.router.navigateByUrl(
    '/auth/claims-manager/application-status'
  );
}


loadClaimsManagerApplicationStatus(): void {
  if (
    this.claimsManagerStatusLoadingState()
  ) {
    return;
  }


  const flow =
    this.tokenStorage.getClaimsManagerFlow();


  if (!flow?.applicationToken) {
    this.claimsManagerStatusErrorState.set(
      'Your application session is unavailable. Please sign in again.'
    );


    return;
  }


  this.claimsManagerStatusLoadingState.set(
    true
  );


  this.claimsManagerStatusErrorState.set(
    null
  );


  this.claimsManagerStatusState.set(
    null
  );


  this.authApi
    .getClaimsManagerApplicationStatus({
      applicationToken:
        flow.applicationToken
    })
    .pipe(
      finalize(() => {
        this.claimsManagerStatusLoadingState.set(
          false
        );
      })
    )
    .subscribe({
      next: response => {
        this.claimsManagerStatusState.set(
          response
        );


        this.claimsManagerStatusErrorState.set(
          null
        );


        this.tokenStorage
          .updateClaimsManagerFlowStatus(
            response.status,
            response.rejectionReason
          );
      },


      error: error => {
        const parsedError =
          parseApiError(error);


        this.claimsManagerStatusErrorState.set(
          parsedError.message
        );


        if (
          parsedError.code ===
            'INVALID_CLAIMS_MANAGER_APPLICATION_TOKEN' ||
          parsedError.code ===
            'CLAIMS_MANAGER_RESTRICTED_ACCESS_NOT_ALLOWED'
        ) {
          this.tokenStorage
            .clearClaimsManagerFlow();
        }
      }
    });
}
applyRefreshedSession(
  response: LoginResponse
): void {
  if (
    !response.accessToken ||
    !response.refreshToken ||
    !response.expiresAtUtc
  ) {
    throw new Error(
      'The refreshed authentication response is incomplete.'
    );
  }


  const authenticatedUser =
    this.createAuthenticatedUser(
      response.accessToken,
      response.expiresAtUtc
    );


  if (!authenticatedUser) {
    throw new Error(
      'The refreshed access token could not be read.'
    );
  }


  this.tokenStorage
    .replaceAuthenticatedSession(
      response
    );


  this.currentUserState.set(
    authenticatedUser
  );
}
clearSessionAfterRefreshFailure(): void {
  this.tokenStorage.clearAll();


  this.currentUserState.set(
    null
  );


  this.loginErrorState.set(
    null
  );


  this.clearMfaVerificationState();
}


clearClaimsManagerStatusState(): void {
  this.claimsManagerStatusLoadingState.set(
    false
  );


  this.claimsManagerStatusState.set(
    null
  );


  this.claimsManagerStatusErrorState.set(
    null
  );
}


retryClaimsManagerStatus(): void {
  this.loadClaimsManagerApplicationStatus();
}


leaveClaimsManagerApplicationFlow(): void {
  this.tokenStorage
    .clearClaimsManagerFlow();


  this.clearClaimsManagerStatusState();


  void this.router.navigateByUrl(
    '/auth/login'
  );
}


startClaimsManagerReapplication(): void {
  const status =
    this.claimsManagerStatusState()?.status;


  if (
    status?.toLowerCase() !==
    'rejected'
  ) {
    return;
  }


  void this.router.navigateByUrl(
    '/auth/claims-manager/reapply'
  );
}


  login(
    request: LoginRequest
  ): void {
    if (this.loginLoadingState()) {
      return;
    }


    this.loginLoadingState.set(true);
    this.loginErrorState.set(null);


    /*
     * A new login attempt replaces any stale authenticated
     * or temporary authentication workflow.
     */
    this.tokenStorage.clearAll();
    this.currentUserState.set(null);


    this.authApi
      .login(request)
      .pipe(
        finalize(() => {
          this.loginLoadingState.set(false);
        })
      )
      .subscribe({
        next: response => {
          try {
            this.processLoginResponse(response);
          } catch {
            this.tokenStorage.clearAll();
            this.currentUserState.set(null);


            this.loginErrorState.set(
              'The login response could not be processed.'
            );
          }
        },


        error: error => {
          this.tokenStorage.clearAll();
          this.currentUserState.set(null);


          const parsedError =
            parseApiError(error);


          this.loginErrorState.set(
            parsedError.message
          );
        }
      });
  }


  /*
   * Handles one of the three possible successful login
   * response types:
   *
   * 1. Claims Manager restricted access
   * 2. MFA challenge
   * 3. Completed authentication
   */
  private processLoginResponse(
    response: LoginResponse
  ): void {
    /*
     * Claims Manager application workflow.
     *
     * The restricted token must never be stored as an
     * access token.
     */
    if (
      response.requiresClaimsManagerAction
    ) {
      if (
        !response.claimsManagerApplicationToken ||
        !response.claimsManagerApplicationStatus
      ) {
        throw new Error(
          'Incomplete Claims Manager login response.'
        );
      }


      this.tokenStorage
        .saveClaimsManagerFlow({
          applicationToken:
            response.claimsManagerApplicationToken,


          status:
            response.claimsManagerApplicationStatus,


          rejectionReason:
            response.rejectionReason
        });


      this.currentUserState.set(null);


      void this.router.navigateByUrl(
        '/auth/claims-manager/application-status'
      );


      return;
    }


    /*
     * MFA workflow.
     *
     * Authentication is not complete at this point.
     */
    if (response.requiresMfa) {
      if (!response.mfaChallengeToken) {
        throw new Error(
          'Incomplete MFA login response.'
        );
      }


      this.tokenStorage.saveMfaFlow(
        response.mfaChallengeToken
      );


      this.currentUserState.set(null);


      void this.router.navigateByUrl(
        '/auth/mfa/verify'
      );


      return;
    }


    /*
     * Normal completed authentication.
     */
    this.completeAuthentication(response);
  }


  /*
   * Verifies the six-digit authenticator code.
   */
  verifyMfa(
    code: string
  ): void {
    if (
      this.mfaVerificationLoadingState()
    ) {
      return;
    }


    const mfaFlow =
      this.tokenStorage.getMfaFlow();


    if (!mfaFlow?.challengeToken) {
      this.mfaChallengeExpiredState.set(true);


      this.mfaVerificationErrorState.set(
        'Your MFA challenge is unavailable. Please sign in again.'
      );


      return;
    }


    this.mfaVerificationLoadingState.set(true);
    this.mfaVerificationErrorState.set(null);
    this.mfaChallengeExpiredState.set(false);


    this.authApi
      .verifyMfa({
        mfaChallengeToken:
          mfaFlow.challengeToken,


        code
      })
      .pipe(
        finalize(() => {
          this.mfaVerificationLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: response => {
          try {
            this.completeAuthentication(
              response
            );


            this.mfaVerificationErrorState.set(
              null
            );


            this.mfaChallengeExpiredState.set(
              false
            );
          } catch {
            this.tokenStorage.clearAll();
            this.currentUserState.set(null);


            this.mfaVerificationErrorState.set(
              'The authentication response could not be processed.'
            );
          }
        },


        error: error => {
          const parsedError =
            parseApiError(error);


          if (
            parsedError.code ===
            'INVALID_MFA_CHALLENGE'
          ) {
            this.tokenStorage.clearMfaFlow();


            this.mfaChallengeExpiredState.set(
              true
            );


            this.mfaVerificationErrorState.set(
              parsedError.message
            );


            return;
          }


          /*
           * INVALID_MFA_CODE does not clear the challenge.
           * The user can enter a new authenticator code.
           */
          this.mfaVerificationErrorState.set(
            parsedError.message
          );
        }
      });
  }


  /*
   * Completes authentication after either normal login or
   * successful MFA verification.
   */
  completeAuthentication(
    response: LoginResponse
  ): void {
    if (
      !response.accessToken ||
      !response.refreshToken ||
      !response.expiresAtUtc
    ) {
      throw new Error(
        'The authentication response is incomplete.'
      );
    }


    const authenticatedUser =
      this.createAuthenticatedUser(
        response.accessToken,
        response.expiresAtUtc
      );


    if (!authenticatedUser) {
      throw new Error(
        'The access token could not be read.'
      );
    }


    /*
     * This also clears the temporary MFA and Claims Manager
     * flow tokens.
     */
    this.tokenStorage
      .saveAuthenticatedSession(response);


    this.currentUserState.set(
      authenticatedUser
    );


    this.loginErrorState.set(null);
    this.mfaVerificationErrorState.set(null);
    this.mfaChallengeExpiredState.set(false);


    void this.router.navigateByUrl(
      this.getDefaultDashboardUrl()
    );
  }


  clearLoginError(): void {
    this.loginErrorState.set(null);
  }


  clearMfaVerificationError(): void {
    this.mfaVerificationErrorState.set(null);
  }


  clearMfaVerificationState(): void {
    this.mfaVerificationLoadingState.set(false);
    this.mfaVerificationErrorState.set(null);
    this.mfaChallengeExpiredState.set(false);
  }


  cancelMfaVerification(): void {
    this.tokenStorage.clearMfaFlow();
    this.clearMfaVerificationState();


    void this.router.navigateByUrl(
      '/auth/login'
    );
  }


  clearAuthentication(): void {
    this.tokenStorage.clearAll();


    this.currentUserState.set(null);
    this.loginLoadingState.set(false);
    this.loginErrorState.set(null);


    this.clearMfaVerificationState();
  }


  hasRole(
    role: string
  ): boolean {
    return this.roles().some(
      savedRole =>
        savedRole.toLowerCase() ===
        role.toLowerCase()
    );
  }


  getDefaultDashboardUrl(): string {
    if (this.hasRole('Admin')) {
      return '/admin/dashboard';
    }


    if (this.hasRole('ClaimsManager')) {
      return '/claims-manager/dashboard';
    }


    if (this.hasRole('Customer')) {
      return '/customer/dashboard';
    }


    return '/';
  }


  /*
   * Restores the frontend user state after a browser
   * refresh when the access token has not expired.
   */
  private restoreCurrentUser():
    AuthenticatedUser | null {
    const accessToken =
      this.tokenStorage.getAccessToken();


    const expiresAtUtc =
      this.tokenStorage.getExpiresAtUtc();


    if (
      !accessToken ||
      !expiresAtUtc
    ) {
      return null;
    }


    const expiration =
      new Date(expiresAtUtc);


    if (
      Number.isNaN(
        expiration.getTime()
      ) ||
      expiration.getTime() <= Date.now()
    ) {
      /*
       * The expired access token does not create an
       * authenticated frontend state.
       *
       * The refresh interceptor will later attempt refresh
       * using the stored refresh token.
       */
      return null;
    }


    return this.createAuthenticatedUser(
      accessToken,
      expiresAtUtc
    );
  }


  private createAuthenticatedUser(
    accessToken: string,
    expiresAtUtc: string
  ): AuthenticatedUser | null {
    const payload =
      decodeJwtPayload(accessToken);


    if (!payload) {
      return null;
    }


    return {
      userId:
        getJwtUserId(payload),


      email:
        getJwtEmail(payload),


      roles:
        getJwtRoles(payload),


      expiresAtUtc
    };
  }
}
