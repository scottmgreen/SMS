USE [SMS]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tbld_EventQueueAttempts](
    [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
    [fldi_EventQueueID] [int] NOT NULL,
    [fldi_AttemptNumber] [int] NOT NULL,
    [fldd_StartedDate] [datetime2](7) NOT NULL,
    [fldd_CompletedDate] [datetime2](7) NULL,
    [fldb_IsSuccess] [bit] NULL,
    [fldv_Worker] [nvarchar](100) NULL,
    [fldv_Error] [nvarchar](2000) NULL,
    [fldv_CreatedBy] [nvarchar](50) NOT NULL,
    [fldd_CreatedDate] [datetime2](7) NOT NULL,
    [fldv_UpdatedBy] [nvarchar](50) NULL,
    [fldd_UpdatedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_tbld_EventQueueAttempts] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
 CONSTRAINT [FK_tbld_EventQueueAttempts_tbld_EventQueue]
    FOREIGN KEY ([fldi_EventQueueID]) REFERENCES [dbo].[tbld_EventQueue]([fldi_ID])
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[tbld_EventQueueAttempts] ADD DEFAULT (getutcdate()) FOR [fldd_StartedDate]
GO
ALTER TABLE [dbo].[tbld_EventQueueAttempts] ADD DEFAULT ('SYSTEM') FOR [fldv_CreatedBy]
GO
ALTER TABLE [dbo].[tbld_EventQueueAttempts] ADD DEFAULT (getutcdate()) FOR [fldd_CreatedDate]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_EventQueueAttempts_EventQueueID]
ON [dbo].[tbld_EventQueueAttempts] ([fldi_EventQueueID],[fldi_AttemptNumber]);
GO