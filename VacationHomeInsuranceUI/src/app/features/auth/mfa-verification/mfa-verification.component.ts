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
  AuthService
} from '../../../core/services/auth.service';


@Component({
  selector: 'app-mfa-verification',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl:
    './mfa-verification.component.html',
  styleUrl:
    './mfa-verification.component.css'
})
export class MfaVerificationComponent {
  private readonly formBuilder =
    inject(FormBuilder);


  private readonly destroyRef =
    inject(DestroyRef);


  private readonly authService =
    inject(AuthService);


  readonly loading =
    this.authService
      .mfaVerificationLoading;


  readonly errorMessage =
    this.authService
      .mfaVerificationErrorMessage;


  readonly challengeExpired =
    this.authService
      .mfaChallengeExpired;


  readonly mfaForm =
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


  constructor() {
    this.authService
      .clearMfaVerificationState();


    this.codeControl.valueChanges
      .pipe(
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe(value => {
        /*
         * Keep only six numeric characters.
         */
        const normalizedCode =
          value
            .replace(/\D/g, '')
            .slice(0, 6);


        if (normalizedCode !== value) {
          this.codeControl.setValue(
            normalizedCode,
            {
              emitEvent: false
            }
          );
        }


        if (this.errorMessage()) {
          this.authService
            .clearMfaVerificationError();
        }
      });
  }


  get codeControl() {
    return this.mfaForm.controls.code;
  }


  submit(): void {
    if (this.challengeExpired()) {
      return;
    }


    if (this.mfaForm.invalid) {
      this.mfaForm.markAllAsTouched();
      return;
    }


    if (this.loading()) {
      return;
    }


    this.authService.verifyMfa(
      this.codeControl.value
    );
  }


  cancel(): void {
    this.authService
      .cancelMfaVerification();
  }


  returnToLogin(): void {
    this.authService
      .cancelMfaVerification();
  }
}