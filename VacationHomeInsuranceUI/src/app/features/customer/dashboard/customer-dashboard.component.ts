import {
  CommonModule
} from '@angular/common';


import {
  Component,
  OnDestroy,
  OnInit,
  computed,
  inject
} from '@angular/core';


import {
  Router,
  RouterLink
} from '@angular/router';


import {
  CustomerStatus
} from '../../../core/models/admin/customer-status.enum';


import {
  OccupancyFrequency
} from '../../../core/models/admin/occupancy-frequency.enum';


import {
  PropertyType
} from '../../../core/models/admin/property-type.enum';


import {
  VacationProperty
} from '../../../core/models/admin/vacation-property.model';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  CustomerService
} from '../../../core/services/customer.service';


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
    'app-customer-dashboard',


  standalone:
    true,


  imports: [
    CommonModule,
    RouterLink
  ],


  templateUrl:
    './customer-dashboard.component.html',


  styleUrl:
    './customer-dashboard.component.css'
})
export class CustomerDashboardComponent
  implements OnInit, OnDestroy {
  private readonly customerService =
    inject(CustomerService);


  private readonly authService =
    inject(AuthService);


  private readonly router =
    inject(Router);


  readonly CustomerStatus =
    CustomerStatus;


  readonly PropertyType =
    PropertyType;


  readonly OccupancyFrequency =
    OccupancyFrequency;
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


  readonly profile =
    this.customerService.profile;


  readonly properties =
    this.customerService.properties;


  readonly profileLoading =
    this.customerService.profileLoading;


  readonly propertiesLoading =
    this.customerService.propertiesLoading;


  readonly errorMessage =
    this.customerService.errorMessage;


  readonly loading =
    computed(() => {
      return (
        this.profileLoading() ||
        this.propertiesLoading()
      );
    });


  readonly propertyItems =
    computed(() => {
      return (
        this.properties().items ??
        []
      );
    });


  readonly recentProperties =
    computed(() => {
      return this.propertyItems()
        .slice(
          0,
          4
        );
    });


  readonly totalProperties =
    computed(() => {
      return (
        this.properties().totalCount ??
        0
      );
    });


  readonly securedProperties =
    computed(() => {
      return this.propertyItems()
        .filter(
          property =>
            property.hasSecuritySystem
        )
        .length;
    });


  readonly disasterExposedProperties =
    computed(() => {
      return this.propertyItems()
        .filter(
          property =>
            property.disasterExposure
        )
        .length;
    });


  readonly totalPropertyValue =
    computed(() => {
      return this.propertyItems()
        .reduce(
          (
            total,
            property
          ) =>
            total +
            property.propertyValue,
          0
        );
    });


  readonly activeProfile =
    computed(() => {
      return (
        this.profile()?.status ===
        CustomerStatus.Active
      );
    });


  ngOnInit(): void {
    this.loadDashboard();
  }


  ngOnDestroy(): void {
    this.customerService.clearError();
  }


  loadDashboard(): void {
    this.customerService.clearError();


    this.customerService
      .loadDashboard();
  }


  refreshDashboard(): void {
    if (this.loading()) {
      return;
    }


    this.loadDashboard();
  }


  displayName(): string {
    const profile =
      this.profile();


    if (!profile) {
      return 'Customer';
    }


    const name =
      [
        profile.firstName,
        profile.lastName
      ]
        .filter(
          value =>
            Boolean(
              value?.trim()
            )
        )
        .map(
          value =>
            value.trim()
        )
        .join(' ');


    return name || 'Customer';
  }


  initials(): string {
    const profile =
      this.profile();


    if (!profile) {
      return 'CU';
    }


    const firstInitial =
      profile.firstName
        ?.trim()
        .charAt(0) ??
      '';


    const lastInitial =
      profile.lastName
        ?.trim()
        .charAt(0) ??
      '';


    const initials =
      (
        firstInitial +        lastInitial
      )
        .trim()
        .toUpperCase();


    return initials || 'CU';
  }


  
  profileLocation(): string {
    const profile =
      this.profile();


    if (!profile) {
      return 'Location not completed';
    }


    const location =
      [
        profile.city,
        profile.state,
        profile.country
      ]
        .filter(
          value =>
            Boolean(
              value?.trim()
            )
        )
        .map(
          value =>
            value!.trim()
        )
        .join(', ');


    return (
      location ||
      'Location not completed'
    );
  }


  openProperties(): void {
    this.navigateTo(
      '/customer/properties'
    );
  }


  addProperty(): void {
    void this.router.navigate(
      [
        '/customer/properties'
      ],
      {
        queryParams: {
          action:
            'create'
        }
      }
    );
  }


  openProperty(
    property:
      VacationProperty
  ): void {
    const propertyId =
      property.propertyId
        ?.trim();


    if (!propertyId) {
      return;
    }


    void this.router.navigate(
      [
        '/customer/properties'
      ],
      {
        queryParams: {
          propertyId
        }
      }
    );
  }


  openProfile(): void {
    this.navigateTo(
      '/customer/profile'
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


  openNavigationItem(
    item:
      CustomerNavigationItem
  ): void {
    if (
      !item.available ||
      !item.route ||
      this.isCurrentNavigationItem(
        item
      )
    ) {
      return;
    }


    this.navigateTo(
      item.route
    );
  }


  isCurrentNavigationItem(
    item:
      CustomerNavigationItem
  ): boolean {
    const currentUrl =
      this.router.url
        .split('?')[0]
        .split('#')[0];


    if (
      item.route ===
      '/customer/dashboard'
    ) {
      return (
        currentUrl ===
        item.route
      );
    }


    return (
      currentUrl ===        item.route ||
      currentUrl.startsWith(
        `${item.route}/`
      )
    );
  }


  propertyTypeLabel(
    type: PropertyType
  ): string {
    switch (type) {
      case PropertyType.House:
        return 'House';


      case PropertyType.Villa:
        return 'Villa';


      case PropertyType.Apartment:
        return 'Apartment';


      case PropertyType.Cottage:
        return 'Cottage';


      default:
        return 'Property';
    }
  }


  occupancyLabel(
    occupancy:
      OccupancyFrequency
  ): string {
    switch (occupancy) {
      case OccupancyFrequency.High:
        return 'High occupancy';


      case OccupancyFrequency.Medium:
        return 'Medium occupancy';


      case OccupancyFrequency.Low:
        return 'Low occupancy';


      default:
        return 'Not available';
    }
  }


  propertyStatusLabel(
    property:
      VacationProperty
  ): string {
    return property.isArchived
      ? 'Archived'
      : 'Active';
  }


  formatCurrency(
    value: number | null
  ): string {
    if (
      value === null ||
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
          'numeric'
      }
    ).format(
      date
    );
  }


  clearError(): void {
    this.customerService
      .clearError();
  }


  logout(): void {
    this.authService.logout();
  }


  private navigateTo(
    route: string
  ): void {
    void this.router.navigateByUrl(
      route
    );
  }
}
    
