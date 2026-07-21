import { Injectable, OnDestroy, signal } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Observable, Subject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification, NotificationFilter, UnreadCount } from '../models/notification.model';
import { PagedResponse } from '../models/api-response.model';
import { ApiService } from './api.service';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private connection?: HubConnection;
  private readonly notificationReceived$ = new Subject<Notification>();

  readonly unreadCount = signal(0);
  readonly notifications = signal<Notification[]>([]);
  readonly loading = signal(false);

  constructor(
    private api: ApiService,
    private tokenService: TokenService,
    private snackBar: MatSnackBar
  ) {}

  get onNotificationReceived(): Observable<Notification> {
    return this.notificationReceived$.asObservable();
  }

  loadNotifications(filter: NotificationFilter = {}): Observable<PagedResponse<Notification>> {
    this.loading.set(true);
    return this.api.get<PagedResponse<Notification>>('/notifications', {
      pageNumber: filter.pageNumber ?? 1,
      pageSize: filter.pageSize ?? 20,
      unreadOnly: filter.unreadOnly
    }).pipe(
      tap(result => {
        this.notifications.set(result.items);
        this.loading.set(false);
      })
    );
  }

  refreshUnreadCount(): Observable<UnreadCount> {
    return this.api.get<UnreadCount>('/notifications/unread-count').pipe(
      tap(result => this.unreadCount.set(result.count))
    );
  }

  markAsRead(id: number): Observable<void> {
    return this.api.patch<void>(`/notifications/${id}/read`).pipe(
      tap(() => {
        this.notifications.update(items =>
          items.map(n => (n.id === id ? { ...n, isRead: true } : n))
        );
        this.unreadCount.update(c => Math.max(0, c - 1));
      })
    );
  }

  markAllAsRead(): Observable<void> {
    return this.api.patch<void>('/notifications/read-all').pipe(
      tap(() => {
        this.notifications.update(items => items.map(n => ({ ...n, isRead: true })));
        this.unreadCount.set(0);
      })
    );
  }

  connect(): void {
    if (this.connection || !this.tokenService.getAccessToken()) return;

    this.connection = new HubConnectionBuilder()
      .withUrl(`${environment.hubUrl}/notifications`, {
        accessTokenFactory: () => this.tokenService.getAccessToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('NotificationReceived', (notification: Notification) => {
      this.notifications.update(items => [notification, ...items].slice(0, 20));
      this.unreadCount.update(c => c + 1);
      this.notificationReceived$.next(notification);
      this.snackBar.open(notification.title, 'View', { duration: 5000 });
    });

    this.connection.start().catch(() => { /* polling still works via API */ });
  }

  disconnect(): void {
    if (this.connection) {
      this.connection.stop().catch(() => undefined);
      this.connection = undefined;
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}
