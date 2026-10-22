import {
  HttpErrorResponse
} from '@angular/common/http';


import {
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';


import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';


import {
  Router,
  RouterLink
} from '@angular/router';


import {
  finalize
} from 'rxjs';




import {
  ConfirmMfaSetupRequest
} from '../../../core/models/auth/confirm-mfa-setup-request.model';


import {
  MfaSetupResponse
} from '../../../core/models/auth/mfa-setup-response.model';


import {
  AuthService
} from '../../../core/services/auth.service';


type MfaView =
  | 'loading'
  | 'setup'
  | 'enabled'
  | 'setup-error';


interface ApiError {
  code: string | null;
  message: string | null;
}


@Component({
  selector: 'app-mfa-management',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl:
    './mfa-management.component.html',
  styleUrl:
    './mfa-management.component.css'
})
export class MfaManagementComponent
  implements OnInit {
  private readonly authService =
    inject(AuthService);


  private readonly formBuilder =
    inject(FormBuilder);


  private readonly router =
    inject(Router);


  readonly view =
    signal<MfaView>('loading');


  readonly setup =
    signal<MfaSetupResponse | null>(
      null
    );


  readonly isSubmitting =
    signal(false);


  readonly isCopying =
    signal(false);


  readonly copied =
    signal(false);


  readonly errorMessage =
    signal<string | null>(null);


    readonly uriCopied =
  signal(false);


readonly isCopyingUri =
  signal(false);


  readonly confirmationForm =
    this.formBuilder.nonNullable.group({
      code: [
        '',
        [
          Validators.required,
          Validators.pattern(
            /^[0-9]{6}$/
          )
        ]
      ]
    });


  readonly disableForm =
    this.formBuilder.nonNullable.group({
      code: [
        '',
        [
          Validators.required,
          Validators.pattern(
            /^[0-9]{6}$/
          )
        ]
      ]
    });


  ngOnInit(): void {
    this.loadMfaState();
  }


async copyAuthenticatorUri(): Promise<void> {
  const authenticatorUri =
    this.setup()?.authenticatorUri;


  if (
    !authenticatorUri ||
    this.isCopyingUri()
  ) {
    return;
  }


  this.isCopyingUri.set(true);
  this.uriCopied.set(false);


  try {
    await navigator.clipboard
      .writeText(
        authenticatorUri
      );


    this.uriCopied.set(true);


    window.setTimeout(
      () => {
        this.uriCopied.set(false);
      },
      2500
    );
  } catch {
    this.errorMessage.set(
      'The authenticator URI could not be copied. Use the manual shared key instead.'
    );
  } finally {
    this.isCopyingUri.set(false);
  }
}


loadMfaState(): void {
  this.view.set('loading');
  this.errorMessage.set(null);
  this.setup.set(null);


  this.authService
    .beginMfaSetup()
    .subscribe({
      next: response => {
        this.setup.set(response);
        this.view.set('setup');
      },
      error: error => {
        const apiError =
          this.readApiError(error);


        if (
          apiError.code ===
          'MFA_ALREADY_ENABLED'
        ) {
          this.view.set('enabled');
          return;
        }


        this.errorMessage.set(
          this.resolveSetupError(
            apiError,
            error
          )
        );


        this.view.set(
          'setup-error'
        );
      }
    });
}
  submitConfirmation(): void {
    this.errorMessage.set(null);


    if (
      this.confirmationForm.invalid ||
      this.isSubmitting()
    ) {
      this.confirmationForm
        .markAllAsTouched();


      return;
    }


    const request:
      ConfirmMfaSetupRequest = {
      code:
        this.confirmationForm
          .getRawValue()
          .code
    };


    this.isSubmitting.set(true);


    this.authService
      .confirmMfaSetup(request)
      .pipe(
        finalize(() => {
          this.isSubmitting.set(
            false
          );
        })
      )
      .subscribe({
        next: () => {
          void this.router.navigate(
            [
              '/auth/login'
            ],
            {
              replaceUrl: true,
              queryParams: {
                mfaEnabled: 'true'
              }
            }
          );
        },
        error: error => {
          const apiError =
            this.readApiError(error);


          this.errorMessage.set(
            this.resolveConfirmationError(
              apiError,
              error
            )
          );
        }
      });
  }


  submitDisable(): void {
    this.errorMessage.set(null);


    if (
      this.disableForm.invalid ||
      this.isSubmitting()
    ) {
      this.disableForm
        .markAllAsTouched();


      return;
    }


    const request:
      ConfirmMfaSetupRequest = {
      code:
        this.disableForm
          .getRawValue()
          .code
    };


    this.isSubmitting.set(true);


    this.authService
      .disableMfa(request)
      .pipe(
        finalize(() => {
          this.isSubmitting.set(
            false
          );
        })
      )
      .subscribe({
        next: () => {
          void this.router.navigate(
            [
              '/auth/login'
            ],
            {
              replaceUrl: true,
              queryParams: {
                mfaDisabled: 'true'
              }
            }
          );
        },
        error: error => {
          const apiError =
            this.readApiError(error);


          if (
            apiError.code ===
            'MFA_NOT_ENABLED'
          ) {
            this.loadMfaState();
            return;
          }


          this.errorMessage.set(
            this.resolveDisableError(
              apiError,
              error
            )
          );
        }
      });
  }


  async copySharedKey(): Promise<void> {
    const sharedKey =
      this.setup()?.sharedKey;


    if (
      !sharedKey ||
      this.isCopying()
    ) {
      return;
    }


    this.isCopying.set(true);
    this.copied.set(false);


    try {
      await navigator.clipboard
        .writeText(
          sharedKey.replace(
            /\s/g,
            ''
          )
        );


      this.copied.set(true);


      window.setTimeout(
        () => {
          this.copied.set(false);
        },
        2500
      );
    } catch {
      this.errorMessage.set(
        'The shared key could not be copied. Select and copy it manually.'
      );
    } finally {
      this.isCopying.set(false);
    }
  }


  sanitizeCode(
    event: Event,
    type: 'confirm' | 'disable'
  ): void {
    const input =
      event.target as HTMLInputElement;


    const code =
      input.value
        .replace(/\D/g, '')
        .slice(0, 6);


    input.value = code;


    const control =
      type === 'confirm'
        ? this.confirmationForm
            .controls.code
        : this.disableForm
            .controls.code;


    control.setValue(code);
  }




  private resolveSetupError(
    apiError: ApiError,
    error: unknown
  ): string {
    if (
      apiError.code ===
      'MFA_KEY_GENERATION_FAILED'
    ) {
      return 'SecureSphere could not generate an authenticator key. Please try again.';
    }


    return this.resolveGeneralError(
      apiError,
      error,
      'SecureSphere could not start MFA setup.'
    );
  }


  private resolveConfirmationError(
    apiError: ApiError,
    error: unknown
  ): string {
    if (
      apiError.code ===
      'INVALID_MFA_CODE'
    ) {
      return 'The authenticator code is invalid. Enter the current six-digit code and try again.';
    }


    if (
      apiError.code ===
      'MFA_ALREADY_ENABLED'
    ) {
      this.view.set('enabled');


      return 'MFA is already enabled for your account.';
    }


    if (
      apiError.code ===
      'MFA_ENABLE_FAILED'
    ) {
      return 'MFA could not be enabled. Please try again.';
    }


    return this.resolveGeneralError(
      apiError,
      error,
      'SecureSphere could not confirm MFA setup.'
    );
  }


  private resolveDisableError(
    apiError: ApiError,
    error: unknown
  ): string {
    if (
      apiError.code ===
      'INVALID_MFA_CODE'
    ) {
      return 'The authenticator code is invalid. Enter the current six-digit code and try again.';
    }


    if (
      apiError.code ===
      'MFA_DISABLE_FAILED'
    ) {
      return 'MFA could not be disabled. Please try again.';
    }


    if (
      apiError.code ===
      'MFA_KEY_RESET_FAILED'
    ) {
      return 'MFA was changed, but the authenticator key could not be reset. Please contact support.';
    }


    return this.resolveGeneralError(
      apiError,
      error,
      'SecureSphere could not disable MFA.'
    );
  }


  private resolveGeneralError(
    apiError: ApiError,
    error: unknown,
    fallback: string
  ): string {
    if (
      error instanceof
        HttpErrorResponse
    ) {
      if (error.status === 0) {
        return 'The authentication service is unavailable. Please try again.';
      }


      if (error.status === 401) {
        return 'Your session has expired. Please sign in again.';
      }


      if (error.status === 403) {
        return 'You are not authorized to manage MFA for this account.';
      }
    }


    return (
      apiError.message ??
      fallback
    );
  }


  private readApiError(
    error: unknown
  ): ApiError {
    if (
      !(error instanceof
        HttpErrorResponse)
    ) {
      return {
        code: null,
        message: null
      };
    }


    if (
      typeof error.error !==
        'object' ||
      error.error === null
    ) {
      return {
        code: null,
        message: null
      };
    }


    const response =
      error.error as Record<
        string,
        unknown
      >;


    return {
      code:
        typeof response['code'] ===
        'string'
          ? response['code']
          : typeof response[
                'errorCode'
              ] === 'string'
            ? response['errorCode']
            : null,


      message:
        typeof response[
          'message'
        ] === 'string'
          ? response['message']
          : typeof response[
                'detail'
              ] === 'string'
            ? response['detail']
            : null
    };
  }
}