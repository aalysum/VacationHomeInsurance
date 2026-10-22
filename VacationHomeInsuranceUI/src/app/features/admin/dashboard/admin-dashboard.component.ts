import {
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';


import {
  Router,
  RouterLink,
  RouterLinkActive
} from '@angular/router';


import {
  ADMIN_NAVIGATION_ITEMS,
  AdminNavigationItem
} from '../../../core/constants/admin-navigation.constants';


import {
  QuoteStatus
} from '../../../core/models/admin/quotes/quote-status.enum';


import {
  AdminService
} from '../../../core/services/admin.service';


import {
  AuthService
} from '../../../core/services/auth.service';


@Component({
  selector:
    'app-admin-dashboard',


  standalone:
    true,


  imports: [
    RouterLink,
    RouterLinkActive
  ],


  templateUrl:
    './admin-dashboard.component.html',


  styleUrl:
    './admin-dashboard.component.css'
})
export class AdminDashboardComponent
  implements OnInit {
  private readonly adminService =
    inject(AdminService);


  private readonly authService =
    inject(AuthService);


  private readonly router =
    inject(Router);


  readonly currentYear =
    new Date().getFullYear();


  readonly mobileNavigationOpen =
    signal(false);


  readonly navigationItems =
    ADMIN_NAVIGATION_ITEMS;


  readonly pendingApplications =
    this.adminService
      .pendingClaimsManagers;


  readonly pendingApplicationCount =
    this.adminService
      .pendingApplicationCount;


  readonly customerCount =
    this.adminService.customerCount;


  readonly customersLoading =
    this.adminService.customersLoading;


  readonly quoteCount =
    this.adminService.quoteCount;


  readonly quotesLoading =
    this.adminService.quotesLoading;


  readonly productsLoading =
    this.adminService.productsLoading;


  readonly productCount =
    this.adminService.productCount;


  readonly activeProductCount =
    this.adminService
      .activeProductCount;


  readonly coverageCount =
    this.adminService.coverageCount;


  readonly errorMessage =
    this.adminService.errorMessage;


  readonly recentApplications =
    computed(
      () =>
        this.pendingApplications()
          .slice(
            0,
            3
          )
    );


  readonly loading =
    computed(
      () =>
        this.adminService.loading() ||
        this.customersLoading() ||
        this.quotesLoading() ||
        this.productsLoading()
    );


  ngOnInit(): void {
    this.loadDashboard();
  }


  refreshDashboard(): void {
    if (this.loading()) {
      return;
    }


    this.adminService.clearError();


    this.adminService
      .refreshPendingClaimsManagers();


    this.adminService
      .refreshCustomers();


    this.adminService
      .refreshQuotes();


    this.adminService
      .refreshProducts();
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


    this.navigateTo(
      item.route
    );
  }


  openPendingApplications(): void {
    this.navigateTo(
      '/admin/claims-managers/pending'
    );
  }


  openCustomers(): void {
    this.navigateTo(
      '/admin/customers'
    );
  }


  openQuotes(): void {
    this.navigateTo(
      '/admin/quotes'
    );
  }


  openProducts(): void {
    this.navigateTo(
      '/admin/products'
    );
  }


  openPricing(): void {
    this.navigateTo(
      '/admin/pricing'
    );
  }


  openOperations(): void {
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


  clearError(): void {
    this.adminService.clearError();
  }


  logout(): void {
    this.closeMobileNavigation();


    this.authService.logout();
  }


  formatSubmittedDate(
    value: string
  ): string {
    if (
      !value ||
      !value.trim()
    ) {
      return 'Date unavailable';
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
      return 'Date unavailable';
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


  private loadDashboard(): void {
    this.adminService.clearError();


    this.adminService
      .loadPendingClaimsManagers();


    this.adminService
      .loadCustomers({
        pageNumber:
          1,


        pageSize:
          10,


        sortBy:
          'createdAtUtc',


        sortDirection:
          'desc'
      });


    this.adminService
      .loadQuotes({
        pageNumber:
          1,


        pageSize:
          10,


        sortBy:
          'generatedAtUtc',


        sortDescending:
          true,


        status:
          QuoteStatus
            .PendingAdminApproval
      });


    this.adminService
      .loadProducts();
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

