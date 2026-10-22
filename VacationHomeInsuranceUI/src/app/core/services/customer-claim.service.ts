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
  ClaimDetails
} from '../models/customer/claims/claim-details.model';

import {
  ClaimCoverageEligibility
} from '../models/customer/claims/claim-coverage-eligibility.model';


import {
  ClaimDocument
} from '../models/customer/claims/claim-document.model';


import {
  ClaimPagedResult
} from '../models/customer/claims/claim-paged-result.model';


import {
  ClaimSearchRequest
} from '../models/customer/claims/claim-search-request.model';


import {
  ClaimSummary
} from '../models/customer/claims/claim-summary.model';


import {
  CreateClaimRequest
} from '../models/customer/claims/create-claim-request.model';


import {
  UpdateClaimRequest
} from '../models/customer/claims/update-claim-request.model';


import {
  UploadClaimDocumentRequest
} from '../models/customer/claims/upload-claim-document-request.model';


import {
  WithdrawClaimRequest
} from '../models/customer/claims/withdraw-claim-request.model';

import {
  Observable
} from 'rxjs';


import {
  parseApiError
} from '../utils/api-error.util';
import { buildApiUrl } from '../utils/api-url.util';
import { GlobalNotificationService } from './global-notification.service';


const EMPTY_CLAIM_RESULT:
  ClaimPagedResult<ClaimSummary> = {
    items: [],
    pageNumber: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0
  };


@Injectable({
  providedIn:
    'root'
})
export class CustomerClaimService {
  private readonly httpClient =
    inject(HttpClient);


  private readonly notifications =
    inject(GlobalNotificationService);

  getEligibleCoverages(
    policyId: string,
    incidentType: number
  ): Observable<ClaimCoverageEligibility> {
    const params =
      new HttpParams()
        .set('policyId', policyId)
        .set('incidentType', String(incidentType));

    return this.httpClient.get<ClaimCoverageEligibility>(
      buildApiUrl('/claims/eligible-coverages'),
      { params }
    );
  }


  private readonly claimsState =
    signal<
      ClaimPagedResult<ClaimSummary>
    >(
      EMPTY_CLAIM_RESULT
    );


  private readonly selectedClaimState =
    signal<ClaimDetails | null>(
      null
    );


  private readonly documentsState =
    signal<ClaimDocument[]>([]);


  private readonly claimsLoadingState =
    signal(false);


  private readonly claimDetailsLoadingState =
    signal(false);


  private readonly documentsLoadingState =
    signal(false);


  private readonly claimProcessingState =
    signal(false);


  private readonly documentProcessingState =
    signal(false);


  private readonly errorMessageState =
    signal<string | null>(
      null
    );


  private readonly successMessageState =
    signal<string | null>(
      null
    );


  private readonly currentSearchState =
    signal<ClaimSearchRequest>({
      status:
        null,


      claimNumber:
        null,


      pageNumber:
        1,


      pageSize:
        20
    });


  readonly claims =
    this.claimsState.asReadonly();


  readonly selectedClaim =
    this.selectedClaimState.asReadonly();


  readonly documents =
    this.documentsState.asReadonly();


  readonly claimsLoading =
    this.claimsLoadingState.asReadonly();


  readonly claimDetailsLoading =
    this.claimDetailsLoadingState.asReadonly();


  readonly documentsLoading =
    this.documentsLoadingState.asReadonly();


  readonly claimProcessing =
    this.claimProcessingState.asReadonly();


  readonly documentProcessing =
    this.documentProcessingState.asReadonly();


  readonly errorMessage =
    this.errorMessageState.asReadonly();


  readonly successMessage =
    this.successMessageState.asReadonly();


  readonly currentSearch =
    this.currentSearchState.asReadonly();


  readonly claimItems =
    computed(() => {
      return this.claimsState().items;
    });


  readonly pageNumber =
    computed(() => {
      return this.claimsState()
        .pageNumber;
    });


  readonly pageSize =
    computed(() => {
      return this.claimsState()
        .pageSize;
    });


  readonly totalCount =
    computed(() => {
      return this.claimsState()
        .totalCount;
    });


  readonly totalPages =
    computed(() => {
      return this.claimsState()
        .totalPages;
    });


  readonly hasPreviousPage =
    computed(() => {
      return this.pageNumber() > 1;
    });


  readonly hasNextPage =
    computed(() => {
      return (
        this.pageNumber() <
        this.totalPages()
      );
    });


  loadClaims(
    request:
      ClaimSearchRequest =
        this.currentSearchState()
  ): void {
    if (
      this.claimsLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.currentSearchState.set(
      request
    );


    this.claimsLoadingState.set(
      true
    );


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
      request.status !== null
    ) {
      params =
        params.set(
          'status',
          String(
            request.status
          )
        );
    }


    if (request.claimNumber) {
      params =
        params.set(
          'claimNumber',
          request.claimNumber
        );
    }


        this.httpClient
        .get<
            ClaimPagedResult<ClaimSummary>
        >(
            buildApiUrl('/claims'),
            {
            params
            }
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


        error: error => {
          this.claimsState.set(
            EMPTY_CLAIM_RESULT
          );


          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
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


    this.clearError();


    this.selectedClaimState.set(
      null
    );


    this.documentsState.set(
      []
    );


    this.claimDetailsLoadingState.set(
      true
    );


    this.httpClient
      .get<ClaimDetails>(
        buildApiUrl(`/claims/${normalizedClaimId}`)
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


          this.documentsState.set(
            claim.documents ?? []
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }


  createClaim(
    request:
      CreateClaimRequest
  ): void {
    if (
      this.claimProcessingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.claimProcessingState.set(
      true
    );


    this.httpClient
      .post<ClaimDetails>(
        buildApiUrl('/claims'),
        request
      )
      .pipe(
        finalize(() => {
          this.claimProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: claim => {
          this.selectedClaimState.set(
            claim
          );


          this.documentsState.set(
            claim.documents ?? []
          );


          this.notifications.showSuccess(
            'Claim submitted successfully.'
          );


          this.loadClaims();
        },


        error: error => {
          this.clearError();
          this.notifications.showError(
            parseApiError(error).message
          );
        }
      });
  }


  updateClaim(
    claimId: string,
    request:
      UpdateClaimRequest
  ): void {
    const normalizedClaimId =
      claimId.trim();


    if (
      !normalizedClaimId ||
      this.claimProcessingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.claimProcessingState.set(
      true
    );


    this.httpClient
      .put<void>(
        buildApiUrl(`/claims/${normalizedClaimId}`),
        request
      )
      .pipe(
        finalize(() => {
          this.claimProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: () => {
          this.notifications.showSuccess(
            'Claim updated successfully.'
          );


          this.loadClaim(
            normalizedClaimId
          );


          this.loadClaims();
        },


        error: error => {
          this.clearError();
          this.notifications.showError(
            parseApiError(error).message
          );
        }
      });
  }


  withdrawClaim(
    claimId: string,
    rowVersion: string,
    reason: string | null
  ): void {
    const normalizedClaimId =
      claimId.trim();


    if (
      !normalizedClaimId ||
      this.claimProcessingState()
    ) {
      return;
    }


    const request:
      WithdrawClaimRequest = {
        reason:
          reason?.trim() ||
          null,


        rowVersion
      };


    this.clearError();
    this.clearSuccessMessage();


    this.claimProcessingState.set(
      true
    );


    this.httpClient
      .post<void>(
        buildApiUrl(`/claims/${normalizedClaimId}/withdraw`),
        request
      )
      .pipe(
        finalize(() => {
          this.claimProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: () => {
          this.notifications.showSuccess(
            'Claim withdrawn successfully.'
          );


          this.loadClaim(
            normalizedClaimId
          );


          this.loadClaims();
        },


        error: error => {
          this.clearError();
          this.notifications.showError(
            parseApiError(error).message
          );
        }
      });
  }


  loadDocuments(
    claimId: string
  ): void {
    const normalizedClaimId =
      claimId.trim();


    if (
      !normalizedClaimId ||
      this.documentsLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.documentsLoadingState.set(
      true
    );


    this.httpClient
      .get<ClaimDocument[]>(
        buildApiUrl(`/claims/${normalizedClaimId}/documents`)
      )
      .pipe(
        finalize(() => {
          this.documentsLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: documents => {
          this.documentsState.set(
            documents ?? []
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }


uploadDocument(
  claimId: string,
  documentType: number,
  file: File
): void {
  const normalizedClaimId =
    claimId.trim();


  if (
    !normalizedClaimId ||
    !file ||
    this.documentProcessingState()
  ) {
    return;
  }


  const formData =
    new FormData();


  formData.append(
    'documentType',
    String(documentType)
  );


  formData.append(
    'file',
    file,
    file.name
  );


  this.clearError();
  this.clearSuccessMessage();


  this.documentProcessingState.set(
    true
  );


  this.httpClient
    .post<ClaimDocument>(
      buildApiUrl(
        `/claims/${normalizedClaimId}/documents`
      ),
      formData
    )
    .pipe(
      finalize(() => {
        this.documentProcessingState.set(
          false
        );
      })
    )
    .subscribe({
      next: document => {
        this.documentsState.update(
          documents => [
            ...documents,
            document
          ]
        );


        this.notifications.showSuccess(
          'Claim document uploaded successfully.'
        );


        this.loadClaim(
          normalizedClaimId
        );
      },


      error: error => {
        this.clearError();
        this.notifications.showError(
          parseApiError(error).message
        );
      }
    });
}


  deleteDocument(
    claimId: string,
    documentId: string
  ): void {
    const normalizedClaimId =
      claimId.trim();


    const normalizedDocumentId =
      documentId.trim();


    if (
      !normalizedClaimId ||
      !normalizedDocumentId ||
      this.documentProcessingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.documentProcessingState.set(
      true
    );


    this.httpClient
      .delete<void>(
        buildApiUrl(`/claims/${normalizedClaimId}/documents/${normalizedDocumentId}`)
      )
      .pipe(
        finalize(() => {
          this.documentProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: () => {
          this.documentsState.update(
            documents =>
              documents.filter(
                document =>
                  document.id !==
                  normalizedDocumentId
              )
          );


          this.notifications.showSuccess(
            'Claim document deleted successfully.'
          );


          this.loadClaim(
            normalizedClaimId
          );
        },


        error: error => {
          this.clearError();
          this.notifications.showError(
            parseApiError(error).message
          );
        }
      });
  }


  refreshClaims(): void {
    this.loadClaims(
      this.currentSearchState()
    );
  }


  goToPreviousPage(): void {
    if (
      !this.hasPreviousPage() ||
      this.claimsLoadingState()
    ) {
      return;
    }


    this.loadClaims({
      ...this.currentSearchState(),


      pageNumber:
        this.pageNumber() - 1
    });
  }


  goToNextPage(): void {
    if (
      !this.hasNextPage() ||
      this.claimsLoadingState()
    ) {
      return;
    }


    this.loadClaims({
      ...this.currentSearchState(),


      pageNumber:
        this.pageNumber() + 1
    });
  }


  clearSelectedClaim(): void {
    this.selectedClaimState.set(
      null
    );


    this.documentsState.set(
      []
    );
  }


  clearError(): void {
    this.errorMessageState.set(
      null
    );
  }
  setError(
  message: string
): void {
  this.errorMessageState.set(
    message
  );
}




  clearSuccessMessage(): void {
    this.successMessageState.set(
      null
    );
  }
}
