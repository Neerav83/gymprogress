import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface AuthUser {
  id: string;
  email: string;
  displayName: string;
}

interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: AuthUser;
}

const API = '/api/v1/auth';
const tokenKey = 'gymprogress.token';
const refreshTokenKey = 'gymprogress.refreshToken';
const userKey = 'gymprogress.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly token = signal<string | null>(localStorage.getItem(tokenKey));
  readonly refreshToken = signal<string | null>(localStorage.getItem(refreshTokenKey));
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

  refresh(): Observable<AuthResponse> {
    const currentRefreshToken = this.refreshToken();
    if (!currentRefreshToken) {
      throw new Error('No refresh token available');
    }

    return this.http
      .post<AuthResponse>(`${API}/refresh`, { refreshToken: currentRefreshToken })
      .pipe(tap((response) => this.store(response)));
  }

  logout(): void {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(refreshTokenKey);
    localStorage.removeItem(userKey);
    this.token.set(null);
    this.refreshToken.set(null);
    this.user.set(null);
    void this.router.navigate(['/login']);
  }

  private store(response: AuthResponse): void {
    localStorage.setItem(tokenKey, response.accessToken);
    localStorage.setItem(refreshTokenKey, response.refreshToken);
    localStorage.setItem(userKey, JSON.stringify(response.user));
    this.token.set(response.accessToken);
    this.refreshToken.set(response.refreshToken);
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
