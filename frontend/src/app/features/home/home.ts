import { HttpErrorResponse } from '@angular/common/http';
import { Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Dashboard, Workout, WorkoutRecommendation, WorkoutTemplate } from '../../core/models/models';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { WorkoutNav } from '../../core/services/workout-nav';
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
  private readonly nav = inject(WorkoutNav);

  protected readonly data = signal<Dashboard | null>(null);
  protected readonly templates = signal<WorkoutTemplate[]>([]);
  protected readonly templatesReady = signal(false);
  protected readonly pickingStart = signal(false);
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

    this.api.workoutTemplates().subscribe({
      next: (templates) => {
        this.templates.set(templates);
        this.templatesReady.set(true);
      },
      error: () => this.templatesReady.set(true),
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
      void this.nav.open(activeId);
      return;
    }

    if (!this.templatesReady() || this.templates().length) {
      this.pickingStart.set(true);
      return;
    }

    this.createEmpty();
  }

  closeStartPicker(): void {
    this.pickingStart.set(false);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.pickingStart()) {
      this.closeStartPicker();
    }
  }

  createEmpty(): void {
    this.pickingStart.set(false);
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

  startFromTemplate(template: WorkoutTemplate): void {
    this.pickingStart.set(false);
    this.starting.set(true);
    this.api.createWorkoutFromTemplate(template.id).subscribe({
      next: (workout) => this.openWorkout(workout),
      error: (err: HttpErrorResponse) => {
        this.starting.set(false);
        const message = typeof err.error?.error === 'string'
          ? err.error.error
          : 'Kunde inte starta pass från mallen.';
        this.toast.error(message);
      },
    });
  }

  private openWorkout(workout: Workout | null | undefined): void {
    if (workout) {
      this.data.update((dashboard) =>
        dashboard ? { ...dashboard, activeWorkout: workout } : dashboard,
      );
    }

    const id = workout?.id;
    if (!id) {
      this.api.dashboard().subscribe({
        next: (dashboard) => {
          this.data.set(dashboard);
          void this.finishOpen(dashboard.activeWorkout?.id);
        },
        error: () => {
          this.starting.set(false);
          this.error.set('Passet skapades men sidan kunde inte öppnas. Öppna det under Historik.');
        },
      });
      return;
    }

    void this.finishOpen(id);
  }

  private async finishOpen(id: string | undefined): Promise<void> {
    const opened = await this.nav.open(id);
    this.starting.set(false);
    if (!opened) {
      this.error.set('Kunde inte öppna passet. Tryck Fortsätt pass.');
    }
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
      next: (workout) => this.openWorkout(workout),
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
