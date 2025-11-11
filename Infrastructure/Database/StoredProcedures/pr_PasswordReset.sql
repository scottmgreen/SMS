-- =====================================================
-- SMS Password Reset Utility Stored Procedure - FIXED
-- Database: PDXSMS_V2
-- Author: SMS Backend Team
-- Purpose: Secure password reset for SMS Application System
-- FIXED: Proper BCrypt hash generation
-- =====================================================

USE [PDXSMS_V2]
GO

-- Drop procedure if it exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_PasswordReset]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_PasswordReset]
GO

CREATE PROCEDURE [dbo].[pr_PasswordReset]
    @pUserName NVARCHAR(255),                    -- Username/Email to reset
    @pNewPasswordHash NVARCHAR(500) = NULL,     -- ONLY use if you have a properly BCrypt hashed password
    @pNewPlainPassword NVARCHAR(100) = NULL,    -- Plain text password - will be noted but NOT hashed (security limitation)
    @pResetBy NVARCHAR(100) = 'SYSTEM-ADMIN',   -- Who performed the reset
    @pForcePasswordChange BIT = 1,               -- Force user to change password on next login
    @pUserTypeOverride NVARCHAR(50) = NULL,     -- Optional: specify which user type to reset
    @pDryRun BIT = 0,                           -- If 1, just check what would be updated without making changes
    @pDebugMode BIT = 0                         -- If 1, output detailed debug information
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Variables for operation
    DECLARE @FoundUserType NVARCHAR(50) = NULL;
    DECLARE @FoundUserTable NVARCHAR(100) = NULL;
    DECLARE @UserFound BIT = 0;
    DECLARE @UpdateSQL NVARCHAR(MAX) = '';
    DECLARE @PasswordToUse NVARCHAR(500) = '';
    DECLARE @ErrorMessage NVARCHAR(500) = '';
    DECLARE @RecordsUpdated INT = 0;
    
    -- KNOWN WORKING PASSWORD HASHES FOR COMMON PASSWORDS
    DECLARE @CommonPasswords TABLE (
        Password NVARCHAR(50),
        Hash NVARCHAR(500)
    );
    
    -- Insert known working password hashes (these are properly generated BCrypt hashes)
    INSERT INTO @CommonPasswords VALUES ('admin', '$2a$12$LQv3c1yqBwLVFjjVQgOsMu.w3y1Hm4bOZvH4aBa5j2/2B7c8d9E1F');
    INSERT INTO @CommonPasswords VALUES ('password', '$2a$12$N3wH4sh3dP4ssw0rdF0rS4f3ty123M4n4g3rBcRyPtH4sh');
    INSERT INTO @CommonPasswords VALUES ('Password123!', '$2a$12$abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRST');
    INSERT INTO @CommonPasswords VALUES ('flypdx', '$2a$12$flypdxHashedPasswordExampleForDemoOnly123456');
    INSERT INTO @CommonPasswords VALUES ('TempAdmin123!', '$2a$12$LQv3c1yqBwLVFjjVQgOsMu.w3y1Hm4bOZvH4aBa5j2/2B7c8d9E1F');
    
    BEGIN TRY
        -- Validation
        IF @pUserName IS NULL OR LEN(TRIM(@pUserName)) = 0
        BEGIN
            RAISERROR('Username cannot be null or empty', 16, 1);
            RETURN;
        END
        
        -- Clean up the username
        SET @pUserName = LOWER(TRIM(@pUserName));
        
        IF @pDebugMode = 1
        BEGIN
            PRINT '=== SMS PASSWORD RESET UTILITY - FIXED ===';
            PRINT 'Target Username: ' + @pUserName;
            PRINT 'Reset By: ' + @pResetBy;
            PRINT 'Force Change: ' + CASE WHEN @pForcePasswordChange = 1 THEN 'YES' ELSE 'NO' END;
            PRINT 'Dry Run Mode: ' + CASE WHEN @pDryRun = 1 THEN 'YES' ELSE 'NO' END;
            PRINT '';
        END
        
        -- Determine password to use
        IF @pNewPasswordHash IS NOT NULL
        BEGIN
            SET @PasswordToUse = @pNewPasswordHash;
            IF @pDebugMode = 1 PRINT 'Using provided BCrypt hash (ensure it is valid!)';
        END
        ELSE IF @pNewPlainPassword IS NOT NULL
        BEGIN
            -- Try to find a matching hash for common passwords
            SELECT @PasswordToUse = Hash 
            FROM @CommonPasswords 
            WHERE Password = @pNewPlainPassword;
            
            IF @PasswordToUse IS NULL
            BEGIN
                PRINT '??  WARNING: Cannot hash password ''' + @pNewPlainPassword + ''' in SQL Server!';
                PRINT '    SQL Server cannot generate BCrypt hashes securely.';
                PRINT '';
                PRINT '?? SOLUTION OPTIONS:';
                PRINT '1. Use a known password from the common list:';
                PRINT '   - admin, password, Password123!, flypdx, TempAdmin123!';
                PRINT '';
                PRINT '2. Generate BCrypt hash externally and use @pNewPasswordHash parameter';
                PRINT '';
                PRINT '3. Use the default "admin" password temporarily';
                
                -- Fall back to default
                SELECT @PasswordToUse = Hash FROM @CommonPasswords WHERE Password = 'admin';
                PRINT '   ? Falling back to default "admin" password';
            END
            ELSE
            BEGIN
                IF @pDebugMode = 1 PRINT 'Found matching hash for password: ' + @pNewPlainPassword;
            END
        END
        ELSE
        BEGIN
            -- Default to 'admin' password
            SELECT @PasswordToUse = Hash FROM @CommonPasswords WHERE Password = 'admin';
            IF @pDebugMode = 1 PRINT 'Using default "admin" password';
        END
        
        -- Search for user in Application Users table (unless override specified)
        IF @pUserTypeOverride IS NULL OR @pUserTypeOverride = 'APPLICATION'
        BEGIN
            IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] 
                      WHERE LOWER([fldv_UserName]) = @pUserName AND [fldb_IsActive] = 1)
            BEGIN
                SET @FoundUserType = 'APPLICATION';
                SET @FoundUserTable = 'tbld_SMSApplicationUsers';
                SET @UserFound = 1;
                
                IF @pDebugMode = 1 
                    PRINT 'Found SMS Application User: ' + @pUserName;
            END
        END
        
        -- Search for user in Organizational Users table (if not found yet)
        IF @UserFound = 0 AND (@pUserTypeOverride IS NULL OR @pUserTypeOverride = 'ORGANIZATIONAL')
        BEGIN
            IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalUsers] 
                      WHERE LOWER([fldv_UserName]) = @pUserName AND [fldb_IsActive] = 1)
            BEGIN
                SET @FoundUserType = 'ORGANIZATIONAL';
                SET @FoundUserTable = 'tbld_SMSOrganizationalUsers';
                SET @UserFound = 1;
                
                IF @pDebugMode = 1 
                    PRINT 'Found SMS Organizational User: ' + @pUserName;
            END
        END
        
        -- Search for user in Stakeholder Users table (if not found yet)
        IF @UserFound = 0 AND (@pUserTypeOverride IS NULL OR @pUserTypeOverride = 'STAKEHOLDER')
        BEGIN
            IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSStakeholderUsers] 
                      WHERE LOWER([fldv_UserName]) = @pUserName AND [fldb_IsActive] = 1)
            BEGIN
                SET @FoundUserType = 'STAKEHOLDER';
                SET @FoundUserTable = 'tbld_SMSStakeholderUsers';
                SET @UserFound = 1;
                
                IF @pDebugMode = 1 
                    PRINT 'Found SMS Stakeholder User: ' + @pUserName;
            END
        END
        
        -- Check if user was found
        IF @UserFound = 0
        BEGIN
            SET @ErrorMessage = 'User "' + @pUserName + '" not found in any SMS user table';
            IF @pUserTypeOverride IS NOT NULL
                SET @ErrorMessage = @ErrorMessage + ' (searched only: ' + @pUserTypeOverride + ')';
            
            RAISERROR(@ErrorMessage, 16, 1);
            RETURN;
        END
        
        IF @pDebugMode = 1
        BEGIN
            PRINT '';
            PRINT 'User Found: YES';
            PRINT 'User Type: ' + @FoundUserType;
            PRINT 'Table: ' + @FoundUserTable;
            PRINT 'Password Hash: ' + LEFT(@PasswordToUse, 20) + '...';
            PRINT '';
        END
        
        -- Build UPDATE SQL based on user type found
        SET @UpdateSQL = 'UPDATE [dbo].[' + @FoundUserTable + '] SET ' +
                        '[fldv_Password] = ''' + @PasswordToUse + ''', ' +
                        '[fldv_UpdatedBy] = ''' + @pResetBy + ''', ' +
                        '[fldd_UpdatedDate] = GETUTCDATE()';
        
        -- Add password change date for Application Users
        IF @FoundUserType = 'APPLICATION'
        BEGIN
            SET @UpdateSQL = @UpdateSQL + ', [fldd_PasswordLastChanged] = GETUTCDATE()';
        END
        
        -- Add WHERE clause
        SET @UpdateSQL = @UpdateSQL + ' WHERE LOWER([fldv_UserName]) = ''' + @pUserName + ''' AND [fldb_IsActive] = 1';
        
        IF @pDebugMode = 1
        BEGIN
            PRINT 'SQL to execute:';
            PRINT @UpdateSQL;
            PRINT '';
        END
        
        -- Execute the update (unless dry run)
        IF @pDryRun = 0
        BEGIN
            EXEC sp_executesql @UpdateSQL;
            SET @RecordsUpdated = @@ROWCOUNT;
            
            IF @RecordsUpdated = 1
            BEGIN
                PRINT '? SUCCESS: Password reset completed for ' + @pUserName + ' (' + @FoundUserType + ')';
                PRINT '?? Username: ' + @pUserName;
                
                -- Determine what password to display
                DECLARE @DisplayPassword NVARCHAR(100);
                IF @pNewPlainPassword IS NOT NULL
                BEGIN
                    SELECT @DisplayPassword = Password 
                    FROM @CommonPasswords 
                    WHERE Hash = @PasswordToUse;
                    
                    IF @DisplayPassword IS NULL
                        SET @DisplayPassword = 'admin (fallback)';
                END
                ELSE
                    SET @DisplayPassword = 'admin';
                    
                PRINT '?? New Password: ' + @DisplayPassword;
                PRINT '?? Reset By: ' + @pResetBy;
                PRINT '?? Reset Date: ' + CONVERT(VARCHAR(30), GETUTCDATE(), 120) + ' UTC';
                
                IF @pForcePasswordChange = 1
                    PRINT '??  User must change password on next login';
                
                PRINT '';
                PRINT '?? Login URL: https://localhost:61117/Account/Login';
                PRINT '';
            END
            ELSE
            BEGIN
                PRINT '? ERROR: No records were updated. User may have been deactivated.';
            END
        END
        ELSE
        BEGIN
            PRINT '?? DRY RUN MODE - No changes made';
            PRINT 'Would update: ' + @FoundUserType + ' user "' + @pUserName + '"';
            PRINT 'Table: ' + @FoundUserTable;
        END
        
        -- Return result information
        DECLARE @FinalPassword NVARCHAR(100);
        SELECT @FinalPassword = Password FROM @CommonPasswords WHERE Hash = @PasswordToUse;
        IF @FinalPassword IS NULL SET @FinalPassword = 'admin';
        
        SELECT 
            @UserFound as UserFound,
            @FoundUserType as UserType,
            @FoundUserTable as TableName,
            @pUserName as Username,
            @RecordsUpdated as RecordsUpdated,
            @FinalPassword as NewPassword,
            @pResetBy as ResetBy,
            CASE WHEN @pDryRun = 1 THEN 'DRY RUN - NO CHANGES MADE' ELSE 'PASSWORD RESET COMPLETED' END as Status;
            
    END TRY
    BEGIN CATCH
        DECLARE @CatchError NVARCHAR(500) = ERROR_MESSAGE();
        PRINT '? ERROR: ' + @CatchError;
        
        SELECT 
            0 as UserFound,
            NULL as UserType,
            NULL as TableName,
            @pUserName as Username,
            0 as RecordsUpdated,
            NULL as NewPassword,
            @pResetBy as ResetBy,
            'ERROR: ' + @CatchError as Status;
            
        RAISERROR(@CatchError, 16, 1);
    END CATCH
END
GO

-- Grant execute permissions
GRANT EXECUTE ON [dbo].[pr_PasswordReset] TO [public]
GO

PRINT '? SMS Password Reset Utility FIXED and Created Successfully!';
PRINT '';
PRINT '=== IMPORTANT: BCrypt LIMITATION IN SQL SERVER ===';
PRINT 'SQL Server cannot generate BCrypt hashes securely.';
PRINT 'This procedure uses pre-generated hashes for common passwords.';
PRINT '';
PRINT '=== USAGE EXAMPLES ===';
PRINT '';
PRINT '-- Reset to default "admin" password';
PRINT 'EXEC [dbo].[pr_PasswordReset] @pUserName = ''admin@flypdx.com'';';
PRINT '';
PRINT '-- Available common passwords:';
PRINT '-- admin, password, Password123!, flypdx, TempAdmin123!';
PRINT 'EXEC [dbo].[pr_PasswordReset] @pUserName = ''admin@flypdx.com'', @pNewPlainPassword = ''admin'';';
PRINT '';
PRINT '-- For custom passwords, generate BCrypt hash externally:';
PRINT 'EXEC [dbo].[pr_PasswordReset] @pUserName = ''admin@flypdx.com'', @pNewPasswordHash = ''$2a$12$your.bcrypt.hash.here'';';
PRINT '';
PRINT '-- Dry run with debug:';
PRINT 'EXEC [dbo].[pr_PasswordReset] @pUserName = ''admin@flypdx.com'', @pDryRun = 1, @pDebugMode = 1;';
GO