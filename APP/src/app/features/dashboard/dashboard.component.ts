import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { forkJoin } from 'rxjs';
import { DashboardService } from '../../core/services/dashboard.service';
import { TokenService } from '../../core/services/token.service';
import {
  ActivityItem,
  BugSummary,
  ChartSlice,
  DashboardSummary,
  MyTaskWidget
} from '../../core/models/dashboard.model';
import { ROLES } from '../../core/constants/roles.constants';

@Component({
  selector: 'app-dashboard',
  imports: [
    DatePipe,
    RouterLink,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatChipsModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private tokenService = inject(TokenService);

  readonly user = this.tokenService.currentUser;
  readonly isAdmin = this.tokenService.hasRole(ROLES.Admin);

  summary = signal<DashboardSummary | null>(null);
  myTasks = signal<MyTaskWidget[]>([]);
  activity = signal<ActivityItem[]>([]);
  bugSummary = signal<BugSummary | null>(null);
  loading = signal(true);

  ngOnInit(): void {
    forkJoin({
      summary: this.dashboardService.getDashboard(),
      myTasks: this.dashboardService.getMyTasks(8),
      activity: this.dashboardService.getRecentActivity(12),
      bugSummary: this.dashboardService.getBugSummary()
    }).subscribe({
      next: data => {
        this.summary.set(data.summary);
        this.myTasks.set(data.myTasks);
        this.activity.set(data.activity);
        this.bugSummary.set(data.bugSummary);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  chartMax(slices: ChartSlice[]): number {
    return Math.max(...slices.map(s => s.value), 1);
  }

  barWidth(slice: ChartSlice, slices: ChartSlice[]): number {
    return (slice.value / this.chartMax(slices)) * 100;
  }

  priorityClass(priority: string): string {
    return priority.toLowerCase();
  }

  taskRoute(task: MyTaskWidget): string[] {
    return ['/app/projects', String(task.projectId), 'tasks', String(task.id)];
  }

  activityRoute(item: ActivityItem): string[] | null {
    if (!item.projectId || !item.entityId || !item.entityType) return null;
    if (item.entityType === 'Task') {
      return ['/app/projects', String(item.projectId), 'tasks', String(item.entityId)];
    }
    if (item.entityType === 'Bug') {
      return ['/app/projects', String(item.projectId), 'bugs', String(item.entityId)];
    }
    return null;
  }
}
