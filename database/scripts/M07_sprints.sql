-- M07 Sprint Management
-- Tables: Sprints, FK TaskItems.SprintId -> Sprints

USE JiraTrackDB;
GO

-- Apply via EF migration:
-- cd API
-- dotnet ef database update

PRINT 'M07 Sprints schema applied via EF migration M07_Sprints';
GO
