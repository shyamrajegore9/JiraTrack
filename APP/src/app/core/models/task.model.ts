export const TASK_STATUSES = ['Todo', 'InProgress', 'CodeReview', 'Testing', 'Done'] as const;
export const TASK_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;

export type TaskStatus = (typeof TASK_STATUSES)[number];
export type TaskPriority = (typeof TASK_PRIORITIES)[number];

export interface TaskListItem {
  id: number;
  taskKey: string;
  title: string;
  status: string;
  priority: string;
  assigneeName?: string;
  assigneeId?: number;
  reporterName: string;
  storyPoints?: number;
  dueDate?: string;
  labels: string[];
  isSubtask: boolean;
  createdDate: string;
}

export interface TaskDetail extends TaskListItem {
  description?: string;
  acceptanceCriteria?: string;
  estimatedHours?: number;
  actualHours: number;
  startDate?: string;
  parentTaskId?: number;
  parentTaskKey?: string;
  checklist: ChecklistItem[];
  subtasks: TaskListItem[];
  labelDetails: LabelDto[];
  timeLogs: TimeLog[];
}

export interface TaskFilter {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: string;
  priority?: string;
  assigneeId?: number;
  labelId?: number;
  parentOnly?: boolean;
  sortBy?: string;
  sortDirection?: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  acceptanceCriteria?: string;
  status?: string;
  priority?: string;
  assigneeId?: number;
  storyPoints?: number;
  estimatedHours?: number;
  startDate?: string;
  dueDate?: string;
  parentTaskId?: number;
  labelIds?: number[];
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  acceptanceCriteria?: string;
  priority: string;
  assigneeId?: number;
  storyPoints?: number;
  estimatedHours?: number;
  actualHours: number;
  startDate?: string;
  dueDate?: string;
}

export interface UpdateTaskStatusRequest {
  status: string;
}

export interface AssignTaskRequest {
  assigneeId?: number;
}

export interface ChecklistItem {
  id: number;
  text: string;
  isCompleted: boolean;
  sortOrder: number;
}

export interface CreateChecklistItemRequest {
  text: string;
}

export interface UpdateChecklistItemRequest {
  text: string;
  isCompleted: boolean;
}

export interface LabelDto {
  id: number;
  name: string;
  color: string;
}

export interface CreateLabelRequest {
  name: string;
  color: string;
}

export interface CreateTimeLogRequest {
  hours: number;
  workDate: string;
  description?: string;
}

export interface TimeLog {
  id: number;
  userId: number;
  userName: string;
  hours: number;
  workDate: string;
  description?: string;
}

export const STATUS_LABELS: Record<string, string> = {
  Todo: 'To Do',
  InProgress: 'In Progress',
  CodeReview: 'Code Review',
  Testing: 'Testing',
  Done: 'Done'
};

export const PRIORITY_COLORS: Record<string, string> = {
  Low: '#81c784',
  Medium: '#ffb74d',
  High: '#e57373',
  Critical: '#c62828'
};
