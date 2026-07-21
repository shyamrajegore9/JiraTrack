import { ChartSlice } from './dashboard.model';
import { Burndown, SprintVelocity } from './sprint.model';

export interface ReportFilter {
  projectId?: number;
  userId?: number;
  startDate?: string;
  endDate?: string;
}

export interface DeveloperReport {
  userId: number;
  userName: string;
  fullName: string;
  tasksCompleted: number;
  hoursLogged: number;
  bugsFixed: number;
  completedTasks: DeveloperTaskRow[];
  timeLogs: ReportTimeLogRow[];
}

export interface DeveloperTaskRow {
  taskKey: string;
  title: string;
  projectName: string;
  completedDate: string;
}

export interface ReportTimeLogRow {
  taskKey: string;
  projectName: string;
  hours: number;
  workDate: string;
  description?: string;
}

export interface BugReport {
  totalBugs: number;
  bySeverity: ChartSlice[];
  byStatus: ChartSlice[];
  byEnvironment: ChartSlice[];
}

export interface SprintReport {
  sprintId: number;
  projectId: number;
  projectName: string;
  sprintName: string;
  sprintStatus: string;
  velocity: SprintVelocity;
  burndown: Burndown;
}

export interface ProjectReport {
  projectId: number;
  projectKey: string;
  projectName: string;
  totalTasks: number;
  openTasks: number;
  completedTasks: number;
  taskCompletionRate: number;
  totalBugs: number;
  openBugs: number;
  memberCount: number;
  hasActiveSprint: boolean;
  activeSprintName?: string;
  tasksByStatus: ChartSlice[];
  bugsBySeverity: ChartSlice[];
}

export interface TimeTrackingReport {
  totalHours: number;
  rows: TimeTrackingRow[];
}

export interface TimeTrackingRow {
  userId: number;
  userName: string;
  projectId: number;
  projectName: string;
  taskKey: string;
  hours: number;
  workDate: string;
  description?: string;
}
