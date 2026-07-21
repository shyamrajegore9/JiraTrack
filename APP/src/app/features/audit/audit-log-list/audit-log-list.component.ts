import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { AuditService } from '../../../core/services/audit.service';
import {
  AUDIT_ACTIONS,
  AUDIT_ENTITY_TYPES,
  AuditLogListItem
} from '../../../core/models/audit.model';
import { PagedResponse } from '../../../core/models/api-response.model';
import { AuditLogDetailDialogComponent } from '../audit-log-detail-dialog/audit-log-detail-dialog.component';

@Component({
  selector: 'app-audit-log-list',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './audit-log-list.component.html',
  styleUrl: './audit-log-list.component.scss'
})
export class AuditLogListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auditService = inject(AuditService);
  private dialog = inject(MatDialog);

  readonly entityTypes = AUDIT_ENTITY_TYPES;
  readonly actions = AUDIT_ACTIONS;

  displayedColumns = ['timestamp', 'entityType', 'entityId', 'action', 'userName', 'ipAddress', 'actions'];
  logs = signal<AuditLogListItem[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  pageSize = 20;
  pageNumber = 1;

  filterForm = this.fb.nonNullable.group({
    entityType: [''],
    entityId: [null as number | null],
    userId: [null as number | null],
    action: [''],
    startDate: [null as Date | null],
    endDate: [null as Date | null]
  });

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.loading.set(true);
    const filter = this.filterForm.getRawValue();

    this.auditService.getAuditLogs({
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      entityType: filter.entityType || undefined,
      entityId: filter.entityId ?? undefined,
      userId: filter.userId ?? undefined,
      action: filter.action || undefined,
      startDate: filter.startDate ? this.toDateString(filter.startDate) : undefined,
      endDate: filter.endDate ? this.toDateString(filter.endDate, true) : undefined
    }).subscribe({
      next: (res: PagedResponse<AuditLogListItem>) => {
        this.logs.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPage(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadLogs();
  }

  clearFilters(): void {
    this.filterForm.reset({
      entityType: '',
      entityId: null,
      userId: null,
      action: '',
      startDate: null,
      endDate: null
    });
    this.pageNumber = 1;
    this.loadLogs();
  }

  viewDetail(log: AuditLogListItem): void {
    this.auditService.getAuditLog(log.id).subscribe(detail => {
      this.dialog.open(AuditLogDetailDialogComponent, {
        data: detail,
        width: '640px'
      });
    });
  }

  private toDateString(date: Date, endOfDay = false): string {
    const d = new Date(date);
    if (endOfDay) {
      d.setHours(23, 59, 59, 999);
    } else {
      d.setHours(0, 0, 0, 0);
    }
    return d.toISOString();
  }
}
