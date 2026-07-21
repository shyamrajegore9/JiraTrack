export interface AuditFilter {
  entityType?: string;
  entityId?: number;
  userId?: number;
  action?: string;
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface AuditLogListItem {
  id: number;
  entityType: string;
  entityId: number;
  action: string;
  userId?: number;
  userName?: string;
  ipAddress?: string;
  timestamp: string;
}

export interface AuditLogDetail extends AuditLogListItem {
  oldValues?: string;
  newValues?: string;
}

export const AUDIT_ENTITY_TYPES = [
  'User',
  'Role',
  'Project',
  'ProjectMember',
  'TaskItem',
  'Bug',
  'Sprint',
  'Comment',
  'Label',
  'ChecklistItem',
  'TimeLog'
] as const;

export const AUDIT_ACTIONS = ['Create', 'Update', 'Delete'] as const;
