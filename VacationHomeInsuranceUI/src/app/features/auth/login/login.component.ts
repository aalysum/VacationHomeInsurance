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
  AuthService
} from '../../../core/services/auth.service';


@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl:
    './login.component.html',
  styleUrl:
    './login.component.css'
})
export class LoginComponent {
  private readonly formBuilder =
    inject(FormBuilder);


  private readonly destroyRef =
    inject(DestroyRef);


  private readonly authService =
    inject(AuthService);


  private readonly route =
    inject(ActivatedRoute);


  readonly passwordVisible =
    signal(false);


  readonly submitting =
    this.authService.loginLoading;


  readonly errorMessage =
    this.authService.loginErrorMessage;


  readonly passwordChanged =
    signal(
      this.route.snapshot
        .queryParamMap
        .get('passwordChanged') ===
        'true'
    );


  readonly mfaEnabled =
    signal(
      this.route.snapshot
        .queryParamMap
        .get('mfaEnabled') ===
        'true'
    );


  readonly mfaDisabled =
    signal(
      this.route.snapshot
        .queryParamMap
        .get('mfaDisabled') ===
        'true'
    );


  readonly loginForm =
    this.formBuilder.nonNullable.group({
      email: [
        '',
        [
          Validators.required,
          Validators.email
        ]
      ],


      password: [
        '',
        [
          Validators.required
        ]
      ]
    });


  constructor() {
    this.loginForm.valueChanges
      .pipe(
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe(() => {
        if (this.errorMessage()) {
          this.authService
            .clearLoginError();
        }
      });
  }


  get emailControl() {
    return this.loginForm
      .controls.email;
  }


  get passwordControl() {
    return this.loginForm
      .controls.password;
  }


  togglePasswordVisibility(): void {
    this.passwordVisible.update(
      visible => !visible
    );
  }


  submit(): void {
    if (this.loginForm.invalid) {
      this.loginForm
        .markAllAsTouched();


      return;
    }


    if (this.submitting()) {
      return;
    }


    const value =
      this.loginForm.getRawValue();


    this.authService.login({
      email:
        value.email.trim(),


      password:
        value.password
    });
  }
}