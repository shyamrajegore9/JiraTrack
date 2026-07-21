import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AuthService } from '../../core/services/auth.service';
import { TokenService } from '../../core/services/token.service';
import { ROLES } from '../../core/constants/roles.constants';
import { NotificationPanelComponent } from '../../features/notifications/notification-panel/notification-panel.component';
import { SearchBarComponent } from '../../features/search/search-bar/search-bar.component';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    NotificationPanelComponent,
    SearchBarComponent
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss'
})
export class MainLayoutComponent {
  private tokenService = inject(TokenService);
  private authService = inject(AuthService);

  sidenavOpened = true;
  readonly user = this.tokenService.currentUser;
  readonly navItems = [
    { label: 'Dashboard', icon: 'dashboard', route: '/app/dashboard', roles: [] as string[] },
    { label: 'Projects', icon: 'folder', route: '/app/projects', roles: [] as string[] },
    { label: 'Reports', icon: 'assessment', route: '/app/reports', roles: [ROLES.Admin, ROLES.ProjectManager, ROLES.QA] },
    { label: 'Users', icon: 'people', route: '/app/users', roles: [ROLES.Admin] },
    { label: 'Audit Log', icon: 'history', route: '/app/audit', roles: [ROLES.Admin] }
  ];

  constructor(breakpointObserver: BreakpointObserver) {
    breakpointObserver.observe([Breakpoints.Handset]).subscribe(result => {
      this.sidenavOpened = !result.matches;
    });
  }

  visibleNavItems() {
    return this.navItems.filter(item =>
      !item.roles.length || this.tokenService.hasAnyRole(item.roles)
    );
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => location.assign('/auth/login'),
      error: () => location.assign('/auth/login')
    });
  }
}
