# Folder Structure — JiraTrack PM (MapsoftERP Style)

**Architecture:** Single ASP.NET Core Web API project with folder-based separation  
**Pattern:** Controllers → BusinessLogic → Repository → Database

---

## Root Repository Layout

```
JIRA/
├── API/                              # Single ASP.NET Core Web API project
├── APP/                              # Angular 22 Frontend (next phase)
├── database/                         # SQL scripts
├── docs/                             # Planning & module documentation
├── jenkins/                          # Jenkins pipeline files
├── postman/                          # Postman collections
└── README.md
```

---

## Backend — Single Project Structure (like MapsoftERP)

```
API/
├── JiraTrack.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
│
├── BusinessLogic/                    # Service layer (business rules)
│   ├── AuthService.cs
│   ├── TokenService.cs
│   ├── PasswordHasher.cs
│   ├── DatabaseSeeder.cs
│   └── BusinessException.cs
│
├── Controllers/                      # API endpoints
│   └── v1/
│       └── AuthController.cs
│
├── Models/                           # Data structures
│   ├── Entities/                     # EF Core entities
│   │   ├── BaseEntity.cs
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── UserRole.cs
│   │   ├── RefreshToken.cs
│   │   └── PasswordResetToken.cs
│   ├── DTOs/                         # Request/Response objects
│   │   └── Auth/
│   ├── Validators/                   # FluentValidation rules
│   │   └── Auth/
│   ├── Common/                       # ApiResponse, Pagination
│   └── Mappings/                     # AutoMapper profiles
│
├── Repository/                       # Data access layer
│   ├── ApplicationDbContext.cs
│   ├── Migrations/
│   ├── Interfaces/
│   │   └── IRepositories.cs          # IGenericRepository, IUnitOfWork, IUserRepository
│   └── Implementations/
│       ├── GenericRepository.cs
│       ├── UnitOfWork.cs
│       └── UserRepository.cs
│
├── Settings/                         # Configuration classes
│   └── AppSettings.cs                # JwtSettings, AppSettings
│
├── Middleware/                       # Cross-cutting concerns
│   └── ExceptionMiddleware.cs
│
├── Hubs/                             # SignalR hubs (future modules)
├── UploadedFiles/                    # File upload storage
├── Logs/                             # Serilog file logs
└── Properties/
    └── launchSettings.json
```

---

## Layer Responsibilities

| Folder | Responsibility | Depends On |
|--------|---------------|------------|
| **Controllers** | HTTP routing, auth attributes, response wrapping | BusinessLogic, Models |
| **BusinessLogic** | Business rules, orchestration, token generation | Repository, Models |
| **Models** | Entities, DTOs, validators, mappings | Nothing |
| **Repository** | EF Core, generic/specific repos, Unit of Work | Models |
| **Settings** | Strongly-typed config binding | Nothing |
| **Middleware** | Exception handling, correlation ID | Models |

---

## Request Flow

```
HTTP Request
  → Middleware (CorrelationId, Exception)
  → Controller (validation via FluentValidation)
  → BusinessLogic (AuthService)
  → Repository (UnitOfWork → UserRepository)
  → ApplicationDbContext (EF Core)
  → SQL Server
```

---

## Frontend (Planned — APP/)

```
APP/
├── src/app/
│   ├── core/           # Guards, interceptors, services
│   ├── shared/         # Reusable components
│   ├── layout/         # Shell, sidenav, toolbar
│   └── features/       # Lazy-loaded modules
└── ...
```

---

## Module File Convention

When adding new modules, follow this pattern:

| Module | BusinessLogic | Controllers | Models | Repository |
|--------|--------------|-------------|--------|------------|
| Users | UserService.cs | UsersController.cs | User DTOs | UserRepository (exists) |
| Projects | ProjectService.cs | ProjectsController.cs | Project entity/DTOs | ProjectRepository.cs |
| Tasks | TaskService.cs | TasksController.cs | Task entity/DTOs | TaskRepository.cs |
