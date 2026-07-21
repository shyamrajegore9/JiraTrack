# JiraTrack Deployment Runbook

## Prerequisites

| Component | Version |
|-----------|---------|
| .NET SDK | 10.0+ |
| Node.js | 22+ |
| SQL Server | 2019+ |
| Docker (optional) | 24+ |

## Environment Variables (Production)

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `JwtSettings__Secret` | JWT signing key (≥32 chars) |
| `Cors__Origins__0` | Frontend URL |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

Never commit production secrets. Override via environment variables or a secret store.

## Local Development

```powershell
# Database
cd API
dotnet ef database update
dotnet run

# Frontend (separate terminal)
cd APP
npm start
```

- API: http://localhost:5005  
- APP: http://localhost:4200  
- Health: http://localhost:5005/health  
- Readiness: http://localhost:5005/health/ready  
- OpenAPI (dev): http://localhost:5005/openapi/v1.json  

## Docker Compose (Dev/Staging)

```powershell
docker compose up --build
```

- Web: http://localhost:4200 (nginx → API proxy)  
- API: http://localhost:5005  

Run migrations inside the API container after first start:

```powershell
docker compose exec api dotnet ef database update
```

## CI/CD (Jenkins)

Pipeline file: `Jenkinsfile`

| Stage | Branch trigger |
|-------|----------------|
| Build + Test | All branches |
| Deploy Dev | `develop` |
| Deploy Staging | `staging` |
| Deploy Prod | `main` |

Configure `SONAR_TOKEN` on Jenkins for optional SonarQube analysis.

## Production Deployment Checklist

1. Apply EF migrations: `dotnet ef database update`
2. Set `JwtSettings:Secret` via environment variable
3. Set `AppSettings:ExposeResetTokenInDev` to `false`
4. Set `AppSettings:EnableOpenApi` to `false` (or restrict network access)
5. Configure HTTPS termination (reverse proxy / load balancer)
6. Mount persistent volume for `UploadedFiles/`
7. Verify `/health` and `/health/ready` in load balancer probes
8. Smoke test login and core workflows

## Rollback

1. Redeploy previous API/APP artifact from Jenkins archive
2. Roll back database migration only if the release included a reversible migration
3. Verify health endpoints before restoring traffic

## Support Contacts

Document your on-call rotation and escalation path here before go-live.
