import { Injectable, signal } from '@angular/core';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
}

interface ConfirmRequest extends Required<Omit<ConfirmOptions, 'danger'>> {
  danger: boolean;
  resolve: (value: boolean) => void;
}

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private readonly current = signal<ConfirmRequest | null>(null);
  readonly request = this.current.asReadonly();

  confirm(options: ConfirmOptions): Promise<boolean> {
    this.current()?.resolve(false);

    return new Promise((resolve) => {
      this.current.set({
        title: options.title,
        message: options.message,
        confirmLabel: options.confirmLabel ?? 'Ta bort',
        cancelLabel: options.cancelLabel ?? 'Avbryt',
        danger: options.danger ?? true,
        resolve,
      });
    });
  }

  close(result: boolean): void {
    const request = this.current();
    this.current.set(null);
    request?.resolve(result);
  }
}
