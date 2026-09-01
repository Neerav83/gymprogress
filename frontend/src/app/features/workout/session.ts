import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Workout } from '../../core/models/models';
import { formatClock, formatKg, lastSessionSummary } from '../../core/services/format';

@Component({
  selector: 'app-workout-session',
  imports: [RouterLink],
  templateUrl: './session.html',
  styleUrl: './session.scss',
})
export class WorkoutSessionPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly workout = signal<Workout | null>(null);
  protected readonly finishing = signal(false);
  protected readonly deleting = signal(false);

  protected clock = formatClock;
  protected kg = formatKg;
  protected last = lastSessionSummary;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.api.workout(id).subscribe((workout) => this.workout.set(workout));
  }

  finish(): void {
    const workout = this.workout();
    if (!workout) {
      return;
    }

    this.finishing.set(true);
    this.api.finishWorkout(workout.id).subscribe({
      next: () => this.router.navigate(['/history', workout.id]),
      error: () => this.finishing.set(false),
    });
  }

  remove(): void {
    const workout = this.workout();
    if (!workout) {
      return;
    }

    if (!confirm('Ta bort det här passet? Det går inte att ångra.')) {
      return;
    }

    this.deleting.set(true);
    this.api.deleteWorkout(workout.id).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => this.deleting.set(false),
    });
  }
}
