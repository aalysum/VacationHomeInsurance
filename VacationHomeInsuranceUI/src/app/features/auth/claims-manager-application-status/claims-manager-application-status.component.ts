import {
  Component,
  OnInit,
  inject
} from '@angular/core';


import {
  DatePipe
} from '@angular/common';


import {
  RouterLink
} from '@angular/router';


import {
  AuthService
} from '../../../core/services/auth.service';


@Component({
  selector:
    'app-claims-manager-application-status',


  standalone: true,


  imports: [
    DatePipe,
    RouterLink
  ],


  templateUrl:
    './claims-manager-application-status.component.html',


  styleUrl:
    './claims-manager-application-status.component.css'
})
export class ClaimsManagerApplicationStatusComponent
  implements OnInit {
  private readonly authService =
    inject(AuthService);


  readonly loading =
    this.authService
      .claimsManagerStatusLoading;


  readonly application =
    this.authService
      .claimsManagerApplicationStatus;


  readonly errorMessage =
    this.authService
      .claimsManagerStatusErrorMessage;


  ngOnInit(): void {
    this.authService
      .clearClaimsManagerStatusState();


    this.authService
      .loadClaimsManagerApplicationStatus();
  }


  isPending(): boolean {
    return (
      this.application()
        ?.status
        .toLowerCase() ===
      'pendingapproval'
    );
  }


  isRejected(): boolean {
    return (
      this.application()
        ?.status
        .toLowerCase() ===
      'rejected'
    );
  }


  retry(): void {
    this.authService
      .retryClaimsManagerStatus();
  }


  reapply(): void {
    this.authService
      .startClaimsManagerReapplication();
  }


  returnToLogin(): void {
    this.authService
      .leaveClaimsManagerApplicationFlow();
  }
}

