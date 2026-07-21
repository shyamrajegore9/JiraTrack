-- M13 Audit Log
-- AuditLogs table is created via EF migration M13_AuditLogs.
-- Run: cd API; dotnet ef database update

USE JiraTrackDB;
GO

IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EntityType NVARCHAR(100) NOT NULL,
        EntityId INT NOT NULL,
        Action NVARCHAR(20) NOT NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        UserId INT NULL,
        IpAddress NVARCHAR(45) NULL,
        Timestamp DATETIME2 NOT NULL,
        CONSTRAINT FK_AuditLogs_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_AuditLogs_EntityType_EntityId ON dbo.AuditLogs(EntityType, EntityId);
    CREATE INDEX IX_AuditLogs_Timestamp ON dbo.AuditLogs(Timestamp);
    CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs(UserId);
END
GO

PRINT 'M13 AuditLogs table ready';
GO
