import { Routes } from '@angular/router';
import { claimsManagerGuard } from '../../core/guards/claims-manager.guard';
import { ClaimsManagerLayoutComponent } from './claims-manager-layout.component';

export const CLAIMS_MANAGER_ROUTES: Routes = [
	{
		path: '',
		canActivate: [claimsManagerGuard],
		component: ClaimsManagerLayoutComponent,
		children: [
			{
				path: '',
				pathMatch: 'full',
				redirectTo: 'dashboard'
			},
			{
				path: 'dashboard',
				canActivate: [claimsManagerGuard],
				loadComponent: () =>
					import('./dashboard/claims-manager-dashboard.component').then(
						module => module.ClaimsManagerDashboardComponent
					),
				title: 'Claims Manager Dashboard | SecureSphere'
			},
			{
				path: 'claims',
				canActivate: [claimsManagerGuard],
				loadComponent: () =>
					import('./claims/claims-manager-claims.component').then(
						module => module.ClaimsManagerClaimsComponent
					),
				title: 'Assigned Claims | SecureSphere'
			},
			{
				path: 'profile',
				canActivate: [claimsManagerGuard],
				loadComponent: () =>
					import('./profile/claims-manager-profile.component').then(
						module => module.ClaimsManagerProfileComponent
					),
				title: 'Claims Manager Profile | SecureSphere'
			}
		]
	}
];
