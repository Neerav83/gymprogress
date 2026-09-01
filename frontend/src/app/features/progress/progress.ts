import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ExerciseProgress } from '../../core/models/models';
import { formatDay, formatKg, recordLabel } from '../../core/services/format';

@Component({
  selector: 'app-progress',
  imports: [RouterLink],
  templateUrl: './progress.html',
  styleUrl: './progress.scss',
})
export class ProgressPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly data = signal<ExerciseProgress | null>(null);
  protected readonly range = signal('all');
  protected readonly ranges = [
    { id: '7d', label: '7 dagar' },
    { id: '30d', label: '30 dagar' },
    { id: '3m', label: '3 mån' },
    { id: '6m', label: '6 mån' },
    { id: 'all', label: 'Alltid' },
  ];

  protected day = formatDay;
  protected kg = formatKg;
  protected recordName = recordLabel;

  ngOnInit(): void {
    this.load();
  }

  select(range: string): void {
    this.range.set(range);
    this.load();
  }

  chartPath(): string {
    const points = this.data()?.points ?? [];
    if (points.length === 0) {
      return '';
    }

    const values = points.map((point) => point.maxWeightKg);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const span = Math.max(max - min, 1);

    return points
      .map((point, index) => {
        const x = points.length === 1 ? 50 : (index / (points.length - 1)) * 100;
        const y = 36 - ((point.maxWeightKg - min) / span) * 28;
        return `${index === 0 ? 'M' : 'L'} ${x.toFixed(2)} ${y.toFixed(2)}`;
      })
      .join(' ');
  }

  private load(): void {
    const exerciseId = this.route.snapshot.paramMap.get('exerciseId')!;
    this.api.progress(exerciseId, this.range()).subscribe((progress) => this.data.set(progress));
  }
}
