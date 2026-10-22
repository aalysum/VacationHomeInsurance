import { Injectable, signal } from '@angular/core';

export interface GlobalNotification {
  id: number;
  message: string;
  type: 'error' | 'success';
}

@Injectable({ providedIn: 'root' })
export class GlobalNotificationService {
  private readonly notificationState =
    signal<GlobalNotification | null>(null);
  private nextId = 0;
  private dismissTimer: ReturnType<typeof setTimeout> | null = null;

  readonly notification = this.notificationState.asReadonly();

  showError(message: string): void {
    this.show(message, 'error');
  }

  showSuccess(message: string): void {
    this.show(message, 'success');
  }

  private show(
    message: string,
    type: GlobalNotification['type']
  ): void {
    const normalizedMessage = message.trim();
    if (!normalizedMessage) {
      return;
    }

    if (this.dismissTimer) {
      clearTimeout(this.dismissTimer);
    }

    const id = ++this.nextId;
    this.notificationState.set({
      id,
      message: normalizedMessage,
      type
    });
    this.dismissTimer = setTimeout(() => {
      if (this.notificationState()?.id === id) {
        this.dismiss();
      }
    }, 8000);
  }

  dismiss(): void {
    if (this.dismissTimer) {
      clearTimeout(this.dismissTimer);
      this.dismissTimer = null;
    }
    this.notificationState.set(null);
  }
}
