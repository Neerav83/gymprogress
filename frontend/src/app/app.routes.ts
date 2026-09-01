import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/services/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login').then((m) => m.LoginPage),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register').then((m) => m.RegisterPage),
  },
  {
    path: '',
    pathMatch: 'full',
    canActivate: [authGuard],
    loadComponent: () => import('./features/home/home').then((m) => m.HomePage),
  },
  {
    path: 'workout/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/workout/session').then((m) => m.WorkoutSessionPage),
  },
  {
    path: 'workout/:id/add-exercise',
    canActivate: [authGuard],
    loadComponent: () => import('./features/workout/exercise-picker').then((m) => m.ExercisePickerPage),
  },
  {
    path: 'workout/:id/exercise/:workoutExerciseId',
    canActivate: [authGuard],
    loadComponent: () => import('./features/workout/set-logger').then((m) => m.SetLoggerPage),
  },
  {
    path: 'history',
    canActivate: [authGuard],
    loadComponent: () => import('./features/history/history').then((m) => m.HistoryPage),
  },
  {
    path: 'history/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/history/detail').then((m) => m.WorkoutDetailPage),
  },
  {
    path: 'progress/:exerciseId',
    canActivate: [authGuard],
    loadComponent: () => import('./features/progress/progress').then((m) => m.ProgressPage),
  },
  {
    path: 'records',
    canActivate: [authGuard],
    loadComponent: () => import('./features/records/records').then((m) => m.RecordsPage),
  },
  { path: '**', redirectTo: '' },
];
