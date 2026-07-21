import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';

export const authGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (tokenService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login']);
};

export const guestGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (!tokenService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/app/dashboard']);
};

export const roleGuard: CanActivateFn = (route) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);
  const roles = route.data['roles'] as string[] | undefined;

  if (!tokenService.isAuthenticated()) {
    return router.createUrlTree(['/auth/login']);
  }

  if (!roles?.length || tokenService.hasAnyRole(roles)) {
    return true;
  }

  return router.createUrlTree(['/app/dashboard']);
};
