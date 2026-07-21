# M08 — Comments & Mentions

Threaded comments on **Tasks** and **Bugs** with @mentions and emoji reactions.

## API Endpoints (`/api/v1/comments`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/comments?entityType=Task&entityId={id}` | List top-level comments with nested replies |
| POST | `/comments` | Create comment or reply |
| PUT | `/comments/{id}` | Edit own comment |
| DELETE | `/comments/{id}` | Delete (owner or Admin) |
| POST | `/comments/{id}/reactions` | Add emoji reaction |
| DELETE | `/comments/{id}/reactions/{emoji}` | Remove reaction (URL-encoded emoji) |

All endpoints require authentication. Project membership is enforced via the linked Task/Bug.

## Features

- **Threaded replies** — up to 3 levels deep
- **@mentions** — parsed from `@username` in content; stored in `CommentMention` (notifications in M09)
- **Emoji reactions** — 👍 ❤️ 😄 🎉 👀 🚀 (toggle per user)
- **Permissions** — author can edit; author or Admin can delete

## Frontend Integration

Comment threads appear on:

- Task detail — `/app/projects/:id/tasks/:taskId`
- Bug detail — `/app/projects/:id/bugs/:bugId`

Components:

- `CommentThreadComponent` — load thread, add top-level comment
- `CommentItemComponent` — recursive replies, edit/delete, reactions

## Database Migration

```powershell
cd API
dotnet ef database update
```

Migration: `M08_Comments` — tables `Comments`, `CommentMention`, `CommentReaction`

## Test Create Comment

```http
POST http://localhost:5005/api/v1/comments
Authorization: Bearer {token}
Content-Type: application/json

{
  "entityType": "Task",
  "entityId": 1,
  "content": "Looks good @developer — please review the checklist."
}
```

## Deferred

- File attachments on comments → **M14**
- @mention push/email notifications → **M09**
