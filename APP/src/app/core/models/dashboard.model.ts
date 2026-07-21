export interface ChartSlice {
  label: string;
  value: number;
}

export interface DashboardSummary {
  openTasks: number;
  openBugs: number;
  sprintProgressPercent: number;
  completedThisWeek: number;
  activeProjects: number;
  tasksByStatus: ChartSlice[];
  bugsBySeverity: ChartSlice[];
}

export interface MyTaskWidget {
  id: number;
  projectId: number;
  taskKey: string;
  title: string;
  status: string;
  priority: string;
  dueDate?: string;
}

export interface ActivityItem {
  type: string;
  message: string;
  projectId?: number;
  entityType?: string;
  entityId?: number;
  createdDate: string;
}

export interface BugSummary {
  open: number;
  closed: number;
  total: number;
  bySeverity: ChartSlice[];
}
