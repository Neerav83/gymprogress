import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-stepper',
  template: `
    <div class="stepper">
      <span class="label">{{ label() }}</span>
      <div class="row">
        <button type="button" class="nudge" (click)="change(-step())" aria-label="Minska">−</button>
        <div class="value mono">
          {{ displayValue() }}
          <small>{{ unit() }}</small>
        </div>
        <button type="button" class="nudge" (click)="change(step())" aria-label="Öka">+</button>
      </div>
    </div>
  `,
  styles: [`
    .stepper {
      display: grid;
      gap: 8px;
    }

    .label {
      color: var(--muted);
      font-size: 0.82rem;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .row {
      display: grid;
      grid-template-columns: 64px 1fr 64px;
      gap: 10px;
      align-items: center;
    }

    .nudge {
      height: 64px;
      border-radius: 20px;
      background: var(--bg-elevated);
      border: 1px solid var(--line);
      font-size: 1.8rem;
      font-weight: 700;
    }

    .value {
      min-height: 64px;
      display: flex;
      align-items: baseline;
      justify-content: center;
      gap: 8px;
      font-size: 2.4rem;
      font-weight: 600;
    }

    small {
      font-size: 0.9rem;
      color: var(--muted);
    }
  `],
})
export class StepperComponent {
  readonly label = input.required<string>();
  readonly value = input.required<number>();
  readonly step = input(1);
  readonly min = input(0);
  readonly max = input(1000);
  readonly unit = input('');
  readonly decimals = input(false);
  readonly valueChange = output<number>();

  displayValue(): string {
    const value = this.value();
    return this.decimals() ? value.toFixed(1).replace('.', ',') : String(value);
  }

  change(delta: number): void {
    const next = Math.min(this.max(), Math.max(this.min(), round(this.value() + delta, this.decimals() ? 1 : 0)));
    this.valueChange.emit(next);
  }
}

function round(value: number, digits: number): number {
  const factor = 10 ** digits;
  return Math.round(value * factor) / factor;
}
