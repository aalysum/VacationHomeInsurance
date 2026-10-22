import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ClaimsManagerService } from '../../../core/services/claims-manager.service';
import { ClaimStatus } from '../../../core/models/customer/claims/claim-status.enum';
import { ClaimType } from '../../../core/models/customer/claims/claim-type.enum';
import { ClaimDocumentType } from '../../../core/models/customer/claims/claim-document-type.enum';
import {
	ClaimsManagerClaimDetail,
	ClaimsManagerPolicyDocument
} from '../../../core/models/claims-manager/claims-manager-claim-detail.model';
import { ApproveClaimRequest } from '../../../core/models/claims-manager/requests/approve-claim-request.model';
import { ClaimTimelineComponent } from '../../../shared/components/claim-timeline/claim-timeline.component';

@Component({
	selector: 'app-claims-manager-claims',
	standalone: true,
	imports: [CommonModule, FormsModule, RouterLink, ClaimTimelineComponent],
	templateUrl: './claims-manager-claims.component.html',
	styleUrl: './claims-manager-claims.component.css'
})
export class ClaimsManagerClaimsComponent implements OnInit {
	private readonly claimsService = inject(ClaimsManagerService);

	readonly claims = this.claimsService.claimItems;
	readonly selectedClaim = this.claimsService.selectedClaim;
	readonly loading = this.claimsService.claimsLoading;
	readonly detailsLoading = this.claimsService.claimDetailsLoading;
	readonly errorMessage = this.claimsService.errorMessage;
	readonly pageNumber = this.claimsService.pageNumber;
	readonly totalPages = this.claimsService.totalPages;
	readonly hasPreviousPage = this.claimsService.hasPreviousPage;
	readonly hasNextPage = this.claimsService.hasNextPage;
	readonly workflowProcessing = this.claimsService.workflowProcessing;
	readonly processingAction = this.claimsService.processingAction;
	readonly downloadingDocument = this.claimsService.downloadingDocument;
	readonly successMessage = this.claimsService.successMessage;
	readonly ClaimStatus = ClaimStatus;

	readonly statusOptions = [
		{ value: ClaimStatus.Submitted, label: 'Submitted' },
		{ value: ClaimStatus.UnderReview, label: 'Under review' },
		{ value: ClaimStatus.AdditionalInformationRequested, label: 'Additional information requested' },
		{ value: ClaimStatus.Approved, label: 'Approved' },
		{ value: ClaimStatus.Rejected, label: 'Rejected' },
		{ value: ClaimStatus.Paid, label: 'Paid' },
		{ value: ClaimStatus.Closed, label: 'Closed' },
		{ value: ClaimStatus.Withdrawn, label: 'Withdrawn' }
	] as const;

	readonly claimTypeOptions = [
		{ value: ClaimType.DwellingDamage, label: 'Dwelling damage' },
		{ value: ClaimType.OtherStructureDamage, label: 'Other structure damage' },
		{ value: ClaimType.PersonalPropertyDamage, label: 'Personal property damage' },
		{ value: ClaimType.Theft, label: 'Theft' },
		{ value: ClaimType.Flood, label: 'Flood' },
		{ value: ClaimType.Earthquake, label: 'Earthquake' },
		{ value: ClaimType.Wildfire, label: 'Wildfire' },
		{ value: ClaimType.Hurricane, label: 'Hurricane' },
		{ value: ClaimType.Fire, label: 'Fire' },
		{ value: ClaimType.Other, label: 'Other' }
	] as const;

	claimNumber = '';
	statusFilter: ClaimStatus | null = null;
	claimTypeFilter: ClaimType | null = null;
	customerIdFilter = '';
	fromDateFilter = '';
	toDateFilter = '';
	validationMessage = '';
	actionValidationMessage = '';
	informationMessage = '';
	rejectionReason = '';
	rejectionNotes = '';
	approvalNotes = '';
	paymentReference = '';
	readonly approvalAmounts: Record<string, number> = {};

	ngOnInit(): void {
		const search = this.claimsService.currentSearch();
		this.claimNumber = search.claimNumber ?? '';
		this.statusFilter = search.status ?? null;
		this.claimTypeFilter = search.claimType ?? null;
		this.customerIdFilter = search.customerId ?? '';
		this.fromDateFilter = search.fromDate ?? '';
		this.toDateFilter = search.toDate ?? '';
		this.claimsService.loadClaims();
	}

	search(): void {
		const claimNumber = this.claimNumber.trim();
		if (claimNumber.length > 100) {
			this.validationMessage = 'Claim number must be 100 characters or fewer.';
			return;
		}
		if (
			this.fromDateFilter &&
			this.toDateFilter &&
			this.fromDateFilter > this.toDateFilter
		) {
			this.validationMessage = 'Submission start date must be on or before the end date.';
			return;
		}
		const customerId = this.customerIdFilter.trim();
		if (customerId && !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(customerId)) {
			this.validationMessage = 'Enter a valid customer ID.';
			return;
		}
		this.validationMessage = '';
		this.claimsService.loadClaims({
			...this.claimsService.currentSearch(),
			status: this.statusFilter,
			claimType: this.claimTypeFilter,
			claimNumber: claimNumber || null,
			customerId: customerId || null,
			fromDate: this.fromDateFilter || null,
			toDate: this.toDateFilter || null,
			pageNumber: 1
		});
	}

	clearSearch(): void {
		this.claimNumber = '';
		this.statusFilter = null;
		this.claimTypeFilter = null;
		this.customerIdFilter = '';
		this.fromDateFilter = '';
		this.toDateFilter = '';
		this.validationMessage = '';
		this.claimsService.loadClaims({
			...this.claimsService.currentSearch(),
			status: null,
			claimType: null,
			claimNumber: null,
			customerId: null,
			fromDate: null,
			toDate: null,
			pageNumber: 1
		});
	}

	openClaim(id: string): void {
		this.actionValidationMessage = '';
		this.informationMessage = '';
		this.rejectionReason = '';
		this.rejectionNotes = '';
		this.approvalNotes = '';
		this.paymentReference = '';
		this.claimsService.loadClaim(id);
	}

	closeClaimDetails(): void {
		if (this.detailsLoading() || this.workflowProcessing()) {
			return;
		}

		this.actionValidationMessage = '';
		this.claimsService.clearSelectedClaim();
	}

	startReview(claimId: string, rowVersion: string): void {
		this.actionValidationMessage = '';
		this.claimsService.moveToReview(claimId, rowVersion);
	}

	requestInformation(claimId: string, rowVersion: string): void {
		const message = this.informationMessage.trim();
		if (!message || message.length > 2000) {
			this.actionValidationMessage = 'Enter an information request message of no more than 2,000 characters.';
			return;
		}
		this.actionValidationMessage = '';
		this.claimsService.requestInformation(claimId, { message, rowVersion });
	}

	approve(claimId: string, rowVersion: string, coverages: ClaimsManagerClaimDetail['coverages']): void {
		const allocations = coverages.map(coverage => ({
			coverageId: coverage.coverageId,
			approvedAmount: this.approvalAmount(coverage.id, coverage.requestedAmount)
		}));
		if (
			allocations.some(allocation =>
				!Number.isFinite(allocation.approvedAmount) ||
				allocation.approvedAmount < 0 ||
				allocation.approvedAmount > (coverages.find(coverage => coverage.coverageId === allocation.coverageId)?.requestedAmount ?? 0) ||
				allocation.approvedAmount > (coverages.find(coverage => coverage.coverageId === allocation.coverageId)?.coverageLimitAmount ?? 0)
			) ||
			allocations.reduce((total, allocation) => total + allocation.approvedAmount, 0) <= 0
		) {
			this.actionValidationMessage = 'Each approved amount must be non-negative and within the requested and coverage limits, and the total must be greater than zero.';
			return;
		}
		if (this.approvalNotes.trim().length > 2000) {
			this.actionValidationMessage = 'Approval notes cannot exceed 2,000 characters.';
			return;
		}
		this.actionValidationMessage = '';
		const request: ApproveClaimRequest = {
			coverages: allocations,
			notes: this.approvalNotes.trim() || null,
			rowVersion
		};
		this.claimsService.approveClaim(claimId, request);
	}

	reject(claimId: string, rowVersion: string): void {
		const reason = this.rejectionReason.trim();
		if (reason.length < 20 || reason.length > 2000) {
			this.actionValidationMessage = 'Rejection reason must be between 20 and 2,000 characters.';
			return;
		}
		if (this.rejectionNotes.trim().length > 2000) {
			this.actionValidationMessage = 'Rejection notes cannot exceed 2,000 characters.';
			return;
		}
		this.actionValidationMessage = '';
		this.claimsService.rejectClaim(claimId, {
			reason,
			notes: this.rejectionNotes.trim() || null,
			rowVersion
		});
	}

	markPaid(claimId: string, rowVersion: string): void {
		const paymentReference = this.paymentReference.trim();
		if (!paymentReference || paymentReference.length > 100) {
			this.actionValidationMessage = 'Enter a payment reference of no more than 100 characters.';
			return;
		}
		this.actionValidationMessage = '';
		this.claimsService.markClaimPaid(claimId, { paymentReference, rowVersion });
	}

	closeClaim(claimId: string, rowVersion: string): void {
		this.actionValidationMessage = '';
		this.claimsService.closeClaim(claimId, rowVersion);
	}

	approvalAmount(coverageId: string, requestedAmount: number): number {
		if (this.approvalAmounts[coverageId] === undefined) {
			this.approvalAmounts[coverageId] = requestedAmount;
		}
		return this.approvalAmounts[coverageId];
	}

	setApprovalAmount(coverageId: string, amount: number): void {
		this.approvalAmounts[coverageId] = amount;
	}

	downloadDocument(claimId: string, document: ClaimsManagerClaimDetail['documents'][number]): void {
		this.claimsService.downloadDocument(claimId, document);
	}

	viewDocument(claimId: string, document: ClaimsManagerClaimDetail['documents'][number]): void {
		this.claimsService.viewDocument(claimId, document);
	}

	downloadPolicyDocument(claim: ClaimsManagerClaimDetail, document: ClaimsManagerPolicyDocument): void {
		this.claimsService.downloadPolicyDocument(
			claim.id,
			claim.policy?.policyNumber ?? claim.policyNumber,
			document
		);
	}

	viewPolicyDocument(claim: ClaimsManagerClaimDetail, document: ClaimsManagerPolicyDocument): void {
		this.claimsService.viewPolicyDocument(
			claim.id,
			claim.policy?.policyNumber ?? claim.policyNumber,
			document
		);
	}

	isPolicyDocumentAvailable(document: ClaimsManagerPolicyDocument): boolean {
		return document.status.toLowerCase() === 'available';
	}

	documentTypeName(type: ClaimDocumentType): string {
		switch (type) {
			case ClaimDocumentType.Photo: return 'Photo';
			case ClaimDocumentType.Invoice: return 'Invoice';
			case ClaimDocumentType.PoliceReport: return 'Police report';
			case ClaimDocumentType.AssessmentReport: return 'Assessment report';
			case ClaimDocumentType.Receipt: return 'Receipt';
			case ClaimDocumentType.Other: return 'Other';
			default: return 'Document';
		}
	}

	claimTypeName(type: ClaimType): string {
		return this.claimTypeOptions.find(option => option.value === type)?.label ?? 'Unknown';
	}

	canStartReview(status: ClaimStatus): boolean {
		return status === ClaimStatus.Submitted || status === ClaimStatus.AdditionalInformationRequested;
	}

	canDecide(status: ClaimStatus): boolean {
		return status === ClaimStatus.UnderReview;
	}

	canMarkPaid(status: ClaimStatus): boolean {
		return status === ClaimStatus.Approved;
	}

	canClose(status: ClaimStatus): boolean {
		return status === ClaimStatus.Paid || status === ClaimStatus.Rejected;
	}

	previousPage(): void {
		this.claimsService.goToPreviousPage();
	}

	nextPage(): void {
		this.claimsService.goToNextPage();
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
