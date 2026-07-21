export const BUG_STATUSES = ['Open', 'InProgress', 'Fixed', 'Retest', 'Closed', 'Reopened'] as const;
export const BUG_SEVERITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export const BUG_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;

export type BugStatus = (typeof BUG_STATUSES)[number];
export type BugSeverity = (typeof BUG_SEVERITIES)[number];
export type BugPriority = (typeof BUG_PRIORITIES)[number];

export interface BugListItem {
  id: number;
  bugKey: string;
  title: string;
  status: string;
  severity: string;
  priority: string;
  developerName?: string;
  developerId?: number;
  testerName?: string;
  testerId?: number;
  reporterName: string;
  environment?: string;
  createdDate: string;
}

export interface BugDetail extends BugListItem {
  description?: string;
  browser?: string;
  operatingSystem?: string;
  stepsToReproduce?: string;
  expectedResult?: string;
  actualResult?: string;
}

export interface BugFilter {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: string;
  severity?: string;
  priority?: string;
  developerId?: number;
  testerId?: number;
  sortBy?: string;
  sortDirection?: string;
}

export interface CreateBugRequest {
  title: string;
  description?: string;
  severity?: string;
  priority?: string;
  status?: string;
  environment?: string;
  browser?: string;
  operatingSystem?: string;
  stepsToReproduce?: string;
  expectedResult?: string;
  actualResult?: string;
  developerId?: number;
  testerId?: number;
}

export interface UpdateBugRequest {
  title: string;
  description?: string;
  severity: string;
  priority: string;
  environment?: string;
  browser?: string;
  operatingSystem?: string;
  stepsToReproduce?: string;
  expectedResult?: string;
  actualResult?: string;
  developerId?: number;
  testerId?: number;
}

export interface UpdateBugStatusRequest {
  status: string;
}

export interface AssignDeveloperRequest {
  developerId?: number;
}

export interface AssignTesterRequest {
  testerId?: number;
}

export const STATUS_LABELS: Record<string, string> = {
  Open: 'Open',
  InProgress: 'In Progress',
  Fixed: 'Fixed',
  Retest: 'Retest',
  Closed: 'Closed',
  Reopened: 'Reopened'
};

export const SEVERITY_COLORS: Record<string, string> = {
  Low: '#81c784',
  Medium: '#ffb74d',
  High: '#e57373',
  Critical: '#c62828'
};

export const PRIORITY_COLORS: Record<string, string> = {
  Low: '#81c784',
  Medium: '#ffb74d',
  High: '#e57373',
  Critical: '#c62828'
};
