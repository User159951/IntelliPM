-- SQL Script to seed multi-organization test data
-- WARNING: This script uses hardcoded password hashes. For production, use the C# seeder instead.
-- 
-- Usage: Execute this script against your SQL Server database
-- 
-- Password for all users: Password123!
-- 
-- Note: This script assumes you have a PasswordHasher that uses the same algorithm.
-- For testing purposes only. Use MultiOrgDataSeeder.cs for proper password hashing.

-- ============================================
-- ORGANIZATION A
-- ============================================

-- Insert Organization A
INSERT INTO Organizations (Name, CreatedAt)
VALUES ('Organization A', GETUTCDATE());

DECLARE @OrgAId INT = SCOPE_IDENTITY();

-- Insert Users for Organization A
-- Password: Password123! (hash and salt - these are examples, use actual hashes from PasswordHasher)
INSERT INTO Users (Username, Email, PasswordHash, PasswordSalt, FirstName, LastName, GlobalRole, IsActive, OrganizationId, CreatedAt, UpdatedAt)
VALUES 
    ('admin-orga', 'admin@orga.com', 'PLACEHOLDER_HASH', 'PLACEHOLDER_SALT', 'Admin', 'OrgA', 2, 1, @OrgAId, GETUTCDATE(), GETUTCDATE()),
    ('pm-orga', 'pm@orga.com', 'PLACEHOLDER_HASH', 'PLACEHOLDER_SALT', 'Project', 'Manager A', 1, 1, @OrgAId, GETUTCDATE(), GETUTCDATE()),
    ('dev-orga', 'dev@orga.com', 'PLACEHOLDER_HASH', 'PLACEHOLDER_SALT', 'Developer', 'OrgA', 1, 1, @OrgAId, GETUTCDATE(), GETUTCDATE());

DECLARE @AdminAId INT = (SELECT Id FROM Users WHERE Username = 'admin-orga' AND OrganizationId = @OrgAId);
DECLARE @PmAId INT = (SELECT Id FROM Users WHERE Username = 'pm-orga' AND OrganizationId = @OrgAId);
DECLARE @DevAId INT = (SELECT Id FROM Users WHERE Username = 'dev-orga' AND OrganizationId = @OrgAId);

-- Insert Project for Organization A
INSERT INTO Projects (Name, Description, Type, SprintDurationDays, OwnerId, OrganizationId, Status, CreatedAt, UpdatedAt)
VALUES ('Project Alpha', 'Main project for Organization A - Web application development', 'Scrum', 14, @AdminAId, @OrgAId, 'Active', GETUTCDATE(), GETUTCDATE());

DECLARE @ProjectAId INT = SCOPE_IDENTITY();

-- Insert Project Members for Organization A
INSERT INTO ProjectMembers (ProjectId, UserId, Role, InvitedById, InvitedAt, JoinedAt)
VALUES 
    (@ProjectAId, @AdminAId, 0, @AdminAId, GETUTCDATE(), GETUTCDATE()), -- ProductOwner
    (@ProjectAId, @PmAId, 1, @AdminAId, GETUTCDATE(), GETUTCDATE()),     -- ScrumMaster
    (@ProjectAId, @DevAId, 2, @AdminAId, GETUTCDATE(), GETUTCDATE());   -- Developer

-- Insert Sprint for Organization A
INSERT INTO Sprints (ProjectId, OrganizationId, Number, Goal, StartDate, EndDate, Status, CreatedAt)
VALUES (@ProjectAId, @OrgAId, 1, 'Sprint 1: Setup and initial features', DATEADD(DAY, -7, GETUTCDATE()), DATEADD(DAY, 7, GETUTCDATE()), 'InProgress', GETUTCDATE());

DECLARE @SprintAId INT = SCOPE_IDENTITY();

-- Insert Tasks for Organization A
INSERT INTO ProjectTasks (ProjectId, OrganizationId, Title, Description, Status, Priority, StoryPoints_Value, AssigneeId, SprintId, CreatedById, CreatedAt, UpdatedAt)
VALUES 
    (@ProjectAId, @OrgAId, 'Setup development environment', 'Configure local development environment with all required tools', 'Done', 'High', 5, @DevAId, @SprintAId, @PmAId, DATEADD(DAY, -6, GETUTCDATE()), DATEADD(DAY, -5, GETUTCDATE())),
    (@ProjectAId, @OrgAId, 'Design database schema', 'Create ERD and database schema for the application', 'InProgress', 'High', 8, @DevAId, @SprintAId, @PmAId, DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE())),
    (@ProjectAId, @OrgAId, 'Implement authentication', 'Build user authentication and authorization system', 'Todo', 'Medium', 13, @DevAId, NULL, @PmAId, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -4, GETUTCDATE())),
    (@ProjectAId, @OrgAId, 'Create project dashboard', 'Build main dashboard with project overview and metrics', 'Todo', 'Medium', 8, NULL, NULL, @PmAId, DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()));

-- ============================================
-- ORGANIZATION B
-- ============================================

-- Insert Organization B
INSERT INTO Organizations (Name, CreatedAt)
VALUES ('Organization B', GETUTCDATE());

DECLARE @OrgBId INT = SCOPE_IDENTITY();

-- Insert Users for Organization B
INSERT INTO Users (Username, Email, PasswordHash, PasswordSalt, FirstName, LastName, GlobalRole, IsActive, OrganizationId, CreatedAt, UpdatedAt)
VALUES 
    ('admin-orgb', 'admin@orgb.com', 'PLACEHOLDER_HASH', 'PLACEHOLDER_SALT', 'Admin', 'OrgB', 2, 1, @OrgBId, GETUTCDATE(), GETUTCDATE()),
    ('pm-orgb', 'pm@orgb.com', 'PLACEHOLDER_HASH', 'PLACEHOLDER_SALT', 'Project', 'Manager B', 1, 1, @OrgBId, GETUTCDATE(), GETUTCDATE()),
    ('dev-orgb', 'dev@orgb.com', 'PLACEHOLDER_HASH', 'PLACEHOLDER_SALT', 'Developer', 'OrgB', 1, 1, @OrgBId, GETUTCDATE(), GETUTCDATE());

DECLARE @AdminBId INT = (SELECT Id FROM Users WHERE Username = 'admin-orgb' AND OrganizationId = @OrgBId);
DECLARE @PmBId INT = (SELECT Id FROM Users WHERE Username = 'pm-orgb' AND OrganizationId = @OrgBId);
DECLARE @DevBId INT = (SELECT Id FROM Users WHERE Username = 'dev-orgb' AND OrganizationId = @OrgBId);

-- Insert Project for Organization B
INSERT INTO Projects (Name, Description, Type, SprintDurationDays, OwnerId, OrganizationId, Status, CreatedAt, UpdatedAt)
VALUES ('Project Beta', 'Main project for Organization B - Mobile app development', 'Scrum', 14, @AdminBId, @OrgBId, 'Active', GETUTCDATE(), GETUTCDATE());

DECLARE @ProjectBId INT = SCOPE_IDENTITY();

-- Insert Project Members for Organization B
INSERT INTO ProjectMembers (ProjectId, UserId, Role, InvitedById, InvitedAt, JoinedAt)
VALUES 
    (@ProjectBId, @AdminBId, 0, @AdminBId, GETUTCDATE(), GETUTCDATE()), -- ProductOwner
    (@ProjectBId, @PmBId, 1, @AdminBId, GETUTCDATE(), GETUTCDATE()),     -- ScrumMaster
    (@ProjectBId, @DevBId, 2, @AdminBId, GETUTCDATE(), GETUTCDATE());   -- Developer

-- Insert Sprint for Organization B
INSERT INTO Sprints (ProjectId, OrganizationId, Number, Goal, StartDate, EndDate, Status, CreatedAt)
VALUES (@ProjectBId, @OrgBId, 1, 'Sprint 1: Foundation and core features', DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, 9, GETUTCDATE()), 'InProgress', GETUTCDATE());

DECLARE @SprintBId INT = SCOPE_IDENTITY();

-- Insert Tasks for Organization B
INSERT INTO ProjectTasks (ProjectId, OrganizationId, Title, Description, Status, Priority, StoryPoints_Value, AssigneeId, SprintId, CreatedById, CreatedAt, UpdatedAt)
VALUES 
    (@ProjectBId, @OrgBId, 'Setup mobile development environment', 'Configure React Native development environment', 'Done', 'High', 5, @DevBId, @SprintBId, @PmBId, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE())),
    (@ProjectBId, @OrgBId, 'Design mobile UI/UX', 'Create wireframes and design mockups for mobile app', 'InProgress', 'High', 8, @DevBId, @SprintBId, @PmBId, DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE())),
    (@ProjectBId, @OrgBId, 'Implement navigation', 'Build navigation structure for the mobile app', 'Todo', 'Medium', 13, @DevBId, NULL, @PmBId, DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE())),
    (@ProjectBId, @OrgBId, 'API integration', 'Integrate mobile app with backend API', 'Todo', 'Medium', 8, NULL, NULL, @PmBId, DATEADD(DAY, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE()));

PRINT 'Multi-organization test data seeded successfully!';
PRINT 'Organization A ID: ' + CAST(@OrgAId AS VARCHAR);
PRINT 'Organization B ID: ' + CAST(@OrgBId AS VARCHAR);
PRINT '';
PRINT 'NOTE: Password hashes are placeholders. Use MultiOrgDataSeeder.cs for proper password hashing.';
PRINT 'All users use password: Password123!';

