import {
  Component,
  inject
} from '@angular/core';


import {
  Router,
  RouterLink
} from '@angular/router';


import {
  AuthService
} from '../../../core/services/auth.service';


@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [
    RouterLink
  ],
  templateUrl:
    './access-denied.component.html',
  styleUrl:
    './access-denied.component.css'
})
export class AccessDeniedComponent {
  private readonly authService =
    inject(AuthService);


  private readonly router =
    inject(Router);


  returnToDashboard(): void {
    void this.router.navigateByUrl(
      this.authService
        .getDefaultDashboardUrl()
    );
  }
}

