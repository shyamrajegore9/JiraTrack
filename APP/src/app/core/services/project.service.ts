import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResponse } from '../models/api-response.model';
import {
  AddProjectMemberRequest,
  CreateProjectRequest,
  ProjectDashboard,
  ProjectDetail,
  ProjectFilter,
  ProjectListItem,
  ProjectMember,
  UpdateProjectRequest,
  UpdateProjectSettingsRequest
} from '../models/project.model';
import { UserLookupDto } from '../models/user.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  constructor(private api: ApiService) {}

  getProjects(filter: ProjectFilter): Observable<PagedResponse<ProjectListItem>> {
    return this.api.get<PagedResponse<ProjectListItem>>('/projects', filter as Record<string, string | number | boolean | undefined>);
  }

  getProject(id: number): Observable<ProjectDetail> {
    return this.api.get<ProjectDetail>(`/projects/${id}`);
  }

  createProject(request: CreateProjectRequest): Observable<ProjectDetail> {
    return this.api.post<ProjectDetail>('/projects', request);
  }

  updateProject(id: number, request: UpdateProjectRequest): Observable<ProjectDetail> {
    return this.api.put<ProjectDetail>(`/projects/${id}`, request);
  }

  deleteProject(id: number): Observable<void> {
    return this.api.delete<void>(`/projects/${id}`);
  }

  archiveProject(id: number): Observable<void> {
    return this.api.patch<void>(`/projects/${id}/archive`);
  }

  unarchiveProject(id: number): Observable<void> {
    return this.api.patch<void>(`/projects/${id}/unarchive`);
  }

  getDashboard(id: number): Observable<ProjectDashboard> {
    return this.api.get<ProjectDashboard>(`/projects/${id}/dashboard`);
  }

  getMembers(id: number): Observable<ProjectMember[]> {
    return this.api.get<ProjectMember[]>(`/projects/${id}/members`);
  }

  addMember(id: number, request: AddProjectMemberRequest): Observable<ProjectMember> {
    return this.api.post<ProjectMember>(`/projects/${id}/members`, request);
  }

  removeMember(projectId: number, userId: number): Observable<void> {
    return this.api.delete<void>(`/projects/${projectId}/members/${userId}`);
  }

  updateSettings(id: number, request: UpdateProjectSettingsRequest): Observable<ProjectDetail> {
    return this.api.put<ProjectDetail>(`/projects/${id}/settings`, request);
  }

  getUserLookup(searchTerm?: string): Observable<UserLookupDto[]> {
    return this.api.get<UserLookupDto[]>('/users/lookup', { searchTerm });
  }
}
