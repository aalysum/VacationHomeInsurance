import {
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';


import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';


import {
  takeUntilDestroyed
} from '@angular/core/rxjs-interop';


import {
  ActivatedRoute,
  RouterLink
} from '@angular/router';


import {
  AuthStoreService
} from '../../../core/services/auth-store.service';


import {
  passwordMatchValidator
} from '../../../shared/validators/password-match.validator';


@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl:
    './reset-password.component.html',
  styleUrl:
    './reset-password.component.css'
})
export class ResetPasswordComponent {
  private readonly formBuilder =
    inject(FormBuilder);


  private readonly route =
    inject(ActivatedRoute);


  private readonly destroyRef =
    inject(DestroyRef);


  private readonly authStore =
    inject(AuthStoreService);


  readonly newPasswordVisible =
    signal(false);


  readonly confirmPasswordVisible =
    signal(false);


  readonly resetLinkValid =
    signal(false);


  readonly linkErrorMessage =
    signal<string | null>(null);


  readonly loading =
    this.authStore.resetPasswordLoading;


  readonly successMessage =
    this.authStore.resetPasswordSuccessMessage;


  readonly errorMessage =
    this.authStore.resetPasswordErrorMessage;


  readonly backendFieldErrors =
    this.authStore.resetPasswordFieldErrors;


  private email = '';
  private token = '';


  readonly resetPasswordForm =
    this.formBuilder.nonNullable.group(
      {
        newPassword: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.pattern(
              /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$/
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
          passwordMatchValidator(
            'newPassword',
            'confirmPassword'
          )
      }
    );


  constructor() {
    this.authStore.clearResetPasswordState();


    this.readResetParameters();


    this.resetPasswordForm.valueChanges
      .pipe(
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe(() => {
        if (
          this.errorMessage() ||
          Object.keys(
            this.backendFieldErrors()
          ).length > 0
        ) {
          this.authStore
            .clearResetPasswordErrors();
        }
      });
  }


  get newPasswordControl() {
    return this.resetPasswordForm
      .controls.newPassword;
  }


  get confirmPasswordControl() {
    return this.resetPasswordForm
      .controls.confirmPassword;
  }


  toggleNewPasswordVisibility(): void {
    this.newPasswordVisible.update(
      visible => !visible
    );
  }


  toggleConfirmPasswordVisibility(): void {
    this.confirmPasswordVisible.update(
      visible => !visible
    );
  }


  hasMinimumLength(): boolean {
    return (
      this.newPasswordControl.value.length >= 8
    );
  }


  hasUppercase(): boolean {
    return /[A-Z]/.test(
      this.newPasswordControl.value
    );
  }


  hasLowercase(): boolean {
    return /[a-z]/.test(
      this.newPasswordControl.value
    );
  }


  hasDigit(): boolean {
    return /\d/.test(
      this.newPasswordControl.value
    );
  }


  hasSpecialCharacter(): boolean {
    return /[^A-Za-z0-9]/.test(
      this.newPasswordControl.value
    );
  }


  getBackendError(
    fieldName: string
  ): string | null {
    return (
      this.backendFieldErrors()[
        fieldName
      ]?.[0] ?? null
    );
  }


  submit(): void {
    if (!this.resetLinkValid()) {
      return;
    }


    if (this.resetPasswordForm.invalid) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }


    if (this.loading()) {
      return;
    }


    const value =
      this.resetPasswordForm.getRawValue();


    this.authStore.resetPassword({
      email: this.email,
      token: this.token,
      newPassword: value.newPassword,
      confirmPassword:
        value.confirmPassword
    });
  }


  private readResetParameters(): void {
    /*
     * The token is retained only in component memory.
     * It is not copied to localStorage or sessionStorage.
     */
    const email =
      this.route.snapshot.queryParamMap
        .get('email')
        ?.trim() ?? '';


    const token =
      this.route.snapshot.queryParamMap
        .get('token')
        ?.trim() ?? '';


    if (!email || !token) {
      this.email = '';
      this.token = '';


      this.resetLinkValid.set(false);


      this.linkErrorMessage.set(
        'This password-reset link is incomplete or invalid. Request a new password-reset link.'
      );


      return;
    }


    this.email = email;
    this.token = token;


    this.resetLinkValid.set(true);
    this.linkErrorMessage.set(null);
  }
}