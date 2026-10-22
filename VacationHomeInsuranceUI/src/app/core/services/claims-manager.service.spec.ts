import { provideHttpClient } from '@angular/common/http';
import {
	HttpTestingController,
	provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { ClaimsManagerService } from './claims-manager.service';

describe('ClaimsManagerService', () => {
	let service: ClaimsManagerService;
	let httpTestingController: HttpTestingController;

	beforeEach(() => {
		TestBed.configureTestingModule({
			providers: [
				ClaimsManagerService,
				provideHttpClient(),
				provideHttpClientTesting()
			]
		});

		service = TestBed.inject(ClaimsManagerService);
		httpTestingController = TestBed.inject(HttpTestingController);
	});

	afterEach(() => {
		httpTestingController.verify();
	});

	it('loads claim details from the configured gateway URL exactly once', () => {
		const claimId = '3e160d18-8a0c-4d7c-b674-dc54de0a6d6d';

		service.loadClaim(claimId);

		const request = httpTestingController.expectOne(
			`${environment.apiGatewayBaseUrl}/admin/claims/${claimId}`
		);

		expect(request.request.method).toBe('GET');
		request.flush({
			id: claimId,
			claimNumber: 'CLM-2026-001004',
			customerId: 'customer-id',
			policyId: 'policy-id',
			policyNumber: 'POL-2026-00004',
			propertyId: 'property-id',
			type: 1,
			status: 1,
			incidentDate: '2026-10-22T00:00:00Z',
			title: 'Damage',
			description: 'Property damage',
			claimedAmount: 1000,
			approvedAmount: null,
			rejectionReason: null,
			submittedAtUtc: '2026-10-22T00:00:00Z',
			coverages: [],
			documents: [],
			timeline: [],
			rowVersion: 'AQID',
			assignedClaimsManagerId: 'manager-id',
			assignedClaimsManagerName: 'Claims Manager',
			assignedClaimsManagerEmail: 'manager@example.com',
			assignedAtUtc: '2026-10-22T00:00:00Z',
			assignmentHistory: []
		});

		expect(service.selectedClaim()?.id).toBe(claimId);
	});

	it('posts workflow actions to the configured gateway URL', () => {
		const claimId = '3e160d18-8a0c-4d7c-b674-dc54de0a6d6d';

		service.moveToReview(claimId, 'AQID');

		const request = httpTestingController.expectOne(
			`${environment.apiGatewayBaseUrl}/admin/claims/${claimId}/review`
		);

		expect(request.request.method).toBe('POST');
		expect(request.request.body).toEqual({ rowVersion: 'AQID' });
		request.flush(null);

		const claimRequest = httpTestingController.expectOne(
			`${environment.apiGatewayBaseUrl}/admin/claims/${claimId}`
		);
		claimRequest.flush({
			id: claimId,
			claimNumber: 'CLM-2026-001004',
			customerId: 'customer-id',
			policyId: 'policy-id',
			policyNumber: 'POL-2026-00004',
			propertyId: 'property-id',
			type: 1,
			status: 2,
			incidentDate: '2026-10-22T00:00:00Z',
			title: 'Damage',
			description: 'Property damage',
			claimedAmount: 1000,
			approvedAmount: null,
			rejectionReason: null,
			submittedAtUtc: '2026-10-22T00:00:00Z',
			coverages: [],
			documents: [],
			timeline: [],
			rowVersion: 'BAUG',
			assignedClaimsManagerId: 'manager-id',
			assignedClaimsManagerName: 'Claims Manager',
			assignedClaimsManagerEmail: 'manager@example.com',
			assignedAtUtc: '2026-10-22T00:00:00Z',
			assignmentHistory: []
		});

		const dashboardRequest = httpTestingController.expectOne(
			`${environment.apiGatewayBaseUrl}/claims-manager/claims/dashboard`
		);
		dashboardRequest.flush({
			totalClaims: 1,
			submittedClaims: 0,
			underReviewClaims: 1,
			informationRequestedClaims: 0,
			approvedClaims: 0,
			rejectedClaims: 0,
			paidClaims: 0,
			closedClaims: 0,
			totalClaimedAmount: 1000,
			totalApprovedAmount: 0,
			recentClaims: []
		});

		const claimsRequest = httpTestingController.expectOne(
			requestTo =>
				requestTo.url === `${environment.apiGatewayBaseUrl}/admin/claims` &&
				requestTo.params.get('pageNumber') === '1' &&
				requestTo.params.get('pageSize') === '20'
		);
		claimsRequest.flush({
			items: [],
			pageNumber: 1,
			pageSize: 20,
			totalCount: 0,
			totalPages: 0
		});
	});

	it('sends supported status, claim type, customer, and inclusive date filters', () => {
		service.loadClaims({
			status: 2,
			claimType: 8,
			claimNumber: 'CLM-2026',
			customerId: '0c0f9a18-142e-4a93-9e45-399e0d22e08c',
			fromDate: '2026-10-01',
			toDate: '2026-10-22',
			pageNumber: 2,
			pageSize: 20
		});

		const request = httpTestingController.expectOne(
			requestTo => requestTo.url === `${environment.apiGatewayBaseUrl}/admin/claims`
		);

		expect(request.request.method).toBe('GET');
		expect(request.request.params.get('status')).toBe('2');
		expect(request.request.params.get('claimType')).toBe('8');
		expect(request.request.params.get('claimNumber')).toBe('CLM-2026');
		expect(request.request.params.get('customerId')).toBe('0c0f9a18-142e-4a93-9e45-399e0d22e08c');
		expect(request.request.params.get('fromDate')).toBe('2026-10-01');
		expect(request.request.params.get('toDate')).toBe('2026-10-22T23:59:59.999');
		expect(request.request.params.get('pageNumber')).toBe('2');
		expect(request.request.params.get('pageSize')).toBe('20');
		request.flush({
			items: [],
			pageNumber: 2,
			pageSize: 20,
			totalCount: 0,
			totalPages: 0
		});
	});

	it('uses the existing endpoint and payload for every supported workflow action', () => {
		const claimId = '3e160d18-8a0c-4d7c-b674-dc54de0a6d6d';
		const rowVersion = 'AQID';
		const baseClaim = {
			id: claimId,
			claimNumber: 'CLM-2026-001004',
			customerId: 'customer-id',
			policyId: 'policy-id',
			policyNumber: 'POL-2026-00004',
			propertyId: 'property-id',
			type: 1,
			status: 2,
			incidentDate: '2026-10-22T00:00:00Z',
			title: 'Damage',
			description: 'Property damage',
			claimedAmount: 1000,
			approvedAmount: null,
			rejectionReason: null,
			submittedAtUtc: '2026-10-22T00:00:00Z',
			coverages: [],
			documents: [],
			timeline: [],
			rowVersion,
			assignedClaimsManagerId: 'manager-id',
			assignedClaimsManagerName: 'Claims Manager',
			assignedClaimsManagerEmail: 'manager@example.com',
			assignedAtUtc: '2026-10-22T00:00:00Z',
			assignmentHistory: []
		};

		const actions: Array<{
			path: string;
			body: object;
			run: () => void;
		}> = [
			{
				path: 'review',
				body: { rowVersion },
				run: () => service.moveToReview(claimId, rowVersion)
			},
			{
				path: 'request-information',
				body: { message: 'Please provide repair invoices.', rowVersion },
				run: () => service.requestInformation(claimId, { message: 'Please provide repair invoices.', rowVersion })
			},
			{
				path: 'approve',
				body: { coverages: [{ coverageId: 'coverage-id', approvedAmount: 500 }], notes: 'Approved.', rowVersion },
				run: () => service.approveClaim(claimId, {
					coverages: [{ coverageId: 'coverage-id', approvedAmount: 500 }],
					notes: 'Approved.',
					rowVersion
				})
			},
			{
				path: 'reject',
				body: { reason: 'Insufficient supporting evidence supplied.', notes: null, rowVersion },
				run: () => service.rejectClaim(claimId, {
					reason: 'Insufficient supporting evidence supplied.',
					notes: null,
					rowVersion
				})
			},
			{
				path: 'pay',
				body: { paymentReference: 'PAY-001', rowVersion },
				run: () => service.markClaimPaid(claimId, { paymentReference: 'PAY-001', rowVersion })
			},
			{
				path: 'close',
				body: { rowVersion },
				run: () => service.closeClaim(claimId, rowVersion)
			}
		];

		for (const action of actions) {
			action.run();
			const request = httpTestingController.expectOne(
				`${environment.apiGatewayBaseUrl}/admin/claims/${claimId}/${action.path}`
			);
			expect(request.request.method).toBe('POST');
			expect(request.request.body).toEqual(action.body);
			request.flush(null);

			httpTestingController.expectOne(
				`${environment.apiGatewayBaseUrl}/admin/claims/${claimId}`
			).flush(baseClaim);
			httpTestingController.expectOne(
				`${environment.apiGatewayBaseUrl}/claims-manager/claims/dashboard`
			).flush({
				totalClaims: 1,
				submittedClaims: 0,
				underReviewClaims: 1,
				informationRequestedClaims: 0,
				approvedClaims: 0,
				rejectedClaims: 0,
				paidClaims: 0,
				closedClaims: 0,
				totalClaimedAmount: 1000,
				totalApprovedAmount: 0,
				recentClaims: []
			});
			httpTestingController.expectOne(
				requestTo => requestTo.url === `${environment.apiGatewayBaseUrl}/admin/claims`
			).flush({
				items: [],
				pageNumber: 1,
				pageSize: 20,
				totalCount: 0,
				totalPages: 0
			});
		}
	});
});
