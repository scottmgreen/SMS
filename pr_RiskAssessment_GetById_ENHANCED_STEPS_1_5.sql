USE [PDXSMS_V2]
GO

/****** Object:  StoredProcedure [dbo].[pr_RiskAssessment_GetById]    Script Date: Enhanced for Steps 1-5 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Enhanced Stored Procedure: pr_RiskAssessment_GetById
-- Description: Retrieves a complete Risk Assessment with available data
-- Version: Enhanced for Steps 1-5 Risk Assessment Wizard (Schema-Compatible)
-- Author: SMS Application Layer Integration
-- Created: Enhanced for comprehensive risk assessment workflow
-- =============================================

ALTER PROCEDURE [dbo].[pr_RiskAssessment_GetById]
    @pID VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_RiskAssessment_GetById';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Enhanced RiskAssessment with ID: ' + @pID;
        --EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_RiskAssessments', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- =============================================
        -- Main Risk Assessment Query - Current Schema Compatible
        -- =============================================
        SELECT 
            -- Core Identification Fields (Current Schema)
            [fldi_ID],
            [fldv_Code],
            [fldv_Name],
            [fldv_Description],
            [fldv_HazardCode],
            [fldv_AssessmentType],
            [fldv_Status],
            [fldv_Stage],
            
            -- Enhanced Core Assessment Fields (Add these when schema is updated)
            -- For now, these will be NULL until you add the columns
            CAST(NULL AS VARCHAR(100)) AS [fldv_LeadAssessorId],
            CAST(NULL AS VARCHAR(60)) AS [fldv_PrimaryHazardId],
            CAST(NULL AS VARCHAR(50)) AS [fldv_RiskAssessmentCategory],
            CAST(NULL AS INT) AS [fldi_CurrentStep],
            CAST(NULL AS DATETIME) AS [fldd_CompletedDate],
            CAST(NULL AS VARCHAR(100)) AS [fldv_CompletedBy],
            CAST(NULL AS VARCHAR(60)) AS [fldv_ParentAssessmentId],
            
            -- Step 1: System Description Fields (Add these when schema is updated)
            CAST(NULL AS VARCHAR(2000)) AS [fldv_SystemDescription],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_SystemBoundaries],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_SystemPurpose],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_PersonnelFactors],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_EquipmentFactors],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_ProcedureFactors],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_ResourceFactors],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_EnvironmentFactors],
            
            -- Step 3: Risk Analysis Fields (Add these when schema is updated)
            CAST(NULL AS VARCHAR(200)) AS [fldv_RiskAnalysisMethod],
            CAST(NULL AS VARCHAR(1000)) AS [fldv_RiskCriteria],
            
            -- Step 4: Risk Assessment Fields (Add these when schema is updated)
            CAST(NULL AS VARCHAR(200)) AS [fldv_TolerabilityFramework],
            CAST(NULL AS VARCHAR(1000)) AS [fldv_RiskAcceptanceCriteria],
            CAST(NULL AS INT) AS [fldi_FinalSeverityScore],
            CAST(NULL AS INT) AS [fldi_FinalLikelihoodScore],
            CAST(NULL AS VARCHAR(50)) AS [fldv_FinalRiskLevel],
            CAST(NULL AS VARCHAR(50)) AS [fldv_RiskTolerability],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_AssessmentRationale],
            
            -- Step 5: Implementation Fields (Add these when schema is updated)
            CAST(NULL AS VARCHAR(2000)) AS [fldv_ImplementationStrategy],
            CAST(NULL AS DATETIME) AS [fldd_OverallTargetDate],
            CAST(NULL AS VARCHAR(2000)) AS [fldv_ImplementationNotes],
            
            -- Progress Tracking Fields (Add these when schema is updated)
            CAST(NULL AS VARCHAR(50)) AS [fldv_CompletedSteps],
            CAST(NULL AS INT) AS [fldi_CompletionPercentage],
            
            -- Audit Fields (These should exist in current schema)
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_RiskAssessments] 
        WHERE [fldv_Code] = @pID;
        
        -- =============================================
        -- Related Data Queries - Current Schema Compatible
        -- =============================================
        
        -- Get Identified Hazards for this Risk Assessment (Current Schema)
        SELECT 
            H.[fldv_Code] AS HazardCode,
            H.[fldv_Name] AS HazardName,
            H.[fldc_Description] AS HazardDescription,
            H.[fldv_ReportCode] AS ReportCode,
            H.[fldv_ScoringPanelCode] AS ScoringPanelCode,
            H.[fldv_AverageScore] AS AverageScore,
            
            -- Enhanced fields (NULL until schema updated)
            CAST(NULL AS VARCHAR(100)) AS HazardType,
            CAST(NULL AS VARCHAR(100)) AS HazardCategory,
            CAST(NULL AS VARCHAR(50)) AS HazardPriority,
            CAST(NULL AS VARCHAR(50)) AS HazardRiskLevel,
            CAST(NULL AS VARCHAR(2000)) AS WorstCredibleOutcome,
            CAST(NULL AS VARCHAR(2000)) AS RootCauseAnalysis,
            CAST(NULL AS VARCHAR(50)) AS HazardStatus
        FROM [dbo].[tbld_Hazards] H
        WHERE H.[fldv_Code] IN (
            -- Link via the current HazardCode field
            SELECT RTRIM(RA.[fldv_HazardCode]) 
            FROM [dbo].[tbld_RiskAssessments] RA 
            WHERE RA.[fldv_Code] = @pID
            AND RA.[fldv_HazardCode] IS NOT NULL
        );
        
        -- Get Panel Scoring Data (Current Schema - Note: table name is tbld_ScoringPanel, not tbld_ScoringPanels)
        SELECT 
            SP.[fldv_Code] AS ScoringPanelCode,
            RTRIM(SP.[fldv_HazardCode]) AS HazardCode,
            RTRIM(SP.[fldv_SMSUserCode]) AS PanelMemberCode,
            RTRIM(SP.[fldv_Severity]) AS SeverityScore,
            RTRIM(SP.[fldv_Likelyhood]) AS LikelihoodScore, -- Note: keeping existing misspelling
            RTRIM(SP.[fldv_Score]) AS CalculatedScore,
            
            -- Get panel member name if SMSApplicationUsers table exists and is properly linked
            COALESCE(AU.[fldv_FirstName] + ' ' + AU.[fldv_LastName], 
                     RTRIM(SP.[fldv_SMSUserCode]), 
                     'Unknown User') AS PanelMemberName,
            GETDATE() AS ScoreSubmittedDate -- Placeholder since no timestamp in current schema
        FROM [dbo].[tbld_ScoringPanel] SP
        LEFT JOIN [dbo].[tbld_SMSApplicationUsers] AU ON RTRIM(SP.[fldv_SMSUserCode]) = AU.[fldv_Code]
        WHERE RTRIM(SP.[fldv_HazardCode]) IN (
            SELECT RTRIM(RA.[fldv_HazardCode]) 
            FROM [dbo].[tbld_RiskAssessments] RA 
            WHERE RA.[fldv_Code] = @pID
            AND RA.[fldv_HazardCode] IS NOT NULL
        )
        ORDER BY SP.[fldv_HazardCode];
        
        -- Get Mitigation Strategies (Placeholder - implement when mitigation tables exist)
        SELECT 
            'MITIGATION_PLACEHOLDER' AS MitigationCode,
            'Mitigation data retrieval needs to be implemented when tbld_Mitigations table exists' AS Note,
            @pID AS RelatedAssessmentId;
        
        -- Get Stakeholder Assignments (Placeholder - implement based on your stakeholder linking)
        SELECT 
            'STAKEHOLDER_PLACEHOLDER' AS StakeholderType,
            'Stakeholder data retrieval needs to be implemented based on your schema' AS Note,
            @pID AS RelatedAssessmentId;
        
        SET @AuditMessage = 'Successfully retrieved Enhanced RiskAssessment with ID: ' + @pID;
        --EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_RiskAssessments', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Enhanced RiskAssessment ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_RiskAssessments', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

/* 
============================================= 
CURRENT SCHEMA COMPATIBILITY NOTES:
============================================= 

? WORKS WITH CURRENT SCHEMA:
- Uses existing tbld_RiskAssessments fields
- Compatible with tbld_Hazards current structure
- Corrected table name: tbld_ScoringPanel (not tbld_ScoringPanels)
- Uses RTRIM() for nchar(10) fields to remove padding
- LEFT JOIN for safer user name retrieval

?? ENHANCED FIELDS (Ready for Schema Update):
- Returns NULL for Steps 1-5 fields not yet in database
- Uses CAST(NULL AS...) to maintain consistent result structure
- Ready to be activated when you add the enhanced columns

?? FIXES APPLIED:
- Corrected scoring panel table name
- Added RTRIM() for fixed-length character fields
- Safer LEFT JOIN for user lookups
- Proper handling of current schema limitations

?? NEXT STEPS TO FULL ENHANCEMENT:
1. Run your ALTER_EXISTING_TABLES_STEPS_1_5.sql script
2. Update the NULL CAST statements to actual column names
3. Implement mitigation and stakeholder table queries
4. Test with your RiskAssessmentRepository

This version works with your CURRENT schema while being ready for enhancement!
*/