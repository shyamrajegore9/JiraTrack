export interface ProjectListItem {
  id: number;
  key: string;
  name: string;
  description?: string;
  leadName: string;
  leadUserId: number;
  isArchived: boolean;
  memberCount: number;
  taskCount: number;
  bugCount: number;
  createdDate: string;
}

export interface ProjectDetail extends ProjectListItem {
  leadEmail?: string;
}

export interface CreateProjectRequest {
  key: string;
  name: string;
  description?: string;
  leadUserId: number;
}

export interface UpdateProjectRequest {
  name: string;
  description?: string;
  leadUserId: number;
}

export interface ProjectFilter {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  isArchived?: boolean;
  sortBy?: string;
  sortDirection?: string;
}

export interface ProjectDashboard {
  projectId: number;
  key: string;
  name: string;
  totalTasks: number;
  openTasks: number;
  totalBugs: number;
  openBugs: number;
  memberCount: number;
  isArchived: boolean;
  hasActiveSprint: boolean;
  activeSprintName?: string;
}

export interface ProjectMember {
  id: number;
  userId: number;
  fullName: string;
  email: string;
  projectRole: string;
  joinedDate: string;
}

export interface AddProjectMemberRequest {
  userId: number;
  projectRole: string;
}

export interface UpdateProjectSettingsRequest {
  name: string;
  description?: string;
}

export const PROJECT_ROLES = ['ProjectManager', 'Developer', 'QA', 'Viewer'] as const;
