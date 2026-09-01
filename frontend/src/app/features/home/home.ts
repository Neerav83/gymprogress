import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, NgZone, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Dashboard, Workout, WorkoutRecommendation } from '../../core/models/models';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { formatDay, formatKg, progressionLabel, recordLabel } from '../../core/services/format';

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class HomePage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly zone = inject(NgZone);

  protected readonly data = signal<Dashboard | null>(null);
  protected readonly starting = signal(false);
  protected readonly asking = signal(false);
  protected readonly recommendation = signal<WorkoutRecommendation | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly coachError = signal<string | null>(null);

  ngOnInit(): void {
    this.api.dashboard().subscribe({
      next: (dashboard) => {
        if (this.starting() || this.router.url.startsWith('/workout')) {
          return;
        }
        this.data.set(dashboard);
      },
      error: () => {
        this.error.set('Kunde inte nå API:t. Är backend och Postgres igång?');
        this.toast.error('Kunde inte nå API:t. Är backend och Postgres igång?');
      },
    });
  }

  protected day = formatDay;
  protected kg = formatKg;
  protected recordName = recordLabel;
  protected progression = progressionLabel;

  greeting(): string {
    const name = this.auth.user()?.displayName;
    return name
      ? `Hej ${name}. Logga ett set på ett par sekunder.`
      : 'Logga ett set på ett par sekunder. Historiken sköter resten.';
  }

  logout(): void {
    this.auth.logout();
  }

  start(): void {
    const activeId = this.data()?.activeWorkout?.id;
    if (activeId) {
      void this.router.navigate(['/workout', activeId]);
      return;
    }

    this.starting.set(true);
    this.api.createWorkout().subscribe({
      next: (workout) => this.openWorkout(workout),
      error: () => {
        this.starting.set(false);
        this.error.set('Kunde inte starta passet.');
        this.toast.error('Kunde inte starta passet.');
      },
    });
  }

  private openWorkout(workout: Workout | null | undefined): void {
    const id = workout?.id;
    if (!id) {
      this.api.dashboard().subscribe({
        next: (dashboard) => {
          this.data.set(dashboard);
          this.goToWorkout(dashboard.activeWorkout?.id);
        },
        error: () => {
          this.starting.set(false);
          this.error.set('Passet skapades men sidan kunde inte öppnas. Öppna det under Historik.');
        },
      });
      return;
    }

    this.goToWorkout(id);
  }

  private goToWorkout(id: string | undefined): void {
    if (!id) {
      this.starting.set(false);
      this.error.set('Passet skapades men sidan kunde inte öppnas. Öppna det under Historik.');
      return;
    }

    this.zone.run(() => {
      setTimeout(() => {
        void this.router.navigateByUrl(`/workout/${id}`).then((opened) => {
          this.starting.set(false);
          if (!opened) {
            this.error.set('Kunde inte öppna passet. Tryck Fortsätt pass.');
          }
        });
      });
    });
  }

  askCoach(): void {
    this.coachError.set(null);
    this.asking.set(true);
    this.api.coachRecommendation().subscribe({
      next: (recommendation) => {
        this.asking.set(false);
        this.recommendation.set(recommendation);
      },
      error: (err: HttpErrorResponse) => {
        this.asking.set(false);
        this.recommendation.set(null);
        const message = typeof err.error?.error === 'string'
          ? err.error.error
          : 'Coachen är inte tillgänglig just nu. Träningsloggen fungerar som vanligt.';
        this.coachError.set(message);
      },
    });
  }
}
