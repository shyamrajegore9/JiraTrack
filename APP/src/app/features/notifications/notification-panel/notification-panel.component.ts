import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NotificationService } from '../../../core/services/notification.service';
import { Notification } from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-panel',
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './notification-panel.component.html',
  styleUrl: './notification-panel.component.scss'
})
export class NotificationPanelComponent implements OnInit {
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  readonly unreadCount = this.notificationService.unreadCount;
  readonly notifications = this.notificationService.notifications;
  readonly loading = this.notificationService.loading;

  ngOnInit(): void {
    this.notificationService.connect();
    this.notificationService.refreshUnreadCount().subscribe();
    this.notificationService.loadNotifications({ pageSize: 15 }).subscribe();
  }

  onMenuOpened(): void {
    this.notificationService.loadNotifications({ pageSize: 15 }).subscribe();
  }

  markAllRead(): void {
    this.notificationService.markAllAsRead().subscribe();
  }

  openNotification(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }

    const route = this.buildRoute(notification);
    if (route) {
      this.router.navigate(route);
    }
  }

  private buildRoute(notification: Notification): string[] | null {
    if (!notification.projectId || !notification.entityId || !notification.entityType) {
      return null;
    }

    if (notification.entityType === 'Task') {
      return ['/app/projects', String(notification.projectId), 'tasks', String(notification.entityId)];
    }

    if (notification.entityType === 'Bug') {
      return ['/app/projects', String(notification.projectId), 'bugs', String(notification.entityId)];
    }

    return null;
  }
}
