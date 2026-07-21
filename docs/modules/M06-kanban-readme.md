# M06 — Kanban Board + SignalR

## API Endpoints (`/api/v1/projects/{projectId}/kanban`)

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/kanban` | Project member / Admin |
| PATCH | `/kanban/move` | Admin, PM, Developer, QA (not Viewer) |
| PATCH | `/kanban/reorder` | Admin, PM, Developer, QA (not Viewer) |

### Query Filters (GET)

| Param | Description |
|-------|-------------|
| assigneeId | Filter by assignee |
| labelId | Filter by label |
| sprintId | Filter by sprint (M07) |

## Kanban Columns

Maps to `TaskItemStatus`:

| Column | Status |
|--------|--------|
| To Do | Todo |
| In Progress | InProgress |
| Code Review | CodeReview |
| Testing | Testing |
| Done | Done |

## SignalR Hub

- **URL:** `/hubs/kanban`
- **Join group:** `JoinProject(projectId)` → group `project-{id}`
- **Events:**
  - `CardMoved` — card moved between columns
  - `CardUpdated` — cards reordered within a column
  - `CardAdded` — new parent task created

### Client Connection (Angular)

```typescript
const connection = new HubConnectionBuilder()
  .withUrl('http://localhost:5005/hubs/kanban', {
    accessTokenFactory: () => tokenService.getAccessToken() ?? ''
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
await connection.invoke('JoinProject', projectId);
connection.on('CardMoved', () => reloadBoard());
```

JWT is passed via `accessTokenFactory` (Bearer token). WebSocket fallback uses `?access_token=` query param (configured in `Program.cs`).

## Frontend Route

- `/app/projects/:id/kanban` — Drag-and-drop board (Angular CDK)

## Move Card Request

```http
PATCH http://localhost:5005/api/v1/projects/1/kanban/move
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": 5,
  "toStatus": "InProgress",
  "newSortOrder": 0
}
```

## Reorder Request

```http
PATCH http://localhost:5005/api/v1/projects/1/kanban/reorder
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Todo",
  "taskIds": [3, 1, 5, 2]
}
```

## Notes

- Only parent tasks appear on the board (subtasks excluded)
- Viewers can view the board but cannot drag cards
- Sprint filter UI reserved for M07 Sprints module
