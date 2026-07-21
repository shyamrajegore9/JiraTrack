# Architecture — JiraTrack PM (MapsoftERP Style)

**Version:** 1.1  
**Date:** July 21, 2026  
**Pattern:** Single-project, folder-based layered architecture

---

## 1. High-Level Architecture

```mermaid
flowchart TB
    subgraph Client["Client Layer"]
        WEB["Angular 22 SPA"]
    end

    subgraph API["JiraTrack.API — Single Project"]
        CTRL["Controllers/"]
        BL["BusinessLogic/"]
        REPO["Repository/"]
        MODELS["Models/"]
        MW["Middleware/"]
        HUB["Hubs/"]
    end

    subgraph Data["Data Layer"]
        SQL["SQL Server"]
        FILES["UploadedFiles/"]
    end

    WEB -->|REST + SignalR| CTRL
    CTRL --> MW --> BL
    BL --> REPO
    REPO --> MODELS
    REPO --> SQL
    BL --> FILES
```

---

## 2. Folder-Based Layers (MapsoftERP Pattern)

```mermaid
flowchart LR
    subgraph API["Single ASP.NET Core Project"]
        direction TB
        C["Controllers"]
        B["BusinessLogic"]
        R["Repository"]
        M["Models"]
        S["Settings"]
    end

    C --> B
    B --> R
    B --> M
    R --> M
    C --> M
    S --> B
```

**Same principles as Clean Architecture, organized as folders instead of separate projects:**

| Folder | Clean Architecture Equivalent |
|--------|--------------------------------|
| Models/Entities | Domain Layer |
| Models/DTOs + Validators | Application Layer (contracts) |
| BusinessLogic | Application Layer (services) |
| Repository | Infrastructure Layer |
| Controllers + Middleware | Presentation Layer |

---

## 3. Request Pipeline

```mermaid
sequenceDiagram
    participant C as Client
    participant M as Middleware
    participant CTRL as Controller
    participant BL as BusinessLogic
    participant R as Repository
    participant D as SQL Server

    C->>M: HTTP Request
    M->>M: Correlation ID
    M->>CTRL: Route to Controller
    CTRL->>CTRL: FluentValidation
    CTRL->>BL: AuthService.LoginAsync()
    BL->>R: UserRepository
    R->>D: EF Core Query
    D-->>R: Data
    R-->>BL: User Entity
    BL-->>CTRL: LoginResponse DTO
    CTRL-->>M: ApiResponse T
    M-->>C: JSON Response
```

---

## 4. Current Module Status

| Module | BusinessLogic | Controller | Repository | Status |
|--------|--------------|------------|------------|--------|
| M01 Auth | AuthService, TokenService | AuthController | UserRepository | Complete |
| M02 Users | Pending | Pending | Pending | Next |
| M03 Projects | Pending | Pending | Pending | Planned |

---

## 5. Cross-Cutting Concerns

| Concern | Location |
|---------|----------|
| Exception Handling | Middleware/ExceptionMiddleware.cs |
| JWT Authentication | Program.cs + BusinessLogic/TokenService.cs |
| Validation | Models/Validators/ + FluentValidation |
| Mapping | Models/Mappings/MappingProfile.cs |
| Logging | Serilog in Program.cs |
| Response Wrapper | Models/Common/ApiResponse.cs |
| Soft Delete | Models/Entities/BaseEntity.cs + EF query filters |
| API Versioning | Controllers/v1/ + Asp.Versioning |

---

## 6. Why Single Project?

Matches your existing **MapsoftERP** convention:
- One solution, one project, clear folders
- Easier navigation in Visual Studio
- Faster development for small-to-medium teams
- Still maintains separation of concerns via folders
- Repository Pattern + Unit of Work preserved
- All enterprise patterns retained (DTOs, validation, JWT, etc.)
