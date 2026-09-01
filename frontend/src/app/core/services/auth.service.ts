import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

export interface AuthUser {
  id: string;
  email: string;
  displayName: string;
}

interface AuthResponse {
  token: string;
  user: AuthUser;
}

const API = '/api/v1/auth';
const tokenKey = 'gymprogress.token';
const userKey = 'gymprogress.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly token = signal<string | null>(localStorage.getItem(tokenKey));
  readonly user = signal<AuthUser | null>(readStoredUser());
  readonly isLoggedIn = computed(() => !!this.token());

  register(email: string, password: string, displayName: string) {
    return this.http
      .post<AuthResponse>(`${API}/register`, { email, password, displayName })
      .pipe(tap((response) => this.store(response)));
  }

  login(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${API}/login`, { email, password })
      .pipe(tap((response) => this.store(response)));
  }

  logout(): void {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(userKey);
    this.token.set(null);
    this.user.set(null);
    void this.router.navigate(['/login']);
  }

  private store(response: AuthResponse): void {
    localStorage.setItem(tokenKey, response.token);
    localStorage.setItem(userKey, JSON.stringify(response.user));
    this.token.set(response.token);
    this.user.set(response.user);
  }
}

function readStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(userKey);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
}
