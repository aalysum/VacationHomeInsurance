import {
  Component,
  DestroyRef,
  inject
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
  RouterLink
} from '@angular/router';


import {
  AuthStoreService
} from '../../../core/services/auth-store.service';


@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl:
    './forgot-password.component.html',
  styleUrl:
    './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  private readonly formBuilder =
    inject(FormBuilder);


  private readonly destroyRef =
    inject(DestroyRef);


  private readonly authStore =
    inject(AuthStoreService);


  readonly loading =
    this.authStore.forgotPasswordLoading;


  readonly successMessage =
    this.authStore.forgotPasswordSuccessMessage;


  readonly errorMessage =
    this.authStore.forgotPasswordErrorMessage;


  readonly backendFieldErrors =
    this.authStore.forgotPasswordFieldErrors;


  readonly forgotPasswordForm =
    this.formBuilder.nonNullable.group({
      email: [
        '',
        [
          Validators.required,
          Validators.email
        ]
      ]
    });


  constructor() {
    this.authStore
      .clearForgotPasswordState();


    this.forgotPasswordForm
      .valueChanges
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
            .clearForgotPasswordErrors();
        }
      });
  }


  get emailControl() {
    return this.forgotPasswordForm
      .controls.email;
  }


  getEmailBackendError():
    string | null {
    return (
      this.backendFieldErrors()[
        'email'
      ]?.[0] ?? null
    );
  }


  submit(): void {
    if (this.forgotPasswordForm.invalid) {
      this.forgotPasswordForm
        .markAllAsTouched();


      return;
    }


    if (this.loading()) {
      return;
    }


    const value =
      this.forgotPasswordForm
        .getRawValue();


    this.authStore.forgotPassword(
      value.email.trim()
    );
  }


  tryAnotherEmail(): void {
    this.authStore
      .clearForgotPasswordState();


    this.forgotPasswordForm.reset({
      email: ''
    });
  }
}