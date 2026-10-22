import {
  CommonModule
} from '@angular/common';


import {
  Component,
  OnDestroy,
  OnInit,
  effect,
  inject,
  signal
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
  ADMIN_NAVIGATION_ITEMS,
  AdminNavigationItem
} from '../../../core/constants/admin-navigation.constants';


import {
  ActiveClaimsManager
} from '../../../core/models/admin/operations/claims/active-claims-manager.model';


import {
  Claim
} from '../../../core/models/admin/operations/claims/claim.model';


import {
  ClaimStatus
} from '../../../core/models/admin/operations/claims/claim-status.enum';


import {
  ClaimType
} from '../../../core/models/admin/operations/claims/claim-type.enum';


import {
  PolicyStatus
} from '../../../core/models/admin/operations/policies/policy-status.enum';


import {
  PolicySummary
} from '../../../core/models/admin/operations/policies/policy-summary.model';

import {
  PolicyDocument
} from '../../../core/models/policy-document.model';


import {
  ProductType
} from '../../../core/models/admin/quotes/product-type.enum';


import {
  AdminService
} from '../../../core/services/admin.service';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  ClaimTimelineComponent
} from '../../../shared/components/claim-timeline/claim-timeline.component';


type OperationsTab =
  | 'policies'
  | 'claims';


@Component({
  selector:
    'app-admin-operations',


  standalone:
    true,


  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    ClaimTimelineComponent
  ],


  templateUrl:
    './admin-operations.component.html',


  styleUrl:
    './admin-operations.component.css'
})
export class AdminOperationsComponent
  implements OnInit, OnDestroy {
  private readonly adminService =
    inject(AdminService);


  private readonly authService =
    inject(AuthService);


  private readonly formBuilder =
    inject(FormBuilder);


  private readonly router =
    inject(Router);


  /*
   * Prevents the assignment success effect from
   * processing the same successful operation more
   * than once.
   */
  private handledAssignmentSuccess =
    false;


  /*
   * Prevents the Claim synchronization effect from
   * unnecessarily resetting the form when the same
   * Claim instance is refreshed without assignment
   * changes.
   */
  private synchronizedAssignmentKey =
    '';


  readonly ProductType =
    ProductType;


  readonly PolicyStatus =
    PolicyStatus;


  readonly ClaimStatus =
    ClaimStatus;


  readonly ClaimType =
    ClaimType;


  readonly navigationItems =
    ADMIN_NAVIGATION_ITEMS;


  readonly pendingApplicationCount =
    this.adminService
      .pendingApplicationCount;


  readonly activeTab =
    signal<OperationsTab>(
      'policies'
    );


  readonly policyDetailsOpen =
    signal(
      false
    );


  readonly claimDetailsOpen =
    signal(
      false
    );


  readonly claimFilterValidationMessage =
    signal<string | null>(
      null
    );


  readonly policies =
    this.adminService.policies;


  readonly policiesLoading =
    this.adminService.policiesLoading;


  readonly selectedPolicy =
    this.adminService.selectedPolicy;


  readonly policyDetailsLoading =
    this.adminService
      .policyDetailsLoading;

  readonly downloadingPolicyDocumentId =
    this.adminService.downloadingPolicyDocumentId;

  readonly retryingPolicyDocumentId =
    this.adminService.retryingPolicyDocumentId;


  readonly policyTotalCount =
    this.adminService.policyTotalCount;


  readonly policyPageNumber =
    this.adminService.policyPageNumber;


  readonly policyPageSize =
    this.adminService.policyPageSize;


  readonly policyTotalPages =
    this.adminService.policyTotalPages;


  readonly policyHasPreviousPage =
    this.adminService
      .policyHasPreviousPage;


  readonly policyHasNextPage =
    this.adminService
      .policyHasNextPage;


  readonly claimsDashboard =
    this.adminService.claimsDashboard;


  readonly claimsDashboardLoading =
    this.adminService
      .claimsDashboardLoading;


  readonly claims =
    this.adminService.claims;


  readonly claimsLoading =
    this.adminService.claimsLoading;


  readonly selectedClaim =
    this.adminService.selectedClaim;


  readonly claimDetailsLoading =
    this.adminService
      .claimDetailsLoading;


  readonly claimTotalCount =
    this.adminService.claimTotalCount;


  readonly claimPageNumber =
    this.adminService.claimPageNumber;


  readonly claimPageSize =
    this.adminService.claimPageSize;


  readonly claimTotalPages =
    this.adminService.claimTotalPages;


  readonly claimHasPreviousPage =
    this.adminService
      .claimHasPreviousPage;


  readonly claimHasNextPage =
    this.adminService
      .claimHasNextPage;


  readonly operationsLoading =
    this.adminService.operationsLoading;


  readonly activeClaimsManagers =
    this.adminService
      .activeClaimsManagers;


  readonly activeClaimsManagersLoading =
    this.adminService
      .activeClaimsManagersLoading;


  readonly assignmentProcessing =
    this.adminService
      .assignmentProcessing;


  readonly assignmentSuccessMessage =
    this.adminService
      .assignmentSuccessMessage;


  readonly canAssignSelectedClaim =
    this.adminService
      .canAssignSelectedClaim;


  readonly selectedClaimIsAssigned =
    this.adminService
      .selectedClaimIsAssigned;


  readonly errorMessage =
    this.adminService.errorMessage;


  readonly policyFilterForm =
    this.formBuilder.nonNullable.group({
      productName: [
        ''
      ],


      status: [
        ''
      ],


      productType: [
        ''
      ],


      pageSize: [
        10
      ]
    });


  readonly claimFilterForm =
    this.formBuilder.nonNullable.group({
      claimNumber: [
        ''
      ],


      customerId: [
        ''
      ],


      status: [
        ''
      ],


      claimType: [
        ''
      ],


      fromDate: [
        ''
      ],


      toDate: [
        ''
      ],


      pageSize: [
        20
      ]
    });


  readonly assignmentForm =
    this.formBuilder.nonNullable.group({
      claimsManagerId: [
        '',
        [
          Validators.required
        ]
      ],


      reason: [
        '',
        [
          Validators.maxLength(
            2000
          )
        ]
      ]
    });


  constructor() {
    /*
     * Keeps the assignment form synchronized whenever
     * the selected Claim is loaded or reloaded.
     */
    effect(() => {
      const claim =
        this.selectedClaim();


      const detailsOpen =
        this.claimDetailsOpen();


      if (
        !claim ||
        !detailsOpen
      ) {
        return;
      }


      const assignmentKey =
        [
          claim.id,
          claim.assignedClaimsManagerId ??
            '',
          claim.assignedAtUtc ??
            ''
        ].join('|');


      if (
        assignmentKey ===
          this.synchronizedAssignmentKey
      ) {
        return;
      }


      this.synchronizedAssignmentKey =
        assignmentKey;


      this.assignmentForm.patchValue(
        {
          claimsManagerId:
            claim.assignedClaimsManagerId ??
            '',


          reason:
            ''
        },
        {
          emitEvent:
            false
        }
      );


      this.assignmentForm
        .markAsPristine();


      this.assignmentForm
        .markAsUntouched();


      this.assignmentForm.controls
        .claimsManagerId
        .setErrors(
          null
        );


      this.assignmentForm.controls
        .reason
        .setErrors(
          null
        );
    });


    /*
     * The existing AdminService exposes assignment as
     * a void operation. Therefore, the component detects
     * successful completion through the success signal.
     *
     * After success:
     * 1. Reload the open Claim.
     * 2. Refresh the Claims table.
     * 3. Refresh the Claims dashboard.
     */
    effect(() => {
      const successMessage =
        this.assignmentSuccessMessage();


      const claim =
        this.selectedClaim();


      if (
        !successMessage ||
        !claim ||
        !this.claimDetailsOpen() ||
        this.handledAssignmentSuccess
      ) {
        return;
      }


      this.handledAssignmentSuccess =
        true;


      this.synchronizedAssignmentKey =
        '';


      this.adminService.loadClaim(
        claim.id
      );


      this.refreshClaimsAfterAssignment();


      this.adminService
        .loadClaimsDashboard();
    });
  }


  ngOnInit(): void {
    this.adminService
      .clearError();


    this.adminService
      .clearAssignmentMessage();


    this.adminService
      .loadPendingClaimsManagers();


    this.adminService
      .loadOperations();
  }


  ngOnDestroy(): void {
    this.adminService
      .clearAssignmentMessage();


    this.adminService
      .clearSelectedPolicy();


    this.adminService
      .clearSelectedClaim();


    this.adminService
      .clearError();


    this.handledAssignmentSuccess =
      false;


    this.synchronizedAssignmentKey =
      '';
  }


  selectTab(
    tab:
      OperationsTab
  ): void {
    if (
      this.activeTab() ===
        tab
    ) {
      return;
    }


    this.activeTab.set(
      tab
    );


    this.adminService
      .clearError();


    this.claimFilterValidationMessage.set(
      null
    );


    if (
      tab === 'policies' &&
      this.policies().length === 0 &&
      !this.policiesLoading()
    ) {
      this.adminService
        .loadPolicies();
    }


    if (
      tab === 'claims'
    ) {
      if (
        !this.claimsDashboard() &&
        !this.claimsDashboardLoading()
      ) {
        this.adminService
          .loadClaimsDashboard();
      }


      if (
        this.claims().length === 0 &&
        !this.claimsLoading()
      ) {
        this.adminService
          .loadClaims();
      }
    }
  }


  refreshOperations(): void {
    if (
      this.operationsLoading() ||
      this.assignmentProcessing()
    ) {
      return;
    }


    this.adminService
      .clearError();


    this.adminService
      .clearAssignmentMessage();


    this.claimFilterValidationMessage.set(
      null
    );


    this.adminService
      .refreshOperations();
  }


  applyPolicyFilters(): void {
    if (
      this.policiesLoading()
    ) {
      return;
    }


    this.adminService
      .clearError();


    const values =
      this.policyFilterForm
        .getRawValue();


    this.adminService.loadPolicies({
      productName:
        values.productName
          .trim() ||
        null,


      status:
        this.parseOptionalNumber<
          PolicyStatus
        >(
          String(
            values.status
          )
        ),


      productType:
        this.parseOptionalNumber<
          ProductType
        >(
          String(
            values.productType
          )
        ),


      pageNumber:
        1,


      pageSize:
        Number(
          values.pageSize
        )
    });
  }


  clearPolicyFilters(): void {
    if (
      this.policiesLoading()
    ) {
      return;
    }


    this.policyFilterForm.setValue({
      productName:
        '',


      status:
        '',


      productType:
        '',


      pageSize:
        10
    });


    this.adminService
      .clearError();


    this.adminService.loadPolicies({
      productName:
        null,


      status:
        null,


      productType:
        null,


      pageNumber:
        1,


      pageSize:
        10
    });
  }


  openPolicy(
    policy:
      PolicySummary
  ): void {
    const policyId =
      policy.id?.trim();


    if (
      !policyId ||
      this.policyDetailsLoading()
    ) {
      return;
    }


    this.adminService
      .clearError();


    this.adminService
      .clearSelectedPolicy();


    this.policyDetailsOpen.set(
      true
    );


    this.adminService.loadPolicy(
      policyId
    );
  }

  downloadPolicyDocument(
    policyId: string,
    document: PolicyDocument
  ): void {
    this.adminService.downloadPolicyDocument(
      policyId,
      document
    );
  }

  retryPolicyDocumentGeneration(
    policyId: string
  ): void {
    this.adminService.retryPolicyDocumentGeneration(
      policyId
    );
  }


  closePolicyDetails(): void {
    if (
      this.policyDetailsLoading()
    ) {
      return;
    }


    this.policyDetailsOpen.set(
      false
    );


    this.adminService
      .clearSelectedPolicy();


    this.adminService
      .clearError();
  }


  previousPolicyPage(): void {
    if (
      this.policiesLoading() ||
      !this.policyHasPreviousPage()
    ) {
      return;
    }


    this.adminService
      .goToPreviousPolicyPage();
  }


  nextPolicyPage(): void {
    if (
      this.policiesLoading() ||
      !this.policyHasNextPage()
    ) {
      return;
    }


    this.adminService
      .goToNextPolicyPage();
  }


  goToPolicyPage(
    pageNumber:
      number
  ): void {
    if (
      this.policiesLoading() ||
      pageNumber < 1 ||
      (
        this.policyTotalPages() > 0 &&
        pageNumber >
          this.policyTotalPages()
      )
    ) {
      return;
    }


    this.adminService.goToPolicyPage(
      pageNumber
    );
  }


  applyClaimFilters(): void {
    if (
      this.claimsLoading()
    ) {
      return;
    }


    this.adminService
      .clearError();


    this.claimFilterValidationMessage.set(
      null
    );


    const values =
      this.claimFilterForm
        .getRawValue();


    const fromDate =
      this.toStartOfDayIso(
        values.fromDate
      );


    const toDate =
      this.toEndOfDayIso(
        values.toDate
      );


    if (
      fromDate &&
      toDate &&
      new Date(
        fromDate
      ).getTime() >
        new Date(
          toDate
        ).getTime()
    ) {
      this.claimFilterValidationMessage.set(
        'From date must not be later than to date.'
      );


      return;
    }


    this.adminService.loadClaims({
      claimNumber:
        values.claimNumber
          .trim() ||
        null,


      customerId:
        values.customerId
          .trim() ||
        null,


      status:
        this.parseOptionalNumber<
          ClaimStatus
        >(
          String(
            values.status
          )
        ),


      claimType:
        this.parseOptionalNumber<
          ClaimType
        >(
          String(
            values.claimType
          )
        ),


      fromDate,


      toDate,


      pageNumber:
        1,


      pageSize:
        Number(
          values.pageSize
        )
    });
  }


  clearClaimFilters(): void {
    if (
      this.claimsLoading()
    ) {
      return;
    }


    this.claimFilterForm.setValue({
      claimNumber:
        '',


      customerId:
        '',


      status:
        '',


      claimType:
        '',


      fromDate:
        '',


      toDate:
        '',


      pageSize:
        20
    });


    this.claimFilterValidationMessage.set(
      null
    );


    this.adminService
      .clearError();


    this.adminService.loadClaims({
      claimNumber:
        null,


      customerId:
        null,


      status:
        null,


      claimType:
        null,


      fromDate:
        null,


      toDate:
        null,


      pageNumber:
        1,


      pageSize:
        20
    });
  }


  openClaim(
    claim:
      Claim
  ): void {
    const claimId =
      claim.id?.trim();


    if (
      !claimId ||
      this.claimDetailsLoading() ||
      this.assignmentProcessing()
    ) {
      return;
    }


    this.handledAssignmentSuccess =
      false;


    this.synchronizedAssignmentKey =
      '';


    this.assignmentForm.reset({
      claimsManagerId:
        claim.assignedClaimsManagerId ??
        '',


      reason:
        ''
    });


    this.assignmentForm
      .markAsPristine();


    this.assignmentForm
      .markAsUntouched();


    this.adminService
      .clearError();


    this.adminService
      .clearAssignmentMessage();


    this.adminService
      .clearSelectedClaim();


    this.claimDetailsOpen.set(
      true
    );


    this.adminService.loadClaim(
      claimId
    );


    this.adminService
      .loadActiveClaimsManagers();
  }


  closeClaimDetails(): void {
    if (
      this.claimDetailsLoading() ||
      this.assignmentProcessing()
    ) {
      return;
    }


    this.claimDetailsOpen.set(
      false
    );


    this.handledAssignmentSuccess =
      false;


    this.synchronizedAssignmentKey =
      '';


    this.assignmentForm.reset({
      claimsManagerId:
        '',


      reason:
        ''
    });


    this.adminService
      .clearAssignmentMessage();


    this.adminService
      .clearSelectedClaim();


    this.adminService
      .clearError();
  }


  previousClaimPage(): void {
    if (
      this.claimsLoading() ||
      !this.claimHasPreviousPage()
    ) {
      return;
    }


    this.adminService
      .goToPreviousClaimPage();
  }


  nextClaimPage(): void {
    if (
      this.claimsLoading() ||
      !this.claimHasNextPage()
    ) {
      return;
    }


    this.adminService
      .goToNextClaimPage();
  }


  goToClaimPage(
    pageNumber:
      number
  ): void {
    if (
      this.claimsLoading() ||
      pageNumber < 1 ||
      (
        this.claimTotalPages() > 0 &&
        pageNumber >
          this.claimTotalPages()
      )
    ) {
      return;
    }


    this.adminService.goToClaimPage(
      pageNumber
    );
  }


  submitClaimsManagerAssignment(): void {
    if (
      this.assignmentProcessing()
    ) {
      return;
    }


    this.adminService
      .clearError();


    this.adminService
      .clearAssignmentMessage();


    this.handledAssignmentSuccess =
      false;


    const claim =
      this.selectedClaim();


    if (!claim) {
      return;
    }


    this.assignmentForm
      .markAllAsTouched();


    const values =
      this.assignmentForm
        .getRawValue();


    const claimsManagerId =
      values.claimsManagerId
        .trim();


    const reason =
      values.reason
        .trim() ||
      null;


    if (!claimsManagerId) {
      this.assignmentForm
        .controls
        .claimsManagerId
        .setErrors({
          required:
            true
        });


      return;
    }


    if (
      !this.canAssignSelectedClaim()
    ) {
      return;
    }


    const currentClaimsManagerId =
      claim.assignedClaimsManagerId
        ?.trim() ||
      null;


    const isReassignment =
      Boolean(
        currentClaimsManagerId
      );


    if (
      isReassignment &&
      !reason
    ) {
      this.assignmentForm
        .controls
        .reason
        .setErrors({
          reassignmentReasonRequired:
            true
        });


      return;
    }


    if (
      currentClaimsManagerId &&
      claimsManagerId ===
        currentClaimsManagerId
    ) {
      this.assignmentForm
        .controls
        .claimsManagerId
        .setErrors({
          alreadyAssigned:
            true
        });


      return;
    }


    if (
      this.assignmentForm.invalid
    ) {
      return;
    }


    this.adminService
      .assignClaimsManager(
        claim.id,
        claimsManagerId,
        reason
      );
  }


  onClaimsManagerSelectionChange(): void {
    const claim =
      this.selectedClaim();


    const control =
      this.assignmentForm
        .controls
        .claimsManagerId;


    if (!claim) {
      return;
    }


    const selectedId =
      control.value.trim();


    const assignedId =
      claim.assignedClaimsManagerId
        ?.trim() ||
      '';


    this.removeControlError(
      'alreadyAssigned'
    );


    if (
      selectedId &&
      selectedId ===
        assignedId
    ) {
      control.setErrors({
        ...control.errors,


        alreadyAssigned:
          true
      });
    }
  }


  onAssignmentReasonInput(): void {
    this.removeReasonControlError(
      'reassignmentReasonRequired'
    );
  }


  claimsManagerSelectionError():
    string | null {
    const control =
      this.assignmentForm
        .controls
        .claimsManagerId;


    if (
      !control.touched &&
      !control.dirty
    ) {
      return null;
    }


    if (
      control.hasError(
        'required'
      )
    ) {
      return 'Select a Claims Manager.';
    }


    if (
      control.hasError(
        'alreadyAssigned'
      )
    ) {
      return 'This Claim is already assigned to the selected Claims Manager.';
    }


    return null;
  }


  assignmentReasonError():
    string | null {
    const control =
      this.assignmentForm
        .controls
        .reason;


    if (
      !control.touched &&
      !control.dirty
    ) {
      return null;
    }


    if (
      control.hasError(
        'reassignmentReasonRequired'
      )
    ) {
      return 'A reason is required when reassigning a Claim.';
    }


    if (
      control.hasError(
        'maxlength'
      )
    ) {
      return 'The assignment reason cannot exceed 2000 characters.';
    }


    return null;
  }


  assignmentActionLabel(): string {
    return this.selectedClaimIsAssigned()
      ? 'Reassign Claims Manager'
      : 'Assign Claims Manager';
  }


  assignmentButtonLabel(): string {
    if (
      this.assignmentProcessing()
    ) {
      return this.selectedClaimIsAssigned()
        ? 'Reassigning...'
        : 'Assigning...';
    }


    return this.assignmentActionLabel();
  }


  assignmentReasonRequired(): boolean {
    return this.selectedClaimIsAssigned();
  }


  selectedActiveClaimsManager():
    ActiveClaimsManager | null {
    const selectedId =
      this.assignmentForm
        .controls
        .claimsManagerId
        .value
        .trim();


    if (!selectedId) {
      return null;
    }


    return (
      this.activeClaimsManagers()
        .find(
          claimsManager =>
            claimsManager.userId ===
              selectedId
        ) ??
      null
    );
  }


  claimsManagerLabel(
    claimsManager:
      ActiveClaimsManager
  ): string {
    const displayName =
      claimsManager.displayName
        ?.trim();


    if (displayName) {
      return (
        `${displayName} · ` +
        claimsManager.email
      );
    }


    const generatedName =
      [
        claimsManager.firstName,
        claimsManager.lastName
      ]
        .filter(
          value =>
            Boolean(
              value?.trim()
            )
        )
        .map(
          value =>
            value.trim()
        )
        .join(' ');


    if (generatedName) {
      return (
        `${generatedName} · ` +
        claimsManager.email
      );
    }


    return claimsManager.email;
  }


  openNavigationItem(
    item:
      AdminNavigationItem
  ): void {
    if (
      !item.available ||
      !item.route
    ) {
      return;
    }


    if (
      this.isCurrentNavigationItem(
        item
      )
    ) {
      return;
    }


    this.navigateTo(
      item.route
    );
  }


  isCurrentNavigationItem(
    item:
      AdminNavigationItem
  ): boolean {
    if (!item.route) {
      return false;
    }


    const currentUrl =
      this.router.url
        .split('?')[0]
        .split('#')[0];


    return (
      currentUrl ===
        item.route
    );
  }


  policyStatusLabel(
    status:
      PolicyStatus | string
  ): string {
    switch (status) {
      case PolicyStatus.Active:
      case 'Active':
        return 'Active';


      case PolicyStatus.Cancelled:
      case 'Cancelled':
        return 'Cancelled';


      case PolicyStatus.Expired:
      case 'Expired':
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


  claimStatusLabel(
    status:
      ClaimStatus
  ): string {
    switch (status) {
      case ClaimStatus.Submitted:
        return 'Submitted';


      case ClaimStatus.UnderReview:
        return 'Under Review';


      case ClaimStatus
        .AdditionalInformationRequested:
        return 'Information Requested';


      case ClaimStatus.Approved:
        return 'Approved';


      case ClaimStatus.Rejected:
        return 'Rejected';


      case ClaimStatus.Paid:
        return 'Paid';


      case ClaimStatus.Closed:
        return 'Closed';


      case ClaimStatus.Withdrawn:
        return 'Withdrawn';


      default:
        return 'Unknown';
    }
  }


  claimStatusClass(
    status:
      ClaimStatus
  ): string {
    switch (status) {
      case ClaimStatus.Submitted:
        return 'status-submitted';


      case ClaimStatus.UnderReview:
        return 'status-review';


      case ClaimStatus
        .AdditionalInformationRequested:
        return 'status-information';


      case ClaimStatus.Approved:
        return 'status-approved';


      case ClaimStatus.Rejected:
        return 'status-rejected';


      case ClaimStatus.Paid:
        return 'status-paid';


      case ClaimStatus.Closed:
        return 'status-closed';


      case ClaimStatus.Withdrawn:
        return 'status-withdrawn';


      default:
        return 'status-neutral';
    }
  }


  claimTypeLabel(
    type:
      ClaimType
  ): string {
    switch (type) {
      case ClaimType.Fire:
        return 'Fire';


      case ClaimType.DwellingDamage:
        return 'Dwelling Damage';


      case ClaimType
        .OtherStructureDamage:
        return 'Other Structure Damage';


      case ClaimType
        .PersonalPropertyDamage:
        return 'Personal Property Damage';


      case ClaimType.Theft:
        return 'Theft';


      case ClaimType.Flood:
        return 'Flood';


      case ClaimType.Earthquake:
        return 'Earthquake';


      case ClaimType.Wildfire:
        return 'Wildfire';


      case ClaimType.Hurricane:
        return 'Hurricane';


      case ClaimType.Other:
        return 'Other';


      default:
        return 'Unknown';
    }
  }


  productTypeLabel(
    type:
      ProductType | string
  ): string {
    switch (type) {
      case ProductType
        .PropertyProtection:
      case 'PropertyProtection':
        return 'Property Protection';


      case ProductType
        .NaturalDisasterProtection:
      case 'NaturalDisasterProtection':
        return 'Natural Disaster Protection';


      case ProductType
        .ComprehensiveProtection:
      case 'ComprehensiveProtection':
        return 'Comprehensive Protection';


      default:
        return 'Unknown Product';
    }
  }


  formatCurrency(
    value:
      number | null
  ): string {
    if (
      value === null ||
      !Number.isFinite(
        value
      )
    ) {
      return 'Not available';
    }


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


  formatPercentage(
    value:
      number | null
  ): string {
    if (
      value === null ||
      !Number.isFinite(
        value
      )
    ) {
      return 'Not available';
    }


    return `${value.toFixed(2)}%`;
  }


  formatDate(
    value:
      string | null
  ): string {
    if (
      !value ||
      !value.trim()
    ) {
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


  formatDateTime(
    value:
      string | null
  ): string {
    if (
      !value ||
      !value.trim()
    ) {
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
          'numeric',


        hour:
          '2-digit',


        minute:
          '2-digit'
      }
    ).format(
      date
    );
  }


  formatFileSize(
    bytes:
      number
  ): string {
    if (
      !Number.isFinite(
        bytes
      ) ||
      bytes < 0
    ) {
      return 'Unknown size';
    }


    if (
      bytes < 1024
    ) {
      return `${bytes} B`;
    }


    const kilobytes =
      bytes /
      1024;


    if (
      kilobytes < 1024
    ) {
      return (
        `${kilobytes.toFixed(1)}` +
        ' KB'
      );
    }


    const megabytes =
      kilobytes /
      1024;


    return (
      `${megabytes.toFixed(1)}` +
      ' MB'
    );
  }


  coverageDecisionLabel(
    approvedAmount:
      number | null
  ): string {
    if (
      approvedAmount === null
    ) {
      return 'Pending Decision';
    }


    if (
      approvedAmount === 0
    ) {
      return 'Not Approved';
    }


    return 'Approved';
  }


  assignmentStatusLabel(
    assignedClaimsManagerId:
      string | null
  ): string {
    return assignedClaimsManagerId
      ? 'Assigned'
      : 'Unassigned';
  }


  trackByPolicyId(
    _index:
      number,
    policy:
      PolicySummary
  ): string {
    return policy.id;
  }


  trackByClaimId(
    _index:
      number,
    claim:
      Claim
  ): string {
    return claim.id;
  }


  clearError(): void {
    this.adminService
      .clearError();


    this.claimFilterValidationMessage.set(
      null
    );
  }


  clearAssignmentMessage(): void {
    this.adminService
      .clearAssignmentMessage();
  }


  goToDashboard(): void {
    this.navigateTo(
      '/admin/dashboard'
    );
  }


  goToApplications(): void {
    this.navigateTo(
      '/admin/claims-managers/pending'
    );
  }


  goToQuotes(): void {
    this.navigateTo(
      '/admin/quotes'
    );
  }


  goToCustomers(): void {
    this.navigateTo(
      '/admin/customers'
    );
  }


  goToProducts(): void {
    this.navigateTo(
      '/admin/products'
    );
  }


  goToPricing(): void {
    this.navigateTo(
      '/admin/pricing'
    );
  }


  goToOperations(): void {
    this.activeTab.set(
      'policies'
    );


    this.adminService
      .clearError();
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
    this.authService.logout();
  }


  private refreshClaimsAfterAssignment():
    void {
    const values =
      this.claimFilterForm
        .getRawValue();


    const fromDate =
      this.toStartOfDayIso(
        values.fromDate
      );


    const toDate =
      this.toEndOfDayIso(
        values.toDate
      );


    this.adminService.loadClaims({
      claimNumber:
        values.claimNumber
          .trim() ||
        null,


      customerId:
        values.customerId
          .trim() ||
        null,


      status:
        this.parseOptionalNumber<
          ClaimStatus
        >(
          String(
            values.status
          )
        ),


      claimType:
        this.parseOptionalNumber<
          ClaimType
        >(
          String(
            values.claimType
          )
        ),


      fromDate,


      toDate,


      pageNumber:
        this.claimPageNumber(),


      pageSize:
        Number(
          values.pageSize
        )
    });
  }


  private parseOptionalNumber<
    T extends number
  >(
    value:
      string
  ): T | null {
    const normalizedValue =
      value.trim();


    if (!normalizedValue) {
      return null;
    }


    const parsedValue =
      Number(
        normalizedValue
      );


    if (
      !Number.isInteger(
        parsedValue
      )
    ) {
      return null;
    }


    return parsedValue as T;
  }


  private toStartOfDayIso(
    value:
      string
  ): string | null {
    const normalizedValue =
      value.trim();


    if (!normalizedValue) {
      return null;
    }


    const date =
      new Date(
        `${normalizedValue}T00:00:00.000`
      );


    if (
      Number.isNaN(
        date.getTime()
      )
    ) {
      return null;
    }


    return date.toISOString();
  }


  private toEndOfDayIso(
    value:
      string
  ): string | null {
    const normalizedValue =
      value.trim();


    if (!normalizedValue) {
      return null;
    }


    const date =
      new Date(
        `${normalizedValue}T23:59:59.999`
      );


    if (
      Number.isNaN(
        date.getTime()
      )
    ) {
      return null;
    }


    return date.toISOString();
  }


  private removeControlError(
    errorName:
      string
  ): void {
    const control =
      this.assignmentForm
        .controls
        .claimsManagerId;


    const errors =
      control.errors;


    if (
      !errors ||
      !errors[errorName]
    ) {
      return;
    }


    const updatedErrors = {
      ...errors
    };


    delete updatedErrors[
      errorName
    ];


    control.setErrors(
      Object.keys(
        updatedErrors
      ).length > 0
        ? updatedErrors
        : null
    );
  }


  private removeReasonControlError(
    errorName:
      string
  ): void {
    const control =
      this.assignmentForm
        .controls
        .reason;


    const errors =
      control.errors;


    if (
      !errors ||
      !errors[errorName]
    ) {
      return;
    }


    const updatedErrors = {
      ...errors
    };


    delete updatedErrors[
      errorName
    ];


    control.setErrors(
      Object.keys(
        updatedErrors
      ).length > 0
        ? updatedErrors
        : null
    );
  }


  private navigateTo(
    route:
      string
  ): void {
    void this.router.navigateByUrl(
      route
    );
  }
}
