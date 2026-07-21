import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResponse } from '../models/api-response.model';
import {
  AssignTaskRequest,
  ChecklistItem,
  CreateChecklistItemRequest,
  CreateLabelRequest,
  CreateTaskRequest,
  CreateTimeLogRequest,
  LabelDto,
  TaskDetail,
  TaskFilter,
  TaskListItem,
  TimeLog,
  UpdateChecklistItemRequest,
  UpdateTaskRequest,
  UpdateTaskStatusRequest
} from '../models/task.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class TaskService {
  constructor(private api: ApiService) {}

  getTasks(projectId: number, filter: TaskFilter): Observable<PagedResponse<TaskListItem>> {
    return this.api.get<PagedResponse<TaskListItem>>(
      `/projects/${projectId}/tasks`,
      filter as Record<string, string | number | boolean | undefined>
    );
  }

  getTask(projectId: number, taskId: number): Observable<TaskDetail> {
    return this.api.get<TaskDetail>(`/projects/${projectId}/tasks/${taskId}`);
  }

  createTask(projectId: number, request: CreateTaskRequest): Observable<TaskDetail> {
    return this.api.post<TaskDetail>(`/projects/${projectId}/tasks`, request);
  }

  updateTask(projectId: number, taskId: number, request: UpdateTaskRequest): Observable<TaskDetail> {
    return this.api.put<TaskDetail>(`/projects/${projectId}/tasks/${taskId}`, request);
  }

  deleteTask(projectId: number, taskId: number): Observable<void> {
    return this.api.delete<void>(`/projects/${projectId}/tasks/${taskId}`);
  }

  updateStatus(projectId: number, taskId: number, request: UpdateTaskStatusRequest): Observable<TaskDetail> {
    return this.api.patch<TaskDetail>(`/projects/${projectId}/tasks/${taskId}/status`, request);
  }

  assignTask(projectId: number, taskId: number, request: AssignTaskRequest): Observable<TaskDetail> {
    return this.api.patch<TaskDetail>(`/projects/${projectId}/tasks/${taskId}/assign`, request);
  }

  getSubtasks(projectId: number, taskId: number): Observable<TaskListItem[]> {
    return this.api.get<TaskListItem[]>(`/projects/${projectId}/tasks/${taskId}/subtasks`);
  }

  createSubtask(projectId: number, taskId: number, request: CreateTaskRequest): Observable<TaskDetail> {
    return this.api.post<TaskDetail>(`/projects/${projectId}/tasks/${taskId}/subtasks`, request);
  }

  getChecklist(projectId: number, taskId: number): Observable<ChecklistItem[]> {
    return this.api.get<ChecklistItem[]>(`/projects/${projectId}/tasks/${taskId}/checklist`);
  }

  addChecklistItem(projectId: number, taskId: number, request: CreateChecklistItemRequest): Observable<ChecklistItem> {
    return this.api.post<ChecklistItem>(`/projects/${projectId}/tasks/${taskId}/checklist`, request);
  }

  updateChecklistItem(
    projectId: number,
    taskId: number,
    itemId: number,
    request: UpdateChecklistItemRequest
  ): Observable<ChecklistItem> {
    return this.api.put<ChecklistItem>(`/projects/${projectId}/tasks/${taskId}/checklist/${itemId}`, request);
  }

  deleteChecklistItem(projectId: number, taskId: number, itemId: number): Observable<void> {
    return this.api.delete<void>(`/projects/${projectId}/tasks/${taskId}/checklist/${itemId}`);
  }

  getLabels(projectId: number): Observable<LabelDto[]> {
    return this.api.get<LabelDto[]>(`/projects/${projectId}/labels`);
  }

  createLabel(projectId: number, request: CreateLabelRequest): Observable<LabelDto> {
    return this.api.post<LabelDto>(`/projects/${projectId}/labels`, request);
  }

  addTimeLog(projectId: number, taskId: number, request: CreateTimeLogRequest): Observable<TimeLog> {
    return this.api.post<TimeLog>(`/projects/${projectId}/tasks/${taskId}/time-logs`, request);
  }

  getTimeLogs(projectId: number, taskId: number): Observable<TimeLog[]> {
    return this.api.get<TimeLog[]>(`/projects/${projectId}/tasks/${taskId}/time-logs`);
  }
}
