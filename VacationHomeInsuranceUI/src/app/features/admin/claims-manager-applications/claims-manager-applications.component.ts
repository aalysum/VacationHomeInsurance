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
	RouterLink,
	RouterLinkActive
} from '@angular/router';
import {
	ADMIN_NAVIGATION_ITEMS,
	AdminNavigationItem
} from '../../../core/constants/admin-navigation.constants';
import {
	PendingClaimsManagerResponse
} from '../../../core/models/admin/pending-claims-manager-response.model';
import {
	AdminService
} from '../../../core/services/admin.service';
import {
	AuthService
} from '../../../core/services/auth.service';

type DecisionMode =
	| 'approve'
	| 'reject'
	| null;

@Component({
	selector:
		'app-claims-manager-applications',
	standalone:
		true,
	imports: [
		ReactiveFormsModule,
		RouterLink,
		RouterLinkActive
	],
	templateUrl:
		'./claims-manager-applications.component.html',
	styleUrl:
		'./claims-manager-applications.component.css'
})
export class ClaimsManagerApplicationsComponent
	implements OnInit {
	private readonly adminService =
		inject(AdminService);
	private readonly authService =
		inject(AuthService);
	private readonly formBuilder =
		inject(FormBuilder);
	private readonly router =
		inject(Router);

	readonly navigationItems =
		ADMIN_NAVIGATION_ITEMS;

	readonly applications =
		this.adminService
			.pendingClaimsManagers;

	readonly loading =
		this.adminService.loading;

	readonly errorMessage =
		this.adminService.errorMessage;

	readonly processingApplicationId =
		this.adminService
			.processingApplicationId;

	readonly downloadingApplicationId =
		this.adminService
			.downloadingApplicationId;

	readonly pendingApplicationCount =
		this.adminService
			.pendingApplicationCount;

	readonly mobileNavigationOpen =
		signal(false);

	readonly searchTerm =
		signal('');

	readonly selectedApplication =
		signal<PendingClaimsManagerResponse | null>(
			null
		);

	readonly decisionMode =
		signal<DecisionMode>(null);

	readonly successMessage =
		signal<string | null>(null);

	readonly filteredApplications =
		computed(() => {
			const normalizedSearch =
				this.searchTerm()
					.trim()
					.toLowerCase();

			if (!normalizedSearch) {
				return this.applications();
			}

			return this.applications()
				.filter(application => {
					const applicationId =
						application.applicationId
							.toLowerCase();
					const professionalSummary =
						application
							.professionalSummary
							?.toLowerCase() ?? '';
					const yearsOfExperience =
						application
							.yearsOfExperience
							.toString();

					return (
						applicationId.includes(
							normalizedSearch
						) ||
						professionalSummary.includes(
							normalizedSearch
						) ||
						yearsOfExperience.includes(
							normalizedSearch
						)
					);
				});
		});

	readonly rejectionForm =
		this.formBuilder.nonNullable.group({
			reason: [
				'',
				[
					Validators.required,
					Validators.minLength(20),
					Validators.maxLength(1000)
				]
			]
		});

	get rejectionReasonControl() {
		return this.rejectionForm
			.controls
			.reason;
	}

	ngOnInit(): void {
		this.adminService.clearError();
		this.adminService
			.loadPendingClaimsManagers();
	}

	updateSearch(event: Event): void {
		const input =
			event.target as HTMLInputElement;
		this.searchTerm.set(input.value);
	}

	clearSearch(): void {
		this.searchTerm.set('');
	}

	selectApplication(
		application: PendingClaimsManagerResponse
	): void {
		this.selectedApplication.set(application);
		this.decisionMode.set(null);
		this.rejectionForm.reset();
		this.successMessage.set(null);
		this.adminService.clearError();
	}

	closeApplicationDetails(): void {
		if (this.processingApplicationId()) {
			return;
		}

		this.selectedApplication.set(null);
		this.decisionMode.set(null);
		this.rejectionForm.reset();
		this.adminService.clearError();
	}

	openApproveDecision(): void {
		if (
			!this.selectedApplication() ||
			this.processingApplicationId()
		) {
			return;
		}

		this.decisionMode.set('approve');
		this.rejectionForm.reset();
		this.successMessage.set(null);
		this.adminService.clearError();
	}

	openRejectDecision(): void {
		if (
			!this.selectedApplication() ||
			this.processingApplicationId()
		) {
			return;
		}

		this.decisionMode.set('reject');
		this.rejectionForm.reset();
		this.successMessage.set(null);
		this.adminService.clearError();
	}

	cancelDecision(): void {
		if (this.processingApplicationId()) {
			return;
		}

		this.decisionMode.set(null);
		this.rejectionForm.reset();
		this.adminService.clearError();
	}

	confirmApproval(): void {
		const application =
			this.selectedApplication();

		if (
			!application ||
			this.processingApplicationId()
		) {
			return;
		}

		this.successMessage.set(null);
		this.adminService.clearError();
		this.adminService
			.approveClaimsManager(
				application.applicationId
			)
			.subscribe({
				next: () => {
					this.successMessage.set(
						'The Claims Manager application was approved successfully.'
					);
					this.selectedApplication.set(null);
					this.decisionMode.set(null);
					this.rejectionForm.reset();
				}
			});
	}

	confirmRejection(): void {
		const application =
			this.selectedApplication();

		if (
			!application ||
			this.processingApplicationId()
		) {
			return;
		}

		if (this.rejectionForm.invalid) {
			this.rejectionForm.markAllAsTouched();
			return;
		}

		const reason =
			this.rejectionForm
				.getRawValue()
				.reason
				.trim();

		if (!reason) {
			this.rejectionReasonControl.setErrors({
				required: true
			});
			return;
		}

		this.successMessage.set(null);
		this.adminService.clearError();
		this.adminService
			.rejectClaimsManager(
				application.applicationId,
				reason
			)
			.subscribe({
				next: () => {
					this.successMessage.set(
						'The Claims Manager application was rejected successfully.'
					);
					this.selectedApplication.set(null);
					this.decisionMode.set(null);
					this.rejectionForm.reset();
				}
			});
	}

	downloadDocument(
		application: PendingClaimsManagerResponse
	): void {
		if (
			this.downloadingApplicationId() ||
			this.processingApplicationId()
		) {
			return;
		}

		this.successMessage.set(null);
		this.adminService.clearError();
		this.adminService
			.downloadClaimsManagerDocument(
				application.applicationId
			)
			.subscribe({
				next: file => {
					this.adminService
						.saveDownloadedFile(file);
				}
			});
	}

	refreshApplications(): void {
		if (
			this.loading() ||
			this.processingApplicationId() ||
			this.downloadingApplicationId()
		) {
			return;
		}

		this.successMessage.set(null);
		this.adminService.clearError();
		this.adminService
			.refreshPendingClaimsManagers();
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
		this.closeMobileNavigation();
	}

	goToQuotes(): void {
		this.navigateTo('/admin/quotes');
	}

	goToCustomers(): void {
		this.navigateTo('/admin/customers');
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

	formatDate(value: string): string {
		if (!value || !value.trim()) {
			return 'Date unavailable';
		}

		const date = new Date(value);
		if (Number.isNaN(date.getTime())) {
			return 'Date unavailable';
		}

		return new Intl.DateTimeFormat(
			'en-IN',
			{
				day: '2-digit',
				month: 'short',
				year: 'numeric',
				hour: '2-digit',
				minute: '2-digit'
			}
		).format(date);
	}

	shortApplicationId(applicationId: string): string {
		const normalizedApplicationId =
			applicationId?.trim() ?? '';

		if (!normalizedApplicationId) {
			return 'UNKNOWN';
		}

		return normalizedApplicationId
			.slice(0, 8)
			.toUpperCase();
	}

	isProcessing(applicationId: string): boolean {
		return this.processingApplicationId() === applicationId;
	}

	isDownloading(applicationId: string): boolean {
		return this.downloadingApplicationId() === applicationId;
	}

	private navigateTo(route: string): void {
		this.closeMobileNavigation();
		void this.router.navigateByUrl(route);
	}
}
