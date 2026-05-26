USE [SMS]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ============================================================
   Get one queue row by QueueGuid
   ============================================================ */
CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_GetByQueueGuid]
    @fldv_QueueGuid nvarchar(36)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        q.*
    FROM dbo.tbld_EventQueue q
    WHERE q.fldv_QueueGuid = @fldv_QueueGuid;
END
GO

/* ============================================================
   Get pending queue rows (without leasing)
   ============================================================ */
CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_GetPending]
    @fldi_MaxResults int = 100,
    @fldi_EventCategory int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@fldi_MaxResults)
        q.*
    FROM dbo.tbld_EventQueue q
    WHERE q.fldi_Status = 0
      AND (q.fldd_NextAttemptDate IS NULL OR q.fldd_NextAttemptDate <= GETUTCDATE())
      AND (@fldi_EventCategory IS NULL OR q.fldi_EventCategory = @fldi_EventCategory)
    ORDER BY
        q.fldi_Priority DESC,
        q.fldd_QueuedDate ASC;
END
GO

/* ============================================================
   Cancel one queued event (pending or processing)
   ============================================================ */
CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_Cancel]
    @fldv_QueueGuid nvarchar(36),
    @fldv_CancelledBy nvarchar(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tbld_EventQueue
       SET fldi_Status = 4, -- Cancelled
           fldv_LockedBy = NULL,
           fldd_LockExpiresDate = NULL,
           fldv_UpdatedBy = @fldv_CancelledBy,
           fldd_UpdatedDate = GETUTCDATE()
     WHERE fldv_QueueGuid = @fldv_QueueGuid
       AND fldi_Status IN (0,1);

    SELECT @@ROWCOUNT AS fldi_RowsAffected;
END
GO

/* ============================================================
   Clear completed/failed/cancelled queue rows
   ============================================================ */
CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_ClearCompleted]
    @fldv_ClearedBy nvarchar(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @tbl TABLE (fldi_ID int);

    DELETE q
    OUTPUT deleted.fldi_ID INTO @tbl(fldi_ID)
    FROM dbo.tbld_EventQueue q
    WHERE q.fldi_Status IN (2,3,4); -- Processed, Failed, Cancelled

    SELECT COUNT(1) AS fldi_RemovedCount FROM @tbl;
END
GO

/* ============================================================
   Clear all queue rows
   ============================================================ */
CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_ClearAll]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @fldi_BeforeCount int;
    SELECT @fldi_BeforeCount = COUNT(1) FROM dbo.tbld_EventQueue;

    DELETE FROM dbo.tbld_EventQueue;

    SELECT @fldi_BeforeCount AS fldi_RemovedCount;
END
GO

/* ============================================================
   Recover expired processing locks
   ============================================================ */
CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_RecoverExpiredLocks]
    @fldi_RecoveryDelaySeconds int = 5,
    @fldv_RecoveredBy nvarchar(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tbld_EventQueue
       SET fldi_Status = 0, -- back to Pending
           fldv_LockedBy = NULL,
           fldd_LockExpiresDate = NULL,
           fldd_ProcessingStartedDate = NULL,
           fldd_NextAttemptDate = DATEADD(SECOND, @fldi_RecoveryDelaySeconds, GETUTCDATE()),
           fldv_UpdatedBy = @fldv_RecoveredBy,
           fldd_UpdatedDate = GETUTCDATE()
     WHERE fldi_Status = 1 -- Processing
       AND fldd_LockExpiresDate IS NOT NULL
       AND fldd_LockExpiresDate < GETUTCDATE();

    SELECT @@ROWCOUNT AS fldi_RecoveredCount;
END
GO

/* ============================================================
   Queue statistics
   ============================================================ */
CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_GetStats]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SUM(CASE WHEN fldi_Status = 0 THEN 1 ELSE 0 END) AS fldi_PendingCount,
        SUM(CASE WHEN fldi_Status = 2 THEN 1 ELSE 0 END) AS fldi_ProcessedCount,
        SUM(CASE WHEN fldi_Status = 3 THEN 1 ELSE 0 END) AS fldi_FailedCount,
        SUM(CASE WHEN fldi_Status = 4 THEN 1 ELSE 0 END) AS fldi_CancelledCount,
        COUNT(1) AS fldi_TotalCount
    FROM dbo.tbld_EventQueue;

    SELECT
        fldi_EventCategory,
        SUM(CASE WHEN fldi_Status = 0 THEN 1 ELSE 0 END) AS fldi_Pending,
        SUM(CASE WHEN fldi_Status = 2 THEN 1 ELSE 0 END) AS fldi_Processed,
        SUM(CASE WHEN fldi_Status = 3 THEN 1 ELSE 0 END) AS fldi_Failed,
        SUM(CASE WHEN fldi_Status = 4 THEN 1 ELSE 0 END) AS fldi_Cancelled,
        COUNT(1) AS fldi_Total
    FROM dbo.tbld_EventQueue
    GROUP BY fldi_EventCategory;

    SELECT
        fldi_Priority,
        COUNT(1) AS fldi_Count
    FROM dbo.tbld_EventQueue
    GROUP BY fldi_Priority;
END
GO