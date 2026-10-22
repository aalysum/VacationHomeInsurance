import {
  HttpClient,
  HttpResponse
} from '@angular/common/http';


import {
  Injectable,
  computed,
  inject,
  signal
} from '@angular/core';


import {
  CancelPolicyRequest
} from '../models/customer/policies/cancel-policy-request.model';


import {
  Payment
} from '../models/customer/policies/payment.model';


import {
  PaymentRequest
} from '../models/customer/policies/payment-request.model';


import {
  PaymentResult
} from '../models/customer/policies/payment-result.model';


import {
  PolicyDetails
} from '../models/customer/policies/policy-details.model';


import {
  PolicySummary
} from '../models/customer/policies/policy-summary.model';
import {
  PolicyDocument
} from '../models/policy-document.model';


import {
  RenewalEligibility
} from '../models/customer/policies/renewal-eligibility.model';


import {
  RenewalResult
} from '../models/customer/policies/renewal-result.model';


import {
  parseApiError
} from '../utils/api-error.util';
import { buildApiUrl } from '../config/api-url.util';
import { GlobalNotificationService } from './global-notification.service';


import {
  Observable,
  catchError,
  finalize,
  tap,
  throwError
} from 'rxjs';


@Injectable({
  providedIn:
    'root'
})
export class CustomerPolicyService {
  private readonly httpClient =
    inject(HttpClient);


  private readonly notifications =
    inject(GlobalNotificationService);


  private readonly policiesState =
    signal<PolicySummary[]>([]);


  private readonly selectedPolicyState =
    signal<PolicyDetails | null>(
      null
    );


  private readonly selectedPaymentState =
    signal<Payment | null>(
      null
    );


  private readonly paymentResultState =
    signal<PaymentResult | null>(
      null
    );


  private readonly renewalEligibilityState =
    signal<RenewalEligibility | null>(
      null
    );


  private readonly renewalResultState =
    signal<RenewalResult | null>(
      null
    );


  private readonly policiesLoadingState =
    signal(false);


  private readonly policyDetailsLoadingState =
    signal(false);

  private readonly downloadingPolicyDocumentIdState =
    signal<string | null>(null);


  private readonly paymentLoadingState =
    signal(false);


  private readonly renewalLoadingState =
    signal(false);


  private readonly policyProcessingState =
    signal(false);


  private readonly errorMessageState =
    signal<string | null>(
      null
    );


  private readonly successMessageState =
    signal<string | null>(
      null
    );


  readonly policies =
    this.policiesState.asReadonly();


  readonly selectedPolicy =
    this.selectedPolicyState.asReadonly();


  readonly selectedPayment =
    this.selectedPaymentState.asReadonly();


  readonly paymentResult =
    this.paymentResultState.asReadonly();


  readonly renewalEligibility =
    this.renewalEligibilityState.asReadonly();


  readonly renewalResult =
    this.renewalResultState.asReadonly();


  readonly policiesLoading =
    this.policiesLoadingState.asReadonly();


  readonly policyDetailsLoading =
    this.policyDetailsLoadingState.asReadonly();

  readonly downloadingPolicyDocumentId =
    this.downloadingPolicyDocumentIdState.asReadonly();


  readonly paymentLoading =
    this.paymentLoadingState.asReadonly();


  readonly renewalLoading =
    this.renewalLoadingState.asReadonly();


  readonly policyProcessing =
    this.policyProcessingState.asReadonly();


  readonly errorMessage =
    this.errorMessageState.asReadonly();


  readonly successMessage =
    this.successMessageState.asReadonly();


  readonly totalPolicies =
    computed(() => {
      return this.policiesState()
        .length;
    });


  loadPolicies(): void {
    if (
      this.policiesLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.policiesLoadingState.set(
      true
    );


    this.httpClient
      .get<PolicySummary[]>(
        buildApiUrl('/policies')
      )
      .pipe(
        finalize(() => {
          this.policiesLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: policies => {
          this.policiesState.set(
            policies ?? []
          );
        },


        error: error => {
          this.policiesState.set(
            []
          );


          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
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


    this.clearError();


    this.selectedPolicyState.set(
      null
    );


    this.renewalEligibilityState.set(
      null
    );


    this.renewalResultState.set(
      null
    );


    this.policyDetailsLoadingState.set(
      true
    );


    this.httpClient
      .get<PolicyDetails>(
        buildApiUrl(`/policies/${normalizedPolicyId}`)
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
          this.selectedPolicyState.set(
            policy
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }

  downloadPolicyDocument(
    policyId: string,
    document: PolicyDocument
  ): void {
    const normalizedPolicyId = policyId.trim();

    if (
      !normalizedPolicyId ||
      !document.id ||
      document.status !== 'Available' ||
      this.downloadingPolicyDocumentIdState()
    ) {
      return;
    }

    this.clearError();
    this.downloadingPolicyDocumentIdState.set(document.id);

    this.httpClient
      .get(
        buildApiUrl(
          `/policies/${encodeURIComponent(normalizedPolicyId)}` +
            `/documents/${encodeURIComponent(document.id)}`
        ),
        {
          observe: 'response',
          responseType: 'blob'
        }
      )
      .pipe(
        finalize(() => {
          this.downloadingPolicyDocumentIdState.set(null);
        })
      )
      .subscribe({
        next: response => {
          this.savePolicyDocument(
            response,
            normalizedPolicyId,
            document
          );
        },
        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }

  private savePolicyDocument(
    response: HttpResponse<Blob>,
    policyId: string,
    policyDocument: PolicyDocument
  ): void {
    const blob = response.body;

    if (!blob || blob.size === 0) {
      this.errorMessageState.set(
        'The policy document response was empty.'
      );
      return;
    }

    const fileName =
      /filename="?([^";]+)"?/i.exec(
        response.headers.get('content-disposition') ?? ''
      )?.[1] ??
      `Policy-${policyId}_Version_${policyDocument.version}.pdf`;
    const objectUrl = URL.createObjectURL(blob);
    const anchor = document.createElement('a');

    anchor.href = objectUrl;
    anchor.download = fileName;
    anchor.style.display = 'none';
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    window.setTimeout(() => URL.revokeObjectURL(objectUrl), 0);
  }


  cancelPolicy(
    policyId: string,
    reason: string
  ): void {
    const normalizedPolicyId =
      policyId.trim();


    const normalizedReason =
      reason.trim();


    if (
      !normalizedPolicyId ||
      !normalizedReason ||
      this.policyProcessingState()
    ) {
      return;
    }


    const request:
      CancelPolicyRequest = {
        reason:
          normalizedReason
      };


    this.clearError();
    this.clearSuccessMessage();


    this.policyProcessingState.set(
      true
    );


    this.httpClient
      .post<void>(
        buildApiUrl(`/policies/${normalizedPolicyId}/cancel`),
        request
      )
      .pipe(
        finalize(() => {
          this.policyProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: () => {
          this.notifications.showSuccess(
            'Policy cancelled successfully.'
          );


          this.loadPolicy(
            normalizedPolicyId
          );


          this.loadPolicies();
        },


        error: error => {
          this.clearError();
          this.notifications.showError(
            parseApiError(error).message
          );
        }
      });
  }
processPayment(
  request: PaymentRequest
): Observable<PaymentResult> {
  if (
    this.paymentLoadingState()
  ) {
    return throwError(
      () =>
        new Error(
          'A payment is already being processed.'
        )
    );
  }


  this.clearError();
  this.clearSuccessMessage();


  this.paymentResultState.set(
    null
  );


  this.paymentLoadingState.set(
    true
  );


  return this.httpClient
    .post<PaymentResult>(
      buildApiUrl(
        '/policies/payments'
      ),
      request
    )
    .pipe(
      tap(result => {
        this.paymentResultState.set(
          result
        );


        this.notifications.showSuccess(
          result.message ||
          'Payment completed successfully.'
        );
      }),


      catchError(error => {
        this.clearError();
        this.notifications.showError(
          parseApiError(error).message
        );


        return throwError(
          () => error
        );
      }),


      finalize(() => {
        this.paymentLoadingState.set(
          false
        );
      })
    );
}
  loadPayment(
    paymentId: string
  ): void {
    const normalizedPaymentId =
      paymentId.trim();


    if (
      !normalizedPaymentId ||
      this.paymentLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.selectedPaymentState.set(
      null
    );


    this.paymentLoadingState.set(
      true
    );


    this.httpClient
      .get<Payment>(
        buildApiUrl(`/policies/payments/${normalizedPaymentId}`)
      )
      .pipe(
        finalize(() => {
          this.paymentLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: payment => {
          this.selectedPaymentState.set(
            payment
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }


  checkRenewalEligibility(
    policyId: string
  ): void {
    const normalizedPolicyId =
      policyId.trim();


    if (
      !normalizedPolicyId ||
      this.renewalLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.renewalEligibilityState.set(
      null
    );


    this.renewalResultState.set(
      null
    );


    this.renewalLoadingState.set(
      true
    );


    this.httpClient
      .get<RenewalEligibility>(
        buildApiUrl(`/policies/${normalizedPolicyId}/renewal-eligibility`)
      )
      .pipe(
        finalize(() => {
          this.renewalLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: eligibility => {
          this.renewalEligibilityState.set(
            eligibility
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }


  createRenewal(
    policyId: string
  ): void {
    const normalizedPolicyId =
      policyId.trim();


    if (
      !normalizedPolicyId ||
      this.renewalLoadingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.renewalResultState.set(
      null
    );


    this.renewalLoadingState.set(
      true
    );


    this.httpClient
      .post<RenewalResult>(
        buildApiUrl(`/policies/${normalizedPolicyId}/renew`),
        {}
      )
      .pipe(
        finalize(() => {
          this.renewalLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: result => {
          this.renewalResultState.set(
            result
          );


          this.notifications.showSuccess(
            result.message ||
            'Renewal quote created successfully.'
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


  refreshPolicies(): void {
    this.loadPolicies();
  }


  clearSelectedPolicy(): void {
    this.selectedPolicyState.set(
      null
    );


    this.renewalEligibilityState.set(
      null
    );


    this.renewalResultState.set(
      null
    );
  }


  clearSelectedPayment(): void {
    this.selectedPaymentState.set(
      null
    );


    this.paymentResultState.set(
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
}
