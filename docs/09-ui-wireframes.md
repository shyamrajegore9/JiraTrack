# UI Wireframes — JiraTrack PM

**Version:** 1.0  
**Date:** July 21, 2026  
**Design System:** Angular Material 3, responsive breakpoints (320 / 768 / 1024 / 1440)

---

## 1. Design Tokens

| Token | Light | Dark |
|-------|-------|------|
| Primary | `#1565C0` | `#90CAF9` |
| Accent | `#FF6F00` | `#FFB74D` |
| Background | `#FAFAFA` | `#121212` |
| Surface | `#FFFFFF` | `#1E1E1E` |
| Error | `#D32F2F` | `#EF5350` |
| Success | `#388E3C` | `#66BB6A` |

**Typography:** Roboto — H1 24px, H2 20px, Body 14px, Caption 12px

---

## 2. App Shell Layout

```
┌─────────────────────────────────────────────────────────────────────┐
│ ☰  JiraTrack          🔍 Search...          🔔(3)  🌙  👤 John ▼  │  ← Toolbar (64px)
├──────────┬──────────────────────────────────────────────────────────┤
│          │                                                          │
│ Dashboard│   ┌──────────────────────────────────────────────────┐  │
│ Projects │   │                                                  │  │
│ Users    │   │              ROUTER OUTLET                       │  │
│ Reports  │   │              (Feature Content)                   │  │
│ Audit    │   │                                                  │  │
│          │   └──────────────────────────────────────────────────┘  │
│ ──────── │                                                          │
│ Profile  │                                                          │
│ Logout   │                                                          │
│          │                                                          │
│ Sidenav  │                                                          │
│ (240px)  │                    Main Content Area                     │
│          │                                                          │
└──────────┴──────────────────────────────────────────────────────────┘

Mobile (<768px): Sidenav collapses to overlay drawer; hamburger toggles
```

---

## 3. Login Screen

```
┌─────────────────────────────────────────┐
│                                         │
│         ┌───────────────────┐         │
│         │   🔷 JiraTrack     │         │
│         │                   │         │
│         │  Email            │         │
│         │  ┌───────────────┐│         │
│         │  │               ││         │
│         │  └───────────────┘│         │
│         │                   │         │
│         │  Password         │         │
│         │  ┌───────────────┐│         │
│         │  │           👁  ││         │
│         │  └───────────────┘│         │
│         │                   │         │
│         │  ☐ Remember me    │         │
│         │                   │         │
│         │  ┌───────────────┐│         │
│         │  │    SIGN IN    ││         │
│         │  └───────────────┘│         │
│         │                   │         │
│         │  Forgot password? │         │
│         └───────────────────┘         │
│                                         │
└─────────────────────────────────────────┘

Centered card (max-width 400px), gradient background
```

---

## 4. Dashboard

```
┌─────────────────────────────────────────────────────────────────────┐
│ Dashboard                                            Welcome, John!  │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐       │
│  │ Open Tasks │ │ Open Bugs  │ │ Sprint     │ │ Completed  │       │
│  │    24      │ │     8      │ │ Progress   │ │ This Week  │       │
│  │  ↑ 3 new   │ │  ↓ 2 fixed │ │   65%      │ │    12      │       │
│  └────────────┘ └────────────┘ └────────────┘ └────────────┘       │
│                                                                      │
│  ┌─────────────────────────────┐  ┌─────────────────────────────┐ │
│  │ My Assigned Tasks           │  │ Recent Activity             │ │
│  │ ─────────────────────────── │  │ ─────────────────────────── │ │
│  │ PROJ-101 Fix login bug  🔴  │  │ John moved PROJ-105 → Done  │ │
│  │ PROJ-102 Add validation 🟡  │  │ Jane commented on BUG-12    │ │
│  │ PROJ-103 Update docs    🟢  │  │ Mike created Sprint 4       │ │
│  │ [View All →]                │  │ [View All →]                │ │
│  └─────────────────────────────┘  └─────────────────────────────┘ │
│                                                                      │
│  ┌─────────────────────────────┐  ┌─────────────────────────────┐ │
│  │ Tasks by Status (Chart)     │  │ Bugs by Severity (Chart)    │ │
│  │  ████ Todo: 10              │  │  █ Critical: 2              │ │
│  │  ████ In Progress: 8        │  │  █ High: 3                  │ │
│  │  ████ Testing: 4            │  │  █ Medium: 2                │ │
│  │  ████ Done: 18              │  │  █ Low: 1                   │ │
│  └─────────────────────────────┘  └─────────────────────────────┘ │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 5. Project List

```
┌─────────────────────────────────────────────────────────────────────┐
│ Projects                                    [+ New Project]          │
├─────────────────────────────────────────────────────────────────────┤
│ 🔍 Search projects...    Filter: [All ▼] [Active ▼]                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ PROJ  Project Alpha          Lead: John    Tasks: 45  Bugs: 8 │  │
│  │       Agile web application   ● Active Sprint: Sprint 3       │  │
│  └──────────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ MOBL  Mobile App             Lead: Jane    Tasks: 32  Bugs: 5 │  │
│  │       iOS/Android app         ○ No active sprint              │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ◄ 1  2  3 ►                              Showing 1-10 of 25       │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 6. Kanban Board

```
┌─────────────────────────────────────────────────────────────────────┐
│ Project Alpha › Kanban          Filter: [Assignee ▼] [Sprint ▼]    │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│ Todo (5)    │ In Progress (3) │ Code Review (2) │ Testing │ Done  │
│ ────────────┼─────────────────┼─────────────────┼─────────┼─────── │
│ ┌─────────┐ │ ┌─────────────┐ │ ┌─────────────┐ │┌───────┐│┌──────┐│
│ │PROJ-101 │ │ │ PROJ-104    │ │ │ PROJ-107    │ ││PROJ-  │││PROJ- ││
│ │Fix bug  │ │ │ API endpoint│ │ │ UI refactor │ ││108    │││109   ││
│ │🔴 👤 J  │ │ │ 🟡 👤 M     │ │ │ 🟢 👤 S     │ ││🟡 👤 Q│││✅    ││
│ │3 pts    │ │ │ 5 pts       │ │ │ 2 pts       │ │└───────┘│└──────┘│
│ └─────────┘ │ └─────────────┘ │ └─────────────┘ │         │       │
│ ┌─────────┐ │ ┌─────────────┐ │                 │         │       │
│ │PROJ-102 │ │ │ PROJ-105    │ │                 │         │       │
│ │Add test │ │ │ Database    │ │                 │         │       │
│ └─────────┘ │ └─────────────┘ │                 │         │       │
│             │                 │                 │         │       │
│ [+ Add]     │                 │                 │         │       │
└─────────────────────────────────────────────────────────────────────┘

Drag cards between columns; column headers show count
Horizontal scroll on mobile; columns min-width 280px
```

---

## 7. Task Detail

```
┌─────────────────────────────────────────────────────────────────────┐
│ ← Back    PROJ-101: Fix login validation bug          [Edit] [⋮]  │
├──────────────────────────────────────┬──────────────────────────────┤
│                                      │                              │
│  Description                         │  Details                     │
│  ─────────────────────────────       │  ─────────────────────       │
│  Rich text editor content...         │  Status:    [In Progress ▼]  │
│                                      │  Priority:  [High ▼]         │
│  Acceptance Criteria                 │  Assignee:  [👤 John ▼]      │
│  ─────────────────────────────       │  Reporter:  👤 Jane          │
│  - User sees error on invalid email  │  Story Pts: 3                │
│  - Password min 8 chars enforced     │  Est Hours: 8h               │
│  - Success redirect to dashboard     │  Actual:    4h               │
│                                      │  Start:     Jul 15, 2026     │
│  Checklist (2/4)                     │  Due:       Jul 25, 2026     │
│  ─────────────────────────────       │                              │
│  ☑ Reproduce bug                     │  Labels                      │
│  ☑ Write unit tests                  │  [bug] [auth] [+ Add]        │
│  ☐ Fix validation logic              │                              │
│  ☐ QA verification                   │  Sprint: Sprint 3            │
│                                      │                              │
│  Subtasks (1)                        │  Attachments (2)             │
│  ─────────────────────────────       │  📎 screenshot.png           │
│  ☐ PROJ-101-1 Write test cases       │  📎 error-log.txt            │
│  [+ Add Subtask]                     │  [+ Upload]                  │
│                                      │                              │
├──────────────────────────────────────┴──────────────────────────────┤
│  Comments (3)                                                        │
│  ─────────────────────────────────────────────────────────────────── │
│  👤 Jane · 2 hours ago                                               │
│  @John please check the email regex pattern                          │
│    └─ 👤 John · 1 hour ago                                           │
│       Updated the regex, please retest                               │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ Write a comment... @mention  📎  😊              [Post]      │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘

Mobile: Details panel stacks below description
```

---

## 8. Bug Detail

```
┌─────────────────────────────────────────────────────────────────────┐
│ ← Back    PROJ-BUG-12: Login fails on Safari           [Edit] [⋮]  │
├──────────────────────────────────────┬──────────────────────────────┤
│  Description                         │  Details                     │
│  Login button unresponsive on Safari │  Severity:  [Critical ▼]     │
│                                      │  Priority:  [High ▼]         │
│  Steps to Reproduce                  │  Status:    [Open ▼]         │
│  1. Open Safari 17                   │  Developer: [👤 Mike ▼]      │
│  2. Navigate to login page           │  Tester:    [👤 QA Sue ▼]    │
│  3. Enter valid credentials          │  Environment: Production     │
│  4. Click Sign In — no response      │  Browser:   Safari 17        │
│                                      │  OS:        macOS 14         │
│  Expected: Redirect to dashboard     │                              │
│  Actual: Button click ignored        │  Screenshots                 │
│                                      │  ┌────────┐ ┌────────┐       │
│  Attachments                         │  │ 📷     │ │ 📷     │       │
│  📎 recording.mp4  🎬               │  └────────┘ └────────┘       │
│  📎 console-log.txt                  │  [+ Upload Screenshot/Video] │
├──────────────────────────────────────┴──────────────────────────────┤
│  Comments section (same as Task Detail)                              │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 9. Sprint Detail + Burndown

```
┌─────────────────────────────────────────────────────────────────────┐
│ Sprint 3 — User Authentication         [Start Sprint] [Close]      │
│ Jul 15 – Jul 29, 2026 · Goal: Complete auth module                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐           │
│  │ 34 pts   │  │ 21 pts   │  │ 13 pts   │  │ 62%      │           │
│  │ Total    │  │ Completed│  │ Remaining│  │ Progress │           │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘           │
│                                                                      │
│  Burndown Chart                                                      │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ 34│\                                                         │   │
│  │   │  \  --- Ideal                                            │   │
│  │   │    \---- Actual                                          │   │
│  │  0│────────────────────────────────────────── Days →         │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  Sprint Backlog (8 tasks)                    [+ Add Tasks]          │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ ☐ PROJ-101  Fix login bug       3pts  👤 John   In Progress  │  │
│  │ ☐ PROJ-102  Add unit tests      2pts  👤 Mike   Todo          │  │
│  │ ☑ PROJ-103  Setup CI pipeline   5pts  👤 Sue    Done          │  │
│  └──────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 10. User Management (Admin)

```
┌─────────────────────────────────────────────────────────────────────┐
│ Users                                              [+ Create User]  │
├─────────────────────────────────────────────────────────────────────┤
│ 🔍 Search...   Role: [All ▼]   Status: [All ▼]                     │
├─────────────────────────────────────────────────────────────────────┤
│ Name          │ Email              │ Role(s)    │ Status │ Actions  │
│───────────────┼────────────────────┼────────────┼────────┼──────────│
│ John Smith    │ john@co.com        │ Developer  │ ● Active│ ✏️ 🚫 🗑 │
│ Jane Doe      │ jane@co.com        │ PM         │ ● Active│ ✏️ 🚫 🗑 │
│ Mike Wilson   │ mike@co.com        │ QA         │ ○ Inactive│ ✏️ ✅ 🗑│
├─────────────────────────────────────────────────────────────────────┤
│ ◄ 1  2 ►                                     Showing 1-10 of 15    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 11. Notification Panel

```
                                    ┌─────────────────────────────┐
                                    │ Notifications    Mark all ✓ │
                                    ├─────────────────────────────┤
                                    │ ● Task PROJ-101 assigned    │
                                    │   to you · 5 min ago        │
                                    ├─────────────────────────────┤
                                    │ ● Jane commented on BUG-12  │
                                    │   · 1 hour ago              │
                                    ├─────────────────────────────┤
                                    │ ○ Sprint 3 started          │
                                    │   · 2 hours ago             │
                                    ├─────────────────────────────┤
                                    │        View All →           │
                                    └─────────────────────────────┘
                                    Slides from right (360px panel)
```

---

## 12. Global Search

```
┌─────────────────────────────────────────────────────────────────────┐
│ 🔍  login                                              [Search]     │
├─────────────────────────────────────────────────────────────────────┤
│ Filters: [All ▼] [Tasks ▼] [Bugs ▼] [Projects ▼]                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  TASK  PROJ-101  Fix **login** validation          In Progress      │
│  BUG   PROJ-BUG-5  **Login** fails on Safari       Open             │
│  PROJECT  Auth Module  User **login** system       Active           │
│                                                                      │
│  3 results found                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 13. Responsive Breakpoints

| Breakpoint | Layout Changes |
|------------|----------------|
| ≥1440px | Full sidenav + 2-column detail pages |
| ≥1024px | Full sidenav + Kanban 5 columns visible |
| ≥768px | Collapsible sidenav, Kanban horizontal scroll |
| <768px | Overlay sidenav, single column, stacked detail panels |
| <480px | Bottom nav for key actions, simplified cards |

---

## 14. Component Library (Shared)

| Component | Usage |
|-----------|-------|
| `PageHeaderComponent` | Title + breadcrumbs + action buttons |
| `DataTableComponent` | Sortable, paginated Material table |
| `StatusBadgeComponent` | Colored status chips |
| `PriorityBadgeComponent` | Priority indicators |
| `ConfirmDialogComponent` | Delete/archive confirmations |
| `FileUploadComponent` | Drag-drop upload with preview |
| `RichTextEditorComponent` | Description/acceptance criteria |
| `EmptyStateComponent` | No data illustrations |
| `ChartCardComponent` | Wrapper for Chart.js/ngx-charts |
