import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ToastService } from '../../core/services/toast.service';
import { Workout } from '../../core/models/models';
import { ConfirmService } from '../../core/services/confirm.service';
import { formatDay, formatKg, lastSessionSummary } from '../../core/services/format';

@Component({
  selector: 'app-workout-detail',
  imports: [RouterLink],
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
})
export class WorkoutDetailPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly confirm = inject(ConfirmService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  protected readonly workout = signal<Workout | null>(null);
  protected readonly deleting = signal(false);
  protected readonly saving = signal(false);
  protected day = formatDay;
  protected kg = formatKg;
  protected last = lastSessionSummary;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.api.workout(id).subscribe((workout) => this.workout.set(workout));
  }

  async remove(): Promise<void> {
    const workout = this.workout();
    if (!workout) {
      return;
    }

    const ok = await this.confirm.confirm({
      title: 'Ta bort passet?',
      message: `Passet från ${this.day(workout.startedAt)} tas bort för alltid.`,
    });
    if (!ok) {
      return;
    }

    this.deleting.set(true);
    this.api.deleteWorkout(workout.id).subscribe({
      next: () => {
        this.toast.success('Pass borttaget');
        this.router.navigate(['/history']);
      },
      error: () => {
        this.deleting.set(false);
        this.toast.error('Kunde inte ta bort pass');
      },
    });
  }

  saveAsTemplate(): void {
    const workout = this.workout();
    if (!workout || this.saving()) {
      return;
    }

    const name = prompt('Namnge mallen:', `${this.day(workout.startedAt)} Pass`);
    if (!name) {
      return;
    }

    this.saving.set(true);
    this.api.createTemplateFromWorkout(workout.id, name, null).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(`Mall "${name}" sparad!`);
      },
      error: () => {
        this.saving.set(false);
        this.toast.error('Kunde inte spara mall');
      },
    });
  }
}
