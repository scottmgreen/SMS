-- =============================================
-- SMS Test Data Cleanup Stored Procedure
-- Truncates all SMS tables to provide clean slate for testing
-- =============================================

USE [PDXSMS_V2]
GO

-- Drop the procedure if it exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMS_TruncateAllTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[pr_SMS_TruncateAllTables]
GO

CREATE PROCEDURE [dbo].[pr_SMS_TruncateAllTables]
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        PRINT 'Starting SMS table truncation...'
        
        -- Disable foreign key constraints to avoid referential integrity issues
        EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
        
        -- Truncate all SMS tables in dependency order
        -- Start with child tables first to avoid foreign key violations
        
        -- Child tables (no dependencies)
        PRINT 'Truncating child tables...'
        TRUNCATE TABLE [dbo].[tbls_AuditLog]
        TRUNCATE TABLE [dbo].[tbld_ScoringPanel]
        TRUNCATE TABLE [dbo].[tbld_MitigationAssignments]
        TRUNCATE TABLE [dbo].[tbld_Interviews]
        TRUNCATE TABLE [dbo].[tbld_ReportValidations]
        
        -- Mid-level tables
        PRINT 'Truncating mid-level tables...'
        TRUNCATE TABLE [dbo].[tbld_RiskAssessments]
        TRUNCATE TABLE [dbo].[tbld_RiskAnalysis]
        TRUNCATE TABLE [dbo].[tbld_Mitigations]
        TRUNCATE TABLE [dbo].[tbld_Investigations]
        TRUNCATE TABLE [dbo].[tbld_AirportSharedDataset]
        
        -- Parent tables (referenced by others)
        PRINT 'Truncating parent tables...'
        TRUNCATE TABLE [dbo].[tbld_Hazards]
        TRUNCATE TABLE [dbo].[tbld_Reports]
        
        -- Re-enable foreign key constraints
        EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
        
        PRINT 'SMS table truncation completed successfully!'
        
        -- Return success indicator
        SELECT 
            'SUCCESS' AS Status,
            'All SMS tables have been truncated successfully' AS Message,
            GETUTCDATE() AS Timestamp
            
    END TRY
    BEGIN CATCH
        -- Re-enable constraints in case of error
        EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
        
        -- Return error information
        SELECT 
            'ERROR' AS Status,
            ERROR_MESSAGE() AS Message,
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_SEVERITY() AS Severity,
            ERROR_STATE() AS State,
            GETUTCDATE() AS Timestamp
            
        -- Re-throw the error
        
    END CATCH
END
GO

-- Grant execute permissions (adjust as needed for your security model)
-- GRANT EXECUTE ON [dbo].[pr_SMS_TruncateAllTables] TO [YourTestUser]

PRINT 'Stored procedure pr_SMS_TruncateAllTables created successfully!'