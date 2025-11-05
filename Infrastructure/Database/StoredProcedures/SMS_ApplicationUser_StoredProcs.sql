USE [PDXSMS_V2]
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_Delete]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_Delete]
    @pID VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required ID
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Application User with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    DELETE FROM [dbo].[tbld_SMSApplicationUsers]
    WHERE [fldv_Code] = @pID;
    
    -- Confirm deletion was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to delete SMS Application User with ID %s', 16, 1, @pID);
        RETURN;
    END
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_GetAll]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID] AS ID,
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_ApplicationRole],
        [fldv_PermissionLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSApplicationUsers]
    ORDER BY [fldv_LastName], [fldv_FirstName];
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_GetById]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_GetById]
    @pID VARCHAR(60) -- The Code 'AU-0001'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_ApplicationRole],
        [fldv_PermissionLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSApplicationUsers]
    WHERE [fldv_Code] = @pID;
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_GetByUserName]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_GetByUserName]
    @pUserName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_ApplicationRole],
        [fldv_PermissionLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSApplicationUsers]
    WHERE [fldv_UserName] = @pUserName;
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_GetByRole]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_GetByRole]
    @pApplicationRole NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_ApplicationRole],
        [fldv_PermissionLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSApplicationUsers]
    WHERE [fldv_ApplicationRole] = @pApplicationRole
    ORDER BY [fldv_LastName], [fldv_FirstName];
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_GetActiveUsers]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_GetActiveUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_ApplicationRole],
        [fldv_PermissionLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSApplicationUsers]
    WHERE [fldb_IsActive] = 1
    ORDER BY [fldv_LastName], [fldv_FirstName];
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_Insert]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_Insert]
    @pCode NVARCHAR(50),
    @pFirstName NVARCHAR(100),
    @pLastName NVARCHAR(100),
    @pUserName NVARCHAR(50),
    @pPassword NVARCHAR(255),
    @pApplicationRole NVARCHAR(100),
    @pPermissionLevel NVARCHAR(50) = 'Standard',
    @pIsActive BIT = 1,
    @pLastLoginDate DATETIME2(7) = NULL,
    @pCreatedBy NVARCHAR(50),
    @pCreatedDate DATETIME2(7),
    @pNewID INT OUTPUT,
    @pNewSMSApplicationUserCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pFirstName IS NULL OR LTRIM(RTRIM(@pFirstName)) = ''
    BEGIN
        RAISERROR('FirstName is required', 16, 1);
        RETURN;
    END
    
    IF @pLastName IS NULL OR LTRIM(RTRIM(@pLastName)) = ''
    BEGIN
        RAISERROR('LastName is required', 16, 1);
        RETURN;
    END
    
    IF @pUserName IS NULL OR LTRIM(RTRIM(@pUserName)) = ''
    BEGIN
        RAISERROR('UserName is required', 16, 1);
        RETURN;
    END
    
    IF @pPassword IS NULL OR LTRIM(RTRIM(@pPassword)) = ''
    BEGIN
        RAISERROR('Password is required', 16, 1);
        RETURN;
    END
    
    IF @pApplicationRole IS NULL OR LTRIM(RTRIM(@pApplicationRole)) = ''
    BEGIN
        RAISERROR('ApplicationRole is required', 16, 1);
        RETURN;
    END
    
    -- Check for duplicate username
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_UserName] = @pUserName)
    BEGIN
        RAISERROR('A user with username %s already exists', 16, 1, @pUserName);
        RETURN;
    END
    
    -- Generate SMS Application User Code using entity registry
    EXEC [pr_GenerateFormattedCode] 
        @EntityName = 'SMSApplicationUser',           -- Uses registry: Table='tbld_SMSApplicationUsers', Prefix='AU'
        @GeneratedCode = @pNewSMSApplicationUserCode OUTPUT;

    INSERT INTO [dbo].[tbld_SMSApplicationUsers] (
        [fldv_Code], [fldv_FirstName], [fldv_LastName], [fldv_UserName], 
        [fldv_Password], [fldv_ApplicationRole], [fldv_PermissionLevel],
        [fldb_IsActive], [fldd_LastLoginDate], [fldv_CreatedBy], [fldd_CreatedDate]
    )
    VALUES (
        @pNewSMSApplicationUserCode, @pFirstName, @pLastName, @pUserName,
        @pPassword, @pApplicationRole, @pPermissionLevel,
        @pIsActive, @pLastLoginDate, @pCreatedBy, @pCreatedDate
    );
    
    SET @pNewID = SCOPE_IDENTITY();
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_Update]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_Update]
    @pID VARCHAR(60),
    @pCode NVARCHAR(50),
    @pFirstName NVARCHAR(100),
    @pLastName NVARCHAR(100),
    @pUserName NVARCHAR(50),
    @pApplicationRole NVARCHAR(100),
    @pPermissionLevel NVARCHAR(50),
    @pIsActive BIT,
    @pLastLoginDate DATETIME2(7) = NULL,
    @pUpdatedBy NVARCHAR(50),
    @pUpdatedDate DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    IF @pFirstName IS NULL OR LTRIM(RTRIM(@pFirstName)) = ''
    BEGIN
        RAISERROR('FirstName is required', 16, 1);
        RETURN;
    END
    
    IF @pLastName IS NULL OR LTRIM(RTRIM(@pLastName)) = ''
    BEGIN
        RAISERROR('LastName is required', 16, 1);
        RETURN;
    END
    
    IF @pUserName IS NULL OR LTRIM(RTRIM(@pUserName)) = ''
    BEGIN
        RAISERROR('UserName is required', 16, 1);
        RETURN;
    END
    
    IF @pApplicationRole IS NULL OR LTRIM(RTRIM(@pApplicationRole)) = ''
    BEGIN
        RAISERROR('ApplicationRole is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Application User with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    -- Check for duplicate username (excluding current record)
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_UserName] = @pUserName AND [fldv_Code] != @pID)
    BEGIN
        RAISERROR('A user with username %s already exists', 16, 1, @pUserName);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSApplicationUsers]
    SET 
        [fldv_Code] = @pCode,
        [fldv_FirstName] = @pFirstName,
        [fldv_LastName] = @pLastName,
        [fldv_UserName] = @pUserName,
        [fldv_ApplicationRole] = @pApplicationRole,
        [fldv_PermissionLevel] = @pPermissionLevel,
        [fldb_IsActive] = @pIsActive,
        [fldd_LastLoginDate] = @pLastLoginDate,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = @pUpdatedDate
    WHERE [fldv_Code] = @pID;
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to update SMS Application User with ID %s', 16, 1, @pID);
        RETURN;
    END
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_UpdatePassword]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_UpdatePassword]
    @pID VARCHAR(60),
    @pPassword NVARCHAR(255),
    @pUpdatedBy NVARCHAR(50),
    @pUpdatedDate DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    IF @pPassword IS NULL OR LTRIM(RTRIM(@pPassword)) = ''
    BEGIN
        RAISERROR('Password is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Application User with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSApplicationUsers]
    SET 
        [fldv_Password] = @pPassword,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = @pUpdatedDate
    WHERE [fldv_Code] = @pID;
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to update password for SMS Application User with ID %s', 16, 1, @pID);
        RETURN;
    END
END

GO
/****** Object:  StoredProcedure [dbo].[pr_SMSApplicationUser_RecordLogin]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_RecordLogin]
    @pID VARCHAR(60),
    @pLoginDate DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Application User with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSApplicationUsers]
    SET [fldd_LastLoginDate] = @pLoginDate
    WHERE [fldv_Code] = @pID;
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to record login for SMS Application User with ID %s', 16, 1, @pID);
        RETURN;
    END
END

GO