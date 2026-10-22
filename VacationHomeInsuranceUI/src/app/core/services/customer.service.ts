import {
  HttpClient,
  HttpParams
} from '@angular/common/http';


import {
  Injectable,
  inject,
  signal
} from '@angular/core';


import {
  finalize
} from 'rxjs';


import {
  PagedResult
} from '../models/customer/paged-result.model';


import {
  CustomerProfile
} from '../models/customer/customer-profile.model';


import {
  VacationProperty
} from '../models/admin/vacation-property.model';


import {
  ArchiveVacationPropertyRequest
} from '../models/customer/properties/archive-vacation-property-request.model';


import {
  CreateVacationPropertyRequest
} from '../models/customer/properties/create-vacation-property-request.model';


import {
  PropertyQuery
} from '../models/customer/properties/property-query.model';


import {
  UpdateVacationPropertyRequest
} from '../models/customer/properties/update-vacation-property-request.model';


import {
  parseApiError
} from '../utils/api-error.util';


import {
  UpdateCustomerProfileRequest
} from '../models/customer/update-customer-profile-request.model';


import {
  environment
} from '../../../environments/environment';


import {
  GlobalNotificationService
} from './global-notification.service';


const EMPTY_PROPERTY_RESULT:
  PagedResult<VacationProperty> = {
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  };


@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private readonly httpClient =
    inject(HttpClient);


  private readonly globalNotifications =
    inject(GlobalNotificationService);


    private readonly apiBaseUrl =
  environment.apiGatewayBaseUrl;


  private readonly profileState =
    signal<CustomerProfile | null>(
      null
    );


  private readonly propertiesState =
    signal<
      PagedResult<VacationProperty>
    >(
      EMPTY_PROPERTY_RESULT
    );


  private readonly selectedPropertyState =
    signal<VacationProperty | null>(
      null
    );


  private readonly profileLoadingState =
    signal(false);


  private readonly propertiesLoadingState =
    signal(false);


  private readonly propertyDetailsLoadingState =
    signal(false);


  private readonly propertyProcessingState =
    signal(false);


  private readonly errorMessageState =
    signal<string | null>(
      null
    );


  private readonly successMessageState =
    signal<string | null>(
      null
    );


  private readonly currentPropertyQueryState =
    signal<PropertyQuery>({
      pageNumber: 1,
      pageSize: 10,
      includeArchived: false,
      sortBy: 'createdAtUtc',
      sortDirection: 'desc'
    });


  readonly profile =
    this.profileState.asReadonly();


  readonly properties =
    this.propertiesState.asReadonly();


  readonly selectedProperty =
    this.selectedPropertyState.asReadonly();


  readonly profileLoading =
    this.profileLoadingState.asReadonly();


  readonly propertiesLoading =
    this.propertiesLoadingState.asReadonly();


  readonly propertyDetailsLoading =
    this.propertyDetailsLoadingState.asReadonly();


  readonly propertyProcessing =
    this.propertyProcessingState.asReadonly();


  readonly errorMessage =
    this.errorMessageState.asReadonly();


  readonly successMessage =
    this.successMessageState.asReadonly();


  readonly currentPropertyQuery =
    this.currentPropertyQueryState.asReadonly();


  loadProfile(): void {
    if (
      this.profileLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.profileLoadingState.set(
      true
    );


    this.httpClient
      .get<CustomerProfile>(
`${this.apiBaseUrl}/customers/profile`
      )
      .pipe(
        finalize(() => {
          this.profileLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: profile => {
          this.profileState.set(
            profile
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }
  updateProfile(
  request:
    UpdateCustomerProfileRequest
): void {
  if (
    this.profileLoadingState()
  ) {
    return;
  }


  this.clearError();
  this.clearSuccessMessage();


  this.profileLoadingState.set(
    true
  );


  this.httpClient
    .patch<void>(
`${this.apiBaseUrl}/customers/profile`,
      request
    )
    .pipe(
      finalize(() => {
        this.profileLoadingState.set(
          false
        );
      })
    )
    .subscribe({
      next: () => {
          this.globalNotifications.showSuccess(
          'Customer profile updated successfully.'
        );


        this.loadProfile();
      },


      error: error => {
        this.clearError();
        this.globalNotifications.showError(
          parseApiError(error).message
        );
      }
    });
}


  loadProperties(
    query:
      PropertyQuery =
        this.currentPropertyQueryState()
  ): void {
    if (
      this.propertiesLoadingState()
    ) {
      return;
    }


    this.clearError();


    this.currentPropertyQueryState.set(
      query
    );


    this.propertiesLoadingState.set(
      true
    );


    const params =
      new HttpParams()
        .set(
          'pageNumber',
          String(query.pageNumber)
        )
        .set(
          'pageSize',
          String(query.pageSize)
        )
        .set(
          'includeArchived',
          String(query.includeArchived)
        )
        .set(
          'sortBy',
          query.sortBy
        )
        .set(
          'sortDirection',
          query.sortDirection
        );


    this.httpClient
      .get<
        PagedResult<VacationProperty>
      >(
`${this.apiBaseUrl}/properties`,
        {
          params
        }
      )
      .pipe(
        finalize(() => {
          this.propertiesLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: result => {
          this.propertiesState.set({
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
          this.propertiesState.set(
            EMPTY_PROPERTY_RESULT
          );


          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }


  loadProperty(
    propertyId: string
  ): void {
    const normalizedPropertyId =
      propertyId.trim();


    if (
      !normalizedPropertyId ||
      this.propertyDetailsLoadingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.selectedPropertyState.set(
      null
    );


    this.propertyDetailsLoadingState.set(
      true
    );


    this.httpClient
      .get<VacationProperty>(
        `${this.apiBaseUrl}/properties/${normalizedPropertyId}`
      )
      .pipe(
        finalize(() => {
          this.propertyDetailsLoadingState.set(
            false
          );
        })
      )
      .subscribe({
        next: property => {
          this.selectedPropertyState.set(
            property
          );
        },


        error: error => {
          this.errorMessageState.set(
            parseApiError(error).message
          );
        }
      });
  }


  


  createProperty(
    request:
      CreateVacationPropertyRequest
  ): void {
    if (
      this.propertyProcessingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.propertyProcessingState.set(
      true
    );


    this.httpClient
      .post<{
        propertyId: string;
      }>(`${this.apiBaseUrl}/properties`,
        request
      )
      .pipe(
        finalize(() => {
          this.propertyProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: response => {
          this.globalNotifications.showSuccess(
            'Vacation property created successfully.'
          );


          this.loadProperties();


          if (response.propertyId) {
            this.loadProperty(
              response.propertyId
            );
          }
        },


        error: error => {
          this.handlePropertyError(error);
        }
      });
  }


  updateProperty(
    propertyId: string,
    request:
      UpdateVacationPropertyRequest
  ): void {
    const normalizedPropertyId =
      propertyId.trim();


    if (
      !normalizedPropertyId ||
      this.propertyProcessingState()
    ) {
      return;
    }


    this.clearError();
    this.clearSuccessMessage();


    this.propertyProcessingState.set(
      true
    );


    this.httpClient
      .put<void>(
        `${this.apiBaseUrl}/properties/${normalizedPropertyId}`,
        request
      )
      .pipe(
        finalize(() => {
          this.propertyProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: () => {
          this.globalNotifications.showSuccess(
            'Vacation property updated successfully.'
          );


          this.loadProperty(
            normalizedPropertyId
          );


          this.loadProperties();
        },


        error: error => {
          this.handlePropertyError(error);
        }
      });
  }


  archiveProperty(
    propertyId: string,
    rowVersion: string
  ): void {
    const normalizedPropertyId =
      propertyId.trim();


    if (
      !normalizedPropertyId ||
      this.propertyProcessingState()
    ) {
      return;
    }


    const request:
      ArchiveVacationPropertyRequest = {
        rowVersion
      };


    this.clearError();
    this.clearSuccessMessage();


    this.propertyProcessingState.set(
      true
    );


    this.httpClient
      .patch<void>(
        `${this.apiBaseUrl}/properties/${normalizedPropertyId}/archive`,
        request
      )
      .pipe(
        finalize(() => {
          this.propertyProcessingState.set(
            false
          );
        })
      )
      .subscribe({
        next: () => {
          this.globalNotifications.showSuccess(
            'Vacation property archived successfully.'
          );


          this.selectedPropertyState.set(
            null
          );


          this.loadProperties();
        },


        error: error => {
          this.clearError();
          this.globalNotifications.showError(
            parseApiError(error).message
          );
        }
      });
  }


  loadDashboard(): void {
    this.loadProfile();


    this.loadProperties({
      pageNumber: 1,
      pageSize: 10,
      includeArchived: false,
      sortBy: 'createdAtUtc',
      sortDirection: 'desc'
    });
  }


  refreshProperties(): void {
    this.loadProperties(
      this.currentPropertyQueryState()
    );
  }


  goToPreviousPropertyPage(): void {
    const result =
      this.propertiesState();


    if (
      !result.hasPreviousPage ||
      this.propertiesLoadingState()
    ) {
      return;
    }


    this.loadProperties({
      ...this.currentPropertyQueryState(),
      pageNumber:
        result.pageNumber - 1
    });
  }


  goToNextPropertyPage(): void {
    const result =
      this.propertiesState();


    if (
      !result.hasNextPage ||
      this.propertiesLoadingState()
    ) {
      return;
    }


    this.loadProperties({
      ...this.currentPropertyQueryState(),
      pageNumber:
        result.pageNumber + 1
    });
  }


  clearSelectedProperty(): void {
    this.selectedPropertyState.set(
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


  private handlePropertyError(
    error: unknown
  ): void {
    const parsedError =
      parseApiError(error);

    if (
      parsedError.code ===
      'PROPERTY_NAME_ALREADY_EXISTS'
    ) {
      this.clearError();
      this.globalNotifications.showError(
        parsedError.message
      );
      return;
    }

    this.clearError();
    this.globalNotifications.showError(
      parsedError.message
    );
  }
}
