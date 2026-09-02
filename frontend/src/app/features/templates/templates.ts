import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ToastService } from '../../core/services/toast.service';
import { WorkoutTemplate } from '../../core/models/models';

@Component({
  selector: 'app-templates',
  imports: [RouterLink],
  templateUrl: './templates.html',
  styleUrl: './templates.scss',
})
export class TemplatesPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  protected readonly templates = signal<WorkoutTemplate[]>([]);

  ngOnInit(): void {
    this.api.workoutTemplates().subscribe({
      next: (templates) => this.templates.set(templates),
      error: () => this.toast.error('Kunde inte ladda mallar'),
    });
  }
}
