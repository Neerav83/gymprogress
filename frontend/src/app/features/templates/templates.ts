import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { ToastService } from '../../core/services/toast.service';
import { WorkoutNav } from '../../core/services/workout-nav';
import { WorkoutTemplate } from '../../core/models/models';

@Component({
  selector: 'app-templates',
  imports: [RouterLink],
  template: `
    <header class="top">
      <a routerLink="/">Tillbaka</a>
      <h1 class="page-title">Mina mallar</h1>
    </header>

    @if (templates().length === 0) {
      <p class="empty">
        Inga mallar än. När du har slutfört ett pass, gå till historiken och spara det som en mall.
      </p>
    } @else {
      <div class="stack">
        @for (template of templates(); track template.id) {
          <article class="card">
            <div>
              <strong>{{ template.name }}</strong>
              <p class="muted">{{ template.exercises.length }} övningar</p>
              @if (template.description) {
                <p class="muted">{{ template.description }}</p>
              }
            </div>
            <div class="actions">
              <button type="button" (click)="startFromTemplate(template)" [disabled]="starting()">
                Starta
              </button>
              <button type="button" class="danger-text" (click)="remove(template)">Ta bort</button>
            </div>
          </article>
        }
      </div>
    }
  `,
  styles: `
    .top {
      margin-bottom: 24px;
    }

    .top h1 {
      margin: 8px 0 0;
    }

    .stack {
      display: grid;
      gap: 12px;
    }

    .card {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .card p {
      margin: 4px 0 0;
    }

    .actions {
      display: flex;
      gap: 12px;
      justify-content: flex-end;
    }

    .actions button {
      padding: 6px 16px;
      border-radius: 6px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .actions button:first-child {
      background: var(--brand);
      color: white;
      border: none;
    }

    .actions button:first-child:hover {
      background: var(--brand-dark);
    }

    .danger-text {
      color: var(--danger) !important;
      background: transparent;
      border: none;
    }

    .danger-text:hover {
      background: rgba(239, 68, 68, 0.1);
    }
  `,
})
export class TemplatesPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly nav = inject(WorkoutNav);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  protected readonly templates = signal<WorkoutTemplate[]>([]);
  protected readonly starting = signal(false);

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.api.workoutTemplates().subscribe({
      next: (templates) => this.templates.set(templates),
      error: () => this.toast.error('Kunde inte ladda mallar'),
    });
  }

  startFromTemplate(template: WorkoutTemplate): void {
    if (this.starting()) {
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
      error: () => {
        this.starting.set(false);
        this.toast.error('Kunde inte starta pass från mall');
      },
    });
  }

  async remove(template: WorkoutTemplate): Promise<void> {
    const ok = await this.confirm.confirm({
      title: 'Ta bort mallen?',
      message: `"${template.name}" tas bort. Dina loggade pass påverkas inte.`,
    });
    if (!ok) {
      return;
    }

    this.api.deleteWorkoutTemplate(template.id).subscribe({
      next: () => {
        this.toast.success('Mall borttagen');
        this.load();
      },
      error: () => this.toast.error('Kunde inte ta bort mall'),
    });
  }
}
