import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-report-dashboard',
  imports: [RouterLink, MatCardModule, MatIconModule],
  templateUrl: './report-dashboard.component.html',
  styleUrl: './report-dashboard.component.scss'
})
export class ReportDashboardComponent {
  readonly reports = [
    { title: 'Developer Report', desc: 'Tasks completed, hours logged, bugs fixed', icon: 'person', route: 'developer' },
    { title: 'Bug Report', desc: 'Bugs by severity, status, and environment', icon: 'bug_report', route: 'bugs' },
    { title: 'Sprint Report', desc: 'Velocity, burndown, and completion rate', icon: 'speed', route: 'sprint' },
    { title: 'Project Report', desc: 'Overall project health metrics', icon: 'folder', route: 'project' },
    { title: 'Time Tracking', desc: 'Hours logged by user, project, and date', icon: 'schedule', route: 'time-tracking' }
  ];
}
