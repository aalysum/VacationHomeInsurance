import {
  CommonModule
} from '@angular/common';


import {
  Component,
  OnDestroy,
  OnInit,
  inject
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
  CustomerStatus
} from '../../../core/models/admin/customer-status.enum';


import {
  UpdateCustomerProfileRequest
} from '../../../core/models/customer/update-customer-profile-request.model';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  CustomerService
} from '../../../core/services/customer.service';


interface CustomerNavigationItem {
  label: string;
  description: string;


  icon:
    | 'overview'
    | 'properties'
    | 'quotes'
    | 'policies'
    | 'claims'
    | 'profile'
    | 'security';


  route: string;
  available: boolean;
}


@Component({
  selector:
    'app-customer-profile',


  standalone:
    true,


  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],


  templateUrl:
    './customer-profile.component.html',


  styleUrl:
    './customer-profile.component.css'
})
export class CustomerProfileComponent
  implements OnInit, OnDestroy {
  private readonly customerService =
    inject(CustomerService);


  private readonly authService =
    inject(AuthService);


  private readonly formBuilder =
    inject(FormBuilder);


  private readonly router =
    inject(Router);


  readonly CustomerStatus =
    CustomerStatus;


  readonly profile =
    this.customerService.profile;


  readonly loading =
    this.customerService.profileLoading;


  readonly errorMessage =
    this.customerService.errorMessage;


  readonly successMessage =
    this.customerService.successMessage;
  readonly navigationItems:
    readonly CustomerNavigationItem[] = [
      {
        label:
          'Overview',


        description:
          'Customer account summary',


        icon:
          'overview',


        route:
          '/customer/dashboard',


        available:
          true
      },


      {
        label:
          'Properties',


        description:
          'Manage vacation properties',


        icon:
          'properties',


        route:
          '/customer/properties',


        available:
          true
      },


      {
        label:
          'Quotes',


        description:
          'Generate and review quotes',


        icon:
          'quotes',


        route:
          '/customer/quotes',


        available:
          true
      },


      {
        label:
          'Policies',


        description:
          'View active Policies',


        icon:
          'policies',


        route:
          '/customer/policies',


        available:
          true
      },


      {
        label:
          'Claims',


        description:
          'Submit and track Claims',


        icon:
          'claims',


        route:
          '/customer/claims',


        available:
          true
      },


      {
        label:
          'Profile',


        description:
          'Customer profile',


        icon:
          'profile',


        route:
          '/customer/profile',


        available:
          true
      },


      {
        label:
          'Security',


        description:
          'Password and MFA settings',


        icon:
          'security',


        route:
          '/account/security/mfa',


        available:
          true
      }
    ];
  readonly profileForm =
    this.formBuilder.nonNullable.group({
      dateOfBirth: [
        ''
      ],


      addressLine1: [
        '',
        [
          Validators.maxLength(
            250
          )
        ]
      ],


      addressLine2: [
        '',
        [
          Validators.maxLength(
            250
          )
        ]
      ],


      city: [
        '',
        [
          Validators.maxLength(
            100
          )
        ]
      ],


      state: [
        '',
        [
          Validators.maxLength(
            100
          )
        ]
      ],


      postalCode: [
        '',
        [
          Validators.maxLength(
            20
          )
        ]
      ],


      country: [
        '',
        [
          Validators.maxLength(
            100
          )
        ]
      ]
    });


  ngOnInit(): void {
    this.customerService.clearError();
    this.customerService.clearSuccessMessage();
    this.customerService.loadProfile();


    const profile =
      this.profile();


    if (profile) {
      this.populateForm();
    }
  }


  ngOnDestroy(): void {
    this.customerService.clearError();
    this.customerService.clearSuccessMessage();
  }


  populateForm(): void {
    const profile =
      this.profile();


    if (!profile) {
      return;
    }


    this.profileForm.setValue({
      dateOfBirth:
        profile.dateOfBirth
          ? profile.dateOfBirth.slice(
              0,
              10
            )
          : '',


      addressLine1:
        profile.addressLine1 ?? '',


      addressLine2:
        profile.addressLine2 ?? '',


      city:
        profile.city ?? '',


      state:
        profile.state ?? '',


      postalCode:
        profile.postalCode ?? '',


      country:
        profile.country ?? ''
    });
  }


  saveProfile(): void {
    this.profileForm.markAllAsTouched();


    if (
      this.profileForm.invalid ||
      this.loading()
    ) {
      return;
    }


    const values =
      this.profileForm.getRawValue();


    const request:
      UpdateCustomerProfileRequest = {
        dateOfBirth:
          values.dateOfBirth || null,


        addressLine1:
          values.addressLine1.trim(),


        addressLine2:
          values.addressLine2.trim(),


        city:
          values.city.trim(),


        state:
          values.state.trim(),


        postalCode:
          values.postalCode.trim(),


        country:
          values.country.trim()
      };


    this.customerService.updateProfile(
      request
    );
  }


  displayName(): string {
    const profile =
      this.profile();


    if (!profile) {
      return 'Customer';
    }


    const name =
      `${profile.firstName} ${profile.lastName}`
        .trim();


    return name || 'Customer';
  }


  initials(): string {
    const profile =
      this.profile();


    if (!profile) {
      return 'CU';
    }


    const initials =
      (
        profile.firstName
          .trim()
          .charAt(0) +
        profile.lastName
          .trim()
          .charAt(0)
      ).toUpperCase();


    return initials || 'CU';
  }


  phoneNumber(): string {
    return (
      this.profile()?.phoneNumber ||
      'Not available'
    );
  }


  accountStatus(): string {
    return (
      this.profile()?.status ===
      CustomerStatus.Active
    )
      ? 'Active'
      : 'Inactive';
  }


  openProperties(): void {
    this.navigateTo(
      '/customer/properties'
    );
  }


  openChangePassword(): void {
    this.navigateTo(
      '/account/security/change-password'
    );
  }


  openMfaSettings(): void {
    this.navigateTo(
      '/account/security/mfa'
    );
  }


  isCurrentNavigationItem(
    item:
      CustomerNavigationItem
  ): boolean {
    const currentUrl =
      this.router.url
        .split('?')[0]
        .split('#')[0];


    return (
      currentUrl ===
        item.route ||
      (
        item.route !==
          '/customer/dashboard' &&
        currentUrl.startsWith(
          `${item.route}/`
        )
      )
    );
  }


  clearError(): void {
    this.customerService.clearError();
  }


  clearSuccessMessage(): void {
    this.customerService
      .clearSuccessMessage();
  }


  logout(): void {
    this.authService.logout();
  }


  private navigateTo(
    route: string
  ): void {
    void this.router.navigateByUrl(
      route
    );
  }
}