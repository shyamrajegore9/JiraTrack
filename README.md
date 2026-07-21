# JiraTrack PM — Project Management & Bug Tracking System

Enterprise-grade Jira-like application built with **Angular 22** + **ASP.NET Core** + **SQL Server**.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 22, Material, Signals, RxJS, SCSS |
| Backend | ASP.NET Core 10 Web API (single project, folder-based) |
| Database | SQL Server 2019+ |
| Real-time | SignalR |
| CI/CD | Jenkins |

## Architecture

**MapsoftERP-style single project** with folder separation:

```
API/
├── BusinessLogic/     ← Services & business rules
├── Controllers/       ← API endpoints
├── Models/            ← Entities, DTOs, Validators
├── Repository/        ← EF Core, Generic Repository, Unit of Work
├── Settings/          ← Configuration classes
├── Middleware/        ← Exception handling
├── Hubs/              ← SignalR
└── UploadedFiles/     ← File storage
```

## Current Status

| Phase | Status |
|-------|--------|
| Planning docs | Complete |
| M01 Authentication (API + APP) | Complete |
| M02 Users & Roles (API + APP) | Complete |
| M03 Projects (API + APP) | Complete |
| M04 Tasks (API + APP) | Complete |
| M05 Bugs (API + APP) | Complete |
| M06 Kanban + SignalR (API + APP) | Complete |
| M07 Sprints (API + APP) | Complete |
| M08 Comments (API + APP) | Complete |
| M09 Notifications (API + APP) | Complete |
| M10 Dashboard (API + APP) | Complete |
| M11 Reports (API + APP) | Complete |
| M12 Search (API + APP) | Complete |
| M13 Audit Log (API + APP) | Complete |
| M14 File Upload (API + APP) | Complete |
| Phase 7 CI/CD & Hardening | Next |

## Quick Start

### API
```powershell
cd API
dotnet ef database update
dotnet run
```

### Frontend
```powershell
cd APP
npm start
```

- **Frontend:** http://localhost:4200  
- **API:** http://localhost:5005  
- **Admin:** `admin@jiratrack.com` / `Admin@123`

## API Endpoints (M01 Auth)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/login` | Login |
| POST | `/api/v1/auth/refresh` | Refresh token |
| POST | `/api/v1/auth/logout` | Logout |
| POST | `/api/v1/auth/forgot-password` | Forgot password |
| POST | `/api/v1/auth/reset-password` | Reset password |
| POST | `/api/v1/auth/change-password` | Change password |
| GET | `/api/v1/auth/profile` | Get profile |
| PUT | `/api/v1/auth/profile` | Update profile |

## Documentation

See [docs/](docs/) for full planning artifacts.
