import {
  CommonModule
} from '@angular/common';


import {
  Component,
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
  Router,
  RouterLink
} from '@angular/router';


import {
  finalize
} from 'rxjs';


import {
  ADMIN_NAVIGATION_ITEMS,
  AdminNavigationItem
} from '../../../core/constants/admin-navigation.constants';


import {
  AdminDecision
} from '../../../core/models/admin/quotes/admin-decision.enum';


import {
  CoverageType
} from '../../../core/models/admin/quotes/coverage-type.enum';


import {
  ProductType
} from '../../../core/models/admin/quotes/product-type.enum';


import {
  QuoteDetails
} from '../../../core/models/admin/quotes/quote-details.model';


import {
  QuoteStatus
} from '../../../core/models/admin/quotes/quote-status.enum';


import {
  QuoteSummary
} from '../../../core/models/admin/quotes/quote-summary.model';


import {
  RiskLevel
} from '../../../core/models/admin/quotes/risk-level.enum';


import {
  AdminService
} from '../../../core/services/admin.service';


import {
  AuthService
} from '../../../core/services/auth.service';
import {
  GlobalNotificationService
} from '../../../core/services/global-notification.service';


type QuoteStatusFilter =
  | 'pending'
  | 'all'
  | 'active'
  | 'approved'
  | 'rejected'
  | 'expired'
  | 'invalidated'
  | 'withdrawn'
  | 'converted';


type QuoteSortOption =
  | 'generated-desc'
  | 'generated-asc'
  | 'expires-desc'
  | 'expires-asc'
  | 'premium-desc'
  | 'premium-asc'
  | 'product-asc'
  | 'product-desc'
  | 'status-asc'
  | 'status-desc';


type QuoteDecisionMode =
  | 'approve'
  | 'reject'
  | null;


@Component({
  selector:
    'app-admin-quotes',


  standalone:
    true,


  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],


  templateUrl:
    './admin-quotes.component.html',


  styleUrl:
    './admin-quotes.component.css'
})
export class AdminQuotesComponent
  implements OnInit {
  private readonly adminService =
    inject(AdminService);


  private readonly authService =
    inject(AuthService);


  private readonly notifications =
    inject(GlobalNotificationService);


  private readonly formBuilder =
    inject(FormBuilder);


  private readonly router =
    inject(Router);


  readonly QuoteStatus =
    QuoteStatus;


  readonly AdminDecision =
    AdminDecision;


  readonly CoverageType =
    CoverageType;


  readonly navigationItems =
    ADMIN_NAVIGATION_ITEMS;


  readonly pendingApplicationCount =
    this.adminService
      .pendingApplicationCount;


  readonly quotes =
    this.adminService.quotes;


  readonly quotesLoading =
    this.adminService.quotesLoading;


  readonly selectedQuote =
    this.adminService.selectedQuote;


  readonly quoteDetailsLoading =
    this.adminService
      .quoteDetailsLoading;


  readonly processingQuoteId =
    this.adminService
      .processingQuoteId;


  readonly errorMessage =
    this.adminService.errorMessage;


  readonly mobileNavigationOpen =
    signal(false);


  readonly quoteDetailsOpen =
    signal(false);


  readonly decisionMode =
    signal<QuoteDecisionMode>(
      null
    );

  readonly priceAdjustmentError =
    signal<string | null>(null);


  readonly successMessage =
    signal<string | null>(
      null
    );


  readonly quoteNumberFilter =
    signal('');


  readonly searchForm =
    this.formBuilder.nonNullable.group({
      quoteNumber: [
        ''
      ],


      status: [
        'pending' as
          QuoteStatusFilter
      ],


      sort: [
        'generated-desc' as
          QuoteSortOption
      ],


      pageSize: [
        10 as 10 | 25 | 50
      ],


      generatedFrom: [
        ''
      ],


      generatedTo: [
        ''
      ]
    });


  readonly approvalForm =
    this.formBuilder.nonNullable.group({
      remarks: [
        '',
        [
          Validators.maxLength(
            1000
          )
        ]
      ],
      revisedAnnualPremium: [
        '',
        [
          Validators.pattern(/^\d+(\.\d{1,2})?$/),
          Validators.min(0.01),
          Validators.max(9007199254740991)
        ]
      ],
      priceAdjustmentReason: [
        '',
        [
          Validators.maxLength(500)
        ]
      ]
    });


  readonly rejectionForm =
    this.formBuilder.nonNullable.group({
      reason: [
        '',
        [
          Validators.required,
          Validators.minLength(
            20
          ),
          Validators.maxLength(
            1000
          )
        ]
      ]
    });


  readonly quoteItems =
    computed(() => {
      const quoteNumber =
        this.quoteNumberFilter()
          .trim()
          .toLowerCase();


      const quotes =
        this.quotes().items;


      if (!quoteNumber) {
        return quotes;
      }


      return quotes.filter(
        quote =>
          quote.quoteNumber
            .toLowerCase()
            .includes(
              quoteNumber
            )
      );
    });


  readonly totalQuotes =
    computed(
      () =>
        this.quotes().totalCount
    );


  readonly displayedPendingQuotes =
    computed(
      () =>
        this.quotes().items
          .filter(
            quote =>
              quote.status ===
              QuoteStatus
                .PendingAdminApproval
          )
          .length
    );


  readonly displayedApprovedQuotes =
    computed(
      () =>
        this.quotes().items
          .filter(
            quote =>
              quote.status ===
              QuoteStatus.AdminApproved
          )
          .length
    );


  readonly displayedRejectedQuotes =
    computed(
      () =>
        this.quotes().items
          .filter(
            quote =>
              quote.status ===
              QuoteStatus.AdminRejected
          )
          .length
    );


  readonly mandatoryCoverages =
    computed(() => {
      const quote =
        this.selectedQuote();


      if (!quote) {
        return [];
      }


      return quote.coverages
        .filter(
          coverage =>
            coverage.coverageType ===
            CoverageType.Mandatory
        )
        .slice()
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
      const quote =
        this.selectedQuote();


      if (!quote) {
        return [];
      }


      return quote.coverages
        .filter(
          coverage =>
            coverage.coverageType ===
            CoverageType.Optional
        )
        .slice()
        .sort(
          (
            first,
            second
          ) =>
            first.displayOrder -
            second.displayOrder
        );
    });


  readonly canDecideSelectedQuote =
    computed(
      () =>
        this.selectedQuote()
          ?.status ===
        QuoteStatus
          .PendingAdminApproval
    );


  get approvalRemarksControl() {
    return this.approvalForm
      .controls
      .remarks;
  }

  get revisedAnnualPremiumControl() {
    return this.approvalForm
      .controls
      .revisedAnnualPremium;
  }

  get priceAdjustmentReasonControl() {
    return this.approvalForm
      .controls
      .priceAdjustmentReason;
  }


  get rejectionReasonControl() {
    return this.rejectionForm
      .controls
      .reason;
  }


  ngOnInit(): void {
    this.adminService.clearError();


    this.adminService
      .loadPendingClaimsManagers();


    this.loadQuotes(
      1
    );
  }


  applyFilters(): void {
    if (
      this.quotesLoading()
    ) {
      return;
    }


    this.successMessage.set(
      null
    );


    this.adminService.clearError();


    this.quoteNumberFilter.set(
      this.searchForm
        .controls
        .quoteNumber
        .value
    );


    this.loadQuotes(
      1
    );
  }


  clearFilters(): void {
    if (
      this.quotesLoading()
    ) {
      return;
    }


    this.searchForm.setValue({
      quoteNumber:
        '',


      status:
        'pending',


      sort:
        'generated-desc',


      pageSize:
        10,


      generatedFrom:
        '',


      generatedTo:
        ''
    });


    this.quoteNumberFilter.set(
      ''
    );


    this.successMessage.set(
      null
    );


    this.adminService.clearError();


    this.loadQuotes(
      1
    );
  }


  refreshQuotes(): void {
    if (
      this.quotesLoading() ||
      this.processingQuoteId()
    ) {
      return;
    }


    this.successMessage.set(
      null
    );


    this.adminService.clearError();


    this.quoteNumberFilter.set(
      this.searchForm
        .controls
        .quoteNumber
        .value
    );


    const currentPage =
      this.quotes().pageNumber;


    this.loadQuotes(
      currentPage > 0
        ? currentPage
        : 1
    );
  }


  changePage(
    pageNumber: number
  ): void {
    const page =
      this.quotes();


    if (
      pageNumber < 1 ||
      (
        page.totalPages > 0 &&
        pageNumber >
          page.totalPages
      ) ||
      this.quotesLoading()
    ) {
      return;
    }


    this.loadQuotes(
      pageNumber
    );
  }


  openQuote(
    quote: QuoteSummary
  ): void {
    if (
      !quote.id ||
      this.processingQuoteId()
    ) {
      return;
    }


    this.successMessage.set(
      null
    );


    this.decisionMode.set(
      null
    );


    this.approvalForm.reset({
      remarks: '',
      revisedAnnualPremium: '',
      priceAdjustmentReason: ''
    });
    this.priceAdjustmentError.set(null);


    this.rejectionForm.reset({
      reason:
        ''
    });


    this.adminService.clearError();


    this.quoteDetailsOpen.set(
      true
    );


    this.adminService
      .loadQuote(
        quote.id
      );
  }


  closeQuoteDetails(): void {
    if (
      this.processingQuoteId() ||
      this.quoteDetailsLoading()
    ) {
      return;
    }


    this.quoteDetailsOpen.set(
      false
    );


    this.decisionMode.set(
      null
    );


    this.approvalForm.reset({
      remarks: '',
      revisedAnnualPremium: '',
      priceAdjustmentReason: ''
    });
    this.priceAdjustmentError.set(null);


    this.rejectionForm.reset({
      reason:
        ''
    });


    this.adminService
      .clearSelectedQuote();


    this.adminService.clearError();
  }


  openApproveDecision(): void {
    if (
      !this.canDecideSelectedQuote() ||
      this.processingQuoteId()
    ) {
      return;
    }


    this.successMessage.set(
      null
    );


    this.adminService.clearError();


    this.rejectionForm.reset({
      reason:
        ''
    });


    this.decisionMode.set(
      'approve'
    );
    this.approvalForm.reset({
      remarks: '',
      revisedAnnualPremium: '',
      priceAdjustmentReason: ''
    });
    this.priceAdjustmentError.set(null);
  }


  openRejectDecision(): void {
    if (
      !this.canDecideSelectedQuote() ||
      this.processingQuoteId()
    ) {
      return;
    }


    this.successMessage.set(
      null
    );


    this.adminService.clearError();


    this.approvalForm.reset({
      remarks: '',
      revisedAnnualPremium: '',
      priceAdjustmentReason: ''
    });
    this.priceAdjustmentError.set(null);


    this.decisionMode.set(
      'reject'
    );
  }


  cancelDecision(): void {
    if (
      this.processingQuoteId()
    ) {
      return;
    }


    this.decisionMode.set(
      null
    );


    this.approvalForm.reset({
      remarks: '',
      revisedAnnualPremium: '',
      priceAdjustmentReason: ''
    });
    this.priceAdjustmentError.set(null);


    this.rejectionForm.reset({
      reason:
        ''
    });


    this.adminService.clearError();
  }


  confirmApproval(): void {
    if (
      !this.canDecideSelectedQuote() ||
      this.processingQuoteId()
    ) {
      return;
    }


    if (
      this.approvalForm.invalid
    ) {
      this.approvalForm
        .markAllAsTouched();


      return;
    }


    const formValue =
      this.approvalForm.getRawValue();
    const remarks = formValue.remarks.trim();
    const revisedPriceText =
      String(formValue.revisedAnnualPremium).trim();
    const revisedAnnualPremium =
      revisedPriceText ? Number(revisedPriceText) : null;
    if (
      revisedPriceText &&
      (revisedAnnualPremium === null ||
        !Number.isFinite(revisedAnnualPremium))
    ) {
      this.priceAdjustmentError.set(
        'Enter a valid annual premium amount.'
      );
      this.revisedAnnualPremiumControl.markAsTouched();
      return;
    }

    const currentPremium =
      this.selectedQuote()?.finalAnnualPremium;
    const priceChanged =
      revisedAnnualPremium !== null &&
      revisedAnnualPremium !== currentPremium;
    const priceAdjustmentReason =
      formValue.priceAdjustmentReason.trim();

    this.priceAdjustmentError.set(null);
    if (
      priceChanged &&
      (priceAdjustmentReason.length < 5 ||
        priceAdjustmentReason.length > 500)
    ) {
      this.priceAdjustmentError.set(
        'Enter a price adjustment reason between 5 and 500 characters.'
      );
      this.priceAdjustmentReasonControl.markAsTouched();
      return;
    }


    this.successMessage.set(
      null
    );


    this.adminService.clearError();


    this.adminService
      .approveQuote(
        remarks,
        priceChanged ? revisedAnnualPremium : null,
        priceChanged ? priceAdjustmentReason : null
      )
      .subscribe({
        next: updatedQuote => {
          this.decisionMode.set(null);
          this.notifications.showSuccess(
            `Quote ${updatedQuote.quoteNumber} was approved successfully.`
          );


          this.quoteDetailsOpen.set(
            false
          );


          this.adminService
            .clearSelectedQuote();


          this.approvalForm.reset({
            remarks: '',
            revisedAnnualPremium: '',
            priceAdjustmentReason: ''
          });
          this.priceAdjustmentError.set(null);


          this.rejectionForm.reset({
            reason:
              ''
          });
        },
        error: () => {
          this.decisionMode.set('approve');
        }
      });
  }


  confirmRejection(): void {
    if (
      !this.canDecideSelectedQuote() ||
      this.processingQuoteId()
    ) {
      return;
    }


    if (
      this.rejectionForm.invalid
    ) {
      this.rejectionForm
        .markAllAsTouched();


      return;
    }


    const reason =
      this.rejectionForm
        .getRawValue()
        .reason
        .trim();


    if (!reason) {
      this.rejectionReasonControl
        .setErrors({
          required:
            true
        });


      this.rejectionReasonControl
        .markAsTouched();


      return;
    }


    this.successMessage.set(
      null
    );


    this.adminService.clearError();


    this.adminService
      .rejectQuote(
        reason
      )
      .pipe(
        finalize(() => {
          this.decisionMode.set(
            null
          );
        })
      )
      .subscribe({
        next: updatedQuote => {
          this.notifications.showSuccess(
            `Quote ${updatedQuote.quoteNumber} was rejected successfully.`
          );


          this.quoteDetailsOpen.set(
            false
          );


          this.adminService
            .clearSelectedQuote();


          this.approvalForm.reset({
            remarks: '',
            revisedAnnualPremium: '',
            priceAdjustmentReason: ''
          });
          this.priceAdjustmentError.set(null);


          this.rejectionForm.reset({
            reason:
              ''
          });
        }
      });
  }


  openNavigationItem(
    item: AdminNavigationItem
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
      this.closeMobileNavigation();


      return;
    }


    this.navigateTo(
      item.route
    );
  }


  isCurrentNavigationItem(
    item: AdminNavigationItem
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


  clearError(): void {
    this.adminService.clearError();
  }


  clearSuccessMessage(): void {
    this.successMessage.set(
      null
    );
  }


  toggleMobileNavigation(): void {
    this.mobileNavigationOpen.update(
      open =>
        !open
    );
  }


  closeMobileNavigation(): void {
    this.mobileNavigationOpen.set(
      false
    );
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
    this.closeMobileNavigation();
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
    this.navigateTo(
      '/admin/operations'
    );
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
    this.closeMobileNavigation();


    this.authService.logout();
  }


  quoteStatusLabel(
    status: QuoteStatus
  ): string {
    switch (status) {
      case QuoteStatus.Active:
        return 'Active';


      case QuoteStatus
        .PendingAdminApproval:
        return 'Pending Admin approval';


      case QuoteStatus.AdminApproved:
        return 'Admin approved';


      case QuoteStatus.AdminRejected:
        return 'Admin rejected';


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


  adminDecisionLabel(
    decision:
      AdminDecision | null
  ): string {
    switch (decision) {
      case AdminDecision.Approved:
        return 'Approved';


      case AdminDecision.Rejected:
        return 'Rejected';


      default:
        return 'Not decided';
    }
  }


  riskLevelLabel(
    level: RiskLevel
  ): string {
    switch (level) {
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


  productTypeLabel(
    type: ProductType
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
        return 'Unknown';
    }
  }


  formatDate(
    value: string | null
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


  formatCurrency(
    value: number
  ): string {
    if (
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


  isQuoteExpired(
    quote:
      QuoteSummary |
      QuoteDetails
  ): boolean {
    if (
      !quote.expiresAtUtc
    ) {
      return false;
    }


    const expiry =
      new Date(
        quote.expiresAtUtc
      ).getTime();


    if (
      Number.isNaN(
        expiry
      )
    ) {
      return false;
    }


    return (
      expiry <=
      Date.now()
    );
  }


  isProcessing(
    quoteId: string
  ): boolean {
    return (
      this.processingQuoteId() ===
      quoteId
    );
  }


  private loadQuotes(
    pageNumber: number
  ): void {
    const values =
      this.searchForm
        .getRawValue();


    const sort =
      this.resolveSort(
        values.sort
      );


    const status =
      this.resolveStatus(
        values.status
      );


    this.adminService.clearError();


    this.adminService
      .loadQuotes({
        pageNumber,


        pageSize:
          values.pageSize,


        sortBy:
          sort.sortBy,


        sortDescending:
          sort.sortDescending,


        status,


        generatedFromUtc:
          this.toStartUtc(
            values.generatedFrom
          ),


        generatedToUtc:
          this.toEndUtc(
            values.generatedTo
          ),


        decisionFromUtc:
          undefined,


        decisionToUtc:
          undefined
      });
  }


  private resolveStatus(
    status:
      QuoteStatusFilter
  ): QuoteStatus | undefined {
    switch (status) {
      case 'active':
        return QuoteStatus.Active;


      case 'pending':
        return QuoteStatus
          .PendingAdminApproval;


      case 'approved':
        return QuoteStatus
          .AdminApproved;


      case 'rejected':
        return QuoteStatus
          .AdminRejected;


      case 'expired':
        return QuoteStatus.Expired;


      case 'invalidated':
        return QuoteStatus
          .Invalidated;


      case 'withdrawn':
        return QuoteStatus
          .Withdrawn;


      case 'converted':
        return QuoteStatus
          .ConvertedToPolicy;


      default:
        return undefined;
    }
  }


  private resolveSort(
    sort: QuoteSortOption
  ): {
    sortBy: string;
    sortDescending: boolean;
  } {
    switch (sort) {
      case 'generated-asc':
        return {
          sortBy:
            'generatedAtUtc',


          sortDescending:
            false
        };


      case 'expires-desc':
        return {
          sortBy:
            'expiresAtUtc',


          sortDescending:
            true
        };


      case 'expires-asc':
        return {
          sortBy:
            'expiresAtUtc',


          sortDescending:
            false
        };


      case 'premium-desc':
        return {
          sortBy:
            'finalAnnualPremium',


          sortDescending:
            true
        };


      case 'premium-asc':
        return {
          sortBy:
            'finalAnnualPremium',


          sortDescending:
            false
        };


      case 'product-asc':
        return {
          sortBy:
            'productName',


          sortDescending:
            false
        };


      case 'product-desc':
        return {
          sortBy:
            'productName',


          sortDescending:
            true
        };


      case 'status-asc':
        return {
          sortBy:
            'status',


          sortDescending:
            false
        };


      case 'status-desc':
        return {
          sortBy:
            'status',


          sortDescending:
            true
        };


      default:
        return {
          sortBy:
            'generatedAtUtc',


          sortDescending:
            true
        };
    }
  }


  private toStartUtc(
    value: string
  ): string | undefined {
    if (!value) {
      return undefined;
    }


    const date =
      new Date(
        `${value}T00:00:00`
      );


    if (
      Number.isNaN(
        date.getTime()
      )
    ) {
      return undefined;
    }


    return date.toISOString();
  }


  private toEndUtc(
    value: string
  ): string | undefined {
    if (!value) {
      return undefined;
    }


    const date =
      new Date(
        `${value}T23:59:59.999`
      );


    if (
      Number.isNaN(
        date.getTime()
      )
    ) {
      return undefined;
    }


    return date.toISOString();
  }


  private navigateTo(
    route: string
  ): void {
    this.closeMobileNavigation();


    void this.router.navigateByUrl(
      route
    );
  }
}
