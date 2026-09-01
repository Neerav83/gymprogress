import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { PersonalRecordHit, Workout, WorkoutExercise } from '../../core/models/models';
import { formatDay, formatKg, lastSessionSummary } from '../../core/services/format';
import { PrBannerComponent } from '../../shared/ui/pr-banner';
import { StepperComponent } from '../../shared/ui/stepper';

@Component({
  selector: 'app-set-logger',
  imports: [RouterLink, StepperComponent, PrBannerComponent],
  templateUrl: './set-logger.html',
  styleUrl: './set-logger.scss',
})
export class SetLoggerPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly workout = signal<Workout | null>(null);
  protected readonly saving = signal(false);
  protected readonly weight = signal(20);
  protected readonly reps = signal(10);
  protected readonly hits = signal<PersonalRecordHit[]>([]);
  protected readonly editingSetId = signal<string | null>(null);

  protected kg = formatKg;
  protected day = formatDay;
  protected last = lastSessionSummary;

  protected readonly exercise = computed(() => {
    const workoutExerciseId = this.route.snapshot.paramMap.get('workoutExerciseId');
    return this.workout()?.exercises.find((item) => item.id === workoutExerciseId) ?? null;
  });

  ngOnInit(): void {
    this.load((exercise) => this.prefill(exercise));
  }

  protected workoutId(): string {
    return this.route.snapshot.paramMap.get('id')!;
  }

  save(): void {
    const exercise = this.exercise();
    if (!exercise || this.saving()) {
      return;
    }

    this.saving.set(true);
    const editId = this.editingSetId();

    if (editId) {
      this.api.updateSet(this.workoutId(), exercise.id, editId, this.weight(), this.reps()).subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.editingSetId.set(null);
          this.hits.set([]);
          this.patchExercise(updated);
        },
        error: () => this.saving.set(false),
      });
      return;
    }

    this.api.addSet(this.workoutId(), exercise.id, this.weight(), this.reps()).subscribe({
      next: (result) => {
        this.saving.set(false);
        this.hits.set(result.personalRecords);
        this.patchExercise(result.exercise);
      },
      error: () => this.saving.set(false),
    });
  }

  edit(setId: string, weightKg: number, reps: number): void {
    this.editingSetId.set(setId);
    this.weight.set(weightKg);
    this.reps.set(reps);
  }

  remove(setId: string): void {
    const exercise = this.exercise();
    if (!exercise) {
      return;
    }

    this.api.deleteSet(this.workoutId(), exercise.id, setId).subscribe(() => this.load());
  }

  private load(after?: (exercise: WorkoutExercise) => void): void {
    this.api.workout(this.workoutId()).subscribe((workout) => {
      this.workout.set(workout);
      const exercise = this.exercise();
      if (exercise && after) {
        after(exercise);
      }
    });
  }

  private prefill(exercise: WorkoutExercise): void {
    const latestToday = exercise.sets.at(-1);
    const last = exercise.lastSession?.sets.at(-1);
    this.weight.set(latestToday?.weightKg ?? last?.weightKg ?? 20);
    this.reps.set(latestToday?.reps ?? last?.reps ?? 10);
  }

  private patchExercise(updated: WorkoutExercise): void {
    const workout = this.workout();
    if (!workout) {
      return;
    }

    this.workout.set({
      ...workout,
      exercises: workout.exercises.map((item) => (item.id === updated.id ? updated : item)),
      totalVolumeKg: workout.exercises
        .map((item) => (item.id === updated.id ? updated : item))
        .flatMap((item) => item.sets)
        .reduce((sum, set) => sum + set.volumeKg, 0),
    });
  }
}
