import {
  HttpErrorResponse,
  HttpResponse
} from '@angular/common/http';


import {
  Injectable,
  computed,
  inject,
  signal
} from '@angular/core';


import {
  catchError,
  finalize,
  map,
  Observable,
  switchMap,
  tap,
  throwError
} from 'rxjs';


import {
  PricingConfiguration
} from '../models/admin/pricing/pricing-configuration.model';


import {
  DownloadedFile
} from '../models/admin/downloaded-file.model';


import {
  PendingClaimsManagerResponse
} from '../models/admin/pending-claims-manager-response.model';


import {
  RejectClaimsManagerRequest
} from '../models/admin/reject-claims-manager-request.model';


import {
  AdminApiService
} from './admin-api.service';
import {
  GlobalNotificationService
} from './global-notification.service';


import {
  AdminCustomerDetails
} from '../models/admin/admin-customer-details.model';


import {
  AdminCustomerPropertyQuery
} from '../models/admin/admin-customer-property-query.model';


import {
  AdminCustomerQuery
} from '../models/admin/admin-customer-query.model';


import {
  AdminCustomerSummary
} from '../models/admin/admin-customer-summary.model';


import {
  CustomerStatus
} from '../models/admin/customer-status.enum';


import {
  PagedResult
} from '../models/admin/paged-result.model';


import {
  VacationProperty
} from '../models/admin/vacation-property.model';


import {
  AdminQuoteSearchQuery
} from '../models/admin/quotes/admin-quote-search-query.model';


import {
  QuoteDetails
} from '../models/admin/quotes/quote-details.model';


import {
  QuoteStatus
} from '../models/admin/quotes/quote-status.enum';


import {
  QuoteSummary
} from '../models/admin/quotes/quote-summary.model';


import {
  ProductCoverage
} from '../models/admin/products/product-coverage.model';


import {
  ProductDetails
} from '../models/admin/products/product-details.model';


import {
  PolicyDetails
} from '../models/admin/operations/policies/policy-details.model';


import {
  PolicyFilterRequest
} from '../models/admin/operations/policies/policy-filter-request.model';


import {
  PolicyPagedResult
} from '../models/admin/operations/policies/policy-paged-result.model';


import {
  PolicySummary
} from '../models/admin/operations/policies/policy-summary.model';
import {
  PolicyDocument
} from '../models/policy-document.model';


import {
  Claim
} from '../models/admin/operations/claims/claim.model';
import {
  ActiveClaimsManager
} from '../models/admin/operations/claims/active-claims-manager.model';


import {
  AssignClaimsManagerRequest
} from '../models/admin/operations/claims/assign-claims-manager-request.model';


import {
  ClaimDashboard
} from '../models/admin/operations/claims/claim-dashboard.model';


import {
  ClaimDashboardFilterRequest
} from '../models/admin/operations/claims/claim-dashboard-filter-request.model';


import {
  ClaimDetails
} from '../models/admin/operations/claims/claim-details.model';


import {
  ClaimPagedResult
} from '../models/admin/operations/claims/claim-paged-result.model';
import { ClaimStatus } from '../models/customer/claims/claim-status.enum';




const DEFAULT_POLICY_FILTER:
  PolicyFilterRequest = {
    status: null,
    productName: null,
    productType: null,
    pageNumber: 1,
    pageSize: 10
  };


const DEFAULT_CLAIM_FILTER:
  ClaimDashboardFilterRequest = {
    status: null,
    claimType: null,
    claimNumber: null,
    customerId: null,
    fromDate: null,
    toDate: null,
    pageNumber: 1,
    pageSize: 20
  };


@Injectable({
  providedIn: 'root'
})
export class AdminService {


    private readonly customersState =
  signal<
    PagedResult<
      AdminCustomerSummary
    >
  >(
    this.createEmptyPagedResult<
      AdminCustomerSummary
    >()
  );


  


private readonly customerQueryState =
  signal<AdminCustomerQuery>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdAtUtc',
    sortDirection: 'desc'
  });


private readonly customersLoadingState =
  signal(false);


private readonly selectedCustomerState =
  signal<
    AdminCustomerDetails | null
  >(null);


private readonly customerDetailsLoadingState =
  signal(false);


private readonly customerPropertiesState =
  signal<
    PagedResult<
      VacationProperty
    >
  >(
    this.createEmptyPagedResult<
      VacationProperty
    >()
  );


private readonly customerPropertyQueryState =
  signal<
    AdminCustomerPropertyQuery
  >({
    pageNumber: 1,
    pageSize: 10,
    includeArchived: true,
    sortBy: 'createdAtUtc',
    sortDirection: 'desc'
  });


private readonly customerPropertiesLoadingState =
  signal(false);
  private readonly adminApiService =
    inject(AdminApiService);


  private readonly notifications =
    inject(GlobalNotificationService);


  private readonly pendingClaimsManagersState =
    signal<PendingClaimsManagerResponse[]>([]);


  private readonly loadingState =
    signal(false);


  private readonly processingApplicationIdState =
    signal<string | null>(null);


  private readonly downloadingApplicationIdState =
    signal<string | null>(null);

  private readonly downloadingPolicyDocumentIdState =
    signal<string | null>(null);

  private readonly retryingPolicyDocumentIdState =
    signal<string | null>(null);


  private readonly updatingUserIdState =
    signal<string | null>(null);


  private readonly errorMessageState =
    signal<string | null>(null);


  readonly pendingClaimsManagers =
    this.pendingClaimsManagersState
      .asReadonly();


  readonly loading =
    this.loadingState
      .asReadonly();


  readonly processingApplicationId =
    this.processingApplicationIdState
      .asReadonly();


  readonly downloadingApplicationId =
    this.downloadingApplicationIdState
      .asReadonly();

  readonly downloadingPolicyDocumentId =
    this.downloadingPolicyDocumentIdState
      .asReadonly();

  readonly retryingPolicyDocumentId =
    this.retryingPolicyDocumentIdState
      .asReadonly();


  readonly updatingUserId =
    this.updatingUserIdState
      .asReadonly();


  readonly errorMessage =
    this.errorMessageState
      .asReadonly();


  readonly pendingApplicationCount =
    computed(
      () =>
        this.pendingClaimsManagersState()
          .length
    );


  readonly hasPendingApplications =
    computed(
      () =>
        this.pendingApplicationCount() > 0
    );


    readonly customers =
  this.customersState
    .asReadonly();


readonly customerQuery =
  this.customerQueryState
    .asReadonly();


readonly customersLoading =
  this.customersLoadingState
    .asReadonly();


readonly selectedCustomer =
  this.selectedCustomerState
    .asReadonly();


readonly customerDetailsLoading =
  this.customerDetailsLoadingState
    .asReadonly();


readonly customerProperties =
  this.customerPropertiesState
    .asReadonly();


readonly customerPropertyQuery =
  this.customerPropertyQueryState
    .asReadonly();


readonly customerPropertiesLoading =
  this.customerPropertiesLoadingState
    .asReadonly();


    private readonly productsState =
  signal<ProductDetails[]>(
    []
  );


private readonly productsLoadingState =
  signal(false);


private readonly selectedProductState =
  signal<ProductDetails | null>(
    null
  );


private readonly productDetailsLoadingState =
  signal(false);


private readonly processingProductIdState =
  signal<string | null>(
    null
  );


private readonly processingCoverageIdState =
  signal<string | null>(
    null
  );


  readonly products =
  this.productsState
    .asReadonly();


readonly productsLoading =
  this.productsLoadingState
    .asReadonly();


readonly selectedProduct =
  this.selectedProductState
    .asReadonly();


readonly productDetailsLoading =
  this.productDetailsLoadingState
    .asReadonly();


readonly processingProductId =
  this.processingProductIdState
    .asReadonly();


readonly processingCoverageId =
  this.processingCoverageIdState
    .asReadonly();


readonly productCount =
  computed(
    () =>
      this.productsState()
        .length
  );


readonly activeProductCount =
  computed(
    () =>
      this.productsState()
        .filter(
          product =>
            product.isActive
        )
        .length
  );


readonly inactiveProductCount =
  computed(
    () =>
      this.productsState()
        .filter(
          product =>
            !product.isActive
        )
        .length
  );


readonly coverageCount =
  computed(
    () =>
      this.productsState()
        .reduce(
          (
            total,
            product
          ) =>
            total +
            product.coverages.length,
          0
        )
  );


readonly customerCount =
  computed(
    () =>
      this.customersState()
        .totalCount
  );


readonly activeCustomerCount =
  computed(
    () =>
      this.customersState()
        .items
        .filter(
          customer =>
            customer.status ===
            CustomerStatus.Active
        )
        .length
  );


readonly inactiveCustomerCount =
  computed(
    () =>
      this.customersState()
        .items
        .filter(
          customer =>
            customer.status ===
            CustomerStatus.Inactive
        )
        .length
  );


  readonly actionLoading =
    computed(
      () =>
        this.processingApplicationIdState() !==
          null ||
        this.updatingUserIdState() !==
          null
    );


   private readonly quotesState =
  signal<
    PagedResult<QuoteSummary>
  >(
    this.createEmptyPagedResult<
      QuoteSummary
    >()
  );


private readonly quoteQueryState =
  signal<AdminQuoteSearchQuery>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'generatedAtUtc',
    sortDescending: true,
    status:
      QuoteStatus.PendingAdminApproval
  });


private readonly quotesLoadingState =
  signal(false);


private readonly selectedQuoteState =
  signal<QuoteDetails | null>(
    null
  );


private readonly quoteDetailsLoadingState =
  signal(false);


private readonly processingQuoteIdState =
  signal<string | null>(
    null
  );
  
  readonly quotes =
  this.quotesState
    .asReadonly();


readonly quoteQuery =
  this.quoteQueryState
    .asReadonly();


readonly quotesLoading =
  this.quotesLoadingState
    .asReadonly();


readonly selectedQuote =
  this.selectedQuoteState
    .asReadonly();


readonly quoteDetailsLoading =
  this.quoteDetailsLoadingState
    .asReadonly();


readonly processingQuoteId =
  this.processingQuoteIdState
    .asReadonly();


readonly quoteCount =
  computed(
    () =>
      this.quotesState()
        .totalCount
  );


readonly pendingQuoteCount =
  computed(
    () =>
      this.quotesState()
        .items
        .filter(
          quote =>
            quote.status ===
            QuoteStatus
              .PendingAdminApproval
        )
        .length
  );
  private readonly pricingState =
  signal<PricingConfiguration | null>(
    null
  );


private readonly pricingLoadingState =
  signal(false);


private readonly pricingUpdatingState =
  signal(false);


readonly pricing =
  this.pricingState
    .asReadonly();


readonly pricingLoading =
  this.pricingLoadingState
    .asReadonly();


readonly pricingUpdating =
  this.pricingUpdatingState
    .asReadonly();


readonly pricingConfigured =
  computed(
    () =>
      this.pricingState() !==
      null
  );
 loadPricing(): void {
  this.getPricing()
    .subscribe();
}
refreshPricing(): void {
  this.loadPricing();
}


private readonly policiesState =
  signal<PolicySummary[]>(
    []
  );


private readonly policiesLoadingState =
  signal(false);


private readonly policyResultState =
  signal<
    PolicyPagedResult<PolicySummary>
  >({
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0
  });


private readonly policyFilterState =
  signal<PolicyFilterRequest>({
    ...DEFAULT_POLICY_FILTER
  });


private readonly selectedPolicyState =
  signal<PolicyDetails | null>(
    null
  );


private readonly policyDetailsLoadingState =
  signal(false);


  private readonly claimsDashboardState =
  signal<ClaimDashboard | null>(
    null
  );


private readonly claimsDashboardLoadingState =
  signal(false);


private readonly claimsState =
  signal<Claim[]>(
    []
  );


private readonly claimsLoadingState =
  signal(false);


private readonly claimResultState =
  signal<
    ClaimPagedResult<Claim>
  >({
    items: [],
    pageNumber: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0
  });


private readonly claimFilterState =
  signal<
    ClaimDashboardFilterRequest
  >({
    ...DEFAULT_CLAIM_FILTER
  });


private readonly selectedClaimState =
  signal<ClaimDetails | null>(
    null
  );


private readonly claimDetailsLoadingState =
  signal(false);
readonly policies =
  this.policiesState
    .asReadonly();


readonly policiesLoading =
  this.policiesLoadingState
    .asReadonly();


readonly policyResult =
  this.policyResultState
    .asReadonly();


readonly policyFilter =
  this.policyFilterState
    .asReadonly();


readonly selectedPolicy =
  this.selectedPolicyState
    .asReadonly();


readonly policyDetailsLoading =
  this.policyDetailsLoadingState
    .asReadonly();


readonly policyTotalCount =
  computed(
    () =>
      this.policyResultState()
        .totalCount
  );


readonly policyPageNumber =
  computed(
    () =>
      this.policyResultState()
        .pageNumber
  );


readonly policyPageSize =
  computed(
    () =>
      this.policyResultState()
        .pageSize
  );


readonly policyTotalPages =
  computed(
    () => {
      const result =
        this.policyResultState();


      if (
        result.pageSize <= 0 ||
        result.totalCount <= 0
      ) {
        return 0;
      }


      return Math.ceil(
        result.totalCount /
        result.pageSize
      );
    }
  );


readonly policyHasPreviousPage =
  computed(
    () =>
      this.policyPageNumber() > 1
  );


readonly policyHasNextPage =
  computed(
    () =>
      this.policyPageNumber() <
      this.policyTotalPages()
  );
  readonly claimsDashboard =
  this.claimsDashboardState
    .asReadonly();


readonly claimsDashboardLoading =
  this.claimsDashboardLoadingState
    .asReadonly();


readonly claims =
  this.claimsState
    .asReadonly();


readonly claimsLoading =
  this.claimsLoadingState
    .asReadonly();


readonly claimResult =
  this.claimResultState
    .asReadonly();


readonly claimFilter =
  this.claimFilterState
    .asReadonly();


readonly selectedClaim =
  this.selectedClaimState
    .asReadonly();


readonly claimDetailsLoading =
  this.claimDetailsLoadingState
    .asReadonly();


readonly claimTotalCount =
  computed(
    () =>
      this.claimResultState()
        .totalCount
  );


readonly claimPageNumber =
  computed(
    () =>
      this.claimResultState()
        .pageNumber
  );


readonly claimPageSize =
  computed(
    () =>
      this.claimResultState()
        .pageSize
  );


readonly claimTotalPages =
  computed(
    () =>
      this.claimResultState()
        .totalPages
  );


readonly claimHasPreviousPage =
  computed(
    () =>
      this.claimPageNumber() > 1
  );


readonly claimHasNextPage =
  computed(
    () =>
      this.claimPageNumber() <
      this.claimTotalPages()
  );


readonly operationsLoading =
  computed(
    () =>
      this.policiesLoadingState() ||
      this.claimsDashboardLoadingState() ||
      this.claimsLoadingState()
  );


private readonly activeClaimsManagersState =
  signal<ActiveClaimsManager[]>(
    []
  );


private readonly activeClaimsManagersLoadingState =
  signal(false);


private readonly assignmentProcessingState =
  signal(false);


private readonly assignmentSuccessMessageState =
  signal<string | null>(
    null
  );


  readonly activeClaimsManagers =
  this.activeClaimsManagersState
    .asReadonly();


readonly activeClaimsManagersLoading =
  this.activeClaimsManagersLoadingState
    .asReadonly();


readonly assignmentProcessing =
  this.assignmentProcessingState
    .asReadonly();


readonly assignmentSuccessMessage =
  this.assignmentSuccessMessageState
    .asReadonly();




readonly selectedClaimIsAssigned =
  computed(
    () =>
      Boolean(
        this.selectedClaimState()
          ?.assignedClaimsManagerId
      )
  );
readonly canAssignSelectedClaim =
  computed(() => {
    const claim =
      this.selectedClaimState();


    if (!claim) {
      return false;
    }


    const blockedStatuses = [
      ClaimStatus.Approved,
      ClaimStatus.Rejected,
      ClaimStatus.Paid,
      ClaimStatus.Closed,
      ClaimStatus.Withdrawn
    ];


    return (
      !blockedStatuses.includes(
        claim.status
      ) &&
      !this.assignmentProcessingState()
    );
  });


loadActiveClaimsManagers(
  force = false
): void {
  if (
    this.activeClaimsManagersLoadingState()
  ) {
    return;
  }


  if (
    !force &&
    this.activeClaimsManagersState()
      .length > 0
  ) {
    return;
  }


  this.activeClaimsManagersLoadingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getActiveClaimsManagers()
    .pipe(
      finalize(() => {
        this.activeClaimsManagersLoadingState.set(
          false
        );
      })
    )
    .subscribe({
      next: claimsManagers => {
        const normalizedClaimsManagers =
          claimsManagers
            .map(
              claimsManager =>
                this.normalizeActiveClaimsManager(
                  claimsManager
                )
            )
            .sort(
              (
                first,
                second
              ) =>
                first.displayName
                  .localeCompare(
                    second.displayName,
                    undefined,
                    {
                      sensitivity:
                        'base'
                    }
                  )
            );


        this.activeClaimsManagersState.set(
          normalizedClaimsManagers
        );
      },


      error: (
        error: unknown
      ) => {
        this.activeClaimsManagersState.set(
          []
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Active Claims Managers could not be loaded.'
          )
        );
      }
    });
}


assignClaimsManager(
  claimId: string,
  claimsManagerId: string,
  reason: string | null
): void {
  if (
    this.assignmentProcessingState()
  ) {
    return;
  }


  const normalizedClaimId =
    claimId.trim();


  const normalizedClaimsManagerId =
    claimsManagerId.trim();


  const normalizedReason =
    reason?.trim() ||
    null;


  const selectedClaim =
    this.selectedClaimState();


  if (
    !normalizedClaimId ||
    !normalizedClaimsManagerId ||
    !selectedClaim ||
    selectedClaim.id !==
      normalizedClaimId
  ) {
    this.errorMessageState.set(
      'Valid Claim and Claims Manager details are required.'
    );


    return;
  }


  if (!selectedClaim.rowVersion) {
    this.errorMessageState.set(
      'Claim concurrency information is missing. Refresh the Claim and try again.'
    );


    return;
  }


  const isReassignment =
    Boolean(
      selectedClaim
        .assignedClaimsManagerId
    );


  if (
    isReassignment &&
    !normalizedReason
  ) {
    this.errorMessageState.set(
      'A reason is required when reassigning a Claim.'
    );


    return;
  }


  if (
    selectedClaim
      .assignedClaimsManagerId ===
    normalizedClaimsManagerId
  ) {
    this.errorMessageState.set(
      'This Claim is already assigned to the selected Claims Manager.'
    );


    return;
  }


  const request:
    AssignClaimsManagerRequest = {
      claimsManagerId:
        normalizedClaimsManagerId,


      reason:
        normalizedReason,


      rowVersion:
        selectedClaim.rowVersion
    };


  this.assignmentProcessingState.set(
    true
  );


  this.assignmentSuccessMessageState.set(
    null
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .assignClaimsManager(
      normalizedClaimId,
      request
    )
    .pipe(
      finalize(() => {
        this.assignmentProcessingState.set(
          false
        );
      })
    )
    .subscribe({
      next: claim => {
        const normalizedClaim =
          this.normalizeClaimDetails(
            claim
          );


        this.selectedClaimState.set(
          normalizedClaim
        );


        this.updateClaimInList(
          normalizedClaim
        );


        this.assignmentSuccessMessageState.set(
          isReassignment
            ? 'Claim was reassigned successfully.'
            : 'Claims Manager was assigned successfully.'
        );


        this.loadClaimsDashboard();


        this.loadActiveClaimsManagers(
          true
        );
      },


      error: (
        error: unknown
      ) => {
        this.assignmentSuccessMessageState.set(
          null
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            isReassignment
              ? 'Claim could not be reassigned.'
              : 'Claims Manager could not be assigned.'
          )
        );
      }
    });
}
clearAssignmentMessage(): void {
  this.assignmentSuccessMessageState.set(
    null
  );
}


clearSelectedClaim(): void {
  if (
    this.claimDetailsLoadingState() ||
    this.assignmentProcessingState()
  ) {
    return;
  }


  this.selectedClaimState.set(
    null
  );


  this.assignmentSuccessMessageState.set(
    null
  );
}


loadPolicies(
  request?: PolicyFilterRequest
): void {
  if (
    this.policiesLoadingState()
  ) {
    return;
  }


  const effectiveRequest =
    this.normalizePolicyFilter(
      request ??
      this.policyFilterState()
    );


  this.policyFilterState.set(
    effectiveRequest
  );


  this.policiesLoadingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getPolicies(
      effectiveRequest
    )
    .pipe(
      finalize(() => {
        this.policiesLoadingState.set(
          false
        );
      })
    )
    .subscribe({
      next: result => {
        const normalizedResult:
          PolicyPagedResult<PolicySummary> = {
            ...result,
            items: [
              ...result.items
            ]
          };


        this.policyResultState.set(
          normalizedResult
        );


        this.policiesState.set(
          normalizedResult.items
        );
      },


      error: (
        error: unknown
      ) => {
        this.policiesState.set(
          []
        );


        this.policyResultState.set({
          items: [],
          pageNumber:
            effectiveRequest.pageNumber,
          pageSize:
            effectiveRequest.pageSize,
          totalCount: 0
        });


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Policies could not be loaded.'
          )
        );
      }
    });
}
refreshPolicies(): void {
  this.loadPolicies(
    this.policyFilterState()
  );
}
loadPolicy(
  policyId: string
): void {
  const normalizedPolicyId =
    policyId.trim();


  if (
    !normalizedPolicyId ||
    this.policyDetailsLoadingState()
  ) {
    return;
  }


  this.policyDetailsLoadingState.set(
    true
  );


  this.selectedPolicyState.set(
    null
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getPolicy(
      normalizedPolicyId
    )
    .pipe(
      finalize(() => {
        this.policyDetailsLoadingState.set(
          false
        );
      })
    )
    .subscribe({
      next: policy => {
        this.selectedPolicyState.set({
          ...policy,
          coverages: [
            ...policy.coverages
          ].sort(
            (
              first,
              second
            ) =>
              first.coverageName
                .localeCompare(
                  second.coverageName
                )
          )
        });
      },


      error: (
        error: unknown
      ) => {
        this.selectedPolicyState.set(
          null
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Policy details could not be loaded.'
          )
        );
      }
    });
}

downloadPolicyDocument(
    policyId: string,
    policyDocument: PolicyDocument
): void {
    const normalizedPolicyId = policyId.trim();

    if (
      !normalizedPolicyId ||
      !policyDocument.id ||
      policyDocument.status !== 'Available' ||
      this.downloadingPolicyDocumentIdState()
    ) {
      return;
    }

    this.downloadingPolicyDocumentIdState.set(
      policyDocument.id
    );
    this.errorMessageState.set(null);

    this.adminApiService
      .getPolicyDocument(
        normalizedPolicyId,
        policyDocument.id
      )
      .pipe(
        map(response =>
          this.createPolicyDownloadedFile(
            response,
            normalizedPolicyId,
            policyDocument.version
          )
        ),
        catchError(error => {
          this.errorMessageState.set(
            this.getErrorMessage(
              error,
              'The policy document could not be downloaded.'
            )
          );
          return throwError(() => error);
        }),
        finalize(() => {
          this.downloadingPolicyDocumentIdState.set(null);
        })
      )
      .subscribe({
        next: file => this.saveDownloadedFile(file)
      });
}

retryPolicyDocumentGeneration(
    policyId: string
): void {
    const normalizedPolicyId = policyId.trim();

    if (
      !normalizedPolicyId ||
      this.retryingPolicyDocumentIdState()
    ) {
      return;
    }

    this.retryingPolicyDocumentIdState.set(
      normalizedPolicyId
    );
    this.errorMessageState.set(null);

    this.adminApiService
      .retryPolicyDocument(normalizedPolicyId)
      .pipe(
        finalize(() => {
          this.retryingPolicyDocumentIdState.set(null);
        })
      )
      .subscribe({
        next: () => this.loadPolicy(normalizedPolicyId),
        error: error => {
          this.errorMessageState.set(
            this.getErrorMessage(
              error,
              'Policy document generation could not be retried.'
            )
          );
        }
      });
}


clearSelectedPolicy(): void {
  if (
    this.policyDetailsLoadingState()
  ) {
    return;
  }


  this.selectedPolicyState.set(
    null
  );
}
goToPolicyPage(
  pageNumber: number
): void {
  const totalPages =
    this.policyTotalPages();


  if (
    pageNumber < 1 ||
    (
      totalPages > 0 &&
      pageNumber > totalPages
    )
  ) {
    return;
  }


  this.loadPolicies({
    ...this.policyFilterState(),
    pageNumber
  });
}


goToPreviousPolicyPage(): void {
  if (
    !this.policyHasPreviousPage()
  ) {
    return;
  }


  this.goToPolicyPage(
    this.policyPageNumber() - 1
  );
}


goToNextPolicyPage(): void {
  if (
    !this.policyHasNextPage()
  ) {
    return;
  }


  this.goToPolicyPage(
    this.policyPageNumber() + 1
  );
}


loadClaimsDashboard(): void {
  if (
    this.claimsDashboardLoadingState()
  ) {
    return;
  }


  this.claimsDashboardLoadingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getClaimsDashboard()
    .pipe(
      finalize(() => {
        this.claimsDashboardLoadingState.set(
          false
        );
      })
    )
    .subscribe({
      next: dashboard => {
        this.claimsDashboardState.set({
          ...dashboard,
          recentClaims: [
            ...dashboard.recentClaims
          ]
        });
      },


      error: (
        error: unknown
      ) => {
        this.claimsDashboardState.set(
          null
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Claim dashboard statistics could not be loaded.'
          )
        );
      }
    });
}
refreshClaimsDashboard(): void {
  this.loadClaimsDashboard();
}
loadClaims(
  request?:
    ClaimDashboardFilterRequest
): void {
  if (
    this.claimsLoadingState()
  ) {
    return;
  }


  const effectiveRequest =
    this.normalizeClaimFilter(
      request ??
      this.claimFilterState()
    );


  this.claimFilterState.set(
    effectiveRequest
  );


  this.claimsLoadingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getClaims(
      effectiveRequest
    )
    .pipe(
      finalize(() => {
        this.claimsLoadingState.set(
          false
        );
      })
    )
    .subscribe({
      next: result => {
        const normalizedResult:
          ClaimPagedResult<Claim> = {
            ...result,
            items: result.items.map(
              claim =>
                this.normalizeClaim(
                  claim
                )
            )
          };


        this.claimResultState.set(
          normalizedResult
        );


        this.claimsState.set(
          normalizedResult.items
        );
      },


      error: (
        error: unknown
      ) => {
        this.claimsState.set(
          []
        );


        this.claimResultState.set({
          items: [],
          pageNumber:
            effectiveRequest.pageNumber,
          pageSize:
            effectiveRequest.pageSize,
          totalCount: 0,
          totalPages: 0
        });


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Claims could not be loaded.'
          )
        );
      }
    });
}
refreshClaims(): void {
  this.loadClaims(
    this.claimFilterState()
  );
}
loadClaim(
  claimId: string
): void {
  const normalizedClaimId =
    claimId.trim();


  if (
    !normalizedClaimId ||
    this.claimDetailsLoadingState()
  ) {
    return;
  }


  this.claimDetailsLoadingState.set(
    true
  );


  this.selectedClaimState.set(
    null
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getClaim(
      normalizedClaimId
    )
    .pipe(
      finalize(() => {
        this.claimDetailsLoadingState.set(
          false
        );
      })
    )
    .subscribe({
      next: claim => {
        this.assignmentSuccessMessageState.set(
          null
        );


        this.selectedClaimState.set(
          this.normalizeClaimDetails(
            claim
          )
        );
      },


      error: (
        error: unknown
      ) => {
        this.selectedClaimState.set(
          null
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Claim details could not be loaded.'
          )
        );
      }
    });
}


goToClaimPage(
  pageNumber: number
): void {
  const totalPages =
    this.claimTotalPages();


  if (
    pageNumber < 1 ||
    (
      totalPages > 0 &&
      pageNumber > totalPages
    )
  ) {
    return;
  }


  this.loadClaims({
    ...this.claimFilterState(),
    pageNumber
  });
}


goToPreviousClaimPage(): void {
  if (
    !this.claimHasPreviousPage()
  ) {
    return;
  }


  this.goToClaimPage(
    this.claimPageNumber() - 1
  );
}


goToNextClaimPage(): void {
  if (
    !this.claimHasNextPage()
  ) {
    return;
  }


  this.goToClaimPage(
    this.claimPageNumber() + 1
  );
}
loadOperations(): void {
  this.loadPolicies();
  this.loadClaimsDashboard();
  this.loadClaims();
}


refreshOperations(): void {
  this.refreshPolicies();
  this.refreshClaimsDashboard();
  this.refreshClaims();
}
private normalizePolicyFilter(
  request: PolicyFilterRequest
): PolicyFilterRequest {
  return {
    status:
      request.status ??
      null,


    productName:
      request.productName
        ?.trim() ||
      null,


    productType:
      request.productType ??
      null,


    pageNumber:
      Math.max(
        1,
        request.pageNumber
      ),


    pageSize:
      this.normalizePageSize(
        request.pageSize,
        10
      )
  };
}


private normalizeClaimFilter(
  request:
    ClaimDashboardFilterRequest
): ClaimDashboardFilterRequest {
  return {
    status:
      request.status ??
      null,


    claimType:
      request.claimType ??
      null,


    claimNumber:
      request.claimNumber
        ?.trim() ||
      null,


    customerId:
      request.customerId
        ?.trim() ||
      null,


    fromDate:
      request.fromDate
        ?.trim() ||
      null,


    toDate:
      request.toDate
        ?.trim() ||
      null,


    pageNumber:
      Math.max(
        1,
        request.pageNumber
      ),


    pageSize:
      this.normalizePageSize(
        request.pageSize,
        20
      )
  };
}


private normalizePageSize(
  pageSize: number,
  fallback: number
): number {
  if (
    !Number.isInteger(
      pageSize
    ) ||
    pageSize < 1 ||
    pageSize > 100
  ) {
    return fallback;
  }


  return pageSize;
}


private normalizeClaim(
  claim: Claim
): Claim {
  return {
    ...claim,


    coverages: [
      ...claim.coverages
    ].sort(
      (
        first,
        second
      ) =>
        first.coverageName
          .localeCompare(
            second.coverageName
          )
    )
  };
}
private normalizeClaimDetails(
  claim: ClaimDetails
): ClaimDetails {
  return {
    ...claim,


    assignedClaimsManagerId:
      claim.assignedClaimsManagerId
        ?.trim() ||
      null,


    assignedClaimsManagerName:
      claim.assignedClaimsManagerName
        ?.trim() ||
      null,


    assignedClaimsManagerEmail:
      claim.assignedClaimsManagerEmail
        ?.trim() ||
      null,


    assignedAtUtc:
      claim.assignedAtUtc ||
      null,


    coverages: [
      ...(claim.coverages ?? [])
    ].sort(
      (
        first,
        second
      ) =>
        first.coverageName
          .localeCompare(
            second.coverageName
          )
    ),


    documents: [
      ...(claim.documents ?? [])
    ].sort(
      (
        first,
        second
      ) =>
        new Date(
          second.uploadedAtUtc
        ).getTime() -
        new Date(
          first.uploadedAtUtc
        ).getTime()
    ),


    timeline: [
      ...(claim.timeline ?? [])
    ].sort(
      (
        first,
        second
      ) =>
        new Date(
          second.changedAtUtc
        ).getTime() -
        new Date(
          first.changedAtUtc
        ).getTime()
    ),


    assignmentHistory: [
      ...(claim.assignmentHistory ?? [])
    ].sort(
      (
        first,
        second
      ) =>
        new Date(
          second.assignedAtUtc
        ).getTime() -
        new Date(
          first.assignedAtUtc
        ).getTime()
    )
  };
}
private normalizeActiveClaimsManager(
  claimsManager:
    ActiveClaimsManager
): ActiveClaimsManager {
  const firstName =
    claimsManager.firstName
      ?.trim() ||
    '';


  const lastName =
    claimsManager.lastName
      ?.trim() ||
    '';


  const generatedDisplayName =
    [
      firstName,
      lastName
    ]
      .filter(
        value => Boolean(value)
      )
      .join(' ');


  return {
    ...claimsManager,


    userId:
      claimsManager.userId
        .trim(),


    firstName,


    lastName,


    email:
      claimsManager.email
        .trim(),


    professionalSummary:
      claimsManager
        .professionalSummary
        ?.trim() ||
      '',


    displayName:
      claimsManager.displayName
        ?.trim() ||
      generatedDisplayName ||
      claimsManager.email
        .trim()
  };
}
private updateClaimInList(
  claim: ClaimDetails
): void {
  this.claimsState.update(
    claims =>
      claims.map(
        existingClaim =>
          existingClaim.id ===
          claim.id
            ? {
                ...existingClaim,


                assignedClaimsManagerId:
                  claim
                    .assignedClaimsManagerId,


                assignedClaimsManagerName:
                  claim
                    .assignedClaimsManagerName,


                assignedClaimsManagerEmail:
                  claim
                    .assignedClaimsManagerEmail,


                assignedAtUtc:
                  claim.assignedAtUtc,


                rowVersion:
                  claim.rowVersion
              }
            : existingClaim
      )
  );


  this.claimResultState.update(
    result => ({
      ...result,


      items:
        result.items.map(
          existingClaim =>
            existingClaim.id ===
            claim.id
              ? {
                  ...existingClaim,


                  assignedClaimsManagerId:
                    claim
                      .assignedClaimsManagerId,


                  assignedClaimsManagerName:
                    claim
                      .assignedClaimsManagerName,


                  assignedClaimsManagerEmail:
                    claim
                      .assignedClaimsManagerEmail,


                  assignedAtUtc:
                    claim.assignedAtUtc,


                  rowVersion:
                    claim.rowVersion
                }
              : existingClaim
        )
    })
  );
}


updatePricing(
  baseRate: number,
  lowRiskMultiplier: number,
  mediumRiskMultiplier: number,
  highRiskMultiplier: number
): Observable<PricingConfiguration> {
  const pricing =
    this.pricingState();


  if (!pricing) {
    return throwError(
      () =>
        new Error(
          'Pricing configuration must be loaded before it can be updated.'
        )
    );
  }


  if (
    this.pricingUpdatingState()
  ) {
    return throwError(
      () =>
        new Error(
          'A Pricing update is already in progress.'
        )
    );
  }


  if (
    !Number.isFinite(
      baseRate
    ) ||
    baseRate < 0.0001 ||
    baseRate > 0.02
  ) {
    return throwError(
      () =>
        new Error(
          'Base rate must be between 0.0001 and 0.02 of the insured property value.'
        )
    );
  }


  if (
    !Number.isFinite(
      lowRiskMultiplier
    ) ||
    lowRiskMultiplier < 0.5 ||
    lowRiskMultiplier > 3
  ) {
    return throwError(
      () =>
        new Error(
          'Low-risk multiplier must be between 0.5 and 3.'
        )
    );
  }


  if (
    !Number.isFinite(
      mediumRiskMultiplier
    ) ||
    mediumRiskMultiplier < 0.5 ||
    mediumRiskMultiplier > 3
  ) {
    return throwError(
      () =>
        new Error(
          'Medium-risk multiplier must be between 0.5 and 3.'
        )
    );
  }


  if (
    !Number.isFinite(
      highRiskMultiplier
    ) ||
    highRiskMultiplier < 0.5 ||
    highRiskMultiplier > 3
  ) {
    return throwError(
      () =>
        new Error(
          'High-risk multiplier must be between 0.5 and 3.'
        )
    );
  }

  if (
    lowRiskMultiplier > mediumRiskMultiplier ||
    mediumRiskMultiplier > highRiskMultiplier
  ) {
    return throwError(
      () =>
        new Error(
          'Risk multipliers must increase from low to high risk.'
        )
    );
  }

  if (
    !pricing.rowVersion.trim()
  ) {
    return throwError(
      () =>
        new Error(
          'Pricing version information is missing. Refresh the configuration and try again.'
        )
    );
  }


  if (
    pricing.baseRate ===
      baseRate &&
    pricing.lowRiskMultiplier ===
      lowRiskMultiplier &&
    pricing.mediumRiskMultiplier ===
      mediumRiskMultiplier &&
    pricing.highRiskMultiplier ===
      highRiskMultiplier
  ) {
    return throwError(
      () =>
        new Error(
          'No Pricing values were changed.'
        )
    );
  }


  this.pricingUpdatingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  return this.adminApiService
    .updatePricing({
      baseRate,
      lowRiskMultiplier,
      mediumRiskMultiplier,
      highRiskMultiplier,
      rowVersion:
        pricing.rowVersion
    })
    .pipe(
      /*
       * PUT returns 204 No Content. Retrieve the Pricing
       * configuration again to obtain its new RowVersion.
       */
      switchMap(
        (): Observable<PricingConfiguration> =>
          this.adminApiService
            .getPricing()
      ),


      tap(
        (
          updatedPricing:
            PricingConfiguration
        ) => {
          this.pricingState.set(
            updatedPricing
          );
        }
      ),


      catchError(
        (
          error: unknown
        ): Observable<never> => {
          this.errorMessageState.set(
            this.getErrorMessage(
              error,
              'Pricing configuration could not be updated.'
            )
          );


          return throwError(
            () => error
          );
        }
      ),


      finalize(() => {
        this.pricingUpdatingState.set(
          false
        );
      })
    );
}
getPricing():
  Observable<PricingConfiguration> {
  if (
    this.pricingLoadingState()
  ) {
    const existingPricing =
      this.pricingState();


    if (existingPricing) {
      return new Observable<
        PricingConfiguration
      >(subscriber => {
        subscriber.next(
          existingPricing
        );


        subscriber.complete();
      });
    }
  }


  this.pricingLoadingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  return this.adminApiService
    .getPricing()
    .pipe(
      tap(pricing => {
        this.pricingState.set(
          pricing
        );
      }),


      catchError(
        (
          error: unknown
        ): Observable<never> => {
          this.pricingState.set(
            null
          );


          this.errorMessageState.set(
            this.getErrorMessage(
              error,
              'Pricing configuration could not be loaded.'
            )
          );


          return throwError(
            () => error
          );
        }
      ),


      finalize(() => {
        this.pricingLoadingState.set(
          false
        );
      })
    );
}


  loadProducts(): void {
  if (
    this.productsLoadingState()
  ) {
    return;
  }


  this.productsLoadingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getProducts()
    .pipe(
      finalize(() => {
        this.productsLoadingState
          .set(false);
      })
    )
    .subscribe({
      next: products => {
        this.productsState.set(
          this.sortProducts(
            products
          )
        );
      },


      error: error => {
        this.productsState.set(
          []
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Products and Coverages could not be loaded.'
          )
        );
      }
    });
}
refreshProducts(): void {
  this.loadProducts();
}
loadProduct(
  productId: string
): void {
  const normalizedProductId =
    productId.trim();


  if (
    !normalizedProductId ||
    this.productDetailsLoadingState()
  ) {
    return;
  }


  this.productDetailsLoadingState.set(
    true
  );


  this.selectedProductState.set(
    null
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getProduct(
      normalizedProductId
    )
    .pipe(
      finalize(() => {
        this.productDetailsLoadingState
          .set(false);
      })
    )
    .subscribe({
      next: product => {
        const normalizedProduct =
          this.normalizeProduct(
            product
          );


        this.selectedProductState.set(
          normalizedProduct
        );


        this.replaceProductInList(
          normalizedProduct
        );
      },


      error: error => {
        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Product details could not be loaded.'
          )
        );
      }
    });
}


changeProductStatus(
  product: ProductDetails,
  isActive: boolean
): Observable<ProductDetails> {
  if (
    this.processingProductIdState()
  ) {
    return throwError(
      () =>
        new Error(
          'Another Product action is already in progress.'
        )
    );
  }


  if (
    product.isActive ===
    isActive
  ) {
    return throwError(
      () =>
        new Error(
          isActive
            ? 'The Product is already active.'
            : 'The Product is already inactive.'
        )
    );
  }


  if (
    !product.rowVersion.trim()
  ) {
    return throwError(
      () =>
        new Error(
          'Product version information is missing. Refresh the Product and try again.'
        )
    );
  }


  this.processingProductIdState.set(
    product.id
  );


  this.errorMessageState.set(
    null
  );


  return this.adminApiService
    .updateProductStatus(
      product.id,
      {
        isActive,
        rowVersion:
          product.rowVersion
      }
    )
    .pipe(
      /*
       * The PUT response is 204 and therefore does not contain
       * the new database-generated RowVersion. Reload the Product
       * immediately before permitting another update.
       */
      switchMap(() =>
        this.adminApiService
          .getProduct(
            product.id
          )
      ),


      tap(updatedProduct => {
        const normalizedProduct =
          this.normalizeProduct(
            updatedProduct
          );


        this.selectedProductState.set(
          normalizedProduct
        );


        this.replaceProductInList(
          normalizedProduct
        );
      }),


      catchError(error => {
        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            isActive
              ? 'The Product could not be activated.'
              : 'The Product could not be deactivated.'
          )
        );


        return throwError(
          () => error
        );
      }),


      finalize(() => {
        this.processingProductIdState
          .set(null);
      })
    );
}


changeCoverageLimit(
  product: ProductDetails,
  coverage: ProductCoverage,
  coverageLimitPercentage: number
): Observable<ProductDetails> {
  if (
    this.processingCoverageIdState()
  ) {
    return throwError(
      () =>
        new Error(
          'Another Coverage action is already in progress.'
        )
    );
  }


  if (
    !Number.isFinite(
      coverageLimitPercentage
    ) ||
    coverageLimitPercentage <= 0 ||
    coverageLimitPercentage > 100
  ) {
    return throwError(
      () =>
        new Error(
          'Coverage-limit percentage must be greater than zero and cannot exceed 100.'
        )
    );
  }


  if (
    !coverage.rowVersion.trim()
  ) {
    return throwError(
      () =>
        new Error(
          'Coverage version information is missing. Refresh the Product and try again.'
        )
    );
  }


  if (
    coverage.coverageLimitPercentage ===
    coverageLimitPercentage
  ) {
    return throwError(
      () =>
        new Error(
          'Enter a different Coverage-limit percentage.'
        )
    );
  }


  this.processingCoverageIdState.set(
    coverage.coverageId
  );


  this.errorMessageState.set(
    null
  );


  return this.adminApiService
    .updateCoverageLimit(
      product.id,
      coverage.coverageId,
      {
        coverageLimitPercentage,
        rowVersion:
          coverage.rowVersion
      }
    )
    .pipe(
      /*
       * Reload the Product because the Coverage-limit endpoint
       * returns 204 and the mapping receives a new RowVersion.
       */
      switchMap(() =>
        this.adminApiService
          .getProduct(
            product.id
          )
      ),


      tap(updatedProduct => {
        const normalizedProduct =
          this.normalizeProduct(
            updatedProduct
          );


        this.selectedProductState.set(
          normalizedProduct
        );


        this.replaceProductInList(
          normalizedProduct
        );
      }),


      catchError(error => {
        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'The Coverage limit could not be updated.'
          )
        );


        return throwError(
          () => error
        );
      }),


      finalize(() => {
        this.processingCoverageIdState
          .set(null);
      })
    );
}
changeCoverageCharge(
  product: ProductDetails,
  coverage: ProductCoverage,
  coverageCharge: number
): Observable<ProductDetails> {
  if (
    this.processingCoverageIdState()
  ) {
    return throwError(
      () =>
        new Error(
          'Another Coverage action is already in progress.'
        )
    );
  }


  if (
    !Number.isFinite(
      coverageCharge
    ) ||
    coverageCharge < 0
  ) {
    return throwError(
      () =>
        new Error(
          'Coverage charge cannot be negative.'
        )
    );
  }


  if (
    !coverage.rowVersion.trim()
  ) {
    return throwError(
      () =>
        new Error(
          'Coverage version information is missing. Refresh the Product and try again.'
        )
    );
  }


  if (
    coverage.coverageCharge ===
    coverageCharge
  ) {
    return throwError(
      () =>
        new Error(
          'Enter a different Coverage Charge.'
        )
    );
  }


  this.processingCoverageIdState.set(
    coverage.coverageId
  );


  this.errorMessageState.set(
    null
  );


  return this.adminApiService
    .updateCoverageCharge(
      product.id,
      coverage.coverageId,
      {
        coverageCharge,
        rowVersion:
          coverage.rowVersion
      }
    )
    .pipe(
      switchMap(() =>
        this.adminApiService
          .getProduct(
            product.id
          )
      ),


      tap(updatedProduct => {
        const normalizedProduct =
          this.normalizeProduct(
            updatedProduct
          );


        this.selectedProductState.set(
          normalizedProduct
        );


        this.replaceProductInList(
          normalizedProduct
        );
      }),


      catchError(error => {
        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'The Coverage charge could not be updated.'
          )
        );


        return throwError(
          () => error
        );
      }),


      finalize(() => {
        this.processingCoverageIdState
          .set(null);
      })
    );
}


clearSelectedProduct(): void {
  if (
    this.processingProductIdState() ||
    this.processingCoverageIdState()
  ) {
    return;
  }


  this.selectedProductState.set(
    null
  );
}
private replaceProductInList(
  product: ProductDetails
): void {
  this.productsState.update(
    products => {
      const productExists =
        products.some(
          item =>
            item.id ===
            product.id
        );


      const updatedProducts =
        productExists
          ? products.map(
              item =>
                item.id ===
                product.id
                  ? product
                  : item
            )
          : [
              ...products,
              product
            ];


      return this.sortProducts(
        updatedProducts
      );
    }
  );
}


private normalizeProduct(
  product: ProductDetails
): ProductDetails {
  return {
    ...product,


    coverages:
      [
        ...product.coverages
      ].sort(
        (
          first,
          second
        ) =>
          first.displayOrder -
            second.displayOrder ||
          first.coverageName
            .localeCompare(
              second.coverageName
            )
      )
  };
}


private sortProducts(
  products: ProductDetails[]
): ProductDetails[] {
  return [
    ...products
  ]
    .map(
      product =>
        this.normalizeProduct(
          product
        )
    )
    .sort(
      (
        first,
        second
      ) => {
        if (
          first.isActive !==
          second.isActive
        ) {
          return first.isActive
            ? -1
            : 1;
        }


        return first.name
          .localeCompare(
            second.name
          );
      }
    );
}


  loadQuotes(
  query?: Partial<
    AdminQuoteSearchQuery
  >
): void {
  if (this.quotesLoadingState()) {
    return;
  }


  const nextQuery:
    AdminQuoteSearchQuery = {
    ...this.quoteQueryState(),
    ...query
  };


  this.quoteQueryState.set(
    nextQuery
  );


  this.quotesLoadingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getQuotes(
      nextQuery
    )
    .pipe(
      finalize(() => {
        this.quotesLoadingState
          .set(false);
      })
    )
    .subscribe({
      next: response => {
        this.quotesState.set(
          response
        );
      },


      error: error => {
        this.quotesState.set(
          this.createEmptyPagedResult<
            QuoteSummary
          >(
            nextQuery.pageNumber,
            nextQuery.pageSize
          )
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Quotes could not be loaded.'
          )
        );
      }
    });
}


refreshQuotes(): void {
  this.loadQuotes(
    this.quoteQueryState()
  );
}


loadQuote(
  quoteId: string
): void {
  const normalizedQuoteId =
    quoteId.trim();


  if (
    !normalizedQuoteId ||
    this.quoteDetailsLoadingState()
  ) {
    return;
  }


  this.quoteDetailsLoadingState.set(
    true
  );


  this.selectedQuoteState.set(
    null
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getQuote(
      normalizedQuoteId
    )
    .pipe(
      finalize(() => {
        this.quoteDetailsLoadingState
          .set(false);
      })
    )
    .subscribe({
      next: quote => {
        this.selectedQuoteState.set(
          quote
        );
      },


      error: error => {
        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Quote details could not be loaded.'
          )
        );
      }
    });
}


approveQuote(
  remarks: string,
  revisedAnnualPremium: number | null = null,
  priceAdjustmentReason: string | null = null
): Observable<QuoteDetails> {
  const quote =
    this.selectedQuoteState();


  if (!quote) {
    return throwError(
      () =>
        new Error(
          'Quote details are required.'
        )
    );
  }


  if (
    quote.status !==
    QuoteStatus.PendingAdminApproval
  ) {
    return throwError(
      () =>
        new Error(
          'Only a pending Quote can be approved.'
        )
    );
  }


  const normalizedRemarks =
    remarks.trim();


  if (
    normalizedRemarks.length > 1000
  ) {
    return throwError(
      () =>
        new Error(
          'Approval remarks cannot exceed 1000 characters.'
        )
    );
  }

  const normalizedPriceAdjustmentReason =
    priceAdjustmentReason?.trim() || null;
  const priceChanged =
    revisedAnnualPremium !== null &&
    revisedAnnualPremium !== quote.finalAnnualPremium;

  if (
    revisedAnnualPremium !== null &&
    (!Number.isFinite(revisedAnnualPremium) ||
      revisedAnnualPremium <= 0 ||
      revisedAnnualPremium > 9999999999999999 ||
      Math.round(revisedAnnualPremium * 100) / 100 !==
        revisedAnnualPremium)
  ) {
    return throwError(
      () =>
        new Error(
          'Revised annual premium must be positive and have no more than two decimal places.'
        )
    );
  }

  if (
    priceChanged &&
    (!normalizedPriceAdjustmentReason ||
      normalizedPriceAdjustmentReason.length < 5 ||
      normalizedPriceAdjustmentReason.length > 500)
  ) {
    return throwError(
      () =>
        new Error(
          'A price adjustment reason of 5 to 500 characters is required.'
        )
    );
  }


  this.processingQuoteIdState.set(
    quote.id
  );


  this.errorMessageState.set(
    null
  );


  return this.adminApiService
    .approveQuote(
      quote.id,
      {
        remarks:
          normalizedRemarks ||
          null,
        revisedAnnualPremium:
          revisedAnnualPremium,
        priceAdjustmentReason:
          normalizedPriceAdjustmentReason,
        rowVersion:
          quote.rowVersion
      }
    )
    .pipe(
      tap(updatedQuote => {
        this.applyQuoteDecision(
          updatedQuote
        );
      }),


      catchError(error => {
        this.clearError();
        this.notifications.showError(
          this.getErrorMessage(
            error,
            'The Quote could not be approved.'
          )
        );


        return throwError(
          () => error
        );
      }),


      finalize(() => {
        this.processingQuoteIdState
          .set(null);
      })
    );
}






rejectQuote(
  reason: string
): Observable<QuoteDetails> {
  const quote =
    this.selectedQuoteState();


  if (!quote) {
    return throwError(
      () =>
        new Error(
          'Quote details are required.'
        )
    );
  }


  if (
    quote.status !==
    QuoteStatus.PendingAdminApproval
  ) {
    return throwError(
      () =>
        new Error(
          'Only a pending Quote can be rejected.'
        )
    );
  }


  const normalizedReason =
    reason.trim();


  if (
    normalizedReason.length < 20 ||
    normalizedReason.length > 1000
  ) {
    return throwError(
      () =>
        new Error(
          'Rejection reason must contain between 20 and 1000 characters.'
        )
    );
  }


  if (
    !quote.rowVersion.trim()
  ) {
    return throwError(
      () =>
        new Error(
          'Quote version information is required. Refresh the Quote and try again.'
        )
    );
  }


  this.processingQuoteIdState.set(
    quote.id
  );


  this.errorMessageState.set(
    null
  );


  return this.adminApiService
    .rejectQuote(
      quote.id,
      {
        reason:
          normalizedReason,


        rowVersion:
          quote.rowVersion
      }
    )
    .pipe(
      tap(updatedQuote => {
        this.applyQuoteDecision(
          updatedQuote
        );
      }),


      catchError(error => {
        this.clearError();
        this.notifications.showError(
          this.getErrorMessage(
            error,
            'The Quote could not be rejected.'
          )
        );


        return throwError(
          () => error
        );
      }),


      finalize(() => {
        this.processingQuoteIdState
          .set(
            null
          );
      })
    );
}


clearSelectedQuote(): void {
  this.selectedQuoteState.set(
    null
  );
}
private applyQuoteDecision(
  updatedQuote: QuoteDetails
): void {
  this.selectedQuoteState.set(
    updatedQuote
  );


  const currentQuery =
    this.quoteQueryState();


  if (
    currentQuery.status ===
      QuoteStatus
        .PendingAdminApproval &&
    updatedQuote.status !==
      QuoteStatus
        .PendingAdminApproval
  ) {
    this.quotesState.update(
      page => {
        const items =
          page.items.filter(
            quote =>
              quote.id !==
              updatedQuote.id
          );


        const totalCount =
          Math.max(
            0,
            page.totalCount - 1
          );


        const totalPages =
          page.pageSize <= 0
            ? 0
            : Math.ceil(
                totalCount /
                page.pageSize
              );


        return {
          ...page,
          items,
          totalCount,
          totalPages,
          hasNextPage:
            page.pageNumber <
            totalPages
        };
      }
    );


    return;
  }


  this.quotesState.update(
    page => ({
      ...page,
      items:
        page.items.map(
          quote =>
            quote.id ===
            updatedQuote.id
              ? {
                  ...quote,
                  status:
                    updatedQuote.status,
                  finalAnnualPremium:
                    updatedQuote.finalAnnualPremium,
                  adminDecision:
                    updatedQuote.adminDecision,
                  paymentDeadlineUtc:
                    updatedQuote
                      .paymentDeadlineUtc
                }
              : quote
        )
    })
  );
}


  loadPendingClaimsManagers():
    void {
    if (this.loadingState()) {
      return;
    }


    this.loadingState.set(true);
    this.errorMessageState.set(
      null
    );


    this.adminApiService
      .getPendingClaimsManagers()
      .pipe(
        finalize(() => {
          this.loadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: applications => {
          this.pendingClaimsManagersState
            .set(
              this.sortApplications(
                applications
              )
            );
        },


        error: error => {
          this.pendingClaimsManagersState
            .set([]);


          this.errorMessageState.set(
            this.getErrorMessage(
              error,
              'Pending Claims Manager applications could not be loaded.'
            )
          );
        }
      });
  }


  loadCustomers(
  query?: Partial<
    AdminCustomerQuery
  >
): void {
  if (
    this.customersLoadingState()
  ) {
    return;
  }


  const nextQuery: AdminCustomerQuery = {
    ...this.customerQueryState(),
    ...query
  };


  this.customerQueryState.set(
    nextQuery
  );


  this.customersLoadingState.set(
    true
  );


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getCustomers(
      nextQuery
    )
    .pipe(
      finalize(() => {
        this.customersLoadingState
          .set(false);
      })
    )
    .subscribe({
      next: response => {
        this.customersState.set(
          response
        );
      },


      error: error => {
        this.customersState.set(
          this.createEmptyPagedResult<
            AdminCustomerSummary
          >(
            nextQuery.pageNumber,
            nextQuery.pageSize
          )
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Customers could not be loaded.'
          )
        );
      }
    });
}


refreshCustomers(): void {
  this.loadCustomers(
    this.customerQueryState()
  );
}


loadCustomer(
  customerId: string
): void {
  const normalizedCustomerId =
    customerId.trim();


  if (
    !normalizedCustomerId ||
    this.customerDetailsLoadingState()
  ) {
    return;
  }


  this.customerDetailsLoadingState
    .set(true);


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getCustomer(
      normalizedCustomerId
    )
    .pipe(
      finalize(() => {
        this.customerDetailsLoadingState
          .set(false);
      })
    )
    .subscribe({
      next: customer => {
        this.selectedCustomerState.set(
          customer
        );


        this.customerPropertyQueryState
          .set({
            pageNumber: 1,
            pageSize: 10,
            includeArchived: true,
            sortBy: 'createdAtUtc',
            sortDirection: 'desc'
          });


        this.loadCustomerProperties(
          customer.customerId
        );
      },


      error: error => {
        this.selectedCustomerState.set(
          null
        );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Customer details could not be loaded.'
          )
        );
      }
    });
}


loadCustomerProperties(
  customerId: string,
  query?: Partial<
    AdminCustomerPropertyQuery
  >
): void {
  const normalizedCustomerId =
    customerId.trim();


  if (
    !normalizedCustomerId ||
    this.customerPropertiesLoadingState()
  ) {
    return;
  }


  const nextQuery:
    AdminCustomerPropertyQuery = {
    ...this.customerPropertyQueryState(),
    ...query
  };


  this.customerPropertyQueryState
    .set(
      nextQuery
    );


  this.customerPropertiesLoadingState
    .set(true);


  this.errorMessageState.set(
    null
  );


  this.adminApiService
    .getCustomerProperties(
      normalizedCustomerId,
      nextQuery
    )
    .pipe(
      finalize(() => {
        this.customerPropertiesLoadingState
          .set(false);
      })
    )
    .subscribe({
      next: response => {
        this.customerPropertiesState
          .set(response);
      },


      error: error => {
        this.customerPropertiesState
          .set(
            this.createEmptyPagedResult<
              VacationProperty
            >(
              nextQuery.pageNumber,
              nextQuery.pageSize
            )
          );


        this.errorMessageState.set(
          this.getErrorMessage(
            error,
            'Customer properties could not be loaded.'
          )
        );
      }
    });
}


changeCustomerStatus(
  customer:
    AdminCustomerSummary |
    AdminCustomerDetails,
  isActive: boolean
): Observable<void> {
  return this.updateUserStatus(
    customer.authUserId,
    isActive
  )
    .pipe(
      tap(() => {
        const status =
          isActive
            ? CustomerStatus.Active
            : CustomerStatus.Inactive;


        this.customersState.update(
          page => ({
            ...page,
            items:
              page.items.map(
                item =>
                  item.customerId ===
                  customer.customerId
                    ? {
                        ...item,
                        status
                      }
                    : item
              )
          })
        );


        this.selectedCustomerState
          .update(
            selectedCustomer =>
              selectedCustomer
                ?.customerId ===
                customer.customerId
                ? {
                    ...selectedCustomer,
                    status
                  }
                : selectedCustomer
          );
      })
    );
}


clearSelectedCustomer(): void {
  this.selectedCustomerState.set(
    null
  );


  this.customerPropertiesState.set(
    this.createEmptyPagedResult<
      VacationProperty
    >()
  );
}
private createEmptyPagedResult<T>(
  pageNumber = 1,
  pageSize = 10
): PagedResult<T> {
  return {
    items: [],
    pageNumber,
    pageSize,
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  };
}


  refreshPendingClaimsManagers():
    void {
    this.loadPendingClaimsManagers();
  }


  approveClaimsManager(
    applicationId: string
  ): Observable<void> {
    const normalizedApplicationId =
      applicationId.trim();


    if (!normalizedApplicationId) {
      return throwError(
        () =>
          new Error(
            'Application identifier is required.'
          )
      );
    }


    this.processingApplicationIdState
      .set(
        normalizedApplicationId
      );


    this.errorMessageState.set(
      null
    );


    return this.adminApiService
      .approveClaimsManager(
        normalizedApplicationId
      )
      .pipe(
        tap(() => {
          this.removeApplication(
            normalizedApplicationId
          );
        }),


        catchError(error => {
          this.errorMessageState.set(
            this.getErrorMessage(
              error,
              'The Claims Manager application could not be approved.'
            )
          );


          return throwError(
            () => error
          );
        }),


        finalize(() => {
          this.processingApplicationIdState
            .set(
              null
            );
        })
      );
  }


  rejectClaimsManager(
    applicationId: string,
    reason: string
  ): Observable<void> {
    const normalizedApplicationId =
      applicationId.trim();


    const normalizedReason =
      reason.trim();


    if (!normalizedApplicationId) {
      return throwError(
        () =>
          new Error(
            'Application identifier is required.'
          )
      );
    }


    if (
      normalizedReason.length < 20 ||
      normalizedReason.length > 1000
    ) {
      return throwError(
        () =>
          new Error(
            'Rejection reason must contain between 20 and 1000 characters.'
          )
      );
    }


    const request:
      RejectClaimsManagerRequest = {
      reason: normalizedReason
    };


    this.processingApplicationIdState
      .set(
        normalizedApplicationId
      );


    this.errorMessageState.set(
      null
    );


    return this.adminApiService
      .rejectClaimsManager(
        normalizedApplicationId,
        request
      )
      .pipe(
        tap(() => {
          this.removeApplication(
            normalizedApplicationId
          );
        }),


        catchError(error => {
          this.errorMessageState.set(
            this.getErrorMessage(
              error,
              'The Claims Manager application could not be rejected.'
            )
          );


          return throwError(
            () => error
          );
        }),


        finalize(() => {
          this.processingApplicationIdState
            .set(
              null
            );
        })
      );
  }


  downloadClaimsManagerDocument(
    applicationId: string
  ): Observable<DownloadedFile> {
    const normalizedApplicationId =
      applicationId.trim();


    if (!normalizedApplicationId) {
      return throwError(
        () =>
          new Error(
            'Application identifier is required.'
          )
      );
    }


    this.downloadingApplicationIdState
      .set(
        normalizedApplicationId
      );


    this.errorMessageState.set(
      null
    );


    return this.adminApiService
      .getClaimsManagerDocument(
        normalizedApplicationId
      )
      .pipe(
        map(response =>
          this.createDownloadedFile(
            response,
            normalizedApplicationId
          )
        ),


        catchError(error => {
          this.errorMessageState.set(
            this.getErrorMessage(
              error,
              'The verification document could not be downloaded.'
            )
          );


          return throwError(
            () => error
          );
        }),


        finalize(() => {
          this.downloadingApplicationIdState
            .set(
              null
            );
        })
      );
  }


  downloadAndSaveClaimsManagerDocument(
    applicationId: string
  ): void {
    this.downloadClaimsManagerDocument(
      applicationId
    )
      .subscribe({
        next: file => {
          this.saveDownloadedFile(
            file
          );
        }
      });
  }


  updateUserStatus(
    userId: string,
    isActive: boolean
  ): Observable<void> {
    const normalizedUserId =
      userId.trim();


    if (!normalizedUserId) {
      return throwError(
        () =>
          new Error(
            'User identifier is required.'
          )
      );
    }


    this.updatingUserIdState.set(
      normalizedUserId
    );


    this.errorMessageState.set(
      null
    );


    return this.adminApiService
      .updateUserStatus(
        normalizedUserId,
        {
          isActive
        }
      )
      .pipe(
        catchError(error => {
          this.clearError();
          this.notifications.showError(
            this.getErrorMessage(
              error,
              isActive
                ? 'The user account could not be activated.'
                : 'The user account could not be deactivated.'
            )
          );


          return throwError(
            () => error
          );
        }),


        finalize(() => {
          this.updatingUserIdState.set(
            null
          );
        })
      );
  }


  saveDownloadedFile(
    file: DownloadedFile
  ): void {
    const objectUrl =
      URL.createObjectURL(
        file.blob
      );


    const anchor =
      document.createElement(
        'a'
      );


    anchor.href =
      objectUrl;


    anchor.download =
      file.fileName;


    anchor.style.display =
      'none';


    document.body.appendChild(
      anchor
    );


    anchor.click();
    anchor.remove();


    window.setTimeout(
      () => {
        URL.revokeObjectURL(
          objectUrl
        );
      },
      0
    );
  }


  clearError(): void {
    this.errorMessageState.set(
      null
    );
  }


  clearPendingApplications(): void {
    this.pendingClaimsManagersState
      .set([]);
  }


  private removeApplication(
    applicationId: string
  ): void {
    this.pendingClaimsManagersState
      .update(
        applications =>
          applications.filter(
            application =>
              application
                .applicationId !==
              applicationId
          )
      );
  }


  private sortApplications(
    applications:
      PendingClaimsManagerResponse[]
  ): PendingClaimsManagerResponse[] {
    return [
      ...applications
    ].sort(
      (
        first,
        second
      ) =>
        this.getDateValue(
          second.submittedAtUtc
        ) -
        this.getDateValue(
          first.submittedAtUtc
        )
    );
  }


  private getDateValue(
    value: string
  ): number {
    const timestamp =
      new Date(
        value
      ).getTime();


    return Number.isNaN(
      timestamp
    )
      ? 0
      : timestamp;
  }


  private createDownloadedFile(
    response:
      HttpResponse<Blob>,
    applicationId: string
  ): DownloadedFile {
    const blob =
      response.body;


    if (
      !blob ||
      blob.size === 0
    ) {
      throw new Error(
        'The verification document response was empty.'
      );
    }

    const contentDisposition =
      response.headers.get(
        'content-disposition'
      );


    const fileName =
      this.extractFileName(
        contentDisposition
      ) ??
      this.createFallbackFileName(
        applicationId,
        blob.type
      );


    return {
      blob,
      fileName
    };
  }

  private createPolicyDownloadedFile(
    response: HttpResponse<Blob>,
    policyId: string,
    version: number
  ): DownloadedFile {
    const blob = response.body;

    if (!blob || blob.size === 0) {
      throw new Error(
        'The policy document response was empty.'
      );
    }

    const fileName =
      this.extractFileName(
        response.headers.get('content-disposition')
      ) ??
      `Policy-${policyId}_Version_${version}.pdf`;

    return {
      blob,
      fileName
    };
  }


  private extractFileName(
    contentDisposition:
      string | null
  ): string | null {
    if (!contentDisposition) {
      return null;
    }


    const encodedFileNameMatch =
      /filename\*=UTF-8''([^;]+)/i
        .exec(
          contentDisposition
        );


    if (
      encodedFileNameMatch?.[1]
    ) {
      try {
        return this.sanitizeFileName(
          decodeURIComponent(
            encodedFileNameMatch[1]
              .trim()
          )
        );
      } catch {
        return this.sanitizeFileName(
          encodedFileNameMatch[1]
            .trim()
        );
      }
    }


    const quotedFileNameMatch =
      /filename="([^"]+)"/i
        .exec(
          contentDisposition
        );


    if (
      quotedFileNameMatch?.[1]
    ) {
      return this.sanitizeFileName(
        quotedFileNameMatch[1]
          .trim()
      );
    }


    const unquotedFileNameMatch =
      /filename=([^;]+)/i
        .exec(
          contentDisposition
        );


    if (
      !unquotedFileNameMatch?.[1]
    ) {
      return null;
    }


    return this.sanitizeFileName(
      unquotedFileNameMatch[1]
        .trim()
    );
  }


  private createFallbackFileName(
    applicationId: string,
    contentType: string
  ): string {
    const extension =
      this.getFileExtension(
        contentType
      );


    return extension
      ? `claims-manager-{extension}`
      : `claims-manager-${applicationId}-document`;
  }


  private getFileExtension(
    contentType: string
  ): string | null {
    switch (
      contentType.toLowerCase()
    ) {
      case 'application/pdf':
        return 'pdf';


      case 'image/jpeg':
        return 'jpg';


      case 'image/png':
        return 'png';


      default:
        return null;
    }
  }


  private sanitizeFileName(
    fileName: string
  ): string {
    const sanitized =
      fileName
        .replace(
          /[<>:"/\\|?*\u0000-\u001F]/g,
          '_'
        )
        .replace(
          /\.+$/,
          ''
        )
        .trim();


    return (
      sanitized ||
      'verification-document'
    );
  }


  private getErrorMessage(
    error: unknown,
    fallbackMessage: string
  ): string {
    if (
      !(error instanceof
        HttpErrorResponse)
    ) {
      if (
        error instanceof Error &&
        error.message
      ) {
        return error.message;
      }


      return fallbackMessage;
    }


    const responseBody =
      typeof error.error === 'object' &&
      error.error !== null
        ? error.error as Record<string, unknown>
        : null;
    const errorCode =
      responseBody?.['code'] ??
      responseBody?.['errorCode'];


    if (
      errorCode ===
        'COMPREHENSIVE_POLICY_CONFLICT' ||
      errorCode ===
        'COMPREHENSIVE_POLICY_EXISTS'
    ) {
      return 'Property Protection and Natural Disaster Protection may coexist, but neither may coexist with Comprehensive Protection on the same property.';
    }


    const backendMessage =
      this.readBackendMessage(
        error.error
      );


    if (backendMessage) {
      return backendMessage;
    }


    if (error.status === 0) {
      return 'The Admin service is unavailable. Please try again.';
    }


    if (error.status === 401) {
      return 'Your session has expired. Please sign in again.';
    }


    if (error.status === 403) {
      return 'You do not have permission to perform this Admin action.';
    }


    if (error.status === 404) {
      return 'The requested Admin record was not found.';
    }


    if (error.status === 409) {
      return 'The record was changed by another request. Refresh the page and try again.';
    }


    return fallbackMessage;
  }


  private readBackendMessage(
    response: unknown
  ): string | null {
    if (
      typeof response !==
        'object' ||
      response === null
    ) {
      return null;
    }


    const body =
      response as Record<
        string,
        unknown
      >;


    const message =
      body['message'];


    if (
      typeof message ===
        'string' &&
      message.trim()
    ) {
      return message.trim();
    }


    const detail =
      body['detail'];


    if (
      typeof detail ===
        'string' &&
      detail.trim()
    ) {
      return detail.trim();
    }


    const errorMessage =
  body['error'];


if (
  typeof errorMessage === 'string' &&
  errorMessage.trim()
) {
  return errorMessage.trim();
}


    const title =
      body['title'];


    const validationMessage =
      this.readValidationMessage(
        body['errors']
      );


    if (validationMessage) {
      return validationMessage;
    }


    if (
      typeof title ===
        'string' &&
      title.trim()
    ) {
      return title.trim();
    }


    return null;
  }


  private readValidationMessage(
    errors: unknown
  ): string | null {
    if (
      typeof errors !==
        'object' ||
      errors === null
    ) {
      return null;
    }


    const validationMessages =
      Object.values(
        errors as Record<
          string,
          unknown
        >
      )
        .flatMap(value => {
          if (Array.isArray(value)) {
            return value.filter(
              item =>
                typeof item ===
                'string'
            ) as string[];
          }


          if (
            typeof value ===
              'string'
          ) {
            return [
              value
            ];
          }


          return [];
        })
        .map(message =>
          message.trim()
        )
        .filter(Boolean);


    return (
      validationMessages[0] ??
      null
    );
  }
}
