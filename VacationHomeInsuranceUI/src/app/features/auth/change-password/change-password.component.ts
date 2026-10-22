import {
  Component,
  inject,
  signal
} from '@angular/core';


import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';


import {
  HttpErrorResponse
} from '@angular/common/http';


import {
  Router,
  RouterLink
} from '@angular/router';


import {
  finalize
} from 'rxjs';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  ChangePasswordRequest
} from '../../../core/models/auth/change-password-request.model';


const PASSWORD_PATTERN =
  /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/;


@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl:
    './change-password.component.html',
  styleUrl:
    './change-password.component.css'
})
export class ChangePasswordComponent {
  private readonly formBuilder =
    inject(FormBuilder);


  private readonly authService =
    inject(AuthService);


  private readonly router =
    inject(Router);


  readonly isSubmitting =
    signal(false);


  readonly errorMessage =
    signal<string | null>(null);


  readonly showCurrentPassword =
    signal(false);


  readonly showNewPassword =
    signal(false);


  readonly showConfirmPassword =
    signal(false);


  readonly form =
    this.formBuilder.nonNullable.group(
      {
        currentPassword: [
          '',
          [
            Validators.required
          ]
        ],
        newPassword: [
          '',
          [
            Validators.required,
            Validators.pattern(
              PASSWORD_PATTERN
            )
          ]
        ],
        confirmPassword: [
          '',
          [
            Validators.required
          ]
        ]
      },
      {
        validators:
          ChangePasswordComponent
            .matchingPasswordsValidator
      }
    );


  get currentPassword():
    AbstractControl<string> {
    return this.form.controls
      .currentPassword;
  }


  get newPassword():
    AbstractControl<string> {
    return this.form.controls
      .newPassword;
  }


  get confirmPassword():
    AbstractControl<string> {
    return this.form.controls
      .confirmPassword;
  }


  toggleCurrentPassword(): void {
    this.showCurrentPassword.update(
      value => !value
    );
  }


  toggleNewPassword(): void {
    this.showNewPassword.update(
      value => !value
    );
  }


  toggleConfirmPassword(): void {
    this.showConfirmPassword.update(
      value => !value
    );
  }


  submit(): void {
    this.errorMessage.set(null);


    if (
      this.form.invalid ||
      this.isSubmitting()
    ) {
      this.form.markAllAsTouched();
      return;
    }


    const formValue =
      this.form.getRawValue();


    const request:
      ChangePasswordRequest = {
      currentPassword:
        formValue.currentPassword,
      newPassword:
        formValue.newPassword,
      confirmPassword:
        formValue.confirmPassword
    };


    this.isSubmitting.set(true);


    this.authService
      .changePassword(request)
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
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
                passwordChanged:
                  'true'
              }
            }
          );
        },
        error: error => {
          this.errorMessage.set(
            this.getErrorMessage(error)
          );
        }
      });
  }


  private getErrorMessage(
    error: unknown
  ): string {
    if (
      error instanceof
        HttpErrorResponse
    ) {
      const backendMessage =
        this.readBackendMessage(
          error.error
        );


      if (backendMessage) {
        return backendMessage;
      }


      if (error.status === 401) {
        return 'Your session has expired. Please sign in again.';
      }


      if (error.status === 403) {
        return 'You are not authorized to change this password.';
      }


      if (error.status === 400) {
        return 'The current password is incorrect or the new password is invalid.';
      }


      if (error.status === 0) {
        return 'The authentication service is unavailable. Please try again.';
      }
    }


    return 'We could not change your password. Please try again.';
  }


  private readBackendMessage(
    response: unknown
  ): string | null {
    if (
      typeof response !== 'object' ||
      response === null
    ) {
      return null;
    }


    const body =
      response as Record<
        string,
        unknown
      >;


    if (
      typeof body['message'] ===
        'string' &&
      body['message'].trim()
    ) {
      return body['message'];
    }


    if (
      typeof body['detail'] ===
        'string' &&
      body['detail'].trim()
    ) {
      return body['detail'];
    }


    const errors =
      body['errors'];


    if (
      typeof errors !== 'object' ||
      errors === null
    ) {
      return null;
    }


    const validationErrors =
      Object.values(
        errors as Record<
          string,
          unknown
        >
      )
        .flatMap(value => {
          if (Array.isArray(value)) {
            return value.filter(
              item =>
                typeof item ===
                'string'
            ) as string[];
          }


          if (
            typeof value === 'string'
          ) {
            return [
              value
            ];
          }


          return [];
        });


    return (
      validationErrors[0] ??
      null
    );
  }


  private static
    matchingPasswordsValidator(
      control: AbstractControl
    ): ValidationErrors | null {
    const newPassword =
      control.get(
        'newPassword'
      )?.value;


    const confirmPassword =
      control.get(
        'confirmPassword'
      )?.value;


    if (
      !newPassword ||
      !confirmPassword
    ) {
      return null;
    }


    return newPassword ===
      confirmPassword
      ? null
      : {
          passwordMismatch: true
        };
  }
}

