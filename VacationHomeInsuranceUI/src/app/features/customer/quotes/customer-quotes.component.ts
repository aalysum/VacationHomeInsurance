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
  PaymentRequest
} from '../../../core/models/customer/policies/payment-request.model';


import {
  AdminDecision
} from '../../../core/models/customer/quotes/admin-decision.enum';


import {
  CoverageType
} from '../../../core/models/customer/quotes/coverage-type.enum';


import {
  GenerateQuoteRequest
} from '../../../core/models/customer/quotes/generate-quote-request.model';


import {
  ProductType
} from '../../../core/models/customer/quotes/product-type.enum';


import {
  QuoteStatus
} from '../../../core/models/customer/quotes/quote-status.enum';


import {
  QuoteSummary
} from '../../../core/models/customer/quotes/quote-summary.model';


import {
  RiskLevel
} from '../../../core/models/customer/quotes/risk-level.enum';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  CustomerPolicyService
} from '../../../core/services/customer-policy.service';


import {
  CustomerQuoteService
} from '../../../core/services/customer-quote.service';


import {
  CustomerService
} from '../../../core/services/customer.service';


type QuoteDialogMode =
  | 'generate'
  | 'details'
  | 'withdraw'
  | 'payment'
  | 'payment-result'
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
    'app-customer-quotes',


  standalone:
    true,


  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],


  templateUrl:
    './customer-quotes.component.html',


  styleUrl:
    './customer-quotes.component.css'
})
export class CustomerQuotesComponent
  implements OnInit, OnDestroy {
  private readonly quoteService =
    inject(CustomerQuoteService);


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


  private readonly convertedQuoteStorageKey =
    'customer-converted-quote-ids';


  private readonly convertedQuoteIds =
    signal<ReadonlySet<string>>(
      this.loadConvertedQuoteIds()
    );


  readonly QuoteStatus =
    QuoteStatus;


  readonly CoverageType =
    CoverageType;


  readonly RiskLevel =
    RiskLevel;


  readonly ProductType =
    ProductType;


  readonly AdminDecision =
    AdminDecision;


  readonly dialogMode =
    signal<QuoteDialogMode>(
      null
    );


  readonly selectedOptionalCoverageIds =
    signal<string[]>([]);


  readonly products =
    this.quoteService.products;


  readonly quotes =
    this.quoteService.quotes;


  readonly quoteItems =
    this.quoteService.quoteItems;


  readonly selectedQuote =
    this.quoteService.selectedQuote;


  readonly selectedProduct =
    this.quoteService.selectedProduct;


  readonly productsLoading =
    this.quoteService.productsLoading;


  readonly quotesLoading =
    this.quoteService.quotesLoading;


  readonly quoteDetailsLoading =
    this.quoteService.quoteDetailsLoading;


  readonly quoteProcessing =
    this.quoteService.quoteProcessing;


  readonly errorMessage =
    this.quoteService.errorMessage;


  readonly successMessage =
    this.quoteService.successMessage;


  readonly paymentLoading =
    this.policyService.paymentLoading;


  readonly paymentResult =
    this.policyService.paymentResult;


  readonly paymentErrorMessage =
    this.policyService.errorMessage;


  readonly paymentSuccessMessage =
    this.policyService.successMessage;


  readonly properties =
    this.customerService.properties;


  readonly propertyItems =
    computed(() => {
      return (
        this.properties().items ?? []
      ).filter(
        property =>
          !property.isArchived
      );
    });


  readonly propertiesLoading =
    this.customerService.propertiesLoading;


  readonly pageNumber =
    this.quoteService.pageNumber;


  readonly pageSize =
    this.quoteService.pageSize;


  readonly totalCount =
    this.quoteService.totalCount;


  readonly totalPages =
    this.quoteService.totalPages;


  readonly hasPreviousPage =
    this.quoteService.hasPreviousPage;


  readonly hasNextPage =
    this.quoteService.hasNextPage;


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


  readonly activeProducts =
    computed(() => {
      return this.products()
        .filter(
          product =>
            product.isActive
        );
    });


  readonly mandatoryCoverages =
    computed(() => {
      return (
        this.selectedProduct()
          ?.coverages ??
        []
      )
        .filter(
          coverage =>
            coverage.isActive &&
            coverage.coverageType ===
              CoverageType.Mandatory
        )
        .sort(
          (
            first,
            second
          ) =>
            first.displayOrder -
            second.displayOrder
        );
    });


  readonly optionalCoverages =
    computed(() => {
      return (
        this.selectedProduct()
          ?.coverages ??
        []
      )
        .filter(
          coverage =>
            coverage.isActive &&
            coverage.coverageType ===
              CoverageType.Optional
        )
        .sort(
          (
            first,
            second
          ) =>
            first.displayOrder -
            second.displayOrder
        );
    });


  readonly generationForm =
    this.formBuilder.nonNullable.group({
      propertyId: [
        ''
      ],


      productId: [
        ''
      ]
    });


  readonly filterForm =
    this.formBuilder.nonNullable.group({
      propertyId: [
        ''
      ],


      productId: [
        ''
      ],


      status: [
        ''
      ],


      pageSize: [
        10
      ]
    });


  readonly withdrawalForm =
    this.formBuilder.nonNullable.group({
      reason: [
        ''
      ]
    });


  readonly paymentForm =
    this.formBuilder.nonNullable.group({
      cardholderName: [
        '',
        [
          Validators.required,
          Validators.maxLength(
            200
          )
        ]
      ],


      cardNumber: [
        '',
        [
          Validators.required,
          Validators.pattern(
            /^\d{12,19}$/
          )
        ]
      ],


      expiryMonth: [
        new Date().getMonth() + 1,
        [
          Validators.required,
          Validators.min(
            1
          ),
          Validators.max(
            12
          )
        ]
      ],


      expiryYear: [
        new Date().getFullYear(),
        [
          Validators.required,
          Validators.min(
            new Date().getFullYear()
          ),
          Validators.max(
            new Date().getFullYear() +
              20
          )
        ]
      ],


      cvv: [
        '',
        [
          Validators.required,
          Validators.pattern(
            /^\d{3,4}$/
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
    this.quoteService
      .clearError();


    this.quoteService
      .clearSuccessMessage();


    this.policyService
      .clearError();


    this.policyService
      .clearSuccessMessage();


    this.quoteService
      .loadProducts();


    this.customerService
      .loadProfile();


    this.customerService.loadProperties({
      pageNumber:
        1,


      pageSize:
        50,


      includeArchived:
        false,


      sortBy:
        'createdAtUtc',


      sortDirection:
        'desc'
    });


    this.loadQuotes();


    const action =
      this.activatedRoute
        .snapshot
        .queryParamMap
        .get(
          'action'
        );


    if (action === 'generate') {
      this.openGenerateQuote();
    }


    const quoteId =
      this.activatedRoute
        .snapshot
        .queryParamMap
        .get(
          'quoteId'
        )
        ?.trim();


    if (quoteId) {
      this.openQuoteById(
        quoteId
      );
    }
  }


  ngOnDestroy(): void {
    this.quoteService
      .clearSelectedProduct();


    this.quoteService
      .clearSelectedQuote();


    this.quoteService
      .clearError();


    this.quoteService
      .clearSuccessMessage();


    this.policyService
      .clearSelectedPayment();


    this.policyService
      .clearError();


    this.policyService
      .clearSuccessMessage();


    this.clearPaymentForm();
  }


  loadQuotes(): void {
    this.quoteService.loadQuotes({
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
  }


  applyFilters(): void {
    if (this.quotesLoading()) {
      return;
    }


    const values =
      this.filterForm.getRawValue();


    const statusValue =
      String(
        values.status
      ).trim();


    this.quoteService.loadQuotes({
      pageNumber:
        1,


      pageSize:
        Number(
          values.pageSize
        ) as 10 | 25 | 50,


      propertyId:
        values.propertyId.trim() ||
        null,


      productId:
        values.productId.trim() ||
        null,


      status:
        statusValue
          ? Number(
              statusValue
            ) as QuoteStatus
          : null,


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
  }


  clearFilters(): void {
    this.filterForm.reset({
      propertyId:
        '',


      productId:
        '',


      status:
        '',


      pageSize:
        10
    });


    this.loadQuotes();
  }


  refreshQuotes(): void {
    if (
      this.quotesLoading() ||
      this.quoteProcessing()
    ) {
      return;
    }


    this.quoteService
      .refreshQuotes();
  }


  openGenerateQuote(): void {
    if (
      this.quoteProcessing()
    ) {
      return;
    }


    this.generationForm.reset({
      propertyId:
        '',


      productId:
        ''
    });


    this.selectedOptionalCoverageIds.set(
      []
    );


    this.quoteService
      .clearSelectedProduct();


    this.dialogMode.set(
      'generate'
    );


    this.updateQueryParameters({
      action:
        'generate',


      quoteId:
        null
    });
  }


  productChanged(): void {
    const productId =
      this.generationForm
        .controls
        .productId
        .value
        .trim();


    this.selectedOptionalCoverageIds.set(
      []
    );


    if (!productId) {
      this.quoteService
        .clearSelectedProduct();


      return;
    }


    const product =
      this.products()
        .find(
          item =>
            item.id === productId
        );


    if (!product) {
      return;
    }


    this.quoteService
      .clearSelectedProduct();


    this.quoteService.loadProduct(
      product.id
    );
  }


  toggleOptionalCoverage(
    coverageId:
      string,
    event:
      Event
  ): void {
    const input =
      event.target as HTMLInputElement;


    const selectedIds =
      this.selectedOptionalCoverageIds();


    if (input.checked) {
      this.selectedOptionalCoverageIds.set(
        [
          ...selectedIds,
          coverageId
        ]
      );


      return;
    }


    this.selectedOptionalCoverageIds.set(
      selectedIds.filter(
        id =>
          id !== coverageId
      )
    );
  }


  generateQuote(): void {
    const values =
      this.generationForm
        .getRawValue();


    if (
      !values.propertyId.trim() ||
      !values.productId.trim() ||
      this.quoteProcessing()
    ) {
      return;
    }


    const request:
      GenerateQuoteRequest = {
        propertyId:
          values.propertyId.trim(),


        productId:
          values.productId.trim(),


        selectedOptionalCoverageIds:
          this.selectedOptionalCoverageIds()
      };


    this.quoteService.generateQuote(
      request
    );


    this.dialogMode.set(
      'details'
    );


    this.updateQueryParameters({
      action:
        null
    });
  }


  openQuote(
    quote:
      QuoteSummary
  ): void {
    const quoteId =
      quote.id?.trim();


    if (!quoteId) {
      return;
    }


    this.openQuoteById(
      quoteId
    );
  }


  acceptQuote(): void {
    const quote =
      this.selectedQuote();


    if (
      !quote ||
      !this.canAcceptQuote() ||
      this.quoteProcessing()
    ) {
      return;
    }


    this.quoteService.acceptQuote(
      quote.id,
      quote.rowVersion
    );
  }


  openWithdrawQuote(): void {
    if (
      !this.canWithdrawQuote()
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
    const quote =
      this.selectedQuote();


    if (
      !quote ||
      !this.canWithdrawQuote() ||
      this.quoteProcessing()
    ) {
      return;
    }


    this.quoteService.withdrawQuote(
      quote.id,
      quote.rowVersion,
      this.withdrawalForm
        .controls
        .reason
        .value
    );


    this.dialogMode.set(
      'details'
    );
  }


  canAcceptQuote(): boolean {
    const quote =
      this.selectedQuote();


    if (!quote) {
      return false;
    }


    if (
      this.isQuoteConverted(
        quote.id
      )
    ) {
      return false;
    }


    const expiresAt =
      new Date(
        quote.expiresAtUtc
      ).getTime();


    const acceptedAfterApproval =
      !!quote.customerAcceptedAtUtc &&
      !!quote.adminDecisionAtUtc &&
      new Date(quote.customerAcceptedAtUtc).getTime() >
        new Date(quote.adminDecisionAtUtc).getTime();
    const paymentDeadline =
      quote.paymentDeadlineUtc
        ? new Date(quote.paymentDeadlineUtc).getTime()
        : null;

    return (
      quote.status ===
        QuoteStatus.AdminApproved &&
      !acceptedAfterApproval &&
      (paymentDeadline === null ||
        (!Number.isNaN(paymentDeadline) &&
          paymentDeadline > Date.now())) &&
      !Number.isNaN(
        expiresAt
      ) &&
      expiresAt >
        Date.now()
    );
  }


  canWithdrawQuote(): boolean {
    const quote =
      this.selectedQuote();


    if (!quote) {
      return false;
    }


    return (
      !this.isQuoteConverted(
        quote.id
      ) &&
      quote.status ===
        QuoteStatus.Active
    );
  }


  canPayQuote(): boolean {
    const quote =
      this.selectedQuote();


    if (!quote) {
      return false;
    }


    if (
      this.isQuoteConverted(
        quote.id
      )
    ) {
      return false;
    }


    if (
      quote.status !==
        QuoteStatus.AdminApproved ||
      quote.paymentEligible !== true
    ) {
      return false;
    }


    const now =
      Date.now();


    const quoteExpiry =
      new Date(
        quote.expiresAtUtc
      ).getTime();


    if (
      Number.isNaN(
        quoteExpiry
      ) ||
      quoteExpiry <= now
    ) {
      return false;
    }


    if (quote.paymentDeadlineUtc) {
      const paymentDeadline =
        new Date(
          quote.paymentDeadlineUtc
        ).getTime();


      if (
        Number.isNaN(
          paymentDeadline
        ) ||
        paymentDeadline <= now
      ) {
        return false;
      }
    }


    return true;
  }


  openPayment(): void {
    if (
      !this.canPayQuote() ||
      this.paymentLoading()
    ) {
      return;
    }


    this.policyService
      .clearError();


    this.policyService
      .clearSuccessMessage();


    this.policyService
      .clearSelectedPayment();


    this.paymentForm.reset({
      cardholderName:
        this.displayName(),


      cardNumber:
        '',


      expiryMonth:
        new Date().getMonth() + 1,


      expiryYear:
        new Date().getFullYear(),


      cvv:
        ''
    });


    this.dialogMode.set(
      'payment'
    );
  }


  submitPayment(): void {
    this.paymentForm
      .markAllAsTouched();


    const quote =
      this.selectedQuote();


    if (
      !quote ||
      !this.canPayQuote() ||
      this.paymentForm.invalid ||
      this.paymentLoading()
    ) {
      return;
    }


    const values =
      this.paymentForm
        .getRawValue();


    const cardholderName =
      values.cardholderName
        .trim();


    const cardNumber =
      values.cardNumber
        .replace(
          /\s+/g,
          ''
        );


    const cvv =
      values.cvv.trim();


    const expiryMonth =
      Number(
        values.expiryMonth
      );


    const expiryYear =
      Number(
        values.expiryYear
      );


    if (
      !this.isCardExpiryValid(
        expiryMonth,
        expiryYear
      )
    ) {
      this.paymentForm
        .controls
        .expiryMonth
        .setErrors({
          expired:
            true
        });


      this.paymentForm
        .controls
        .expiryYear
        .setErrors({
          expired:
            true
        });


      return;
    }


    const request:
      PaymentRequest = {
        quoteId:
          quote.id,


        cardholderName,


        cardNumber,


        expiryMonth,


        expiryYear,


        cvv,


        /*
         * The Customer must never enter or alter
         * the amount manually.
         */
        amount:
          quote.finalAnnualPremium
      };


    this.policyService
      .clearError();


    this.policyService
      .clearSuccessMessage();


    this.policyService
      .processPayment(
        request
      )
      .subscribe({
        next: () => {
          /*
           * Mark the quote locally before refreshing.
           * This immediately hides payment-related UI
           * and changes its effective status.
           */
          this.markQuoteAsConverted(
            quote.id
          );


          this.clearPaymentForm();


          this.dialogMode.set(
            'payment-result'
          );


          this.quoteService
            .refreshQuotes();


          this.policyService
            .refreshPolicies();
        },


        error: () => {
          this.dialogMode.set(
            'payment'
          );
        }
      });
  }


  openPaymentPolicy(): void {
    const result =
      this.paymentResult();


    const policyId =
      result?.policyId?.trim();


    if (!policyId) {
      return;
    }


    this.clearPaymentForm();


    this.policyService
      .clearSelectedPayment();


    this.dialogMode.set(
      null
    );


    this.updateQueryParameters({
      quoteId:
        null,


      action:
        null
    });


    void this.router.navigate(
      [
        '/customer/policies'
      ],
      {
        queryParams: {
          policyId
        }
      }
    );
  }


  returnToQuoteDetails(): void {
    if (
      this.quoteProcessing() ||
      this.paymentLoading()
    ) {
      return;
    }


    this.policyService
      .clearSelectedPayment();


    this.policyService
      .clearError();


    this.policyService
      .clearSuccessMessage();


    this.clearPaymentForm();


    this.dialogMode.set(
      'details'
    );
  }


  closeDialog(): void {
    if (
      this.quoteProcessing() ||
      this.paymentLoading()
    ) {
      return;
    }


    this.dialogMode.set(
      null
    );


    this.selectedOptionalCoverageIds.set(
      []
    );


    this.clearPaymentForm();


    this.policyService
      .clearSelectedPayment();


    this.policyService
      .clearError();


    this.policyService
      .clearSuccessMessage();


    this.quoteService
      .clearSelectedProduct();


    this.quoteService
      .clearSelectedQuote();


    this.updateQueryParameters({
      action:
        null,


      quoteId:
        null
    });
  }


  previousPage(): void {
    this.quoteService
      .goToPreviousPage();
  }


  nextPage(): void {
    this.quoteService
      .goToNextPage();
  }


  isQuoteConverted(
    quoteId:
      string | null | undefined
  ): boolean {
    const normalizedQuoteId =
      this.normalizeQuoteId(
        quoteId
      );


    if (!normalizedQuoteId) {
      return false;
    }


    return this.convertedQuoteIds()
      .has(
        normalizedQuoteId
      );
  }


  effectiveQuoteStatus(
    quoteId:
      string | null | undefined,
    status:
      QuoteStatus
  ): QuoteStatus {
    if (
      status ===
        QuoteStatus.ConvertedToPolicy ||
      this.isQuoteConverted(
        quoteId
      )
    ) {
      return QuoteStatus
        .ConvertedToPolicy;
    }


    return status;
  }


  quoteStatusLabel(
    status:
      QuoteStatus
  ): string {
    switch (status) {
      case QuoteStatus.Active:
        return 'Active';


      case QuoteStatus
        .PendingAdminApproval:
        return 'Pending Admin Approval';


      case QuoteStatus.AdminApproved:
        return 'Admin Approved';


      case QuoteStatus.AdminRejected:
        return 'Admin Rejected';


      case QuoteStatus.Expired:
        return 'Expired';


      case QuoteStatus.Invalidated:
        return 'Invalidated';


      case QuoteStatus.Withdrawn:
        return 'Withdrawn';


      case QuoteStatus
        .ConvertedToPolicy:
        return 'Converted to Policy';


      default:
        return 'Unknown';
    }
  }


  quoteStatusClass(
    status:
      QuoteStatus
  ): string {
    switch (status) {
      case QuoteStatus.Active:
        return 'status-active';


      case QuoteStatus
        .PendingAdminApproval:
        return 'status-pending';


      case QuoteStatus.AdminApproved:
        return 'status-approved';


      case QuoteStatus.AdminRejected:
        return 'status-rejected';


      case QuoteStatus.Expired:
        return 'status-expired';


      case QuoteStatus.Invalidated:
        return 'status-invalidated';


      case QuoteStatus.Withdrawn:
        return 'status-withdrawn';


      case QuoteStatus
        .ConvertedToPolicy:
        return 'status-converted';


      default:
        return 'status-neutral';
    }
  }


  productTypeLabel(
    type:
      ProductType
  ): string {
    switch (type) {
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


  riskLevelLabel(
    riskLevel:
      RiskLevel
  ): string {
    switch (riskLevel) {
      case RiskLevel.Low:
        return 'Low';


      case RiskLevel.Medium:
        return 'Medium';


      case RiskLevel.High:
        return 'High';


      default:
        return 'Unknown';
    }
  }


  coverageTypeLabel(
    coverageType:
      CoverageType
  ): string {
    return (
      coverageType ===
      CoverageType.Mandatory
    )
      ? 'Mandatory'
      : 'Optional';
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


    return (
      profile.firstName
        .charAt(0) +
      profile.lastName
        .charAt(0)
    ).toUpperCase() || 'CU';
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
    this.quoteService
      .clearError();
  }


  clearSuccessMessage(): void {
    this.quoteService
      .clearSuccessMessage();
  }


  clearPaymentError(): void {
    this.policyService
      .clearError();
  }


  clearPaymentSuccessMessage(): void {
    this.policyService
      .clearSuccessMessage();
  }


  logout(): void {
    this.authService.logout();
  }


  private openQuoteById(
    quoteId:
      string
  ): void {
    this.quoteService.loadQuote(
      quoteId
    );


    this.dialogMode.set(
      'details'
    );


    this.updateQueryParameters({
      quoteId,


      action:
        null
    });
  }


  private isCardExpiryValid(
    expiryMonth:
      number,
    expiryYear:
      number
  ): boolean {
    if (
      !Number.isInteger(
        expiryMonth
      ) ||
      expiryMonth < 1 ||
      expiryMonth > 12 ||
      !Number.isInteger(
        expiryYear
      )
    ) {
      return false;
    }


    const now =
      new Date();


    const currentMonth =
      now.getMonth() + 1;


    const currentYear =
      now.getFullYear();


    return (
      expiryYear >
        currentYear ||
      (
        expiryYear ===
          currentYear &&
        expiryMonth >=
          currentMonth
      )
    );
  }


  private clearPaymentForm(): void {
    this.paymentForm.reset({
      cardholderName:
        '',


      cardNumber:
        '',


      expiryMonth:
        new Date().getMonth() + 1,


      expiryYear:
        new Date().getFullYear(),


      cvv:
        ''
    });
  }


  private loadConvertedQuoteIds():
    ReadonlySet<string> {
    const storedValue =
      sessionStorage.getItem(
        this.convertedQuoteStorageKey
      );


    if (!storedValue) {
      return new Set<string>();
    }


    try {
      const parsedValue:
        unknown =
        JSON.parse(
          storedValue
        );


      if (
        !Array.isArray(
          parsedValue
        )
      ) {
        sessionStorage.removeItem(
          this.convertedQuoteStorageKey
        );


        return new Set<string>();
      }


      const normalizedQuoteIds =
        parsedValue
          .filter(
            (
              quoteId
            ): quoteId is string =>
              typeof quoteId ===
                'string'
          )
          .map(
            quoteId =>
              this.normalizeQuoteId(
                quoteId
              )
          )
          .filter(
            (
              quoteId
            ): quoteId is string =>
              quoteId !== null
          );


      return new Set<string>(
        normalizedQuoteIds
      );
    } catch {
      sessionStorage.removeItem(
        this.convertedQuoteStorageKey
      );


      return new Set<string>();
    }
  }


  private markQuoteAsConverted(
    quoteId:
      string
  ): void {
    const normalizedQuoteId =
      this.normalizeQuoteId(
        quoteId
      );


    if (!normalizedQuoteId) {
      return;
    }


    this.convertedQuoteIds.update(
      currentIds => {
        const updatedIds =
          new Set<string>(
            currentIds
          );


        updatedIds.add(
          normalizedQuoteId
        );


        sessionStorage.setItem(
          this.convertedQuoteStorageKey,
          JSON.stringify(
            Array.from(
              updatedIds
            )
          )
        );


        return updatedIds;
      }
    );
  }


  private normalizeQuoteId(
    quoteId:
      string | null | undefined
  ): string | null {
    const normalizedQuoteId =
      quoteId
        ?.trim()
        .toLowerCase();


    return normalizedQuoteId ||
      null;
  }


  private updateQueryParameters(
    parameters: {
      quoteId?:
        string | null;


      action?:
        string | null;
    }
  ): void {
    void this.router.navigate(
      [],
      {
        relativeTo:
          this.activatedRoute,


        queryParams:
          parameters,


        queryParamsHandling:
          'merge',


        replaceUrl:
          true
      }
    );
  }
}