SET NOCOUNT ON;
BEGIN TRY
BEGIN TRAN;
------------------------------------------------------------
-- 0) Resolve Admin Role
------------------------------------------------------------
DECLARE @AdminRoleId UNIQUEIDENTIFIER =
(
    SELECT TOP (1) [Id]
    FROM [dbo].[AspNetRoles]
    WHERE [NormalizedName] = 'ADMIN'
);
IF @AdminRoleId IS NULL
BEGIN
    RAISERROR('Admin role not found. Seed IdentityRole for ADMIN first.', 16, 1);
    ROLLBACK TRAN;
    RETURN;
END

------------------------------------------------------------
-- 1) Seed Users (if missing)
------------------------------------------------------------
DECLARE @User1Email NVARCHAR(256) = 'admin1@ev.local';
DECLARE @User2Email NVARCHAR(256) = 'admin2@ev.local';

DECLARE @User1Id UNIQUEIDENTIFIER = (
    SELECT [Id] FROM [dbo].[AspNetUsers] WHERE [NormalizedUserName] = UPPER(@User1Email)
);
DECLARE @User2Id UNIQUEIDENTIFIER = (
    SELECT [Id] FROM [dbo].[AspNetUsers] WHERE [NormalizedUserName] = UPPER(@User2Email)
);

-- Use deterministic IDs if creating new
IF @User1Id IS NULL SET @User1Id = 'C0010000-0000-0000-0000-000000000001';
IF @User2Id IS NULL SET @User2Id = 'C0010000-0000-0000-0000-000000000002';

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUsers] WHERE [Id] = @User1Id)
BEGIN
    INSERT [dbo].[AspNetUsers]
    (
        [Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],
        [PasswordHash],[SecurityStamp],[ConcurrencyStamp],
        [PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],
        [LockoutEnd],[LockoutEnabled],[AccessFailedCount]
    )
    VALUES
    (
        @User1Id, @User1Email, UPPER(@User1Email), @User1Email, UPPER(@User1Email), 1,
        'AQAAAAIAAYagAAAAEBzoDN2fJEecqOdFPmYBLy6GLNyRqVBrLnBINGXUVB8e/xQziEHFXleOGAdb34KgDg==', NEWID(), NEWID(),
        NULL, 0, 0,
        NULL, 1, 0
    );
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUsers] WHERE [Id] = @User2Id)
BEGIN
    INSERT [dbo].[AspNetUsers]
    (
        [Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],
        [PasswordHash],[SecurityStamp],[ConcurrencyStamp],
        [PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],
        [LockoutEnd],[LockoutEnabled],[AccessFailedCount]
    )
    VALUES
    (
        @User2Id, @User2Email, UPPER(@User2Email), @User2Email, UPPER(@User2Email), 1,
        'AQAAAAIAAYagAAAAEJ6PAMstluvfm+wIWBlCU9XsQ48IL+OILVBJU29WUhLlkXw3KDigA29VRIYEvoixjw==', NEWID(), NEWID(),
        NULL, 0, 0,
        NULL, 1, 0
    );
END

------------------------------------------------------------
-- 2) Grant Admin role to both users
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @User1Id AND [RoleId] = @AdminRoleId)
    INSERT [dbo].[AspNetUserRoles]([UserId],[RoleId]) VALUES (@User1Id, @AdminRoleId);

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @User2Id AND [RoleId] = @AdminRoleId)
    INSERT [dbo].[AspNetUserRoles]([UserId],[RoleId]) VALUES (@User2Id, @AdminRoleId);

------------------------------------------------------------
-- 3) Domain Admins (dbo.admins) pointing to Identity UserId
--    Columns assumed from your EF config:
--    [AdminId] UNIQUEIDENTIFIER PK
--    [Title] NVARCHAR(100), [Notes] NVARCHAR(500), [HireDate] DATETIME2
--    [UserId] UNIQUEIDENTIFIER (FK to AspNetUsers, if you added it)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [dbo].[admins] WHERE [UserId] = @User1Id)
    INSERT [dbo].[admins]([AdminId],[Title],[Notes],[HireDate],[UserId])
    VALUES (NEWID(), N'Super Admin', N'Seeded admin', SYSUTCDATETIME(), @User1Id);

IF NOT EXISTS (SELECT 1 FROM [dbo].[admins] WHERE [UserId] = @User2Id)
    INSERT [dbo].[admins]([AdminId],[Title],[Notes],[HireDate],[UserId])
    VALUES (NEWID(), N'Ops Admin', N'Seeded admin', SYSUTCDATETIME(), @User2Id);

COMMIT TRAN;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRAN;

    DECLARE @Err NVARCHAR(MAX) =
        CONCAT(ERROR_NUMBER(), ': ', ERROR_MESSAGE(), ' (Line ', ERROR_LINE(), ', State ', ERROR_STATE(), ')');
    RAISERROR(@Err, 16, 1);
END CATCH;
