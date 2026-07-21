# M14 — File Upload

Secure file upload and download for tasks, bugs, and profile pictures.

## API Endpoints (`/api/v1/files`)

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/files?entityType=&entityId=` | Authorized (entity access) |
| POST | `/files/upload` | All (write access on entity) |
| POST | `/files/profile-picture` | All (own profile) |
| GET | `/files/{id}` | Authorized (entity access) |
| DELETE | `/files/{id}` | Owner, Admin |

**Upload:** multipart/form-data — `file`, `entityType`, `entityId`

## File Limits

| Type | Extensions | Max Size |
|------|------------|----------|
| Image | jpg, png, gif, webp | 10 MB |
| Document | pdf, docx, xlsx | 25 MB |
| Video | mp4, webm | 100 MB |

## Backend

- **Attachments** table — polymorphic entity reference, stored path, MIME, size, type
- **LocalFileStorageService** — saves under `UploadedFiles/{year}/{month}/{type}/`
- **FileValidationHelper** — extension whitelist, content-type check, magic-byte validation
- **NoOpVirusScanService** — integration hook for future virus scanning

## Frontend

- **FileUploadComponent** — drag-and-drop upload zone (shared)
- **AttachmentListComponent** — list, image preview, download, delete on task/bug detail
- **ProfilePictureUploadComponent** — crop preview dialog, updates profile picture

## Test Upload

```http
POST http://localhost:5005/api/v1/files/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: (binary)
entityType: Task
entityId: 1
```

## Migration

```bash
cd API
dotnet ef database update
```

Optional manual script: `database/scripts/M14_files.sql`
