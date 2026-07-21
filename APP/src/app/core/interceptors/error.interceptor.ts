import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'An unexpected error occurred';

      if (error.error?.errors?.length) {
        message = error.error.errors[0];
      } else if (error.error?.message) {
        message = error.error.message;
      } else if (error.status === 0) {
        message = 'Unable to connect to server';
      } else if (error.status === 403) {
        message = 'You do not have permission to perform this action';
      }

      if (!req.url.includes('/auth/login')) {
        snackBar.open(message, 'Close', { duration: 5000, panelClass: 'error-snackbar' });
      }

      return throwError(() => error);
    })
  );
};
