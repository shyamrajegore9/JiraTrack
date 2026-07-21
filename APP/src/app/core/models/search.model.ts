export type SearchResultType = 'Project' | 'Task' | 'Bug';

export type SearchScope = 'all' | 'projects' | 'tasks' | 'bugs';

export interface SearchResult {
  type: SearchResultType;
  id: number;
  projectId?: number;
  key: string;
  title: string;
  subtitle?: string;
  status?: string;
  matchedField?: string;
}

export interface SearchResponse {
  items: SearchResult[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface SearchFilter {
  q: string;
  pageNumber?: number;
  pageSize?: number;
}
