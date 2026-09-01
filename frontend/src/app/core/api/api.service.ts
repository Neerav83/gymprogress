import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  AddSetResponse,
  Dashboard,
  Exercise,
  ExerciseProgress,
  PersonalRecord,
  Workout,
  WorkoutExercise,
  WorkoutRecommendation,
  WorkoutSummary,
  WorkoutTemplate,
} from '../models/models';

const API = '/api/v1';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private readonly http: HttpClient) {}

  dashboard() {
    return this.http.get<Dashboard>(`${API}/dashboard`);
  }

  exercises() {
    return this.http.get<Exercise[]>(`${API}/exercises`);
  }

  workouts() {
    return this.http.get<WorkoutSummary[]>(`${API}/workouts`);
  }

  workout(id: string) {
    return this.http.get<Workout>(`${API}/workouts/${id}`);
  }

  createWorkout() {
    return this.http.post<Workout>(`${API}/workouts`, {});
  }

  createWorkoutFromRecommendation(recommendation: WorkoutRecommendation) {
    return this.http.post<Workout>(`${API}/workouts/from-recommendation`, {
      workoutType: recommendation.workoutType,
      exercises: recommendation.exercises.map((e) => ({
        exerciseId: e.exerciseId,
        sets: e.sets,
        suggestedWeight: e.suggestedWeight,
        targetRepsMin: e.targetRepsMin,
        targetRepsMax: e.targetRepsMax,
      })),
    });
  }

  finishWorkout(id: string) {
    return this.http.post<Workout>(`${API}/workouts/${id}/finish`, {});
  }

  deleteWorkout(id: string) {
    return this.http.delete<void>(`${API}/workouts/${id}`);
  }

  addExercise(workoutId: string, exerciseId: string) {
    return this.http.post<WorkoutExercise>(`${API}/workouts/${workoutId}/exercises`, { exerciseId });
  }

  removeExercise(workoutId: string, workoutExerciseId: string) {
    return this.http.delete<void>(`${API}/workouts/${workoutId}/exercises/${workoutExerciseId}`);
  }

  addSet(workoutId: string, workoutExerciseId: string, weightKg: number, reps: number) {
    return this.http.post<AddSetResponse>(
      `${API}/workouts/${workoutId}/exercises/${workoutExerciseId}/sets`,
      { weightKg, reps },
    );
  }

  updateSet(
    workoutId: string,
    workoutExerciseId: string,
    setId: string,
    weightKg: number,
    reps: number,
  ) {
    return this.http.put<WorkoutExercise>(
      `${API}/workouts/${workoutId}/exercises/${workoutExerciseId}/sets/${setId}`,
      { weightKg, reps },
    );
  }

  deleteSet(workoutId: string, workoutExerciseId: string, setId: string) {
    return this.http.delete<void>(
      `${API}/workouts/${workoutId}/exercises/${workoutExerciseId}/sets/${setId}`,
    );
  }

  progress(exerciseId: string, range = 'all') {
    return this.http.get<ExerciseProgress>(`${API}/progress/${exerciseId}`, { params: { range } });
  }

  personalRecords() {
    return this.http.get<PersonalRecord[]>(`${API}/personal-records`);
  }

  coachRecommendation() {
    return this.http.get<WorkoutRecommendation>(`${API}/coach/recommendation`);
  }

  workoutTemplates() {
    return this.http.get<WorkoutTemplate[]>(`${API}/workout-templates`);
  }

  workoutTemplate(id: string) {
    return this.http.get<WorkoutTemplate>(`${API}/workout-templates/${id}`);
  }

  createTemplateFromWorkout(workoutId: string, name: string, description: string | null) {
    return this.http.post<WorkoutTemplate>(`${API}/workout-templates`, {
      workoutId,
      name,
      description,
    });
  }

  createWorkoutFromTemplate(templateId: string) {
    return this.http.post<Workout>(`${API}/workouts/from-template/${templateId}`, {});
  }

  deleteWorkoutTemplate(id: string) {
    return this.http.delete<void>(`${API}/workout-templates/${id}`);
  }
}
