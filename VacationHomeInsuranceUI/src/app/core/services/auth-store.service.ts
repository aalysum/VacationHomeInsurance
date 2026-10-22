import {
  computed,
  inject,
  Injectable,
  signal
} from '@angular/core';


import {
  finalize
} from 'rxjs';


import {
  AuthApiService
} from './auth-api.service';


import {
  RegisterCustomerRequest
} from '../models/auth/register-customer-request.model';


import {
  RegisterClaimsManagerRequest
} from '../models/auth/register-claims-manager-request.model';


import {
  ResetPasswordRequest
} from '../models/auth/reset-password-request.model';


import {
  ApiValidationErrors
} from '../models/auth/api-error.model';


import {
  parseApiError
} from '../utils/api-error.util';


@Injectable({
  providedIn: 'root'
})
export class AuthStoreService {
  private readonly authApi =
    inject(AuthApiService);


  /*
   * Customer registration state
   */
  private readonly customerRegistrationLoadingState =
    signal(false);


  private readonly customerRegistrationSuccessState =
    signal<string | null>(null);


  private readonly customerRegistrationErrorState =
    signal<string | null>(null);


  private readonly customerRegistrationFieldErrorsState =
    signal<ApiValidationErrors>({});


  readonly registrationLoading =
    this.customerRegistrationLoadingState.asReadonly();


  readonly registrationSuccessMessage =
    this.customerRegistrationSuccessState.asReadonly();


  readonly registrationErrorMessage =
    this.customerRegistrationErrorState.asReadonly();


  readonly registrationFieldErrors =
    this.customerRegistrationFieldErrorsState.asReadonly();


  readonly registrationSucceeded =
    computed(
      () =>
        this.customerRegistrationSuccessState() !== null
    );


  /*
   * Claims Manager registration state
   */
  private readonly claimsManagerLoadingState =
    signal(false);


  private readonly claimsManagerSuccessState =
    signal<string | null>(null);


  private readonly claimsManagerStatusState =
    signal<string | null>(null);


  private readonly claimsManagerErrorState =
    signal<string | null>(null);


  private readonly claimsManagerFieldErrorsState =
    signal<ApiValidationErrors>({});


  readonly claimsManagerRegistrationLoading =
    this.claimsManagerLoadingState.asReadonly();


  readonly claimsManagerRegistrationSuccessMessage =
    this.claimsManagerSuccessState.asReadonly();


  readonly claimsManagerRegistrationStatus =
    this.claimsManagerStatusState.asReadonly();


  readonly claimsManagerRegistrationErrorMessage =
    this.claimsManagerErrorState.asReadonly();


  readonly claimsManagerRegistrationFieldErrors =
    this.claimsManagerFieldErrorsState.asReadonly();


  readonly claimsManagerRegistrationSucceeded =
    computed(
      () =>
        this.claimsManagerSuccessState() !== null
    );


  /*
   * Forgot-password state
   */
  private readonly forgotPasswordLoadingState =
    signal(false);


  private readonly forgotPasswordSuccessState =
    signal<string | null>(null);


  private readonly forgotPasswordErrorState =
    signal<string | null>(null);


  private readonly forgotPasswordFieldErrorsState =
    signal<ApiValidationErrors>({});


  readonly forgotPasswordLoading =
    this.forgotPasswordLoadingState.asReadonly();


  readonly forgotPasswordSuccessMessage =
    this.forgotPasswordSuccessState.asReadonly();


  readonly forgotPasswordErrorMessage =
    this.forgotPasswordErrorState.asReadonly();


  readonly forgotPasswordFieldErrors =
    this.forgotPasswordFieldErrorsState.asReadonly();


  readonly forgotPasswordSucceeded =
    computed(
      () =>
        this.forgotPasswordSuccessState() !== null
    );


  /*
   * Reset-password state
   */
  private readonly resetPasswordLoadingState =
    signal(false);


  private readonly resetPasswordSuccessState =
    signal<string | null>(null);


  private readonly resetPasswordErrorState =
    signal<string | null>(null);


  private readonly resetPasswordFieldErrorsState =
    signal<ApiValidationErrors>({});


  readonly resetPasswordLoading =
    this.resetPasswordLoadingState.asReadonly();


  readonly resetPasswordSuccessMessage =
    this.resetPasswordSuccessState.asReadonly();


  readonly resetPasswordErrorMessage =
    this.resetPasswordErrorState.asReadonly();


  readonly resetPasswordFieldErrors =
    this.resetPasswordFieldErrorsState.asReadonly();


  readonly resetPasswordSucceeded =
    computed(
      () =>
        this.resetPasswordSuccessState() !== null
    );


  /*
   * Customer registration
   */
  registerCustomer(
    request: RegisterCustomerRequest
  ): void {
    if (
      this.customerRegistrationLoadingState()
    ) {
      return;
    }


    this.customerRegistrationLoadingState.set(true);


    this.customerRegistrationSuccessState.set(
      null
    );


    this.customerRegistrationErrorState.set(
      null
    );


    this.customerRegistrationFieldErrorsState.set(
      {}
    );


    this.authApi
      .registerCustomer(request)
      .pipe(
        finalize(() => {
          this.customerRegistrationLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: response => {
          this.customerRegistrationSuccessState.set(
            response.message
          );


          this.customerRegistrationErrorState.set(
            null
          );


          this.customerRegistrationFieldErrorsState.set(
            {}
          );
        },


        error: error => {
          const parsedError =
            parseApiError(error);


          this.customerRegistrationErrorState.set(
            parsedError.message
          );


          this.customerRegistrationFieldErrorsState.set(
            parsedError.fieldErrors
          );
        }
      });
  }


  clearCustomerRegistrationState(): void {
    this.customerRegistrationLoadingState.set(
      false
    );


    this.customerRegistrationSuccessState.set(
      null
    );


    this.customerRegistrationErrorState.set(
      null
    );


    this.customerRegistrationFieldErrorsState.set(
      {}
    );
  }


  clearCustomerRegistrationErrors(): void {
    this.customerRegistrationErrorState.set(
      null
    );


    this.customerRegistrationFieldErrorsState.set(
      {}
    );
  }


  /*
   * Claims Manager registration
   */
  registerClaimsManager(
    request: RegisterClaimsManagerRequest,
    verificationDocument: File
  ): void {
    if (
      this.claimsManagerLoadingState()
    ) {
      return;
    }


    this.claimsManagerLoadingState.set(
      true
    );


    this.claimsManagerSuccessState.set(
      null
    );


    this.claimsManagerStatusState.set(
      null
    );


    this.claimsManagerErrorState.set(
      null
    );


    this.claimsManagerFieldErrorsState.set(
      {}
    );


    this.authApi
      .registerClaimsManager(
        request,
        verificationDocument
      )
      .pipe(
        finalize(() => {
          this.claimsManagerLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: response => {
          this.claimsManagerSuccessState.set(
            response.message
          );


          this.claimsManagerStatusState.set(
            response.status
          );


          this.claimsManagerErrorState.set(
            null
          );


          this.claimsManagerFieldErrorsState.set(
            {}
          );
        },


        error: error => {
          const parsedError =
            parseApiError(error);


          this.claimsManagerErrorState.set(
            parsedError.message
          );


          this.claimsManagerFieldErrorsState.set(
            parsedError.fieldErrors
          );
        }
      });
  }


  clearClaimsManagerRegistrationState(): void {
    this.claimsManagerLoadingState.set(
      false
    );


    this.claimsManagerSuccessState.set(
      null
    );


    this.claimsManagerStatusState.set(
      null
    );


    this.claimsManagerErrorState.set(
      null
    );


    this.claimsManagerFieldErrorsState.set(
      {}
    );
  }


  clearClaimsManagerRegistrationErrors(): void {
    this.claimsManagerErrorState.set(
      null
    );


    this.claimsManagerFieldErrorsState.set(
      {}
    );
  }


  /*
   * Forgot password
   */
  forgotPassword(
    email: string
  ): void {
    if (
      this.forgotPasswordLoadingState()
    ) {
      return;
    }


    this.forgotPasswordLoadingState.set(
      true
    );


    this.forgotPasswordSuccessState.set(
      null
    );


    this.forgotPasswordErrorState.set(
      null
    );


    this.forgotPasswordFieldErrorsState.set(
      {}
    );


    this.authApi
      .forgotPassword({
        email
      })
      .pipe(
        finalize(() => {
          this.forgotPasswordLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: response => {
          /*
           * The backend always returns the same response,
           * regardless of whether the account exists.
           */
          this.forgotPasswordSuccessState.set(
            response.message
          );


          this.forgotPasswordErrorState.set(
            null
          );


          this.forgotPasswordFieldErrorsState.set(
            {}
          );
        },


        error: error => {
          const parsedError =
            parseApiError(error);


          this.forgotPasswordErrorState.set(
            parsedError.message
          );


          this.forgotPasswordFieldErrorsState.set(
            parsedError.fieldErrors
          );
        }
      });
  }


  clearForgotPasswordState(): void {
    this.forgotPasswordLoadingState.set(
      false
    );


    this.forgotPasswordSuccessState.set(
      null
    );


    this.forgotPasswordErrorState.set(
      null
    );


    this.forgotPasswordFieldErrorsState.set(
      {}
    );
  }


  clearForgotPasswordErrors(): void {
    this.forgotPasswordErrorState.set(
      null
    );


    this.forgotPasswordFieldErrorsState.set(
      {}
    );
  }


  /*
   * Reset password
   */
  resetPassword(
    request: ResetPasswordRequest
  ): void {
    if (
      this.resetPasswordLoadingState()
    ) {
      return;
    }


    this.resetPasswordLoadingState.set(
      true
    );


    this.resetPasswordSuccessState.set(
      null
    );


    this.resetPasswordErrorState.set(
      null
    );


    this.resetPasswordFieldErrorsState.set(
      {}
    );


    this.authApi
      .resetPassword(request)
      .pipe(
        finalize(() => {
          this.resetPasswordLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: response => {
          this.resetPasswordSuccessState.set(
            response.message
          );


          this.resetPasswordErrorState.set(
            null
          );


          this.resetPasswordFieldErrorsState.set(
            {}
          );
        },


        error: error => {
          const parsedError =
            parseApiError(error);


          this.resetPasswordErrorState.set(
            parsedError.message
          );


          this.resetPasswordFieldErrorsState.set(
            parsedError.fieldErrors
          );
        }
      });
  }


  clearResetPasswordState(): void {
    this.resetPasswordLoadingState.set(
      false
    );


    this.resetPasswordSuccessState.set(
      null
    );


    this.resetPasswordErrorState.set(
      null
    );


    this.resetPasswordFieldErrorsState.set(
      {}
    );
  }


  clearResetPasswordErrors(): void {
    this.resetPasswordErrorState.set(
      null
    );


    this.resetPasswordFieldErrorsState.set(
      {}
    );
  }
}
