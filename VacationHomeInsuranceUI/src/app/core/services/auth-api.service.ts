import {
  HttpClient
} from '@angular/common/http';


import {
  inject,
  Injectable
} from '@angular/core';


import {
  Observable
} from 'rxjs';


import {
  buildApiUrl
} from '../config/api-url.util';


import {
  AUTH_ENDPOINTS
} from '../constants/auth-endpoints';


import {
  LoginRequest
} from '../models/auth/login-request.model';


import {
  LoginResponse
} from '../models/auth/login-response.model';


import {
  RegisterCustomerRequest
} from '../models/auth/register-customer-request.model';


import {
  RegisterClaimsManagerRequest
} from '../models/auth/register-claims-manager-request.model';


import {
  MessageResponse
} from '../models/auth/message-response.model';


import {
  ClaimsManagerRegistrationResponse
} from '../models/auth/claims-manager-registration-response.model';


import {
  ForgotPasswordRequest
} from '../models/auth/forgot-password-request.model';


import {
  ResetPasswordRequest
} from '../models/auth/reset-password-request.model';


import {
  VerifyMfaRequest
} from '../models/auth/verify-mfa-request.model';


import {
  ClaimsManagerApplicationTokenRequest
} from '../models/auth/claims-manager-application-token-request.model';


import {
  ClaimsManagerApplicationStatusResponse
} from '../models/auth/claims-manager-application-status-response.model';


import {
  ReapplyClaimsManagerRequest
} from '../models/auth/reapply-claims-manager-request.model';


import {
  ClaimsManagerReapplicationResponse
} from '../models/auth/claims-manager-reapplication-response.model';


import {
  RefreshTokenRequest
} from '../models/auth/refresh-token-request.model';


import {
  LogoutRequest
} from '../models/auth/logout-request.model';


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
export class AuthApiService {
  private readonly http =
    inject(HttpClient);


beginMfaSetup():
  Observable<MfaSetupResponse> {
  return this.http.post<MfaSetupResponse>(
    buildApiUrl(
      AUTH_ENDPOINTS.beginMfaSetup
    ),
    {}
  );
}


confirmMfaSetup(
  request: ConfirmMfaSetupRequest
): Observable<void> {
  return this.http.post<void>(
    buildApiUrl(
      AUTH_ENDPOINTS.confirmMfaSetup
    ),
    request
  );
}


disableMfa(
  request: ConfirmMfaSetupRequest
): Observable<void> {
  return this.http.post<void>(
    buildApiUrl(
      AUTH_ENDPOINTS.disableMfa
    ),
    request
  );
}


login(
  request: LoginRequest
): Observable<LoginResponse> {
  return this.http.post<LoginResponse>(
    buildApiUrl(AUTH_ENDPOINTS.login),
    request
  );
}


  registerCustomer(
    request: RegisterCustomerRequest
  ): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(
      buildApiUrl(
        AUTH_ENDPOINTS.registerCustomer
      ),
      request
    );
  }


 resetPassword(
  request: ResetPasswordRequest
): Observable<MessageResponse> {
  return this.http.post<MessageResponse>(
    buildApiUrl(
      AUTH_ENDPOINTS.resetPassword
    ),
    request
  );
} 


logout(
  request: LogoutRequest
): Observable<void> {
  return this.http.post<void>(
    buildApiUrl(
      AUTH_ENDPOINTS.logout
    ),
    request
  );
}




refreshToken(
  request: RefreshTokenRequest
): Observable<LoginResponse> {
  return this.http.post<LoginResponse>(
    buildApiUrl(
      AUTH_ENDPOINTS.refresh
    ),
    request
  );
}
verifyMfa(
  request: VerifyMfaRequest
): Observable<LoginResponse> {
  return this.http.post<LoginResponse>(
    buildApiUrl(
      AUTH_ENDPOINTS.verifyMfa
    ),
    request);
}


changePassword(
  request: ChangePasswordRequest
): Observable<void> {
  return this.http.put<void>(
    buildApiUrl(
      AUTH_ENDPOINTS.changePassword
    ),
    request
  );
}


  forgotPassword(
  request: ForgotPasswordRequest
): Observable<MessageResponse> {
  return this.http.post<MessageResponse>(
    buildApiUrl(
      AUTH_ENDPOINTS.forgotPassword
    ),
    request
  );
}


reapplyClaimsManager(
  applicationToken: string,
  request: ReapplyClaimsManagerRequest,
  verificationDocument: File
): Observable<ClaimsManagerReapplicationResponse> {
  const formData =
    new FormData();


  formData.append(
    'applicationToken',
    applicationToken
  );


  formData.append(
    'firstName',
    request.firstName
  );


  formData.append(
    'lastName',
    request.lastName
  );


  formData.append(
    'phoneNumber',
    request.phoneNumber
  );


  formData.append(
    'yearsOfExperience',
    request.yearsOfExperience.toString()
  );


  formData.append(
    'professionalSummary',
    request.professionalSummary
  );


  formData.append(
    'verificationDocument',
    verificationDocument,
    verificationDocument.name
  );


  return this.http.post<
    ClaimsManagerReapplicationResponse
  >(
    buildApiUrl(
      AUTH_ENDPOINTS.reapplyClaimsManager
    ),
    formData
  );
}


getClaimsManagerApplicationStatus(
  request: ClaimsManagerApplicationTokenRequest
): Observable<ClaimsManagerApplicationStatusResponse> {
  return this.http.post<
    ClaimsManagerApplicationStatusResponse
  >(
    buildApiUrl(
      AUTH_ENDPOINTS.claimsManagerApplicationStatus
    ),
    request
  );
}


  registerClaimsManager(
    request: RegisterClaimsManagerRequest,
    verificationDocument: File
  ): Observable<ClaimsManagerRegistrationResponse> {
    const formData =
      new FormData();


    formData.append(
      'firstName',
      request.firstName
    );


    formData.append(
      'lastName',
      request.lastName
    );


    formData.append(
      'email',
      request.email
    );


    formData.append(
      'phoneNumber',
      request.phoneNumber
    );


    formData.append(
      'yearsOfExperience',
      request.yearsOfExperience.toString()
    );


    formData.append(
      'professionalSummary',
      request.professionalSummary
    );


    formData.append(
      'password',
      request.password
    );


    formData.append(
      'confirmPassword',
      request.confirmPassword
    );


    formData.append(
      'verificationDocument',
      verificationDocument,
      verificationDocument.name
    );


    return this.http.post<
      ClaimsManagerRegistrationResponse
    >(
      buildApiUrl(
        AUTH_ENDPOINTS.registerClaimsManager
      ),
      formData
    );
  }
}
