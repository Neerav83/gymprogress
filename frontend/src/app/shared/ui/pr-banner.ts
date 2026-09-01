import { Component, input } from '@angular/core';
import { PersonalRecordHit } from '../../core/models/models';
import { recordLabel } from '../../core/services/format';

@Component({
  selector: 'app-pr-banner',
  template: `
    @if (hits().length) {
      <aside class="pr">
        <p class="kicker">Nytt PR</p>
        @for (hit of hits(); track hit.type) {
          <p>
            <strong>{{ label(hit.type) }}</strong>
            <span>{{ hit.label }}</span>
            @if (hit.previousLabel) {
              <small>Förra: {{ hit.previousLabel }}</small>
            }
          </p>
        }
      </aside>
    }
  `,
  styles: [`
    .pr {
      background: var(--accent);
      color: var(--accent-ink);
      border-radius: 20px;
      padding: 16px 18px;
      display: grid;
      gap: 8px;
    }

    .kicker,
    small {
      margin: 0;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      font-size: 0.72rem;
      font-weight: 700;
    }

    p {
      margin: 0;
      display: grid;
    }

    strong {
      font-family: var(--display);
    }
  `],
})
export class PrBannerComponent {
  readonly hits = input<PersonalRecordHit[]>([]);

  label(type: string): string {
    return recordLabel(type);
  }
}
