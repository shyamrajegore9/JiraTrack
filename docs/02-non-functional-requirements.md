# Non-Functional Requirements — JiraTrack PM

**Version:** 1.0  
**Date:** July 21, 2026

---

## 1. Performance

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-PERF-01 | API response time (p95) for read operations | < 200 ms |
| NFR-PERF-02 | API response time (p95) for write operations | < 500 ms |
| NFR-PERF-03 | Kanban board initial load (100 cards) | < 2 s |
| NFR-PERF-04 | Global search results | < 1 s |
| NFR-PERF-05 | SignalR notification delivery | < 500 ms |
| NFR-PERF-06 | Support concurrent users per instance | 500+ |
| NFR-PERF-07 | Database queries use indexed columns; no N+1 queries | Enforced |

---

## 2. Scalability

| ID | Requirement |
|----|-------------|
| NFR-SCALE-01 | Stateless API design for horizontal scaling |
| NFR-SCALE-02 | SignalR with Redis backplane (production) |
| NFR-SCALE-03 | File storage abstracted (local dev, Azure Blob prod) |
| NFR-SCALE-04 | Pagination mandatory for all list endpoints (default 20, max 100) |

---

## 3. Security

| ID | Requirement |
|----|-------------|
| NFR-SEC-01 | JWT access token expiry: 15 minutes |
| NFR-SEC-02 | Refresh token expiry: 7 days; stored hashed in DB |
| NFR-SEC-03 | HTTPS only in production |
| NFR-SEC-04 | CORS restricted to configured origins |
| NFR-SEC-05 | Input validation on all endpoints (FluentValidation) |
| NFR-SEC-06 | SQL injection prevention via EF Core parameterized queries |
| NFR-SEC-07 | XSS prevention: sanitize rich text output |
| NFR-SEC-08 | Rate limiting on auth endpoints (5 req/min) |
| NFR-SEC-09 | Secrets in environment variables / Azure Key Vault |
| NFR-SEC-10 | Role-based authorization on every protected endpoint |
| NFR-SEC-11 | File upload: whitelist extensions, scan MIME type |
| NFR-SEC-12 | Audit trail for all data mutations |

---

## 4. Reliability & Availability

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-REL-01 | API uptime | 99.9% |
| NFR-REL-02 | Database backup | Daily automated |
| NFR-REL-03 | Graceful degradation if SignalR unavailable | UI polling fallback |
| NFR-REL-04 | Global exception handling; no stack traces in production responses | Enforced |
| NFR-REL-05 | Health check endpoints (`/health`, `/health/ready`) | Required |

---

## 5. Maintainability

| ID | Requirement |
|----|-------------|
| NFR-MAINT-01 | Clean Architecture with clear layer boundaries |
| NFR-MAINT-02 | SOLID principles enforced |
| NFR-MAINT-03 | Dependency Injection throughout |
| NFR-MAINT-04 | API versioning (URL: `/api/v1/...`) |
| NFR-MAINT-05 | Swagger/OpenAPI documentation for all endpoints |
| NFR-MAINT-06 | Serilog structured logging (JSON in production) |
| NFR-MAINT-07 | Consistent response wrapper format |
| NFR-MAINT-08 | Code analysis: nullable reference types enabled |

---

## 6. Usability

| ID | Requirement |
|----|-------------|
| NFR-UX-01 | Responsive design: mobile (≥320px), tablet, desktop |
| NFR-UX-02 | Dark theme and light theme with user preference persistence |
| NFR-UX-03 | Loading states, error messages, empty states on all screens |
| NFR-UX-04 | Form validation with inline error messages |
| NFR-UX-05 | Keyboard navigation for Kanban drag-drop alternative |
| NFR-UX-06 | WCAG 2.1 Level AA compliance target |

---

## 7. Compatibility

| ID | Requirement |
|----|-------------|
| NFR-COMPAT-01 | Browsers: Chrome, Edge, Firefox, Safari (latest 2 versions) |
| NFR-COMPAT-02 | SQL Server 2019+ |
| NFR-COMPAT-03 | .NET 9 LTS |
| NFR-COMPAT-04 | Node.js 22 LTS for Angular build |

---

## 8. Observability

| ID | Requirement |
|----|-------------|
| NFR-OBS-01 | Serilog: Console (dev), File + Seq/Application Insights (prod) |
| NFR-OBS-02 | Correlation ID per request (X-Correlation-Id header) |
| NFR-OBS-03 | Log levels: Debug (dev), Information (prod), Error always |
| NFR-OBS-04 | Metrics: request count, duration, error rate |

---

## 9. CI/CD (Jenkins)

| ID | Requirement |
|----|-------------|
| NFR-CICD-01 | Pipeline stages: Build → Test → SonarQube → Deploy |
| NFR-CICD-02 | Backend: `dotnet build`, `dotnet test`, publish artifact |
| NFR-CICD-03 | Frontend: `npm ci`, `ng build --configuration=production`, publish artifact |
| NFR-CICD-04 | Database migrations run on deploy |
| NFR-CICD-05 | Environment-specific configs (Dev, Staging, Prod) |
| NFR-CICD-06 | Automated deployment to IIS / Docker container |

---

## 10. Data Integrity

| ID | Requirement |
|----|-------------|
| NFR-DATA-01 | Foreign key constraints on all relationships |
| NFR-DATA-02 | Unique constraints on email, project key, task/bug keys |
| NFR-DATA-03 | Soft delete global query filter in EF Core |
| NFR-DATA-04 | Optimistic concurrency via RowVersion on critical entities |
| NFR-DATA-05 | Transaction scope via Unit of Work pattern |

---

## 11. Localization (Future)

| ID | Requirement |
|----|-------------|
| NFR-L10N-01 | UI strings externalized for i18n (v1: English only) |
| NFR-L10N-02 | Date/time displayed in user timezone |

---

## 12. Documentation

| ID | Requirement |
|----|-------------|
| NFR-DOC-01 | API documentation via Swagger |
| NFR-DOC-02 | README with setup instructions per module |
| NFR-DOC-03 | Architecture decision records (ADRs) for key decisions |
| NFR-DOC-04 | Postman collection for API testing |
