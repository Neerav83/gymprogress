import { Injectable, signal } from '@angular/core';

export interface RestTimer {
  isActive: boolean;
  secondsRemaining: number;
  totalSeconds: number;
}

@Injectable({
  providedIn: 'root',
})
export class RestTimerService {
  private readonly timer = signal<RestTimer>({
    isActive: false,
    secondsRemaining: 0,
    totalSeconds: 0,
  });

  private intervalId: any = null;

  readonly state = this.timer.asReadonly();

  start(seconds: number): void {
    this.stop();

    this.timer.set({
      isActive: true,
      secondsRemaining: seconds,
      totalSeconds: seconds,
    });

    this.intervalId = setInterval(() => {
      const current = this.timer();
      const remaining = current.secondsRemaining - 1;

      if (remaining <= 0) {
        this.complete();
      } else {
        this.timer.update((state) => ({
          ...state,
          secondsRemaining: remaining,
        }));
      }
    }, 1000);

    this.notifyUser('Timer startad', `Vila i ${seconds} sekunder`);
  }

  stop(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }

    this.timer.set({
      isActive: false,
      secondsRemaining: 0,
      totalSeconds: 0,
    });
  }

  private complete(): void {
    this.stop();
    this.notifyUser('Vila klar!', 'Dags för nästa set');
    this.playSound();
  }

  private notifyUser(title: string, body: string): void {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification(title, { body, icon: '/favicon.ico' });
    }
  }

  private playSound(): void {
    const context = new AudioContext();
    const oscillator = context.createOscillator();
    const gainNode = context.createGain();

    oscillator.connect(gainNode);
    gainNode.connect(context.destination);

    oscillator.frequency.value = 800;
    oscillator.type = 'sine';

    gainNode.gain.setValueAtTime(0.3, context.currentTime);
    gainNode.gain.exponentialRampToValueAtTime(0.01, context.currentTime + 0.5);

    oscillator.start(context.currentTime);
    oscillator.stop(context.currentTime + 0.5);
  }

  requestNotificationPermission(): void {
    if ('Notification' in window && Notification.permission === 'default') {
      Notification.requestPermission();
    }
  }
}
