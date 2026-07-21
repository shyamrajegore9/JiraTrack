import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/auth.guard';
import { ROLES } from '../../core/constants/roles.constants';

const pmRoles = [ROLES.Admin, ROLES.ProjectManager];
const qaRoles = [ROLES.Admin, ROLES.ProjectManager, ROLES.QA];

export const REPORT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./report-dashboard/report-dashboard.component').then(m => m.ReportDashboardComponent),
    canActivate: [roleGuard],
    data: { roles: qaRoles }
  },
  {
    path: 'developer',
    loadComponent: () => import('./developer-report/developer-report.component').then(m => m.DeveloperReportComponent),
    canActivate: [roleGuard],
    data: { roles: pmRoles }
  },
  {
    path: 'bugs',
    loadComponent: () => import('./bug-report/bug-report.component').then(m => m.BugReportComponent),
    canActivate: [roleGuard],
    data: { roles: qaRoles }
  },
  {
    path: 'sprint',
    loadComponent: () => import('./sprint-report/sprint-report-selector.component').then(m => m.SprintReportSelectorComponent),
    canActivate: [roleGuard],
    data: { roles: pmRoles }
  },
  {
    path: 'sprint/:sprintId',
    loadComponent: () => import('./sprint-report/sprint-report.component').then(m => m.SprintReportComponent),
    canActivate: [roleGuard],
    data: { roles: pmRoles }
  },
  {
    path: 'project',
    loadComponent: () => import('./project-report/project-report-selector.component').then(m => m.ProjectReportSelectorComponent),
    canActivate: [roleGuard],
    data: { roles: pmRoles }
  },
  {
    path: 'project/:projectId',
    loadComponent: () => import('./project-report/project-report.component').then(m => m.ProjectReportComponent),
    canActivate: [roleGuard],
    data: { roles: pmRoles }
  },
  {
    path: 'time-tracking',
    loadComponent: () => import('./time-tracking-report/time-tracking-report.component').then(m => m.TimeTrackingReportComponent),
    canActivate: [roleGuard],
    data: { roles: pmRoles }
  }
];
