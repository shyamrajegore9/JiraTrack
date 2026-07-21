-- M14 File Upload
-- Attachments table is created via EF migration M14_Attachments.
-- Run: cd API; dotnet ef database update

USE JiraTrackDB;
GO

IF OBJECT_ID('dbo.Attachments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attachments (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EntityType NVARCHAR(20) NOT NULL,
        EntityId INT NOT NULL,
        FileName NVARCHAR(255) NOT NULL,
        StoredFileName NVARCHAR(255) NOT NULL,
        ContentType NVARCHAR(100) NOT NULL,
        FileSize BIGINT NOT NULL,
        FileType INT NOT NULL,
        UploadedBy INT NOT NULL,
        UploadedDate DATETIME2 NOT NULL,
        CONSTRAINT FK_Attachments_Users_UploadedBy FOREIGN KEY (UploadedBy) REFERENCES dbo.Users(Id)
    );

    CREATE INDEX IX_Attachments_EntityType_EntityId ON dbo.Attachments(EntityType, EntityId);
END
GO

PRINT 'M14 Attachments table ready';
GO
