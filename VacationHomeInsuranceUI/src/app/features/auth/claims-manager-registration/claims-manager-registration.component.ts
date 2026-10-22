import {
  Component,
  DestroyRef,
  ElementRef,
  inject,
  signal,
  viewChild
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
  CLAIMS_MANAGER_DOCUMENT_CONFIG
} from '../../../core/constants/document-validation.constants';


import {
  passwordMatchValidator
} from '../../../shared/validators/password-match.validator';


@Component({
  selector:
    'app-claims-manager-registration',


  standalone: true,


  imports: [
    ReactiveFormsModule,
    RouterLink
  ],


  templateUrl:
    './claims-manager-registration.component.html',


  styleUrl:
    './claims-manager-registration.component.css'
})
export class ClaimsManagerRegistrationComponent {
  private readonly formBuilder =
    inject(FormBuilder);


  private readonly destroyRef =
    inject(DestroyRef);


  private readonly authStore =
    inject(AuthStoreService);


  readonly fileInput =
    viewChild<ElementRef<HTMLInputElement>>(
      'fileInput'
    );


  readonly passwordVisible =
    signal(false);


  readonly confirmPasswordVisible =
    signal(false);


  readonly selectedFile =
    signal<File | null>(null);


  readonly fileError =
    signal<string | null>(null);


  readonly isDragActive =
    signal(false);


  readonly registrationLoading =
    this.authStore
      .claimsManagerRegistrationLoading;


  readonly successMessage =
    this.authStore
      .claimsManagerRegistrationSuccessMessage;


  readonly applicationStatus =
    this.authStore
      .claimsManagerRegistrationStatus;


  readonly errorMessage =
    this.authStore
      .claimsManagerRegistrationErrorMessage;


  readonly backendFieldErrors =
    this.authStore
      .claimsManagerRegistrationFieldErrors;


  readonly acceptedFileTypes =
    CLAIMS_MANAGER_DOCUMENT_CONFIG
      .acceptAttribute;


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
            Validators.required,
            Validators.maxLength(10)
          ]
        ],


        yearsOfExperience: [
          0,
          [
            Validators.required,
            Validators.min(0)
          ]
        ],


        professionalSummary: [
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
      .clearClaimsManagerRegistrationState();


    this.registrationForm
      .valueChanges
      .pipe(
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe(() => {
        this.authStore
          .clearClaimsManagerRegistrationErrors();
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


  get yearsOfExperienceControl() {
    return this.registrationForm
      .controls.yearsOfExperience;
  }


  get professionalSummaryControl() {
    return this.registrationForm
      .controls.professionalSummary;
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


  openFilePicker(): void {
    this.fileInput()?.nativeElement.click();
  }


  onFileSelected(
    event: Event
  ): void {
    const input =
      event.target as HTMLInputElement;


    const file =
      input.files?.[0] ?? null;


    this.processFile(file);
  }


  onDragOver(
    event: DragEvent
  ): void {
    event.preventDefault();


    if (!this.registrationLoading()) {
      this.isDragActive.set(true);
    }
  }


  onDragLeave(
    event: DragEvent
  ): void {
    event.preventDefault();
    this.isDragActive.set(false);
  }


  onFileDropped(
    event: DragEvent
  ): void {
    event.preventDefault();
    this.isDragActive.set(false);


    if (this.registrationLoading()) {
      return;
    }


    const file =
      event.dataTransfer?.files?.[0] ??
      null;


    this.processFile(file);
  }


  removeSelectedFile(): void {
    this.selectedFile.set(null);
    this.fileError.set(null);


    const input =
      this.fileInput()?.nativeElement;


    if (input) {
      input.value = '';
    }
  }


  getSelectedFileSize(): string {
    const file =
      this.selectedFile();


    if (!file) {
      return '';
    }


    if (file.size < 1024 * 1024) {
      return `${Math.ceil(file.size / 1024)} KB`;
    }


    return `${
      (file.size / (1024 * 1024))
        .toFixed(2)
    } MB`;
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
    this.fileError.set(null);


    if (this.registrationForm.invalid) {
      this.registrationForm
        .markAllAsTouched();
    }


    const file =
      this.selectedFile();


    if (!file) {
      this.fileError.set(
        'Verification document is required.'
      );
    }


    if (
      this.registrationForm.invalid ||
      !file
    ) {
      return;
    }


    const fileValidationError =
      this.validateFile(file);


    if (fileValidationError) {
      this.fileError.set(
        fileValidationError
      );


      return;
    }


    if (this.registrationLoading()) {
      return;
    }


    const value =
      this.registrationForm
        .getRawValue();


    this.authStore.registerClaimsManager(
      {
        firstName:
          value.firstName.trim(),


        lastName:
          value.lastName.trim(),


        email:
          value.email.trim(),


        phoneNumber:
          value.phoneNumber.trim(),


        yearsOfExperience:
          Number(
            value.yearsOfExperience
          ),


        professionalSummary:
          value.professionalSummary.trim(),


        password:
          value.password,


        confirmPassword:
          value.confirmPassword
      },


      file
    );
  }


  private processFile(
    file: File | null
  ): void {
    this.fileError.set(null);


    if (!file) {
      this.removeSelectedFile();
      return;
    }


    const error =
      this.validateFile(file);


    if (error) {
      this.selectedFile.set(null);
      this.fileError.set(error);


      const input =
        this.fileInput()?.nativeElement;


      if (input) {
        input.value = '';
      }


      return;
    }


    this.selectedFile.set(file);


    this.authStore
      .clearClaimsManagerRegistrationErrors();
  }


  private validateFile(
    file: File
  ): string | null {
    if (file.size === 0) {
      return 'Verification document cannot be empty.';
    }


    if (
      file.size >
      CLAIMS_MANAGER_DOCUMENT_CONFIG
        .maxFileSizeBytes
    ) {
      return 'Verification document cannot exceed 10 MB.';
    }


    const extension =
      this.getFileExtension(
        file.name
      );


    const extensionAllowed =
      CLAIMS_MANAGER_DOCUMENT_CONFIG
        .acceptedExtensions
        .some(
          allowedExtension =>
            allowedExtension === extension
        );


    if (!extensionAllowed) {
      return 'Only PDF, JPG, JPEG, and PNG files are allowed.';
    }


    const mimeAllowed =
      CLAIMS_MANAGER_DOCUMENT_CONFIG
        .acceptedMimeTypes
        .some(
          allowedMimeType =>
            allowedMimeType ===
            file.type.toLowerCase()
        );


    if (!mimeAllowed) {
      return 'The selected file has an unsupported MIME type.';
    }


    const expectedMimeType =
      extension === '.pdf'
        ? 'application/pdf'
        : extension === '.png'
          ? 'image/png'
          : 'image/jpeg';


    if (
      file.type.toLowerCase() !==
      expectedMimeType
    ) {
      return 'The document MIME type does not match its file extension.';
    }


    return null;
  }


  private getFileExtension(
    fileName: string
  ): string {
    const finalDotIndex =
      fileName.lastIndexOf('.');


    if (finalDotIndex < 0) {
      return '';
    }


    return fileName
      .slice(finalDotIndex)
      .toLowerCase();
  }


  private noControlCharactersValidator(
    control: AbstractControl
  ): ValidationErrors | null {
    const value =
      typeof control.value === 'string'
        ? control.value
        : '';


    return /[\u0000-\u001F\u007F]/.test(
      value
    )
      ? {
          controlCharacters: true
        }
      : null;
  }
}

