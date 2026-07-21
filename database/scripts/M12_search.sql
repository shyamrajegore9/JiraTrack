-- M12 Global Search
-- Optional full-text search infrastructure (API uses EF Contains by default)

USE JiraTrackDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'JiraTrackCatalog')
BEGIN
    CREATE FULLTEXT CATALOG JiraTrackCatalog AS DEFAULT;
END
GO

-- Full-text indexes require unique index on PK (already exists via PK_* constraints)
-- Run manually if FTS is desired:
-- CREATE FULLTEXT INDEX ON TaskItems(Title, Description) KEY INDEX PK_TaskItems;
-- CREATE FULLTEXT INDEX ON Bugs(Title, Description) KEY INDEX PK_Bugs;
-- CREATE FULLTEXT INDEX ON Projects(Name, Description) KEY INDEX PK_Projects;
GO

IF OBJECT_ID('dbo.sp_GlobalSearch', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GlobalSearch;
GO

CREATE PROCEDURE dbo.sp_GlobalSearch
    @SearchTerm NVARCHAR(200),
    @UserId INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @Term NVARCHAR(202) = '%' + @SearchTerm + '%';
    DECLARE @IsAdmin BIT = CASE WHEN EXISTS (
        SELECT 1 FROM UserRoles ur
        INNER JOIN Roles r ON r.Id = ur.RoleId
        WHERE ur.UserId = @UserId AND r.Name = 'Admin'
    ) THEN 1 ELSE 0 END;

    ;WITH Results AS (
        SELECT 'Project' AS [Type], p.Id, p.Id AS ProjectId, p.[Key], p.Name AS Title,
               p.Description AS Subtitle, CASE WHEN p.IsArchived = 1 THEN 'Archived' ELSE 'Active' END AS [Status]
        FROM Projects p
        WHERE p.IsDeleted = 0 AND p.IsArchived = 0
          AND (p.[Key] LIKE @Term OR p.Name LIKE @Term OR p.Description LIKE @Term)
          AND (@IsAdmin = 1 OR EXISTS (SELECT 1 FROM ProjectMembers pm WHERE pm.ProjectId = p.Id AND pm.UserId = @UserId))

        UNION ALL

        SELECT 'Task', t.Id, t.ProjectId, t.TaskKey, t.Title, pr.Name, CAST(t.Status AS NVARCHAR(20))
        FROM TaskItems t
        INNER JOIN Projects pr ON pr.Id = t.ProjectId
        WHERE t.IsDeleted = 0 AND t.ParentTaskId IS NULL
          AND (t.TaskKey LIKE @Term OR t.Title LIKE @Term OR t.Description LIKE @Term)
          AND (@IsAdmin = 1 OR EXISTS (SELECT 1 FROM ProjectMembers pm WHERE pm.ProjectId = t.ProjectId AND pm.UserId = @UserId))

        UNION ALL

        SELECT 'Bug', b.Id, b.ProjectId, b.BugKey, b.Title, pr.Name, CAST(b.Status AS NVARCHAR(20))
        FROM Bugs b
        INNER JOIN Projects pr ON pr.Id = b.ProjectId
        WHERE b.IsDeleted = 0
          AND (b.BugKey LIKE @Term OR b.Title LIKE @Term OR b.Description LIKE @Term)
          AND (@IsAdmin = 1 OR EXISTS (SELECT 1 FROM ProjectMembers pm WHERE pm.ProjectId = b.ProjectId AND pm.UserId = @UserId))
    )
    SELECT * FROM Results
    ORDER BY [Type], Title
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT 'M12 Search optional FTS catalog and sp_GlobalSearch created';
GO
