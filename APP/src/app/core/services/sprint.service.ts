import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AddTaskToSprintBacklogRequest,
  Burndown,
  CreateSprintRequest,
  SprintBacklogTask,
  SprintDetail,
  SprintListItem,
  SprintVelocity,
  UpdateSprintRequest
} from '../models/sprint.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class SprintService {
  constructor(private api: ApiService) {}

  getSprints(projectId: number): Observable<SprintListItem[]> {
    return this.api.get<SprintListItem[]>(`/projects/${projectId}/sprints`);
  }

  getSprint(projectId: number, sprintId: number): Observable<SprintDetail> {
    return this.api.get<SprintDetail>(`/projects/${projectId}/sprints/${sprintId}`);
  }

  createSprint(projectId: number, request: CreateSprintRequest): Observable<SprintDetail> {
    return this.api.post<SprintDetail>(`/projects/${projectId}/sprints`, request);
  }

  updateSprint(projectId: number, sprintId: number, request: UpdateSprintRequest): Observable<SprintDetail> {
    return this.api.put<SprintDetail>(`/projects/${projectId}/sprints/${sprintId}`, request);
  }

  deleteSprint(projectId: number, sprintId: number): Observable<void> {
    return this.api.delete<void>(`/projects/${projectId}/sprints/${sprintId}`);
  }

  startSprint(projectId: number, sprintId: number): Observable<SprintDetail> {
    return this.api.post<SprintDetail>(`/projects/${projectId}/sprints/${sprintId}/start`, {});
  }

  closeSprint(projectId: number, sprintId: number): Observable<SprintDetail> {
    return this.api.post<SprintDetail>(`/projects/${projectId}/sprints/${sprintId}/close`, {});
  }

  getBacklog(projectId: number, sprintId: number): Observable<SprintBacklogTask[]> {
    return this.api.get<SprintBacklogTask[]>(`/projects/${projectId}/sprints/${sprintId}/backlog`);
  }

  addToBacklog(projectId: number, sprintId: number, request: AddTaskToSprintBacklogRequest): Observable<SprintBacklogTask> {
    return this.api.post<SprintBacklogTask>(`/projects/${projectId}/sprints/${sprintId}/backlog`, request);
  }

  removeFromBacklog(projectId: number, sprintId: number, taskId: number): Observable<void> {
    return this.api.delete<void>(`/projects/${projectId}/sprints/${sprintId}/backlog/${taskId}`);
  }

  getVelocity(projectId: number, sprintId: number): Observable<SprintVelocity> {
    return this.api.get<SprintVelocity>(`/projects/${projectId}/sprints/${sprintId}/velocity`);
  }

  getBurndown(projectId: number, sprintId: number): Observable<Burndown> {
    return this.api.get<Burndown>(`/projects/${projectId}/sprints/${sprintId}/burndown`);
  }
}
