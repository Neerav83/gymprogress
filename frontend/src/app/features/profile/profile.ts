import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { BodyMetrics, UserProfile } from '../../core/models/models';
import { ToastService } from '../../core/services/toast.service';
import { formatDay } from '../../core/services/format';

@Component({
  selector: 'app-profile',
  imports: [FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class ProfilePage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  protected readonly profile = signal<UserProfile | null>(null);
  protected readonly metrics = signal<BodyMetrics[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);

  // Profile editing
  protected readonly editingProfile = signal(false);
  protected readonly displayName = signal('');
  protected readonly profileImageUrl = signal('');

  // Password change
  protected readonly changingPassword = signal(false);
  protected readonly currentPassword = signal('');
  protected readonly newPassword = signal('');
  protected readonly confirmPassword = signal('');

  // Body metrics
  protected readonly addingMetrics = signal(false);
  protected readonly newWeightKg = signal<number | null>(null);
  protected readonly newHeightCm = signal<number | null>(null);
  protected readonly newChestCm = signal<number | null>(null);
  protected readonly newWaistCm = signal<number | null>(null);
  protected readonly newHipsCm = signal<number | null>(null);
  protected readonly newArmCm = signal<number | null>(null);
  protected readonly newThighCm = signal<number | null>(null);
  protected readonly newNotes = signal('');

  ngOnInit(): void {
    this.loadProfile();
    this.loadMetrics();
  }

  protected day = formatDay;

  private loadProfile(): void {
    this.api.getUserProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.displayName.set(profile.displayName);
        this.profileImageUrl.set(profile.profileImageUrl || '');
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Kunde inte ladda profil');
        this.loading.set(false);
      },
    });
  }

  private loadMetrics(): void {
    this.api.getBodyMetrics().subscribe({
      next: (data) => {
        this.metrics.set(data.metrics);
      },
      error: () => {
        this.toast.error('Kunde inte ladda mått');
      },
    });
  }

  startEditProfile(): void {
    this.editingProfile.set(true);
  }

  cancelEditProfile(): void {
    const p = this.profile();
    if (p) {
      this.displayName.set(p.displayName);
      this.profileImageUrl.set(p.profileImageUrl || '');
    }
    this.editingProfile.set(false);
  }

  saveProfile(): void {
    this.saving.set(true);
    this.api
      .updateProfile({
        displayName: this.displayName(),
        profileImageUrl: this.profileImageUrl() || undefined,
      })
      .subscribe({
        next: (profile) => {
          this.profile.set(profile);
          this.editingProfile.set(false);
          this.saving.set(false);
          this.toast.success('Profilen uppdaterad!');
        },
        error: () => {
          this.saving.set(false);
          this.toast.error('Kunde inte uppdatera profil');
        },
      });
  }

  startChangePassword(): void {
    this.changingPassword.set(true);
    this.currentPassword.set('');
    this.newPassword.set('');
    this.confirmPassword.set('');
  }

  cancelChangePassword(): void {
    this.changingPassword.set(false);
    this.currentPassword.set('');
    this.newPassword.set('');
    this.confirmPassword.set('');
  }

  changePassword(): void {
    if (this.newPassword() !== this.confirmPassword()) {
      this.toast.error('Lösenorden matchar inte');
      return;
    }

    if (this.newPassword().length < 6) {
      this.toast.error('Lösenordet måste vara minst 6 tecken');
      return;
    }

    this.saving.set(true);
    this.api.changePassword(this.currentPassword(), this.newPassword()).subscribe({
      next: () => {
        this.changingPassword.set(false);
        this.saving.set(false);
        this.currentPassword.set('');
        this.newPassword.set('');
        this.confirmPassword.set('');
        this.toast.success('Lösenordet ändrat!');
      },
      error: () => {
        this.saving.set(false);
        this.toast.error('Kunde inte ändra lösenord. Kontrollera nuvarande lösenord.');
      },
    });
  }

  startAddMetrics(): void {
    this.addingMetrics.set(true);
  }

  cancelAddMetrics(): void {
    this.addingMetrics.set(false);
    this.newWeightKg.set(null);
    this.newHeightCm.set(null);
    this.newChestCm.set(null);
    this.newWaistCm.set(null);
    this.newHipsCm.set(null);
    this.newArmCm.set(null);
    this.newThighCm.set(null);
    this.newNotes.set('');
  }

  saveMetrics(): void {
    const payload: any = {};
    if (this.newWeightKg()) payload.weightKg = this.newWeightKg();
    if (this.newHeightCm()) payload.heightCm = this.newHeightCm();
    if (this.newChestCm()) payload.chestCm = this.newChestCm();
    if (this.newWaistCm()) payload.waistCm = this.newWaistCm();
    if (this.newHipsCm()) payload.hipsCm = this.newHipsCm();
    if (this.newArmCm()) payload.armCm = this.newArmCm();
    if (this.newThighCm()) payload.thighCm = this.newThighCm();
    if (this.newNotes()) payload.notes = this.newNotes();

    this.saving.set(true);
    this.api.addBodyMetrics(payload).subscribe({
      next: (metrics) => {
        this.metrics.update((m) => [metrics, ...m]);
        this.cancelAddMetrics();
        this.saving.set(false);
        this.toast.success('Mått tillagt!');
      },
      error: () => {
        this.saving.set(false);
        this.toast.error('Kunde inte lägga till mått');
      },
    });
  }

  deleteMetrics(id: string): void {
    if (!confirm('Är du säker på att du vill ta bort dessa mått?')) {
      return;
    }

    this.api.deleteBodyMetrics(id).subscribe({
      next: () => {
        this.metrics.update((m) => m.filter((item) => item.id !== id));
        this.toast.success('Mått borttaget');
      },
      error: () => {
        this.toast.error('Kunde inte ta bort mått');
      },
    });
  }

  getLatestMetric(key: keyof BodyMetrics): number | null {
    const m = this.metrics();
    if (!m.length) return null;
    const value = m[0][key];
    return typeof value === 'number' ? value : null;
  }

  getMetricChange(key: keyof BodyMetrics): number | null {
    const m = this.metrics();
    if (m.length < 2) return null;
    const latest = m[0][key];
    const previous = m[1][key];
    if (typeof latest !== 'number' || typeof previous !== 'number') return null;
    return latest - previous;
  }
}
