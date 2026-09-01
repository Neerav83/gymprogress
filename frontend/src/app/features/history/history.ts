import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { WorkoutSummary } from '../../core/models/models';
import { formatDay, formatKg } from '../../core/services/format';

@Component({
  selector: 'app-history',
  imports: [RouterLink],
  templateUrl: './history.html',
  styleUrl: './history.scss',
})
export class HistoryPage implements OnInit {
  private readonly api = inject(ApiService);
  protected readonly workouts = signal<WorkoutSummary[]>([]);
  protected readonly deletingId = signal<string | null>(null);
  protected day = formatDay;
  protected kg = formatKg;

  ngOnInit(): void {
    this.reload();
  }

  remove(workout: WorkoutSummary, event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    if (!confirm(`Ta bort passet från ${this.day(workout.startedAt)}? Det går inte att ångra.`)) {
      return;
    }

    this.deletingId.set(workout.id);
    this.api.deleteWorkout(workout.id).subscribe({
      next: () => {
        this.workouts.update((items) => items.filter((item) => item.id !== workout.id));
        this.deletingId.set(null);
      },
      error: () => this.deletingId.set(null),
    });
  }

  private reload(): void {
    this.api.workouts().subscribe((workouts) => this.workouts.set(workouts));
  }
}
