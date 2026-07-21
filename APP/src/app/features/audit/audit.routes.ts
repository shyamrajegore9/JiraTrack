import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/auth.guard';
import { ROLES } from '../../core/constants/roles.constants';

export const AUDIT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./audit-log-list/audit-log-list.component').then(m => m.AuditLogListComponent),
    canActivate: [roleGuard],
    data: { roles: [ROLES.Admin] }
  }
];
