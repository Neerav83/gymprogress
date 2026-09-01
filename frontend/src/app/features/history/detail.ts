import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Workout } from '../../core/models/models';
import { formatDay, formatKg, lastSessionSummary } from '../../core/services/format';

@Component({
  selector: 'app-workout-detail',
  imports: [RouterLink],
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
})
export class WorkoutDetailPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly workout = signal<Workout | null>(null);
  protected readonly deleting = signal(false);
  protected day = formatDay;
  protected kg = formatKg;
  protected last = lastSessionSummary;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.api.workout(id).subscribe((workout) => this.workout.set(workout));
  }

  remove(): void {
    const workout = this.workout();
    if (!workout) {
      return;
    }

    if (!confirm(`Ta bort passet från ${this.day(workout.startedAt)}? Det går inte att ångra.`)) {
      return;
    }

    this.deleting.set(true);
    this.api.deleteWorkout(workout.id).subscribe({
      next: () => this.router.navigate(['/history']),
      error: () => this.deleting.set(false),
    });
  }
}
