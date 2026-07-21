-- M10 Dashboard
-- Stored procedure for project-level statistics (optional; global dashboard uses EF)

USE JiraTrackDB;
GO

IF OBJECT_ID('dbo.sp_GetProjectStatistics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetProjectStatistics;
GO

CREATE PROCEDURE dbo.sp_GetProjectStatistics
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id AS ProjectId,
        p.[Key],
        p.Name,
        (SELECT COUNT(*) FROM TaskItems t WHERE t.ProjectId = p.Id AND t.ParentTaskId IS NULL AND t.IsDeleted = 0) AS TotalTasks,
        (SELECT COUNT(*) FROM TaskItems t WHERE t.ProjectId = p.Id AND t.ParentTaskId IS NULL AND t.IsDeleted = 0 AND t.Status <> 4) AS OpenTasks,
        (SELECT COUNT(*) FROM Bugs b WHERE b.ProjectId = p.Id AND b.IsDeleted = 0) AS TotalBugs,
        (SELECT COUNT(*) FROM Bugs b WHERE b.ProjectId = p.Id AND b.IsDeleted = 0 AND b.Status <> 4) AS OpenBugs,
        (SELECT COUNT(*) FROM ProjectMembers pm WHERE pm.ProjectId = p.Id) AS MemberCount,
        (SELECT TOP 1 s.Name FROM Sprints s WHERE s.ProjectId = p.Id AND s.Status = 1 AND s.IsDeleted = 0) AS ActiveSprintName
    FROM Projects p
    WHERE p.Id = @ProjectId AND p.IsDeleted = 0;
END
GO

PRINT 'M10 Dashboard stored procedure sp_GetProjectStatistics created';
GO
