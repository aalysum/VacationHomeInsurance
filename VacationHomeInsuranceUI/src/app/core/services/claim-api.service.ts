import {
  HttpClient,
  HttpParams
} from '@angular/common/http';


import {
  inject,
  Injectable
} from '@angular/core';


import {
  Observable
} from 'rxjs';


import {
  CLAIM_ENDPOINTS
} from '../constants/claim-endpoints.constants';


import {
  ClaimDetails
} from '../models/customer/claims/claim-details.model';


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
  WithdrawClaimRequest
} from '../models/customer/claims/withdraw-claim-request.model';


import {
  buildApiUrl
} from '../utils/api-url.util';


@Injectable({
  providedIn: 'root'
})
export class ClaimApiService {
  private readonly http =
    inject(HttpClient);


  createClaim(
    request: CreateClaimRequest
  ): Observable<ClaimDetails> {
    return this.http.post<
      ClaimDetails
    >(
      buildApiUrl(
        CLAIM_ENDPOINTS.claims
      ),
      request
    );
  }


  searchClaims(
    request: ClaimSearchRequest
  ): Observable<
    ClaimPagedResult<ClaimSummary>
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


    return this.http.get<
      ClaimPagedResult<ClaimSummary>
    >(
      buildApiUrl(
        CLAIM_ENDPOINTS.claims
      ),
      {
        params
      }
    );
  }


  getClaim(
    claimId: string
  ): Observable<ClaimDetails> {
    return this.http.get<
      ClaimDetails
    >(
      buildApiUrl(
        CLAIM_ENDPOINTS.claim(
          claimId
        )
      )
    );
  }


  updateClaim(
    claimId: string,
    request: UpdateClaimRequest
  ): Observable<void> {
    return this.http.put<void>(
      buildApiUrl(
        CLAIM_ENDPOINTS.claim(
          claimId
        )
      ),
      request
    );
  }


  withdrawClaim(
    claimId: string,
    request:
      WithdrawClaimRequest
  ): Observable<void> {
    return this.http.post<void>(
      buildApiUrl(
        CLAIM_ENDPOINTS.withdraw(
          claimId
        )
      ),
      request
    );
  }
}
