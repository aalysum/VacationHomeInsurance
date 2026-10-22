import {
  HttpClient,
  HttpParams,
  HttpResponse
} from '@angular/common/http';


import {
  Injectable,
  inject
} from '@angular/core';


import {
  Observable
} from 'rxjs';


import {
  ADMIN_ENDPOINTS
} from '../constants/admin-endpoints.constants';


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
  PagedResult
} from '../models/admin/paged-result.model';


import {
  PendingClaimsManagerResponse
} from '../models/admin/pending-claims-manager-response.model';


import {
  RejectClaimsManagerRequest
} from '../models/admin/reject-claims-manager-request.model';


import {
  UpdateUserStatusRequest
} from '../models/admin/update-user-status-request.model';


import {
  VacationProperty
} from '../models/admin/vacation-property.model';


import {
  buildApiUrl
} from '../utils/api-url.util';


import {
  AdminQuoteSearchQuery
} from '../models/admin/quotes/admin-quote-search-query.model';


import {
  ApproveQuoteRequest
} from '../models/admin/quotes/approve-quote-request.model';


import {
  InvalidateQuoteRequest
} from '../models/admin/quotes/invalidate-quote-request.model';


import {
  QuoteDetails
} from '../models/admin/quotes/quote-details.model';


import {
  QuoteSummary
} from '../models/admin/quotes/quote-summary.model';


import {
  RejectQuoteRequest
} from '../models/admin/quotes/reject-quote-request.model';


import {
  ProductDetails
} from '../models/admin/products/product-details.model';


import {
  UpdateCoverageLimitRequest
} from '../models/admin/products/update-coverage-limit-request.model';


import {
  UpdateProductStatusRequest
} from '../models/admin/products/update-product-status-request.model';


import {
  PricingConfiguration
} from '../models/admin/pricing/pricing-configuration.model';


import {
  UpdatePricingRequest
} from '../models/admin/pricing/update-pricing-request.model';


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
import {
  ActiveClaimsManager
} from '../models/admin/operations/claims/active-claims-manager.model';


import {
  AssignClaimsManagerRequest
} from '../models/admin/operations/claims/assign-claims-manager-request.model';
import { UpdateCoverageChargeRequest } from '../models/admin/pricing/update-coverage-charge-request.model';




@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private readonly http =
    inject(HttpClient);


    getPricing():
  Observable<PricingConfiguration> {
  return this.http.get<
    PricingConfiguration
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.pricing
    )
  );
}


updateCoverageCharge(
  productId: string,
  coverageId: string,
  request:
    UpdateCoverageChargeRequest
): Observable<void> {
  return this.http.put<void>(
    buildApiUrl(
      ADMIN_ENDPOINTS
        .updateCoverageCharge(
          productId,
          coverageId
        )
    ),
    request
  );
}




getClaimsDashboard():
  Observable<ClaimDashboard> {
  return this.http.get<
    ClaimDashboard
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS
        .claimsDashboard
    )
  );
}


getClaims(
  request:
    ClaimDashboardFilterRequest
): Observable<
  ClaimPagedResult<Claim>
> {
  let params =
    new HttpParams()
      .set(
        'pageNumber',
        request.pageNumber
          .toString()
      )
      .set(
        'pageSize',
        request.pageSize
          .toString()
      );


  if (
    request.status !== null &&
    request.status !== undefined
  ) {
    params =
      params.set(
        'status',
        request.status.toString()
      );
  }


  if (
    request.claimType !== null &&
    request.claimType !== undefined
  ) {
    params =
      params.set(
        'claimType',
        request.claimType
          .toString()
      );
  }


  const claimNumber =
    request.claimNumber
      ?.trim();


  if (claimNumber) {
    params =
      params.set(
        'claimNumber',
        claimNumber
      );
  }


  const customerId =
    request.customerId
      ?.trim();


  if (customerId) {
    params =
      params.set(
        'customerId',
        customerId
      );
  }


  const fromDate =
    request.fromDate
      ?.trim();


  if (fromDate) {
    params =
      params.set(
        'fromDate',
        fromDate
      );
  }


  const toDate =
    request.toDate
      ?.trim();


  if (toDate) {
    params =
      params.set(
        'toDate',
        toDate
      );
  }


  return this.http.get<
    ClaimPagedResult<Claim>
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.claims
    ),
    {
      params
    }
  );
}


getClaim(
  claimId: string
): Observable<ClaimDetails> {
  const normalizedClaimId =
    claimId.trim();


  if (!normalizedClaimId) {
    throw new Error(
      'Claim identifier is required.'
    );
  }


  return this.http.get<
    ClaimDetails
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.claim(
        normalizedClaimId
      )
    )
  );
}


getActiveClaimsManagers():
  Observable<ActiveClaimsManager[]> {
  return this.http.get<
    ActiveClaimsManager[]
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS
        .activeClaimsManagers
    )
  );
}


assignClaimsManager(
  claimId: string,
  request:
    AssignClaimsManagerRequest
): Observable<ClaimDetails> {
  const normalizedClaimId =
    claimId.trim();


  if (!normalizedClaimId) {
    throw new Error(
      'Claim identifier is required.'
    );
  }


  const normalizedClaimsManagerId =
    request.claimsManagerId.trim();


  if (!normalizedClaimsManagerId) {
    throw new Error(
      'Claims Manager identifier is required.'
    );
  }


  const normalizedRowVersion =
    request.rowVersion.trim();


  if (!normalizedRowVersion) {
    throw new Error(
      'Claim RowVersion is required.'
    );
  }


  const payload:
    AssignClaimsManagerRequest = {
      claimsManagerId:
        normalizedClaimsManagerId,


      reason:
        request.reason
          ?.trim() ||
        null,


      rowVersion:
        normalizedRowVersion
    };


  return this.http.post<
    ClaimDetails
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS
        .assignClaimsManager(
          normalizedClaimId
        )
    ),
    payload
  );
}


getPolicies(
  request: PolicyFilterRequest
): Observable<
  PolicyPagedResult<PolicySummary>
> {
  let params =
    new HttpParams()
      .set(
        'pageNumber',
        request.pageNumber
          .toString()
      )
      .set(
        'pageSize',
        request.pageSize
          .toString()
      );


  if (
    request.status !== null &&
    request.status !== undefined
  ) {
    params =
      params.set(
        'status',
        request.status.toString()
      );
  }


  if (
    request.productType !== null &&
    request.productType !== undefined
  ) {
    params =
      params.set(
        'productType',
        request.productType.toString()
      );
  }


  const productName =
    request.productName
      ?.trim();


  if (productName) {
    params =
      params.set(
        'productName',
        productName
      );
  }


  return this.http.get<
    PolicyPagedResult<PolicySummary>
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.policies
    ),
    {
      params
    }
  );
}


getPolicy(
  policyId: string
): Observable<PolicyDetails> {
  const normalizedPolicyId =
    policyId.trim();


  if (!normalizedPolicyId) {
    throw new Error(
      'Policy identifier is required.'
    );
  }


  return this.http.get<
    PolicyDetails
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.policy(
        normalizedPolicyId
      )
    )
  );
}

getPolicyDocument(
  policyId: string,
  documentId: string
): Observable<HttpResponse<Blob>> {
  return this.http.get(
    buildApiUrl(
      ADMIN_ENDPOINTS.policyDocument(
        policyId,
        documentId
      )
    ),
    {
      observe: 'response',
      responseType: 'blob'
    }
  );
}

retryPolicyDocument(
  policyId: string
): Observable<PolicyDocument> {
  return this.http.post<PolicyDocument>(
    buildApiUrl(
      ADMIN_ENDPOINTS.retryPolicyDocument(
        policyId
      )
    ),
    null
  );
}







updatePricing(
  request: UpdatePricingRequest
): Observable<void> {
  return this.http.put<void>(
    buildApiUrl(
      ADMIN_ENDPOINTS.pricing
    ),
    request
  );
}


    getProducts():
  Observable<ProductDetails[]> {
  return this.http.get<
    ProductDetails[]
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.products
    )
  );
}


getProduct(
  productId: string
): Observable<ProductDetails> {
  return this.http.get<
    ProductDetails
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.product(
        productId
      )
    )
  );
}


updateProductStatus(
  productId: string,
  request:
    UpdateProductStatusRequest
): Observable<void> {
  return this.http.put<void>(
    buildApiUrl(
      ADMIN_ENDPOINTS
        .updateProductStatus(
          productId
        )
    ),
    request
  );
}


updateCoverageLimit(
  productId: string,
  coverageId: string,
  request:
    UpdateCoverageLimitRequest
): Observable<void> {
  return this.http.put<void>(
    buildApiUrl(
      ADMIN_ENDPOINTS
        .updateCoverageLimit(
          productId,
          coverageId
        )
    ),
    request
  );
}


  getQuotes(
  query: AdminQuoteSearchQuery
): Observable<
  PagedResult<QuoteSummary>
> {
  return this.http.get<
    PagedResult<QuoteSummary>
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.quotes
    ),
    {
      params:
        this.createQuoteParams(
          query
        )
    }
  );
}


getQuote(
  quoteId: string
): Observable<QuoteDetails> {
  return this.http.get<
    QuoteDetails
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.quote(
        quoteId
      )
    )
  );
}


private createQuoteParams(
  query: AdminQuoteSearchQuery
): HttpParams {
  let params =
    new HttpParams()
      .set(
        'pageNumber',
        query.pageNumber
      )
      .set(
        'pageSize',
        query.pageSize
      )
      .set(
        'sortDescending',
        query.sortDescending
      );


  if (query.sortBy?.trim()) {
    params =
      params.set(
        'sortBy',
        query.sortBy.trim()
      );
  }


  if (query.customerId?.trim()) {
    params =
      params.set(
        'customerId',
        query.customerId.trim()
      );
  }


  if (query.propertyId?.trim()) {
    params =
      params.set(
        'propertyId',
        query.propertyId.trim()
      );
  }


  if (query.productId?.trim()) {
    params =
      params.set(
        'productId',
        query.productId.trim()
      );
  }


  if (
    query.status !== undefined
  ) {
    params =
      params.set(
        'status',
        query.status
      );
  }


  if (query.generatedFromUtc) {
    params =
      params.set(
        'generatedFromUtc',
        query.generatedFromUtc
      );
  }


  if (query.generatedToUtc) {
    params =
      params.set(
        'generatedToUtc',
        query.generatedToUtc
      );
  }


  if (query.decisionFromUtc) {
    params =
      params.set(
        'decisionFromUtc',
        query.decisionFromUtc
      );
  }


  if (query.decisionToUtc) {
    params =
      params.set(
        'decisionToUtc',
        query.decisionToUtc
      );
  }


  return params;
}


approveQuote(
  quoteId: string,
  request: ApproveQuoteRequest
): Observable<QuoteDetails> {
  return this.http.post<
    QuoteDetails
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.approveQuote(
        quoteId
      )
    ),
    request
  );
}


rejectQuote(
  quoteId: string,
  request: RejectQuoteRequest
): Observable<QuoteDetails> {
  return this.http.post<
    QuoteDetails
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.rejectQuote(
        quoteId
      )
    ),
    request
  );
}


invalidateQuote(
  quoteId: string,
  request: InvalidateQuoteRequest
): Observable<QuoteDetails> {
  return this.http.post<
    QuoteDetails
  >(
    buildApiUrl(
      ADMIN_ENDPOINTS.invalidateQuote(
        quoteId
      )
    ),
    request
  );
}  


  getPendingClaimsManagers():
    Observable<
      PendingClaimsManagerResponse[]
    > {
    return this.http.get<
      PendingClaimsManagerResponse[]
    >(
      buildApiUrl(
        ADMIN_ENDPOINTS
          .pendingClaimsManagers
      )
    );
  }


  approveClaimsManager(
    applicationId: string
  ): Observable<void> {
    return this.http.put<void>(
      buildApiUrl(
        ADMIN_ENDPOINTS
          .approveClaimsManager(
            applicationId
          )
      ),
      null
    );
  }


  rejectClaimsManager(
    applicationId: string,
    request:
      RejectClaimsManagerRequest
  ): Observable<void> {
    return this.http.put<void>(
      buildApiUrl(
        ADMIN_ENDPOINTS
          .rejectClaimsManager(
            applicationId
          )
      ),
      request
    );
  }


  getClaimsManagerDocument(
    applicationId: string
  ): Observable<
    HttpResponse<Blob>
  > {
    return this.http.get(
      buildApiUrl(
        ADMIN_ENDPOINTS
          .claimsManagerDocument(
            applicationId
          )
      ),
      {
        observe: 'response',
        responseType: 'blob'
      }
    );
  }


  updateUserStatus(
    userId: string,
    request:
      UpdateUserStatusRequest
  ): Observable<void> {
    return this.http.put<void>(
      buildApiUrl(
        ADMIN_ENDPOINTS
          .updateUserStatus(
            userId
          )
      ),
      request
    );
  }


  getCustomers(
    query: AdminCustomerQuery
  ): Observable<
    PagedResult<
      AdminCustomerSummary
    >
  > {
    return this.http.get<
      PagedResult<
        AdminCustomerSummary
      >
    >(
      buildApiUrl(
        ADMIN_ENDPOINTS.customers
      ),
      {
        params:
          this.createCustomerParams(
            query
          )
      }
    );
  }


  getCustomer(
    customerId: string
  ): Observable<
    AdminCustomerDetails
  > {
    return this.http.get<
      AdminCustomerDetails
    >(
      buildApiUrl(
        ADMIN_ENDPOINTS.customer(
          customerId
        )
      )
    );
  }


  getCustomerProperties(
    customerId: string,
    query:
      AdminCustomerPropertyQuery
  ): Observable<
    PagedResult<
      VacationProperty
    >
  > {
    return this.http.get<
      PagedResult<
        VacationProperty
      >
    >(
      buildApiUrl(
        ADMIN_ENDPOINTS
          .customerProperties(
            customerId
          )
      ),
      {
        params:
          this.createPropertyParams(
            query
          )
      }
    );
  }


  private createCustomerParams(
    query: AdminCustomerQuery
  ): HttpParams {
    let params =
      new HttpParams()
        .set(
          'pageNumber',
          query.pageNumber
        )
        .set(
          'pageSize',
          query.pageSize
        )
        .set(
          'sortBy',
          query.sortBy
        )
        .set(
          'sortDirection',
          query.sortDirection
        );


    const name =
      query.name?.trim();


    if (name) {
      params =
        params.set(
          'name',
          name
        );
    }


    if (
      query.status !==
      undefined
    ) {
      params =
        params.set(
          'status',
          query.status
        );
    }


    return params;
  }


  private createPropertyParams(
    query:
      AdminCustomerPropertyQuery
  ): HttpParams {
    return new HttpParams()
      .set(
        'pageNumber',
        query.pageNumber
      )
      .set(
        'pageSize',
        query.pageSize
      )
      .set(
        'includeArchived',
        query.includeArchived
      )
      .set(
        'sortBy',
        query.sortBy
      )
      .set(
        'sortDirection',
        query.sortDirection
      );
  }
}
