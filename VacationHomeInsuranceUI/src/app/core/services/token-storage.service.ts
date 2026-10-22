import {
  Injectable,
  PLATFORM_ID,
  inject
} from '@angular/core';

import { isPlatformBrowser } from '@angular/common';


import {
  LoginResponse
} from '../models/auth/login-response.model';


export interface MfaFlowState {
  challengeToken: string;
}


export interface ClaimsManagerFlowState {
  applicationToken: string;
  status: string;
  rejectionReason: string | null;
}


@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private readonly browserStorageAvailable =
    isPlatformBrowser(inject(PLATFORM_ID));

  private readonly accessTokenKey =
    'securesphere.accessToken';


  private readonly refreshTokenKey =
    'securesphere.refreshToken';


  private readonly expiresAtKey =
    'securesphere.expiresAtUtc';


  private readonly mfaFlowKey =
    'securesphere.mfaFlow';


  private readonly claimsManagerFlowKey =
    'securesphere.claimsManagerFlow';


  getAccessToken(): string | null {
    if (!this.browserStorageAvailable) return null;

    return localStorage.getItem(
      this.accessTokenKey
    );
  }


  getRefreshToken(): string | null {
    if (!this.browserStorageAvailable) return null;

    return localStorage.getItem(
      this.refreshTokenKey
    );
  }


  getExpiresAtUtc(): string | null {
    if (!this.browserStorageAvailable) return null;

    return localStorage.getItem(
      this.expiresAtKey
    );
  }


  saveAuthenticatedSession(
    response: LoginResponse
  ): void {
    if (!this.browserStorageAvailable) return;

    if (
      !response.accessToken ||
      !response.refreshToken ||
      !response.expiresAtUtc
    ) {
      throw new Error(
        'The authentication response is incomplete.'
      );
    }


    localStorage.setItem(
      this.accessTokenKey,
      response.accessToken
    );


    localStorage.setItem(
      this.refreshTokenKey,
      response.refreshToken
    );


    localStorage.setItem(
      this.expiresAtKey,
      response.expiresAtUtc
    );


    this.clearTemporaryFlows();
  }


  replaceAuthenticatedSession(
    response: LoginResponse
  ): void {
    this.saveAuthenticatedSession(response);
  }


  saveMfaFlow(
    challengeToken: string
  ): void {
    if (!this.browserStorageAvailable) return;

    const state: MfaFlowState = {
      challengeToken
    };


    sessionStorage.setItem(
      this.mfaFlowKey,
      JSON.stringify(state)
    );


    this.clearClaimsManagerFlow();
    this.clearAuthenticatedSession();
  }


  getMfaFlow(): MfaFlowState | null {
    return this.readSessionValue<MfaFlowState>(
      this.mfaFlowKey
    );
  }


  clearMfaFlow(): void {
    if (!this.browserStorageAvailable) return;

    sessionStorage.removeItem(
      this.mfaFlowKey
    );
  }


  saveClaimsManagerFlow(
    state: ClaimsManagerFlowState
  ): void {
    if (!this.browserStorageAvailable) return;

    sessionStorage.setItem(
      this.claimsManagerFlowKey,
      JSON.stringify(state)
    );


    this.clearMfaFlow();
    this.clearAuthenticatedSession();
  }


  updateClaimsManagerFlowStatus(
    status: string,
    rejectionReason: string | null
  ): void {
    const currentState =
      this.getClaimsManagerFlow();


    if (!currentState) {
      return;
    }


    this.saveClaimsManagerFlow({
      ...currentState,
      status,
      rejectionReason
    });
  }


  getClaimsManagerFlow():
    ClaimsManagerFlowState | null {
    return this.readSessionValue<
      ClaimsManagerFlowState
    >(
      this.claimsManagerFlowKey
    );
  }


  clearClaimsManagerFlow(): void {
    if (!this.browserStorageAvailable) return;

    sessionStorage.removeItem(
      this.claimsManagerFlowKey
    );
  }


  clearAuthenticatedSession(): void {
    if (!this.browserStorageAvailable) return;

    localStorage.removeItem(
      this.accessTokenKey
    );


    localStorage.removeItem(
      this.refreshTokenKey
    );


    localStorage.removeItem(
      this.expiresAtKey
    );
  }


  clearTemporaryFlows(): void {
    this.clearMfaFlow();
    this.clearClaimsManagerFlow();
  }


  clearAll(): void {
    this.clearAuthenticatedSession();
    this.clearTemporaryFlows();
  }
  
  clearAllAuthStorage(): void {
    if (!this.browserStorageAvailable) return;

  localStorage.removeItem(
    'securesphere.accessToken'
  );


  localStorage.removeItem(
    'securesphere.refreshToken'
  );


  localStorage.removeItem(
    'securesphere.expiresAtUtc'
  );


  sessionStorage.removeItem(
    'securesphere.mfaFlow'
  );


  sessionStorage.removeItem(
    'securesphere.claimsManagerFlow'
  );
}
  private readSessionValue<T>(
    key: string
  ): T | null {
    if (!this.browserStorageAvailable) return null;

    const storedValue =
      sessionStorage.getItem(key);


    if (!storedValue) {
      return null;
    }


    try {
      return JSON.parse(
        storedValue
      ) as T;
    } catch {
      sessionStorage.removeItem(key);
      return null;
    }
  }
}
