import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/auth.guard';
import { ROLES } from '../../core/constants/roles.constants';

export const PROJECT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./project-list/project-list.component').then(m => m.ProjectListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./project-form/project-form.component').then(m => m.ProjectFormComponent),
    canActivate: [roleGuard],
    data: { roles: [ROLES.Admin, ROLES.ProjectManager] }
  },
  {
    path: ':projectId/edit',
    loadComponent: () => import('./project-form/project-form.component').then(m => m.ProjectFormComponent),
    canActivate: [roleGuard],
    data: { roles: [ROLES.Admin, ROLES.ProjectManager] }
  },
  {
    path: ':projectId',
    loadComponent: () => import('./project-detail/project-detail.component').then(m => m.ProjectDetailComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./project-dashboard/project-dashboard.component').then(m => m.ProjectDashboardComponent)
      },
      {
        path: 'tasks',
        loadComponent: () => import('./task-list/task-list.component').then(m => m.TaskListComponent)
      },
      {
        path: 'tasks/new',
        loadComponent: () => import('./task-form/task-form.component').then(m => m.TaskFormComponent)
      },
      {
        path: 'tasks/:taskId',
        loadComponent: () => import('./task-detail/task-detail.component').then(m => m.TaskDetailComponent)
      },
      {
        path: 'tasks/:taskId/edit',
        loadComponent: () => import('./task-form/task-form.component').then(m => m.TaskFormComponent)
      },
      {
        path: 'kanban',
        loadComponent: () => import('./kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent)
      },
      {
        path: 'sprints',
        loadComponent: () => import('./sprint-list/sprint-list.component').then(m => m.SprintListComponent)
      },
      {
        path: 'sprints/new',
        loadComponent: () => import('./sprint-form/sprint-form.component').then(m => m.SprintFormComponent)
      },
      {
        path: 'sprints/:sprintId',
        loadComponent: () => import('./sprint-detail/sprint-detail.component').then(m => m.SprintDetailComponent)
      },
      {
        path: 'sprints/:sprintId/edit',
        loadComponent: () => import('./sprint-form/sprint-form.component').then(m => m.SprintFormComponent)
      },
      {
        path: 'bugs',
        loadComponent: () => import('./bug-list/bug-list.component').then(m => m.BugListComponent)
      },
      {
        path: 'bugs/new',
        loadComponent: () => import('./bug-form/bug-form.component').then(m => m.BugFormComponent)
      },
      {
        path: 'bugs/:bugId',
        loadComponent: () => import('./bug-detail/bug-detail.component').then(m => m.BugDetailComponent)
      },
      {
        path: 'bugs/:bugId/edit',
        loadComponent: () => import('./bug-form/bug-form.component').then(m => m.BugFormComponent)
      },
      {
        path: 'members',
        loadComponent: () => import('./project-members/project-members.component').then(m => m.ProjectMembersComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./project-settings/project-settings.component').then(m => m.ProjectSettingsComponent)
      }
    ]
  }
];
