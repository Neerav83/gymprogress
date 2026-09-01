import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: string;
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
  duration?: number;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private readonly toasts = signal<Toast[]>([]);
  readonly activeToasts = this.toasts.asReadonly();

  show(message: string, type: Toast['type'] = 'info', duration: number = 4000): void {
    const id = crypto.randomUUID();
    const toast: Toast = { id, message, type, duration };

    this.toasts.update((current) => [...current, toast]);

    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }
  }

  success(message: string, duration?: number): void {
    this.show(message, 'success', duration);
  }

  error(message: string, duration?: number): void {
    this.show(message, 'error', duration);
  }

  info(message: string, duration?: number): void {
    this.show(message, 'info', duration);
  }

  warning(message: string, duration?: number): void {
    this.show(message, 'warning', duration);
  }

  dismiss(id: string): void {
    this.toasts.update((current) => current.filter((toast) => toast.id !== id));
  }

  clear(): void {
    this.toasts.set([]);
  }
}
