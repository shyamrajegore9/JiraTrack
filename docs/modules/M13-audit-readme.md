# M13 — Audit Log

Complete audit trail for CRUD operations with admin search and detail view.

## API Endpoints (`/api/v1/audit`)

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/audit` | Admin |
| GET | `/audit/{id}` | Admin |

**Query params:** `entityType`, `entityId`, `userId`, `action`, `startDate`, `endDate`, `pageNumber`, `pageSize`

## Backend

- **AuditLogs** table — entity type, entity ID, action, old/new JSON values, user, IP, timestamp
- **AuditSaveChangesInterceptor** — captures Create/Update/Delete (including soft delete) on `BaseEntity` types
- Excludes sensitive fields (`PasswordHash`, `RowVersion`) and auth tokens from audit payloads

## Frontend

- **Route:** `/app/audit` (Admin only)
- **List** — filters by entity type, entity ID, user ID, action, date range
- **Detail dialog** — formatted old/new JSON values

## Test Audit

```http
# Create or update any entity, then query audit log
GET http://localhost:5005/api/v1/audit?entityType=Project&pageNumber=1&pageSize=20
Authorization: Bearer {admin_token}
```

## Migration

```bash
cd API
dotnet ef database update
```

Optional manual script: `database/scripts/M13_audit.sql`
