# M09 — Notifications

Real-time in-app notifications via SignalR for task/bug events and @mentions.

## API Endpoints (`/api/v1/notifications`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/notifications` | List notifications (paged) |
| GET | `/notifications/unread-count` | Unread count |
| PATCH | `/notifications/{id}/read` | Mark one as read |
| PATCH | `/notifications/read-all` | Mark all as read |

**Query params:** `pageNumber`, `pageSize`, `unreadOnly`

**SignalR Hub:** `/hubs/notifications` — Event: `NotificationReceived`

## Notification Types

| Type | Trigger |
|------|---------|
| `TaskAssigned` | Task created/updated/assigned with assignee |
| `TaskUpdated` | Task fields updated (assignee unchanged) |
| `TaskStatusChanged` | Task status changed |
| `BugAssigned` | Developer or tester assigned |
| `BugStatusChanged` | Bug status changed |
| `CommentAdded` | New comment on task/bug (assignee or dev/tester) |
| `Mention` | `@username` in comment text |

## Frontend

- Notification bell in main toolbar with unread badge
- Dropdown panel with mark-all-read and click-to-navigate
- Toast snackbar on real-time delivery

## Database Migration

```powershell
cd API
dotnet ef database update
```

Migration: `M09_Notifications` — table `Notifications`

## Test Unread Count

```http
GET http://localhost:5005/api/v1/notifications/unread-count
Authorization: Bearer {token}
```

## Deferred

- Email/push notifications
- Per-user notification preferences
