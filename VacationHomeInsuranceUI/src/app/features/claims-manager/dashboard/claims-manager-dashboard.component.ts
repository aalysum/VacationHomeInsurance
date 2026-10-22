import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ClaimsManagerService } from '../../../core/services/claims-manager.service';
import { ClaimStatus } from '../../../core/models/customer/claims/claim-status.enum';

@Component({
	selector: 'app-claims-manager-dashboard',
	standalone: true,
	imports: [CommonModule, RouterLink],
	templateUrl: './claims-manager-dashboard.component.html',
	styleUrl: './claims-manager-dashboard.component.css'
})
export class ClaimsManagerDashboardComponent implements OnInit {
	private readonly claimsService = inject(ClaimsManagerService);

	readonly dashboard = this.claimsService.dashboard;
	readonly loading = this.claimsService.dashboardLoading;
	readonly errorMessage = this.claimsService.errorMessage;

	ngOnInit(): void {
		this.claimsService.loadDashboard();
	}

	statusName(status: ClaimStatus): string {
		switch (status) {
			case ClaimStatus.Submitted: return 'Submitted';
			case ClaimStatus.UnderReview: return 'Under review';
			case ClaimStatus.AdditionalInformationRequested: return 'Awaiting information';
			case ClaimStatus.Approved: return 'Approved';
			case ClaimStatus.Rejected: return 'Rejected';
			case ClaimStatus.Paid: return 'Paid';
			case ClaimStatus.Closed: return 'Closed';
			case ClaimStatus.Withdrawn: return 'Withdrawn';
			default: return 'Unknown';
		}
	}

	statusClass(status: ClaimStatus): string {
		switch (status) {
			case ClaimStatus.Submitted: return 'status-submitted';
			case ClaimStatus.UnderReview: return 'status-review';
			case ClaimStatus.AdditionalInformationRequested: return 'status-information';
			case ClaimStatus.Approved: return 'status-approved';
			case ClaimStatus.Paid: return 'status-paid';
			case ClaimStatus.Rejected: return 'status-rejected';
			case ClaimStatus.Withdrawn: return 'status-withdrawn';
			case ClaimStatus.Closed: return 'status-closed';
			default: return 'status-unknown';
		}
	}

	formatCurrency(value: number): string {
		return new Intl.NumberFormat('en-IN', {
			style: 'currency',
			currency: 'INR',
			minimumFractionDigits: 2,
			maximumFractionDigits: 2
		}).format(value);
	}
}
