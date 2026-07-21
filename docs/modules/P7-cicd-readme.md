# Phase 7 — CI/CD & Hardening

Production readiness: Jenkins pipeline, Docker, health checks, security headers, tests, and deployment docs.

## Deliverables

| Item | Location |
|------|----------|
| Jenkins pipeline | `Jenkinsfile` |
| Solution + tests | `JiraTrack.sln`, `Tests/` |
| Docker | `docker-compose.yml`, `API/Dockerfile`, `APP/Dockerfile` |
| Health endpoints | `GET /health`, `GET /health/ready` |
| Security headers | `SecurityHeadersMiddleware` |
| Auth guard | Re-enabled on `/app` routes |
| Postman collection | `docs/postman/JiraTrack.postman_collection.json` |
| Deployment runbook | `docs/deployment-runbook.md` |

## Health Checks

```http
GET http://localhost:5005/health
GET http://localhost:5005/health/ready
```

`/health/ready` verifies SQL Server connectivity.

## Run Tests

```powershell
dotnet test Tests/JiraTrack.Tests.csproj -c Release
```

## OpenAPI

- Development: http://localhost:5005/openapi/v1.json
- Staging: enabled via `AppSettings:EnableOpenApi`
- Production: disabled by default (`appsettings.Production.json`)

## Hardening Checklist

- [x] Security headers middleware
- [x] Auth guard on protected routes
- [x] Production config (`ExposeResetTokenInDev: false`)
- [x] Unit tests for password hashing and file validation
- [x] Jenkins CI pipeline
- [x] Docker Compose for integrated deployment
- [x] Postman collection
- [x] Deployment runbook
- [ ] SonarQube (configure `SONAR_TOKEN` in Jenkins)
- [ ] Performance testing (500 concurrent users)
- [ ] WCAG accessibility audit
- [ ] ≥80% code coverage (expand test suite)
