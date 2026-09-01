import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token();
  const request = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/api/v1/auth/')) {
        if (!isRefreshing && auth.refreshToken()) {
          isRefreshing = true;
          return auth.refresh().pipe(
            switchMap(() => {
              isRefreshing = false;
              const newToken = auth.token();
              const retryRequest = newToken
                ? req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } })
                : req;
              return next(retryRequest);
            }),
            catchError((refreshError) => {
              isRefreshing = false;
              auth.logout();
              return throwError(() => refreshError);
            }),
          );
        }
        auth.logout();
      }
      return throwError(() => error);
    }),
  );
};
