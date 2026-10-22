import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
	selector: 'app-claims-manager-profile',
	standalone: true,
	imports: [RouterLink],
	templateUrl: './claims-manager-profile.component.html',
	styleUrl: './claims-manager-profile.component.css'
})
export class ClaimsManagerProfileComponent {
	private readonly authService = inject(AuthService);

	readonly user = this.authService.currentUser;
}
