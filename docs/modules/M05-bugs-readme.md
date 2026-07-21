# M05 — Bug Management

## API Endpoints (`/api/v1/projects/{projectId}`)

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/bugs` | Project member / Admin |
| GET | `/bugs/{id}` | Project member / Admin |
| POST | `/bugs` | Admin, PM, Developer, QA (project member) |
| PUT | `/bugs/{id}` | Admin, PM, Developer, QA (project member) |
| DELETE | `/bugs/{id}` | Admin, ProjectManager (project member) |
| PATCH | `/bugs/{id}/status` | Admin, PM, Developer, QA (project member) |
| PATCH | `/bugs/{id}/assign-developer` | Admin, ProjectManager (project member) |
| PATCH | `/bugs/{id}/assign-tester` | Admin, PM, QA (project member) |

## Bug Status Workflow

`Open` → `InProgress` → `Fixed` → `Retest` → `Closed` / `Reopened`

## Frontend Routes

- `/app/projects/:id/bugs` — Bug list with filters
- `/app/projects/:id/bugs/new` — Report bug
- `/app/projects/:id/bugs/:bugId` — Bug detail
- `/app/projects/:id/bugs/:bugId/edit` — Edit bug

## Database Migration

```powershell
cd API
dotnet ef database update
```

## Test Create Bug

```http
POST http://localhost:5005/api/v1/projects/1/bugs
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Login button not responding",
  "description": "Clicking login does nothing on mobile Safari",
  "severity": "High",
  "priority": "Critical",
  "environment": "Staging",
  "browser": "Safari 17",
  "operatingSystem": "iOS 17",
  "stepsToReproduce": "1. Open app\n2. Enter credentials\n3. Tap Login",
  "expectedResult": "User is logged in",
  "actualResult": "Nothing happens"
}
```

## Bug Key Format

Auto-generated as `{PROJECTKEY}-BUG-{n}` (e.g., `DEMO-BUG-1`).

## Entity Fields

- **Severity / Priority** — Low, Medium, High, Critical
- **Environment, Browser, OS** — Reproduction context
- **Steps to Reproduce, Expected/Actual Result** — QA documentation
- **Developer / Tester** — Assignment fields
