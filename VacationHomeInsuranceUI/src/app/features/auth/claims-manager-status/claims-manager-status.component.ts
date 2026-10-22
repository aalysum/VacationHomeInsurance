import {
  Component,
  inject
} from '@angular/core';


import {
  Router,
  RouterLink
} from '@angular/router';


import {
  TokenStorageService
} from '../../../core/services/token-storage.service';


@Component({
  selector: 'app-claims-manager-status',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main class="flex min-h-screen items-center justify-center bg-[#EEF4ED] px-5">
      <section class="w-full max-w-xl rounded-[28px] border border-white bg-white/80 p-8 text-center shadow-[0_30px_80px_rgba(19,49,92,0.1)]">
        <p class="text-xs font-semibold uppercase tracking-[0.18em] text-[#7890A3]">
          Claims Manager application
        </p>


        <h1 class="mt-5 text-4xl font-medium tracking-[-0.05em] text-[#0B2545]">
          Application status
        </h1>


        @if (flow?.status === 'PendingApproval') {
          <span class="mt-6 inline-flex rounded-full bg-amber-50 px-4 py-2 text-xs font-semibold text-amber-700">
            Pending Approval
          </span>


          <p class="mt-5 text-sm leading-7 text-[#6B7F92]">
            Your application is waiting for administrator review.
          </p>
        } @else if (flow?.status === 'Rejected') {
          <span class="mt-6 inline-flex rounded-full bg-red-50 px-4 py-2 text-xs font-semibold text-red-700">
            Rejected
          </span>


          <p class="mt-5 text-sm leading-7 text-[#6B7F92]">
            {{ flow?.rejectionReason ?? 'The application was rejected.' }}
          </p>
        }


        <a
          routerLink="/auth/login"
          class="mt-8 inline-flex rounded-full bg-[#0B2545] px-6 py-3 text-sm font-medium text-white"
        >
          Return to login
        </a>
      </section>
    </main>
  `
})
export class ClaimsManagerStatusComponent {
  private readonly storage =
    inject(TokenStorageService);


  private readonly router =
    inject(Router);


  readonly flow =
    this.storage.getClaimsManagerFlow();


  constructor() {
    if (!this.flow) {
      this.router.navigateByUrl(
        '/auth/login'
      );
    }
  }
}

