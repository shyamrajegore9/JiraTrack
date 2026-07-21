# M04 — Task Management

## API Endpoints (`/api/v1/projects/{projectId}`)

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/tasks` | Project member / Admin |
| GET | `/tasks/{id}` | Project member / Admin |
| POST | `/tasks` | Admin, PM, Developer, QA (project member) |
| PUT | `/tasks/{id}` | Admin, PM, Developer, QA (project member) |
| DELETE | `/tasks/{id}` | Admin, ProjectManager (project member) |
| PATCH | `/tasks/{id}/status` | Admin, PM, Developer, QA (project member) |
| PATCH | `/tasks/{id}/assign` | Admin, ProjectManager (project member) |
| GET | `/tasks/{id}/subtasks` | Project member / Admin |
| POST | `/tasks/{id}/subtasks` | Admin, PM, Developer, QA (project member) |
| GET | `/tasks/{id}/checklist` | Project member / Admin |
| POST | `/tasks/{id}/checklist` | Admin, PM, Developer, QA (project member) |
| PUT | `/tasks/{id}/checklist/{itemId}` | Admin, PM, Developer, QA (project member) |
| DELETE | `/tasks/{id}/checklist/{itemId}` | Admin, PM, Developer, QA (project member) |
| PUT | `/tasks/{id}/labels` | Admin, PM, Developer, QA (project member) |
| POST | `/tasks/{id}/time-logs` | Admin, PM, Developer, QA (project member) |
| GET | `/tasks/{id}/time-logs` | Project member / Admin |
| GET | `/labels` | Project member / Admin |
| POST | `/labels` | Admin, PM, Developer, QA (project member) |

## Task Status Workflow

`Todo` → `InProgress` → `CodeReview` → `Testing` → `Done`

## Frontend Routes

- `/app/projects/:id/tasks` — Task list with filters
- `/app/projects/:id/tasks/new` — Create task
- `/app/projects/:id/tasks/:taskId` — Task detail (checklist, subtasks, time logs)
- `/app/projects/:id/tasks/:taskId/edit` — Edit task

## Database Migration

```powershell
cd API
dotnet ef database update
```

## Test Create Task

```http
POST http://localhost:5005/api/v1/projects/1/tasks
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Implement login page",
  "description": "Build Angular login form with validation",
  "status": "Todo",
  "priority": "High",
  "assigneeId": 1,
  "storyPoints": 3
}
```

## Entities

- **TaskItem** — Main work item with key `{PROJECTKEY}-{n}`
- **Label** — Project-scoped tags
- **TaskLabel** — Many-to-many task ↔ label
- **ChecklistItem** — Sub-items on a task
- **TimeLog** — Logged hours per user
