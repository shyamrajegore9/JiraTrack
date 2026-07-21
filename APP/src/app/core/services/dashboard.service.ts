import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ActivityItem,
  BugSummary,
  DashboardSummary,
  MyTaskWidget
} from '../models/dashboard.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private api: ApiService) {}

  getDashboard(): Observable<DashboardSummary> {
    return this.api.get<DashboardSummary>('/dashboard');
  }

  getMyTasks(limit = 10): Observable<MyTaskWidget[]> {
    return this.api.get<MyTaskWidget[]>('/dashboard/my-tasks', { limit });
  }

  getRecentActivity(limit = 15): Observable<ActivityItem[]> {
    return this.api.get<ActivityItem[]>('/dashboard/recent-activity', { limit });
  }

  getBugSummary(): Observable<BugSummary> {
    return this.api.get<BugSummary>('/dashboard/bug-summary');
  }
}
