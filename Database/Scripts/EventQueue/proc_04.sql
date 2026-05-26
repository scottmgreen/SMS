CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_Insert]
    @fldv_Code nvarchar(50),
    @fldv_QueueGuid nvarchar(36),
    @fldi_EventCategory int,
    @fldv_EventType nvarchar(100),
    @fldv_EventData nvarchar(max),
    @fldv_ReportCode nvarchar(50) = NULL,
    @fldv_TargetSystem nvarchar(100) = NULL,
    @fldi_Priority int = 1,
    @fldi_MaxAttempts int = 5,
    @fldv_QueuedBy nvarchar(50) = 'SYSTEM',
    @fldv_CreatedBy nvarchar(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tbld_EventQueue
    (
        fldv_Code, fldv_QueueGuid, fldi_EventCategory, fldv_EventType, fldv_EventData,
        fldv_ReportCode, fldv_TargetSystem, fldi_Priority, fldi_Status, fldi_AttemptCount,
        fldi_MaxAttempts, fldv_QueuedBy, fldv_CreatedBy, fldd_CreatedDate
    )
    VALUES
    (
        @fldv_Code, @fldv_QueueGuid, @fldi_EventCategory, @fldv_EventType, @fldv_EventData,
        @fldv_ReportCode, @fldv_TargetSystem, @fldi_Priority, 0, 0,
        @fldi_MaxAttempts, @fldv_QueuedBy, @fldv_CreatedBy, GETUTCDATE()
    );

    SELECT SCOPE_IDENTITY() AS fldi_ID;
END
GO