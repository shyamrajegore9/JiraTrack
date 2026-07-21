# M01 + M02 — Authentication & User Management

## Backend Endpoints

### Auth (`/api/v1/auth`)
- POST login, refresh, logout, forgot-password, reset-password, change-password
- GET/PUT profile

### Users (`/api/v1/users`) — Admin only
- GET list (paged, search, filter)
- GET by id, POST create, PUT update, DELETE soft-delete
- PATCH activate/deactivate, PUT assign roles
- GET lookup

### Roles (`/api/v1/roles`) — Admin only
- GET list, GET by id

## Default Credentials
- Email: `admin@jiratrack.com`
- Password: `Admin@123`

## Run API
```powershell
cd API
dotnet ef database update
dotnet run
```

## Run Frontend
```powershell
cd APP
npm start
```

Frontend: http://localhost:4200  
API: http://localhost:5005

## Test Login
```http
POST http://localhost:5005/api/v1/auth/login
Content-Type: application/json

{
  "email": "admin@jiratrack.com",
  "password": "Admin@123"
}
```
