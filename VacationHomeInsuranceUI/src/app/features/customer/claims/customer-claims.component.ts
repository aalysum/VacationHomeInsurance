import {
  CommonModule
} from '@angular/common';


import {
  Component,
  OnDestroy,
  OnInit,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';


import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  Subscription,
  finalize
} from 'rxjs';


import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';


import {
  ClaimDocumentType
} from '../../../core/models/customer/claims/claim-document-type.enum';


import {
  ClaimDocument
} from '../../../core/models/customer/claims/claim-document.model';


import {
  ClaimStatus
} from '../../../core/models/customer/claims/claim-status.enum';


import {
  ClaimSummary
} from '../../../core/models/customer/claims/claim-summary.model';


import {
  ClaimType
} from '../../../core/models/customer/claims/claim-type.enum';


import {
  CreateClaimRequest
} from '../../../core/models/customer/claims/create-claim-request.model';


import {
  PolicyStatus
} from '../../../core/models/customer/policies/policy-status.enum';

import {
  PolicyDetails
} from '../../../core/models/customer/policies/policy-details.model';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  CustomerClaimService
} from '../../../core/services/customer-claim.service';


import {
  CustomerPolicyService
} from '../../../core/services/customer-policy.service';


import {
  CustomerService
} from '../../../core/services/customer.service';

import {
  parseApiError
} from '../../../core/utils/api-error.util';


import {
  ClaimTimelineComponent
} from '../../../shared/components/claim-timeline/claim-timeline.component';


type ClaimDialogMode =
  | 'create'
  | 'details'
  | 'withdraw'
  | 'document'
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


interface ClaimTypeOption {
  value: ClaimType;
  label: string;
}


interface ClaimCoverageFormValue {
  coverageId: string;
  coverageName: string;
  coverageLimitAmount: number;
  isOnSelectedPolicy: boolean;
  selected: boolean;
  requestedAmount: number;
}


@Component({
  selector:
    'app-customer-claims',


  standalone:
    true,


  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    ClaimTimelineComponent
  ],


  templateUrl:
    './customer-claims.component.html',


  styleUrl:
    './customer-claims.component.css'
})
export class CustomerClaimsComponent
  implements OnInit, OnDestroy {
  private readonly claimService =
    inject(CustomerClaimService);


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

  private eligibilityRequest:
    Subscription | null = null;

  private eligibilityRequestId = 0;

  private readonly subscriptions =
    new Subscription();


  readonly allClaimTypeOptions:
    readonly ClaimTypeOption[] = [
      {
        value: ClaimType.Fire,
        label: 'Fire'
      },
      {
        value: ClaimType.Flood,
        label: 'Flood'
      },
      {
        value: ClaimType.Theft,
        label: 'Theft'
      },
      {
        value: ClaimType.Earthquake,
        label: 'Earthquake'
      },
      {
        value: ClaimType.Wildfire,
        label: 'Wildfire'
      },
      {
        value: ClaimType.Hurricane,
        label: 'Hurricane'
      },
      {
        value: ClaimType.Other,
        label: 'Other'
      }
    ];


  readonly ClaimStatus =
    ClaimStatus;


  readonly ClaimType =
    ClaimType;


  readonly ClaimDocumentType =
    ClaimDocumentType;


  readonly dialogMode =
    signal<ClaimDialogMode>(
      null
    );


  readonly eligibilityLoading =
    signal(false);

  readonly eligibilityMessage =
    signal<string | null>(null);

  readonly eligibleCoverageIds =
    signal<ReadonlySet<string>>(
      new Set<string>()
    );


  readonly selectedDocumentFile =
    signal<File | null>(
      null
    );


  readonly documentFileTouched =
    signal(
      false
    );


  readonly claims =
    this.claimService.claims;


  readonly claimItems =
    this.claimService.claimItems;


  readonly selectedClaim =
    this.claimService.selectedClaim;


  readonly documents =
    this.claimService.documents;


  readonly claimsLoading =
    this.claimService.claimsLoading;


  readonly claimDetailsLoading =
    this.claimService.claimDetailsLoading;


  readonly claimProcessing =
    this.claimService.claimProcessing;


  readonly documentProcessing =
    this.claimService.documentProcessing;


  readonly errorMessage =
    this.claimService.errorMessage;


  readonly successMessage =
    this.claimService.successMessage;


  readonly pageNumber =
    this.claimService.pageNumber;


  readonly pageSize =
    this.claimService.pageSize;


  readonly totalCount =
    this.claimService.totalCount;


  readonly totalPages =
    this.claimService.totalPages;


  readonly hasPreviousPage =
    this.claimService.hasPreviousPage;


  readonly hasNextPage =
    this.claimService.hasNextPage;


  readonly policies =
    this.policyService.policies;


  readonly selectedPolicy =
    this.policyService.selectedPolicy;


  readonly policyDetailsLoading =
    this.policyService.policyDetailsLoading;


  readonly activePolicies =
    computed(() => {
      return this.policies()
        .filter(
          policy =>
            policy.status ===
              PolicyStatus.Active
        );
    });


  readonly pageStart =
    computed(() => {
      if (
        this.totalCount() === 0
      ) {
        return 0;
      }


      return (
        (
          this.pageNumber() - 1
        ) *
        this.pageSize()
      ) + 1;
    });


  readonly pageEnd =
    computed(() => {
      return Math.min(
        this.pageNumber() *
          this.pageSize(),
        this.totalCount()
      );
    });


  readonly filterForm =
    this.formBuilder.nonNullable.group({
      claimNumber: [
        ''
      ],


      status: [
        ''
      ],


      pageSize: [
        20
      ]
    });


  readonly claimForm =
    this.formBuilder.group({
      policyId:
        this.formBuilder.nonNullable.control(
          '',
          [
            Validators.required
          ]
        ),


      type:
        this.formBuilder.control<ClaimType | null>(
          null,
          [
            Validators.required
          ]
        ),


      incidentDate:
        this.formBuilder.nonNullable.control(
          '',
          [
            Validators.required
          ]
        ),


      title:
        this.formBuilder.nonNullable.control(
          '',
          [
            Validators.required,
            Validators.maxLength(
              200
            )
          ]
        ),


      description:
        this.formBuilder.nonNullable.control(
          '',
          [
            Validators.required,
            Validators.maxLength(
              4000
            )
          ]
        ),


      coverages:
        this.formBuilder.array([])
    });


  readonly withdrawalForm =
    this.formBuilder.nonNullable.group({
      reason: [
        '',
        [
          Validators.maxLength(
            1000
          )
        ]
      ]
    });


  readonly documentForm =
    this.formBuilder.nonNullable.group({
      documentType: [
        ClaimDocumentType.Photo,
        [
          Validators.required
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


  constructor() {
    effect(() => {
      const policy =
        this.selectedPolicy();


      if (
        !policy ||
        this.dialogMode() !==
          'create'
      ) {
        return;
      }

      this.populateCoverageControls(policy);
      this.loadEligibleCoverages();
    });

    this.subscriptions.add(
      this.claimForm.controls.type.valueChanges
        .subscribe(() => this.loadEligibleCoverages())
    );
  }


  get coverageControls():
    FormArray {
    return this.claimForm.controls
      .coverages;
  }


  ngOnInit(): void {
    this.claimService
      .clearError();


    this.claimService
      .clearSuccessMessage();


    this.customerService
      .loadProfile();


    this.policyService
      .loadPolicies();


    this.loadClaims();


    const claimId =
      this.activatedRoute
        .snapshot
        .queryParamMap
        .get(
          'claimId'
        )
        ?.trim();


    if (claimId) {
      this.openClaimById(
        claimId
      );
    }
  }


  ngOnDestroy(): void {
    this.eligibilityRequest?.unsubscribe();
    this.subscriptions.unsubscribe();

    this.claimService
      .clearSelectedClaim();


    this.claimService
      .clearError();


    this.claimService
      .clearSuccessMessage();


    this.policyService
      .clearSelectedPolicy();


    this.coverageControls.clear();
    this.clearEligibilityState();


    this.selectedDocumentFile.set(
      null
    );
  }


  loadClaims(): void {
    this.claimService.loadClaims({
      status:
        null,


      claimNumber:
        null,


      pageNumber:
        1,


      pageSize:
        20
    });
  }


  applyFilters(): void {
    if (
      this.claimsLoading()
    ) {
      return;
    }


    const values =
      this.filterForm.getRawValue();


    const status =
      String(
        values.status
      ).trim();


    this.claimService.loadClaims({
      status:
        status
          ? Number(
              status
            ) as ClaimStatus
          : null,


      claimNumber:
        values.claimNumber
          .trim() ||
        null,


      pageNumber:
        1,


      pageSize:
        Number(
          values.pageSize
        )
    });
  }


  clearFilters(): void {
    this.filterForm.reset({
      claimNumber:
        '',


      status:
        '',


      pageSize:
        20
    });


    this.loadClaims();
  }


  refreshClaims(): void {
    if (
      this.claimsLoading() ||
      this.claimProcessing()
    ) {
      return;
    }


    this.claimService
      .refreshClaims();
  }


  openCreateClaim(): void {
    if (
      this.claimProcessing()
    ) {
      return;
    }


    this.policyService
      .clearSelectedPolicy();

    this.resetClaimForm();


    this.claimService
      .clearError();


    this.claimService
      .clearSuccessMessage();


    this.dialogMode.set(
      'create'
    );
  }


  policyChanged(): void {
    const policyId =
      this.claimForm
        .controls
        .policyId
        .value
        .trim();


    this.clearEligibilityState();

    if (!policyId) {
      this.policyService
        .clearSelectedPolicy();


      return;
    }


    this.policyService
      .clearSelectedPolicy();


    this.policyService.loadPolicy(
      policyId
    );
  }


  submitClaim(): void {
    this.claimForm
      .markAllAsTouched();


    const policy =
      this.selectedPolicy();


    if (
      !policy ||
      this.claimProcessing() ||
      this.eligibilityLoading()
    ) {
      return;
    }

    if (!this.isIncidentDateWithinPolicy()) {
      this.claimService.setError(
        'Incident date must be today or earlier and within the selected Policy period.'
      );
      return;
    }

    const coverageValues =
      this.coverageControls
        .getRawValue() as
        ClaimCoverageFormValue[];


    const selectedCoverages =
      coverageValues.filter(
        coverage =>
          coverage.selected
      );

    const invalidSelectedCoverage =
      selectedCoverages.find(
        coverage =>
          !this.isCoverageEligibleById(
            coverage.coverageId
          ) ||
          !coverage.isOnSelectedPolicy
      );

    if (invalidSelectedCoverage) {
      this.claimService.setError(
        `${invalidSelectedCoverage.coverageName} is no longer eligible for the selected Policy and incident type. Unselect it or choose a different incident type.`
      );
      return;
    }

    if (
      selectedCoverages.length === 0
    ) {
      this.claimService.setError(
        'Select at least one affected Coverage.'
      );


      return;
    }


    const coverageWithoutAmount =
      selectedCoverages.find(
        coverage =>
          !Number.isFinite(
            Number(
              coverage.requestedAmount
            )
          ) ||
          Number(
            coverage.requestedAmount
          ) <= 0
      );


    if (coverageWithoutAmount) {
      this.claimService.setError(
        `Enter a requested amount for ${coverageWithoutAmount.coverageName}.`
      );


      return;
    }


    const exceededCoverage =
      selectedCoverages.find(
        coverage =>
          Number(
            coverage.requestedAmount
          ) >
          Number(
            coverage.coverageLimitAmount
          )
      );


    if (exceededCoverage) {
      this.claimService.setError(
        `${exceededCoverage.coverageName} exceeds the available Coverage limit.`
      );


      return;
    }


    const selectedClaimType =
      this.claimForm.controls.type
        .value;


    if (selectedClaimType === null) {
      this.claimService.setError(
        'Select an incident type before selecting eligible Coverages.'
      );
      return;
    }

    if (this.eligibleCoverageIds().size === 0) {
      this.claimService.setError(
        this.eligibilityMessage() ??
          'No eligible Coverages are available for this incident type on the selected Policy.'
      );
      return;
    }


    if (
      this.claimForm.invalid
    ) {
      return;
    }


    const values =
      this.claimForm.getRawValue();


    const coverages =
      selectedCoverages.map(
        coverage => ({
          coverageId:
            coverage.coverageId,


          requestedAmount:
            Number(
              coverage.requestedAmount
            )
        })
      );


    const request:
      CreateClaimRequest = {
        policyId:
          policy.id,


        propertyId:
          policy.propertyId,


        type:
          selectedClaimType,


        incidentDate:
          values.incidentDate,


        title:
          values.title.trim(),


        description:
          values.description.trim(),


        coverages
      };


    this.claimService
      .clearError();


    this.claimService.createClaim(
      request
    );
  }


  openClaim(
    claim:
      ClaimSummary
  ): void {
    const claimId =
      claim.id?.trim();


    if (!claimId) {
      return;
    }


    this.openClaimById(
      claimId
    );
  }


  openWithdrawal(): void {
    if (
      !this.canWithdrawClaim()
    ) {
      return;
    }


    this.withdrawalForm.reset({
      reason:
        ''
    });


    this.dialogMode.set(
      'withdraw'
    );
  }


  confirmWithdrawal(): void {
    const claim =
      this.selectedClaim();


    if (
      !claim ||
      !this.canWithdrawClaim() ||
      this.claimProcessing()
    ) {
      return;
    }


    this.claimService.withdrawClaim(
      claim.id,
      claim.rowVersion,
      this.withdrawalForm
        .controls
        .reason
        .value
    );


    this.dialogMode.set(
      'details'
    );
  }


  openDocumentForm(): void {
    if (
      !this.canManageDocuments()
    ) {
      return;
    }


    this.documentForm.reset({
      documentType:
        ClaimDocumentType.Photo
    });


    this.selectedDocumentFile.set(
      null
    );


    this.documentFileTouched.set(
      false
    );


    this.claimService
      .clearError();


    this.claimService
      .clearSuccessMessage();


    this.dialogMode.set(
      'document'
    );
  }


  documentFileChanged(
    event:
      Event
  ): void {
    const input =
      event.target as HTMLInputElement;


    const file =
      input.files?.item(
        0
      ) ??
      null;


    this.documentFileTouched.set(
      true
    );


    if (!file) {
      this.selectedDocumentFile.set(
        null
      );


      return;
    }


    const allowedContentTypes =
      new Set<string>([
        'application/pdf',
        'image/jpeg',
        'image/png'
      ]);


    const maximumFileSize =
      10 *
      1024 *
      1024;


    if (
      !allowedContentTypes.has(
        file.type
      )
    ) {
      this.selectedDocumentFile.set(
        null
      );


      input.value =
        '';


      this.claimService.setError(
        'Only PDF, JPG, and PNG files are allowed.'
      );


      return;
    }


    if (
      file.size >
        maximumFileSize
    ) {
      this.selectedDocumentFile.set(
        null
      );


      input.value =
        '';


      this.claimService.setError(
        'Maximum file size allowed is 10 MB.'
      );


      return;
    }


    this.claimService
      .clearError();


    this.selectedDocumentFile.set(
      file
    );
  }


  submitDocument(): void {
    this.documentForm
      .markAllAsTouched();


    this.documentFileTouched.set(
      true
    );


    const claim =
      this.selectedClaim();


    const file =
      this.selectedDocumentFile();


    if (
      !claim ||
      !file ||
      this.documentForm.invalid ||
      this.documentProcessing()
    ) {
      return;
    }


    const documentType =
      Number(
        this.documentForm.controls
          .documentType.value
      );


    this.claimService.uploadDocument(
      claim.id,
      documentType,
      file
    );


    this.selectedDocumentFile.set(
      null
    );


    this.documentFileTouched.set(
      false
    );


    this.dialogMode.set(
      'details'
    );
  }


  deleteDocument(
    document:
      ClaimDocument
  ): void {
    const claim =
      this.selectedClaim();


    if (
      !claim ||
      !this.canManageDocuments() ||
      this.documentProcessing()
    ) {
      return;
    }


    this.claimService.deleteDocument(
      claim.id,
      document.id
    );
  }


  returnToDetails(): void {
    if (
      this.claimProcessing() ||
      this.documentProcessing()
    ) {
      return;
    }


    this.selectedDocumentFile.set(
      null
    );


    this.documentFileTouched.set(
      false
    );


    this.claimService
      .clearError();


    this.dialogMode.set(
      'details'
    );
  }


  closeDialog(): void {
    if (
      this.claimProcessing() ||
      this.documentProcessing()
    ) {
      return;
    }


    this.dialogMode.set(
      null
    );


    this.claimService
      .clearSelectedClaim();


    this.policyService
      .clearSelectedPolicy();


    this.coverageControls.clear();
    this.clearEligibilityState();


    this.selectedDocumentFile.set(
      null
    );


    this.documentFileTouched.set(
      false
    );


    this.updateClaimQueryParameter(
      null
    );
  }


  previousPage(): void {
    this.claimService
      .goToPreviousPage();
  }


  nextPage(): void {
    this.claimService
      .goToNextPage();
  }


  canWithdrawClaim(): boolean {
    const status =
      this.selectedClaim()?.status;


    return (
      status !== undefined &&
      ![
        ClaimStatus.Approved,
        ClaimStatus.Paid,
        ClaimStatus.Closed,
        ClaimStatus.Rejected,
        ClaimStatus.Withdrawn
      ].includes(
        status
      )
    );
  }


  canManageDocuments(): boolean {
    const status =
      this.selectedClaim()?.status;


    return (
      status !== undefined &&
      ![
        ClaimStatus.Closed,
        ClaimStatus.Rejected,
        ClaimStatus.Withdrawn
      ].includes(
        status
      )
    );
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


      case ClaimType.OtherStructureDamage:
        return 'Other Structure Damage';


      case ClaimType.PersonalPropertyDamage:
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
        return 'Other';
    }
  }


  documentTypeLabel(
    type:
      ClaimDocumentType
  ): string {
    switch (type) {
      case ClaimDocumentType.Photo:
        return 'Photo';


      case ClaimDocumentType.Invoice:
        return 'Invoice';


      case ClaimDocumentType.PoliceReport:
        return 'Police Report';


      case ClaimDocumentType.AssessmentReport:
        return 'Assessment Report';


      case ClaimDocumentType.Receipt:
        return 'Receipt';


      default:
        return 'Other';
    }
  }


  formatCurrency(
    value:
      number | null
  ): string {
    if (value === null) {
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


  formatFileSize(
    bytes:
      number
  ): string {
    if (
      bytes < 1024
    ) {
      return `${bytes} B`;
    }


    if (
      bytes <
        1024 *
        1024
    ) {
      return `${(
        bytes /
        1024
      ).toFixed(1)} KB`;
    }


    return `${(
      bytes /
      (
        1024 *
        1024
      )
    ).toFixed(1)} MB`;
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


    return initials ||
      'CU';
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
    this.claimService
      .clearError();
  }


  clearSuccessMessage(): void {
    this.claimService
      .clearSuccessMessage();
  }


  logout(): void {
    this.authService.logout();
  }


  isCoverageEligible(
    control: AbstractControl
  ): boolean {
    const coverageId =
      control.get('coverageId')?.value as
        string | undefined;

    return Boolean(
      coverageId &&
      control.get('isOnSelectedPolicy')?.value &&
      this.eligibleCoverageIds().has(coverageId)
    );
  }

  isSelectedCoverageInvalid(
    control: AbstractControl
  ): boolean {
    return Boolean(
      control.get('selected')?.value &&
      !this.eligibilityLoading() &&
      !this.isCoverageEligible(control)
    );
  }

  coverageSelectionChanged(
    control: AbstractControl
  ): void {
    this.updateCoverageAmountValidators(control);
  }

  incidentDateMinimum(): string {
    return this.selectedPolicy()
      ?.effectiveDateUtc.slice(0, 10) ?? '';
  }

  incidentDateMaximum(): string {
    const expiryDate =
      this.selectedPolicy()
        ?.expiryDateUtc.slice(0, 10);
    const today =
      new Date().toISOString().slice(0, 10);

    return expiryDate && expiryDate < today
      ? expiryDate
      : today;
  }

  private populateCoverageControls(
    policy: PolicyDetails
  ): void {
    const previousValues =
      this.coverageControls.getRawValue() as
        ClaimCoverageFormValue[];
    const previousById =
      new Map(
        previousValues.map(value => [
          value.coverageId,
          value
        ])
      );
    const currentCoverageIds =
      new Set(
        policy.coverages.map(
          coverage => coverage.coverageId
        )
      );
    const formValues: ClaimCoverageFormValue[] =
      policy.coverages.map(coverage => {
        const previous =
          previousById.get(coverage.coverageId);

        return {
          coverageId: coverage.coverageId,
          coverageName: coverage.coverageName,
          coverageLimitAmount: coverage.coverageLimitAmount,
          isOnSelectedPolicy: true,
          selected: previous?.selected ?? false,
          requestedAmount:
            previous?.requestedAmount ?? 0
        };
      });

    for (const previous of previousValues) {
      if (
        previous.selected &&
        !currentCoverageIds.has(previous.coverageId)
      ) {
        formValues.push({
          ...previous,
          isOnSelectedPolicy: false
        });
      }
    }

    this.coverageControls.clear();

    for (const value of formValues) {
      const group =
        this.formBuilder.nonNullable.group({
          coverageId: [value.coverageId],
          coverageName: [value.coverageName],
          coverageLimitAmount: [
            value.coverageLimitAmount
          ],
          isOnSelectedPolicy: [
            value.isOnSelectedPolicy
          ],
          selected: [value.selected],
          requestedAmount: [value.requestedAmount]
        });

      this.coverageControls.push(group);
      this.updateCoverageAmountValidators(group);
    }
  }

  private loadEligibleCoverages(): void {
    this.eligibilityRequestId++;
    const requestId = this.eligibilityRequestId;
    this.eligibilityRequest?.unsubscribe();
    this.eligibilityLoading.set(false);
    this.eligibleCoverageIds.set(new Set<string>());
    this.eligibilityMessage.set(null);

    const policy = this.selectedPolicy();
    const incidentType =
      this.claimForm.controls.type.value;

    if (!policy || incidentType === null) {
      this.updateAllCoverageAmountValidators();
      return;
    }

    this.eligibilityLoading.set(true);
    this.eligibilityRequest =
      this.claimService
        .getEligibleCoverages(
          policy.id,
          incidentType
        )
        .pipe(
          finalize(() => {
            if (requestId === this.eligibilityRequestId) {
              this.eligibilityLoading.set(false);
            }
          })
        )
        .subscribe({
          next: result => {
            const eligibleCoverages =
              result.coverages ?? [];
            this.eligibleCoverageIds.set(
              new Set(
                eligibleCoverages.map(
                  coverage => coverage.coverageId
                )
              )
            );

            for (const coverage of eligibleCoverages) {
              const control =
                this.coverageControls.controls.find(
                  item =>
                    item.get('coverageId')?.value ===
                    coverage.coverageId
                );
              control?.get('coverageLimitAmount')
                ?.setValue(
                  coverage.remainingLimitAmount,
                  { emitEvent: false }
                );
            }

            this.eligibilityMessage.set(
              result.message || null
            );
            this.updateAllCoverageAmountValidators();
          },
          error: error => {
            this.eligibilityMessage.set(
              `Unable to load eligible Coverages: ${parseApiError(error).message}`
            );
            this.updateAllCoverageAmountValidators();
          }
        });
  }

  private updateAllCoverageAmountValidators(): void {
    for (const control of this.coverageControls.controls) {
      this.updateCoverageAmountValidators(control);
    }
  }

  private updateCoverageAmountValidators(
    control: AbstractControl
  ): void {
    const amountControl =
      control.get('requestedAmount');

    if (
      !amountControl ||
      !control.get('selected')?.value ||
      !this.isCoverageEligible(control)
    ) {
      amountControl?.clearValidators();
      amountControl?.updateValueAndValidity({
        emitEvent: false
      });
      return;
    }

    const remainingLimit =
      Number(
        control.get('coverageLimitAmount')?.value
      );

    amountControl.setValidators([
      Validators.required,
      Validators.min(0.01),
      Validators.max(remainingLimit)
    ]);
    amountControl.updateValueAndValidity({
      emitEvent: false
    });
  }

  private isCoverageEligibleById(
    coverageId: string
  ): boolean {
    return this.eligibleCoverageIds().has(coverageId);
  }

  private isIncidentDateWithinPolicy(): boolean {
    const incidentDate =
      this.claimForm.controls.incidentDate.value;
    const policy = this.selectedPolicy();

    return Boolean(
      incidentDate &&
      policy &&
      incidentDate >= policy.effectiveDateUtc.slice(0, 10) &&
      incidentDate <= policy.expiryDateUtc.slice(0, 10) &&
      incidentDate <= new Date().toISOString().slice(0, 10)
    );
  }

  private clearEligibilityState(): void {
    this.eligibilityRequestId++;
    this.eligibilityRequest?.unsubscribe();
    this.eligibilityRequest = null;
    this.eligibilityLoading.set(false);
    this.eligibilityMessage.set(null);
    this.eligibleCoverageIds.set(new Set<string>());
    this.updateAllCoverageAmountValidators();
  }


  private openClaimById(
    claimId:
      string
  ): void {
    this.claimService.loadClaim(
      claimId
    );


    this.dialogMode.set(
      'details'
    );


    this.updateClaimQueryParameter(
      claimId
    );
  }


  private resetClaimForm(): void {
    this.claimForm.reset({
      policyId:
        '',


      type:
        null,


      incidentDate:
        '',


      title:
        '',


      description:
        '',


      coverages:
        []
    });


    this.coverageControls.clear();
    this.clearEligibilityState();
  }


  private updateClaimQueryParameter(
    claimId:
      string | null
  ): void {
    void this.router.navigate(
      [],
      {
        relativeTo:
          this.activatedRoute,


        queryParams: {
          claimId
        },


        queryParamsHandling:
          'merge',


        replaceUrl:
          true
      }
    );
  }
}