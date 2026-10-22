import {
  HttpClient,
  HttpParams
} from '@angular/common/http';


import {
  Injectable,
  computed,
  inject,
  signal
} from '@angular/core';


import {
  finalize
} from 'rxjs';


import {
  ClaimPagedResult
} from '../models/customer/claims/claim-paged-result.model';


import {
  ClaimsManagerClaim
} from '../models/claims-manager/claims-manager-claim.model';


import {
  ClaimsManagerClaimDetail
} from '../models/claims-manager/claims-manager-claim-detail.model';


import {
  ClaimsManagerClaimSearchRequest
} from '../models/claims-manager/claims-manager-claim-search-request.model';


import {
  ClaimsManagerDashboard
} from '../models/claims-manager/claims-manager-dashboard.model';


import {
  ApproveClaimRequest
} from '../models/claims-manager/requests/approve-claim-request.model';


import {
  ClaimRowVersionRequest
} from '../models/claims-manager/requests/claim-row-version-request.model';


import {
  MarkClaimPaidRequest
} from '../models/claims-manager/requests/mark-claim-paid-request.model';


import {
  RejectClaimRequest
} from '../models/claims-manager/requests/reject-claim-request.model';


import {
  RequestClaimInformationRequest
} from '../models/claims-manager/requests/request-claim-information-request.model';
import { ClaimDocument } from '../models/customer/claims/claim-document.model';
import { ClaimsManagerPolicyDocument } from '../models/claims-manager/claims-manager-claim-detail.model';
import { buildApiUrl } from '../config/api-url.util';


const EMPTY_CLAIM_RESULT:
  ClaimPagedResult<ClaimsManagerClaim> = {
    items: [],
    pageNumber: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0
  };


@Injectable({
  providedIn: 'root'
})
export class ClaimsManagerService {
  private readonly httpClient =
    inject(HttpClient);


  private readonly baseUrl =
    buildApiUrl('/admin/claims');


  private readonly dashboardState =
    signal<ClaimsManagerDashboard | null>(
      null
    );


  private readonly claimsState =
    signal<
      ClaimPagedResult<ClaimsManagerClaim>
    >(
      EMPTY_CLAIM_RESULT
    );


  private readonly selectedClaimState =
    signal<ClaimsManagerClaimDetail | null>(
      null
    );


  private readonly dashboardLoadingState =
    signal(false);


  private readonly claimsLoadingState =
    signal(false);


  private readonly claimDetailsLoadingState =
    signal(false);


  private readonly workflowProcessingState =
    signal(false);


  private readonly processingActionState =
    signal<string | null>(
      null
    );

  private readonly downloadingDocumentState =
    signal<string | null>(null);


  private readonly errorMessageState =
    signal<string | null>(
      null
    );


  private readonly successMessageState =
    signal<string | null>(
      null
    );


  private readonly currentSearchState =
    signal<ClaimsManagerClaimSearchRequest>({
      status: null,
      claimType: null,
      claimNumber: null,
      customerId: null,
      fromDate: null,
      toDate: null,
      pageNumber: 1,
      pageSize: 20
    });


  readonly dashboard =
    this.dashboardState.asReadonly();


  readonly claims =
    this.claimsState.asReadonly();


  readonly selectedClaim =
    this.selectedClaimState.asReadonly();


  readonly dashboardLoading =
    this.dashboardLoadingState.asReadonly();


  readonly claimsLoading =
    this.claimsLoadingState.asReadonly();


  readonly claimDetailsLoading =
    this.claimDetailsLoadingState.asReadonly();


  readonly workflowProcessing =
    this.workflowProcessingState.asReadonly();


  readonly processingAction =
    this.processingActionState.asReadonly();

  readonly downloadingDocument =
    this.downloadingDocumentState.asReadonly();


  readonly errorMessage =
    this.errorMessageState.asReadonly();


  readonly successMessage =
    this.successMessageState.asReadonly();


  readonly currentSearch =
    this.currentSearchState.asReadonly();


  readonly claimItems =
    computed(
      () =>
        this.claimsState().items
    );


  readonly totalClaims =
    computed(
      () =>
        this.claimsState().totalCount
    );


  readonly pageNumber =
    computed(
      () =>
        this.claimsState().pageNumber
    );


  readonly pageSize =
    computed(
      () =>
        this.claimsState().pageSize
    );


  readonly totalPages =
    computed(
      () =>
        this.claimsState().totalPages
    );


  readonly hasPreviousPage =
    computed(
      () =>
        this.claimsState().pageNumber > 1
    );


  readonly hasNextPage =
    computed(() => {
      const result =
        this.claimsState();


      return (
        result.totalPages > 0 &&
        result.pageNumber <
          result.totalPages
      );
    });


  loadDashboard(): void {
    if (
      this.dashboardLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.dashboardLoadingState.set(
      true
    );


    this.httpClient
      .get<ClaimsManagerDashboard>(
        buildApiUrl(`/claims-manager/claims/dashboard`)
      )
      .pipe(
        finalize(
          () =>
            this.dashboardLoadingState.set(
              false
            )
        )
      )
      .subscribe({
        next:
          dashboard => {
            this.dashboardState.set(
              dashboard
            );
          },


        error:
          error => {
            this.errorMessageState.set(
              this.resolveErrorMessage(
                error,
                'Unable to load the Claims Manager dashboard.'
              )
            );
          }
      });
  }


  loadClaims(
    request:
      ClaimsManagerClaimSearchRequest =
        this.currentSearchState()
  ): void {
    if (
      this.claimsLoadingState()
    ) {
      return;
    }
    


    this.clearError();


    const normalizedRequest =
      this.normalizeSearchRequest(
        request
      );


    this.currentSearchState.set(
      normalizedRequest
    );


    this.claimsLoadingState.set(
      true
    );


    const params =
      this.buildSearchParams(
        normalizedRequest
      );
      


    this.httpClient
      .get<
        ClaimPagedResult<ClaimsManagerClaim>
      >(
        this.baseUrl,
        {
          params
        }
      )
      .pipe(
        finalize(
          () =>
            this.claimsLoadingState.set(
              false
            )
        )
      )
      .subscribe({
        next:
          result => {
            this.claimsState.set({
              items:
                result.items ?? [],


              pageNumber:
                result.pageNumber,


              pageSize:
                result.pageSize,


              totalCount:
                result.totalCount,


              totalPages:
                result.totalPages
            });
          },


        error:
          error => {
            this.claimsState.set(
              EMPTY_CLAIM_RESULT
            );


            this.errorMessageState.set(
              this.resolveErrorMessage(
                error,
                'Unable to load assigned Claims.'
              )
            );
          }
      });
  }


loadClaim(
  claimId: string,
  preserveSuccessMessage:
    boolean = false
): void {
  const normalizedClaimId =
    claimId.trim();


  if (
    !normalizedClaimId ||
    this.claimDetailsLoadingState()
  ) {
    return;
  }


  this.clearError();


  if (
    !preserveSuccessMessage
  ) {
    this.clearSuccessMessage();
  }


  this.selectedClaimState.set(
    null
  );


  this.claimDetailsLoadingState.set(
    true
  );


  this.httpClient
    .get<ClaimsManagerClaimDetail>(
      `${this.baseUrl}/${encodeURIComponent(normalizedClaimId)}`
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
        this.selectedClaimState.set(
          claim
        );
      },


      error: error => {
        this.errorMessageState.set(
          this.resolveErrorMessage(
            error,
            'Unable to load Claim details.'
          )
        );
      }
    });
}


  moveToReview(
    claimId: string,
    rowVersion: string
  ): void {
    const request:
      ClaimRowVersionRequest = {
        rowVersion
      };


    this.executeWorkflowAction(
      claimId,
      'review',
      request,
      'Starting review',
      'Claim review started successfully.'
    );
  }


  requestInformation(
    claimId: string,
    request:
      RequestClaimInformationRequest
  ): void {
    this.executeWorkflowAction(
      claimId,
      'request-information',
      request,
      'Requesting information',
      'Additional information was requested successfully.'
    );
  }


  approveClaim(
    claimId: string,
    request:
      ApproveClaimRequest
  ): void {
    this.executeWorkflowAction(
      claimId,
      'approve',
      request,
      'Approving Claim',
      'The Claim was approved successfully.'
    );
  }


  rejectClaim(
    claimId: string,
    request:
      RejectClaimRequest
  ): void {
    this.executeWorkflowAction(
      claimId,
      'reject',
      request,
      'Rejecting Claim',
      'The Claim was rejected successfully.'
    );
  }


  markClaimPaid(
    claimId: string,
    request:
      MarkClaimPaidRequest
  ): void {
    this.executeWorkflowAction(
      claimId,
      'pay',
      request,
      'Processing payment',
      'The Claim payment was recorded successfully.'
    );
  }


  closeClaim(
    claimId: string,
    rowVersion: string
  ): void {
    const request:
      ClaimRowVersionRequest = {
        rowVersion
      };


    this.executeWorkflowAction(
      claimId,
      'close',
      request,
      'Closing Claim',
      'The Claim was closed successfully.'
    );
  }

  downloadDocument(
    claimId: string,
    claimDocument: ClaimDocument
  ): void {
    this.transferDocument(claimId, claimDocument, false);
  }

  viewDocument(
    claimId: string,
    claimDocument: ClaimDocument
  ): void {
    this.transferDocument(claimId, claimDocument, true);
  }

  downloadPolicyDocument(
    claimId: string,
    policyNumber: string,
    policyDocument: ClaimsManagerPolicyDocument
  ): void {
    this.transferPolicyDocument(
      claimId,
      policyNumber,
      policyDocument,
      false
    );
  }

  viewPolicyDocument(
    claimId: string,
    policyNumber: string,
    policyDocument: ClaimsManagerPolicyDocument
  ): void {
    this.transferPolicyDocument(
      claimId,
      policyNumber,
      policyDocument,
      true
    );
  }

  private transferPolicyDocument(
    claimId: string,
    policyNumber: string,
    policyDocument: ClaimsManagerPolicyDocument,
    openInBrowser: boolean
  ): void {
    if (
      this.downloadingDocumentState() ||
      policyDocument.status.toLowerCase() !== 'available'
    ) {
      return;
    }

    this.clearError();
    this.downloadingDocumentState.set(policyDocument.id);
    this.httpClient
      .get(
        buildApiUrl(
          `/admin/claims/${encodeURIComponent(claimId)}/policy-documents/${encodeURIComponent(policyDocument.id)}/download`
        ),
        { responseType: 'blob' }
      )
      .pipe(finalize(() => this.downloadingDocumentState.set(null)))
      .subscribe({
        next: content => {
          const objectUrl = URL.createObjectURL(content);
          const link = document.createElement('a');
          link.href = objectUrl;
          if (openInBrowser) {
            link.target = '_blank';
            link.rel = 'noopener';
          } else {
            link.download = `${policyNumber}-v${policyDocument.version}.pdf`;
          }
          link.click();
          if (openInBrowser) {
            window.setTimeout(() => URL.revokeObjectURL(objectUrl), 60_000);
          } else {
            URL.revokeObjectURL(objectUrl);
          }
        },
        error: error => {
          this.errorMessageState.set(
            this.resolveErrorMessage(
              error,
              'Unable to open the policy document.'
            )
          );
        }
      });
  }

  private transferDocument(
    claimId: string,
    claimDocument: ClaimDocument,
    openInBrowser: boolean
  ): void {
    if (this.downloadingDocumentState()) {
      return;
    }

    this.clearError();
    this.downloadingDocumentState.set(claimDocument.id);
    this.httpClient
      .get(
        buildApiUrl(
          `/claims/${encodeURIComponent(claimId)}/documents/${encodeURIComponent(claimDocument.id)}/download`
        ),
        { responseType: 'blob' }
      )
      .pipe(finalize(() => this.downloadingDocumentState.set(null)))
      .subscribe({
        next: content => {
          const objectUrl = URL.createObjectURL(content);
          const link = document.createElement('a');
          link.href = objectUrl;
          if (openInBrowser) {
            link.target = '_blank';
            link.rel = 'noopener';
          } else {
            link.download = claimDocument.fileName;
          }
          link.click();
          if (openInBrowser) {
            window.setTimeout(() => URL.revokeObjectURL(objectUrl), 60_000);
          } else {
            URL.revokeObjectURL(objectUrl);
          }
        },
        error: error => {
          this.errorMessageState.set(
            this.resolveErrorMessage(
              error,
              'Unable to download the claim document.'
            )
          );
        }
      });
  }


  refreshClaims(): void {
    this.loadClaims(
      this.currentSearchState()
    );
  }


  goToPage(
    pageNumber: number
  ): void {
    const currentRequest = {
      ...this.currentSearchState()
    };


    if (
      pageNumber < 1 ||
      (
        this.totalPages() > 0 &&
        pageNumber >
          this.totalPages()
      )
    ) {
      return;
    }


    this.loadClaims({
      ...currentRequest,
      pageNumber
    });
  }


  goToPreviousPage(): void {
    if (
      !this.hasPreviousPage()
    ) {
      return;
    }


    this.goToPage(
      this.pageNumber() - 1
    );
  }


  goToNextPage(): void {
    if (
      !this.hasNextPage()
    ) {
      return;
    }


    this.goToPage(
      this.pageNumber() + 1
    );
  }


  clearSelectedClaim(): void {
    this.selectedClaimState.set(
      null
    );
  }


  clearError(): void {
    this.errorMessageState.set(
      null
    );
  }


  clearSuccessMessage(): void {
    this.successMessageState.set(
      null
    );
  }


  private executeWorkflowAction(
    claimId: string,
    action: string,
    request: object,
    processingLabel: string,
    successMessage: string
  ): void {
    const normalizedClaimId =
      claimId.trim();


    if (
      !normalizedClaimId ||
      this.workflowProcessingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.workflowProcessingState.set(
      true
    );


    this.processingActionState.set(
      processingLabel
    );


    this.httpClient
      .post<void>(
        `${this.baseUrl}/${encodeURIComponent(normalizedClaimId)}/${action}`,
        request
      )
      .pipe(
        finalize(() => {
          this.workflowProcessingState.set(
            false
          );


          this.processingActionState.set(
            null
          );
        })
      )
      .subscribe({
        next:
          () => {
            this.successMessageState.set(
              successMessage
            );


            this.reloadAfterWorkflowAction(
              normalizedClaimId
            );
          },


        error:
          error => {
            this.errorMessageState.set(
              this.resolveErrorMessage(
                error,
                'The Claim workflow action could not be completed.'
              )
            );
          }
      });
  }


private reloadAfterWorkflowAction(
  claimId: string
): void {
  this.loadClaim(
    claimId,
    true
  );


  this.loadDashboard();


  this.refreshClaims();
}


  private buildSearchParams(
    request:
      ClaimsManagerClaimSearchRequest
  ): HttpParams {
    let params =
      new HttpParams()
        .set(
          'pageNumber',
          String(
            request.pageNumber
          )
        )
        .set(
          'pageSize',
          String(
            request.pageSize
          )
        );


    if (
      request.status !== null &&
      request.status !== undefined
    ) {
      params =
        params.set(
          'status',
          String(
            request.status
          )
        );
    }


    if (
      request.claimType !== null &&
      request.claimType !== undefined
    ) {
      params =
        params.set(
          'claimType',
          String(
            request.claimType
          )
        );
    }


    if (
      request.claimNumber?.trim()
    ) {
      params =
        params.set(
          'claimNumber',
          request.claimNumber.trim()
        );
    }


    if (
      request.customerId?.trim()
    ) {
      params =
        params.set(
          'customerId',
          request.customerId.trim()
        );
    }


    if (
      request.fromDate?.trim()
    ) {
      params =
        params.set(
          'fromDate',
          request.fromDate.trim()
        );
    }


    if (
      request.toDate?.trim()
    ) {
      params =
        params.set(
          'toDate',
          `${request.toDate.trim()}T23:59:59.999`
        );
    }


    return params;
  }


  private normalizeSearchRequest(
    request:
      ClaimsManagerClaimSearchRequest
  ): ClaimsManagerClaimSearchRequest {
    const pageNumber =
      Number.isInteger(
        request.pageNumber
      ) &&
      request.pageNumber > 0
        ? request.pageNumber
        : 1;


    const pageSize =
      Number.isInteger(
        request.pageSize
      ) &&
      request.pageSize >= 1 &&
      request.pageSize <= 100
        ? request.pageSize
        : 20;


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


      pageNumber,


      pageSize
    };
  }


  private resolveErrorMessage(
    error: unknown,
    fallbackMessage: string
  ): string {
    if (
      typeof error === 'object' &&
      error !== null
    ) {
      const possibleError =
        error as {
          error?: {
            message?: unknown;
            detail?: unknown;
            title?: unknown;
          } | string;


          message?: unknown;
        };


      if (
        typeof possibleError.error ===
        'string' &&
        possibleError.error.trim()
      ) {
        return possibleError.error.trim();
      }


      if (
        typeof possibleError.error ===
          'object' &&
        possibleError.error !== null
      ) {
        if (
          typeof possibleError.error.message ===
            'string' &&
          possibleError.error.message.trim()
        ) {
          return possibleError.error.message.trim();
        }


        if (
          typeof possibleError.error.detail ===
            'string' &&
          possibleError.error.detail.trim()
        ) {
          return possibleError.error.detail.trim();
        }


        if (
          typeof possibleError.error.title ===
            'string' &&
          possibleError.error.title.trim()
        ) {
          return possibleError.error.title.trim();
        }
      }


      if (
        typeof possibleError.message ===
          'string' &&
        possibleError.message.trim()
      ) {
        return possibleError.message.trim();
      }
    }


    return fallbackMessage;
  }
}
