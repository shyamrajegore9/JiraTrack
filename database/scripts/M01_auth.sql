-- JiraTrack M01 Authentication - Database Script
-- Run on SQL Server 2019+

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'JiraTrackDB')
BEGIN
    CREATE DATABASE JiraTrackDB;
END
GO

USE JiraTrackDB;
GO

-- Note: Prefer running EF migrations for schema sync:
-- dotnet ef database update

-- Default admin user (password: Admin@123) is seeded automatically on first API startup.

-- Roles seeded via EF:
-- 1 Admin, 2 ProjectManager, 3 Developer, 4 QA, 5 Viewer

PRINT 'JiraTrackDB ready. Start API to apply migrations and seed admin user.';
GO
