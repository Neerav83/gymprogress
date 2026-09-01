import { Injectable, isDevMode, signal } from '@angular/core';
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
    if (!('serviceWorker' in navigator)) {
      return;
    }

    if (isDevMode()) {
      void this.unregisterWorkers();
      return;
    }

    window.addEventListener('load', () => {
      navigator.serviceWorker
        .register('/service-worker.js')
        .then((registration) => {
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

  private async unregisterWorkers(): Promise<void> {
    const registrations = await navigator.serviceWorker.getRegistrations();
    await Promise.all(registrations.map((registration) => registration.unregister()));
    if ('caches' in window) {
      const keys = await caches.keys();
      await Promise.all(keys.map((key) => caches.delete(key)));
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
