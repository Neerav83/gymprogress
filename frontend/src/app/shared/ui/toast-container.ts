import { Component, inject } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [],
  template: `
    <div class="toast-container">
      @for (toast of toastService.activeToasts(); track toast.id) {
        <div class="toast toast-{{ toast.type }}" (click)="toastService.dismiss(toast.id)">
          <div class="toast-icon">
            @switch (toast.type) {
              @case ('success') {
                <span>✓</span>
              }
              @case ('error') {
                <span>✕</span>
              }
              @case ('warning') {
                <span>⚠</span>
              }
              @default {
                <span>ℹ</span>
              }
            }
          </div>
          <div class="toast-message">{{ toast.message }}</div>
        </div>
      }
    </div>
  `,
  styles: `
    .toast-container {
      position: fixed;
      top: 1rem;
      right: 1rem;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      max-width: 420px;
    }

    .toast {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 1rem 1.25rem;
      border-radius: 0.5rem;
      background: white;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      cursor: pointer;
      animation: slideIn 0.3s ease-out;
      transition: transform 0.2s, opacity 0.2s;
    }

    .toast:hover {
      transform: translateX(-4px);
      opacity: 0.9;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .toast-icon {
      flex-shrink: 0;
      width: 1.5rem;
      height: 1.5rem;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      font-size: 1rem;
    }

    .toast-message {
      flex: 1;
      font-size: 0.9rem;
      line-height: 1.4;
      color: #333;
    }

    .toast-success .toast-icon {
      background: #10b981;
      color: white;
    }

    .toast-error .toast-icon {
      background: #ef4444;
      color: white;
    }

    .toast-warning .toast-icon {
      background: #f59e0b;
      color: white;
    }

    .toast-info .toast-icon {
      background: #3b82f6;
      color: white;
    }

    @media (max-width: 640px) {
      .toast-container {
        left: 1rem;
        right: 1rem;
        max-width: none;
      }
    }
  `,
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);
}
