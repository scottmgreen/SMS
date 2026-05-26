USE [SMS]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[pr_EventQueue_GetByStatus]
(
      @pmEventStatus     INT
    , @pmMaxResults      INT = 100
    , @pmEventCategory   INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@pmMaxResults)
          EQ.[fldi_ID]
        , EQ.[fldv_Code]
        , EQ.[fldv_QueueGuid]
        , EQ.[fldi_EventCategory]
        , EQ.[fldv_EventType]
        , EQ.[fldv_EventData]
        , EQ.[fldv_ReportCode]
        , EQ.[fldv_TargetSystem]
        , EQ.[fldi_Priority]
        , EQ.[fldi_Status]
        , EQ.[fldi_AttemptCount]
        , EQ.[fldi_MaxAttempts]
        , EQ.[fldv_LastError]
        , EQ.[fldd_QueuedDate]
        , EQ.[fldd_ProcessingStartedDate]
        , EQ.[fldd_ProcessedDate]
        , EQ.[fldd_NextAttemptDate]
        , EQ.[fldv_LockedBy]
        , EQ.[fldd_LockExpiresDate]
        , EQ.[fldv_QueuedBy]
        , EQ.[fldv_CorrelationId]
        , EQ.[fldv_CausationId]
        , EQ.[fldv_CreatedBy]
        , EQ.[fldd_CreatedDate]
        , EQ.[fldv_UpdatedBy]
        , EQ.[fldd_UpdatedDate]
    FROM [dbo].[tbld_EventQueue] AS EQ
    WHERE EQ.[fldi_Status] = @pmEventStatus
      AND (
            @pmEventCategory IS NULL
            OR EQ.[fldi_EventCategory] = @pmEventCategory
          )
    ORDER BY
          EQ.[fldi_Priority] DESC
        , EQ.[fldd_QueuedDate] ASC;
END
GO