-- **************************************************
-- UPDATED STORED PROCEDURE FOR RISK ASSESSMENT WIZARD
-- Supports all Steps 1-5 fields in tbld_RiskAssessments
-- **************************************************
USE [PDXSMS_V2]
GO

-- Drop existing procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'pr_RiskAssessment_Update')
    DROP PROCEDURE [dbo].[pr_RiskAssessment_Update]
GO

CREATE PROCEDURE [dbo].[pr_RiskAssessment_Update]
    -- **************************************************
    -- CORE IDENTIFICATION PARAMETERS
    -- **************************************************
    @pID VARCHAR(60),
    
    -- **************************************************
    -- ORIGINAL CORE FIELDS (Updated sizes)
    -- **************************************************
    @pRiskAssessmentCode NVARCHAR(50) = NULL,
    @pRiskAssessmentName NVARCHAR(200) = NULL,
    @pRiskAssessmentDescription NTEXT = NULL,
    @pRiskAssessmentHazardCode NVARCHAR(50) = NULL,
    @pRiskAssessmentType NVARCHAR(50) = NULL,
    @pRiskAssessmentStatus NVARCHAR(50) = NULL,
    @pRiskAssessmentStage NVARCHAR(50) = NULL,
    
    -- **************************************************
    -- NEW CORE FIELDS (Steps 1-5 Support)
    -- **************************************************
    @pLeadAssessorId NVARCHAR(50) = NULL,
    @pPrimaryHazardId NVARCHAR(50) = NULL,
    @pRiskAssessmentCategory NVARCHAR(50) = NULL,
    @pCurrentStep INT = NULL,
    @pCompletedDate DATETIME = NULL,
    @pCompletedBy NVARCHAR(50) = NULL,
    @pParentAssessmentId NVARCHAR(50) = NULL,
    
    -- **************************************************
    -- STEP 1 - SYSTEM DESCRIPTION PARAMETERS
    -- **************************************************
    @pSystemDescription NTEXT = NULL,
    @pSystemBoundaries NTEXT = NULL,
    @pSystemPurpose NTEXT = NULL,
    @pPersonnelFactors NTEXT = NULL,
    @pEquipmentFactors NTEXT = NULL,
    @pProcedureFactors NTEXT = NULL,
    @pResourceFactors NTEXT = NULL,
    @pEnvironmentFactors NTEXT = NULL,
    
    -- **************************************************
    -- STEP 3 - RISK ANALYSIS PARAMETERS
    -- **************************************************
    @pRiskAnalysisMethod NVARCHAR(100) = NULL,
    @pRiskCriteria NTEXT = NULL,
    
    -- **************************************************
    -- STEP 4 - RISK ASSESSMENT PARAMETERS
    -- **************************************************
    @pTolerabilityFramework NVARCHAR(100) = NULL,
    @pRiskAcceptanceCriteria NTEXT = NULL,
    @pFinalSeverityScore INT = NULL,
    @pFinalLikelihoodScore INT = NULL,
    @pFinalRiskLevel NVARCHAR(10) = NULL,
    @pRiskTolerability NVARCHAR(50) = NULL,
    @pAssessmentRationale NTEXT = NULL,
    
    -- **************************************************
    -- STEP 5 - IMPLEMENTATION PARAMETERS
    -- **************************************************
    @pImplementationStrategy NTEXT = NULL,
    @pOverallTargetDate DATETIME = NULL,
    @pImplementationNotes NTEXT = NULL,
    
    -- **************************************************
    -- PROGRESS TRACKING PARAMETERS
    -- **************************************************
    @pCompletedSteps NVARCHAR(50) = NULL,
    @pCompletionPercentage INT = NULL,
    
    -- **************************************************
    -- AUDIT PARAMETERS
    -- **************************************************
    @pUpdatedBy VARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_RiskAssessment_Update', @RowsAffected INT;
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting RiskAssessment Steps 1-5 update for ID: ' + CAST(@pID AS VARCHAR(60));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_RiskAssessments', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- **************************************************
        -- COMPREHENSIVE UPDATE WITH ALL STEP FIELDS
        -- **************************************************
        UPDATE [dbo].[tbld_RiskAssessments] 
        SET 
            -- Original Core Fields
            [fldv_Code] = COALESCE(@pRiskAssessmentCode, [fldv_Code]),
            [fldv_Name] = COALESCE(@pRiskAssessmentName, [fldv_Name]),
            [fldv_Description] = COALESCE(@pRiskAssessmentDescription, [fldv_Description]),
            [fldv_HazardCode] = COALESCE(@pRiskAssessmentHazardCode, [fldv_HazardCode]),
            [fldv_AssessmentType] = COALESCE(@pRiskAssessmentType, [fldv_AssessmentType]),
            [fldv_Status] = COALESCE(@pRiskAssessmentStatus, [fldv_Status]),
            [fldv_Stage] = COALESCE(@pRiskAssessmentStage, [fldv_Stage]),
            
            -- New Core Fields
            [fldv_LeadAssessorId] = COALESCE(@pLeadAssessorId, [fldv_LeadAssessorId]),
            [fldv_PrimaryHazardId] = COALESCE(@pPrimaryHazardId, [fldv_PrimaryHazardId]),
            [fldv_RiskAssessmentCategory] = COALESCE(@pRiskAssessmentCategory, [fldv_RiskAssessmentCategory]),
            [fldi_CurrentStep] = COALESCE(@pCurrentStep, [fldi_CurrentStep]),
            [fldd_CompletedDate] = COALESCE(@pCompletedDate, [fldd_CompletedDate]),
            [fldv_CompletedBy] = COALESCE(@pCompletedBy, [fldv_CompletedBy]),
            [fldv_ParentAssessmentId] = COALESCE(@pParentAssessmentId, [fldv_ParentAssessmentId]),
            
            -- Step 1 Fields
            [fldv_SystemDescription] = COALESCE(@pSystemDescription, [fldv_SystemDescription]),
            [fldv_SystemBoundaries] = COALESCE(@pSystemBoundaries, [fldv_SystemBoundaries]),
            [fldv_SystemPurpose] = COALESCE(@pSystemPurpose, [fldv_SystemPurpose]),
            [fldv_PersonnelFactors] = COALESCE(@pPersonnelFactors, [fldv_PersonnelFactors]),
            [fldv_EquipmentFactors] = COALESCE(@pEquipmentFactors, [fldv_EquipmentFactors]),
            [fldv_ProcedureFactors] = COALESCE(@pProcedureFactors, [fldv_ProcedureFactors]),
            [fldv_ResourceFactors] = COALESCE(@pResourceFactors, [fldv_ResourceFactors]),
            [fldv_EnvironmentFactors] = COALESCE(@pEnvironmentFactors, [fldv_EnvironmentFactors]),
            
            -- Step 3 Fields
            [fldv_RiskAnalysisMethod] = COALESCE(@pRiskAnalysisMethod, [fldv_RiskAnalysisMethod]),
            [fldv_RiskCriteria] = COALESCE(@pRiskCriteria, [fldv_RiskCriteria]),
            
            -- Step 4 Fields
            [fldv_TolerabilityFramework] = COALESCE(@pTolerabilityFramework, [fldv_TolerabilityFramework]),
            [fldv_RiskAcceptanceCriteria] = COALESCE(@pRiskAcceptanceCriteria, [fldv_RiskAcceptanceCriteria]),
            [fldi_FinalSeverityScore] = COALESCE(@pFinalSeverityScore, [fldi_FinalSeverityScore]),
            [fldi_FinalLikelihoodScore] = COALESCE(@pFinalLikelihoodScore, [fldi_FinalLikelihoodScore]),
            [fldv_FinalRiskLevel] = COALESCE(@pFinalRiskLevel, [fldv_FinalRiskLevel]),
            [fldv_RiskTolerability] = COALESCE(@pRiskTolerability, [fldv_RiskTolerability]),
            [fldv_AssessmentRationale] = COALESCE(@pAssessmentRationale, [fldv_AssessmentRationale]),
            
            -- Step 5 Fields
            [fldv_ImplementationStrategy] = COALESCE(@pImplementationStrategy, [fldv_ImplementationStrategy]),
            [fldd_OverallTargetDate] = COALESCE(@pOverallTargetDate, [fldd_OverallTargetDate]),
            [fldv_ImplementationNotes] = COALESCE(@pImplementationNotes, [fldv_ImplementationNotes]),
            
            -- Progress Tracking Fields
            [fldv_CompletedSteps] = COALESCE(@pCompletedSteps, [fldv_CompletedSteps]),
            [fldi_CompletionPercentage] = COALESCE(@pCompletionPercentage, [fldi_CompletionPercentage]),
            
            -- Audit Fields
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
            
        WHERE [fldv_Code] = @pID;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        -- Enhanced audit message with step information
        DECLARE @StepInfo NVARCHAR(100) = '';
        IF @pCurrentStep IS NOT NULL
            SET @StepInfo = ', Current Step: ' + CAST(@pCurrentStep AS VARCHAR(10));
        IF @pCompletionPercentage IS NOT NULL
            SET @StepInfo = @StepInfo + ', Completion: ' + CAST(@pCompletionPercentage AS VARCHAR(10)) + '%';
            
        SET @AuditMessage = 'Successfully updated RiskAssessment ID: ' + CAST(@pID AS VARCHAR(60)) + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10)) + @StepInfo;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_RiskAssessments', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- Return success indicator
        SELECT 
            @RowsAffected AS RowsAffected,
            @pID AS UpdatedID,
            @pCurrentStep AS CurrentStep,
            @pCompletionPercentage AS CompletionPercentage,
            'Success' AS Result
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating RiskAssessment ID ' + CAST(@pID AS VARCHAR(60)) + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_RiskAssessments', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- **************************************************
-- COMPANION STORED PROCEDURES FOR STEP-SPECIFIC UPDATES
-- **************************************************

-- Step 1 Specific Update Procedure
CREATE PROCEDURE [dbo].[pr_RiskAssessment_UpdateStep1]
    @pID VARCHAR(60),
    @pLeadAssessorId NVARCHAR(50),
    @pSystemDescription NTEXT,
    @pSystemBoundaries NTEXT,
    @pSystemPurpose NTEXT,
    @pPersonnelFactors NTEXT,
    @pEquipmentFactors NTEXT,
    @pProcedureFactors NTEXT,
    @pResourceFactors NTEXT,
    @pEnvironmentFactors NTEXT,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    EXEC [dbo].[pr_RiskAssessment_Update]
        @pID = @pID,
        @pLeadAssessorId = @pLeadAssessorId,
        @pSystemDescription = @pSystemDescription,
        @pSystemBoundaries = @pSystemBoundaries,
        @pSystemPurpose = @pSystemPurpose,
        @pPersonnelFactors = @pPersonnelFactors,
        @pEquipmentFactors = @pEquipmentFactors,
        @pProcedureFactors = @pProcedureFactors,
        @pResourceFactors = @pResourceFactors,
        @pEnvironmentFactors = @pEnvironmentFactors,
        @pCurrentStep = 1,
        @pUpdatedBy = @pUpdatedBy;
END;
GO

-- Step 3 Specific Update Procedure
CREATE PROCEDURE [dbo].[pr_RiskAssessment_UpdateStep3]
    @pID VARCHAR(60),
    @pRiskAnalysisMethod NVARCHAR(100),
    @pRiskCriteria NTEXT,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    EXEC [dbo].[pr_RiskAssessment_Update]
        @pID = @pID,
        @pRiskAnalysisMethod = @pRiskAnalysisMethod,
        @pRiskCriteria = @pRiskCriteria,
        @pCurrentStep = 3,
        @pUpdatedBy = @pUpdatedBy;
END;
GO

-- Step 4 Specific Update Procedure
CREATE PROCEDURE [dbo].[pr_RiskAssessment_UpdateStep4]
    @pID VARCHAR(60),
    @pTolerabilityFramework NVARCHAR(100),
    @pRiskAcceptanceCriteria NTEXT,
    @pFinalSeverityScore INT,
    @pFinalLikelihoodScore INT,
    @pFinalRiskLevel NVARCHAR(10),
    @pRiskTolerability NVARCHAR(50),
    @pAssessmentRationale NTEXT,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    EXEC [dbo].[pr_RiskAssessment_Update]
        @pID = @pID,
        @pTolerabilityFramework = @pTolerabilityFramework,
        @pRiskAcceptanceCriteria = @pRiskAcceptanceCriteria,
        @pFinalSeverityScore = @pFinalSeverityScore,
        @pFinalLikelihoodScore = @pFinalLikelihoodScore,
        @pFinalRiskLevel = @pFinalRiskLevel,
        @pRiskTolerability = @pRiskTolerability,
        @pAssessmentRationale = @pAssessmentRationale,
        @pCurrentStep = 4,
        @pUpdatedBy = @pUpdatedBy;
END;
GO

-- Step 5 Specific Update Procedure
CREATE PROCEDURE [dbo].[pr_RiskAssessment_UpdateStep5]
    @pID VARCHAR(60),
    @pImplementationStrategy NTEXT,
    @pOverallTargetDate DATETIME,
    @pImplementationNotes NTEXT,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    EXEC [dbo].[pr_RiskAssessment_Update]
        @pID = @pID,
        @pImplementationStrategy = @pImplementationStrategy,
        @pOverallTargetDate = @pOverallTargetDate,
        @pImplementationNotes = @pImplementationNotes,
        @pCurrentStep = 5,
        @pUpdatedBy = @pUpdatedBy;
END;
GO

-- Progress Update Procedure
CREATE PROCEDURE [dbo].[pr_RiskAssessment_UpdateProgress]
    @pID VARCHAR(60),
    @pCurrentStep INT,
    @pCompletedSteps NVARCHAR(50),
    @pCompletionPercentage INT,
    @pStatus NVARCHAR(50) = NULL,
    @pStage NVARCHAR(50) = NULL,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    EXEC [dbo].[pr_RiskAssessment_Update]
        @pID = @pID,
        @pCurrentStep = @pCurrentStep,
        @pCompletedSteps = @pCompletedSteps,
        @pCompletionPercentage = @pCompletionPercentage,
        @pRiskAssessmentStatus = @pStatus,
        @pRiskAssessmentStage = @pStage,
        @pUpdatedBy = @pUpdatedBy;
END;
GO

-- **************************************************
-- TEST QUERY TO VERIFY THE UPDATE
-- **************************************************

-- Example usage for Step 1 update:
/*
EXEC [dbo].[pr_RiskAssessment_UpdateStep1]
    @pID = 'RISK-ASSESSMENT-001',
    @pLeadAssessorId = 'USER-001',
    @pSystemDescription = 'Updated system description for runway operations',
    @pSystemBoundaries = 'Runway 10L-28R and associated taxiways',
    @pSystemPurpose = 'Safe aircraft landing and takeoff operations',
    @pPersonnelFactors = 'ATC controllers, ground personnel, pilots',
    @pEquipmentFactors = 'Navigation aids, lighting systems, communication equipment',
    @pProcedureFactors = 'Standard operating procedures, emergency procedures',
    @pResourceFactors = 'Fuel trucks, ground support equipment',
    @pEnvironmentFactors = 'Weather conditions, visibility, wind patterns',
    @pUpdatedBy = 'WIZARD-USER-001';
*/

PRINT '***************************************************';
PRINT 'Risk Assessment Update stored procedures created successfully!';
PRINT '***************************************************';
PRINT 'PROCEDURES CREATED:';
PRINT '- pr_RiskAssessment_Update (comprehensive update)';
PRINT '- pr_RiskAssessment_UpdateStep1 (Step 1 specific)';
PRINT '- pr_RiskAssessment_UpdateStep3 (Step 3 specific)';
PRINT '- pr_RiskAssessment_UpdateStep4 (Step 4 specific)';
PRINT '- pr_RiskAssessment_UpdateStep5 (Step 5 specific)';
PRINT '- pr_RiskAssessment_UpdateProgress (progress tracking)';
PRINT '***************************************************';

GO