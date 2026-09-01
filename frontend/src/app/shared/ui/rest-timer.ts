import { Component, inject } from '@angular/core';
import { RestTimerService } from '../../core/services/rest-timer.service';

@Component({
  selector: 'app-rest-timer',
  imports: [],
  template: `
    @if (timer.state().isActive) {
      <div class="rest-timer">
        <div class="timer-content">
          <div class="timer-display">
            <span class="time">{{ formatTime(timer.state().secondsRemaining) }}</span>
            <div class="progress-ring">
              <svg width="120" height="120">
                <circle
                  cx="60"
                  cy="60"
                  r="54"
                  stroke="#e5e7eb"
                  stroke-width="8"
                  fill="none"
                />
                <circle
                  cx="60"
                  cy="60"
                  r="54"
                  stroke="#3b82f6"
                  stroke-width="8"
                  fill="none"
                  [style.stroke-dasharray]="circumference"
                  [style.stroke-dashoffset]="strokeDashoffset()"
                  style="transform: rotate(-90deg); transform-origin: 50% 50%"
                />
              </svg>
            </div>
          </div>
          <button class="stop-btn" (click)="timer.stop()">Stoppa</button>
        </div>
      </div>
    }
  `,
  styles: `
    .rest-timer {
      position: fixed;
      bottom: 5rem;
      right: 1rem;
      z-index: 1000;
      background: white;
      border-radius: 1rem;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
      padding: 1.5rem;
      animation: slideUp 0.3s ease-out;
    }

    @keyframes slideUp {
      from {
        transform: translateY(100%);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }

    .timer-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;
    }

    .timer-display {
      position: relative;
      width: 120px;
      height: 120px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .progress-ring {
      position: absolute;
      top: 0;
      left: 0;
    }

    .time {
      font-size: 2rem;
      font-weight: bold;
      color: #1f2937;
      position: relative;
      z-index: 1;
    }

    .stop-btn {
      padding: 0.5rem 1.5rem;
      background: #ef4444;
      color: white;
      border: none;
      border-radius: 0.5rem;
      font-weight: 500;
      cursor: pointer;
      transition: background 0.2s;
    }

    .stop-btn:hover {
      background: #dc2626;
    }

    @media (max-width: 640px) {
      .rest-timer {
        left: 50%;
        right: auto;
        transform: translateX(-50%);
        bottom: 4.5rem;
      }
    }
  `,
})
export class RestTimerComponent {
  protected readonly timer = inject(RestTimerService);
  protected readonly circumference = 2 * Math.PI * 54;

  protected formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  protected strokeDashoffset(): number {
    const state = this.timer.state();
    const progress = state.secondsRemaining / state.totalSeconds;
    return this.circumference * (1 - progress);
  }
}
