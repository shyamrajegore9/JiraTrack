import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { AuditLogDetail } from '../../../core/models/audit.model';

@Component({
  selector: 'app-audit-log-detail-dialog',
  imports: [DatePipe, MatDialogModule, MatButtonModule],
  templateUrl: './audit-log-detail-dialog.component.html',
  styleUrl: './audit-log-detail-dialog.component.scss'
})
export class AuditLogDetailDialogComponent {
  readonly log = inject<AuditLogDetail>(MAT_DIALOG_DATA);

  formatJson(value?: string): string {
    if (!value) return '—';
    try {
      return JSON.stringify(JSON.parse(value), null, 2);
    } catch {
      return value;
    }
  }
}
