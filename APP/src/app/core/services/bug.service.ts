import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResponse } from '../models/api-response.model';
import {
  AssignDeveloperRequest,
  AssignTesterRequest,
  BugDetail,
  BugFilter,
  BugListItem,
  CreateBugRequest,
  UpdateBugRequest,
  UpdateBugStatusRequest
} from '../models/bug.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class BugService {
  constructor(private api: ApiService) {}

  getBugs(projectId: number, filter: BugFilter): Observable<PagedResponse<BugListItem>> {
    return this.api.get<PagedResponse<BugListItem>>(
      `/projects/${projectId}/bugs`,
      filter as Record<string, string | number | boolean | undefined>
    );
  }

  getBug(projectId: number, bugId: number): Observable<BugDetail> {
    return this.api.get<BugDetail>(`/projects/${projectId}/bugs/${bugId}`);
  }

  createBug(projectId: number, request: CreateBugRequest): Observable<BugDetail> {
    return this.api.post<BugDetail>(`/projects/${projectId}/bugs`, request);
  }

  updateBug(projectId: number, bugId: number, request: UpdateBugRequest): Observable<BugDetail> {
    return this.api.put<BugDetail>(`/projects/${projectId}/bugs/${bugId}`, request);
  }

  deleteBug(projectId: number, bugId: number): Observable<void> {
    return this.api.delete<void>(`/projects/${projectId}/bugs/${bugId}`);
  }

  updateStatus(projectId: number, bugId: number, request: UpdateBugStatusRequest): Observable<BugDetail> {
    return this.api.patch<BugDetail>(`/projects/${projectId}/bugs/${bugId}/status`, request);
  }

  assignDeveloper(projectId: number, bugId: number, request: AssignDeveloperRequest): Observable<BugDetail> {
    return this.api.patch<BugDetail>(`/projects/${projectId}/bugs/${bugId}/assign-developer`, request);
  }

  assignTester(projectId: number, bugId: number, request: AssignTesterRequest): Observable<BugDetail> {
    return this.api.patch<BugDetail>(`/projects/${projectId}/bugs/${bugId}/assign-tester`, request);
  }
}
