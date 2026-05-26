CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_LeaseBatch]
    @fldi_BatchSize int = 25,
    @fldv_Worker nvarchar(100),
    @fldi_LockSeconds int = 60
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH cte AS
    (
        SELECT TOP (@fldi_BatchSize) q.fldi_ID
        FROM dbo.tbld_EventQueue q WITH (UPDLOCK, READPAST, ROWLOCK)
        WHERE q.fldi_Status = 0
          AND (q.fldd_NextAttemptDate IS NULL OR q.fldd_NextAttemptDate <= GETUTCDATE())
        ORDER BY q.fldi_Priority DESC, q.fldd_QueuedDate ASC
    )
    UPDATE q
       SET q.fldi_Status = 1,
           q.fldd_ProcessingStartedDate = GETUTCDATE(),
           q.fldv_LockedBy = @fldv_Worker,
           q.fldd_LockExpiresDate = DATEADD(SECOND, @fldi_LockSeconds, GETUTCDATE()),
           q.fldv_UpdatedBy = @fldv_Worker,
           q.fldd_UpdatedDate = GETUTCDATE()
    OUTPUT inserted.*
      FROM dbo.tbld_EventQueue q
      JOIN cte ON cte.fldi_ID = q.fldi_ID;
END
GO