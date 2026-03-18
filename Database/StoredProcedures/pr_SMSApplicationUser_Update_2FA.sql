-- Updated stored procedure with 2FA support
USE [SMS]
GO

ALTER PROCEDURE [dbo].[pr_SMSApplicationUser_Update]
    @pID VARCHAR(60),
    @pCode NVARCHAR(50),
    @pFirstName NVARCHAR(100),
    @pLastName NVARCHAR(100),
    @pUserName NVARCHAR(50),
    @pApplicationUserType NVARCHAR(100),
    @pSMSUserRole NVARCHAR(50),
    @pIsActive BIT,
    @pLastLoginDate DATETIME2(7) = NULL,
    -- ?? NEW 2FA PARAMETERS
    @pTwoFactorSecretKey NVARCHAR(255) = NULL,
    @pTwoFactorEnabled BIT = NULL,
    @pBackupCodes NVARCHAR(MAX) = NULL,
    @pTwoFactorSetupDate DATETIME2(7) = NULL,
    @pFailedTwoFactorAttempts INT = NULL,
    @pTwoFactorLockedUntil DATETIME2(7) = NULL,
    -- Standard audit fields
    @pUpdatedBy NVARCHAR(50),
    @pUpdatedDate DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters (existing validation...)
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
    
    IF @pSMSUserRole IS NULL OR LTRIM(RTRIM(@pSMSUserRole)) = ''
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
    
    -- UPDATE with 2FA fields (only update if parameter is provided)
    UPDATE [dbo].[tbld_SMSApplicationUsers]
    SET 
        [fldv_Code] = @pCode,
        [fldv_FirstName] = @pFirstName,
        [fldv_LastName] = @pLastName,
        [fldv_UserName] = @pUserName,
        [fldv_ApplicationUserTypeCode] = @pApplicationUserType,
        [fldv_SMSUserRoleCode] = @pSMSUserRole,
        [fldb_IsActive] = @pIsActive,
        [fldd_LastLoginDate] = @pLastLoginDate,
        -- ?? 2FA FIELDS - Only update if parameters are provided
        [fldv_TwoFactorSecretKey] = CASE WHEN @pTwoFactorSecretKey IS NOT NULL THEN @pTwoFactorSecretKey ELSE [fldv_TwoFactorSecretKey] END,
        [fldb_TwoFactorEnabled] = CASE WHEN @pTwoFactorEnabled IS NOT NULL THEN @pTwoFactorEnabled ELSE [fldb_TwoFactorEnabled] END,
        [fldv_BackupCodes] = CASE WHEN @pBackupCodes IS NOT NULL THEN @pBackupCodes ELSE [fldv_BackupCodes] END,
        [fldd_TwoFactorSetupDate] = CASE WHEN @pTwoFactorSetupDate IS NOT NULL THEN @pTwoFactorSetupDate ELSE [fldd_TwoFactorSetupDate] END,
        [fldi_FailedTwoFactorAttempts] = CASE WHEN @pFailedTwoFactorAttempts IS NOT NULL THEN @pFailedTwoFactorAttempts ELSE [fldi_FailedTwoFactorAttempts] END,
        [fldd_TwoFactorLockedUntil] = CASE WHEN @pTwoFactorLockedUntil IS NOT NULL THEN @pTwoFactorLockedUntil ELSE [fldd_TwoFactorLockedUntil] END,
        -- Standard audit fields
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