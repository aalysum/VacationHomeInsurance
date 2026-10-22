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
  finalize,
  switchMap
} from 'rxjs';


import {
    ADMIN_NAVIGATION_ITEMS,
    AdminNavigationItem
} from '../../../core/constants/admin-navigation.constants';


import {
    CoverageType
} from '../../../core/models/admin/quotes/coverage-type.enum';


import {
    ProductType
} from '../../../core/models/admin/quotes/product-type.enum';


import {
    ProductCoverage
} from '../../../core/models/admin/products/product-coverage.model';


import {
    ProductDetails
} from '../../../core/models/admin/products/product-details.model';


import {
    AdminService
} from '../../../core/services/admin.service';


import {
    AuthService
} from '../../../core/services/auth.service';


type ProductStatusFilter =
    | 'all'
    | 'active'
    | 'inactive';


type ProductSortOption =
    | 'name-asc'
    | 'name-desc'
    | 'status-active'
    | 'coverage-desc';


type ProductDecisionMode =
    | 'activate'
    | 'deactivate'
    | null;


@Component({
    selector:
        'app-admin-products',


    standalone:
        true,


    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterLink
    ],


    templateUrl:
        './admin-products.component.html',


    styleUrl:
        './admin-products.component.css'
})
export class AdminProductsComponent
    implements OnInit {
    private readonly adminService =
        inject(AdminService);


    private readonly authService =
        inject(AuthService);


    private readonly formBuilder =
        inject(FormBuilder);


    private readonly router =
        inject(Router);


    readonly ProductType =
        ProductType;


    readonly CoverageType =
        CoverageType;


    readonly navigationItems =
        ADMIN_NAVIGATION_ITEMS;


    readonly pendingApplicationCount =
        this.adminService
            .pendingApplicationCount;


    readonly products =
        this.adminService.products;


    readonly productsLoading =
        this.adminService.productsLoading;


    readonly selectedProduct =
        this.adminService.selectedProduct;


    readonly productDetailsLoading =
        this.adminService
            .productDetailsLoading;


    readonly processingProductId =
        this.adminService
            .processingProductId;


    readonly processingCoverageId =
        this.adminService
            .processingCoverageId;


    readonly errorMessage =
        this.adminService.errorMessage;


    readonly productCount =
        this.adminService.productCount;


    readonly activeProductCount =
        this.adminService
            .activeProductCount;


    readonly inactiveProductCount =
        this.adminService
            .inactiveProductCount;


    readonly coverageCount =
        this.adminService.coverageCount;


    readonly mobileNavigationOpen =
        signal(false);


    readonly productDetailsOpen =
        signal(false);


    readonly decisionMode =
        signal<ProductDecisionMode>(
            null
        );


    readonly editingCoverageId =
        signal<string | null>(
            null
        );


    readonly successMessage =
        signal<string | null>(
            null
        );


    readonly filterForm =
        this.formBuilder.nonNullable.group({
            search: [
                ''
            ],


            status: [
                'all' as
                ProductStatusFilter
            ],


            sort: [
                'name-asc' as
                ProductSortOption
            ]
        });


        readonly coverageForm =
        this.formBuilder.nonNullable.group({
            coverageLimitPercentage: [
            0,
            [
                Validators.required,
                Validators.min(0.01),
                Validators.max(100)
            ]
            ],


            coverageCharge: [
            0,
            [
                Validators.required,
                Validators.min(0)
            ]
            ]
        });


    readonly filteredProducts =
        computed(() => {
            const controls =
                this.filterForm
                    .getRawValue();


            const search =
                controls.search
                    .trim()
                    .toLowerCase();


            const filteredProducts =
                this.products()
                    .filter(product => {
                        if (
                            controls.status ===
                            'active' &&
                            !product.isActive
                        ) {
                            return false;
                        }


                        if (
                            controls.status ===
                            'inactive' &&
                            product.isActive
                        ) {
                            return false;
                        }


                        if (!search) {
                            return true;
                        }


                        const productName =
                            product.name
                                ?.toLowerCase() ??
                            '';


                        const description =
                            product.description
                                ?.toLowerCase() ??
                            '';


                        const productType =
                            this.productTypeLabel(
                                product.productType
                            )
                                .toLowerCase();


                        return (
                            productName.includes(
                                search
                            ) ||
                            description.includes(
                                search
                            ) ||
                            productType.includes(
                                search
                            )
                        );
                    });


            const sortedProducts = [
                ...filteredProducts
            ];


            switch (controls.sort) {
                case 'name-desc':
                    return sortedProducts.sort(
                        (
                            first,
                            second
                        ) =>
                            second.name.localeCompare(
                                first.name
                            )
                    );


                case 'status-active':
                    return sortedProducts.sort(
                        (
                            first,
                            second
                        ) => {
                            if (
                                first.isActive !==
                                second.isActive
                            ) {
                                return first.isActive
                                    ? -1
                                    : 1;
                            }


                            return first.name.localeCompare(
                                second.name
                            );
                        }
                    );


                case 'coverage-desc':
                    return sortedProducts.sort(
                        (
                            first,
                            second
                        ) =>
                            second.coverages.length -
                            first.coverages.length ||
                            first.name.localeCompare(
                                second.name
                            )
                    );


                default:
                    return sortedProducts.sort(
                        (
                            first,
                            second
                        ) =>
                            first.name.localeCompare(
                                second.name
                            )
                    );
            }
        });


    readonly mandatoryCoverages =
        computed(
            () =>
                this.selectedProduct()
                    ?.coverages
                    .filter(
                        coverage =>
                            coverage.coverageType ===
                            CoverageType.Mandatory
                    ) ??
                []
        );


    readonly optionalCoverages =
        computed(
            () =>
                this.selectedProduct()
                    ?.coverages
                    .filter(
                        coverage =>
                            coverage.coverageType ===
                            CoverageType.Optional
                    ) ??
                []
        );


get coverageLimitControl() {
  return this.coverageForm
    .controls
    .coverageLimitPercentage;
}


get coverageChargeControl() {
  return this.coverageForm
    .controls
    .coverageCharge;
}


    ngOnInit(): void {
        this.adminService.clearError();


        this.adminService
            .loadPendingClaimsManagers();


        this.adminService
            .loadProducts();
    }


    refreshProducts(): void {
        if (
            this.productsLoading() ||
            this.processingProductId() ||
            this.processingCoverageId()
        ) {
            return;
        }


        this.successMessage.set(
            null
        );


        this.adminService.clearError();


        this.adminService
            .refreshProducts();
    }


    clearFilters(): void {
        this.filterForm.setValue({
            search:
                '',


            status:
                'all',


            sort:
                'name-asc'
        });
    }


    openProduct(
        product: ProductDetails
    ): void {
        if (
            !product.id
        ) {
            return;
        }


        this.successMessage.set(
            null
        );


        this.decisionMode.set(
            null
        );


        this.cancelCoverageEditing();


        this.adminService.clearError();


        this.productDetailsOpen.set(
            true
        );


        this.adminService
            .loadProduct(
                product.id
            );
    }


    closeProductDetails(): void {
        if (
            this.processingProductId() ||
            this.processingCoverageId() ||
            this.productDetailsLoading()
        ) {
            return;
        }


        this.productDetailsOpen.set(
            false
        );


        this.decisionMode.set(
            null
        );


        this.cancelCoverageEditing();


        this.adminService
            .clearSelectedProduct();
    }


    openActivateDecision(): void {
        const product =
            this.selectedProduct();


        if (
            !product ||
            product.isActive ||
            this.processingProductId() ||
            this.processingCoverageId()
        ) {
            return;
        }


        this.cancelCoverageEditing();


        this.successMessage.set(
            null
        );


        this.decisionMode.set(
            'activate'
        );


        this.adminService.clearError();
    }


    openDeactivateDecision(): void {
        const product =
            this.selectedProduct();


        if (
            !product ||
            !product.isActive ||
            this.processingProductId() ||
            this.processingCoverageId()
        ) {
            return;
        }


        this.cancelCoverageEditing();


        this.successMessage.set(
            null
        );


        this.decisionMode.set(
            'deactivate'
        );


        this.adminService.clearError();
    }


    cancelDecision(): void {
        if (
            this.processingProductId()
        ) {
            return;
        }


        this.decisionMode.set(
            null
        );


        this.adminService.clearError();
    }


    confirmProductStatusChange(): void {
        const product =
            this.selectedProduct();


        const mode =
            this.decisionMode();


        if (
            !product ||
            !mode ||
            this.processingProductId() ||
            this.processingCoverageId()
        ) {
            return;
        }


        const isActive =
            mode ===
            'activate';


        this.successMessage.set(
            null
        );


        this.adminService.clearError();


        this.adminService
            .changeProductStatus(
                product,
                isActive
            )
            .pipe(
                finalize(() => {
                    this.decisionMode.set(
                        null
                    );
                })
            )
            .subscribe({
                next: updatedProduct => {
                    this.successMessage.set(
                        updatedProduct.isActive
                            ? `${updatedProduct.name} was activated successfully.`
                            : `${updatedProduct.name} was deactivated successfully.`
                    );
                }
            });
    }


    beginCoverageEditing(
        coverage: ProductCoverage
    ): void {
        if (
            this.processingCoverageId() ||
            this.processingProductId()
        ) {
            return;
        }


        this.decisionMode.set(
            null
        );


        this.successMessage.set(
            null
        );


        this.editingCoverageId.set(
            coverage.coverageId
        );


        this.coverageForm.setValue({
        coverageLimitPercentage:
            coverage.coverageLimitPercentage,


        coverageCharge:
            coverage.coverageCharge
        });


        this.coverageForm
            .markAsPristine();


        this.coverageForm
            .markAsUntouched();


        this.adminService.clearError();
    }


    cancelCoverageEditing(): void {
        if (
            this.processingCoverageId()
        ) {
            return;
        }


        this.editingCoverageId.set(
            null
        );


        this.coverageForm.reset({
        coverageLimitPercentage: 0,
        coverageCharge: 0
        });
    }


saveCoverage(
  coverage: ProductCoverage
): void {
  const product =
    this.selectedProduct();


  if (
    !product ||
    this.editingCoverageId() !==
      coverage.coverageId ||
    this.processingCoverageId() ||
    this.processingProductId()
  ) {
    return;
  }


  if (
    this.coverageForm.invalid
  ) {
    this.coverageForm
      .markAllAsTouched();


    return;
  }


  const formValue =
    this.coverageForm
      .getRawValue();


  const coverageLimitPercentage =
    Number(
      formValue
        .coverageLimitPercentage
    );


  const coverageCharge =
    Number(
      formValue
        .coverageCharge
    );


  if (
    !Number.isFinite(
      coverageLimitPercentage
    ) ||
    coverageLimitPercentage <
      0.01 ||
    coverageLimitPercentage >
      100
  ) {
    this.coverageLimitControl
      .setErrors({
        invalidPercentage:
          true
      });


    this.coverageLimitControl
      .markAsTouched();


    return;
  }


  if (
    !Number.isFinite(
      coverageCharge
    ) ||
    coverageCharge < 0
  ) {
    this.coverageChargeControl
      .setErrors({
        invalidCharge:
          true
      });


    this.coverageChargeControl
      .markAsTouched();


    return;
  }


  this.successMessage.set(
    null
  );


  this.adminService.clearError();


  this.adminService
    .changeCoverageLimit(
      product,
      coverage,
      coverageLimitPercentage
    )
    .pipe(
      switchMap(
        updatedProduct => {
          const refreshedCoverage =
            updatedProduct.coverages.find(
              item =>
                item.coverageId ===
                coverage.coverageId
            );


          if (
            !refreshedCoverage
          ) {
            throw new Error(
              'Updated Coverage could not be found.'
            );
          }


          return this.adminService
            .changeCoverageCharge(
              updatedProduct,
              refreshedCoverage,
              coverageCharge
            );
        }
      ),
      finalize(() => {
        this.editingCoverageId.set(
          null
        );


        this.coverageForm.reset({
          coverageLimitPercentage:
            0,


          coverageCharge:
            0
        });
      })
    )
    .subscribe({
      next: updatedProduct => {
        this.successMessage.set(
          `The Coverage settings for ${coverage.coverageName} were updated successfully.`
        );


        const refreshedCoverage =
          updatedProduct.coverages.find(
            item =>
              item.coverageId ===
              coverage.coverageId
          );


        if (
          !refreshedCoverage
        ) {
          this.adminService
            .loadProduct(
              updatedProduct.id
            );
        }
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


    isCoverageBeingProcessed(
        coverageId: string
    ): boolean {
        return (
            this.processingCoverageId() ===
            coverageId
        );
    }


    isCoverageBeingEdited(
        coverageId: string
    ): boolean {
        return (
            this.editingCoverageId() ===
            coverageId
        );
    }


    activeCoverageCount(
        product: ProductDetails
    ): number {
        return product.coverages
            .filter(
                coverage =>
                    coverage.isActive
            )
            .length;
    }


    mandatoryCoverageCount(
        product: ProductDetails
    ): number {
        return product.coverages
            .filter(
                coverage =>
                    coverage.coverageType ===
                    CoverageType.Mandatory
            )
            .length;
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
                return 'Unknown Product';
        }
    }


    coverageTypeLabel(
        type: CoverageType
    ): string {
        switch (type) {
            case CoverageType.Mandatory:
                return 'Mandatory';


            case CoverageType.Optional:
                return 'Optional';


            default:
                return 'Unknown';
        }
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
        this.navigateTo(
            '/admin/quotes'
        );
    }


    goToCustomers(): void {
        this.navigateTo(
            '/admin/customers'
        );
    }


    goToProducts(): void {
        this.closeMobileNavigation();
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


    private navigateTo(
        route: string
    ): void {
        this.closeMobileNavigation();


        void this.router.navigateByUrl(
            route
        );
    }
}

