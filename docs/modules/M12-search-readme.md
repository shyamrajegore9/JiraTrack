# M12 — Global Search

Cross-project search for projects, tasks, and bugs with role-based access.

## API Endpoints (`/api/v1/search`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/search` | Global search (all types) |
| GET | `/search/projects` | Projects only |
| GET | `/search/tasks` | Tasks only |
| GET | `/search/bugs` | Bugs only |

**Query params:** `q` (min 2 chars), `pageNumber`, `pageSize`

Results respect project membership (Admin sees all non-archived projects).

## Frontend

- **Toolbar search bar** — Enter navigates to `/app/search?q=...`
- **Search page** — Type filter, pagination, highlight matches, type badges
- **Recent searches** — Stored in `localStorage` (client-side)

## Optional SQL

`database/scripts/M12_search.sql` — Full-text catalog, indexes, and `sp_GlobalSearch` stored procedure.

The API uses EF `Contains` queries by default (no FTS required).

## Test Search

```http
GET http://localhost:5005/api/v1/search?q=login&pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

## Relevance Ranking (Global)

1. Exact key match
2. Key prefix match
3. Title contains
4. Description contains
