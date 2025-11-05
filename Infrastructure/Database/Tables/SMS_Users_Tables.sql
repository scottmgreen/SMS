USE [PDXSMS_V2]
GO

/****** Object:  Table [dbo].[tbld_SMSApplicationUsers]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tbld_SMSApplicationUsers](
    [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
    [fldv_Code] [varchar](60) NOT NULL,
    [fldv_FirstName] [nvarchar](100) NOT NULL,
    [fldv_LastName] [nvarchar](100) NOT NULL,
    [fldv_UserName] [nvarchar](50) NOT NULL,
    [fldv_Password] [nvarchar](255) NOT NULL,
    [fldv_ApplicationRole] [nvarchar](100) NOT NULL,
    [fldv_PermissionLevel] [nvarchar](50) NOT NULL DEFAULT ('Standard'),
    [fldb_IsActive] [bit] NOT NULL DEFAULT (1),
    [fldd_LastLoginDate] [datetime2](7) NULL,
    [fldv_CreatedBy] [nvarchar](50) NOT NULL,
    [fldd_CreatedDate] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
    [fldv_UpdatedBy] [nvarchar](50) NULL,
    [fldd_UpdatedDate] [datetime2](7) NULL,
    CONSTRAINT [PK_tbld_SMSApplicationUsers] PRIMARY KEY CLUSTERED 
    (
        [fldi_ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
    CONSTRAINT [UK_tbld_SMSApplicationUsers_Code] UNIQUE NONCLUSTERED 
    (
        [fldv_Code] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
    CONSTRAINT [UK_tbld_SMSApplicationUsers_UserName] UNIQUE NONCLUSTERED 
    (
        [fldv_UserName] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX [IX_tbld_SMSApplicationUsers_ApplicationRole] ON [dbo].[tbld_SMSApplicationUsers]
(
    [fldv_ApplicationRole] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSApplicationUsers_IsActive] ON [dbo].[tbld_SMSApplicationUsers]
(
    [fldb_IsActive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSApplicationUsers_PermissionLevel] ON [dbo].[tbld_SMSApplicationUsers]
(
    [fldv_PermissionLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSApplicationUsers_Name] ON [dbo].[tbld_SMSApplicationUsers]
(
    [fldv_LastName] ASC,
    [fldv_FirstName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[tbld_SMSOrganizationalUsers]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tbld_SMSOrganizationalUsers](
    [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
    [fldv_Code] [varchar](60) NOT NULL,
    [fldv_FirstName] [nvarchar](100) NOT NULL,
    [fldv_LastName] [nvarchar](100) NOT NULL,
    [fldv_UserName] [nvarchar](50) NOT NULL,
    [fldv_Password] [nvarchar](255) NOT NULL,
    [fldv_Department] [nvarchar](100) NOT NULL,
    [fldv_Position] [nvarchar](100) NOT NULL,
    [fldv_OrganizationLevel] [nvarchar](50) NOT NULL DEFAULT ('Standard'),
    [fldb_IsActive] [bit] NOT NULL DEFAULT (1),
    [fldd_LastLoginDate] [datetime2](7) NULL,
    [fldv_CreatedBy] [nvarchar](50) NOT NULL,
    [fldd_CreatedDate] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
    [fldv_UpdatedBy] [nvarchar](50) NULL,
    [fldd_UpdatedDate] [datetime2](7) NULL,
    CONSTRAINT [PK_tbld_SMSOrganizationalUsers] PRIMARY KEY CLUSTERED 
    (
        [fldi_ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
    CONSTRAINT [UK_tbld_SMSOrganizationalUsers_Code] UNIQUE NONCLUSTERED 
    (
        [fldv_Code] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
    CONSTRAINT [UK_tbld_SMSOrganizationalUsers_UserName] UNIQUE NONCLUSTERED 
    (
        [fldv_UserName] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX [IX_tbld_SMSOrganizationalUsers_Department] ON [dbo].[tbld_SMSOrganizationalUsers]
(
    [fldv_Department] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSOrganizationalUsers_Position] ON [dbo].[tbld_SMSOrganizationalUsers]
(
    [fldv_Position] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSOrganizationalUsers_IsActive] ON [dbo].[tbld_SMSOrganizationalUsers]
(
    [fldb_IsActive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSOrganizationalUsers_OrganizationLevel] ON [dbo].[tbld_SMSOrganizationalUsers]
(
    [fldv_OrganizationLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSOrganizationalUsers_Name] ON [dbo].[tbld_SMSOrganizationalUsers]
(
    [fldv_LastName] ASC,
    [fldv_FirstName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[tbld_SMSStakeholderUsers]    Script Date: 11/5/2025 12:47:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tbld_SMSStakeholderUsers](
    [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
    [fldv_Code] [varchar](60) NOT NULL,
    [fldv_FirstName] [nvarchar](100) NOT NULL,
    [fldv_LastName] [nvarchar](100) NOT NULL,
    [fldv_UserName] [nvarchar](50) NOT NULL,
    [fldv_Password] [nvarchar](255) NOT NULL,
    [fldv_StakeholderType] [nvarchar](100) NOT NULL,
    [fldv_Organization] [nvarchar](200) NOT NULL,
    [fldv_AccessLevel] [nvarchar](50) NOT NULL DEFAULT ('Limited'),
    [fldb_IsActive] [bit] NOT NULL DEFAULT (1),
    [fldd_LastLoginDate] [datetime2](7) NULL,
    [fldv_CreatedBy] [nvarchar](50) NOT NULL,
    [fldd_CreatedDate] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
    [fldv_UpdatedBy] [nvarchar](50) NULL,
    [fldd_UpdatedDate] [datetime2](7) NULL,
    CONSTRAINT [PK_tbld_SMSStakeholderUsers] PRIMARY KEY CLUSTERED 
    (
        [fldi_ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
    CONSTRAINT [UK_tbld_SMSStakeholderUsers_Code] UNIQUE NONCLUSTERED 
    (
        [fldv_Code] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
    CONSTRAINT [UK_tbld_SMSStakeholderUsers_UserName] UNIQUE NONCLUSTERED 
    (
        [fldv_UserName] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUsers_StakeholderType] ON [dbo].[tbld_SMSStakeholderUsers]
(
    [fldv_StakeholderType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUsers_Organization] ON [dbo].[tbld_SMSStakeholderUsers]
(
    [fldv_Organization] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUsers_IsActive] ON [dbo].[tbld_SMSStakeholderUsers]
(
    [fldb_IsActive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUsers_AccessLevel] ON [dbo].[tbld_SMSStakeholderUsers]
(
    [fldv_AccessLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUsers_Name] ON [dbo].[tbld_SMSStakeholderUsers]
(
    [fldv_LastName] ASC,
    [fldv_FirstName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO