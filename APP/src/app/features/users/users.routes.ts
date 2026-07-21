import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/auth.guard';
import { ROLES } from '../../core/constants/roles.constants';

export const USER_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./user-list/user-list.component').then(m => m.UserListComponent),
    canActivate: [roleGuard],
    data: { roles: [ROLES.Admin] }
  },
  {
    path: 'new',
    loadComponent: () => import('./user-form/user-form.component').then(m => m.UserFormComponent),
    canActivate: [roleGuard],
    data: { roles: [ROLES.Admin] }
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./user-form/user-form.component').then(m => m.UserFormComponent),
    canActivate: [roleGuard],
    data: { roles: [ROLES.Admin] }
  }
];
