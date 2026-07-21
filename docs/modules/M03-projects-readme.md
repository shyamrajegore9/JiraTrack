# M03 — Project Management

## API Endpoints (`/api/v1/projects`)

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/projects` | All (members see own projects; Admin sees all) |
| GET | `/projects/{id}` | Project member / Admin |
| POST | `/projects` | Admin, ProjectManager |
| PUT | `/projects/{id}` | Admin, ProjectManager (member) |
| DELETE | `/projects/{id}` | Admin, ProjectManager (member) |
| PATCH | `/projects/{id}/archive` | Admin, ProjectManager (member) |
| PATCH | `/projects/{id}/unarchive` | Admin, ProjectManager (member) |
| GET | `/projects/{id}/dashboard` | Project member / Admin |
| GET | `/projects/{id}/members` | Project member / Admin |
| POST | `/projects/{id}/members` | Admin, ProjectManager (member) |
| DELETE | `/projects/{id}/members/{userId}` | Admin, ProjectManager (member) |
| PUT | `/projects/{id}/settings` | Admin, ProjectManager (member) |

## Frontend Routes

- `/app/projects` — Project list
- `/app/projects/new` — Create project
- `/app/projects/:id/dashboard` — Project overview
- `/app/projects/:id/members` — Team management
- `/app/projects/:id/settings` — Project settings
- `/app/projects/:id/edit` — Edit project

## Database Migration

```powershell
cd API
dotnet ef database update
```

## Test Create Project

```http
POST http://localhost:5005/api/v1/projects
Authorization: Bearer {token}
Content-Type: application/json

{
  "key": "DEMO",
  "name": "Demo Project",
  "description": "Sample project for testing",
  "leadUserId": 1
}
```
