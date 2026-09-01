import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './login.scss',
})
export class RegisterPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected displayName = '';
  protected email = '';
  protected password = '';
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  submit(): void {
    this.error.set(null);
    if (this.password.length < 8) {
      this.error.set('Lösenordet måste vara minst 8 tecken.');
      return;
    }

    this.submitting.set(true);
    this.auth.register(this.email, this.password, this.displayName).subscribe({
      next: () => {
        this.submitting.set(false);
        void this.router.navigate(['/']);
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.error.set(typeof err.error?.error === 'string' ? err.error.error : 'Kunde inte skapa kontot.');
      },
    });
  }
}
