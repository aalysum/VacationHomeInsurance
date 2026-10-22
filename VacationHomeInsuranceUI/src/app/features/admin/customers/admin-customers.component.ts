import {
	Component,
	OnInit,
	computed,
	inject,
	signal
} from '@angular/core';
import {
	FormBuilder,
	ReactiveFormsModule
} from '@angular/forms';
import {
	Router,
	RouterLink,
	RouterLinkActive
} from '@angular/router';
import {
	finalize
} from 'rxjs';
import {
	ADMIN_NAVIGATION_ITEMS,
	AdminNavigationItem
} from '../../../core/constants/admin-navigation.constants';
import {
	AdminCustomerDetails
} from '../../../core/models/admin/admin-customer-details.model';
import {
	AdminCustomerSummary
} from '../../../core/models/admin/admin-customer-summary.model';
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
	AdminService
} from '../../../core/services/admin.service';
import {
	AuthService
} from '../../../core/services/auth.service';
import {
	GlobalNotificationService
} from '../../../core/services/global-notification.service';

type CustomerStatusFilter =
	| 'all'
	| 'active'
	| 'inactive';

type CustomerSortOption =
	| 'created-desc'
	| 'created-asc'
	| 'name-asc'
	| 'name-desc';

type CustomerDecisionMode =
	| 'activate'
	| 'deactivate'
	| null;

@Component({
	selector:
		'app-admin-customers',
	standalone:
		true,
	imports: [
		ReactiveFormsModule,
		RouterLink,
		RouterLinkActive
	],
	templateUrl:
		'./admin-customers.component.html',
	styleUrl:
		'./admin-customers.component.css'
})
export class AdminCustomersComponent
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

	readonly CustomerStatus =
		CustomerStatus;

	readonly navigationItems =
		ADMIN_NAVIGATION_ITEMS;

	readonly pendingApplicationCount =
		this.adminService
			.pendingApplicationCount;

	readonly customers =
		this.adminService.customers;

	readonly customersLoading =
		this.adminService
			.customersLoading;

	readonly selectedCustomer =
		this.adminService
			.selectedCustomer;

	readonly customerDetailsLoading =
		this.adminService
			.customerDetailsLoading;

	readonly customerProperties =
		this.adminService
			.customerProperties;

	readonly customerPropertiesLoading =
		this.adminService
			.customerPropertiesLoading;

	readonly updatingUserId =
		this.adminService.updatingUserId;

	readonly errorMessage =
		this.adminService.errorMessage;

	readonly mobileNavigationOpen =
		signal(false);

	readonly customerDetailsOpen =
		signal(false);

	readonly decisionMode =
		signal<CustomerDecisionMode>(
			null
		);

	readonly successMessage =
		signal<string | null>(null);

	readonly filterForm =
		this.formBuilder.nonNullable.group({
			name: [''],
			status: [
				'all' as CustomerStatusFilter
			],
			sort: [
				'created-desc' as CustomerSortOption
			],
			pageSize: [10 as 10 | 25 | 50]
		});

	readonly propertyControls =
		this.formBuilder.nonNullable.group({
			includeArchived: [true],
			pageSize: [10 as 10 | 25 | 50]
		});

	readonly loadedCustomers =
		computed(
			() => this.customers().items
		);

	readonly totalCustomers =
		computed(
			() => this.customers().totalCount
		);

	readonly displayedActiveCustomers =
		computed(
			() =>
				this.loadedCustomers()
					.filter(
						customer =>
							customer.status ===
							CustomerStatus.Active
					)
					.length
		);

	readonly displayedInactiveCustomers =
		computed(
			() =>
				this.loadedCustomers()
					.filter(
						customer =>
							customer.status ===
							CustomerStatus.Inactive
					)
					.length
		);

	readonly selectedCustomerName =
		computed(() => {
			const customer =
				this.selectedCustomer();

			if (!customer) {
				return '';
			}

			return [
				customer.firstName,
				customer.lastName
			]
				.filter(value => Boolean(value))
				.join(' ')
				.trim();
		});

	ngOnInit(): void {
		this.adminService.clearError();
		this.adminService
			.loadPendingClaimsManagers();
		this.loadCustomers(1);
	}

	searchCustomers(): void {
		if (this.customersLoading()) {
			return;
		}

		this.successMessage.set(null);
		this.adminService.clearError();
		this.loadCustomers(1);
	}

	clearFilters(): void {
		if (this.customersLoading()) {
			return;
		}

		this.filterForm.setValue({
			name: '',
			status: 'all',
			sort: 'created-desc',
			pageSize: 10
		});
		this.successMessage.set(null);
		this.adminService.clearError();
		this.loadCustomers(1);
	}

	refreshCustomers(): void {
		if (this.customersLoading()) {
			return;
		}

		this.successMessage.set(null);
		this.adminService.clearError();
		this.loadCustomers(
			this.customers().pageNumber || 1
		);
	}

	changeCustomerPage(pageNumber: number): void {
		const page = this.customers();

		if (
			pageNumber < 1 ||
			(
				page.totalPages > 0 &&
				pageNumber > page.totalPages
			) ||
			this.customersLoading()
		) {
			return;
		}

		this.loadCustomers(pageNumber);
	}

	openCustomer(
		customer: AdminCustomerSummary
	): void {
		if (!customer.customerId?.trim()) {
			return;
		}

		this.successMessage.set(null);
		this.decisionMode.set(null);
		this.adminService.clearError();
		this.customerDetailsOpen.set(true);
		this.adminService.loadCustomer(
			customer.customerId
		);
	}

	closeCustomerDetails(): void {
		if (
			this.updatingUserId() ||
			this.customerDetailsLoading()
		) {
			return;
		}

		this.customerDetailsOpen.set(false);
		this.decisionMode.set(null);
		this.adminService.clearSelectedCustomer();
	}

	openActivateDecision(): void {
		if (
			!this.selectedCustomer() ||
			this.updatingUserId()
		) {
			return;
		}

		this.successMessage.set(null);
		this.decisionMode.set('activate');
		this.adminService.clearError();
	}

	openDeactivateDecision(): void {
		if (
			!this.selectedCustomer() ||
			this.updatingUserId()
		) {
			return;
		}

		this.successMessage.set(null);
		this.decisionMode.set('deactivate');
		this.adminService.clearError();
	}

	cancelDecision(): void {
		if (this.updatingUserId()) {
			return;
		}

		this.decisionMode.set(null);
		this.adminService.clearError();
	}

	confirmStatusChange(): void {
		const customer = this.selectedCustomer();
		const mode = this.decisionMode();

		if (
			!customer ||
			!mode ||
			this.updatingUserId()
		) {
			return;
		}

		const isActive = mode === 'activate';
		this.successMessage.set(null);
		this.adminService.clearError();
		this.adminService
			.changeCustomerStatus(
				customer,
				isActive
			)
			.pipe(
				finalize(() => {
					this.decisionMode.set(null);
				})
			)
			.subscribe({
				next: () => {
					this.notifications.showSuccess(
						isActive
							? 'The Customer account was activated successfully.'
							: 'The Customer account was deactivated successfully.'
					);
				}
			});
	}

	changePropertyPage(pageNumber: number): void {
		const customer = this.selectedCustomer();
		const page = this.customerProperties();

		if (
			!customer ||
			pageNumber < 1 ||
			(
				page.totalPages > 0 &&
				pageNumber > page.totalPages
			) ||
			this.customerPropertiesLoading()
		) {
			return;
		}

		const controls =
			this.propertyControls.getRawValue();
		this.adminService.loadCustomerProperties(
			customer.customerId,
			{
				pageNumber,
				pageSize: controls.pageSize,
				includeArchived: controls.includeArchived
			}
		);
	}

	updatePropertyFilters(): void {
		const customer = this.selectedCustomer();

		if (
			!customer ||
			this.customerPropertiesLoading()
		) {
			return;
		}

		const values =
			this.propertyControls.getRawValue();
		this.adminService.loadCustomerProperties(
			customer.customerId,
			{
				pageNumber: 1,
				pageSize: values.pageSize,
				includeArchived: values.includeArchived
			}
		);
	}

	openNavigationItem(
		item: AdminNavigationItem
	): void {
		if (!item.available || !item.route) {
			return;
		}

		this.navigateTo(item.route);
	}

	clearError(): void {
		this.adminService.clearError();
	}

	clearSuccessMessage(): void {
		this.successMessage.set(null);
	}

	toggleMobileNavigation(): void {
		this.mobileNavigationOpen.update(
			open => !open
		);
	}

	closeMobileNavigation(): void {
		this.mobileNavigationOpen.set(false);
	}

	goToDashboard(): void {
		this.navigateTo('/admin/dashboard');
	}

	goToApplications(): void {
		this.navigateTo('/admin/claims-managers/pending');
	}

	goToQuotes(): void {
		this.navigateTo('/admin/quotes');
	}

	goToCustomers(): void {
		this.closeMobileNavigation();
	}

	goToProducts(): void {
		this.navigateTo('/admin/products');
	}

	goToPricing(): void {
		this.navigateTo('/admin/pricing');
	}

	goToOperations(): void {
		this.navigateTo('/admin/operations');
	}

	openChangePassword(): void {
		this.navigateTo('/account/security/change-password');
	}

	openMfaSettings(): void {
		this.navigateTo('/account/security/mfa');
	}

	logout(): void {
		this.closeMobileNavigation();
		this.authService.logout();
	}

	customerName(
		customer: AdminCustomerSummary | AdminCustomerDetails
	): string {
		const name = [
			customer.firstName,
			customer.lastName
		]
			.filter(value => Boolean(value?.trim()))
			.map(value => value.trim())
			.join(' ')
			.trim();

		return name || 'Customer';
	}

	customerInitials(
		customer: AdminCustomerSummary | AdminCustomerDetails
	): string {
		const firstInitial =
			customer.firstName?.trim().charAt(0) ?? '';
		const lastInitial =
			customer.lastName?.trim().charAt(0) ?? '';

		return `firstInitial{lastInitial}`
			.toUpperCase() || 'CU';
	}

	customerStatusLabel(status: CustomerStatus): string {
		switch (status) {
			case CustomerStatus.Active:
				return 'Active';
			case CustomerStatus.Inactive:
				return 'Inactive';
			default:
				return 'Unknown';
		}
	}

	propertyTypeLabel(type: PropertyType): string {
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
				return 'Unknown';
		}
	}

	occupancyLabel(occupancy: OccupancyFrequency): string {
		switch (occupancy) {
			case OccupancyFrequency.High:
				return 'High';
			case OccupancyFrequency.Medium:
				return 'Medium';
			case OccupancyFrequency.Low:
				return 'Low';
			default:
				return 'Unknown';
		}
	}

	formatDate(value: string | null): string {
		if (!value || !value.trim()) {
			return 'Not available';
		}

		const date = new Date(value);
		if (Number.isNaN(date.getTime())) {
			return 'Not available';
		}

		return new Intl.DateTimeFormat(
			'en-IN',
			{
				day: '2-digit',
				month: 'short',
				year: 'numeric'
			}
		).format(date);
	}

	formatCurrency(value: number): string {
		if (!Number.isFinite(value)) {
			return 'Not available';
		}

		return new Intl.NumberFormat(
			'en-IN',
			{
				style: 'currency',
				currency: 'INR',
				minimumFractionDigits: 2,
				maximumFractionDigits: 2
			}
		).format(value);
	}

	propertyAddress(property: VacationProperty): string {
		const address = [
			property.addressLine1,
			property.addressLine2,
			property.city,
			property.state,
			property.postalCode,
			property.country
		]
			.filter(
				(value): value is string =>
					typeof value === 'string' &&
					value.trim().length > 0
			)
			.map(value => value.trim())
			.join(', ');

		return address || 'Property address not available';
	}

	customerAddress(customer: AdminCustomerDetails): string {
		const address = [
			customer.city,
			customer.state,
			customer.postalCode,
			customer.country
		]
			.filter(
				(value): value is string =>
					typeof value === 'string' &&
					value.trim().length > 0
			)
			.map(value => value.trim())
			.join(', ');

		return address || 'Profile address not available';
	}

	private loadCustomers(pageNumber: number): void {
		const values = this.filterForm.getRawValue();
		const sort = this.resolveCustomerSort(values.sort);
		const status = this.resolveCustomerStatus(values.status);

		this.adminService.loadCustomers({
			pageNumber,
			pageSize: values.pageSize,
			name: values.name.trim() || undefined,
			status,
			sortBy: sort.sortBy,
			sortDirection: sort.sortDirection
		});
	}

	private resolveCustomerStatus(
		value: CustomerStatusFilter
	): CustomerStatus | undefined {
		switch (value) {
			case 'active':
				return CustomerStatus.Active;
			case 'inactive':
				return CustomerStatus.Inactive;
			default:
				return undefined;
		}
	}

	private resolveCustomerSort(
		value: CustomerSortOption
	): {
		sortBy: string;
		sortDirection: 'asc' | 'desc';
	} {
		switch (value) {
			case 'created-asc':
				return {
					sortBy: 'createdAtUtc',
					sortDirection: 'asc'
				};
			case 'name-asc':
				return {
					sortBy: 'name',
					sortDirection: 'asc'
				};
			case 'name-desc':
				return {
					sortBy: 'name',
					sortDirection: 'desc'
				};
			default:
				return {
					sortBy: 'createdAtUtc',
					sortDirection: 'desc'
				};
		}
	}

	private navigateTo(route: string): void {
		this.closeMobileNavigation();
		void this.router.navigateByUrl(route);
	}
}
