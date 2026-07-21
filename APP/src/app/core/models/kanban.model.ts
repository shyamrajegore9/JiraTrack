export interface KanbanCard {
  id: number;
  taskKey: string;
  title: string;
  status: string;
  priority: string;
  assigneeName?: string;
  assigneeId?: number;
  labels: string[];
  storyPoints?: number;
  sortOrder: number;
}

export interface KanbanColumn {
  status: string;
  title: string;
  count: number;
  cards: KanbanCard[];
}

export interface KanbanBoard {
  projectId: number;
  columns: KanbanColumn[];
}

export interface KanbanFilter {
  assigneeId?: number;
  sprintId?: number;
  labelId?: number;
}

export interface MoveKanbanCardRequest {
  taskId: number;
  toStatus: string;
  newSortOrder: number;
}

export interface ReorderKanbanCardsRequest {
  status: string;
  taskIds: number[];
}

export const PRIORITY_COLORS: Record<string, string> = {
  Low: '#81c784',
  Medium: '#ffb74d',
  High: '#e57373',
  Critical: '#c62828'
};
