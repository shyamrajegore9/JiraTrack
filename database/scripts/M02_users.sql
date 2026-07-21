-- M02 Users & Roles - No additional tables required
-- Uses Users, Roles, UserRoles from M01

USE JiraTrackDB;
GO

-- Verify roles
SELECT Id, Name, Description FROM Roles WHERE IsDeleted = 0;
GO
