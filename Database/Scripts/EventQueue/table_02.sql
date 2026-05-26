USE [SMS]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tbld_EventQueue](
    [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
    [fldv_Code] [nvarchar](50) NOT NULL,                  -- EVQ-...
    [fldv_QueueGuid] [nvarchar](36) NOT NULL,             -- maps current Guid queue id
    [fldi_EventCategory] [int] NOT NULL,                  -- 0=Domain,1=UI,2=Integration
    [fldv_EventType] [nvarchar](100) NOT NULL,            -- HAZARD_CREATED, etc.
    [fldv_EventData] [nvarchar](max) NOT NULL,            -- JSON payload
    [fldv_ReportCode] [nvarchar](50) NULL,
    [fldv_TargetSystem] [nvarchar](100) NULL,
    [fldi_Priority] [int] NOT NULL,                       -- 0=Low,1=Normal,2=High,3=Critical
    [fldi_Status] [int] NOT NULL,                         -- 0=Pending,1=Processing,2=Processed,3=Failed,4=Cancelled
    [fldi_AttemptCount] [int] NOT NULL,
    [fldi_MaxAttempts] [int] NOT NULL,
    [fldv_LastError] [nvarchar](2000) NULL,
    [fldd_QueuedDate] [datetime2](7) NOT NULL,
    [fldd_ProcessingStartedDate] [datetime2](7) NULL,
    [fldd_ProcessedDate] [datetime2](7) NULL,
    [fldd_NextAttemptDate] [datetime2](7) NULL,
    [fldv_LockedBy] [nvarchar](100) NULL,
    [fldd_LockExpiresDate] [datetime2](7) NULL,
    [fldv_QueuedBy] [nvarchar](50) NOT NULL,
    [fldv_CorrelationId] [nvarchar](36) NULL,
    [fldv_CausationId] [nvarchar](36) NULL,
    [fldv_CreatedBy] [nvarchar](50) NOT NULL,
    [fldd_CreatedDate] [datetime2](7) NOT NULL,
    [fldv_UpdatedBy] [nvarchar](50) NULL,
    [fldd_UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_tbld_EventQueue] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
 CONSTRAINT [UQ_tbld_EventQueue_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC),
 CONSTRAINT [UQ_tbld_EventQueue_QueueGuid] UNIQUE NONCLUSTERED ([fldv_QueueGuid] ASC),
 CONSTRAINT [CK_tbld_EventQueue_EventCategory] CHECK ([fldi_EventCategory] IN (0,1,2)),
 CONSTRAINT [CK_tbld_EventQueue_Status] CHECK ([fldi_Status] IN (0,1,2,3,4)),
 CONSTRAINT [CK_tbld_EventQueue_Priority] CHECK ([fldi_Priority] IN (0,1,2,3)),
 CONSTRAINT [CK_tbld_EventQueue_AttemptCount] CHECK ([fldi_AttemptCount] >= 0),
 CONSTRAINT [CK_tbld_EventQueue_MaxAttempts] CHECK ([fldi_MaxAttempts] > 0)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[tbld_EventQueue] ADD DEFAULT ('SYSTEM') FOR [fldv_QueuedBy]
GO
ALTER TABLE [dbo].[tbld_EventQueue] ADD DEFAULT ((1)) FOR [fldi_Priority]
GO
ALTER TABLE [dbo].[tbld_EventQueue] ADD DEFAULT ((0)) FOR [fldi_Status]
GO
ALTER TABLE [dbo].[tbld_EventQueue] ADD DEFAULT ((0)) FOR [fldi_AttemptCount]
GO
ALTER TABLE [dbo].[tbld_EventQueue] ADD DEFAULT ((5)) FOR [fldi_MaxAttempts]
GO
ALTER TABLE [dbo].[tbld_EventQueue] ADD DEFAULT (getutcdate()) FOR [fldd_QueuedDate]
GO
ALTER TABLE [dbo].[tbld_EventQueue] ADD DEFAULT ('SYSTEM') FOR [fldv_CreatedBy]
GO
ALTER TABLE [dbo].[tbld_EventQueue] ADD DEFAULT (getutcdate()) FOR [fldd_CreatedDate]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_EventQueue_Status_NextAttempt_Priority]
ON [dbo].[tbld_EventQueue] ([fldi_Status],[fldd_NextAttemptDate],[fldi_Priority],[fldd_QueuedDate]);
GO

CREATE NONCLUSTERED INDEX [IX_tbld_EventQueue_ReportCode]
ON [dbo].[tbld_EventQueue] ([fldv_ReportCode],[fldd_QueuedDate]);
GO

CREATE NONCLUSTERED INDEX [IX_tbld_EventQueue_EventType]
ON [dbo].[tbld_EventQueue] ([fldv_EventType],[fldd_QueuedDate]);
GO