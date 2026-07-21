export const SPRINT_STATUSES = ['Planning', 'Active', 'Closed'] as const;

export type SprintStatus = (typeof SPRINT_STATUSES)[number];

export interface SprintListItem {
  id: number;
  name: string;
  goal?: string;
  status: string;
  startDate?: string;
  endDate?: string;
  taskCount: number;
  totalStoryPoints: number;
  completedStoryPoints: number;
  createdDate: string;
}

export interface SprintBacklogTask {
  id: number;
  taskKey: string;
  title: string;
  status: string;
  priority: string;
  assigneeName?: string;
  storyPoints?: number;
}

export interface SprintDetail extends SprintListItem {
  backlog: SprintBacklogTask[];
}

export interface CreateSprintRequest {
  name: string;
  goal?: string;
  startDate?: string;
  endDate?: string;
}

export interface UpdateSprintRequest {
  name: string;
  goal?: string;
  startDate?: string;
  endDate?: string;
}

export interface AddTaskToSprintBacklogRequest {
  taskId: number;
}

export interface SprintVelocity {
  sprintId: number;
  sprintName: string;
  completedStoryPoints: number;
  totalStoryPoints: number;
  totalTasks: number;
  completedTasks: number;
  completionRate: number;
}

export interface BurndownPoint {
  date: string;
  idealRemaining: number;
  actualRemaining: number;
}

export interface Burndown {
  sprintId: number;
  totalStoryPoints: number;
  points: BurndownPoint[];
}

export const STATUS_LABELS: Record<string, string> = {
  Planning: 'Planning',
  Active: 'Active',
  Closed: 'Closed'
};

export const STATUS_COLORS: Record<string, string> = {
  Planning: '#ffb74d',
  Active: '#64b5f6',
  Closed: '#81c784'
};
