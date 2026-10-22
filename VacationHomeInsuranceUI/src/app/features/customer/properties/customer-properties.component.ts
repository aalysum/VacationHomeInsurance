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
  OccupancyFrequency
} from '../../../core/models/admin/occupancy-frequency.enum';


import {
  PropertyType
} from '../../../core/models/admin/property-type.enum';


import {
  VacationProperty
} from '../../../core/models/admin/vacation-property.model';


import {
  CreateVacationPropertyRequest
} from '../../../core/models/customer/properties/create-vacation-property-request.model';


import {
  UpdateVacationPropertyRequest
} from '../../../core/models/customer/properties/update-vacation-property-request.model';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  CustomerService
} from '../../../core/services/customer.service';


type PropertyDialogMode =
  | 'details'
  | 'create'
  | 'edit'
  | 'archive'
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
    'app-customer-properties',


  standalone:
    true,


  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],


  templateUrl:
    './customer-properties.component.html',


  styleUrl:
    './customer-properties.component.css'
})
export class CustomerPropertiesComponent
  implements OnInit, OnDestroy {
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


  readonly PropertyType =
    PropertyType;


  readonly OccupancyFrequency =
    OccupancyFrequency;


  readonly dialogMode =
    signal<PropertyDialogMode>(
      null
    );


  readonly includeArchived =
    signal(false);


  readonly pageSize =
    signal(10);


    readonly navigationItems:
     CustomerNavigationItem[] = [
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


  readonly selectedProperty =
    this.customerService.selectedProperty;


  readonly propertiesLoading =
    this.customerService.propertiesLoading;


  readonly propertyDetailsLoading =
    this.customerService.propertyDetailsLoading;


  readonly propertyProcessing =
    this.customerService.propertyProcessing;


  readonly errorMessage =
    this.customerService.errorMessage;


  readonly successMessage =
    this.customerService.successMessage;


  readonly propertyItems =
    computed(() => {
      return (
        this.properties().items ??
        []
      );
    });


  readonly totalProperties =
    computed(() => {
      return this.properties().totalCount;
    });


  readonly pageNumber =
    computed(() => {
      return this.properties().pageNumber;
    });


  readonly totalPages =
    computed(() => {
      return this.properties().totalPages;
    });


  readonly hasPreviousPage =
    computed(() => {
      return this.properties()
        .hasPreviousPage;
    });


  readonly hasNextPage =
    computed(() => {
      return this.properties()
        .hasNextPage;
    });


  readonly pageStart =
    computed(() => {
      if (
        this.totalProperties() === 0
      ) {
        return 0;
      }


      return (
        (
          this.pageNumber() - 1
        ) *
        this.properties().pageSize
      ) + 1;
    });


  readonly pageEnd =
    computed(() => {
      return Math.min(
        this.pageNumber() *
          this.properties().pageSize,
        this.totalProperties()
      );
    });


  readonly propertyForm =
    this.formBuilder.nonNullable.group({
      propertyName: [
        '',
        [
          Validators.required,
          Validators.maxLength(
            200
          )
        ]
      ],


      propertyType: [
        PropertyType.House,
        [
          Validators.required
        ]
      ],


      addressLine1: [
        '',
        [
          Validators.required,
          Validators.maxLength(
            250
          )
        ]
      ],


      addressLine2: [
        '',
        [
          Validators.maxLength(
            250
          )
        ]
      ],


      city: [
        '',
        [
          Validators.required,
          Validators.maxLength(
            100
          )
        ]
      ],


      state: [
        '',
        [
          Validators.required,
          Validators.maxLength(
            100
          )
        ]
      ],


      postalCode: [
        '',
        [
          Validators.required,
          Validators.maxLength(
            20
          )
        ]
      ],


      country: [
        '',
        [
          Validators.required,
          Validators.maxLength(
            100
          )
        ]
      ],


  propertyValue: [
  0,
  [
    Validators.required,
    Validators.min(50000),
    Validators.max(100000000)
  ]
],


      constructionYear: [
        new Date().getFullYear(),
        [
          Validators.required,
          Validators.min(
            1800
          ),
          Validators.max(
            new Date().getFullYear()
          )
        ]
      ],


      occupancyFrequency: [
        OccupancyFrequency.Medium,
        [
          Validators.required
        ]
      ],


      hasSecuritySystem: [
        false
      ],


      previousClaimsCount: [
        0,
        [
          Validators.required,
          Validators.min(
            0
          )
        ]
      ],


      disasterExposure: [
        false
      ]
    });


  ngOnInit(): void {
    this.customerService.clearError();
    this.customerService.clearSuccessMessage();


    this.customerService.loadProfile();


    this.loadProperties();


    const action =
      this.activatedRoute
        .snapshot
        .queryParamMap
        .get(
          'action'
        );


    if (action === 'create') {
      this.openCreateProperty();
    }


    const propertyId =
      this.activatedRoute
        .snapshot
        .queryParamMap
        .get(
          'propertyId'
        )
        ?.trim();


    if (propertyId) {
      this.openPropertyById(
        propertyId
      );
    }
  }


  ngOnDestroy(): void {
    this.customerService
      .clearSelectedProperty();


    this.customerService
      .clearError();


    this.customerService
      .clearSuccessMessage();
  }


  loadProperties(): void {
    this.customerService.loadProperties({
      pageNumber:
        1,


      pageSize:
        this.pageSize(),


      includeArchived:
        this.includeArchived(),


      sortBy:
        'createdAtUtc',


      sortDirection:
        'desc'
    });
  }


  refreshProperties(): void {
    if (
      this.propertiesLoading() ||
      this.propertyProcessing()
    ) {
      return;
    }


    this.customerService
      .refreshProperties();
  }


  toggleArchived(
    event: Event
  ): void {
    const input =
      event.target as HTMLInputElement;


    this.includeArchived.set(
      input.checked
    );


    this.loadProperties();
  }


  changePageSize(
    event: Event
  ): void {
    const select =
      event.target as HTMLSelectElement;


    const pageSize =
      Number(
        select.value
      );


    if (
      ![
        10,
        25,
        50
      ].includes(
        pageSize
      )
    ) {
      return;
    }


    this.pageSize.set(
      pageSize
    );


    this.loadProperties();
  }


  previousPage(): void {
    this.customerService
      .goToPreviousPropertyPage();
  }


  nextPage(): void {
    this.customerService
      .goToNextPropertyPage();
  }


  openCreateProperty(): void {
    if (
      this.propertyProcessing()
    ) {
      return;
    }


    this.customerService
      .clearSelectedProperty();


    this.resetPropertyForm();


    this.dialogMode.set(
      'create'
    );


    this.updateQueryParameters({
      action:
        'create',


      propertyId:
        null
    });
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


    this.openPropertyById(
      propertyId
    );
  }


  openEditProperty(): void {
    const property =
      this.selectedProperty();


    if (
      !property ||
      property.isArchived
    ) {
      return;
    }


    this.populatePropertyForm(
      property
    );


    this.dialogMode.set(
      'edit'
    );
  }


  openArchiveProperty(): void {
    const property =
      this.selectedProperty();


    if (
      !property ||
      property.isArchived
    ) {
      return;
    }


    this.dialogMode.set(
      'archive'
    );
  }


  returnToDetails(): void {
    if (
      this.propertyProcessing()
    ) {
      return;
    }


    this.dialogMode.set(
      'details'
    );
  }


  closeDialog(): void {
    if (
      this.propertyProcessing()
    ) {
      return;
    }


    this.dialogMode.set(
      null
    );


    this.customerService
      .clearSelectedProperty();


    this.resetPropertyForm();


    this.updateQueryParameters({
      action:
        null,


      propertyId:
        null
    });
  }


  submitProperty(): void {
    this.propertyForm
      .markAllAsTouched();


    if (
      this.propertyForm.invalid ||
      this.propertyProcessing()
    ) {
      return;
    }


    const values =
      this.propertyForm
        .getRawValue();


    if (
      this.dialogMode() ===
      'create'
    ) {
      const request:
        CreateVacationPropertyRequest = {
          propertyName:
            values.propertyName.trim(),


          propertyType:
            values.propertyType,


          addressLine1:
            values.addressLine1.trim(),


          addressLine2:
            values.addressLine2
              .trim() ||
            null,


          city:
            values.city.trim(),


          state:
            values.state.trim(),


          postalCode:
            values.postalCode.trim(),


          country:
            values.country.trim(),


          propertyValue:
            Number(
              values.propertyValue
            ),


          constructionYear:
            Number(
              values.constructionYear
            ),


          occupancyFrequency:
            values.occupancyFrequency,


          hasSecuritySystem:
            values.hasSecuritySystem,


          previousClaimsCount:
            Number(
              values.previousClaimsCount
            ),


          disasterExposure:
            values.disasterExposure
        };


      this.customerService
        .createProperty(
          request
        );


      return;
    }


    const property =
      this.selectedProperty();


    if (
      this.dialogMode() !==
        'edit' ||
      !property
    ) {
      return;
    }


    const request:
      UpdateVacationPropertyRequest = {
        propertyName:
          values.propertyName.trim(),


        propertyType:
          values.propertyType,


        addressLine1:
          values.addressLine1.trim(),


        addressLine2:
          values.addressLine2
            .trim() ||
          null,


        city:
          values.city.trim(),


        state:
          values.state.trim(),


        postalCode:
          values.postalCode.trim(),


        country:
          values.country.trim(),


        propertyValue:
          Number(
            values.propertyValue
          ),


        constructionYear:
          Number(
            values.constructionYear
          ),


        occupancyFrequency:
          values.occupancyFrequency,


        hasSecuritySystem:
          values.hasSecuritySystem,


        previousClaimsCount:
          Number(
            values.previousClaimsCount
          ),


        disasterExposure:
          values.disasterExposure,


        rowVersion:
          property.rowVersion
      };


    this.customerService
      .updateProperty(
        property.propertyId,
        request
      );
  }


  confirmArchive(): void {
    const property =
      this.selectedProperty();


    if (
      !property ||
      property.isArchived ||
      this.propertyProcessing()
    ) {
      return;
    }


    this.customerService
      .archiveProperty(
        property.propertyId,
        property.rowVersion
      );


    this.dialogMode.set(
      null
    );


    this.updateQueryParameters({
      propertyId:
        null,


      action:
        null
    });
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
        .join(' ');


    return name || 'Customer';
  }


  initials(): string {
    const profile =
      this.profile();


    if (!profile) {
      return 'CU';
    }


    const initials =
      (
        profile.firstName
          ?.trim()
          .charAt(0) +
        profile.lastName
          ?.trim()
          .charAt(0)
      )
        .toUpperCase();


    return initials || 'CU';
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
    frequency:
      OccupancyFrequency
  ): string {
    switch (frequency) {
      case OccupancyFrequency.High:
        return 'High';


      case OccupancyFrequency.Medium:
        return 'Medium';


      case OccupancyFrequency.Low:
        return 'Low';


      default:
        return 'Not available';
    }
  }


  formatCurrency(
    value: number
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
    value: string | null
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
      currentUrl ===
        item.route ||
      currentUrl.startsWith(
        `${item.route}/`
      )
    );
  }


  clearError(): void {
    this.customerService
      .clearError();
  }


  clearSuccessMessage(): void {
    this.customerService
      .clearSuccessMessage();
  }


  logout(): void {
    this.authService.logout();
  }


  private openPropertyById(
    propertyId: string
  ): void {
    this.dialogMode.set(
      'details'
    );


    this.customerService
      .loadProperty(
        propertyId
      );


    this.updateQueryParameters({
      propertyId,
      action:
        null
    });
  }


  private populatePropertyForm(
    property:
      VacationProperty
  ): void {
    this.propertyForm.setValue({
      propertyName:
        property.propertyName,


      propertyType:
        property.propertyType,


      addressLine1:
        property.addressLine1,


      addressLine2:
        property.addressLine2 ??
        '',


      city:
        property.city,


      state:
        property.state,


      postalCode:
        property.postalCode,


      country:
        property.country,


      propertyValue:
        property.propertyValue,


      constructionYear:
        property.constructionYear,


      occupancyFrequency:
        property.occupancyFrequency,


      hasSecuritySystem:
        property.hasSecuritySystem,


      previousClaimsCount:
        property.previousClaimsCount,


      disasterExposure:
        property.disasterExposure
    });
  }


  private resetPropertyForm(): void {
    this.propertyForm.reset({
      propertyName:
        '',


      propertyType:
        PropertyType.House,


      addressLine1:
        '',


      addressLine2:
        '',


      city:
        '',


      state:
        '',


      postalCode:
        '',


      country:
        '',


      propertyValue:
        0,


      constructionYear:
        new Date()
          .getFullYear(),


      occupancyFrequency:
        OccupancyFrequency.Medium,


      hasSecuritySystem:
        false,


      previousClaimsCount:
        0,


      disasterExposure:
        false
    });
  }


  private updateQueryParameters(
    queryParameters: {
      propertyId?:
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
          queryParameters,


        queryParamsHandling:
          'merge',


        replaceUrl:
          true
      }
    );
  }
}