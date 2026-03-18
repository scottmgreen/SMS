-- 2FA stored procedures for SMS Organizational Users
USE [SMS]
GO

-- ================================================================================
-- PROCEDURE: Setup 2FA for Organizational User
-- ================================================================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUser_Setup2FA]
    @pUserCode VARCHAR(60),
    @pSecretKey NVARCHAR(255),
    @pBackupCodes NVARCHAR(MAX) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM-2FA'
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @pUserCode IS NULL OR @pSecretKey IS NULL
    BEGIN
        RAISERROR('UserCode and SecretKey are required', 16, 1);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSOrganizationalUsers]
    SET 
        [fldv_TwoFactorSecretKey] = @pSecretKey,
        [fldb_TwoFactorEnabled] = 1,
        [fldv_BackupCodes] = @pBackupCodes,
        [fldd_TwoFactorSetupDate] = GETUTCDATE(),
        [fldi_FailedTwoFactorAttempts] = 0,
        [fldd_TwoFactorLockedUntil] = NULL,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = GETUTCDATE()
    WHERE [fldv_Code] = @pUserCode;
    
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Organizational User not found: %s', 16, 1, @pUserCode);
        RETURN;
    END
END
GO

-- ================================================================================
-- PROCEDURE: Reset 2FA Failed Attempts for Organizational User
-- ================================================================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUser_Reset2FAFailedAttempts]
    @pUserCode VARCHAR(60),
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM-2FA'
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @pUserCode IS NULL
    BEGIN
        RAISERROR('UserCode is required', 16, 1);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSOrganizationalUsers]
    SET 
        [fldi_FailedTwoFactorAttempts] = 0,
        [fldd_TwoFactorLockedUntil] = NULL,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = GETUTCDATE()
    WHERE [fldv_Code] = @pUserCode;
    
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Organizational User not found: %s', 16, 1, @pUserCode);
        RETURN;
    END
END
GO

-- ================================================================================
-- PROCEDURE: Update 2FA Failed Attempts for Organizational User
-- ================================================================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSOrganizationalUser_Update2FAFailedAttempts]
    @pUserCode VARCHAR(60),
    @pFailedAttempts INT,
    @pLockoutUntil DATETIME2(7) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM-2FA'
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @pUserCode IS NULL
    BEGIN
        RAISERROR('UserCode is required', 16, 1);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSOrganizationalUsers]
    SET 
        [fldi_FailedTwoFactorAttempts] = @pFailedAttempts,
        [fldd_TwoFactorLockedUntil] = @pLockoutUntil,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = GETUTCDATE()
    WHERE [fldv_Code] = @pUserCode;
    
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Organizational User not found: %s', 16, 1, @pUserCode);
        RETURN;
    END
END
GO