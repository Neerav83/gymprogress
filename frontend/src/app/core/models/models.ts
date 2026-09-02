export interface Exercise {
  id: string;
  name: string;
  muscleGroups: string[];
  equipment: string;
}

export interface WorkoutSet {
  id: string;
  setNumber: number;
  weightKg: number;
  reps: number;
  volumeKg: number;
  estimatedOneRepMax: number;
  completedAt: string;
}

export interface LastSession {
  performedAt: string;
  sets: WorkoutSet[];
}

export interface WorkoutExercise {
  id: string;
  exerciseId: string;
  exerciseName: string;
  muscleGroups: string[];
  equipment: string;
  sortOrder: number;
  sets: WorkoutSet[];
  lastSession: LastSession | null;
}

export interface Workout {
  id: string;
  startedAt: string;
  finishedAt: string | null;
  isActive: boolean;
  notes: string | null;
  exercises: WorkoutExercise[];
  totalVolumeKg: number;
}

export interface WorkoutSummary {
  id: string;
  startedAt: string;
  finishedAt: string | null;
  isActive: boolean;
  exerciseCount: number;
  setCount: number;
  totalVolumeKg: number;
  exerciseNames: string[];
}

export interface PersonalRecord {
  exerciseId: string;
  exerciseName: string;
  type: string;
  weightKg: number;
  reps: number;
  value: number;
  achievedAt: string;
  label: string;
}

export interface PersonalRecordHit {
  type: string;
  label: string;
  weightKg: number;
  reps: number;
  previousLabel: string | null;
}

export interface AddSetResponse {
  set: WorkoutSet;
  exercise: WorkoutExercise;
  personalRecords: PersonalRecordHit[];
}

export interface ProgressPoint {
  date: string;
  maxWeightKg: number;
  totalReps: number;
  volumeKg: number;
  estimatedOneRepMax: number;
}

export interface ExerciseProgress {
  exerciseId: string;
  exerciseName: string;
  range: string;
  points: ProgressPoint[];
  records: PersonalRecord[];
}

export interface Dashboard {
  activeWorkout: Workout | null;
  recentWorkouts: WorkoutSummary[];
  recentRecords: PersonalRecord[];
  workoutsThisWeek: number;
}

export interface WorkoutRecommendationExercise {
  exerciseId: string;
  exerciseName: string;
  muscleGroups: string[];
  equipment: string;
  sets: number;
  targetRepsMin: number;
  targetRepsMax: number;
  suggestedWeight: number;
  progression: 'increase' | 'maintain' | 'decrease' | string;
  reason: string;
}

export interface WorkoutRecommendation {
  workoutType: string;
  coachNote: string;
  exercises: WorkoutRecommendationExercise[];
}

export interface WorkoutTemplate {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
  exercises: WorkoutTemplateExercise[];
}

export interface WorkoutTemplateExercise {
  exerciseId: string;
  exerciseName: string;
  muscleGroups: string[];
  equipment: string;
  sortOrder: number;
  suggestedSets: number | null;
  suggestedWeight: number | null;
  suggestedRepsMin: number | null;
  suggestedRepsMax: number | null;
}

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  profileImageUrl: string | null;
  createdAt: string;
}

export interface BodyMetrics {
  id: string;
  date: string;
  weightKg: number | null;
  heightCm: number | null;
  chestCm: number | null;
  waistCm: number | null;
  hipsCm: number | null;
  armCm: number | null;
  thighCm: number | null;
  notes: string | null;
}

export interface BodyMetricsHistory {
  metrics: BodyMetrics[];
}

