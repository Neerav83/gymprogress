import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { Exercise } from '../../core/models/models';
import { equipmentLabel, muscleLabel } from '../../core/services/format';

@Component({
  selector: 'app-exercise-picker',
  imports: [RouterLink, FormsModule],
  templateUrl: './exercise-picker.html',
  styleUrl: './exercise-picker.scss',
})
export class ExercisePickerPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly exercises = signal<Exercise[]>([]);
  protected readonly query = signal('');
  protected readonly adding = signal<string | null>(null);

  protected equipment = equipmentLabel;

  protected muscles(exercise: Exercise): string {
    return exercise.muscleGroups.map((group) => muscleLabel(group)).join(', ');
  }

  protected routeId(): string {
    return this.route.snapshot.paramMap.get('id')!;
  }

  ngOnInit(): void {
    this.api.exercises().subscribe((exercises) => this.exercises.set(exercises));
  }

  filtered(): Exercise[] {
    const q = this.query().trim().toLowerCase();
    return this.exercises().filter((exercise) =>
      !q ||
      exercise.name.toLowerCase().includes(q) ||
      exercise.muscleGroups.some((group) => muscleLabel(group).toLowerCase().includes(q)),
    );
  }

  add(exercise: Exercise): void {
    const workoutId = this.route.snapshot.paramMap.get('id')!;
    this.adding.set(exercise.id);
    this.api.addExercise(workoutId, exercise.id).subscribe({
      next: (workoutExercise) => {
        void this.router.navigate(['/workout', workoutId, 'exercise', workoutExercise.id]);
      },
      error: () => this.adding.set(null),
    });
  }
}
