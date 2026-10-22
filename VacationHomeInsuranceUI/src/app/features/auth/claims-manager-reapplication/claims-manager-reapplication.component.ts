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
  AuthService
} from '../../../core/services/auth.service';


import {
  CLAIMS_MANAGER_DOCUMENT_CONFIG
} from '../../../core/constants/document-validation.constants';


@Component({
  selector: 'app-claims-manager-reapplication',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl:
    './claims-manager-reapplication.component.html',
  styleUrl:
    './claims-manager-reapplication.component.css'
})
export class ClaimsManagerReapplicationComponent {
  /*
   * Dependencies
   */
  private readonly formBuilder =
    inject(FormBuilder);


  private readonly destroyRef =
    inject(DestroyRef);


  private readonly authService =
    inject(AuthService);


  /*
   * Native file input.
   *
   * The HTML must contain:
   *
   * <input #fileInput type="file" ... />
   */
  readonly fileInput =
    viewChild<
      ElementRef<HTMLInputElement>
    >(
      'fileInput'
    );


  /*
   * Local UI state
   */
  readonly selectedFile =
    signal<File | null>(null);


  readonly fileError =
    signal<string | null>(null);


  readonly dragActive =
    signal(false);


  /*
   * Authentication workflow state
   */
  readonly loading =
    this.authService
      .claimsManagerReapplicationLoading;


  readonly successMessage =
    this.authService
      .claimsManagerReapplicationSuccessMessage;


  readonly errorMessage =
    this.authService
      .claimsManagerReapplicationErrorMessage;


  readonly backendFieldErrors =
    this.authService
      .claimsManagerReapplicationFieldErrors;


  /*
   * File picker configuration
   */
  readonly acceptedFileTypes =
    CLAIMS_MANAGER_DOCUMENT_CONFIG
      .acceptAttribute;


  /*
   * Reapplication form
   *
   * Matches ReapplyClaimsManagerRequest:
   *
   * firstName
   * lastName
   * phoneNumber
   * yearsOfExperience
   * professionalSummary
   */
  readonly reapplicationForm =
    this.formBuilder.nonNullable.group({
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
          this.noControlCharactersValidator        ]
      ],


      phoneNumber: [
        '',
        [
          Validators.required,
          Validators.maxLength(30)
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
          Validators.required,
          Validators.maxLength(2000),
          this.noControlCharactersValidator
        ]
      ]
    });


  constructor() {
    this.authService
      .clearClaimsManagerReapplicationState();


    this.reapplicationForm
      .valueChanges
      .pipe(
        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe(() => {
        const hasBackendFieldErrors =
          Object.keys(
            this.backendFieldErrors()
          ).length > 0;


        if (
          this.errorMessage() ||
          hasBackendFieldErrors
        ) {
          this.authService
            .clearClaimsManagerReapplicationErrors();
        }
      });
  }


  /*
   * Form-control getters used by the template.
   */
  get firstNameControl() {
    return this.reapplicationForm
      .controls.firstName;
  }


  get lastNameControl() {
    return this.reapplicationForm
      .controls.lastName;
  }


  get phoneNumberControl() {
    return this.reapplicationForm
      .controls.phoneNumber;
  }


  get yearsOfExperienceControl() {
    return this.reapplicationForm
      .controls.yearsOfExperience;
  }


  get professionalSummaryControl() {
    return this.reapplicationForm
      .controls.professionalSummary;
  }


  /*
   * Returns the first backend validation message
   * for the specified form field.
   */
  getBackendError(
    fieldName: string
  ): string | null {
    const fieldErrors =
      this.backendFieldErrors();


    return (
      fieldErrors[fieldName]?.[0] ??
      null
    );
  }


  /*
   * Opens the hidden native file input.
   */
  openFilePicker(): void {
    if (this.loading()) {
      return;
    }


    this.fileInput()
      ?.nativeElement
      .click();
  }


  /*
   * Handles file selection through the file picker.
   */
  onFileSelected(
    event: Event
  ): void {
    const input =
      event.target as HTMLInputElement;


    const file =
      input.files?.item(0) ??
      null;


    this.processFile(file);
  }


  /*
   * Enables the visual drag state.
   */
  onDragOver(
    event: DragEvent
  ): void {
    event.preventDefault();
    event.stopPropagation();


    if (this.loading()) {
      return;
    }


    this.dragActive.set(true);
  }


  /*
   * Clears the visual drag state.
   */
  onDragLeave(
    event: DragEvent
  ): void {
    event.preventDefault();
    event.stopPropagation();


    this.dragActive.set(false);
  }


  /*
   * Handles a dropped verification document.
   */
  onFileDropped(
    event: DragEvent
  ): void {
    event.preventDefault();
    event.stopPropagation();


    this.dragActive.set(false);


    if (this.loading()) {
      return;
    }


    const file =
      event.dataTransfer
        ?.files
        ?.item(0) ??
      null;


    this.processFile(file);
  }


  /*
   * Removes the selected verification document.
   */
  removeSelectedFile(): void {
    this.selectedFile.set(null);
    this.fileError.set(null);
    this.dragActive.set(false);


    const input =
      this.fileInput()
        ?.nativeElement;


    if (input) {
      input.value = '';
    }
  }


  /*
   * Formats the selected file size for display.
   */
  getSelectedFileSize(): string {
    const file =
      this.selectedFile();


    if (!file) {
      return '';
    }


    const oneKilobyte =
      1024;


    const oneMegabyte =
      1024 * 1024;


    if (file.size < oneKilobyte) {
      return `${file.size} bytes`;
    }


    if (file.size < oneMegabyte) {
      return `${
        (
          file.size /
          oneKilobyte
        ).toFixed(1)
      } KB`;
    }


    return `${
      (
        file.size /
        oneMegabyte
      ).toFixed(2)
    } MB`;
  }


  /*
   * Submits the Claims Manager reapplication.
   */
  submit(): void {
    this.fileError.set(null);


    if (
      this.reapplicationForm.invalid
    ) {
      this.reapplicationForm
        .markAllAsTouched();
    }


    const verificationDocument =
      this.selectedFile();


    if (!verificationDocument) {
      this.fileError.set(
        'A new verification document is required.'
      );
    }


    if (
      this.reapplicationForm.invalid ||
      !verificationDocument
    ) {
      return;
    }


    const fileValidationError =
      this.validateFile(
        verificationDocument
      );


    if (fileValidationError) {
      this.fileError.set(
        fileValidationError
      );


      return;
    }


    if (this.loading()) {
      return;
    }


    const formValue =
      this.reapplicationForm
        .getRawValue();


    this.authService
      .reapplyClaimsManager(
        {
          firstName:
            formValue.firstName.trim(),


          lastName:
            formValue.lastName.trim(),


          phoneNumber:
            formValue.phoneNumber.trim(),


          yearsOfExperience:
            Number(
              formValue.yearsOfExperience
            ),


          professionalSummary:
            formValue
              .professionalSummary
              .trim()
        },


        verificationDocument
      );
  }


  /*
   * Returns to the application-status page.
   */
  returnToStatus(): void {
    this.authService
      .returnToClaimsManagerStatus();
  }


  /*
   * Validates and preserves a selected file.
   */
  private processFile(
    file: File | null
  ): void {
    this.fileError.set(null);


    if (!file) {
      this.removeSelectedFile();
      return;
    }


    const validationError =
      this.validateFile(file);


    if (validationError) {
      this.selectedFile.set(null);


      this.fileError.set(
        validationError
      );


      const input =
        this.fileInput()
          ?.nativeElement;


      if (input) {
        input.value = '';
      }


      return;
    }


    this.selectedFile.set(file);


    this.authService
      .clearClaimsManagerReapplicationErrors();
  }


  /*
   * Applies the same extension, MIME-type,
   * emptiness, and size rules as the backend.
   *
   * The backend remains responsible for authoritative
   * file-signature validation.
   */
  private validateFile(
    file: File
  ): string | null {
    if (file.size === 0) {
      return (
        'Verification document cannot be empty.'
      );
    }


    if (
      file.size >
      CLAIMS_MANAGER_DOCUMENT_CONFIG
        .maxFileSizeBytes
    ) {
      return (
        'Verification document cannot exceed 10 MB.'
      );
    }


    const extension =
      this.getFileExtension(
        file.name
      );


    if (!extension) {
      return (
        'The selected file does not have a supported extension.'
      );
    }


    const supportedExtensions:
      readonly string[] =
      CLAIMS_MANAGER_DOCUMENT_CONFIG
        .acceptedExtensions;


    if (
      !supportedExtensions.includes(
        extension
      )
    ) {
      return (
        'Only PDF, JPG, JPEG, and PNG files are allowed.'
      );
    }


    const normalizedMimeType =
      file.type
        .trim()
        .toLowerCase();


    if (!normalizedMimeType) {
      return (
        'The selected file does not have a valid MIME type.'
      );
    }


    const expectedMimeType =
      this.getExpectedMimeType(
        extension
      );


    if (!expectedMimeType) {
      return (
        'The selected document type is unsupported.'
      );
    }


    if (
      normalizedMimeType !==
      expectedMimeType
    ) {
      return (
        'The document MIME type does not match its file extension.'
      );
    }


    return null;
  }


  /*
   * Returns the MIME type expected by the backend.
   */
  private getExpectedMimeType(
    extension: string
  ): string {
    switch (extension) {
      case '.pdf':
        return 'application/pdf';


      case '.jpg':
      case '.jpeg':
        return 'image/jpeg';


      case '.png':
        return 'image/png';


      default:
        return '';
    }
  }


  /*
   * Extracts the final normalized file extension.
   */
  private getFileExtension(
    fileName: string
  ): string {
    const finalDotIndex =
      fileName.lastIndexOf('.');


    if (
      finalDotIndex < 0 ||
      finalDotIndex ===
        fileName.length - 1
    ) {
      return '';
    }


    return fileName
      .slice(finalDotIndex)
      .trim()
      .toLowerCase();
  }


  /*
   * Matches the backend control-character validation.
   */
  private noControlCharactersValidator(
    control: AbstractControl
  ): ValidationErrors | null {
    const value =
      typeof control.value === 'string'
        ? control.value
        : '';


    const containsControlCharacters =
      /[\u0000-\u001F\u007F]/.test(
        value
      );


    return containsControlCharacters
      ? {
          controlCharacters: true
        }
      : null;
  }
}

