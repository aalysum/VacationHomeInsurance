import {
  CommonModule
} from '@angular/common';


import {
  Component,
  OnDestroy,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';


import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';


import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';


import {
  PolicyStatus
} from '../../../core/models/customer/policies/policy-status.enum';


import {
  PolicySummary
} from '../../../core/models/customer/policies/policy-summary.model';
import {
  PolicyDocument
} from '../../../core/models/policy-document.model';


import {
  ProductType
} from '../../../core/models/customer/quotes/product-type.enum';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  CustomerPolicyService
} from '../../../core/services/customer-policy.service';


import {
  CustomerService
} from '../../../core/services/customer.service';


type PolicyDialogMode =
  | 'details'
  | 'cancel'
  | 'renew'
  | null;


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
    'app-customer-policies',


  standalone:
    true,


  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],


  templateUrl:
    './customer-policies.component.html',


  styleUrl:
    './customer-policies.component.css'
})
export class CustomerPoliciesComponent
  implements OnInit, OnDestroy {
  private readonly policyService =
    inject(CustomerPolicyService);


  private readonly customerService =
    inject(CustomerService);


  private readonly authService =
    inject(AuthService);


  private readonly formBuilder =
    inject(FormBuilder);


  private readonly router =
    inject(Router);


  private readonly activatedRoute =
    inject(ActivatedRoute);


  /*
   * This value must match the renewal window
   * configured in the backend.
   *
   * For example, 30 means renewal becomes available
   * 30 days before the Policy expiry date.
   */
  private readonly renewalWindowDays =
    30;


  readonly PolicyStatus =
    PolicyStatus;


  readonly ProductType =
    ProductType;


  readonly dialogMode =
    signal<PolicyDialogMode>(
      null
    );


  readonly policies =
    this.policyService.policies;


  readonly selectedPolicy =
    this.policyService.selectedPolicy;


  readonly renewalEligibility =
    this.policyService.renewalEligibility;


  readonly renewalResult =
    this.policyService.renewalResult;


  readonly policiesLoading =
    this.policyService.policiesLoading;


  readonly policyDetailsLoading =
    this.policyService.policyDetailsLoading;

  readonly downloadingPolicyDocumentId =
    this.policyService.downloadingPolicyDocumentId;


  readonly renewalLoading =
    this.policyService.renewalLoading;


  readonly policyProcessing =
    this.policyService.policyProcessing;


  readonly errorMessage =
    this.policyService.errorMessage;


  readonly successMessage =
    this.policyService.successMessage;


  readonly totalPolicies =
    this.policyService.totalPolicies;


  readonly activePolicies =
    computed(() => {
      return this.policies()
        .filter(
          policy =>
            policy.status ===
              PolicyStatus.Active
        );
    });


  readonly expiredPolicies =
    computed(() => {
      return this.policies()
        .filter(
          policy =>
            policy.status ===
              PolicyStatus.Expired
        );
    });


  readonly cancelledPolicies =
    computed(() => {
      return this.policies()
        .filter(
          policy =>
            policy.status ===
              PolicyStatus.Cancelled
        );
    });


  readonly totalAnnualPremium =
    computed(() => {
      return this.activePolicies()
        .reduce(
          (
            total,
            policy
          ) =>
            total +
            policy.annualPremium,
          0
        );
    });


  readonly cancellationForm =
    this.formBuilder.nonNullable.group({
      reason: [
        '',
        [
          Validators.required,
          Validators.minLength(
            10
          ),
          Validators.maxLength(
            1000
          )
        ]
      ]
    });


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


  ngOnInit(): void {
    this.policyService
      .clearError();


    this.policyService
      .clearSuccessMessage();


    this.customerService
      .loadProfile();


    this.policyService
      .loadPolicies();


    const policyId =
      this.activatedRoute
        .snapshot
        .queryParamMap
        .get(
          'policyId'
        )
        ?.trim();


    if (policyId) {
      this.openPolicyById(
        policyId
      );
    }
  }


  ngOnDestroy(): void {
    this.policyService
      .clearSelectedPolicy();


    this.policyService
      .clearError();


    this.policyService
      .clearSuccessMessage();
  }


  refreshPolicies(): void {
    if (
      this.policiesLoading() ||
      this.policyProcessing()
    ) {
      return;
    }


    this.policyService
      .refreshPolicies();
  }


  openPolicy(
    policy:
      PolicySummary
  ): void {
    const policyId =
      policy.id?.trim();


    if (!policyId) {
      return;
    }


    this.openPolicyById(
      policyId
    );
  }

  downloadPolicyDocument(
    policyDocument: PolicyDocument
  ): void {
    const policy = this.selectedPolicy();

    if (policy) {
      this.policyService.downloadPolicyDocument(
        policy.id,
        policyDocument
      );
    }
  }


  openCancellation(): void {
    const policy =
      this.selectedPolicy();


    if (
      !policy ||
      policy.status !==
        PolicyStatus.Active
    ) {
      return;
    }


    this.cancellationForm.reset({
      reason:
        ''
    });


    this.dialogMode.set(
      'cancel'
    );
  }


  confirmCancellation(): void {
    this.cancellationForm
      .markAllAsTouched();


    const policy =
      this.selectedPolicy();


    if (
      !policy ||
      this.cancellationForm.invalid ||
      this.policyProcessing()
    ) {
      return;
    }


    this.policyService.cancelPolicy(
      policy.id,
      this.cancellationForm
        .controls
        .reason
        .value
    );


    this.dialogMode.set(
      'details'
    );
  }


  openRenewal(): void {
    const policy =
      this.selectedPolicy();


    if (
      !policy ||
      !this.canCheckRenewal() ||
      this.renewalLoading()
    ) {
      return;
    }


    this.dialogMode.set(
      'renew'
    );


    this.policyService
      .checkRenewalEligibility(
        policy.id
      );
  }


  createRenewal(): void {
    const policy =
      this.selectedPolicy();


    const eligibility =
      this.renewalEligibility();


    if (
      !policy ||
      !eligibility?.eligible ||
      this.renewalLoading()
    ) {
      return;
    }


    this.policyService
      .createRenewal(
        policy.id
      );
  }


  openRenewalQuote(): void {
    const result =
      this.renewalResult();


    const quoteId =
      result?.quoteId?.trim();


    if (!quoteId) {
      return;
    }


    void this.router.navigate(
      [
        '/customer/quotes'
      ],
      {
        queryParams: {
          quoteId
        }
      }
    );
  }


  returnToDetails(): void {
    if (
      this.policyProcessing() ||
      this.renewalLoading()
    ) {
      return;
    }


    this.dialogMode.set(
      'details'
    );
  }


  closeDialog(): void {
    if (
      this.policyProcessing() ||
      this.renewalLoading()
    ) {
      return;
    }


    this.dialogMode.set(
      null
    );


    this.policyService
      .clearSelectedPolicy();


    this.cancellationForm.reset({
      reason:
        ''
    });


    this.updateQueryParameters(
      null
    );
  }


  policyStatusLabel(
    status:
      PolicyStatus
  ): string {
    switch (status) {
      case PolicyStatus.Active:
        return 'Active';


      case PolicyStatus.Cancelled:
        return 'Cancelled';


      case PolicyStatus.Expired:
        return 'Expired';


      default:
        return 'Unknown';
    }
  }


  policyStatusClass(
    status:
      PolicyStatus
  ): string {
    switch (status) {
      case PolicyStatus.Active:
        return 'status-active';


      case PolicyStatus.Cancelled:
        return 'status-cancelled';


      case PolicyStatus.Expired:
        return 'status-expired';


      default:
        return 'status-neutral';
    }
  }


  productTypeLabel(
    productType:
      ProductType
  ): string {
    switch (productType) {
      case ProductType
        .PropertyProtection:
        return 'Property Protection';


      case ProductType
        .NaturalDisasterProtection:
        return 'Natural Disaster Protection';


      case ProductType
        .ComprehensiveProtection:
        return 'Comprehensive Protection';


      default:
        return 'Insurance Product';
    }
  }


  canCancelPolicy(): boolean {
    return (
      this.selectedPolicy()?.status ===
        PolicyStatus.Active
    );
  }


  canCheckRenewal(): boolean {
    const status =
      this.selectedPolicy()?.status;


    return (
      status ===
        PolicyStatus.Active ||
      status ===
        PolicyStatus.Expired
    );
  }


  /**
   * Returns true when an ineligible renewal result
   * specifically indicates that the renewal window
   * has not opened yet.
   */
  isRenewalWindowNotOpen(
    reason:
      string | null | undefined
  ): boolean {
    const normalizedReason =
      reason
        ?.trim()
        .toLowerCase();


    if (!normalizedReason) {
      return false;
    }


    return (
      normalizedReason.includes(
        'renewal window has not opened'
      ) ||
      normalizedReason.includes(
        'renewal window is not open'
      ) ||
      normalizedReason.includes(
        'renewal window has not yet opened'
      ) ||
      normalizedReason.includes(
        'renewal window is not yet open'
      )
    );
  }


  /**
   * Calculates the date when the renewal window opens.
   *
   * The calculation is:
   * Policy expiry date - renewalWindowDays.
   */
  renewalWindowStartDate(
    expiryDateUtc:
      string | null | undefined
  ): Date | null {
    if (!expiryDateUtc) {
      return null;
    }


    const expiryDate =
      new Date(
        expiryDateUtc
      );


    if (
      Number.isNaN(
        expiryDate.getTime()
      )
    ) {
      return null;
    }


    const renewalWindowStart =
      new Date(
        expiryDate.getTime()
      );


    renewalWindowStart.setUTCDate(
      renewalWindowStart.getUTCDate() -
        this.renewalWindowDays
    );


    return renewalWindowStart;
  }


  /**
   * Returns the number of calendar days remaining
   * until the renewal window opens.
   *
   * Returns:
   * - null when the date cannot be calculated
   * - 0 when the renewal window is already open
   * - a positive number before the window opens
   */
  daysUntilRenewalWindow(
    expiryDateUtc:
      string | null | undefined
  ): number | null {
    const renewalWindowStart =
      this.renewalWindowStartDate(
        expiryDateUtc
      );


    if (!renewalWindowStart) {
      return null;
    }


    const now =
      new Date();


    const currentUtcDate =
      Date.UTC(
        now.getUTCFullYear(),
        now.getUTCMonth(),
        now.getUTCDate()
      );


    const renewalStartUtcDate =
      Date.UTC(
        renewalWindowStart
          .getUTCFullYear(),
        renewalWindowStart
          .getUTCMonth(),
        renewalWindowStart
          .getUTCDate()
      );


    const difference =
      renewalStartUtcDate -
      currentUtcDate;


    if (difference <= 0) {
      return 0;
    }


    return Math.ceil(
      difference /
        (
          1000 *
          60 *
          60 *
          24
        )
    );
  }


  /**
   * Produces a customer-friendly message explaining
   * exactly when the Policy can be renewed.
   */
  renewalAvailabilityMessage(
    expiryDateUtc:
      string | null | undefined
  ): string {
    const renewalWindowStart =
      this.renewalWindowStartDate(
        expiryDateUtc
      );


    const daysRemaining =
      this.daysUntilRenewalWindow(
        expiryDateUtc
      );


    if (
      !renewalWindowStart ||
      daysRemaining === null
    ) {
      return (
        'The renewal availability date could not ' +
        'be determined.'
      );
    }


    const formattedDate =
      this.formatDate(
        renewalWindowStart.toISOString()
      );


    if (daysRemaining === 0) {
      return (
        'The renewal window opens on ' +
        `${formattedDate}.`
      );
    }


    if (daysRemaining === 1) {
      return (
        `You can renew this Policy from ` +
        `${formattedDate}, in 1 day.`
      );
    }


    return (
      `You can renew this Policy from ` +
      `${formattedDate}, in ` +
      `${daysRemaining} days.`
    );
  }


  daysUntilExpiry(
    expiryDateUtc:
      string
  ): number {
    const expiryTime =
      new Date(
        expiryDateUtc
      ).getTime();


    if (
      Number.isNaN(
        expiryTime
      )
    ) {
      return 0;
    }


    const difference =
      expiryTime -
      Date.now();


    return Math.ceil(
      difference /
        (
          1000 *
          60 *
          60 *
          24
        )
    );
  }


  expiryLabel(
    expiryDateUtc:
      string
  ): string {
    const days =
      this.daysUntilExpiry(
        expiryDateUtc
      );


    if (days < 0) {
      return 'Expired';
    }


    if (days === 0) {
      return 'Expires today';
    }


    if (days === 1) {
      return 'Expires in 1 day';
    }


    return `Expires in ${days} days`;
  }


  formatCurrency(
    value:
      number
  ): string {
    return new Intl.NumberFormat(
      'en-IN',
      {
        style:
          'currency',


        currency:
          'INR',


        minimumFractionDigits:
          2,


        maximumFractionDigits:
          2
      }
    ).format(
      value
    );
  }


  formatDate(
    value:
      string | null
  ): string {
    if (!value) {
      return 'Not available';
    }


    const date =
      new Date(
        value
      );


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
        day:
          '2-digit',


        month:
          'short',


        year:
          'numeric'
      }
    ).format(
      date
    );
  }


  displayName(): string {
    const profile =
      this.customerService.profile();


    if (!profile) {
      return 'Customer';
    }


    return (
      `${profile.firstName} ${profile.lastName}`
        .trim() ||
      'Customer'
    );
  }


  initials(): string {
    const profile =
      this.customerService.profile();


    if (!profile) {
      return 'CU';
    }


    const firstInitial =
      profile.firstName
        ?.trim()
        .charAt(0) ??
      '';


    const lastInitial =
      profile.lastName
        ?.trim()
        .charAt(0) ??
      '';


    const initials =
      (
        firstInitial +
        lastInitial
      )
        .trim()
        .toUpperCase();


    return initials || 'CU';
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
    this.policyService
      .clearError();
  }


  clearSuccessMessage(): void {
    this.policyService
      .clearSuccessMessage();
  }


  logout(): void {
    this.authService.logout();
  }


  private openPolicyById(
    policyId:
      string
  ): void {
    this.policyService.loadPolicy(
      policyId
    );


    this.dialogMode.set(
      'details'
    );


    this.updateQueryParameters(
      policyId
    );
  }


  private updateQueryParameters(
    policyId:
      string | null
  ): void {
    void this.router.navigate(
      [],
      {
        relativeTo:
          this.activatedRoute,


        queryParams: {
          policyId
        },


        queryParamsHandling:
          'merge',


        replaceUrl:
          true
      }
    );
  }
}