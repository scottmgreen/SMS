-- =============================================
-- SMS ROLE MANAGEMENT TABLES
-- Database: PDXSMS_V2
-- Description: Complete SMS Role and User Role Assignment tables
-- Author: System Generated
-- Date: Generated for SMS User Management System
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- Table: tbld_SMSRoles
-- Description: SMS Role definitions and metadata
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbld_SMSRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tbld_SMSRoles](
        [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
        [fldv_Code] [varchar](60) NOT NULL,
        [fldv_RoleName] [nvarchar](100) NOT NULL,
        [fldv_AuthorityLevel] [nvarchar](50) NOT NULL,
        [fldv_RoleCategory] [nvarchar](50) NOT NULL,
        [fldv_Description] [nvarchar](500) NULL,
        [fldb_IsActive] [bit] NOT NULL,
        [fldv_CreatedBy] [nvarchar](50) NOT NULL,
        [fldd_CreatedDate] [datetime2](7) NOT NULL,
        [fldv_UpdatedBy] [nvarchar](50) NULL,
        [fldd_UpdatedDate] [datetime2](7) NULL,
        CONSTRAINT [PK_tbld_SMSRoles] PRIMARY KEY CLUSTERED ([fldi_ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UK_tbld_SMSRoles_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UK_tbld_SMSRoles_RoleName] UNIQUE NONCLUSTERED ([fldv_RoleName] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]

    -- Add default constraints
    ALTER TABLE [dbo].[tbld_SMSRoles] ADD DEFAULT ('Standard') FOR [fldv_AuthorityLevel]
    ALTER TABLE [dbo].[tbld_SMSRoles] ADD DEFAULT ((1)) FOR [fldb_IsActive]
    ALTER TABLE [dbo].[tbld_SMSRoles] ADD DEFAULT (getutcdate()) FOR [fldd_CreatedDate]

    PRINT 'Table tbld_SMSRoles created successfully'
END
ELSE
BEGIN
    PRINT 'Table tbld_SMSRoles already exists'
END
GO

-- =============================================
-- Table: tbld_SMSUserRoles  
-- Description: SMS User Role assignments with temporal validity
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbld_SMSUserRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tbld_SMSUserRoles](
        [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
        [fldv_Code] [varchar](60) NOT NULL,
        [fldv_UserID] [varchar](60) NOT NULL,
        [fldv_UserType] [nvarchar](50) NOT NULL,
        [fldv_SMSRoleCode] [varchar](60) NOT NULL,
        [fldv_Department] [nvarchar](100) NOT NULL,
        [fldd_EffectiveDate] [datetime2](7) NOT NULL,
        [fldd_ExpirationDate] [datetime2](7) NULL,
        [fldb_IsActive] [bit] NOT NULL,
        [fldv_AssignedBy] [nvarchar](50) NOT NULL,
        [fldd_AssignedDate] [datetime2](7) NOT NULL,
        [fldv_DeactivatedBy] [nvarchar](50) NULL,
        [fldd_DeactivatedDate] [datetime2](7) NULL,
        [fldv_CreatedBy] [nvarchar](50) NOT NULL,
        [fldd_CreatedDate] [datetime2](7) NOT NULL,
        [fldv_UpdatedBy] [nvarchar](50) NULL,
        [fldd_UpdatedDate] [datetime2](7) NULL,
        CONSTRAINT [PK_tbld_SMSUserRoles] PRIMARY KEY CLUSTERED ([fldi_ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UK_tbld_SMSUserRoles_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]

    -- Add default constraints
    ALTER TABLE [dbo].[tbld_SMSUserRoles] ADD DEFAULT ((1)) FOR [fldb_IsActive]
    ALTER TABLE [dbo].[tbld_SMSUserRoles] ADD DEFAULT (getutcdate()) FOR [fldd_EffectiveDate]
    ALTER TABLE [dbo].[tbld_SMSUserRoles] ADD DEFAULT (getutcdate()) FOR [fldd_AssignedDate]
    ALTER TABLE [dbo].[tbld_SMSUserRoles] ADD DEFAULT (getutcdate()) FOR [fldd_CreatedDate]

    -- Add foreign key constraint
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbld_SMSRoles]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[tbld_SMSUserRoles] ADD CONSTRAINT [FK_tbld_SMSUserRoles_SMSRole] 
            FOREIGN KEY ([fldv_SMSRoleCode]) REFERENCES [dbo].[tbld_SMSRoles]([fldv_Code])
    END

    PRINT 'Table tbld_SMSUserRoles created successfully'
END
ELSE
BEGIN
    PRINT 'Table tbld_SMSUserRoles already exists'
END
GO

-- =============================================
-- Performance Indexes for tbld_SMSUserRoles
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSUserRoles_UserID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSUserRoles_UserID] 
    ON [dbo].[tbld_SMSUserRoles] ([fldv_UserID])
    PRINT 'Index IX_tbld_SMSUserRoles_UserID created'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSUserRoles_UserType')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSUserRoles_UserType] 
    ON [dbo].[tbld_SMSUserRoles] ([fldv_UserType])
    PRINT 'Index IX_tbld_SMSUserRoles_UserType created'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSUserRoles_Department')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSUserRoles_Department] 
    ON [dbo].[tbld_SMSUserRoles] ([fldv_Department])
    PRINT 'Index IX_tbld_SMSUserRoles_Department created'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSUserRoles_EffectiveDate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSUserRoles_EffectiveDate] 
    ON [dbo].[tbld_SMSUserRoles] ([fldd_EffectiveDate])
    PRINT 'Index IX_tbld_SMSUserRoles_EffectiveDate created'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSUserRoles_IsActive')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSUserRoles_IsActive] 
    ON [dbo].[tbld_SMSUserRoles] ([fldb_IsActive])
    PRINT 'Index IX_tbld_SMSUserRoles_IsActive created'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSUserRoles_SMSRoleCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSUserRoles_SMSRoleCode] 
    ON [dbo].[tbld_SMSUserRoles] ([fldv_SMSRoleCode])
    PRINT 'Index IX_tbld_SMSUserRoles_SMSRoleCode created'
END
GO

-- =============================================
-- Check constraints for data integrity
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbld_SMSUserRoles_UserType')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSUserRoles] 
    ADD CONSTRAINT [CK_tbld_SMSUserRoles_UserType] 
    CHECK ([fldv_UserType] IN ('SMSApplicationUser', 'SMSOrganizationalUser', 'SMSStakeholderUser'))
    PRINT 'Check constraint CK_tbld_SMSUserRoles_UserType created'
END

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbld_SMSRoles_AuthorityLevel')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSRoles] 
    ADD CONSTRAINT [CK_tbld_SMSRoles_AuthorityLevel] 
    CHECK ([fldv_AuthorityLevel] IN ('Standard', 'Elevated', 'Supervisor', 'Manager', 'Director', 'Executive', 'Administrator'))
    PRINT 'Check constraint CK_tbld_SMSRoles_AuthorityLevel created'
END

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbld_SMSRoles_RoleCategory')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSRoles] 
    ADD CONSTRAINT [CK_tbld_SMSRoles_RoleCategory] 
    CHECK ([fldv_RoleCategory] IN ('Safety', 'Security', 'Operations', 'Quality', 'Management', 'Technical', 'Administrative'))
    PRINT 'Check constraint CK_tbld_SMSRoles_RoleCategory created'
END
GO

PRINT '============================================='
PRINT 'SMS Role Management Tables Creation Complete'
PRINT '============================================='
PRINT 'Created:'
PRINT '- tbld_SMSRoles (Role definitions)'
PRINT '- tbld_SMSUserRoles (User role assignments)'
PRINT '- 6 Performance indexes'
PRINT '- 3 Data integrity constraints'
PRINT '- Foreign key relationships'
PRINT '============================================='