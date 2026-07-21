# M11 — Reports

Five report types with filters and PDF/Excel export (developer and bug reports).

## API Endpoints (`/api/v1/reports`)

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/reports/developer` | Admin, PM |
| GET | `/reports/bugs` | Admin, PM, QA |
| GET | `/reports/sprint/{sprintId}?projectId=` | Admin, PM |
| GET | `/reports/project/{projectId}` | Admin, PM |
| GET | `/reports/time-tracking` | Admin, PM |
| GET | `/reports/developer/export/pdf` | Admin, PM |
| GET | `/reports/developer/export/excel` | Admin, PM |
| GET | `/reports/bugs/export/pdf` | Admin, PM |
| GET | `/reports/bugs/export/excel` | Admin, PM |

**Query params:** `projectId`, `userId`, `startDate`, `endDate`

## Report Types

| Report | Contents |
|--------|----------|
| Developer | Tasks completed, hours logged, bugs fixed |
| Bug | By severity, status, environment |
| Sprint | Velocity, burndown, completion rate |
| Project | Task/bug health, status distribution |
| Time Tracking | Hours by user, project, date |

## Frontend Routes

- `/app/reports` — Report hub
- `/app/reports/developer`
- `/app/reports/bugs`
- `/app/reports/sprint` → select sprint → `/app/reports/sprint/:sprintId`
- `/app/reports/project` → select project → `/app/reports/project/:projectId`
- `/app/reports/time-tracking`

## Export Libraries

- **PDF:** QuestPDF (Community license)
- **Excel:** ClosedXML

## Test Developer Report

```http
GET http://localhost:5005/api/v1/reports/developer?userId=2&projectId=1
Authorization: Bearer {token}
```

## Notes

No new database tables — reports use existing EF data.
Optional SQL script: `database/scripts/M11_reports.sql` (placeholder).
