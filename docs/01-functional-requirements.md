# Functional Requirements — JiraTrack PM

**Product:** Enterprise Project Management & Bug Tracking System  
**Version:** 1.0  
**Date:** July 21, 2026  
**Status:** Draft — Pending Approval

---

## 1. Purpose

Build an enterprise-grade project management and bug tracking platform comparable to Jira, supporting agile workflows (Kanban, Sprints), role-based access, real-time collaboration, reporting, and audit compliance.

---

## 2. Actors & Roles

| Role | Description |
|------|-------------|
| **Admin** | Full system access; user/role management; global settings |
| **Project Manager** | Create/manage projects, sprints, members; assign work; view reports |
| **Developer** | Work on tasks/bugs; update status; log time; comment |
| **QA** | Create/verify bugs; move items through testing workflow |
| **Viewer** | Read-only access to assigned projects |

---

## 3. Module Requirements

### 3.1 Authentication (FR-AUTH)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-AUTH-01 | User shall log in with email/username and password | Must |
| FR-AUTH-02 | System shall issue JWT access token and refresh token on successful login | Must |
| FR-AUTH-03 | User shall refresh access token using refresh token without re-login | Must |
| FR-AUTH-04 | User shall log out and invalidate refresh token | Must |
| FR-AUTH-05 | User shall request password reset via email link | Must |
| FR-AUTH-06 | User shall reset password using secure token | Must |
| FR-AUTH-07 | Authenticated user shall change password (requires current password) | Must |
| FR-AUTH-08 | User shall view and update profile (name, avatar, timezone, preferences) | Must |
| FR-AUTH-09 | Failed login attempts shall be logged | Should |
| FR-AUTH-10 | Password policy: min 8 chars, uppercase, lowercase, digit, special char | Must |

### 3.2 Roles & Authorization (FR-ROLE)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-ROLE-01 | System shall support predefined roles: Admin, Project Manager, Developer, QA, Viewer | Must |
| FR-ROLE-02 | Permissions shall be enforced at API and UI level | Must |
| FR-ROLE-03 | Admin shall assign/revoke roles for users | Must |
| FR-ROLE-04 | Project-level roles may override global role for project scope | Should |

**Permission Matrix (Summary)**

| Action | Admin | PM | Dev | QA | Viewer |
|--------|-------|-----|-----|-----|--------|
| Manage users | ✓ | — | — | — | — |
| Create project | ✓ | ✓ | — | — | — |
| Manage sprints | ✓ | ✓ | — | — | — |
| Create/edit tasks | ✓ | ✓ | ✓ | ✓ | — |
| Create/edit bugs | ✓ | ✓ | ✓ | ✓ | — |
| Delete tasks/bugs | ✓ | ✓ | — | — | — |
| View reports | ✓ | ✓ | ✓ | ✓ | ✓ |
| Kanban drag-drop | ✓ | ✓ | ✓ | ✓ | — |

### 3.3 User Management (FR-USER)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-USER-01 | Admin shall create users with role assignment | Must |
| FR-USER-02 | Admin shall edit user details | Must |
| FR-USER-03 | Admin shall soft-delete users | Must |
| FR-USER-04 | Admin shall activate/deactivate users | Must |
| FR-USER-05 | Admin shall search users by name, email, role | Must |
| FR-USER-06 | User list shall support pagination, sorting, filtering | Must |
| FR-USER-07 | Deactivated users cannot log in | Must |

### 3.4 Project Management (FR-PROJ)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-PROJ-01 | Authorized users shall create projects (key, name, description, lead) | Must |
| FR-PROJ-02 | Authorized users shall update project details | Must |
| FR-PROJ-03 | Authorized users shall soft-delete projects | Must |
| FR-PROJ-04 | Authorized users shall archive/unarchive projects | Must |
| FR-PROJ-05 | PM shall add/remove project members with project role | Must |
| FR-PROJ-06 | Project dashboard shall show task/bug counts, sprint status, activity | Must |
| FR-PROJ-07 | Project settings: workflow columns, labels, priorities, severities | Should |

### 3.5 Task Management (FR-TASK)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-TASK-01 | Create task with title, description, type, priority, status | Must |
| FR-TASK-02 | Assign assignee and reporter | Must |
| FR-TASK-03 | Set story points, estimated hours, actual hours | Must |
| FR-TASK-04 | Set start date and due date | Must |
| FR-TASK-05 | Add/remove labels | Must |
| FR-TASK-06 | Upload attachments (images, documents) | Must |
| FR-TASK-07 | Define acceptance criteria (rich text) | Must |
| FR-TASK-08 | Manage checklist items (add, toggle, delete) | Must |
| FR-TASK-09 | Create subtasks linked to parent task | Must |
| FR-TASK-10 | Edit, soft-delete tasks | Must |
| FR-TASK-11 | Task search with filters (status, assignee, priority, labels) | Must |
| FR-TASK-12 | Auto-generate task key (e.g., PROJ-101) | Must |

### 3.6 Bug Management (FR-BUG)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-BUG-01 | Create bug with title, description, severity, priority | Must |
| FR-BUG-02 | Capture environment, browser, OS | Must |
| FR-BUG-03 | Upload screenshot and video attachments | Must |
| FR-BUG-04 | Steps to reproduce, expected result, actual result | Must |
| FR-BUG-05 | Assign developer and tester | Must |
| FR-BUG-06 | Bug status workflow: Open → In Progress → Fixed → Retest → Closed/Reopened | Must |
| FR-BUG-07 | Edit, soft-delete bugs | Must |
| FR-BUG-08 | Bug search with filters | Must |
| FR-BUG-09 | Auto-generate bug key (e.g., PROJ-BUG-42) | Must |

### 3.7 Kanban Board (FR-KANBAN)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-KANBAN-01 | Display columns: Todo, In Progress, Code Review, Testing, Done | Must |
| FR-KANBAN-02 | Drag-and-drop cards between columns | Must |
| FR-KANBAN-03 | Card shows key, title, assignee, priority, labels | Must |
| FR-KANBAN-04 | Filter board by assignee, sprint, labels | Must |
| FR-KANBAN-05 | Real-time updates via SignalR when any user moves/updates card | Must |

### 3.8 Sprint Management (FR-SPRINT)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-SPRINT-01 | Create sprint with name, goal, start/end dates | Must |
| FR-SPRINT-02 | Start sprint (lock backlog selection) | Must |
| FR-SPRINT-03 | Close sprint (move incomplete items to backlog) | Must |
| FR-SPRINT-04 | Manage sprint backlog (add/remove tasks) | Must |
| FR-SPRINT-05 | Calculate and display velocity (completed story points) | Must |
| FR-SPRINT-06 | Display burndown chart (ideal vs actual) | Must |

### 3.9 Dashboard (FR-DASH)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-DASH-01 | Summary cards: open tasks, open bugs, sprint progress | Must |
| FR-DASH-02 | Recent activity feed | Must |
| FR-DASH-03 | My assigned tasks widget | Must |
| FR-DASH-04 | Open/closed bugs widget | Must |
| FR-DASH-05 | Project statistics charts (status distribution, priority breakdown) | Must |
| FR-DASH-06 | Role-based dashboard content | Should |

### 3.10 Comments (FR-COMMENT)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-COMMENT-01 | Add comments on tasks and bugs | Must |
| FR-COMMENT-02 | Nested replies (threaded comments) | Must |
| FR-COMMENT-03 | @mention users (triggers notification) | Must |
| FR-COMMENT-04 | Attach files to comments | Must |
| FR-COMMENT-05 | Emoji reactions on comments | Should |
| FR-COMMENT-06 | Edit/delete own comments | Must |

### 3.11 Notifications (FR-NOTIF)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-NOTIF-01 | Real-time notifications via SignalR | Must |
| FR-NOTIF-02 | Notify on: task assigned, task updated, comment added, status changed | Must |
| FR-NOTIF-03 | Notification bell with unread count | Must |
| FR-NOTIF-04 | Mark as read / mark all read | Must |
| FR-NOTIF-05 | Notification history with pagination | Must |

### 3.12 Reports (FR-REPORT)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-REPORT-01 | Developer report: tasks completed, hours logged, bugs fixed | Must |
| FR-REPORT-02 | Bug report: by severity, status, environment | Must |
| FR-REPORT-03 | Sprint report: velocity, burndown, completion rate | Must |
| FR-REPORT-04 | Project report: overall health metrics | Must |
| FR-REPORT-05 | Time tracking report by user/project/date range | Must |
| FR-REPORT-06 | Export reports to PDF and Excel | Must |

### 3.13 Search (FR-SEARCH)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-SEARCH-01 | Global search across projects, tasks, bugs | Must |
| FR-SEARCH-02 | Scoped search: tasks only, bugs only, projects only | Must |
| FR-SEARCH-03 | Search results with highlighting and type badges | Must |
| FR-SEARCH-04 | Recent searches (client-side) | Should |

### 3.14 Audit Log (FR-AUDIT)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-AUDIT-01 | All entities track CreatedBy, CreatedDate, UpdatedBy, UpdatedDate | Must |
| FR-AUDIT-02 | Soft-deleted entities track DeletedBy, DeletedDate | Must |
| FR-AUDIT-03 | Audit log table captures entity changes with old/new values | Must |
| FR-AUDIT-04 | Capture client IP address on create/update/delete | Must |
| FR-AUDIT-05 | Admin can view audit log with filters | Must |

### 3.15 File Upload (FR-FILE)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-FILE-01 | Upload images (jpg, png, gif, webp) max 10 MB | Must |
| FR-FILE-02 | Upload documents (pdf, docx, xlsx) max 25 MB | Must |
| FR-FILE-03 | Upload videos (mp4, webm) max 100 MB | Must |
| FR-FILE-04 | Profile picture upload with crop preview | Must |
| FR-FILE-05 | Secure file download with authorization check | Must |
| FR-FILE-06 | Virus scan hook (integration point) | Should |

---

## 4. Business Rules

1. **Task/Bug Keys** are unique per project; auto-incremented.
2. **Sprint** can only have one active sprint per project at a time.
3. **Soft Delete** — records are never physically removed; `IsDeleted = true`.
4. **Status Transitions** follow defined workflow rules per entity type.
5. **Time Logging** — actual hours cannot be negative; estimated hours optional.
6. **Project Membership** required to view project data (except Admin).
7. **Refresh Token** rotation on each refresh; old token invalidated.

---

## 5. Out of Scope (v1.0)

- Email server integration (SMTP config only; mock in dev)
- Multi-tenant SaaS billing
- Custom workflow designer (fixed columns in v1)
- Mobile native apps
- Third-party integrations (GitHub, Slack)
- LDAP/SSO (future phase)

---

## 6. Acceptance Criteria (Release)

- All Must requirements implemented and tested
- API coverage ≥ 80% unit test coverage on services
- All screens responsive (mobile, tablet, desktop)
- Lighthouse accessibility score ≥ 85
- Zero critical/high security vulnerabilities in dependency scan
