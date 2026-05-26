CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_MarkFailed]
    @fldv_QueueGuid nvarchar(36),
    @fldv_Worker nvarchar(100),
    @fldv_LastError nvarchar(2000),
    @fldi_BackoffSeconds int = 30
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE q
       SET q.fldi_AttemptCount = q.fldi_AttemptCount + 1,
           q.fldv_LastError = @fldv_LastError,
           q.fldi_Status = CASE WHEN q.fldi_AttemptCount + 1 >= q.fldi_MaxAttempts THEN 3 ELSE 0 END,
           q.fldd_NextAttemptDate = CASE WHEN q.fldi_AttemptCount + 1 >= q.fldi_MaxAttempts THEN NULL ELSE DATEADD(SECOND, @fldi_BackoffSeconds, GETUTCDATE()) END,
           q.fldv_LockedBy = NULL,
           q.fldd_LockExpiresDate = NULL,
           q.fldv_UpdatedBy = @fldv_Worker,
           q.fldd_UpdatedDate = GETUTCDATE()
      FROM dbo.tbld_EventQueue q
     WHERE q.fldv_QueueGuid = @fldv_QueueGuid
       AND q.fldi_Status = 1
       AND q.fldv_LockedBy = @fldv_Worker;
END
GO