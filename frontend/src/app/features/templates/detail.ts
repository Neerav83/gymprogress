import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Exercise, WorkoutTemplate, WorkoutTemplateExercise } from '../../core/models/models';
import { ConfirmService } from '../../core/services/confirm.service';
import { ToastService } from '../../core/services/toast.service';
import { WorkoutNav } from '../../core/services/workout-nav';
import { equipmentLabel, muscleLabel } from '../../core/services/format';

@Component({
  selector: 'app-template-detail',
  imports: [RouterLink, FormsModule],
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
})
export class TemplateDetailPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly confirm = inject(ConfirmService);
  private readonly nav = inject(WorkoutNav);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly template = signal<WorkoutTemplate | null>(null);
  protected readonly name = signal('');
  protected readonly description = signal('');
  protected readonly exercises = signal<WorkoutTemplateExercise[]>([]);
  protected readonly catalog = signal<Exercise[]>([]);
  protected readonly query = signal('');
  protected readonly adding = signal(false);
  protected readonly saving = signal(false);
  protected readonly starting = signal(false);
  protected readonly deleting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly dirty = computed(() => {
    const original = this.template();
    if (!original) {
      return false;
    }

    const originalDescription = original.description ?? '';
    const originalIds = original.exercises.map((exercise) => exercise.exerciseId).join(',');
    const currentIds = this.exercises()
      .map((exercise) => exercise.exerciseId)
      .join(',');

    return (
      this.name().trim() !== original.name ||
      this.description().trim() !== originalDescription ||
      originalIds !== currentIds
    );
  });

  protected equipment = equipmentLabel;

  protected muscles(exercise: { muscleGroups: string[] }): string {
    return exercise.muscleGroups.map((group) => muscleLabel(group)).join(', ');
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('Mallen kunde inte öppnas.');
      return;
    }

    this.api.workoutTemplate(id).subscribe({
      next: (template) => this.apply(template),
      error: () => this.error.set('Kunde inte hämta mallen.'),
    });

    this.api.exercises().subscribe({
      next: (exercises) => this.catalog.set(exercises),
    });
  }

  filteredCatalog(): Exercise[] {
    const q = this.query().trim().toLowerCase();
    const taken = new Set(this.exercises().map((exercise) => exercise.exerciseId));
    return this.catalog().filter((exercise) => {
      if (taken.has(exercise.id)) {
        return false;
      }
      return (
        !q ||
        exercise.name.toLowerCase().includes(q) ||
        exercise.muscleGroups.some((group) => muscleLabel(group).toLowerCase().includes(q))
      );
    });
  }

  add(exercise: Exercise): void {
    this.adding.set(false);
    this.query.set('');
    this.exercises.update((current) => [
      ...current,
      {
        exerciseId: exercise.id,
        exerciseName: exercise.name,
        muscleGroups: exercise.muscleGroups,
        equipment: exercise.equipment,
        sortOrder: current.length,
        suggestedSets: null,
        suggestedWeight: null,
        suggestedRepsMin: null,
        suggestedRepsMax: null,
      },
    ]);
  }

  removeExercise(exerciseId: string): void {
    this.exercises.update((current) => current.filter((exercise) => exercise.exerciseId !== exerciseId));
  }

  move(exerciseId: string, direction: -1 | 1): void {
    this.exercises.update((current) => {
      const index = current.findIndex((exercise) => exercise.exerciseId === exerciseId);
      const next = index + direction;
      if (index < 0 || next < 0 || next >= current.length) {
        return current;
      }

      const copy = [...current];
      [copy[index], copy[next]] = [copy[next], copy[index]];
      return copy;
    });
  }

  save(): void {
    const template = this.template();
    if (!template || this.saving()) {
      return;
    }

    const name = this.name().trim();
    if (!name) {
      this.toast.error('Mallen måste ha ett namn.');
      return;
    }

    if (!this.exercises().length) {
      this.toast.error('Mallen måste ha minst en övning.');
      return;
    }

    this.saving.set(true);
    this.api
      .updateWorkoutTemplate(template.id, {
        name,
        description: this.description().trim() || null,
        exerciseIds: this.exercises().map((exercise) => exercise.exerciseId),
      })
      .subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.apply(updated);
          this.toast.success('Mallen sparad');
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          const message =
            typeof err.error?.error === 'string' ? err.error.error : 'Kunde inte spara mallen.';
          this.toast.error(message);
        },
      });
  }

  start(): void {
    const template = this.template();
    if (!template || this.starting()) {
      return;
    }

    if (this.dirty()) {
      this.toast.error('Spara mallen innan du startar passet.');
      return;
    }

    this.starting.set(true);
    this.api.createWorkoutFromTemplate(template.id).subscribe({
      next: async (workout) => {
        const opened = await this.nav.open(workout?.id);
        this.starting.set(false);
        if (!opened) {
          this.toast.error('Passet skapades. Öppna det från Idag.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.starting.set(false);
        const message =
          typeof err.error?.error === 'string' ? err.error.error : 'Kunde inte starta pass från mallen.';
        this.toast.error(message);
      },
    });
  }

  async remove(): Promise<void> {
    const template = this.template();
    if (!template) {
      return;
    }

    const ok = await this.confirm.confirm({
      title: 'Ta bort mallen?',
      message: `"${template.name}" tas bort. Dina loggade pass påverkas inte.`,
    });
    if (!ok) {
      return;
    }

    this.deleting.set(true);
    this.api.deleteWorkoutTemplate(template.id).subscribe({
      next: () => {
        this.toast.success('Mall borttagen');
        void this.router.navigate(['/templates']);
      },
      error: () => {
        this.deleting.set(false);
        this.toast.error('Kunde inte ta bort mall');
      },
    });
  }

  private apply(template: WorkoutTemplate): void {
    this.template.set(template);
    this.name.set(template.name);
    this.description.set(template.description ?? '');
    this.exercises.set([...template.exercises]);
  }
}
