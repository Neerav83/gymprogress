import { Injectable, signal } from '@angular/core';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root',
})
export class PwaService {
  private readonly isOnline = signal(navigator.onLine);
  readonly online = this.isOnline.asReadonly();

  constructor(private readonly toast: ToastService) {
    this.initializeServiceWorker();
    this.setupOnlineOfflineListeners();
  }

  private initializeServiceWorker(): void {
    if ('serviceWorker' in navigator) {
      window.addEventListener('load', () => {
        navigator.serviceWorker
          .register('/service-worker.js')
          .then((registration) => {
            console.log('Service Worker registrerad:', registration.scope);

            registration.addEventListener('updatefound', () => {
              const newWorker = registration.installing;
              if (newWorker) {
                newWorker.addEventListener('statechange', () => {
                  if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                    this.toast.info('En ny version finns tillgänglig. Ladda om sidan.', 0);
                  }
                });
              }
            });
          })
          .catch((error) => {
            console.error('Service Worker registrering misslyckades:', error);
          });
      });
    }
  }

  private setupOnlineOfflineListeners(): void {
    window.addEventListener('online', () => {
      this.isOnline.set(true);
      this.toast.success('Du är online igen!');
    });

    window.addEventListener('offline', () => {
      this.isOnline.set(false);
      this.toast.warning('Du är offline. Ändringar sparas när du är online igen.');
    });
  }

  checkForUpdates(): void {
    if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
      navigator.serviceWorker.controller.postMessage({ type: 'CHECK_FOR_UPDATES' });
    }
  }
}
