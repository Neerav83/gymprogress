import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { WorkoutSummary } from '../../core/models/models';
import { ConfirmService } from '../../core/services/confirm.service';
import { formatDay, formatKg } from '../../core/services/format';

@Component({
  selector: 'app-history',
  imports: [RouterLink],
  templateUrl: './history.html',
  styleUrl: './history.scss',
})
export class HistoryPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly confirm = inject(ConfirmService);
  protected readonly workouts = signal<WorkoutSummary[]>([]);
  protected readonly deletingId = signal<string | null>(null);
  protected day = formatDay;
  protected kg = formatKg;

  ngOnInit(): void {
    this.reload();
  }

  async remove(workout: WorkoutSummary, event: Event): Promise<void> {
    event.preventDefault();
    event.stopPropagation();

    const ok = await this.confirm.confirm({
      title: 'Ta bort passet?',
      message: `Passet från ${this.day(workout.startedAt)} tas bort för alltid.`,
    });
    if (!ok) {
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
