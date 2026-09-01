import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { PersonalRecord } from '../../core/models/models';
import { formatDay, recordLabel } from '../../core/services/format';

@Component({
  selector: 'app-records',
  imports: [RouterLink],
  templateUrl: './records.html',
  styleUrl: './records.scss',
})
export class RecordsPage implements OnInit {
  private readonly api = inject(ApiService);
  protected readonly records = signal<PersonalRecord[]>([]);
  protected day = formatDay;
  protected recordName = recordLabel;

  ngOnInit(): void {
    this.api.personalRecords().subscribe((records) => this.records.set(records));
  }
}
