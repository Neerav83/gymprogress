import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Dashboard, WorkoutRecommendation } from '../../core/models/models';
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

  protected readonly data = signal<Dashboard | null>(null);
  protected readonly starting = signal(false);
  protected readonly asking = signal(false);
  protected readonly recommendation = signal<WorkoutRecommendation | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly coachError = signal<string | null>(null);

  ngOnInit(): void {
    this.api.dashboard().subscribe({
      next: (dashboard) => this.data.set(dashboard),
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
    const active = this.data()?.activeWorkout;
    if (active) {
      void this.router.navigate(['/workout', active.id]);
      return;
    }

    this.starting.set(true);
    this.api.createWorkout().subscribe({
      next: (workout) => {
        this.starting.set(false);
        this.toast.success('Nytt pass startat!');
        void this.router.navigate(['/workout', workout.id]);
      },
      error: () => {
        this.starting.set(false);
        this.error.set('Kunde inte starta passet.');
        this.toast.error('Kunde inte starta passet.');
      },
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

  createFromRecommendation(): void {
    const rec = this.recommendation();
    if (!rec || this.starting()) {
      return;
    }

    this.starting.set(true);
    this.api.createWorkoutFromRecommendation(rec).subscribe({
      next: (workout) => {
        this.starting.set(false);
        this.toast.success('Pass skapat från coachens rekommendation!');
        void this.router.navigate(['/workout', workout.id]);
      },
      error: (err: HttpErrorResponse) => {
        this.starting.set(false);
        const message = typeof err.error?.error === 'string'
          ? err.error.error
          : 'Kunde inte skapa pass från rekommendation.';
        this.toast.error(message);
      },
    });
  }
}
