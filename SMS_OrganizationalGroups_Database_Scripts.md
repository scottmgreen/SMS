# SMS Organizational Groups - Database Scripts

This document contains all the SQL scripts required to implement SMS Organizational Groups functionality in the PDXSMS_V2 system.

## Table of Contents

1. [Database Tables](#database-tables)
2. [SMS Organizational Group Procedures](#sms-organizational-group-procedures)
3. [User-Group Management Procedures](#user-group-management-procedures)
4. [Enhanced SMS Organizational User Procedures](#enhanced-sms-organizational-user-procedures)
5. [Sample Data and Indexes](#sample-data-and-indexes)

---

## Database Tables

### 1. SMS Organizational Groups Table

```sql
-- Create SMS Organizational Groups table
CREATE TABLE [dbo].[tbld_SMSOrganizationalGroups]
(
    [fldv_Code] NVARCHAR(50) NOT NULL PRIMARY KEY,
    [fldv_GroupName] NVARCHAR(255) NOT NULL,
    [fldv_Description] NVARCHAR(1000) NULL,
    [fldv_GroupType] NVARCHAR(100) NOT NULL DEFAULT 'Department',
    [fldv_AuthorityLevel] NVARCHAR(100) NOT NULL DEFAULT 'Standard',
    [fldb_IsActive] BIT NOT NULL DEFAULT 1,
    [fldv_CreatedBy] NVARCHAR(100) NOT NULL,
    [fldd_CreatedDate] DATETIME2(7) NOT NULL,
    [fldv_UpdatedBy] NVARCHAR(100) NULL,
    [fldd_UpdatedDate] DATETIME2(7) NULL
);
```

### 2. User-Group Relationship Table

```sql
-- Create relationship table for SMS Organizational User-Group associations
CREATE TABLE [dbo].[tblr_SMSOrganizationalUserGroups]
(
    [fldv_UserCode] NVARCHAR(50) NOT NULL,
    [fldv_GroupCode] NVARCHAR(50) NOT NULL,
    [fldd_AssignedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [fldv_AssignedBy] NVARCHAR(100) NOT NULL,
    
    CONSTRAINT [PK_SMSOrganizationalUserGroups] PRIMARY KEY ([fldv_UserCode], [fldv_GroupCode]),
    CONSTRAINT [FK_SMSOrganizationalUserGroups_User] FOREIGN KEY ([fldv_UserCode]) 
        REFERENCES [dbo].[tbld_SMSOrganizationalUsers]([fldv_Code]) ON DELETE CASCADE,
    CONSTRAINT [FK_SMSOrganizationalUserGroups_Group] FOREIGN KEY ([fldv_GroupCode]) 
        REFERENCES [dbo].[tbld_SMSOrganizationalGroups]([fldv_Code]) ON DELETE CASCADE
);
```

### 3. Enhanced SMS Organizational Users Table

```sql
-- Add new SMS role fields to existing SMSOrganizationalUsers table
ALTER TABLE [dbo].[tbld_SMSOrganizationalUsers]
ADD 
    [fldv_SMSRole] NVARCHAR(100) NULL,
    [fldv_AuthorityLevel] NVARCHAR(100) NULL,
    [fldv_RiskApprovalAuthority] NVARCHAR(100) NULL;
```

---

## SMS Organizational Group Procedures

### 1. Insert SMS Organizational Group

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalGroup_Insert]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalGroup_Insert
-- Description: Create a new SMS organizational group
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalGroup_Insert]
    @pGroupCode VARCHAR(60),
    @pName VARCHAR(255),
    @pDescription VARCHAR(1000) = NULL,
    @pGroupType VARCHAR(100) = 'Department',
    @pAuthorityLevel VARCHAR(100) = 'Standard',
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewGroupCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSOrganizationalGroup_Insert', @RowsAffected INT;
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting SMS Organizational Group creation for Code: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check for duplicate code
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalGroups] WHERE [fldv_Code] = @pGroupCode)
        BEGIN
            RAISERROR('SMS Organizational group code already exists: %s', 16, 1, @pGroupCode);
        END
        
        -- Check for duplicate group name
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalGroups] WHERE [fldv_GroupName] = @pName)
        BEGIN
            RAISERROR('SMS Organizational group name already exists: %s', 16, 1, @pName);
        END
        
        EXEC [pr_GenerateFormattedCode] 
        @EntityName = 'SMSOrganizationalGroup',           -- Uses registry: Table='tbld_SMSOrganizationalGroups', Prefix='OG'
        @GeneratedCode = @pNewGroupCode OUTPUT;

        INSERT INTO [dbo].[tbld_SMSOrganizationalGroups] (
            [fldv_Code],
            [fldv_GroupName], 
            [fldv_Description],
            [fldv_GroupType],
            [fldv_AuthorityLevel],
            [fldb_IsActive],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pNewGroupCode,
            @pName,
            @pDescription,
            @pGroupType,
            @pAuthorityLevel,
            1, -- Active by default
            @UserID,
            @pCreatedDate
        );
        
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully created SMS Organizational Group Code: ' + @pNewGroupCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error creating SMS Organizational Group Code ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 2. Update SMS Organizational Group

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalGroup_Update]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalGroup_Update
-- Description: Update an existing SMS organizational group
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalGroup_Update]
    @pGroupCode VARCHAR(60),
    @pName VARCHAR(255),
    @pDescription VARCHAR(1000) = NULL,
    @pGroupType VARCHAR(100),
    @pAuthorityLevel VARCHAR(100),
    @pIsActive BIT,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSOrganizationalGroup_Update', @RowsAffected INT;
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting SMS Organizational Group update for Code: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if group exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalGroups] WHERE [fldv_Code] = @pGroupCode)
        BEGIN
            RAISERROR('SMS Organizational group code not found: %s', 16, 1, @pGroupCode);
        END
        
        -- Check for duplicate group name (excluding current group)
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalGroups] WHERE [fldv_GroupName] = @pName AND [fldv_Code] <> @pGroupCode)
        BEGIN
            RAISERROR('SMS Organizational group name already exists: %s', 16, 1, @pName);
        END
        
        UPDATE [dbo].[tbld_SMSOrganizationalGroups]
        SET 
            [fldv_GroupName] = @pName,
            [fldv_Description] = @pDescription,
            [fldv_GroupType] = @pGroupType,
            [fldv_AuthorityLevel] = @pAuthorityLevel,
            [fldb_IsActive] = @pIsActive,
            [fldv_UpdatedBy] = @UserID,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_Code] = @pGroupCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully updated SMS Organizational Group Code: ' + @pGroupCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating SMS Organizational Group Code ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 3. Delete SMS Organizational Group

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalGroup_Delete]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalGroup_Delete
-- Description: Delete an SMS organizational group and its associations
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalGroup_Delete]
    @pGroupCode VARCHAR(60),
    @pDeletedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pDeletedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSOrganizationalGroup_Delete', @RowsAffected INT;
    DECLARE @UserGroupRowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting SMS Organizational Group deletion for Code: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if group exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalGroups] WHERE [fldv_Code] = @pGroupCode)
        BEGIN
            RAISERROR('SMS Organizational group code not found: %s', 16, 1, @pGroupCode);
        END
        
        -- First remove all user-group associations
        DELETE FROM [dbo].[tblr_SMSOrganizationalUserGroups]
        WHERE [fldv_GroupCode] = @pGroupCode;
        SET @UserGroupRowsAffected = @@ROWCOUNT;
        
        -- Then delete the group
        DELETE FROM [dbo].[tbld_SMSOrganizationalGroups]
        WHERE [fldv_Code] = @pGroupCode;
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully deleted SMS Organizational Group Code: ' + @pGroupCode + ', Group rows affected: ' + CAST(@RowsAffected AS VARCHAR(10)) + ', User-Group associations removed: ' + CAST(@UserGroupRowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting SMS Organizational Group Code ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 4. Get All SMS Organizational Groups

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalGroup_GetAll]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalGroup_GetAll
-- Description: Get all SMS organizational groups
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalGroup_GetAll]
    @pActiveOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldv_Code],
        [fldv_GroupName],
        [fldv_Description],
        [fldv_GroupType],
        [fldv_AuthorityLevel],
        [fldb_IsActive],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSOrganizationalGroups]
    WHERE (@pActiveOnly = 0 OR [fldb_IsActive] = 1)
    ORDER BY [fldv_GroupName];
END;
```

### 5. Get SMS Organizational Group by Code

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalGroup_GetByCode]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalGroup_GetByCode
-- Description: Get SMS organizational group by code
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalGroup_GetByCode]
    @pGroupCode VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldv_Code],
        [fldv_GroupName],
        [fldv_Description],
        [fldv_GroupType],
        [fldv_AuthorityLevel],
        [fldb_IsActive],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSOrganizationalGroups]
    WHERE [fldv_Code] = @pGroupCode;
END;
```

---

## User-Group Management Procedures

### 1. Assign User to Organizational Group

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalUserGroup_Assign]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalUserGroup_Assign
-- Description: Assign a user to an SMS organizational group
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUserGroup_Assign]
    @pUserCode VARCHAR(60),
    @pGroupCode VARCHAR(60),
    @pAssignedBy VARCHAR(50) = 'SYSTEM',
    @pAssignedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pAssignedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSOrganizationalUserGroup_Assign', @RowsAffected INT;
    IF @pAssignedDate IS NULL SET @pAssignedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting SMS Organizational User-Group assignment for User: ' + @pUserCode + ', Group: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalUsers] WHERE [fldv_Code] = @pUserCode)
        BEGIN
            RAISERROR('SMS Organizational user code not found: %s', 16, 1, @pUserCode);
        END
        
        -- Check if group exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalGroups] WHERE [fldv_Code] = @pGroupCode)
        BEGIN
            RAISERROR('SMS Organizational group code not found: %s', 16, 1, @pGroupCode);
        END
        
        -- Check if assignment already exists
        IF NOT EXISTS (
            SELECT 1 FROM [dbo].[tblr_SMSOrganizationalUserGroups]
            WHERE [fldv_UserCode] = @pUserCode AND [fldv_GroupCode] = @pGroupCode
        )
        BEGIN
            INSERT INTO [dbo].[tblr_SMSOrganizationalUserGroups]
            (
                [fldv_UserCode],
                [fldv_GroupCode],
                [fldd_AssignedDate],
                [fldv_AssignedBy]
            )
            VALUES
            (
                @pUserCode,
                @pGroupCode,
                @pAssignedDate,
                @UserID
            );
            
            SET @RowsAffected = @@ROWCOUNT;
        END
        ELSE
        BEGIN
            SET @RowsAffected = 0;
            SET @AuditMessage = 'SMS Organizational User-Group assignment already exists for User: ' + @pUserCode + ', Group: ' + @pGroupCode;
        END
        
        COMMIT TRANSACTION;
        
        IF @RowsAffected > 0
        BEGIN
            SET @AuditMessage = 'Successfully assigned SMS Organizational User: ' + @pUserCode + ' to Group: ' + @pGroupCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        END
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error assigning SMS Organizational User: ' + @pUserCode + ' to Group: ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 2. Remove User from Organizational Group

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalUserGroup_Remove]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalUserGroup_Remove
-- Description: Remove a user from an SMS organizational group
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUserGroup_Remove]
    @pUserCode VARCHAR(60),
    @pGroupCode VARCHAR(60),
    @pRemovedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pRemovedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSOrganizationalUserGroup_Remove', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting SMS Organizational User-Group removal for User: ' + @pUserCode + ', Group: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        DELETE FROM [dbo].[tblr_SMSOrganizationalUserGroups]
        WHERE [fldv_UserCode] = @pUserCode 
          AND [fldv_GroupCode] = @pGroupCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully removed SMS Organizational User: ' + @pUserCode + ' from Group: ' + @pGroupCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error removing SMS Organizational User: ' + @pUserCode + ' from Group: ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 3. Get Users by Organizational Group Code

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalUserGroup_GetUsersByGroup]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalUserGroup_GetUsersByGroup
-- Description: Get all users assigned to a specific SMS organizational group
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUserGroup_GetUsersByGroup]
    @pGroupCode VARCHAR(60),
    @pActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.[fldv_Code],
        u.[fldv_FirstName],
        u.[fldv_LastName],
        u.[fldv_UserName],
        u.[fldv_Password],
        u.[fldv_Department],
        u.[fldv_Position],
        u.[fldv_OrganizationLevel],
        u.[fldv_SMSRole],
        u.[fldv_AuthorityLevel],
        u.[fldv_RiskApprovalAuthority],
        u.[fldb_IsActive],
        u.[fldd_LastLoginDate],
        u.[fldv_CreatedBy],
        u.[fldd_CreatedDate],
        u.[fldv_UpdatedBy],
        u.[fldd_UpdatedDate],
        ug.[fldd_AssignedDate],
        ug.[fldv_AssignedBy]
    FROM [dbo].[tbld_SMSOrganizationalUsers] u
    INNER JOIN [dbo].[tblr_SMSOrganizationalUserGroups] ug 
        ON u.[fldv_Code] = ug.[fldv_UserCode]
    WHERE ug.[fldv_GroupCode] = @pGroupCode
      AND (@pActiveOnly = 0 OR u.[fldb_IsActive] = 1)
    ORDER BY u.[fldv_LastName], u.[fldv_FirstName];
END;
```

### 4. Clear User Group Memberships

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalUserGroup_ClearUserGroups]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalUserGroup_ClearUserGroups
-- Description: Remove all group memberships for a specific SMS organizational user
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUserGroup_ClearUserGroups]
    @pUserCode VARCHAR(60),
    @pClearedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pClearedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSOrganizationalUserGroup_ClearUserGroups', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting SMS Organizational User group clearance for User: ' + @pUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        DELETE FROM [dbo].[tblr_SMSOrganizationalUserGroups]
        WHERE [fldv_UserCode] = @pUserCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully cleared SMS Organizational User groups for User: ' + @pUserCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error clearing SMS Organizational User groups for User: ' + @pUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 5. Get SMS Organizational Groups by User Code

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalGroup_GetByUserCode]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalGroup_GetByUserCode
-- Description: Get all SMS organizational groups for a specific user
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalGroup_GetByUserCode]
    @pUserCode VARCHAR(60),
    @pActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        g.[fldv_Code],
        g.[fldv_GroupName],
        g.[fldv_Description],
        g.[fldv_GroupType],
        g.[fldv_AuthorityLevel],
        g.[fldb_IsActive],
        g.[fldv_CreatedBy],
        g.[fldd_CreatedDate],
        g.[fldv_UpdatedBy],
        g.[fldd_UpdatedDate],
        ug.[fldd_AssignedDate],
        ug.[fldv_AssignedBy]
    FROM [dbo].[tbld_SMSOrganizationalGroups] g
    INNER JOIN [dbo].[tblr_SMSOrganizationalUserGroups] ug 
        ON g.[fldv_Code] = ug.[fldv_GroupCode]
    WHERE ug.[fldv_UserCode] = @pUserCode
      AND (@pActiveOnly = 0 OR g.[fldb_IsActive] = 1)
    ORDER BY g.[fldv_GroupName];
END;
```

---

## Enhanced SMS Organizational User Procedures

### 1. Enhanced Insert Procedure

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalUser_Insert]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalUser_Insert
-- Description: Create a new SMS organizational user with enhanced SMS role fields
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUser_Insert]
    @pUserCode VARCHAR(60),
    @pFirstName VARCHAR(100),
    @pLastName VARCHAR(100),
    @pUserName VARCHAR(100),
    @pPassword VARCHAR(255),
    @pDepartment VARCHAR(100),
    @pPosition VARCHAR(100),
    @pOrganizationLevel VARCHAR(100),
    @pSMSRole VARCHAR(100) = NULL,
    @pAuthorityLevel VARCHAR(100) = NULL,
    @pRiskApprovalAuthority VARCHAR(100) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewUserCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSOrganizationalUser_Insert', @RowsAffected INT;
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting SMS Organizational User creation for Code: ' + @pUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Users', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check for duplicate code
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalUsers] WHERE [fldv_Code] = @pUserCode)
        BEGIN
            RAISERROR('SMS Organizational user code already exists: %s', 16, 1, @pUserCode);
        END
        
        -- Check for duplicate username
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalUsers] WHERE [fldv_UserName] = @pUserName)
        BEGIN
            RAISERROR('SMS Organizational username already exists: %s', 16, 1, @pUserName);
        END
        
        EXEC [pr_GenerateFormattedCode] 
        @EntityName = 'SMSOrganizationalUser',           -- Uses registry: Table='tbld_SMSOrganizationalUsers', Prefix='OU'
        @GeneratedCode = @pNewUserCode OUTPUT;

        INSERT INTO [dbo].[tbld_SMSOrganizationalUsers]
        (
            [fldv_Code],
            [fldv_FirstName],
            [fldv_LastName],
            [fldv_UserName],
            [fldv_Password],
            [fldv_Department],
            [fldv_Position],
            [fldv_OrganizationLevel],
            [fldv_SMSRole],
            [fldv_AuthorityLevel],
            [fldv_RiskApprovalAuthority],
            [fldb_IsActive],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES
        (
            @pNewUserCode,
            @pFirstName,
            @pLastName,
            @pUserName,
            @pPassword,
            @pDepartment,
            @pPosition,
            @pOrganizationLevel,
            @pSMSRole,
            @pAuthorityLevel,
            @pRiskApprovalAuthority,
            1, -- Active by default
            @UserID,
            @pCreatedDate
        );
        
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully created SMS Organizational User Code: ' + @pNewUserCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Users', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error creating SMS Organizational User Code ' + @pUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Users', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 2. Enhanced Update Procedure

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalUser_Update]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalUser_Update
-- Description: Update an existing SMS organizational user with enhanced SMS role fields
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUser_Update]
    @pUserCode VARCHAR(60),
    @pFirstName VARCHAR(100),
    @pLastName VARCHAR(100),
    @pUserName VARCHAR(100),
    @pPassword VARCHAR(255) = NULL,
    @pDepartment VARCHAR(100),
    @pPosition VARCHAR(100),
    @pOrganizationLevel VARCHAR(100),
    @pSMSRole VARCHAR(100) = NULL,
    @pAuthorityLevel VARCHAR(100) = NULL,
    @pRiskApprovalAuthority VARCHAR(100) = NULL,
    @pIsActive BIT,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSOrganizationalUser_Update', @RowsAffected INT;
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting SMS Organizational User update for Code: ' + @pUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Users', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalUsers] WHERE [fldv_Code] = @pUserCode)
        BEGIN
            RAISERROR('SMS Organizational user code not found: %s', 16, 1, @pUserCode);
        END
        
        -- Check for duplicate username (excluding current user)
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalUsers] WHERE [fldv_UserName] = @pUserName AND [fldv_Code] <> @pUserCode)
        BEGIN
            RAISERROR('SMS Organizational username already exists: %s', 16, 1, @pUserName);
        END
        
        UPDATE [dbo].[tbld_SMSOrganizationalUsers]
        SET 
            [fldv_FirstName] = @pFirstName,
            [fldv_LastName] = @pLastName,
            [fldv_UserName] = @pUserName,
            [fldv_Password] = ISNULL(@pPassword, [fldv_Password]), -- Only update if provided
            [fldv_Department] = @pDepartment,
            [fldv_Position] = @pPosition,
            [fldv_OrganizationLevel] = @pOrganizationLevel,
            [fldv_SMSRole] = @pSMSRole,
            [fldv_AuthorityLevel] = @pAuthorityLevel,
            [fldv_RiskApprovalAuthority] = @pRiskApprovalAuthority,
            [fldb_IsActive] = @pIsActive,
            [fldv_UpdatedBy] = @UserID,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_Code] = @pUserCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully updated SMS Organizational User Code: ' + @pUserCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Users', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating SMS Organizational User Code ' + @pUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Users', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 3. Enhanced GetAll Procedure

```sql
USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_SMSOrganizationalUser_GetAll]    Script Date: 12/17/2024 2:29:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Procedure: pr_SMSOrganizationalUser_GetAll
-- Description: Get all SMS organizational users with enhanced SMS role fields
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUser_GetAll]
    @pActiveOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName],
        [fldv_UserName],
        [fldv_Password],
        [fldv_Department],
        [fldv_Position],
        [fldv_OrganizationLevel],
        [fldv_SMSRole],
        [fldv_AuthorityLevel],
        [fldv_RiskApprovalAuthority],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSOrganizationalUsers]
    WHERE (@pActiveOnly = 0 OR [fldb_IsActive] = 1)
    ORDER BY [fldv_LastName], [fldv_FirstName];
END;
```

---

## Sample Data and Indexes

### 1. Default Organizational Groups

```sql
-- Insert default organizational groups
INSERT INTO [dbo].[tbld_SMSOrganizationalGroups] 
(
    [fldv_Code], 
    [fldv_GroupName], 
    [fldv_Description], 
    [fldv_GroupType], 
    [fldv_AuthorityLevel],
    [fldb_IsActive], 
    [fldv_CreatedBy], 
    [fldd_CreatedDate]
)
VALUES 
    ('OG-20240101-EXEC01', 'Executive Team', 'Senior executive leadership', 'Management Team', 'Executive', 1, 'SYSTEM', GETUTCDATE()),
    ('OG-20240101-SAFETY01', 'Safety Department', 'SMS safety management department', 'Department', 'Operational', 1, 'SYSTEM', GETUTCDATE()),
    ('OG-20240101-OPS01', 'Operations Department', 'Airport operations department', 'Department', 'Operational', 1, 'SYSTEM', GETUTCDATE()),
    ('OG-20240101-MAINT01', 'Maintenance Department', 'Facility and equipment maintenance', 'Department', 'Process', 1, 'SYSTEM', GETUTCDATE()),
    ('OG-20240101-COMM01', 'Safety Committee', 'SMS safety review committee', 'Committee', 'Strategic', 1, 'SYSTEM', GETUTCDATE()),
    ('OG-20240101-MGMT01', 'Senior Management', 'Senior management oversight', 'Management Team', 'Strategic', 1, 'SYSTEM', GETUTCDATE()),
    ('OG-20240101-AUDIT01', 'Audit Department', 'Internal SMS auditing and compliance', 'Department', 'Process', 1, 'SYSTEM', GETUTCDATE()),
    ('OG-20240101-TRAIN01', 'Training Department', 'SMS training and development', 'Department', 'Support', 1, 'SYSTEM', GETUTCDATE());
GO
```

### 2. Performance Indexes

```sql
-- Add indexes for better query performance
CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalGroups_Name] 
ON [dbo].[tbld_SMSOrganizationalGroups] ([fldv_GroupName]);

CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalGroups_IsActive] 
ON [dbo].[tbld_SMSOrganizationalGroups] ([fldb_IsActive]);

CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalGroups_GroupType] 
ON [dbo].[tbld_SMSOrganizationalGroups] ([fldv_GroupType]);

CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalGroups_AuthorityLevel] 
ON [dbo].[tbld_SMSOrganizationalGroups] ([fldv_AuthorityLevel]);

CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalUserGroups_UserCode] 
ON [dbo].[tblr_SMSOrganizationalUserGroups] ([fldv_UserCode]);

CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalUserGroups_GroupCode] 
ON [dbo].[tblr_SMSOrganizationalUserGroups] ([fldv_GroupCode]);

CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalUserGroups_AssignedDate] 
ON [dbo].[tblr_SMSOrganizationalUserGroups] ([fldd_AssignedDate]);

-- Enhanced indexes for SMS Organizational Users
CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalUsers_SMSRole] 
ON [dbo].[tbld_SMSOrganizationalUsers] ([fldv_SMSRole]) 
WHERE [fldv_SMSRole] IS NOT NULL;

CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalUsers_AuthorityLevel] 
ON [dbo].[tbld_SMSOrganizationalUsers] ([fldv_AuthorityLevel]) 
WHERE [fldv_AuthorityLevel] IS NOT NULL;

CREATE NONCLUSTERED INDEX [IX_SMSOrganizationalUsers_Department] 
ON [dbo].[tbld_SMSOrganizationalUsers] ([fldv_Department]);
```

### 3. Data Validation and Constraints

```sql
-- Add check constraints for data validation
ALTER TABLE [dbo].[tbld_SMSOrganizationalGroups]
ADD CONSTRAINT [CK_SMSOrganizationalGroups_GroupType] 
CHECK ([fldv_GroupType] IN ('Department', 'SMS Role', 'Committee', 'Work Group', 'Management Team'));

ALTER TABLE [dbo].[tbld_SMSOrganizationalGroups]
ADD CONSTRAINT [CK_SMSOrganizationalGroups_AuthorityLevel] 
CHECK ([fldv_AuthorityLevel] IN ('Strategic', 'Executive', 'Operational', 'Process', 'Support', 'Standard'));

-- Ensure group names are unique
ALTER TABLE [dbo].[tbld_SMSOrganizationalGroups]
ADD CONSTRAINT [UQ_SMSOrganizationalGroups_GroupName] 
UNIQUE ([fldv_GroupName]);
```

---

## Execution Order

To implement these changes, execute the scripts in this order:

1. **Database Tables** - Create the new tables and modify existing ones
2. **SMS Organizational Group Procedures** - Core group management
3. **User-Group Management Procedures** - Group membership operations
4. **Enhanced SMS Organizational User Procedures** - Updated user procedures
5. **Sample Data and Indexes** - Performance optimizations and default data

---

## Notes

- All procedures follow the existing naming conventions and patterns used in the PDXSMS_V2 system
- **Full error handling and transaction management** with proper rollback on failure
- **Comprehensive audit logging** for all CRUD operations using `sp_AddAuditLogEntry`
- **Automatic code generation** using `pr_GenerateFormattedCode` for consistency
- **Duplicate validation** for codes and names to prevent conflicts
- The structure mirrors the existing StakeholderGroups and ApplicationGroups functionality
- Backward compatibility is maintained for existing SMS Organizational Users
- The new SMS role fields (SMSRole, AuthorityLevel, RiskApprovalAuthority) are optional and nullable
- Foreign key constraints ensure data integrity between users and groups
- Indexes are optimized for common query patterns in the Blazor application
- **Relationship table uses `tblr_` prefix** following workspace naming conventions