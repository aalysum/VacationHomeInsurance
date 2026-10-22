import {
  Component,
  DestroyRef,
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
  takeUntilDestroyed
} from '@angular/core/rxjs-interop';


import {
  RouterLink
} from '@angular/router';


import {
  AuthStoreService
} from '../../../core/services/auth-store.service';


import {
  passwordMatchValidator
} from '../../../shared/validators/password-match.validator';


@Component({
  selector: 'app-customer-registration',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl:
    './customer-registration.component.html',
  styleUrl:
    './customer-registration.component.css'
})
export class CustomerRegistrationComponent {
  private readonly formBuilder =
    inject(FormBuilder);


  private readonly destroyRef =
    inject(DestroyRef);


  readonly authStore =
    inject(AuthStoreService);


  readonly passwordVisible =
    signal(false);


  readonly confirmPasswordVisible =
    signal(false);


  readonly registrationLoading =
    this.authStore.registrationLoading;


  readonly successMessage =
    this.authStore.registrationSuccessMessage;


  readonly errorMessage =
    this.authStore.registrationErrorMessage;


  readonly backendFieldErrors =
    this.authStore.registrationFieldErrors;


  readonly registrationForm =
    this.formBuilder.nonNullable.group(
      {
        firstName: [
          '',
          [
            Validators.required,
            Validators.maxLength(100),
            this.noControlCharactersValidator
          ]
        ],


        lastName: [
          '',
          [
            Validators.required,
            Validators.maxLength(100),
            this.noControlCharactersValidator
          ]
        ],


        email: [
          '',
          [
            Validators.required,
            Validators.email
          ]
        ],


        phoneNumber: [
          '',
          [
            Validators.required
          ]
        ],


        password: [
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
            'password',
            'confirmPassword'
          )
      }
    );


  constructor() {
    this.authStore
      .clearCustomerRegistrationState();


    this.registrationForm.valueChanges
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
            .clearCustomerRegistrationErrors();
        }
      });
  }


  get firstNameControl() {
    return this.registrationForm
      .controls.firstName;
  }


  get lastNameControl() {
    return this.registrationForm
      .controls.lastName;
  }


  get emailControl() {
    return this.registrationForm
      .controls.email;
  }


  get phoneNumberControl() {
    return this.registrationForm
      .controls.phoneNumber;
  }


  get passwordControl() {
    return this.registrationForm
      .controls.password;
  }


  get confirmPasswordControl() {
    return this.registrationForm
      .controls.confirmPassword;
  }


  togglePasswordVisibility(): void {
    this.passwordVisible.update(
      visible => !visible
    );
  }


  toggleConfirmPasswordVisibility(): void {
    this.confirmPasswordVisible.update(
      visible => !visible
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


  hasMinimumLength(): boolean {
    return (
      this.passwordControl.value.length >= 8
    );
  }


  hasUppercase(): boolean {
    return /[A-Z]/.test(
      this.passwordControl.value
    );
  }


  hasLowercase(): boolean {
    return /[a-z]/.test(
      this.passwordControl.value
    );
  }


  hasDigit(): boolean {
    return /\d/.test(
      this.passwordControl.value
    );
  }


  hasSpecialCharacter(): boolean {
    return /[^A-Za-z0-9]/.test(
      this.passwordControl.value
    );
  }


  submit(): void {
    if (this.registrationForm.invalid) {
      this.registrationForm
        .markAllAsTouched();


      return;
    }


    if (this.registrationLoading()) {
      return;
    }


    const value =
      this.registrationForm.getRawValue();


    this.authStore.registerCustomer({
      firstName:
        value.firstName.trim(),


      lastName:
        value.lastName.trim(),


      email:
        value.email.trim(),


      phoneNumber:
        value.phoneNumber.trim(),


      password:
        value.password,


      confirmPassword:
        value.confirmPassword
    });
  }


  private noControlCharactersValidator(
    control: AbstractControl
  ): ValidationErrors | null {
    const value =
      typeof control.value === 'string'
        ? control.value
        : '';


    return /[\u0000-\u001F\u007F]/.test(value)
      ? {
          controlCharacters: true
        }
      : null;
  }
}
