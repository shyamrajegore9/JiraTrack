import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResponse } from '../models/api-response.model';
import { AuditFilter, AuditLogDetail, AuditLogListItem } from '../models/audit.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AuditService {
  constructor(private api: ApiService) {}

  getAuditLogs(filter: AuditFilter): Observable<PagedResponse<AuditLogListItem>> {
    return this.api.get<PagedResponse<AuditLogListItem>>('/audit', filter as Record<string, string | number | boolean | undefined>);
  }

  getAuditLog(id: number): Observable<AuditLogDetail> {
    return this.api.get<AuditLogDetail>(`/audit/${id}`);
  }
}
