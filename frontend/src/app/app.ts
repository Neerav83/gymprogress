import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { PwaService } from './core/services/pwa.service';
import { ToastContainerComponent } from './shared/ui/toast-container';
import { RestTimerComponent } from './shared/ui/rest-timer';
import { ConfirmDialogComponent } from './shared/ui/confirm-dialog';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ToastContainerComponent,
    RestTimerComponent,
    ConfirmDialogComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly auth = inject(AuthService);
  private readonly pwa = inject(PwaService);
}
