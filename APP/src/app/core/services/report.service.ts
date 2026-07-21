import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  BugReport,
  DeveloperReport,
  ProjectReport,
  ReportFilter,
  SprintReport,
  TimeTrackingReport
} from '../models/report.model';

@Injectable({ providedIn: 'root' })
export class ReportService {
  constructor(private http: HttpClient) {}

  getDeveloperReport(filter: ReportFilter): Observable<DeveloperReport> {
    return this.apiGet<DeveloperReport>('/reports/developer', filter);
  }

  getBugReport(filter: ReportFilter): Observable<BugReport> {
    return this.apiGet<BugReport>('/reports/bugs', filter);
  }

  getSprintReport(projectId: number, sprintId: number): Observable<SprintReport> {
    return this.apiGet<SprintReport>(`/reports/sprint/${sprintId}`, { projectId });
  }

  getProjectReport(projectId: number): Observable<ProjectReport> {
    return this.apiGet<ProjectReport>(`/reports/project/${projectId}`);
  }

  getTimeTrackingReport(filter: ReportFilter): Observable<TimeTrackingReport> {
    return this.apiGet<TimeTrackingReport>('/reports/time-tracking', filter);
  }

  exportDeveloperPdf(filter: ReportFilter): void {
    this.download('/reports/developer/export/pdf', filter, 'developer-report.pdf');
  }

  exportDeveloperExcel(filter: ReportFilter): void {
    this.download('/reports/developer/export/excel', filter, 'developer-report.xlsx');
  }

  exportBugPdf(filter: ReportFilter): void {
    this.download('/reports/bugs/export/pdf', filter, 'bug-report.pdf');
  }

  exportBugExcel(filter: ReportFilter): void {
    this.download('/reports/bugs/export/excel', filter, 'bug-report.xlsx');
  }

  private apiGet<T>(url: string, filter?: ReportFilter | { projectId: number }): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(`${environment.apiUrl}${url}`, { params: this.buildParams(filter) })
      .pipe(map(r => {
        if (!r.success) throw new Error(r.message);
        return r.data;
      }));
  }

  private download(path: string, filter: ReportFilter, filename: string): void {
    this.http
      .get(`${environment.apiUrl}${path}`, { params: this.buildParams(filter), responseType: 'blob' })
      .subscribe(blob => this.saveBlob(blob, filename));
  }

  private buildParams(filter?: ReportFilter | { projectId: number }): HttpParams {
    let params = new HttpParams();
    if (!filter) return params;
    if ('projectId' in filter && filter.projectId) params = params.set('projectId', filter.projectId);
    if ('userId' in filter && filter.userId) params = params.set('userId', filter.userId);
    if ('startDate' in filter && filter.startDate) params = params.set('startDate', filter.startDate);
    if ('endDate' in filter && filter.endDate) params = params.set('endDate', filter.endDate);
    return params;
  }

  private saveBlob(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = filename;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
