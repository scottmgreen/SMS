# Enhanced Mitigation Stored Procedures

## Overview
This document contains the updated stored procedures for the comprehensive Mitigation system that supports all fields in the `tbld_Mitigations` table schema. These procedures replace the basic versions and provide full CRUD functionality for all mitigation properties.

## Updated Stored Procedures

### 1. pr_Mitigation_Insert (Enhanced)

```sql
-- =============================================
-- Enhanced Mitigation Insert Procedure
-- Supports all comprehensive mitigation fields
-- =============================================

ALTER PROCEDURE [dbo].[pr_Mitigation_Insert]
    -- Core Fields
    @pMitigationCode NCHAR(10) = NULL,
    @pMitigationHazardCode NCHAR(10) = NULL,
    @pMitigationName NVARCHAR(100) = NULL,
    @pMitigationDescription NVARCHAR(MAX) = NULL,
    @pMitigationType NVARCHAR(50) = NULL,
    @pMitigationStatus NVARCHAR(50) = 'Proposed',
    @pMitigationPriority NVARCHAR(20) = NULL,
    @pMitigationRiskAssessmentCode NCHAR(10) = NULL,
    
    -- Timeline Fields
    @pMitigationTargetDate DATETIME = NULL,
    @pMitigationImplementationDate DATETIME = NULL,
    @pMitigationCompletionDate DATETIME = NULL,
    
    -- Assignment Fields
    @pMitigationAssignedDepartment NVARCHAR(100) = NULL,
    @pMitigationAssignedTo NVARCHAR(100) = NULL,
    @pMitigationApprovedBy NVARCHAR(100) = NULL,
    @pMitigationApprovedDate DATETIME = NULL,
    
    -- Progress Fields
    @pMitigationProgress INT = 0,
    @pMitigationProgressNotes NVARCHAR(MAX) = NULL,
    @pMitigationLastProgressUpdate DATETIME = NULL,
    @pMitigationProgressUpdatedBy NVARCHAR(100) = NULL,
    
    -- Cost and Resource Fields
    @pMitigationEstimatedCost DECIMAL(18,2) = NULL,
    @pMitigationActualCost DECIMAL(18,2) = NULL,
    @pMitigationResourceRequirements NVARCHAR(MAX) = NULL,
    @pMitigationEstimatedHours INT = NULL,
    @pMitigationActualHours INT = NULL,
    
    -- Effectiveness Fields
    @pMitigationEffectivenessRating NVARCHAR(50) = NULL,
    @pMitigationEffectivenessNotes NVARCHAR(MAX) = NULL,
    @pMitigationEffectivenessReviewDate DATETIME = NULL,
    @pMitigationEffectivenessReviewedBy NVARCHAR(100) = NULL,
    
    -- Monitoring Fields
    @pMitigationMonitoringRequirements NVARCHAR(MAX) = NULL,
    @pMitigationMonitoringFrequency NVARCHAR(50) = NULL,
    
    -- Risk Reduction Fields
    @pMitigationExpectedSeverityReduction INT = NULL,
    @pMitigationExpectedLikelihoodReduction INT = NULL,
    @pMitigationActualSeverityReduction INT = NULL,
    @pMitigationActualLikelihoodReduction INT = NULL,
    @pMitigationResidualRiskLevel NVARCHAR(20) = NULL,
    
    -- Dependency Fields
    @pMitigationPrerequisites NVARCHAR(MAX) = NULL,
    @pMitigationDependencies NVARCHAR(MAX) = NULL,
    @pMitigationHasDependencies BIT = 0,
    @pMitigationIsPrerequisite BIT = 0,
    
    -- Planning Fields
    @pMitigationImplementationPlan NVARCHAR(MAX) = NULL,
    @pMitigationCommunicationPlan NVARCHAR(MAX) = NULL,
    @pMitigationTrainingRequirements NVARCHAR(MAX) = NULL,
    @pMitigationDocumentationUpdates NVARCHAR(MAX) = NULL,
    
    -- Testing Fields
    @pMitigationTestingProcedure NVARCHAR(MAX) = NULL,
    @pMitigationTestingCompletedDate DATETIME = NULL,
    @pMitigationTestingResults NVARCHAR(MAX) = NULL,
    
    -- Validation Fields
    @pMitigationValidationRequired BIT = 0,
    @pMitigationValidationDate DATETIME = NULL,
    @pMitigationValidatedBy NVARCHAR(100) = NULL,
    
    -- Additional Fields
    @pMitigationNotes NVARCHAR(MAX) = NULL,
    @pMitigationLessonsLearned NVARCHAR(MAX) = NULL,
    @pMitigationRecommendationsForFuture NVARCHAR(MAX) = NULL,
    
    -- Audit Fields
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    
    -- Output Parameters
    @pNewID INT OUTPUT,
    @pNewMitigationCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_Mitigation_Insert';
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = 'Starting enhanced Mitigation insert operation';
        
        -- Generate Mitigation Code using entity registry
        EXEC [pr_GenerateFormattedCode] 
            @EntityName = 'Mitigation',
            @GeneratedCode = @pNewMitigationCode OUTPUT;

        INSERT INTO [dbo].[tbld_Mitigations] (
            [fldv_Code], [fldv_HazardCode], [fldv_Name], [fldc_Description], 
            [fldv_Type], [fldv_Status], [fldv_Priority], [fldv_RiskAssessmentCode],
            
            -- Timeline Fields
            [fldd_TargetDate], [fldd_ImplementationDate], [fldd_CompletionDate],
            
            -- Assignment Fields
            [fldv_AssignedDepartment], [fldv_AssignedTo], [fldv_ApprovedBy], [fldd_ApprovedDate],
            
            -- Progress Fields
            [fldi_Progress], [fldv_ProgressNotes], [fldd_LastProgressUpdate], [fldv_ProgressUpdatedBy],
            
            -- Cost and Resource Fields
            [fldd_EstimatedCost], [fldd_ActualCost], [fldv_ResourceRequirements], 
            [fldi_EstimatedHours], [fldi_ActualHours],
            
            -- Effectiveness Fields
            [fldv_EffectivenessRating], [fldc_EffectivenessNotes], [fldd_EffectivenessReviewDate], [fldv_EffectivenessReviewedBy],
            
            -- Monitoring Fields
            [fldc_MonitoringRequirements], [fldv_MonitoringFrequency],
            
            -- Risk Reduction Fields
            [fldi_ExpectedSeverityReduction], [fldi_ExpectedLikelihoodReduction], 
            [fldi_ActualSeverityReduction], [fldi_ActualLikelihoodReduction], [fldv_ResidualRiskLevel],
            
            -- Dependency Fields
            [fldv_Prerequisites], [fldv_Dependencies], [fldb_HasDependencies], [fldb_IsPrerequisite],
            
            -- Planning Fields
            [fldc_ImplementationPlan], [fldc_CommunicationPlan], [fldc_TrainingRequirements], [fldc_DocumentationUpdates],
            
            -- Testing Fields
            [fldc_TestingProcedure], [fldd_TestingCompletedDate], [fldv_TestingResults],
            
            -- Validation Fields
            [fldb_ValidationRequired], [fldd_ValidationDate], [fldv_ValidatedBy],
            
            -- Additional Fields
            [fldc_Notes], [fldc_LessonsLearned], [fldc_RecommendationsForFuture],
            
            -- Audit Fields
            [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES (
            @pNewMitigationCode, @pMitigationHazardCode, @pMitigationName, @pMitigationDescription,
            @pMitigationType, @pMitigationStatus, @pMitigationPriority, @pMitigationRiskAssessmentCode,
            
            -- Timeline Values
            @pMitigationTargetDate, @pMitigationImplementationDate, @pMitigationCompletionDate,
            
            -- Assignment Values
            @pMitigationAssignedDepartment, @pMitigationAssignedTo, @pMitigationApprovedBy, @pMitigationApprovedDate,
            
            -- Progress Values
            @pMitigationProgress, @pMitigationProgressNotes, @pMitigationLastProgressUpdate, @pMitigationProgressUpdatedBy,
            
            -- Cost and Resource Values
            @pMitigationEstimatedCost, @pMitigationActualCost, @pMitigationResourceRequirements,
            @pMitigationEstimatedHours, @pMitigationActualHours,
            
            -- Effectiveness Values
            @pMitigationEffectivenessRating, @pMitigationEffectivenessNotes, @pMitigationEffectivenessReviewDate, @pMitigationEffectivenessReviewedBy,
            
            -- Monitoring Values
            @pMitigationMonitoringRequirements, @pMitigationMonitoringFrequency,
            
            -- Risk Reduction Values
            @pMitigationExpectedSeverityReduction, @pMitigationExpectedLikelihoodReduction,
            @pMitigationActualSeverityReduction, @pMitigationActualLikelihoodReduction, @pMitigationResidualRiskLevel,
            
            -- Dependency Values
            @pMitigationPrerequisites, @pMitigationDependencies, @pMitigationHasDependencies, @pMitigationIsPrerequisite,
            
            -- Planning Values
            @pMitigationImplementationPlan, @pMitigationCommunicationPlan, @pMitigationTrainingRequirements, @pMitigationDocumentationUpdates,
            
            -- Testing Values
            @pMitigationTestingProcedure, @pMitigationTestingCompletedDate, @pMitigationTestingResults,
            
            -- Validation Values
            @pMitigationValidationRequired, @pMitigationValidationDate, @pMitigationValidatedBy,
            
            -- Additional Values
            @pMitigationNotes, @pMitigationLessonsLearned, @pMitigationRecommendationsForFuture,
            
            -- Audit Values
            @pCreatedBy, @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully inserted enhanced Mitigation with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewMitigationCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting enhanced Mitigation: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 2. pr_Mitigation_Update (Enhanced)

```sql
-- =============================================
-- Enhanced Mitigation Update Procedure
-- Supports all comprehensive mitigation fields
-- =============================================

ALTER PROCEDURE [dbo].[pr_Mitigation_Update]
    -- Primary Key
    @pID VARCHAR(60),
    
    -- Core Fields
    @pMitigationCode NCHAR(10) = NULL,
    @pMitigationHazardCode NCHAR(10) = NULL,
    @pMitigationName NVARCHAR(100) = NULL,
    @pMitigationDescription NVARCHAR(MAX) = NULL,
    @pMitigationType NVARCHAR(50) = NULL,
    @pMitigationStatus NVARCHAR(50) = NULL,
    @pMitigationPriority NVARCHAR(20) = NULL,
    @pMitigationRiskAssessmentCode NCHAR(10) = NULL,
    
    -- Timeline Fields
    @pMitigationTargetDate DATETIME = NULL,
    @pMitigationImplementationDate DATETIME = NULL,
    @pMitigationCompletionDate DATETIME = NULL,
    
    -- Assignment Fields
    @pMitigationAssignedDepartment NVARCHAR(100) = NULL,
    @pMitigationAssignedTo NVARCHAR(100) = NULL,
    @pMitigationApprovedBy NVARCHAR(100) = NULL,
    @pMitigationApprovedDate DATETIME = NULL,
    
    -- Progress Fields
    @pMitigationProgress INT = NULL,
    @pMitigationProgressNotes NVARCHAR(MAX) = NULL,
    @pMitigationLastProgressUpdate DATETIME = NULL,
    @pMitigationProgressUpdatedBy NVARCHAR(100) = NULL,
    
    -- Cost and Resource Fields
    @pMitigationEstimatedCost DECIMAL(18,2) = NULL,
    @pMitigationActualCost DECIMAL(18,2) = NULL,
    @pMitigationResourceRequirements NVARCHAR(MAX) = NULL,
    @pMitigationEstimatedHours INT = NULL,
    @pMitigationActualHours INT = NULL,
    
    -- Effectiveness Fields
    @pMitigationEffectivenessRating NVARCHAR(50) = NULL,
    @pMitigationEffectivenessNotes NVARCHAR(MAX) = NULL,
    @pMitigationEffectivenessReviewDate DATETIME = NULL,
    @pMitigationEffectivenessReviewedBy NVARCHAR(100) = NULL,
    
    -- Monitoring Fields
    @pMitigationMonitoringRequirements NVARCHAR(MAX) = NULL,
    @pMitigationMonitoringFrequency NVARCHAR(50) = NULL,
    
    -- Risk Reduction Fields
    @pMitigationExpectedSeverityReduction INT = NULL,
    @pMitigationExpectedLikelihoodReduction INT = NULL,
    @pMitigationActualSeverityReduction INT = NULL,
    @pMitigationActualLikelihoodReduction INT = NULL,
    @pMitigationResidualRiskLevel NVARCHAR(20) = NULL,
    
    -- Dependency Fields
    @pMitigationPrerequisites NVARCHAR(MAX) = NULL,
    @pMitigationDependencies NVARCHAR(MAX) = NULL,
    @pMitigationHasDependencies BIT = NULL,
    @pMitigationIsPrerequisite BIT = NULL,
    
    -- Planning Fields
    @pMitigationImplementationPlan NVARCHAR(MAX) = NULL,
    @pMitigationCommunicationPlan NVARCHAR(MAX) = NULL,
    @pMitigationTrainingRequirements NVARCHAR(MAX) = NULL,
    @pMitigationDocumentationUpdates NVARCHAR(MAX) = NULL,
    
    -- Testing Fields
    @pMitigationTestingProcedure NVARCHAR(MAX) = NULL,
    @pMitigationTestingCompletedDate DATETIME = NULL,
    @pMitigationTestingResults NVARCHAR(MAX) = NULL,
    
    -- Validation Fields
    @pMitigationValidationRequired BIT = NULL,
    @pMitigationValidationDate DATETIME = NULL,
    @pMitigationValidatedBy NVARCHAR(100) = NULL,
    
    -- Additional Fields
    @pMitigationNotes NVARCHAR(MAX) = NULL,
    @pMitigationLessonsLearned NVARCHAR(MAX) = NULL,
    @pMitigationRecommendationsForFuture NVARCHAR(MAX) = NULL,
    
    -- Audit Fields
    @pUpdatedBy VARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_Mitigation_Update', @RowsAffected INT;
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting enhanced Mitigation update for Code: ' + @pID;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Mitigations] SET 
            [fldv_Code] = COALESCE(@pMitigationCode, [fldv_Code]),
            [fldv_HazardCode] = COALESCE(@pMitigationHazardCode, [fldv_HazardCode]),
            [fldv_Name] = COALESCE(@pMitigationName, [fldv_Name]),
            [fldc_Description] = COALESCE(@pMitigationDescription, [fldc_Description]),
            [fldv_Type] = COALESCE(@pMitigationType, [fldv_Type]),
            [fldv_Status] = COALESCE(@pMitigationStatus, [fldv_Status]),
            [fldv_Priority] = COALESCE(@pMitigationPriority, [fldv_Priority]),
            [fldv_RiskAssessmentCode] = COALESCE(@pMitigationRiskAssessmentCode, [fldv_RiskAssessmentCode]),
            
            -- Timeline Fields
            [fldd_TargetDate] = COALESCE(@pMitigationTargetDate, [fldd_TargetDate]),
            [fldd_ImplementationDate] = COALESCE(@pMitigationImplementationDate, [fldd_ImplementationDate]),
            [fldd_CompletionDate] = COALESCE(@pMitigationCompletionDate, [fldd_CompletionDate]),
            
            -- Assignment Fields
            [fldv_AssignedDepartment] = COALESCE(@pMitigationAssignedDepartment, [fldv_AssignedDepartment]),
            [fldv_AssignedTo] = COALESCE(@pMitigationAssignedTo, [fldv_AssignedTo]),
            [fldv_ApprovedBy] = COALESCE(@pMitigationApprovedBy, [fldv_ApprovedBy]),
            [fldd_ApprovedDate] = COALESCE(@pMitigationApprovedDate, [fldd_ApprovedDate]),
            
            -- Progress Fields
            [fldi_Progress] = COALESCE(@pMitigationProgress, [fldi_Progress]),
            [fldv_ProgressNotes] = COALESCE(@pMitigationProgressNotes, [fldv_ProgressNotes]),
            [fldd_LastProgressUpdate] = COALESCE(@pMitigationLastProgressUpdate, [fldd_LastProgressUpdate]),
            [fldv_ProgressUpdatedBy] = COALESCE(@pMitigationProgressUpdatedBy, [fldv_ProgressUpdatedBy]),
            
            -- Cost and Resource Fields
            [fldd_EstimatedCost] = COALESCE(@pMitigationEstimatedCost, [fldd_EstimatedCost]),
            [fldd_ActualCost] = COALESCE(@pMitigationActualCost, [fldd_ActualCost]),
            [fldv_ResourceRequirements] = COALESCE(@pMitigationResourceRequirements, [fldv_ResourceRequirements]),
            [fldi_EstimatedHours] = COALESCE(@pMitigationEstimatedHours, [fldi_EstimatedHours]),
            [fldi_ActualHours] = COALESCE(@pMitigationActualHours, [fldi_ActualHours]),
            
            -- Effectiveness Fields
            [fldv_EffectivenessRating] = COALESCE(@pMitigationEffectivenessRating, [fldv_EffectivenessRating]),
            [fldc_EffectivenessNotes] = COALESCE(@pMitigationEffectivenessNotes, [fldc_EffectivenessNotes]),
            [fldd_EffectivenessReviewDate] = COALESCE(@pMitigationEffectivenessReviewDate, [fldd_EffectivenessReviewDate]),
            [fldv_EffectivenessReviewedBy] = COALESCE(@pMitigationEffectivenessReviewedBy, [fldv_EffectivenessReviewedBy]),
            
            -- Monitoring Fields
            [fldc_MonitoringRequirements] = COALESCE(@pMitigationMonitoringRequirements, [fldc_MonitoringRequirements]),
            [fldv_MonitoringFrequency] = COALESCE(@pMitigationMonitoringFrequency, [fldv_MonitoringFrequency]),
            
            -- Risk Reduction Fields
            [fldi_ExpectedSeverityReduction] = COALESCE(@pMitigationExpectedSeverityReduction, [fldi_ExpectedSeverityReduction]),
            [fldi_ExpectedLikelihoodReduction] = COALESCE(@pMitigationExpectedLikelihoodReduction, [fldi_ExpectedLikelihoodReduction]),
            [fldi_ActualSeverityReduction] = COALESCE(@pMitigationActualSeverityReduction, [fldi_ActualSeverityReduction]),
            [fldi_ActualLikelihoodReduction] = COALESCE(@pMitigationActualLikelihoodReduction, [fldi_ActualLikelihoodReduction]),
            [fldv_ResidualRiskLevel] = COALESCE(@pMitigationResidualRiskLevel, [fldv_ResidualRiskLevel]),
            
            -- Dependency Fields
            [fldv_Prerequisites] = COALESCE(@pMitigationPrerequisites, [fldv_Prerequisites]),
            [fldv_Dependencies] = COALESCE(@pMitigationDependencies, [fldv_Dependencies]),
            [fldb_HasDependencies] = COALESCE(@pMitigationHasDependencies, [fldb_HasDependencies]),
            [fldb_IsPrerequisite] = COALESCE(@pMitigationIsPrerequisite, [fldb_IsPrerequisite]),
            
            -- Planning Fields
            [fldc_ImplementationPlan] = COALESCE(@pMitigationImplementationPlan, [fldc_ImplementationPlan]),
            [fldc_CommunicationPlan] = COALESCE(@pMitigationCommunicationPlan, [fldc_CommunicationPlan]),
            [fldc_TrainingRequirements] = COALESCE(@pMitigationTrainingRequirements, [fldc_TrainingRequirements]),
            [fldc_DocumentationUpdates] = COALESCE(@pMitigationDocumentationUpdates, [fldc_DocumentationUpdates]),
            
            -- Testing Fields
            [fldc_TestingProcedure] = COALESCE(@pMitigationTestingProcedure, [fldc_TestingProcedure]),
            [fldd_TestingCompletedDate] = COALESCE(@pMitigationTestingCompletedDate, [fldd_TestingCompletedDate]),
            [fldv_TestingResults] = COALESCE(@pMitigationTestingResults, [fldv_TestingResults]),
            
            -- Validation Fields
            [fldb_ValidationRequired] = COALESCE(@pMitigationValidationRequired, [fldb_ValidationRequired]),
            [fldd_ValidationDate] = COALESCE(@pMitigationValidationDate, [fldd_ValidationDate]),
            [fldv_ValidatedBy] = COALESCE(@pMitigationValidatedBy, [fldv_ValidatedBy]),
            
            -- Additional Fields
            [fldc_Notes] = COALESCE(@pMitigationNotes, [fldc_Notes]),
            [fldc_LessonsLearned] = COALESCE(@pMitigationLessonsLearned, [fldc_LessonsLearned]),
            [fldc_RecommendationsForFuture] = COALESCE(@pMitigationRecommendationsForFuture, [fldc_RecommendationsForFuture]),
            
            -- Audit Fields
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
            
        WHERE [fldv_Code] = @pID;
        
        SET @RowsAffected = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully updated enhanced Mitigation Code: ' + @pID + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating enhanced Mitigation Code ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 3. pr_Mitigation_GetById (Enhanced)

```sql
-- =============================================
-- Enhanced Mitigation GetById Procedure - Already Comprehensive
-- The existing procedure is already comprehensive and should work with the enhanced fields
-- No changes needed - it already returns all enhanced fields
-- =============================================

-- Note: The existing pr_Mitigation_GetById procedure you provided is already comprehensive
-- and includes all the enhanced fields. No modifications needed.
```

### 4. pr_Mitigation_GetByHazardCode (Enhanced)

```sql
-- =============================================
-- Enhanced Mitigation GetByHazardCode Procedure
-- Returns all comprehensive mitigation fields for a hazard
-- =============================================

ALTER PROCEDURE [dbo].[pr_Mitigation_GetByHazardCode]
    @pHazardCode VARCHAR(50),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
    DECLARE @FunctionName VARCHAR(50) = 'pr_Mitigation_GetByHazardCode';
    
    BEGIN TRY        
        -- Get all mitigations for the specified hazard code with complete data
        SELECT 
            -- Core Fields
            [fldi_ID],
            [fldv_Code],
            [fldv_HazardCode],
            [fldv_Name],
            [fldc_Description],
            [fldv_Type],
            [fldv_Status],
            [fldv_Priority],
            [fldv_RiskAssessmentCode],
            
            -- Timeline Fields
            [fldd_TargetDate],
            [fldd_ImplementationDate],
            [fldd_CompletionDate],
            
            -- Assignment Fields
            [fldv_AssignedDepartment],
            [fldv_AssignedTo],
            [fldv_ApprovedBy],
            [fldd_ApprovedDate],
            
            -- Progress Fields
            [fldi_Progress],
            [fldv_ProgressNotes],
            [fldd_LastProgressUpdate],
            [fldv_ProgressUpdatedBy],
            
            -- Cost and Resource Fields
            [fldd_EstimatedCost],
            [fldd_ActualCost],
            [fldv_ResourceRequirements],
            [fldi_EstimatedHours],
            [fldi_ActualHours],
            
            -- Effectiveness Fields
            [fldv_EffectivenessRating],
            [fldc_EffectivenessNotes],
            [fldd_EffectivenessReviewDate],
            [fldv_EffectivenessReviewedBy],
            
            -- Monitoring Fields
            [fldc_MonitoringRequirements],
            [fldv_MonitoringFrequency],
            
            -- Risk Reduction Fields
            [fldi_ExpectedSeverityReduction],
            [fldi_ExpectedLikelihoodReduction],
            [fldi_ActualSeverityReduction],
            [fldi_ActualLikelihoodReduction],
            [fldv_ResidualRiskLevel],
            
            -- Dependency Fields
            [fldv_Prerequisites],
            [fldv_Dependencies],
            [fldb_HasDependencies],
            [fldb_IsPrerequisite],
            
            -- Planning Fields
            [fldc_ImplementationPlan],
            [fldc_CommunicationPlan],
            [fldc_TrainingRequirements],
            [fldc_DocumentationUpdates],
            
            -- Testing Fields
            [fldc_TestingProcedure],
            [fldd_TestingCompletedDate],
            [fldv_TestingResults],
            
            -- Validation Fields
            [fldb_ValidationRequired],
            [fldd_ValidationDate],
            [fldv_ValidatedBy],
            
            -- Additional Fields
            [fldc_Notes],
            [fldc_LessonsLearned],
            [fldc_RecommendationsForFuture],
            
            -- Audit Fields
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_Mitigations]
        WHERE [fldv_HazardCode] = @pHazardCode
        ORDER BY [fldv_Priority], [fldd_TargetDate];
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @ErrorMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

### 5. pr_Mitigation_GetAll (Enhanced)

```sql
-- =============================================
-- Enhanced Mitigation GetAll Procedure
-- Returns all comprehensive mitigation fields
-- =============================================

ALTER PROCEDURE [dbo].[pr_Mitigation_GetAll]
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_Mitigation_GetAll', @RecordCount INT;
    
    BEGIN TRY
       -- EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = 'Retrieving all enhanced Mitigations';
        
        SELECT 
            -- Core Fields
            [fldi_ID],
            [fldv_Code],
            [fldv_HazardCode],
            [fldv_Name],
            [fldc_Description],
            [fldv_Type],
            [fldv_Status],
            [fldv_Priority],
            [fldv_RiskAssessmentCode],
            
            -- Timeline Fields
            [fldd_TargetDate],
            [fldd_ImplementationDate],
            [fldd_CompletionDate],
            
            -- Assignment Fields
            [fldv_AssignedDepartment],
            [fldv_AssignedTo],
            [fldv_ApprovedBy],
            [fldd_ApprovedDate],
            
            -- Progress Fields
            [fldi_Progress],
            [fldv_ProgressNotes],
            [fldd_LastProgressUpdate],
            [fldv_ProgressUpdatedBy],
            
            -- Cost and Resource Fields
            [fldd_EstimatedCost],
            [fldd_ActualCost],
            [fldv_ResourceRequirements],
            [fldi_EstimatedHours],
            [fldi_ActualHours],
            
            -- Effectiveness Fields
            [fldv_EffectivenessRating],
            [fldc_EffectivenessNotes],
            [fldd_EffectivenessReviewDate],
            [fldv_EffectivenessReviewedBy],
            
            -- Monitoring Fields
            [fldc_MonitoringRequirements],
            [fldv_MonitoringFrequency],
            
            -- Risk Reduction Fields
            [fldi_ExpectedSeverityReduction],
            [fldi_ExpectedLikelihoodReduction],
            [fldi_ActualSeverityReduction],
            [fldi_ActualLikelihoodReduction],
            [fldv_ResidualRiskLevel],
            
            -- Dependency Fields
            [fldv_Prerequisites],
            [fldv_Dependencies],
            [fldb_HasDependencies],
            [fldb_IsPrerequisite],
            
            -- Planning Fields
            [fldc_ImplementationPlan],
            [fldc_CommunicationPlan],
            [fldc_TrainingRequirements],
            [fldc_DocumentationUpdates],
            
            -- Testing Fields
            [fldc_TestingProcedure],
            [fldd_TestingCompletedDate],
            [fldv_TestingResults],
            
            -- Validation Fields
            [fldb_ValidationRequired],
            [fldd_ValidationDate],
            [fldv_ValidatedBy],
            
            -- Additional Fields
            [fldc_Notes],
            [fldc_LessonsLearned],
            [fldc_RecommendationsForFuture],
            
            -- Audit Fields
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_Mitigations] 
        ORDER BY [fldv_Code];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' enhanced Mitigations';
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving all enhanced Mitigations: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
```

## Missing Parameter Names for ParameterNames.cs

Add these parameters to the **bottom** of your `ParameterNames.cs` file:

```csharp
    /// <summary>
    /// ENHANCED MITIGATION PARAMETERS - All comprehensive mitigation fields
    /// </summary>
    
    // Core Mitigation Fields
    private static readonly Lazy<string> _pmMitigationName = new Lazy<string>(() => "@pMitigationName");
    public static string pmMitigationName => _pmMitigationName.Value;

    private static readonly Lazy<string> _pmMitigationDescription = new Lazy<string>(() => "@pMitigationDescription");
    public static string pmMitigationDescription => _pmMitigationDescription.Value;

    private static readonly Lazy<string> _pmMitigationType = new Lazy<string>(() => "@pMitigationType");
    public static string pmMitigationType => _pmMitigationType.Value;

    private static readonly Lazy<string> _pmMitigationStatus = new Lazy<string>(() => "@pMitigationStatus");
    public static string pmMitigationStatus => _pmMitigationStatus.Value;

    private static readonly Lazy<string> _pmMitigationPriority = new Lazy<string>(() => "@pMitigationPriority");
    public static string pmMitigationPriority => _pmMitigationPriority.Value;

    private static readonly Lazy<string> _pmMitigationRiskAssessmentCode = new Lazy<string>(() => "@pMitigationRiskAssessmentCode");
    public static string pmMitigationRiskAssessmentCode => _pmMitigationRiskAssessmentCode.Value;

    // Timeline Fields
    private static readonly Lazy<string> _pmMitigationTargetDate = new Lazy<string>(() => "@pMitigationTargetDate");
    public static string pmMitigationTargetDate => _pmMitigationTargetDate.Value;

    private static readonly Lazy<string> _pmMitigationImplementationDate = new Lazy<string>(() => "@pMitigationImplementationDate");
    public static string pmMitigationImplementationDate => _pmMitigationImplementationDate.Value;

    private static readonly Lazy<string> _pmMitigationCompletionDate = new Lazy<string>(() => "@pMitigationCompletionDate");
    public static string pmMitigationCompletionDate => _pmMitigationCompletionDate.Value;

    // Assignment Fields
    private static readonly Lazy<string> _pmMitigationAssignedDepartment = new Lazy<string>(() => "@pMitigationAssignedDepartment");
    public static string pmMitigationAssignedDepartment => _pmMitigationAssignedDepartment.Value;

    private static readonly Lazy<string> _pmMitigationAssignedTo = new Lazy<string>(() => "@pMitigationAssignedTo");
    public static string pmMitigationAssignedTo => _pmMitigationAssignedTo.Value;

    private static readonly Lazy<string> _pmMitigationApprovedBy = new Lazy<string>(() => "@pMitigationApprovedBy");
    public static string pmMitigationApprovedBy => _pmMitigationApprovedBy.Value;

    private static readonly Lazy<string> _pmMitigationApprovedDate = new Lazy<string>(() => "@pMitigationApprovedDate");
    public static string pmMitigationApprovedDate => _pmMitigationApprovedDate.Value;

    // Progress Fields
    private static readonly Lazy<string> _pmMitigationProgress = new Lazy<string>(() => "@pMitigationProgress");
    public static string pmMitigationProgress => _pmMitigationProgress.Value;

    private static readonly Lazy<string> _pmMitigationProgressNotes = new Lazy<string>(() => "@pMitigationProgressNotes");
    public static string pmMitigationProgressNotes => _pmMitigationProgressNotes.Value;

    private static readonly Lazy<string> _pmMitigationLastProgressUpdate = new Lazy<string>(() => "@pMitigationLastProgressUpdate");
    public static string pmMitigationLastProgressUpdate => _pmMitigationLastProgressUpdate.Value;

    private static readonly Lazy<string> _pmMitigationProgressUpdatedBy = new Lazy<string>(() => "@pMitigationProgressUpdatedBy");
    public static string pmMitigationProgressUpdatedBy => _pmMitigationProgressUpdatedBy.Value;

    // Cost and Resource Fields
    private static readonly Lazy<string> _pmMitigationEstimatedCost = new Lazy<string>(() => "@pMitigationEstimatedCost");
    public static string pmMitigationEstimatedCost => _pmMitigationEstimatedCost.Value;

    private static readonly Lazy<string> _pmMitigationActualCost = new Lazy<string>(() => "@pMitigationActualCost");
    public static string pmMitigationActualCost => _pmMitigationActualCost.Value;

    private static readonly Lazy<string> _pmMitigationResourceRequirements = new Lazy<string>(() => "@pMitigationResourceRequirements");
    public static string pmMitigationResourceRequirements => _pmMitigationResourceRequirements.Value;

    private static readonly Lazy<string> _pmMitigationEstimatedHours = new Lazy<string>(() => "@pMitigationEstimatedHours");
    public static string pmMitigationEstimatedHours => _pmMitigationEstimatedHours.Value;

    private static readonly Lazy<string> _pmMitigationActualHours = new Lazy<string>(() => "@pMitigationActualHours");
    public static string pmMitigationActualHours => _pmMitigationActualHours.Value;

    // Effectiveness Fields
    private static readonly Lazy<string> _pmMitigationEffectivenessRating = new Lazy<string>(() => "@pMitigationEffectivenessRating");
    public static string pmMitigationEffectivenessRating => _pmMitigationEffectivenessRating.Value;

    private static readonly Lazy<string> _pmMitigationEffectivenessNotes = new Lazy<string>(() => "@pMitigationEffectivenessNotes");
    public static string pmMitigationEffectivenessNotes => _pmMitigationEffectivenessNotes.Value;

    private static readonly Lazy<string> _pmMitigationEffectivenessReviewDate = new Lazy<string>(() => "@pMitigationEffectivenessReviewDate");
    public static string pmMitigationEffectivenessReviewDate => _pmMitigationEffectivenessReviewDate.Value;

    private static readonly Lazy<string> _pmMitigationEffectivenessReviewedBy = new Lazy<string>(() => "@pMitigationEffectivenessReviewedBy");
    public static string pmMitigationEffectivenessReviewedBy => _pmMitigationEffectivenessReviewedBy.Value;

    // Monitoring Fields
    private static readonly Lazy<string> _pmMitigationMonitoringRequirements = new Lazy<string>(() => "@pMitigationMonitoringRequirements");
    public static string pmMitigationMonitoringRequirements => _pmMitigationMonitoringRequirements.Value;

    private static readonly Lazy<string> _pmMitigationMonitoringFrequency = new Lazy<string>(() => "@pMitigationMonitoringFrequency");
    public static string pmMitigationMonitoringFrequency => _pmMitigationMonitoringFrequency.Value;

    // Risk Reduction Fields
    private static readonly Lazy<string> _pmMitigationExpectedSeverityReduction = new Lazy<string>(() => "@pMitigationExpectedSeverityReduction");
    public static string pmMitigationExpectedSeverityReduction => _pmMitigationExpectedSeverityReduction.Value;

    private static readonly Lazy<string> _pmMitigationExpectedLikelihoodReduction = new Lazy<string>(() => "@pMitigationExpectedLikelihoodReduction");
    public static string pmMitigationExpectedLikelihoodReduction => _pmMitigationExpectedLikelihoodReduction.Value;

    private static readonly Lazy<string> _pmMitigationActualSeverityReduction = new Lazy<string>(() => "@pMitigationActualSeverityReduction");
    public static string pmMitigationActualSeverityReduction => _pmMitigationActualSeverityReduction.Value;

    private static readonly Lazy<string> _pmMitigationActualLikelihoodReduction = new Lazy<string>(() => "@pMitigationActualLikelihoodReduction");
    public static string pmMitigationActualLikelihoodReduction => _pmMitigationActualLikelihoodReduction.Value;

    private static readonly Lazy<string> _pmMitigationResidualRiskLevel = new Lazy<string>(() => "@pMitigationResidualRiskLevel");
    public static string pmMitigationResidualRiskLevel => _pmMitigationResidualRiskLevel.Value;

    // Dependency Fields
    private static readonly Lazy<string> _pmMitigationPrerequisites = new Lazy<string>(() => "@pMitigationPrerequisites");
    public static string pmMitigationPrerequisites => _pmMitigationPrerequisites.Value;

    private static readonly Lazy<string> _pmMitigationDependencies = new Lazy<string>(() => "@pMitigationDependencies");
    public static string pmMitigationDependencies => _pmMitigationDependencies.Value;

    private static readonly Lazy<string> _pmMitigationHasDependencies = new Lazy<string>(() => "@pMitigationHasDependencies");
    public static string pmMitigationHasDependencies => _pmMitigationHasDependencies.Value;

    private static readonly Lazy<string> _pmMitigationIsPrerequisite = new Lazy<string>(() => "@pMitigationIsPrerequisite");
    public static string pmMitigationIsPrerequisite => _pmMitigationIsPrerequisite.Value;

    // Planning Fields
    private static readonly Lazy<string> _pmMitigationImplementationPlan = new Lazy<string>(() => "@pMitigationImplementationPlan");
    public static string pmMitigationImplementationPlan => _pmMitigationImplementationPlan.Value;

    private static readonly Lazy<string> _pmMitigationCommunicationPlan = new Lazy<string>(() => "@pMitigationCommunicationPlan");
    public static string pmMitigationCommunicationPlan => _pmMitigationCommunicationPlan.Value;

    private static readonly Lazy<string> _pmMitigationTrainingRequirements = new Lazy<string>(() => "@pMitigationTrainingRequirements");
    public static string pmMitigationTrainingRequirements => _pmMitigationTrainingRequirements.Value;

    private static readonly Lazy<string> _pmMitigationDocumentationUpdates = new Lazy<string>(() => "@pMitigationDocumentationUpdates");
    public static string pmMitigationDocumentationUpdates => _pmMitigationDocumentationUpdates.Value;

    // Testing Fields
    private static readonly Lazy<string> _pmMitigationTestingProcedure = new Lazy<string>(() => "@pMitigationTestingProcedure");
    public static string pmMitigationTestingProcedure => _pmMitigationTestingProcedure.Value;

    private static readonly Lazy<string> _pmMitigationTestingCompletedDate = new Lazy<string>(() => "@pMitigationTestingCompletedDate");
    public static string pmMitigationTestingCompletedDate => _pmMitigationTestingCompletedDate.Value;

    private static readonly Lazy<string> _pmMitigationTestingResults = new Lazy<string>(() => "@pMitigationTestingResults");
    public static string pmMitigationTestingResults => _pmMitigationTestingResults.Value;

    // Validation Fields
    private static readonly Lazy<string> _pmMitigationValidationRequired = new Lazy<string>(() => "@pMitigationValidationRequired");
    public static string pmMitigationValidationRequired => _pmMitigationValidationRequired.Value;

    private static readonly Lazy<string> _pmMitigationValidationDate = new Lazy<string>(() => "@pMitigationValidationDate");
    public static string pmMitigationValidationDate => _pmMitigationValidationDate.Value;

    private static readonly Lazy<string> _pmMitigationValidatedBy = new Lazy<string>(() => "@pMitigationValidatedBy");
    public static string pmMitigationValidatedBy => _pmMitigationValidatedBy.Value;

    // Additional Fields
    private static readonly Lazy<string> _pmMitigationNotes = new Lazy<string>(() => "@pMitigationNotes");
    public static string pmMitigationNotes => _pmMitigationNotes.Value;

    private static readonly Lazy<string> _pmMitigationLessonsLearned = new Lazy<string>(() => "@pMitigationLessonsLearned");
    public static string pmMitigationLessonsLearned => _pmMitigationLessonsLearned.Value;

    private static readonly Lazy<string> _pmMitigationRecommendationsForFuture = new Lazy<string>(() => "@pMitigationRecommendationsForFuture");
    public static string pmMitigationRecommendationsForFuture => _pmMitigationRecommendationsForFuture.Value;
```

## Implementation Order

1. **First**: Add the missing parameter names to `ParameterNames.cs` (code block above)
2. **Second**: Update the stored procedures in your database (SQL scripts above) 
3. **Third**: Test the mitigation creation - should now save all field values properly

This will resolve the NULL values issue and provide full comprehensive mitigation functionality with proper database persistence.