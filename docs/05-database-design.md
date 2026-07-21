# Database Design — JiraTrack PM

**Version:** 1.0  
**Date:** July 21, 2026  
**RDBMS:** SQL Server 2019+

---

## 1. Design Principles

- **Naming:** PascalCase tables/columns; plural table names
- **Primary Keys:** `INT IDENTITY` for most entities; `UNIQUEIDENTIFIER` for files if needed
- **Audit:** All business tables include audit + soft delete columns
- **Concurrency:** `RowVersion` (timestamp) on TaskItem, Bug, Sprint, Project
- **Indexes:** FK columns, search columns, composite indexes for common queries

---

## 2. Standard Audit Columns

Every auditable table includes:

| Column | Type | Notes |
|--------|------|-------|
| CreatedBy | INT NULL | FK → Users.Id |
| CreatedDate | DATETIME2 | UTC default GETUTCDATE() |
| UpdatedBy | INT NULL | FK → Users.Id |
| UpdatedDate | DATETIME2 NULL | |
| DeletedBy | INT NULL | FK → Users.Id |
| DeletedDate | DATETIME2 NULL | |
| IsDeleted | BIT | Default 0; global filter |
| RowVersion | ROWVERSION | Optimistic concurrency |

---

## 3. Entity Definitions

### 3.1 Users

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| Email | NVARCHAR(256) | UNIQUE, NOT NULL |
| UserName | NVARCHAR(100) | UNIQUE, NOT NULL |
| PasswordHash | NVARCHAR(500) | NOT NULL |
| FirstName | NVARCHAR(100) | NOT NULL |
| LastName | NVARCHAR(100) | NOT NULL |
| PhoneNumber | NVARCHAR(20) | NULL |
| ProfilePictureUrl | NVARCHAR(500) | NULL |
| TimeZone | NVARCHAR(50) | Default 'UTC' |
| IsActive | BIT | Default 1 |
| LastLoginDate | DATETIME2 | NULL |
| + audit columns | | |

**Indexes:** `IX_Users_Email`, `IX_Users_UserName`, `IX_Users_IsActive_IsDeleted`

### 3.2 Roles

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| Name | NVARCHAR(50) | UNIQUE, NOT NULL |
| Description | NVARCHAR(200) | NULL |
| + audit columns | | |

**Seed Data:** Admin, ProjectManager, Developer, QA, Viewer

### 3.3 UserRoles

| Column | Type | Constraints |
|--------|------|-------------|
| UserId | INT | PK, FK → Users |
| RoleId | INT | PK, FK → Roles |

### 3.4 RefreshTokens

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| UserId | INT | FK → Users, NOT NULL |
| TokenHash | NVARCHAR(500) | NOT NULL |
| ExpiresAt | DATETIME2 | NOT NULL |
| CreatedAt | DATETIME2 | NOT NULL |
| RevokedAt | DATETIME2 | NULL |
| ReplacedByTokenHash | NVARCHAR(500) | NULL |
| CreatedByIp | NVARCHAR(45) | NULL |

**Indexes:** `IX_RefreshTokens_UserId`, `IX_RefreshTokens_TokenHash`

### 3.5 PasswordResetTokens

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| UserId | INT | FK → Users |
| TokenHash | NVARCHAR(500) | NOT NULL |
| ExpiresAt | DATETIME2 | NOT NULL |
| IsUsed | BIT | Default 0 |
| CreatedAt | DATETIME2 | NOT NULL |

### 3.6 Projects

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| Key | NVARCHAR(10) | UNIQUE, NOT NULL (e.g., PROJ) |
| Name | NVARCHAR(200) | NOT NULL |
| Description | NVARCHAR(MAX) | NULL |
| LeadUserId | INT | FK → Users |
| IsArchived | BIT | Default 0 |
| TaskCounter | INT | Default 0 (for key generation) |
| BugCounter | INT | Default 0 |
| + audit columns | | |

**Indexes:** `IX_Projects_Key`, `IX_Projects_LeadUserId`, `IX_Projects_IsArchived`

### 3.7 ProjectMembers

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| ProjectId | INT | FK → Projects |
| UserId | INT | FK → Users |
| ProjectRole | NVARCHAR(50) | NOT NULL |
| JoinedDate | DATETIME2 | NOT NULL |
| + audit columns | | |

**Unique:** `UQ_ProjectMembers_ProjectId_UserId`  
**Indexes:** `IX_ProjectMembers_ProjectId`, `IX_ProjectMembers_UserId`

### 3.8 Sprints

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| ProjectId | INT | FK → Projects |
| Name | NVARCHAR(200) | NOT NULL |
| Goal | NVARCHAR(500) | NULL |
| StartDate | DATETIME2 | NULL |
| EndDate | DATETIME2 | NULL |
| Status | INT | 0=Planning, 1=Active, 2=Closed |
| + audit columns | | |

**Indexes:** `IX_Sprints_ProjectId_Status`

### 3.9 TaskItems

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| ProjectId | INT | FK → Projects |
| SprintId | INT NULL | FK → Sprints |
| TaskKey | NVARCHAR(20) | NOT NULL (PROJ-101) |
| Title | NVARCHAR(300) | NOT NULL |
| Description | NVARCHAR(MAX) | NULL |
| AcceptanceCriteria | NVARCHAR(MAX) | NULL |
| Status | INT | 0=Todo..4=Done |
| Priority | INT | 0=Low..3=Critical |
| AssigneeId | INT NULL | FK → Users |
| ReporterId | INT | FK → Users |
| StoryPoints | DECIMAL(5,1) | NULL |
| EstimatedHours | DECIMAL(8,2) | NULL |
| ActualHours | DECIMAL(8,2) | Default 0 |
| StartDate | DATETIME2 | NULL |
| DueDate | DATETIME2 | NULL |
| SortOrder | INT | Default 0 (Kanban) |
| ParentTaskId | INT NULL | FK → TaskItems (subtasks) |
| + audit columns | | |

**Unique:** `UQ_TaskItems_ProjectId_TaskKey`  
**Indexes:** `IX_TaskItems_ProjectId_Status`, `IX_TaskItems_AssigneeId`, `IX_TaskItems_SprintId`, `IX_TaskItems_Title` (full-text)

### 3.10 Bugs

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| ProjectId | INT | FK → Projects |
| BugKey | NVARCHAR(20) | NOT NULL |
| Title | NVARCHAR(300) | NOT NULL |
| Description | NVARCHAR(MAX) | NULL |
| Severity | INT | 0=Low..3=Critical |
| Priority | INT | 0=Low..3=Critical |
| Status | INT | Workflow enum |
| Environment | NVARCHAR(100) | NULL |
| Browser | NVARCHAR(100) | NULL |
| OperatingSystem | NVARCHAR(100) | NULL |
| StepsToReproduce | NVARCHAR(MAX) | NULL |
| ExpectedResult | NVARCHAR(MAX) | NULL |
| ActualResult | NVARCHAR(MAX) | NULL |
| DeveloperId | INT NULL | FK → Users |
| TesterId | INT NULL | FK → Users |
| ReporterId | INT | FK → Users |
| + audit columns | | |

**Unique:** `UQ_Bugs_ProjectId_BugKey`  
**Indexes:** `IX_Bugs_ProjectId_Status`, `IX_Bugs_DeveloperId`, `IX_Bugs_Severity`

### 3.11 Labels

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| ProjectId | INT | FK → Projects |
| Name | NVARCHAR(50) | NOT NULL |
| Color | NVARCHAR(7) | Default '#757575' |
| + audit columns | | |

**Unique:** `UQ_Labels_ProjectId_Name`

### 3.12 TaskLabels (M:M)

| TaskId | INT | PK, FK → TaskItems |
| LabelId | INT | PK, FK → Labels |

### 3.13 ChecklistItems

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| TaskId | INT | FK → TaskItems |
| Text | NVARCHAR(500) | NOT NULL |
| IsCompleted | BIT | Default 0 |
| SortOrder | INT | Default 0 |
| + audit columns | | |

### 3.14 Comments

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| EntityType | NVARCHAR(20) | Task / Bug |
| EntityId | INT | NOT NULL |
| ParentCommentId | INT NULL | FK → Comments |
| UserId | INT | FK → Users |
| Content | NVARCHAR(MAX) | NOT NULL |
| + audit columns | | |

**Indexes:** `IX_Comments_EntityType_EntityId`, `IX_Comments_ParentCommentId`

### 3.15 CommentMentions

| CommentId | INT | PK, FK → Comments |
| UserId | INT | PK, FK → Users |

### 3.16 CommentReactions

| Column | Type | Constraints |
|--------|------|-------------|
| CommentId | INT | PK, FK → Comments |
| UserId | INT | PK, FK → Users |
| Emoji | NVARCHAR(10) | PK |

### 3.17 Attachments

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| EntityType | NVARCHAR(20) | Task/Bug/Comment/User |
| EntityId | INT | NOT NULL |
| FileName | NVARCHAR(255) | NOT NULL |
| StoredFileName | NVARCHAR(255) | NOT NULL |
| ContentType | NVARCHAR(100) | NOT NULL |
| FileSize | BIGINT | NOT NULL |
| FileType | INT | Image/Document/Video |
| UploadedBy | INT | FK → Users |
| UploadedDate | DATETIME2 | NOT NULL |

**Indexes:** `IX_Attachments_EntityType_EntityId`

### 3.18 Notifications

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| UserId | INT | FK → Users |
| Type | INT | Enum |
| Title | NVARCHAR(200) | NOT NULL |
| Message | NVARCHAR(500) | NOT NULL |
| EntityType | NVARCHAR(20) | NULL |
| EntityId | INT | NULL |
| IsRead | BIT | Default 0 |
| CreatedDate | DATETIME2 | NOT NULL |

**Indexes:** `IX_Notifications_UserId_IsRead`, `IX_Notifications_CreatedDate`

### 3.19 TimeLogs

| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT IDENTITY | PK |
| TaskId | INT | FK → TaskItems |
| UserId | INT | FK → Users |
| Hours | DECIMAL(8,2) | NOT NULL |
| WorkDate | DATE | NOT NULL |
| Description | NVARCHAR(500) | NULL |
| + audit columns | | |

**Indexes:** `IX_TimeLogs_TaskId`, `IX_TimeLogs_UserId_WorkDate`

### 3.20 SprintTasks (M:M)

| SprintId | INT | PK, FK → Sprints |
| TaskId | INT | PK, FK → TaskItems |
| AddedDate | DATETIME2 | NOT NULL |

### 3.21 AuditLogs

| Column | Type | Constraints |
|--------|------|-------------|
| Id | BIGINT IDENTITY | PK |
| EntityType | NVARCHAR(100) | NOT NULL |
| EntityId | INT | NOT NULL |
| Action | NVARCHAR(20) | Create/Update/Delete |
| OldValues | NVARCHAR(MAX) | JSON |
| NewValues | NVARCHAR(MAX) | JSON |
| UserId | INT NULL | FK → Users |
| IpAddress | NVARCHAR(45) | NULL |
| Timestamp | DATETIME2 | NOT NULL |

**Indexes:** `IX_AuditLogs_EntityType_EntityId`, `IX_AuditLogs_Timestamp`, `IX_AuditLogs_UserId`

---

## 4. Relationships Summary

| Parent | Child | Relationship |
|--------|-------|--------------|
| Users | UserRoles | 1:M |
| Roles | UserRoles | 1:M |
| Users | RefreshTokens | 1:M |
| Users | Projects (Lead) | 1:M |
| Projects | ProjectMembers | 1:M |
| Users | ProjectMembers | 1:M |
| Projects | Sprints | 1:M |
| Projects | TaskItems | 1:M |
| Projects | Bugs | 1:M |
| Projects | Labels | 1:M |
| Sprints | TaskItems | 1:M (optional FK) |
| Sprints | SprintTasks | M:M via junction |
| TaskItems | ChecklistItems | 1:M |
| TaskItems | TaskLabels | M:M |
| TaskItems | TaskItems (Parent) | 1:M self-ref |
| TaskItems | TimeLogs | 1:M |
| Comments | Comments (Parent) | 1:M self-ref |
| Comments | CommentMentions | 1:M |
| Users | Notifications | 1:M |

---

## 5. Stored Procedures

### sp_GetSprintBurndown
**Input:** `@SprintId INT`  
**Output:** Daily remaining story points vs ideal line  
**Logic:** Aggregate completed tasks by date; calculate ideal burndown from sprint dates and total points

### sp_GetProjectStatistics
**Input:** `@ProjectId INT`  
**Output:** Task/bug counts by status, priority distribution, sprint velocity history

### sp_GlobalSearch
**Input:** `@SearchTerm NVARCHAR(200)`, `@UserId INT`, `@PageNumber INT`, `@PageSize INT`  
**Output:** Unified search results from Projects, TaskItems, Bugs (respecting project membership)  
**Uses:** Full-text indexes on Title, Description, TaskKey, BugKey

---

## 6. Full-Text Indexes

```sql
CREATE FULLTEXT CATALOG JiraTrackCatalog AS DEFAULT;
CREATE FULLTEXT INDEX ON TaskItems(Title, Description) KEY INDEX PK_TaskItems;
CREATE FULLTEXT INDEX ON Bugs(Title, Description) KEY INDEX PK_Bugs;
CREATE FULLTEXT INDEX ON Projects(Name, Description) KEY INDEX PK_Projects;
```

---

## 7. Key Constraints & Rules

1. **Project.Key** — uppercase alphanumeric, 2–10 chars, unique globally
2. **TaskKey** — `{ProjectKey}-{TaskCounter}` generated in transaction
3. **BugKey** — `{ProjectKey}-BUG-{BugCounter}`
4. **One Active Sprint** — filtered unique index: `UX_Sprints_ProjectId_Active WHERE Status = 1 AND IsDeleted = 0`
5. **Cascade:** Soft delete only; no physical CASCADE DELETE
6. **FK behavior:** RESTRICT on delete for all foreign keys

---

## 8. Sample Seed Data

```sql
-- Roles
INSERT INTO Roles (Name, Description) VALUES
('Admin', 'System Administrator'),
('ProjectManager', 'Project Manager'),
('Developer', 'Software Developer'),
('QA', 'Quality Assurance'),
('Viewer', 'Read-only Viewer');

-- Default Admin (password hashed at app layer)
-- Email: admin@jiratrack.com
```
