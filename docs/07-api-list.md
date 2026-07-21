# API List — JiraTrack PM

**Base URL:** `/api/v1`  
**Auth:** Bearer JWT (except public auth endpoints)  
**Response Format:** `ApiResponse<T>`

```json
{
  "success": true,
  "message": "Operation completed",
  "data": { },
  "errors": [],
  "correlationId": "guid"
}
```

**Pagination Query Params:** `pageNumber`, `pageSize`, `sortBy`, `sortDirection`, `searchTerm`, `filters`

---

## 1. Authentication — `/auth`

| Method | Endpoint | Description | Auth | Roles |
|--------|----------|-------------|------|-------|
| POST | `/auth/login` | Login with credentials | Public | — |
| POST | `/auth/logout` | Revoke refresh token | Required | All |
| POST | `/auth/refresh` | Refresh access token | Public | — |
| POST | `/auth/forgot-password` | Send reset email | Public | — |
| POST | `/auth/reset-password` | Reset with token | Public | — |
| POST | `/auth/change-password` | Change password | Required | All |
| GET | `/auth/profile` | Get current user profile | Required | All |
| PUT | `/auth/profile` | Update profile | Required | All |

### Request/Response Examples

**POST /auth/login**
```json
// Request
{ "email": "user@example.com", "password": "Pass@1234" }

// Response
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "abc...",
    "expiresIn": 900,
    "user": { "id": 1, "email": "...", "roles": ["Developer"] }
  }
}
```

---

## 2. Users — `/users`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/users` | List users (paged, search, filter) | Admin |
| GET | `/users/{id}` | Get user by ID | Admin |
| POST | `/users` | Create user | Admin |
| PUT | `/users/{id}` | Update user | Admin |
| DELETE | `/users/{id}` | Soft delete user | Admin |
| PATCH | `/users/{id}/activate` | Activate user | Admin |
| PATCH | `/users/{id}/deactivate` | Deactivate user | Admin |
| PUT | `/users/{id}/roles` | Assign roles | Admin |
| GET | `/users/lookup` | User dropdown lookup | All |

---

## 3. Roles — `/roles`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/roles` | List all roles | Admin |
| GET | `/roles/{id}` | Get role by ID | Admin |

---

## 4. Projects — `/projects`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/projects` | List projects (paged) | All |
| GET | `/projects/{id}` | Get project details | Member+ |
| POST | `/projects` | Create project | Admin, PM |
| PUT | `/projects/{id}` | Update project | Admin, PM |
| DELETE | `/projects/{id}` | Soft delete | Admin, PM |
| PATCH | `/projects/{id}/archive` | Archive project | Admin, PM |
| PATCH | `/projects/{id}/unarchive` | Unarchive project | Admin, PM |
| GET | `/projects/{id}/dashboard` | Project dashboard stats | Member+ |
| GET | `/projects/{id}/members` | List members | Member+ |
| POST | `/projects/{id}/members` | Add member | Admin, PM |
| DELETE | `/projects/{id}/members/{userId}` | Remove member | Admin, PM |
| PUT | `/projects/{id}/settings` | Update settings | Admin, PM |

---

## 5. Tasks — `/projects/{projectId}/tasks`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/projects/{projectId}/tasks` | List tasks (paged, filter) | Member+ |
| GET | `/projects/{projectId}/tasks/{id}` | Get task detail | Member+ |
| POST | `/projects/{projectId}/tasks` | Create task | Member+ (not Viewer) |
| PUT | `/projects/{projectId}/tasks/{id}` | Update task | Member+ (not Viewer) |
| DELETE | `/projects/{projectId}/tasks/{id}` | Soft delete | Admin, PM |
| PATCH | `/projects/{projectId}/tasks/{id}/status` | Update status | Member+ (not Viewer) |
| PATCH | `/projects/{projectId}/tasks/{id}/assign` | Assign user | Admin, PM |
| GET | `/projects/{projectId}/tasks/{id}/subtasks` | List subtasks | Member+ |
| POST | `/projects/{projectId}/tasks/{id}/subtasks` | Create subtask | Member+ |
| GET | `/projects/{projectId}/tasks/{id}/checklist` | Get checklist | Member+ |
| POST | `/projects/{projectId}/tasks/{id}/checklist` | Add checklist item | Member+ |
| PUT | `/projects/{projectId}/tasks/{id}/checklist/{itemId}` | Update item | Member+ |
| DELETE | `/projects/{projectId}/tasks/{id}/checklist/{itemId}` | Delete item | Member+ |
| PUT | `/projects/{projectId}/tasks/{id}/labels` | Set labels | Member+ |
| POST | `/projects/{projectId}/tasks/{id}/time-logs` | Log time | Member+ |
| GET | `/projects/{projectId}/tasks/{id}/time-logs` | Get time logs | Member+ |

---

## 6. Bugs — `/projects/{projectId}/bugs`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/projects/{projectId}/bugs` | List bugs (paged, filter) | Member+ |
| GET | `/projects/{projectId}/bugs/{id}` | Get bug detail | Member+ |
| POST | `/projects/{projectId}/bugs` | Create bug | Member+ (not Viewer) |
| PUT | `/projects/{projectId}/bugs/{id}` | Update bug | Member+ (not Viewer) |
| DELETE | `/projects/{projectId}/bugs/{id}` | Soft delete | Admin, PM |
| PATCH | `/projects/{projectId}/bugs/{id}/status` | Update status | Member+ (not Viewer) |
| PATCH | `/projects/{projectId}/bugs/{id}/assign-developer` | Assign developer | Admin, PM |
| PATCH | `/projects/{projectId}/bugs/{id}/assign-tester` | Assign tester | Admin, PM, QA |

---

## 7. Kanban — `/projects/{projectId}/kanban`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/projects/{projectId}/kanban` | Get board columns with cards | Member+ |
| PATCH | `/projects/{projectId}/kanban/move` | Move card to column | Member+ (not Viewer) |
| PATCH | `/projects/{projectId}/kanban/reorder` | Reorder cards in column | Member+ (not Viewer) |

**SignalR Hub:** `/hubs/kanban` — Events: `CardMoved`, `CardUpdated`, `CardAdded`

---

## 8. Sprints — `/projects/{projectId}/sprints`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/projects/{projectId}/sprints` | List sprints | Member+ |
| GET | `/projects/{projectId}/sprints/{id}` | Get sprint detail | Member+ |
| POST | `/projects/{projectId}/sprints` | Create sprint | Admin, PM |
| PUT | `/projects/{projectId}/sprints/{id}` | Update sprint | Admin, PM |
| DELETE | `/projects/{projectId}/sprints/{id}` | Soft delete | Admin, PM |
| POST | `/projects/{projectId}/sprints/{id}/start` | Start sprint | Admin, PM |
| POST | `/projects/{projectId}/sprints/{id}/close` | Close sprint | Admin, PM |
| GET | `/projects/{projectId}/sprints/{id}/backlog` | Sprint backlog | Member+ |
| POST | `/projects/{projectId}/sprints/{id}/backlog` | Add task to sprint | Admin, PM |
| DELETE | `/projects/{projectId}/sprints/{id}/backlog/{taskId}` | Remove from sprint | Admin, PM |
| GET | `/projects/{projectId}/sprints/{id}/velocity` | Sprint velocity | Member+ |
| GET | `/projects/{projectId}/sprints/{id}/burndown` | Burndown data | Member+ |

---

## 9. Labels — `/projects/{projectId}/labels`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/projects/{projectId}/labels` | List labels | Member+ |
| POST | `/projects/{projectId}/labels` | Create label | Admin, PM |
| PUT | `/projects/{projectId}/labels/{id}` | Update label | Admin, PM |
| DELETE | `/projects/{projectId}/labels/{id}` | Delete label | Admin, PM |

---

## 10. Comments — `/comments`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/comments` | Get comments by entity | Member+ |
| POST | `/comments` | Add comment | Member+ (not Viewer) |
| PUT | `/comments/{id}` | Edit comment | Owner |
| DELETE | `/comments/{id}` | Delete comment | Owner, Admin |
| POST | `/comments/{id}/reactions` | Add emoji reaction | Member+ |
| DELETE | `/comments/{id}/reactions/{emoji}` | Remove reaction | Owner |

**Query:** `?entityType=Task&entityId=123`

---

## 11. Notifications — `/notifications`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/notifications` | List notifications (paged) | All |
| GET | `/notifications/unread-count` | Unread count | All |
| PATCH | `/notifications/{id}/read` | Mark as read | All |
| PATCH | `/notifications/read-all` | Mark all read | All |

**SignalR Hub:** `/hubs/notifications` — Events: `NotificationReceived`

---

## 12. Dashboard — `/dashboard`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/dashboard` | Global dashboard data | All |
| GET | `/dashboard/my-tasks` | Assigned tasks | All |
| GET | `/dashboard/recent-activity` | Recent activity feed | All |
| GET | `/dashboard/bug-summary` | Open/closed bugs | All |

---

## 13. Reports — `/reports`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/reports/developer` | Developer report | Admin, PM |
| GET | `/reports/bugs` | Bug report | Admin, PM, QA |
| GET | `/reports/sprint/{sprintId}` | Sprint report | Admin, PM |
| GET | `/reports/project/{projectId}` | Project report | Admin, PM |
| GET | `/reports/time-tracking` | Time tracking report | Admin, PM |
| GET | `/reports/developer/export/pdf` | Export PDF | Admin, PM |
| GET | `/reports/developer/export/excel` | Export Excel | Admin, PM |
| GET | `/reports/bugs/export/pdf` | Export PDF | Admin, PM |
| GET | `/reports/bugs/export/excel` | Export Excel | Admin, PM |

**Query Params:** `projectId`, `userId`, `startDate`, `endDate`

---

## 14. Search — `/search`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/search` | Global search | All |
| GET | `/search/tasks` | Task search | All |
| GET | `/search/bugs` | Bug search | All |
| GET | `/search/projects` | Project search | All |

**Query:** `?q=keyword&pageNumber=1&pageSize=20`

---

## 15. Audit — `/audit`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/audit` | List audit logs (paged, filter) | Admin |
| GET | `/audit/{id}` | Get audit log detail | Admin |

**Filters:** `entityType`, `entityId`, `userId`, `action`, `startDate`, `endDate`

---

## 16. Files — `/files`

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| POST | `/files/upload` | Upload file | All |
| GET | `/files/{id}` | Download file | Authorized |
| DELETE | `/files/{id}` | Delete file | Owner, Admin |
| POST | `/files/profile-picture` | Upload profile picture | All |

**Upload:** multipart/form-data — `file`, `entityType`, `entityId`

---

## 17. Health — `/health`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/health` | Health check | Public |
| GET | `/health/ready` | Readiness (DB check) | Public |

---

## HTTP Status Codes

| Code | Usage |
|------|-------|
| 200 | Success |
| 201 | Created |
| 400 | Validation error |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not found |
| 409 | Conflict (duplicate key, active sprint) |
| 500 | Internal error |

---

## API Versioning

- Current: `v1` via URL segment `/api/v1/...`
- Header alternative: `X-Api-Version: 1.0`
- Swagger: `/swagger/v1/swagger.json`

---

## Total Endpoint Count

| Module | Endpoints |
|--------|-----------|
| Authentication | 8 |
| Users | 9 |
| Roles | 2 |
| Projects | 12 |
| Tasks | 16 |
| Bugs | 8 |
| Kanban | 3 |
| Sprints | 12 |
| Labels | 4 |
| Comments | 6 |
| Notifications | 4 |
| Dashboard | 4 |
| Reports | 9 |
| Search | 4 |
| Audit | 2 |
| Files | 4 |
| Health | 2 |
| **Total** | **109** |
