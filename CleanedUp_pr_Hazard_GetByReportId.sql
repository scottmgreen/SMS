-- =============================================
-- Author: System  
-- Create date: [Date]
-- Description: Get all hazards by Report ID (CLEANED UP - NO MITIGATIONS)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_Hazard_GetByReportId]
    @pID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        -- Core Fields
        h.[fldi_ID],
        h.[fldv_Code],
        h.[fldv_Name],
        h.[fldc_Description],
        h.[fldv_Category],
        h.[fldv_FiveMComponent],
        h.[fldv_ReportCode],
        h.[fldv_RiskMatrixCode],
        h.[fldv_AverageScore],
        
        -- Enhanced Classification Fields
        h.[fldv_HazardType],
        h.[fldv_Category],
        h.[fldv_Status],
        h.[fldv_Priority],
        h.[fldv_RiskLevel],
        
        -- Reporting Fields
        h.[fldv_ReportedBy],
        h.[fldd_ReportedOn],
        h.[fldv_ReportingDepartment],
        h.[fldb_IsConfidential],
        h.[fldb_IsAnonymous],
        
        -- Step 3 Risk Analysis Fields
        r.[fldv_WorstCredibleOutcome],
        r.[fldv_RootCause],
        r.[fldv_AdditionalComments],
        
        -- Investigation Fields
        h.[fldb_RequiresInvestigation],
        h.[fldd_InvestigationCompletedDate],
        h.[fldc_InvestigationNotes],
        
        -- Location Fields
        h.[fldv_Location],
        h.[fldv_LocationArea],
        h.[fldv_LocationSubArea],
        
        -- Audit Fields
        h.[fldv_CreatedBy],
        h.[fldd_CreatedDate],
        h.[fldv_UpdatedBy],
        h.[fldd_UpdatedDate]
        
    FROM [dbo].[tbld_Hazards] h
    LEFT JOIN [dbo].[tbld_RiskAnalysis] r ON h.[fldv_Code] = r.[fldv_HazardCode]
    WHERE h.[fldv_ReportCode] = @pID
    ORDER BY h.[fldd_CreatedDate];
    
END