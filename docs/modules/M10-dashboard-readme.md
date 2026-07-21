# M10 — Dashboard

Role-aware global dashboard with summary cards, widgets, and chart data.

## API Endpoints (`/api/v1/dashboard`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/dashboard` | Summary cards + chart data |
| GET | `/dashboard/my-tasks` | Current user's open assigned tasks |
| GET | `/dashboard/recent-activity` | Recent activity feed |
| GET | `/dashboard/bug-summary` | Open/closed bugs + severity breakdown |

**Query params:** `limit` on my-tasks and recent-activity (max 50)

## Summary Cards

| Card | Source |
|------|--------|
| Open Tasks | Non-done parent tasks in accessible projects |
| Open Bugs | Non-closed bugs |
| Sprint Progress | Average completion % across active sprints |
| Completed This Week | Tasks marked Done in last 7 days |
| Active Projects | Admin only — count of accessible projects |

## Role-Based Access

- **Admin** — stats across all non-archived projects
- **Members** — stats limited to projects they belong to

## Frontend

Route: `/app/dashboard`

- Summary metric cards
- My Assigned Tasks widget (links to task detail)
- Recent Activity feed
- Tasks by Status bar chart
- Bugs by Severity bar chart with open/closed counts

## Stored Procedure

Optional SQL script: `database/scripts/M10_dashboard.sql` — `sp_GetProjectStatistics` for project-level stats.

Dashboard API uses EF aggregation queries (no new tables).

## Test Dashboard

```http
GET http://localhost:5005/api/v1/dashboard
Authorization: Bearer {token}
```
