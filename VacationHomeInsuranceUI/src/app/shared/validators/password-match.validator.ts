import {
  AbstractControl,
  ValidationErrors,
  ValidatorFn
} from '@angular/forms';


export function passwordMatchValidator(
  passwordControlName: string,
  confirmPasswordControlName: string
): ValidatorFn {
  return (
    group: AbstractControl
  ): ValidationErrors | null => {
    const passwordControl =
      group.get(passwordControlName);


    const confirmPasswordControl =
      group.get(
        confirmPasswordControlName
      );


    if (
      !passwordControl ||
      !confirmPasswordControl
    ) {
      return null;
    }


    const password =
      passwordControl.value as string;


    const confirmation =
      confirmPasswordControl.value as string;


    if (
      !confirmation ||
      password === confirmation
    ) {
      if (
        confirmPasswordControl
          .hasError('passwordMismatch')
      ) {
        const errors = {
          ...confirmPasswordControl.errors
        };


        delete errors[
          'passwordMismatch'
        ];


        confirmPasswordControl
          .setErrors(
            Object.keys(errors).length
              ? errors
              : null
          );
      }


      return null;
    }


    confirmPasswordControl.setErrors({
      ...confirmPasswordControl.errors,
      passwordMismatch: true
    });


    return {
      passwordMismatch: true
    };
  };
}
