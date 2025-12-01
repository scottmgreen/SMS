CREATE PROCEDURE [dbo].[pr_SMSStakeholderUser_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Dataset 1: All SMSStakeholderUsers
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderTypeCode],
        [fldv_Organization],
        [fldv_SMSUserRoleCode],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers]
    ORDER BY [fldv_LastName], [fldv_FirstName];

    -- Dataset 2: DISTINCT SMSUserRoles used by stakeholder users
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name],
        ur.[fldv_Description],
        ur.[fldb_IsActive],
        ur.[fldv_CreatedBy],
        ur.[fldd_CreatedDate],
        ur.[fldv_UpdatedBy],
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbls_SMSUserRoles] ur
    WHERE ur.[fldv_Code] IN (
        SELECT DISTINCT [fldv_SMSUserRoleCode]
        FROM [dbo].[tbld_SMSStakeholderUsers]
        WHERE [fldv_SMSUserRoleCode] IS NOT NULL
    )
    ORDER BY ur.[fldv_Name];

    -- Dataset 3: DISTINCT SMSUserRolePermissions for stakeholder user roles
    SELECT DISTINCT
        urp.[fldi_Id],
        urp.[fldv_Code],
        urp.[fldv_SMSUserRoleCode],
        urp.[fldv_Module],
        urp.[fldb_Create],
        urp.[fldb_Read],
        urp.[fldb_Update],
        urp.[fldb_Delete],
        urp.[fldv_CreatedBy],
        urp.[fldd_CreatedDate],
        urp.[fldv_UpdatedBy],
        urp.[fldd_UpdatedDate]
    FROM [dbo].[tbls_SMSUserRolePermissions] urp
    WHERE urp.[fldv_SMSUserRoleCode] IN (
        SELECT DISTINCT [fldv_SMSUserRoleCode]
        FROM [dbo].[tbld_SMSStakeholderUsers]
        WHERE [fldv_SMSUserRoleCode] IS NOT NULL
    )
    ORDER BY urp.[fldv_SMSUserRoleCode], urp.[fldv_Module];

END