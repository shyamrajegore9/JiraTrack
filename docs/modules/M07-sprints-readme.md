# M07 — Sprint Management

## API Endpoints (`/api/v1/projects/{projectId}/sprints`)

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/sprints` | Project member / Admin |
| GET | `/sprints/{id}` | Project member / Admin |
| POST | `/sprints` | Admin, ProjectManager |
| PUT | `/sprints/{id}` | Admin, ProjectManager (Planning only) |
| DELETE | `/sprints/{id}` | Admin, ProjectManager |
| POST | `/sprints/{id}/start` | Admin, ProjectManager |
| POST | `/sprints/{id}/close` | Admin, ProjectManager |
| GET | `/sprints/{id}/backlog` | Project member / Admin |
| POST | `/sprints/{id}/backlog` | Admin, ProjectManager (Planning only) |
| DELETE | `/sprints/{id}/backlog/{taskId}` | Admin, ProjectManager (Planning only) |
| GET | `/sprints/{id}/velocity` | Project member / Admin |
| GET | `/sprints/{id}/burndown` | Project member / Admin |

## Sprint Lifecycle

`Planning` → `Active` → `Closed`

- Only **one active sprint** per project at a time
- Backlog add/remove locked once sprint is **Active**
- Closing a sprint returns incomplete tasks to the product backlog (`SprintId = null`)

## Frontend Routes

- `/app/projects/:id/sprints` — Sprint list
- `/app/projects/:id/sprints/new` — Create sprint
- `/app/projects/:id/sprints/:sprintId` — Detail, backlog, burndown, velocity
- `/app/projects/:id/sprints/:sprintId/edit` — Edit (Planning only)

## Database Migration

```powershell
cd API
dotnet ef database update
```

## Test Create Sprint

```http
POST http://localhost:5005/api/v1/projects/1/sprints
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Sprint 1",
  "goal": "User authentication module",
  "startDate": "2026-07-21T00:00:00Z",
  "endDate": "2026-08-04T00:00:00Z"
}
```

## Burndown & Velocity

- **Velocity** — completed vs total story points and task count
- **Burndown** — ideal linear burn vs actual remaining points per day
