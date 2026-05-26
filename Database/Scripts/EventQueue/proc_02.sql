CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_MarkProcessed]
    @fldv_QueueGuid nvarchar(36),
    @fldv_Worker nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tbld_EventQueue
       SET fldi_Status = 2,
           fldi_AttemptCount = fldi_AttemptCount + 1,
           fldd_ProcessedDate = GETUTCDATE(),
           fldv_LastError = NULL,
           fldv_LockedBy = NULL,
           fldd_LockExpiresDate = NULL,
           fldv_UpdatedBy = @fldv_Worker,
           fldd_UpdatedDate = GETUTCDATE()
     WHERE fldv_QueueGuid = @fldv_QueueGuid
       AND fldi_Status = 1
       AND fldv_LockedBy = @fldv_Worker;
END
GO