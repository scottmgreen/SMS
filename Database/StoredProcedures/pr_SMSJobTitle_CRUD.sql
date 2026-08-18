USE [SMS]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[tbld_SMSJobTitles]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[tbld_SMSJobTitles]
	(
		[fldi_Id] INT IDENTITY(1,1) NOT NULL,
		[fldv_Code] NCHAR(10) NOT NULL,
		[fldv_Name] NCHAR(60) NOT NULL,
		[fldv_CreatedBy] NVARCHAR(50) NULL,
		[fldd_CreatedDate] DATETIME NULL,
		[fldv_UpdatedBy] NVARCHAR(50) NULL,
		[fldd_UpdatedDate] DATETIME NULL,
		CONSTRAINT [PK_tbld_SMSJobTitles] PRIMARY KEY CLUSTERED ([fldi_Id] ASC),
		CONSTRAINT [UQ_tbld_SMSJobTitles_Code] UNIQUE ([fldv_Code])
	) ON [PRIMARY]
END
GO

IF OBJECT_ID('[dbo].[pr_SMSJobTitle_GetAll]', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[pr_SMSJobTitle_GetAll]
GO
CREATE PROCEDURE [dbo].[pr_SMSJobTitle_GetAll]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[fldi_Id],
		[fldv_Code],
		[fldv_Name],
		[fldv_CreatedBy],
		[fldd_CreatedDate],
		[fldv_UpdatedBy],
		[fldd_UpdatedDate]
	FROM [dbo].[tbld_SMSJobTitles]
	ORDER BY [fldv_Name];
END
GO

IF OBJECT_ID('[dbo].[pr_SMSJobTitle_GetByCode]', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[pr_SMSJobTitle_GetByCode]
GO
CREATE PROCEDURE [dbo].[pr_SMSJobTitle_GetByCode]
	@pCode NCHAR(10)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[fldi_Id],
		[fldv_Code],
		[fldv_Name],
		[fldv_CreatedBy],
		[fldd_CreatedDate],
		[fldv_UpdatedBy],
		[fldd_UpdatedDate]
	FROM [dbo].[tbld_SMSJobTitles]
	WHERE [fldv_Code] = @pCode;
END
GO

IF OBJECT_ID('[dbo].[pr_SMSJobTitle_Insert]', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[pr_SMSJobTitle_Insert]
GO
CREATE PROCEDURE [dbo].[pr_SMSJobTitle_Insert]
	@pCode NCHAR(10),
	@pName NCHAR(60),
	@pCreatedBy NVARCHAR(50),
	@pCreatedDate DATETIME = NULL,
	@pNewID INT OUTPUT,
	@pNewCode NCHAR(10) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	IF @pCreatedDate IS NULL
		SET @pCreatedDate = GETDATE();

	INSERT INTO [dbo].[tbld_SMSJobTitles]
	(
		[fldv_Code],
		[fldv_Name],
		[fldv_CreatedBy],
		[fldd_CreatedDate]
	)
	VALUES
	(
		@pCode,
		@pName,
		@pCreatedBy,
		@pCreatedDate
	);

	SET @pNewID = SCOPE_IDENTITY();
	SET @pNewCode = @pCode;
END
GO

IF OBJECT_ID('[dbo].[pr_SMSJobTitle_Update]', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[pr_SMSJobTitle_Update]
GO
CREATE PROCEDURE [dbo].[pr_SMSJobTitle_Update]
	@pCode NCHAR(10),
	@pName NCHAR(60),
	@pUpdatedBy NVARCHAR(50),
	@pUpdatedDate DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF @pUpdatedDate IS NULL
		SET @pUpdatedDate = GETDATE();

	UPDATE [dbo].[tbld_SMSJobTitles]
	SET
		[fldv_Name] = @pName,
		[fldv_UpdatedBy] = @pUpdatedBy,
		[fldd_UpdatedDate] = @pUpdatedDate
	WHERE [fldv_Code] = @pCode;
END
GO

IF OBJECT_ID('[dbo].[pr_SMSJobTitle_Delete]', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[pr_SMSJobTitle_Delete]
GO
CREATE PROCEDURE [dbo].[pr_SMSJobTitle_Delete]
	@pCode NCHAR(10),
	@pDeletedBy NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[tbld_SMSJobTitles]
	WHERE [fldv_Code] = @pCode;
END
GO
