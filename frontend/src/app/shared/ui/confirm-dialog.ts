import { Component, HostListener, inject } from '@angular/core';
import { ConfirmService } from '../../core/services/confirm.service';

@Component({
  selector: 'app-confirm-dialog',
  template: `
    @if (confirm.request(); as dialog) {
      <div class="layer">
        <div class="scrim" (click)="confirm.close(false)"></div>
        <div class="sheet" role="dialog" aria-modal="true" aria-labelledby="confirm-title">
          <h2 id="confirm-title" class="title">{{ dialog.title }}</h2>
          <p class="message">{{ dialog.message }}</p>
          <div class="actions">
            <button class="btn btn-ghost" type="button" (click)="confirm.close(false)">
              {{ dialog.cancelLabel }}
            </button>
            <button
              class="btn"
              [class.btn-danger]="dialog.danger"
              [class.btn-primary]="!dialog.danger"
              type="button"
              (click)="confirm.close(true)"
            >
              {{ dialog.confirmLabel }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: `
    .layer {
      position: fixed;
      inset: 0;
      z-index: 10000;
      display: grid;
      place-items: end center;
      padding: 18px calc(18px + env(safe-area-inset-right)) calc(18px + env(safe-area-inset-bottom))
        calc(18px + env(safe-area-inset-left));
    }

    @media (min-width: 520px) {
      .layer {
        place-items: center;
      }
    }

    .scrim {
      position: absolute;
      inset: 0;
      background: rgba(27, 25, 20, 0.42);
    }

    .sheet {
      position: relative;
      width: min(420px, 100%);
      padding: 24px 22px 18px;
      border-radius: 28px 28px 22px 22px;
      background: var(--bg-card);
      border: 1px solid var(--line);
      box-shadow: var(--shadow);
    }

    .title {
      margin: 0;
      font-family: var(--display);
      font-size: 1.55rem;
      letter-spacing: -0.03em;
      line-height: 1.15;
    }

    .message {
      margin: 10px 0 22px;
      color: var(--muted);
    }

    .actions {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 10px;
    }

    .actions .btn {
      width: 100%;
    }
  `,
})
export class ConfirmDialogComponent {
  protected readonly confirm = inject(ConfirmService);

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.confirm.request()) {
      this.confirm.close(false);
    }
  }
}
