# ?? **SMS DATABASE - MISSING STORED PROCEDURES ONLY**

## ?? **THESE ARE THE ONLY NEW PROCEDURES NEEDED**

Based on your database analysis, these are the **ONLY** missing procedures that need to be **ADDED** with proper error handling:

### **?? MISSING PROCEDURES (Need to be Created)**

#### **1. Investigation Extensions**
```sql
CREATE PROCEDURE [dbo].[pr_Investigation_GetByReportCode]
    @pReportCode VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_Investigation_GetByReportCode', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Investigations for Report Code: ' + CAST(@pReportCode AS VARCHAR(60));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT [fldi_ID], [fldv_Code], [fldv_ReportCode], [fldv_InvestigationNotes]
        FROM [dbo].[tbld_Investigations] 
        WHERE [fldv_ReportCode] = @pReportCode
        ORDER BY [fldv_Code];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' Investigations for Report: ' + @pReportCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Investigations for Report ' + @pReportCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
```

#### **2. Interview Extensions**
```sql
CREATE PROCEDURE [dbo].[pr_Interview_GetByInvestigationCode]
    @pInvestigationCode VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_Interview_GetByInvestigationCode', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Interviews for Investigation Code: ' + CAST(@pInvestigationCode AS VARCHAR(60));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT [fldi_ID], [fldv_Code], [fldv_InvestigationCode], [fldv_SMSInvestigatorCode], [fldv_PersonInterviewed], [fldv_PersonInterviewedNotes], [fldv_InvestigatorNotes]
        FROM [dbo].[tbld_Interviews] 
        WHERE [fldv_InvestigationCode] = @pInvestigationCode
        ORDER BY [fldv_Code];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' Interviews for Investigation: ' + @pInvestigationCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Interviews for Investigation ' + @pInvestigationCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
```

#### **3. Missing SMS User Role Procedures (If Not in Database)**

```sql
-- Only add if this specific procedure is missing:
CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetActiveByUserId]
    @pUserID VARCHAR(60),
    @pUserID_Audit VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSUserRole_GetActiveByUserId', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Active User Roles for User ID: ' + @pUserID;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID_Audit, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_UserRoles', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            ur.[fldi_ID],
            ur.[fldv_Code],
            ur.[fldv_UserID],
            ur.[fldv_UserType],
            ur.[fldv_SMSRoleCode],
            r.[fldv_RoleName],
            r.[fldv_AuthorityLevel],
            r.[fldv_RoleCategory],
            ur.[fldv_Department],
            ur.[fldd_EffectiveDate],
            ur.[fldd_ExpirationDate],
            ur.[fldb_IsActive],
            ur.[fldv_AssignedBy],
            ur.[fldd_AssignedDate]
        FROM [dbo].[tbld_SMSUserRoles] ur
        INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
        WHERE ur.[fldv_UserID] = @pUserID
          AND ur.[fldb_IsActive] = 1
          AND ur.[fldd_EffectiveDate] <= GETUTCDATE()
          AND (ur.[fldd_ExpirationDate] IS NULL OR ur.[fldd_ExpirationDate] > GETUTCDATE())
        ORDER BY r.[fldv_AuthorityLevel];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' Active User Roles for User: ' + @pUserID;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID_Audit, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_UserRoles', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Active User Roles for User ' + @pUserID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID_Audit, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_UserRoles', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
```

---

## ?? **DUPLICATION ANALYSIS**

### **? ROLE PROCEDURES - ALL EXIST IN DATABASE**

Based on your database dump, **ALL** SMS Role procedures already exist:
- `pr_SMSRole_Delete` ? Exists
- `pr_SMSRole_GetActiveRoles` ? Exists  
- `pr_SMSRole_GetAll` ? Exists
- `pr_SMSRole_GetByAuthorityLevel` ? Exists
- `pr_SMSRole_GetByCategory` ? Exists
- `pr_SMSRole_GetById` ? Exists
- `pr_SMSRole_GetByName` ? Exists
- `pr_SMSRole_Insert` ? Exists
- `pr_SMSRole_Update` ? Exists

### **? SMS USER ROLE PROCEDURES - ALL EXIST IN DATABASE**

Based on your database dump, **ALL** SMS User Role procedures already exist:
- `pr_SMSUserRole_Deactivate` ? Exists
- `pr_SMSUserRole_Delete` ? Exists
- `pr_SMSUserRole_ExtendExpiration` ? Exists
- `pr_SMSUserRole_GetActiveAssignments` ? Exists
- `pr_SMSUserRole_GetAll` ? Exists
- `pr_SMSUserRole_GetByDepartment` ? Exists
- `pr_SMSUserRole_GetById` ? Exists
- `pr_SMSUserRole_GetByRole` ? Exists
- `pr_SMSUserRole_GetByUserId` ? Exists
- `pr_SMSUserRole_GetByUserType` ? Exists
- `pr_SMSUserRole_GetExpiredAssignments` ? Exists
- `pr_SMSUserRole_Insert` ? Exists
- `pr_SMSUserRole_Reactivate` ? Exists
- `pr_SMSUserRole_Update` ? Exists

---

## ?? **RESULT**

**NO DUPLICATES FOUND** - All Role and User Role procedures exist in your database with proper error handling and audit logging.

**ONLY ADD**: The 2-3 missing extension procedures above (Investigation/Interview by parent ID lookups).

**Your Infrastructure**: Already references all existing procedures correctly via `StoredProcs.cs` ?