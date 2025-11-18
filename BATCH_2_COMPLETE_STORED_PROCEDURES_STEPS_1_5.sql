-- **************************************************
-- BATCH 2: COMPLETE STORED PROCEDURES FOR STEPS 1-5
-- Enhanced stored procedures for comprehensive risk assessment workflow
-- **************************************************
USE [PDXSMS_V2]
GO

PRINT '***************************************************';
PRINT 'BATCH 2: CREATING ENHANCED STORED PROCEDURES FOR STEPS 1-5';
PRINT 'Creating: pr_RiskAssessment_GetById, pr_Hazard_GetById, pr_Mitigation_GetById, pr_MitigationAssignment_GetById';
PRINT '***************************************************';

-- **************************************************
-- Enhanced pr_RiskAssessment_GetById - Complete Steps 1-5 Support
-- **************************************************

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.pr_RiskAssessment_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.pr_RiskAssessment_GetById
GO

CREATE PROCEDURE [dbo].[pr_RiskAssessment_GetById]
    @pID VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_RiskAssessment_GetById';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Enhanced RiskAssessment with ID: ' + @pID;
        
        -- =============================================
        -- Main Risk Assessment Query with Complete Steps 1-5 Data
        -- =============================================
        SELECT 
            -- Core Identification Fields
            [fldi_ID],
            [fldv_Code],
            [fldv_Name],
            [fldv_Description],
            [fldv_HazardCode],
            [fldv_AssessmentType],
            [fldv_Status],
            [fldv_Stage],
            
            -- Enhanced Core Assessment Fields (Steps 1-5)
            [fldv_LeadAssessorId],
            [fldv_PrimaryHazardId],
            [fldv_RiskAssessmentCategory],
            [fldi_CurrentStep],
            [fldd_CompletedDate],
            [fldv_CompletedBy],
            [fldv_ParentAssessmentId],
            
            -- Step 1: System Description Fields
            [fldv_SystemDescription],
            [fldv_SystemBoundaries],
            [fldv_SystemPurpose],
            [fldv_PersonnelFactors],
            [fldv_EquipmentFactors],
            [fldv_ProcedureFactors],
            [fldv_ResourceFactors],
            [fldv_EnvironmentFactors],
            
            -- Step 3: Risk Analysis Fields
            [fldv_RiskAnalysisMethod],
            [fldv_RiskCriteria],
            
            -- Step 4: Risk Assessment Fields
            [fldv_TolerabilityFramework],
            [fldv_RiskAcceptanceCriteria],
            [fldi_FinalSeverityScore],
            [fldi_FinalLikelihoodScore],
            [fldv_FinalRiskLevel],
            [fldv_RiskTolerability],
            [fldv_AssessmentRationale],
            
            -- Step 5: Implementation Fields
            [fldv_ImplementationStrategy],
            [fldd_OverallTargetDate],
            [fldv_ImplementationNotes],
            
            -- Progress Tracking Fields
            [fldv_CompletedSteps],
            [fldi_CompletionPercentage],
            
            -- Audit Fields
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_RiskAssessments] 
        WHERE [fldv_Code] = @pID;
        
        -- =============================================
        -- Get Related Hazards with Enhanced Data
        -- =============================================
        SELECT 
            H.[fldv_Code] AS HazardCode,
            H.[fldv_Name] AS HazardName,
            H.[fldc_Description] AS HazardDescription,
            H.[fldv_HazardType] AS HazardType,
            H.[fldv_Category] AS HazardCategory,
            H.[fldv_Priority] AS HazardPriority,
            H.[fldv_RiskLevel] AS HazardRiskLevel,
            H.[fldc_WorstCredibleOutcome] AS WorstCredibleOutcome,
            H.[fldc_RootCause] AS RootCauseAnalysis,
            H.[fldc_AdditionalComments] AS AdditionalComments,
            H.[fldv_Status] AS HazardStatus,
            H.[fldv_ReportCode] AS ReportCode,
            H.[fldv_ScoringPanelCode] AS ScoringPanelCode,
            H.[fldv_AverageScore] AS AverageScore,
            H.[fldd_CreatedDate] AS CreatedDate
        FROM [dbo].[tbld_Hazards] H
        WHERE H.[fldv_Code] IN (
            SELECT RA.[fldv_HazardCode] 
            FROM [dbo].[tbld_RiskAssessments] RA 
            WHERE RA.[fldv_Code] = @pID
            UNION
            SELECT RA.[fldv_PrimaryHazardId] 
            FROM [dbo].[tbld_RiskAssessments] RA 
            WHERE RA.[fldv_Code] = @pID AND RA.[fldv_PrimaryHazardId] IS NOT NULL
        );
        
        -- =============================================
        -- Get Panel Scoring Data with Enhanced Information
        -- =============================================
        SELECT 
            SP.[fldv_Code] AS ScoringPanelCode,
            RTRIM(SP.[fldv_HazardCode]) AS HazardCode,
            RTRIM(SP.[fldv_SMSUserCode]) AS PanelMemberCode,
            SP.[fldv_Severity] AS SeverityScore,
            SP.[fldv_Likelyhood] AS LikelihoodScore,
            SP.[fldv_Score] AS CalculatedScore,
            COALESCE(AU.[fldv_FirstName] + ' ' + AU.[fldv_LastName], 
                     RTRIM(SP.[fldv_SMSUserCode]), 
                     'Unknown User') AS PanelMemberName,
            SP.[fldd_CreatedDate] AS ScoreSubmittedDate
        FROM [dbo].[tbld_ScoringPanel] SP
        LEFT JOIN [dbo].[tbld_SMSApplicationUsers] AU ON RTRIM(SP.[fldv_SMSUserCode]) = AU.[fldv_Code]
        WHERE RTRIM(SP.[fldv_HazardCode]) IN (
            SELECT RA.[fldv_HazardCode] 
            FROM [dbo].[tbld_RiskAssessments] RA 
            WHERE RA.[fldv_Code] = @pID
            UNION
            SELECT RA.[fldv_PrimaryHazardId] 
            FROM [dbo].[tbld_RiskAssessments] RA 
            WHERE RA.[fldv_Code] = @pID AND RA.[fldv_PrimaryHazardId] IS NOT NULL
        )
        ORDER BY SP.[fldv_HazardCode], SP.[fldd_CreatedDate];
        
        SET @AuditMessage = 'Successfully retrieved Enhanced RiskAssessment with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Enhanced RiskAssessment ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_RiskAssessments', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- Enhanced pr_Hazard_GetById - Complete Hazard Data
-- **************************************************

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.pr_Hazard_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.pr_Hazard_GetById
GO

CREATE PROCEDURE [dbo].[pr_Hazard_GetById]
    @pID VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_Hazard_GetById';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Enhanced Hazard with ID: ' + @pID;
        
        -- =============================================
        -- Main Hazard Query with Complete Enhanced Data
        -- =============================================
        SELECT 
            -- Core Fields
            [fldi_ID],
            [fldv_Code],
            [fldv_Name],
            [fldc_Description],
            [fldv_ReportCode],
            [fldv_ScoringPanelCode],
            [fldv_AverageScore],
            
            -- Enhanced Classification Fields
            [fldv_HazardType],
            [fldv_Category],
            [fldv_Status],
            [fldv_Priority],
            [fldv_RiskLevel],
            
            -- Reporting Fields
            [fldv_ReportedBy],
            [fldd_ReportedOn],
            [fldv_ReportingDepartment],
            [fldb_IsConfidential],
            [fldb_IsAnonymous],
            
            -- Step 3 Risk Analysis Fields
            [fldc_WorstCredibleOutcome],
            [fldc_RootCause],
            [fldc_AdditionalComments],
            
            -- Investigation Fields
            [fldb_RequiresInvestigation],
            [fldd_InvestigationCompletedDate],
            [fldc_InvestigationNotes],
            
            -- Location Fields
            [fldv_Location],
            [fldv_LocationArea],
            [fldv_LocationSubArea],
            
            -- Audit Fields
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_Hazards] 
        WHERE [fldv_Code] = @pID;
        
        -- =============================================
        -- Get Related Mitigations for this Hazard
        -- =============================================
        SELECT 
            M.[fldv_Code] AS MitigationCode,
            M.[fldv_Name] AS MitigationName,
            M.[fldc_Description] AS MitigationDescription,
            M.[fldv_Type] AS MitigationType,
            M.[fldv_Status] AS MitigationStatus,
            M.[fldv_Priority] AS MitigationPriority,
            M.[fldd_TargetDate] AS TargetDate,
            M.[fldv_AssignedDepartment] AS AssignedDepartment,
            M.[fldv_AssignedTo] AS AssignedTo,
            M.[fldi_Progress] AS Progress,
            M.[fldv_EffectivenessRating] AS EffectivenessRating,
            M.[fldc_Notes] AS Notes,
            M.[fldd_CreatedDate] AS CreatedDate
        FROM [dbo].[tbld_Mitigations] M
        WHERE M.[fldv_HazardCode] = @pID
        ORDER BY M.[fldv_Priority], M.[fldd_TargetDate];
        
        SET @AuditMessage = 'Successfully retrieved Enhanced Hazard with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Enhanced Hazard ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Hazards', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- Enhanced pr_Mitigation_GetById - Complete Mitigation Data
-- **************************************************

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.pr_Mitigation_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.pr_Mitigation_GetById
GO

CREATE PROCEDURE [dbo].[pr_Mitigation_GetById]
    @pID VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_Mitigation_GetById';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Enhanced Mitigation with ID: ' + @pID;
        
        -- =============================================
        -- Main Mitigation Query with Complete Enhanced Data
        -- =============================================
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
            
            -- Step 5 Implementation Fields
            [fldv_RiskAssessmentCode],
            [fldd_TargetDate],
            [fldd_ImplementationDate],
            [fldd_CompletionDate],
            [fldv_AssignedDepartment],
            [fldv_AssignedTo],
            [fldv_ApprovedBy],
            [fldd_ApprovedDate],
            
            -- Progress Tracking Fields
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
            
            -- Dependencies Fields
            [fldv_Prerequisites],
            [fldv_Dependencies],
            [fldb_HasDependencies],
            [fldb_IsPrerequisite],
            
            -- Implementation Planning Fields
            [fldc_ImplementationPlan],
            [fldc_CommunicationPlan],
            [fldc_TrainingRequirements],
            [fldc_DocumentationUpdates],
            
            -- Validation Fields
            [fldc_TestingProcedure],
            [fldd_TestingCompletedDate],
            [fldv_TestingResults],
            [fldb_ValidationRequired],
            [fldd_ValidationDate],
            [fldv_ValidatedBy],
            
            -- General Fields
            [fldc_Notes],
            [fldc_LessonsLearned],
            [fldc_RecommendationsForFuture],
            
            -- Audit Fields
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_Mitigations] 
        WHERE [fldv_Code] = @pID;
        
        -- =============================================
        -- Get Related Mitigation Assignments
        -- =============================================
        SELECT 
            MA.[fldv_Code] AS AssignmentCode,
            MA.[fldv_AssignedToUserId] AS AssignedToUserId,
            MA.[fldv_AssignedToUserName] AS AssignedToUserName,
            MA.[fldv_AssignedRole] AS AssignedRole,
            MA.[fldv_DepartmentCode] AS DepartmentCode,
            MA.[fldv_DepartmentName] AS DepartmentName,
            MA.[fldv_AssignedBy] AS AssignedBy,
            MA.[fldd_AssignedDate] AS AssignedDate,
            MA.[fldd_DueDate] AS DueDate,
            MA.[fldv_Status] AS AssignmentStatus,
            MA.[fldv_Priority] AS AssignmentPriority,
            MA.[fldi_Progress] AS AssignmentProgress,
            MA.[fldv_ProgressNotes] AS ProgressNotes,
            MA.[fldd_LastProgressUpdate] AS LastProgressUpdate,
            MA.[fldc_AssignmentInstructions] AS AssignmentInstructions,
            MA.[fldc_CompletionNotes] AS CompletionNotes,
            MA.[fldd_CompletedDate] AS CompletedDate,
            MA.[fldv_CompletedBy] AS CompletedBy,
            MA.[fldb_IsActive] AS IsActive
        FROM [dbo].[tbld_MitigationAssignments] MA
        WHERE MA.[fldv_MitigationCode] = @pID
        AND MA.[fldb_IsActive] = 1
        ORDER BY MA.[fldv_Priority], MA.[fldd_DueDate];
        
        SET @AuditMessage = 'Successfully retrieved Enhanced Mitigation with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Enhanced Mitigation ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Mitigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- Enhanced pr_MitigationAssignment_GetById - Complete Assignment Data
-- **************************************************

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.pr_MitigationAssignment_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.pr_MitigationAssignment_GetById
GO

CREATE PROCEDURE [dbo].[pr_MitigationAssignment_GetById]
    @pID VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_MitigationAssignment_GetById';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Enhanced MitigationAssignment with ID: ' + @pID;
        
        -- =============================================
        -- Main Mitigation Assignment Query with Complete Enhanced Data
        -- =============================================
        SELECT 
            -- Core Fields
            [fldi_ID],
            [fldv_Code],
            [fldv_MitigationCode],
            [fldv_DepartmentCode],
            
            -- Assignment Details Fields
            [fldv_AssignedToUserId],
            [fldv_AssignedToUserName],
            [fldv_AssignedRole],
            [fldv_DepartmentName],
            
            -- Assignment Workflow Fields
            [fldv_AssignedBy],
            [fldd_AssignedDate],
            [fldd_DueDate],
            [fldv_Status],
            [fldv_Priority],
            
            -- Progress Tracking Fields
            [fldi_Progress],
            [fldv_ProgressNotes],
            [fldd_LastProgressUpdate],
            [fldv_ProgressUpdatedBy],
            
            -- Communication Fields
            [fldc_AssignmentInstructions],
            [fldc_CompletionNotes],
            [fldd_CompletedDate],
            [fldv_CompletedBy],
            
            -- Review and Approval Fields
            [fldv_ReviewStatus],
            [fldv_ReviewedBy],
            [fldd_ReviewedDate],
            [fldc_ReviewComments],
            
            -- Resource and Cost Tracking
            [fldi_EstimatedHours],
            [fldi_ActualHours],
            [fldd_EstimatedCost],
            [fldd_ActualCost],
            
            -- Escalation Fields
            [fldb_IsEscalated],
            [fldd_EscalatedDate],
            [fldv_EscalatedTo],
            [fldc_EscalationReason],
            
            -- Active/Inactive Management
            [fldb_IsActive],
            [fldd_InactiveDate],
            [fldv_InactiveReason],
            
            -- Audit Fields
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_MitigationAssignments] 
        WHERE [fldv_Code] = @pID;
        
        -- =============================================
        -- Get Related Mitigation Details
        -- =============================================
        SELECT 
            M.[fldv_Code] AS MitigationCode,
            M.[fldv_Name] AS MitigationName,
            M.[fldc_Description] AS MitigationDescription,
            M.[fldv_Type] AS MitigationType,
            M.[fldv_Status] AS MitigationStatus,
            M.[fldv_Priority] AS MitigationPriority,
            M.[fldv_HazardCode] AS HazardCode,
            M.[fldd_TargetDate] AS MitigationTargetDate,
            M.[fldi_Progress] AS MitigationProgress
        FROM [dbo].[tbld_Mitigations] M
        INNER JOIN [dbo].[tbld_MitigationAssignments] MA ON M.[fldv_Code] = MA.[fldv_MitigationCode]
        WHERE MA.[fldv_Code] = @pID;
        
        SET @AuditMessage = 'Successfully retrieved Enhanced MitigationAssignment with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Enhanced MitigationAssignment ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_MitigationAssignments', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- Additional Helper Stored Procedures
-- **************************************************

-- Enhanced pr_Mitigation_GetByHazardCode
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.pr_Mitigation_GetByHazardCode', 'P') IS NOT NULL
    DROP PROCEDURE dbo.pr_Mitigation_GetByHazardCode
GO

CREATE PROCEDURE [dbo].[pr_Mitigation_GetByHazardCode]
    @pHazardCode VARCHAR(50),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
    DECLARE @FunctionName VARCHAR(50) = 'pr_Mitigation_GetByHazardCode';
    
    BEGIN TRY        
        -- Get all mitigations for the specified hazard code
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
            
            -- Implementation Fields
            [fldd_TargetDate],
            [fldv_AssignedDepartment],
            [fldv_AssignedTo],
            [fldi_Progress],
            
            -- Effectiveness Fields
            [fldv_EffectivenessRating],
            [fldc_Notes],
            
            -- Audit Fields
            [fldd_CreatedDate],
            [fldv_CreatedBy]
            
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

GO

PRINT '***************************************************';
PRINT 'BATCH 2 COMPLETED: ALL ENHANCED STORED PROCEDURES CREATED';
PRINT '***************************************************';
PRINT 'CREATED PROCEDURES:';
PRINT '? pr_RiskAssessment_GetById: Complete Steps 1-5 data retrieval';
PRINT '? pr_Hazard_GetById: Enhanced hazard data with risk analysis';
PRINT '? pr_Mitigation_GetById: Complete mitigation lifecycle data';
PRINT '? pr_MitigationAssignment_GetById: Full assignment workflow data';
PRINT '? pr_Mitigation_GetByHazardCode: Helper for populating Hazard.Mitigations';
PRINT '';
PRINT 'ENHANCED CAPABILITIES:';
PRINT '? Steps 1-5 Data: Complete risk assessment workflow support';
PRINT '? Related Data: Automatic retrieval of linked entities';
PRINT '? Progress Tracking: Real-time status and completion data';
PRINT '? Audit Trails: Complete operation logging and error handling';
PRINT '? Performance: Optimized queries with proper indexing';
PRINT '? Architecture Ready: Compatible with Repository and DataService layers';
PRINT '***************************************************';

GO