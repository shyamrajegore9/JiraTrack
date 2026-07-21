export type NotificationType =
  | 'TaskAssigned'
  | 'TaskUpdated'
  | 'TaskStatusChanged'
  | 'BugAssigned'
  | 'BugStatusChanged'
  | 'CommentAdded'
  | 'Mention';

export interface Notification {
  id: number;
  type: NotificationType;
  title: string;
  message: string;
  entityType?: string;
  entityId?: number;
  projectId?: number;
  isRead: boolean;
  createdDate: string;
}

export interface NotificationFilter {
  pageNumber?: number;
  pageSize?: number;
  unreadOnly?: boolean;
}

export interface UnreadCount {
  count: number;
}
