import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Workout } from '../../core/models/models';
import { ConfirmService } from '../../core/services/confirm.service';
import { formatClock, formatKg, lastSessionSummary } from '../../core/services/format';

@Component({
  selector: 'app-workout-session',
  imports: [RouterLink],
  templateUrl: './session.html',
  styleUrl: './session.scss',
})
export class WorkoutSessionPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly confirm = inject(ConfirmService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly workout = signal<Workout | null>(null);
  protected readonly finishing = signal(false);
  protected readonly deleting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected clock = formatClock;
  protected kg = formatKg;
  protected last = lastSessionSummary;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id || id === 'undefined') {
      this.error.set('Passet kunde inte öppnas.');
      return;
    }

    this.api.workout(id).subscribe({
      next: (workout) => this.workout.set(workout),
      error: () => this.error.set('Kunde inte hämta passet.'),
    });
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

  async remove(): Promise<void> {
    const workout = this.workout();
    if (!workout) {
      return;
    }

    const ok = await this.confirm.confirm({
      title: 'Ta bort passet?',
      message: 'Det här passet tas bort för alltid. Set och volym försvinner med det.',
    });
    if (!ok) {
      return;
    }

    this.deleting.set(true);
    this.api.deleteWorkout(workout.id).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => this.deleting.set(false),
    });
  }
}
