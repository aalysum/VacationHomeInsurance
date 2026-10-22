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
  PagedResult
} from '../models/admin/paged-result.model';


import {
  AcceptQuoteRequest
} from '../models/customer/quotes/accept-quote-request.model';


import {
  GenerateQuoteRequest
} from '../models/customer/quotes/generate-quote-request.model';


import {
  ProductDetails
} from '../models/customer/quotes/product-details.model';


import {
  QuoteDetails
} from '../models/customer/quotes/quote-details.model';


import {
  QuoteSearchRequest
} from '../models/customer/quotes/quote-search-request.model';


import {
  QuoteSummary
} from '../models/customer/quotes/quote-summary.model';


import {
  WithdrawQuoteRequest
} from '../models/customer/quotes/withdraw-quote-request.model';


import {
  parseApiError
} from '../utils/api-error.util';
import { buildApiUrl } from '../config/api-url.util';
import { GlobalNotificationService } from './global-notification.service';


const EMPTY_QUOTE_RESULT:
  PagedResult<QuoteSummary> = {
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  };


@Injectable({
  providedIn:
    'root'
})
export class CustomerQuoteService {
  private readonly httpClient =
    inject(HttpClient);


  private readonly notifications =
    inject(GlobalNotificationService);


  private readonly productsState =
    signal<ProductDetails[]>([]);


  private readonly selectedProductState =
    signal<ProductDetails | null>(
      null
    );


  private readonly quotesState =
    signal<
      PagedResult<QuoteSummary>
    >(
      EMPTY_QUOTE_RESULT
    );


  private readonly selectedQuoteState =
    signal<QuoteDetails | null>(
      null
    );


  private readonly productsLoadingState =
    signal(false);


  private readonly quotesLoadingState =
    signal(false);


  private readonly quoteDetailsLoadingState =
    signal(false);


  private readonly quoteProcessingState =
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
    signal<QuoteSearchRequest>({
      pageNumber:
        1,


      pageSize:
        10,


      propertyId:
        null,


      productId:
        null,


      status:
        null,


      generatedFromUtc:
        null,


      generatedToUtc:
        null,


      customerAccepted:
        null,


      adminDecision:
        null,


      paymentEligible:
        null
    });


  readonly products =
    this.productsState.asReadonly();


  readonly selectedProduct =
    this.selectedProductState.asReadonly();


  readonly quotes =
    this.quotesState.asReadonly();


  readonly selectedQuote =
    this.selectedQuoteState.asReadonly();


  readonly productsLoading =
    this.productsLoadingState.asReadonly();


  readonly quotesLoading =
    this.quotesLoadingState.asReadonly();


  readonly quoteDetailsLoading =
    this.quoteDetailsLoadingState.asReadonly();


  readonly quoteProcessing =
    this.quoteProcessingState.asReadonly();


  readonly errorMessage =
    this.errorMessageState.asReadonly();


  readonly successMessage =
    this.successMessageState.asReadonly();


  readonly currentSearch =
    this.currentSearchState.asReadonly();


  readonly quoteItems =
    computed(
      () =>
        this.quotesState().items
    );


  readonly pageNumber =
    computed(
      () =>
        this.quotesState().pageNumber
    );


  readonly pageSize =
    computed(
      () =>
        this.quotesState().pageSize
    );


  readonly totalCount =
    computed(
      () =>
        this.quotesState().totalCount
    );


  readonly totalPages =
    computed(
      () =>
        this.quotesState().totalPages
    );


  readonly hasPreviousPage =
    computed(
      () =>
        this.quotesState()
          .hasPreviousPage
    );


  readonly hasNextPage =
    computed(
      () =>
        this.quotesState()
          .hasNextPage
    );


  loadProducts(): void {
    if (
      this.productsLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.productsLoadingState.set(
      true
    );


    this.httpClient
      .get<ProductDetails[]>(
        buildApiUrl('/products')
      )
      .pipe(
        finalize(() => {
          this.productsLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: products => {
          this.productsState.set(
            products ?? []
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }


  loadProduct(
    productId: string
  ): void {
    const normalizedProductId =
      productId.trim();


    if (
      !normalizedProductId ||
      this.productsLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.selectedProductState.set(
      null
    );


    this.productsLoadingState.set(
      true
    );


    this.httpClient
      .get<ProductDetails>(
        buildApiUrl(`/products/${normalizedProductId}`)
      )
      .pipe(
        finalize(() => {
          this.productsLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: product => {
          this.selectedProductState.set(
            product
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }


  loadQuotes(
    request:
      QuoteSearchRequest =
        this.currentSearchState()
  ): void {
    if (
      this.quotesLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.currentSearchState.set(
      request
    );


    this.quotesLoadingState.set(
      true
    );


    const params =
      this.buildSearchParameters(
        request
      );


    this.httpClient
      .get<
        PagedResult<QuoteSummary>
      >(
        buildApiUrl('/quotes'),
        {
          params
        }
      )
      .pipe(
        finalize(() => {
          this.quotesLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: result => {
          this.quotesState.set({
            items:
              result.items ?? [],


            pageNumber:
              result.pageNumber,


            pageSize:
              result.pageSize,


            totalCount:
              result.totalCount,


            totalPages:
              result.totalPages,


            hasPreviousPage:
              result.hasPreviousPage,


            hasNextPage:
              result.hasNextPage
          });
        },


        error: error => {
          this.quotesState.set(
            EMPTY_QUOTE_RESULT
          );


          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
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


    this.clearError();


    this.selectedQuoteState.set(
      null
    );


    this.quoteDetailsLoadingState.set(
      true
    );


    this.httpClient
      .get<QuoteDetails>(
        buildApiUrl(`/quotes/${normalizedQuoteId}`)
      )
      .pipe(
        finalize(() => {
          this.quoteDetailsLoadingState.set(
            false
          );
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
            parseApiError(error).message
          );
        }
      });
  }


  generateQuote(
    request:
      GenerateQuoteRequest
  ): void {
    if (
      this.quoteProcessingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.quoteProcessingState.set(
      true
    );


    this.httpClient
      .post<QuoteDetails>(
        buildApiUrl('/quotes'),
        request
      )
      .pipe(
        finalize(() => {
          this.quoteProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: quote => {
          this.selectedQuoteState.set(
            quote
          );


          this.notifications.showSuccess(
            'Quote generated successfully.'
          );


          this.loadQuotes();
        },


        error: error => {
          this.clearError();
          this.notifications.showError(
            parseApiError(error).message
          );
        }
      });
  }


  acceptQuote(
    quoteId: string,
    rowVersion: string
  ): void {
    const normalizedQuoteId =
      quoteId.trim();


    if (
      !normalizedQuoteId ||
      this.quoteProcessingState()
    ) {
      return;
    }


    const request:
      AcceptQuoteRequest = {
        rowVersion
      };


    this.executeQuoteAction(
      normalizedQuoteId,
      'accept',
      request,
      'Approved quote accepted. You can now continue to payment.'
    );
  }


  withdrawQuote(
    quoteId: string,
    rowVersion: string,
    reason: string | null
  ): void {
    const normalizedQuoteId =
      quoteId.trim();


    if (
      !normalizedQuoteId ||
      this.quoteProcessingState()
    ) {
      return;
    }


    const request:
      WithdrawQuoteRequest = {
        reason:
          reason?.trim() ||
          null,


        rowVersion
      };


    this.executeQuoteAction(
      normalizedQuoteId,
      'withdraw',
      request,
      'Quote withdrawn successfully.'
    );
  }


  refreshQuotes(): void {
    this.loadQuotes(
      this.currentSearchState()
    );
  }


  goToPreviousPage(): void {
    const result =
      this.quotesState();


    if (
      !result.hasPreviousPage ||
      this.quotesLoadingState()
    ) {
      return;
    }


    this.loadQuotes({
      ...this.currentSearchState(),


      pageNumber:
        result.pageNumber - 1
    });
  }


  goToNextPage(): void {
    const result =
      this.quotesState();


    if (
      !result.hasNextPage ||
      this.quotesLoadingState()
    ) {
      return;
    }


    this.loadQuotes({
      ...this.currentSearchState(),


      pageNumber:
        result.pageNumber + 1
    });
  }


  clearSelectedProduct(): void {
    this.selectedProductState.set(
      null
    );
  }


  clearSelectedQuote(): void {
    this.selectedQuoteState.set(
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


  private executeQuoteAction(
    quoteId: string,
    action: 'accept' | 'withdraw',
    request:
      AcceptQuoteRequest |
      WithdrawQuoteRequest,
    successMessage: string
  ): void {
    this.clearError();
    this.clearSuccessMessage();


    this.quoteProcessingState.set(
      true
    );


    this.httpClient
      .post<QuoteDetails>(
        buildApiUrl(`/quotes/${quoteId}/${action}`),
        request
      )
      .pipe(
        finalize(() => {
          this.quoteProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: quote => {
          this.selectedQuoteState.set(
            quote
          );


          this.notifications.showSuccess(
            successMessage
          );


          this.refreshQuotes();
        },


        error: error => {
          this.clearError();
          this.notifications.showError(
            parseApiError(error).message
          );
        }
      });
  }


  private buildSearchParameters(
    request:
      QuoteSearchRequest
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


    if (request.propertyId) {
      params =
        params.set(
          'propertyId',
          request.propertyId
        );
    }


    if (request.productId) {
      params =
        params.set(
          'productId',
          request.productId
        );
    }


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


    if (request.generatedFromUtc) {
      params =
        params.set(
          'generatedFromUtc',
          request.generatedFromUtc
        );
    }


    if (request.generatedToUtc) {
      params =
        params.set(
          'generatedToUtc',
          request.generatedToUtc
        );
    }


    if (
      request.customerAccepted !==
      null
    ) {
      params =
        params.set(
          'customerAccepted',
          String(
            request.customerAccepted
          )
        );
    }


    if (
      request.adminDecision !==
      null
    ) {
      params =
        params.set(
          'adminDecision',
          String(
            request.adminDecision
          )
        );
    }


    if (
      request.paymentEligible !==
      null
    ) {
      params =
        params.set(
          'paymentEligible',
          String(
            request.paymentEligible
          )
        );
    }


    return params;
  }
}
