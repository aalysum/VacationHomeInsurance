import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface ClaimsManagerNavigationItem {
	label: string;
	description: string;
	route: string;
	icon: 'overview' | 'claims' | 'profile' | 'security';
}

@Component({
	selector: 'app-claims-manager-layout',
	standalone: true,
	imports: [CommonModule, RouterLink, RouterOutlet],
	templateUrl: './claims-manager-layout.component.html',
	styleUrl: './claims-manager-layout.component.css'
})
export class ClaimsManagerLayoutComponent {
	private readonly authService = inject(AuthService);
	private readonly router = inject(Router);

	readonly user = this.authService.currentUser;
	readonly displayName = computed(() => {
		const email = this.user()?.email?.trim();
		return email?.split('@')[0] || 'Claims Manager';
	});
	readonly initials = computed(() => {
		const name = this.displayName();
		return name
			.split(/[.\s_-]+/)
			.filter(Boolean)
			.slice(0, 2)
			.map(part => part[0].toUpperCase())
			.join('');
	});

	readonly navigationItems: readonly ClaimsManagerNavigationItem[] = [
		{
			label: 'Overview',
			description: 'Assigned claim activity',
			route: '/claims-manager/dashboard',
			icon: 'overview'
		},
		{
			label: 'Assigned Claims',
			description: 'Review and process claims',
			route: '/claims-manager/claims',
			icon: 'claims'
		},
		{
			label: 'Profile',
			description: 'Claims Manager profile',
			route: '/claims-manager/profile',
			icon: 'profile'
		},
		{
			label: 'MFA settings',
			description: 'Password and multi-factor security',
			route: '/account/security/mfa',
			icon: 'security'
		}
	];

	logout(): void {
		this.authService.logout();
	}

	isCurrentNavigationItem(item: ClaimsManagerNavigationItem): boolean {
		const currentUrl = this.router.url.split('?')[0];
		return item.route === '/claims-manager/dashboard'
			? currentUrl === item.route
			: currentUrl === item.route || currentUrl.startsWith(`${item.route}/`);
	}
}
