import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected email = '';
  protected password = '';
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  submit(): void {
    this.error.set(null);
    this.submitting.set(true);
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.submitting.set(false);
        void this.router.navigate(['/']);
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.error.set(typeof err.error?.error === 'string' ? err.error.error : 'Kunde inte logga in.');
      },
    });
  }
}
