import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchFilter, SearchResponse, SearchScope } from '../models/search.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class SearchService {
  constructor(private api: ApiService) {}

  search(filter: SearchFilter, scope: SearchScope = 'all'): Observable<SearchResponse> {
    const params = {
      q: filter.q,
      pageNumber: filter.pageNumber ?? 1,
      pageSize: filter.pageSize ?? 20
    };

    const path = scope === 'all' ? '/search'
      : scope === 'projects' ? '/search/projects'
      : scope === 'tasks' ? '/search/tasks'
      : '/search/bugs';

    return this.api.get<SearchResponse>(path, params);
  }
}
