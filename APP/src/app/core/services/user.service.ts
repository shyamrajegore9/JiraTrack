import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResponse } from '../models/api-response.model';
import {
  AssignRolesRequest,
  CreateUserRequest,
  Role,
  UpdateUserRequest,
  UserDetail,
  UserFilter,
  UserListItem
} from '../models/user.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(private api: ApiService) {}

  getUsers(filter: UserFilter): Observable<PagedResponse<UserListItem>> {
    return this.api.get<PagedResponse<UserListItem>>('/users', filter as Record<string, string | number | boolean | undefined>);
  }

  getUser(id: number): Observable<UserDetail> {
    return this.api.get<UserDetail>(`/users/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<UserDetail> {
    return this.api.post<UserDetail>('/users', request);
  }

  updateUser(id: number, request: UpdateUserRequest): Observable<UserDetail> {
    return this.api.put<UserDetail>(`/users/${id}`, request);
  }

  deleteUser(id: number): Observable<void> {
    return this.api.delete<void>(`/users/${id}`);
  }

  activateUser(id: number): Observable<void> {
    return this.api.patch<void>(`/users/${id}/activate`);
  }

  deactivateUser(id: number): Observable<void> {
    return this.api.patch<void>(`/users/${id}/deactivate`);
  }

  assignRoles(id: number, request: AssignRolesRequest): Observable<UserDetail> {
    return this.api.put<UserDetail>(`/users/${id}/roles`, request);
  }

  getRoles(): Observable<Role[]> {
    return this.api.get<Role[]>('/roles');
  }
}
