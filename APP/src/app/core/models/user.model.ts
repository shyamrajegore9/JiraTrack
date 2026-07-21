export interface UserListItem {
  id: number;
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
  isActive: boolean;
  createdDate: string;
  lastLoginDate?: string;
}

export interface UserDetail extends UserListItem {
  phoneNumber?: string;
  profilePictureUrl?: string;
  timeZone: string;
}

export interface CreateUserRequest {
  email: string;
  userName: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  roleIds: number[];
}

export interface UpdateUserRequest {
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  timeZone: string;
}

export interface AssignRolesRequest {
  roleIds: number[];
}

export interface Role {
  id: number;
  name: string;
  description?: string;
}

export interface UserFilter {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  roleId?: number;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
}

export interface UserLookupDto {
  id: number;
  fullName: string;
  email: string;
}
