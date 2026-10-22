import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { GlobalNotificationComponent } from './shared/components/global-notification.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, GlobalNotificationComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('VacationHomeInsuranceUI');
}
