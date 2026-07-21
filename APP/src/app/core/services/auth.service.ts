import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {
  ChangePasswordRequest,
  ForgotPasswordRequest,
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
  ResetPasswordRequest,
  UpdateProfileRequest,
  UserProfile
} from '../models/auth.model';
import { ApiService } from './api.service';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private api: ApiService,
    private tokenService: TokenService
  ) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse>('/auth/login', request).pipe(
      tap(res => this.tokenService.setTokens(res.accessToken, res.refreshToken, res.user))
    );
  }

  refresh(refreshToken: string): Observable<LoginResponse> {
    const request: RefreshTokenRequest = { refreshToken };
    return this.api.post<LoginResponse>('/auth/refresh', request).pipe(
      tap(res => this.tokenService.setTokens(res.accessToken, res.refreshToken, res.user))
    );
  }

  logout(): Observable<void> {
    const refreshToken = this.tokenService.getRefreshToken();
    return this.api.post<void>('/auth/logout', { refreshToken }).pipe(
      tap(() => this.tokenService.clear())
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<{ message: string; resetToken?: string }> {
    return this.api.post<{ message: string; resetToken?: string }>('/auth/forgot-password', request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.api.post<void>('/auth/reset-password', request);
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.api.post<void>('/auth/change-password', request);
  }

  getProfile(): Observable<UserProfile> {
    return this.api.get<UserProfile>('/auth/profile').pipe(
      tap(user => this.tokenService.updateUser(user))
    );
  }

  updateProfile(request: UpdateProfileRequest): Observable<UserProfile> {
    return this.api.put<UserProfile>('/auth/profile', request).pipe(
      tap(user => this.tokenService.updateUser(user))
    );
  }
}
