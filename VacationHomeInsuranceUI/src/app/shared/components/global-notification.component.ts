import { Component, inject } from '@angular/core';
import { GlobalNotificationService } from '../../core/services/global-notification.service';

@Component({
  selector: 'app-global-notification',
  standalone: true,
  template: `
    @if (notifications.notification(); as notification) {
      <div class="notification-region">
        <div
          class="notification-toast"
          [class.notification-toast-error]="notification.type === 'error'"
          [class.notification-toast-success]="notification.type === 'success'"
          [attr.role]="notification.type === 'error' ? 'alert' : 'status'"
          aria-live="assertive"
          aria-atomic="true"
        >
          <span>{{ notification.message }}</span>
          <button
            type="button"
            aria-label="Dismiss notification"
            (click)="dismiss()"
          >
            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
              <path d="M6 6L18 18M18 6L6 18" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
            </svg>
          </button>
        </div>
      </div>
    }
  `,
  styles: `
    .notification-region {
      position: fixed;
      z-index: 2000;
      top: max(1rem, env(safe-area-inset-top));
      right: max(1rem, env(safe-area-inset-right));
      width: min(28rem, calc(100vw - 2rem));
      pointer-events: none;
    }

    .notification-toast {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
      padding: 1rem 1.125rem;
      border: 1px solid;
      border-radius: 0.75rem;
      box-shadow: 0 16px 44px rgb(7 26 48 / 24%);
      font-size: 0.875rem;
      line-height: 1.45;
      pointer-events: auto;
    }

    .notification-toast-error {
      border-color: #f0b9b6;
      background: #fff8f7;
      color: #84251d;
    }

    .notification-toast-success {
      border-color: #9ad5b5;
      background: #f2fbf5;
      color: #17633d;
    }

    .notification-toast span {
      min-width: 0;
      overflow-wrap: anywhere;
    }

    .notification-toast button {
      display: grid;
      width: 2rem;
      height: 2rem;
      flex: 0 0 2rem;
      place-items: center;
      margin: -0.25rem -0.375rem 0 0;
      border: 0;
      border-radius: 0.375rem;
      background: transparent;
      color: inherit;
      cursor: pointer;
    }

    .notification-toast button:hover {
      background: rgb(132 37 29 / 8%);
    }

    .notification-toast svg {
      width: 1rem;
      height: 1rem;
    }
  `
})
export class GlobalNotificationComponent {
  readonly notifications = inject(GlobalNotificationService);

  dismiss(): void {
    this.notifications.dismiss();
  }
}
