import {
  CommonModule
} from '@angular/common';


import {
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';


import {
  toSignal
} from '@angular/core/rxjs-interop';


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
  finalize,
  startWith
} from 'rxjs';


import {
  ADMIN_NAVIGATION_ITEMS,
  AdminNavigationItem
} from '../../../core/constants/admin-navigation.constants';


import {
  AdminService
} from '../../../core/services/admin.service';


import {
  AuthService
} from '../../../core/services/auth.service';


@Component({
  selector: 'app-admin-pricing',


  standalone: true,


  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],


  templateUrl:
    './admin-pricing.component.html',


  styleUrl:
    './admin-pricing.component.css'
})
export class AdminPricingComponent
  implements OnInit {


  private readonly adminService =
    inject(AdminService);


  private readonly authService =
    inject(AuthService);


  private readonly formBuilder =
    inject(FormBuilder);


  private readonly router =
    inject(Router);


  readonly navigationItems =
    ADMIN_NAVIGATION_ITEMS;


  readonly pricing =
    this.adminService.pricing;


  readonly pricingLoading =
    this.adminService.pricingLoading;


  readonly pricingUpdating =
    this.adminService.pricingUpdating;


  readonly pendingApplicationCount =
    this.adminService.pendingApplicationCount;


  readonly errorMessage =
    this.adminService.errorMessage;


  readonly mobileNavigationOpen =
    signal(false);


  readonly confirmationOpen =
    signal(false);


  readonly successMessage =
    signal<string | null>(null);


  readonly pricingForm =
    this.formBuilder.nonNullable.group({
      baseRate: [
        0,
        [
          Validators.required,
          Validators.min(0.0001),
          Validators.max(0.02)
        ]
      ],


      lowRiskMultiplier: [
        0,
        [
          Validators.required,
          Validators.min(0.5),
          Validators.max(3)
        ]
      ],


      mediumRiskMultiplier: [
        0,
        [
          Validators.required,
          Validators.min(0.5),
          Validators.max(3)
        ]
      ],


      highRiskMultiplier: [
        0,
        [
          Validators.required,
          Validators.min(0.5),
          Validators.max(3)
        ]
      ]
    }, {
      validators: control => {
        const low = Number(
          control.get('lowRiskMultiplier')?.value
        );
        const medium = Number(
          control.get('mediumRiskMultiplier')?.value
        );
        const high = Number(
          control.get('highRiskMultiplier')?.value
        );

        return low <= medium && medium <= high
          ? null
          : { riskMultiplierOrder: true };
      }
    });


  /**
   * Reactive Forms are observable-based rather than signal-based.
   *
   * Converting valueChanges into a signal ensures that hasChanges()
   * recalculates whenever an administrator changes a form value.
   */
  private readonly pricingFormValue =
    toSignal(
      this.pricingForm.valueChanges.pipe(
        startWith(
          this.pricingForm.getRawValue()
        )
      ),
      {
        initialValue:
          this.pricingForm.getRawValue()
      }
    );


  readonly hasChanges =
    computed(() => {
      const currentPricing =
        this.pricing();


      if (!currentPricing) {
        return false;
      }


      const values =
        this.pricingFormValue();


      return (
        Number(values.baseRate) !==
          currentPricing.baseRate ||
        Number(values.lowRiskMultiplier) !==
          currentPricing.lowRiskMultiplier ||
        Number(values.mediumRiskMultiplier) !==
          currentPricing.mediumRiskMultiplier ||
        Number(values.highRiskMultiplier) !==
          currentPricing.highRiskMultiplier
      );
    });


  get baseRateControl() {
    return this.pricingForm.controls.baseRate;
  }


  get lowRiskMultiplierControl() {
    return this.pricingForm.controls.lowRiskMultiplier;
  }


  get mediumRiskMultiplierControl() {
    return this.pricingForm.controls.mediumRiskMultiplier;
  }


  get highRiskMultiplierControl() {
    return this.pricingForm.controls.highRiskMultiplier;
  }


  ngOnInit(): void {
    this.adminService.clearError();


    this.adminService
      .loadPendingClaimsManagers();


    this.loadPricing();
  }


  refreshPricing(): void {
    if (
      this.pricingUpdating() ||
      this.pricingLoading()
    ) {
      return;
    }


    this.confirmationOpen.set(false);
    this.successMessage.set(null);


    this.adminService.clearError();


    this.loadPricing();
  }


  openConfirmation(): void {
    this.successMessage.set(null);
    this.adminService.clearError();


    if (this.pricingForm.invalid) {
      this.pricingForm.markAllAsTouched();


      return;
    }


    if (
      !this.hasChanges() ||
      this.pricingUpdating()
    ) {
      return;
    }


    this.confirmationOpen.set(true);
  }


  cancelConfirmation(): void {
    if (this.pricingUpdating()) {
      return;
    }


    this.confirmationOpen.set(false);
  }


  resetPricingForm(): void {
    if (this.pricingUpdating()) {
      return;
    }


    const currentPricing =
      this.pricing();


    if (!currentPricing) {
      return;
    }


    this.populateForm(
      currentPricing.baseRate,
      currentPricing.lowRiskMultiplier,
      currentPricing.mediumRiskMultiplier,
      currentPricing.highRiskMultiplier
    );


    this.confirmationOpen.set(false);
    this.successMessage.set(null);


    this.adminService.clearError();
  }


  confirmUpdate(): void {
    if (this.pricingForm.invalid) {
      this.pricingForm.markAllAsTouched();


      return;
    }


    if (
      !this.hasChanges() ||
      this.pricingUpdating()
    ) {
      return;
    }


    const values =
      this.pricingForm.getRawValue();


    this.successMessage.set(null);
    this.adminService.clearError();


    this.adminService
      .updatePricing(
        Number(values.baseRate),
        Number(values.lowRiskMultiplier),
        Number(values.mediumRiskMultiplier),
        Number(values.highRiskMultiplier)
      )
      .pipe(
        finalize(() => {
          this.confirmationOpen.set(false);
        })
      )
      .subscribe({
        next: updatedPricing => {
          this.populateForm(
            updatedPricing.baseRate,
            updatedPricing.lowRiskMultiplier,
            updatedPricing.mediumRiskMultiplier,
            updatedPricing.highRiskMultiplier
          );


          this.successMessage.set(
            'Pricing configuration was updated successfully.'
          );
        }
      });
  }


  openNavigationItem(
    item: AdminNavigationItem
  ): void {
    if (
      !item.available ||
      !item.route
    ) {
      return;
    }


    if (this.router.url === item.route) {
      this.closeMobileNavigation();


      return;
    }


    this.navigateTo(item.route);
  }


  isCurrentNavigationItem(
    item: AdminNavigationItem
  ): boolean {
    if (!item.route) {
      return false;
    }


    const currentUrl =
      this.router.url
        .split('?')[0]
        .split('#')[0];


    return currentUrl === item.route;
  }


  formatDate(
    value: string | null
  ): string {
    if (
      !value ||
      !value.trim()
    ) {
      return 'Not updated';
    }


    const date =
      new Date(value);


    if (
      Number.isNaN(
        date.getTime()
      )
    ) {
      return 'Not available';
    }


    return new Intl.DateTimeFormat(
      'en-IN',
      {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      }
    ).format(date);
  }


  formatDecimal(
    value: number
  ): string {
    if (!Number.isFinite(value)) {
      return '0.00';
    }


    return new Intl.NumberFormat(
      'en-IN',
      {
        minimumFractionDigits: 2,
        maximumFractionDigits: 4
      }
    ).format(value);
  }


  clearSuccessMessage(): void {
    this.successMessage.set(null);
  }


  clearError(): void {
    this.adminService.clearError();
  }


  toggleMobileNavigation(): void {
    this.mobileNavigationOpen.update(
      open => !open
    );
  }


  closeMobileNavigation(): void {
    this.mobileNavigationOpen.set(false);
  }


  goToDashboard(): void {
    this.navigateTo('/admin/dashboard');
  }


  goToApplications(): void {
    this.navigateTo(
      '/admin/claims-managers/pending'
    );
  }


  goToQuotes(): void {
    this.navigateTo('/admin/quotes');
  }


  goToCustomers(): void {
    this.navigateTo('/admin/customers');
  }


  goToProducts(): void {
    this.navigateTo('/admin/products');
  }


  goToPricing(): void {
    this.closeMobileNavigation();
  }


  goToOperations(): void {
    this.navigateTo('/admin/operations');
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


  logout(): void {
    this.closeMobileNavigation();


    this.authService.logout();
  }


  private loadPricing(): void {
    this.adminService.clearError();


    this.adminService
      .getPricing()
      .subscribe({
        next: currentPricing => {
          this.populateForm(
            currentPricing.baseRate,
            currentPricing.lowRiskMultiplier,
            currentPricing.mediumRiskMultiplier,
            currentPricing.highRiskMultiplier
          );
        }
      });
  }


  private populateForm(
    baseRate: number,
    lowRiskMultiplier: number,
    mediumRiskMultiplier: number,
    highRiskMultiplier: number
  ): void {
    this.pricingForm.setValue({
      baseRate,
      lowRiskMultiplier,
      mediumRiskMultiplier,
      highRiskMultiplier
    });


    this.pricingForm.markAsPristine();
    this.pricingForm.markAsUntouched();


    this.pricingForm.updateValueAndValidity({
      emitEvent: false
    });
  }


  private navigateTo(
    route: string
  ): void {
    this.closeMobileNavigation();


    void this.router.navigateByUrl(route);
  }
}
