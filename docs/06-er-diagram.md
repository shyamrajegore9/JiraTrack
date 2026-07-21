# ER Diagram — JiraTrack PM

**Version:** 1.0  
**Date:** July 21, 2026

---

## Entity Relationship Diagram

```mermaid
erDiagram
    Users ||--o{ UserRoles : has
    Roles ||--o{ UserRoles : assigned
    Users ||--o{ RefreshTokens : has
    Users ||--o{ PasswordResetTokens : has
    Users ||--o{ Projects : leads
    Users ||--o{ ProjectMembers : member
    Projects ||--o{ ProjectMembers : has
    Projects ||--o{ Sprints : contains
    Projects ||--o{ TaskItems : contains
    Projects ||--o{ Bugs : contains
    Projects ||--o{ Labels : has
    Sprints ||--o{ TaskItems : includes
    Sprints ||--o{ SprintTasks : backlog
    TaskItems ||--o{ SprintTasks : in
    TaskItems ||--o{ ChecklistItems : has
    TaskItems ||--o{ TaskLabels : tagged
    Labels ||--o{ TaskLabels : applied
    TaskItems ||--o{ TimeLogs : logged
    Users ||--o{ TimeLogs : logs
    TaskItems ||--o{ TaskItems : subtask
    Users ||--o{ TaskItems : assigned
    Users ||--o{ TaskItems : reported
    Users ||--o{ Bugs : develops
    Users ||--o{ Bugs : tests
    Users ||--o{ Bugs : reported
    Comments ||--o{ Comments : replies
    Users ||--o{ Comments : writes
    Comments ||--o{ CommentMentions : mentions
    Users ||--o{ CommentMentions : mentioned
    Comments ||--o{ CommentReactions : has
    Users ||--o{ CommentReactions : reacts
    Users ||--o{ Notifications : receives
    Users ||--o{ Attachments : uploads
    Users ||--o{ AuditLogs : performs

    Users {
        int Id PK
        string Email UK
        string UserName UK
        string PasswordHash
        string FirstName
        string LastName
        string ProfilePictureUrl
        bool IsActive
        datetime LastLoginDate
        audit_fields audit
    }

    Roles {
        int Id PK
        string Name UK
        string Description
    }

    UserRoles {
        int UserId PK_FK
        int RoleId PK_FK
    }

    RefreshTokens {
        int Id PK
        int UserId FK
        string TokenHash
        datetime ExpiresAt
        datetime RevokedAt
    }

    Projects {
        int Id PK
        string Key UK
        string Name
        int LeadUserId FK
        bool IsArchived
        int TaskCounter
        int BugCounter
        audit_fields audit
    }

    ProjectMembers {
        int Id PK
        int ProjectId FK
        int UserId FK
        string ProjectRole
    }

    Sprints {
        int Id PK
        int ProjectId FK
        string Name
        datetime StartDate
        datetime EndDate
        int Status
        audit_fields audit
    }

    TaskItems {
        int Id PK
        int ProjectId FK
        int SprintId FK
        string TaskKey UK
        string Title
        int Status
        int Priority
        int AssigneeId FK
        int ReporterId FK
        decimal StoryPoints
        decimal EstimatedHours
        decimal ActualHours
        int ParentTaskId FK
        audit_fields audit
    }

    Bugs {
        int Id PK
        int ProjectId FK
        string BugKey UK
        string Title
        int Severity
        int Priority
        int Status
        int DeveloperId FK
        int TesterId FK
        int ReporterId FK
        audit_fields audit
    }

    Labels {
        int Id PK
        int ProjectId FK
        string Name
        string Color
    }

    TaskLabels {
        int TaskId PK_FK
        int LabelId PK_FK
    }

    ChecklistItems {
        int Id PK
        int TaskId FK
        string Text
        bool IsCompleted
    }

    SprintTasks {
        int SprintId PK_FK
        int TaskId PK_FK
    }

    Comments {
        int Id PK
        string EntityType
        int EntityId
        int ParentCommentId FK
        int UserId FK
        string Content
        audit_fields audit
    }

    CommentMentions {
        int CommentId PK_FK
        int UserId PK_FK
    }

    CommentReactions {
        int CommentId PK_FK
        int UserId PK_FK
        string Emoji PK
    }

    Attachments {
        int Id PK
        string EntityType
        int EntityId
        string FileName
        string StoredFileName
        int UploadedBy FK
    }

    Notifications {
        int Id PK
        int UserId FK
        int Type
        string Title
        bool IsRead
        datetime CreatedDate
    }

    TimeLogs {
        int Id PK
        int TaskId FK
        int UserId FK
        decimal Hours
        date WorkDate
    }

    AuditLogs {
        bigint Id PK
        string EntityType
        int EntityId
        string Action
        string OldValues
        string NewValues
        int UserId FK
        string IpAddress
        datetime Timestamp
    }
```

---

## Relationship Cardinality Notes

| Relationship | Type | Business Rule |
|--------------|------|---------------|
| User ↔ Role | M:N | Via UserRoles; user can have multiple roles |
| Project ↔ User | M:N | Via ProjectMembers; project-scoped role |
| Project → Sprint | 1:N | One project has many sprints; only one active |
| Project → Task | 1:N | Tasks belong to exactly one project |
| Sprint → Task | 1:N | Optional; task can be in backlog (null SprintId) |
| Task → Task | 1:N | Self-referencing for subtasks |
| Task ↔ Label | M:N | Via TaskLabels |
| Comment → Comment | 1:N | Nested replies, max depth 3 (app rule) |
| Attachment → Entity | Polymorphic | EntityType + EntityId pattern |

---

## Index Strategy Diagram

```mermaid
flowchart TB
    subgraph HighTraffic["High-Traffic Query Paths"]
        Q1["Kanban: ProjectId + Status"]
        Q2["My Tasks: AssigneeId + Status"]
        Q3["Notifications: UserId + IsRead"]
        Q4["Search: Full-text Title/Description"]
        Q5["Audit: EntityType + EntityId + Timestamp"]
    end

    subgraph Indexes["Supporting Indexes"]
        I1["IX_TaskItems_ProjectId_Status"]
        I2["IX_TaskItems_AssigneeId"]
        I3["IX_Notifications_UserId_IsRead"]
        I4["FT_TaskItems_Title_Description"]
        I5["IX_AuditLogs_EntityType_EntityId"]
    end

    Q1 --> I1
    Q2 --> I2
    Q3 --> I3
    Q4 --> I4
    Q5 --> I5
```

---

## Data Flow: Task Key Generation

```mermaid
sequenceDiagram
    participant S as TaskService
    participant U as UnitOfWork
    participant P as Project Repo
    participant T as Task Repo

    S->>U: BeginTransaction
    S->>P: GetProject(projectId)
    P-->>S: Project (TaskCounter=100)
    S->>S: TaskKey = PROJ-101
    S->>P: Increment TaskCounter
    S->>T: Insert TaskItem
    S->>U: Commit
```
