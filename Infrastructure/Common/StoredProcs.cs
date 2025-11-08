namespace SMS_Infrastructure.Common
{
    public static class StoredProcs
    {
        /// <summary>
        /// System Operations
        /// </summary>
        private static readonly Lazy<string> _cn_spAddAuditLogEntry = new Lazy<string>(() => "sp_AddAuditLogEntry");
        public static string cn_spAddAuditLogEntry => _cn_spAddAuditLogEntry.Value;

        private static readonly Lazy<string> _sp_AddAuditLogEntry = new Lazy<string>(() => "sp_AddAuditLogEntry");
        public static string sp_AddAuditLogEntry => _sp_AddAuditLogEntry.Value;

        #region SMS Application User Stored Procedures
        
        /// <summary>
        /// Stored procedures for SMS Application User operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSApplicationUser_Insert = new(() => "pr_SMSApplicationUser_Insert");
        public static string pr_SMSApplicationUser_Insert => _pr_SMSApplicationUser_Insert.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_Update = new(() => "pr_SMSApplicationUser_Update");
        public static string pr_SMSApplicationUser_Update => _pr_SMSApplicationUser_Update.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_UpdatePassword = new(() => "pr_SMSApplicationUser_UpdatePassword");
        public static string pr_SMSApplicationUser_UpdatePassword => _pr_SMSApplicationUser_UpdatePassword.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_Delete = new(() => "pr_SMSApplicationUser_Delete");
        public static string pr_SMSApplicationUser_Delete => _pr_SMSApplicationUser_Delete.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_GetAll = new(() => "pr_SMSApplicationUser_GetAll");
        public static string pr_SMSApplicationUser_GetAll => _pr_SMSApplicationUser_GetAll.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_GetById = new(() => "pr_SMSApplicationUser_GetById");
        public static string pr_SMSApplicationUser_GetById => _pr_SMSApplicationUser_GetById.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_GetByUserName = new(() => "pr_SMSApplicationUser_GetByUserName");
        public static string pr_SMSApplicationUser_GetByUserName => _pr_SMSApplicationUser_GetByUserName.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_GetByRole = new(() => "pr_SMSApplicationUser_GetByRole");
        public static string pr_SMSApplicationUser_GetByRole => _pr_SMSApplicationUser_GetByRole.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_GetByPermissionLevel = new(() => "pr_SMSApplicationUser_GetByPermissionLevel");
        public static string pr_SMSApplicationUser_GetByPermissionLevel => _pr_SMSApplicationUser_GetByPermissionLevel.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_GetActiveUsers = new(() => "pr_SMSApplicationUser_GetActiveUsers");
        public static string pr_SMSApplicationUser_GetActiveUsers => _pr_SMSApplicationUser_GetActiveUsers.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_GetUsersRequiringPasswordChange = new(() => "pr_SMSApplicationUser_GetUsersRequiringPasswordChange");
        public static string pr_SMSApplicationUser_GetUsersRequiringPasswordChange => _pr_SMSApplicationUser_GetUsersRequiringPasswordChange.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_RecordLogin = new(() => "pr_SMSApplicationUser_RecordLogin");
        public static string pr_SMSApplicationUser_RecordLogin => _pr_SMSApplicationUser_RecordLogin.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_UpdateLoginInfo = new(() => "pr_SMSApplicationUser_UpdateLoginInfo");
        public static string pr_SMSApplicationUser_UpdateLoginInfo => _pr_SMSApplicationUser_UpdateLoginInfo.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_RecordFailedLogin = new(() => "pr_SMSApplicationUser_RecordFailedLogin");
        public static string pr_SMSApplicationUser_RecordFailedLogin => _pr_SMSApplicationUser_RecordFailedLogin.Value;
        
        #endregion
        
        #region SMS Organizational User Stored Procedures
        
        /// <summary>
        /// Stored procedures for SMS Organizational User operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_Insert = new(() => "pr_SMSOrganizationalUser_Insert");
        public static string pr_SMSOrganizationalUser_Insert => _pr_SMSOrganizationalUser_Insert.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_Update = new(() => "pr_SMSOrganizationalUser_Update");
        public static string pr_SMSOrganizationalUser_Update => _pr_SMSOrganizationalUser_Update.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_UpdatePassword = new(() => "pr_SMSOrganizationalUser_UpdatePassword");
        public static string pr_SMSOrganizationalUser_UpdatePassword => _pr_SMSOrganizationalUser_UpdatePassword.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_Delete = new(() => "pr_SMSOrganizationalUser_Delete");
        public static string pr_SMSOrganizationalUser_Delete => _pr_SMSOrganizationalUser_Delete.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetAll = new(() => "pr_SMSOrganizationalUser_GetAll");
        public static string pr_SMSOrganizationalUser_GetAll => _pr_SMSOrganizationalUser_GetAll.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetById = new(() => "pr_SMSOrganizationalUser_GetById");
        public static string pr_SMSOrganizationalUser_GetById => _pr_SMSOrganizationalUser_GetById.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetByUserName = new(() => "pr_SMSOrganizationalUser_GetByUserName");
        public static string pr_SMSOrganizationalUser_GetByUserName => _pr_SMSOrganizationalUser_GetByUserName.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetByDepartment = new(() => "pr_SMSOrganizationalUser_GetByDepartment");
        public static string pr_SMSOrganizationalUser_GetByDepartment => _pr_SMSOrganizationalUser_GetByDepartment.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetByPosition = new(() => "pr_SMSOrganizationalUser_GetByPosition");
        public static string pr_SMSOrganizationalUser_GetByPosition => _pr_SMSOrganizationalUser_GetByPosition.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetByOrganizationLevel = new(() => "pr_SMSOrganizationalUser_GetByOrganizationLevel");
        public static string pr_SMSOrganizationalUser_GetByOrganizationLevel => _pr_SMSOrganizationalUser_GetByOrganizationLevel.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetActiveUsers = new(() => "pr_SMSOrganizationalUser_GetActiveUsers");
        public static string pr_SMSOrganizationalUser_GetActiveUsers => _pr_SMSOrganizationalUser_GetActiveUsers.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_RecordLogin = new(() => "pr_SMSOrganizationalUser_RecordLogin");
        public static string pr_SMSOrganizationalUser_RecordLogin => _pr_SMSOrganizationalUser_RecordLogin.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUser_UpdateLoginInfo = new(() => "pr_SMSOrganizationalUser_UpdateLoginInfo");
        public static string pr_SMSOrganizationalUser_UpdateLoginInfo => _pr_SMSOrganizationalUser_UpdateLoginInfo.Value;
        
        #endregion
        
        #region SMS Stakeholder User Stored Procedures
        
        /// <summary>
        /// Stored procedures for SMS Stakeholder User operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSStakeholderUser_Insert = new(() => "pr_SMSStakeholderUser_Insert");
        public static string pr_SMSStakeholderUser_Insert => _pr_SMSStakeholderUser_Insert.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_Update = new(() => "pr_SMSStakeholderUser_Update");
        public static string pr_SMSStakeholderUser_Update => _pr_SMSStakeholderUser_Update.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_UpdatePassword = new(() => "pr_SMSStakeholderUser_UpdatePassword");
        public static string pr_SMSStakeholderUser_UpdatePassword => _pr_SMSStakeholderUser_UpdatePassword.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_Delete = new(() => "pr_SMSStakeholderUser_Delete");
        public static string pr_SMSStakeholderUser_Delete => _pr_SMSStakeholderUser_Delete.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetAll = new(() => "pr_SMSStakeholderUser_GetAll");
        public static string pr_SMSStakeholderUser_GetAll => _pr_SMSStakeholderUser_GetAll.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetById = new(() => "pr_SMSStakeholderUser_GetById");
        public static string pr_SMSStakeholderUser_GetById => _pr_SMSStakeholderUser_GetById.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetByUserName = new(() => "pr_SMSStakeholderUser_GetByUserName");
        public static string pr_SMSStakeholderUser_GetByUserName => _pr_SMSStakeholderUser_GetByUserName.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetByStakeholderType = new(() => "pr_SMSStakeholderUser_GetByStakeholderType");
        public static string pr_SMSStakeholderUser_GetByStakeholderType => _pr_SMSStakeholderUser_GetByStakeholderType.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetByOrganization = new(() => "pr_SMSStakeholderUser_GetByOrganization");
        public static string pr_SMSStakeholderUser_GetByOrganization => _pr_SMSStakeholderUser_GetByOrganization.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetByAccessLevel = new(() => "pr_SMSStakeholderUser_GetByAccessLevel");
        public static string pr_SMSStakeholderUser_GetByAccessLevel => _pr_SMSStakeholderUser_GetByAccessLevel.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetActiveUsers = new(() => "pr_SMSStakeholderUser_GetActiveUsers");
        public static string pr_SMSStakeholderUser_GetActiveUsers => _pr_SMSStakeholderUser_GetActiveUsers.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_RecordLogin = new(() => "pr_SMSStakeholderUser_RecordLogin");
        public static string pr_SMSStakeholderUser_RecordLogin => _pr_SMSStakeholderUser_RecordLogin.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUser_UpdateLoginInfo = new(() => "pr_SMSStakeholderUser_UpdateLoginInfo");
        public static string pr_SMSStakeholderUser_UpdateLoginInfo => _pr_SMSStakeholderUser_UpdateLoginInfo.Value;
        
        #endregion

        #region SMS Role Stored Procedures

        /// <summary>
        /// SMS Role Management Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSRole_GetAll = new Lazy<string>(() => "pr_SMSRole_GetAll");
        public static string pr_SMSRole_GetAll => _pr_SMSRole_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSRole_GetById = new Lazy<string>(() => "pr_SMSRole_GetById");
        public static string pr_SMSRole_GetById => _pr_SMSRole_GetById.Value;

        private static readonly Lazy<string> _pr_SMSRole_GetByName = new Lazy<string>(() => "pr_SMSRole_GetByName");
        public static string pr_SMSRole_GetByName => _pr_SMSRole_GetByName.Value;

        private static readonly Lazy<string> _pr_SMSRole_GetByCategory = new Lazy<string>(() => "pr_SMSRole_GetByCategory");
        public static string pr_SMSRole_GetByCategory => _pr_SMSRole_GetByCategory.Value;

        private static readonly Lazy<string> _pr_SMSRole_GetByAuthorityLevel = new Lazy<string>(() => "pr_SMSRole_GetByAuthorityLevel");
        public static string pr_SMSRole_GetByAuthorityLevel => _pr_SMSRole_GetByAuthorityLevel.Value;

        private static readonly Lazy<string> _pr_SMSRole_GetActiveRoles = new Lazy<string>(() => "pr_SMSRole_GetActiveRoles");
        public static string pr_SMSRole_GetActiveRoles => _pr_SMSRole_GetActiveRoles.Value;

        private static readonly Lazy<string> _pr_SMSRole_Insert = new Lazy<string>(() => "pr_SMSRole_Insert");
        public static string pr_SMSRole_Insert => _pr_SMSRole_Insert.Value;

        private static readonly Lazy<string> _pr_SMSRole_Update = new Lazy<string>(() => "pr_SMSRole_Update");
        public static string pr_SMSRole_Update => _pr_SMSRole_Update.Value;

        private static readonly Lazy<string> _pr_SMSRole_Delete = new Lazy<string>(() => "pr_SMSRole_Delete");
        public static string pr_SMSRole_Delete => _pr_SMSRole_Delete.Value;

        #endregion

        #region SMS User Role Stored Procedures

        /// <summary>
        /// SMS User Role Management CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSUserRole_Insert = new Lazy<string>(() => "pr_SMSUserRole_Insert");
        public static string pr_SMSUserRole_Insert => _pr_SMSUserRole_Insert.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetAll = new Lazy<string>(() => "pr_SMSUserRole_GetAll");
        public static string pr_SMSUserRole_GetAll => _pr_SMSUserRole_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetById = new Lazy<string>(() => "pr_SMSUserRole_GetById");
        public static string pr_SMSUserRole_GetById => _pr_SMSUserRole_GetById.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByUserId = new Lazy<string>(() => "pr_SMSUserRole_GetByUserId");
        public static string pr_SMSUserRole_GetByUserId => _pr_SMSUserRole_GetByUserId.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetActiveByUserId = new Lazy<string>(() => "pr_SMSUserRole_GetActiveByUserId");
        public static string pr_SMSUserRole_GetActiveByUserId => _pr_SMSUserRole_GetActiveByUserId.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByRole = new Lazy<string>(() => "pr_SMSUserRole_GetByRole");
        public static string pr_SMSUserRole_GetByRole => _pr_SMSUserRole_GetByRole.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByDepartment = new Lazy<string>(() => "pr_SMSUserRole_GetByDepartment");
        public static string pr_SMSUserRole_GetByDepartment => _pr_SMSUserRole_GetByDepartment.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByUserType = new Lazy<string>(() => "pr_SMSUserRole_GetByUserType");
        public static string pr_SMSUserRole_GetByUserType => _pr_SMSUserRole_GetByUserType.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetActiveAssignments = new Lazy<string>(() => "pr_SMSUserRole_GetActiveAssignments");
        public static string pr_SMSUserRole_GetActiveAssignments => _pr_SMSUserRole_GetActiveAssignments.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetExpiredAssignments = new Lazy<string>(() => "pr_SMSUserRole_GetExpiredAssignments");
        public static string pr_SMSUserRole_GetExpiredAssignments => _pr_SMSUserRole_GetExpiredAssignments.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetExpiringRoles = new Lazy<string>(() => "pr_SMSUserRole_GetExpiringRoles");
        public static string pr_SMSUserRole_GetExpiringRoles => _pr_SMSUserRole_GetExpiringRoles.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_Update = new Lazy<string>(() => "pr_SMSUserRole_Update");
        public static string pr_SMSUserRole_Update => _pr_SMSUserRole_Update.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_Deactivate = new Lazy<string>(() => "pr_SMSUserRole_Deactivate");
        public static string pr_SMSUserRole_Deactivate => _pr_SMSUserRole_Deactivate.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_Delete = new Lazy<string>(() => "pr_SMSUserRole_Delete");
        public static string pr_SMSUserRole_Delete => _pr_SMSUserRole_Delete.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_ExtendExpiration = new Lazy<string>(() => "pr_SMSUserRole_ExtendExpiration");
        public static string pr_SMSUserRole_ExtendExpiration => _pr_SMSUserRole_ExtendExpiration.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_Reactivate = new Lazy<string>(() => "pr_SMSUserRole_Reactivate");
        public static string pr_SMSUserRole_Reactivate => _pr_SMSUserRole_Reactivate.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetStatsReport = new Lazy<string>(() => "pr_SMSUserRole_GetStatsReport");
        public static string pr_SMSUserRole_GetStatsReport => _pr_SMSUserRole_GetStatsReport.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_ValidateUserAuthorization = new Lazy<string>(() => "pr_SMSUserRole_ValidateUserAuthorization");
        public static string pr_SMSUserRole_ValidateUserAuthorization => _pr_SMSUserRole_ValidateUserAuthorization.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetUserMaxAuthorityLevel = new Lazy<string>(() => "pr_SMSUserRole_GetUserMaxAuthorityLevel");
        public static string pr_SMSUserRole_GetUserMaxAuthorityLevel => _pr_SMSUserRole_GetUserMaxAuthorityLevel.Value;

        #endregion

        #region Report Management Procedures

        /// <summary>
        /// Report Management Operations
        /// </summary>
        private static readonly Lazy<string> _pr_AddReport = new Lazy<string>(() => "pr_AddReport");
        public static string pr_AddReport => _pr_AddReport.Value;

        private static readonly Lazy<string> _pr_Report_GetAll = new Lazy<string>(() => "pr_Report_GetAll");
        public static string pr_Report_GetAll => _pr_Report_GetAll.Value;

        private static readonly Lazy<string> _pr_Report_GetById = new Lazy<string>(() => "pr_Report_GetById");
        public static string pr_Report_GetById => _pr_Report_GetById.Value;

        private static readonly Lazy<string> _pr_Report_Insert = new Lazy<string>(() => "pr_Report_Insert");
        public static string pr_Report_Insert => _pr_Report_Insert.Value;

        private static readonly Lazy<string> _pr_Report_Update = new Lazy<string>(() => "pr_Report_Update");
        public static string pr_Report_Update => _pr_Report_Update.Value;

        private static readonly Lazy<string> _pr_Report_Delete = new Lazy<string>(() => "pr_Report_Delete");
        public static string pr_Report_Delete => _pr_Report_Delete.Value;

        #endregion

        #region SMS Hazard CRUD Operations
        /// <summary>
        /// SMS Hazard CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Hazard_Insert = new Lazy<string>(() => "pr_Hazard_Insert");
        public static string pr_Hazard_Insert => _pr_Hazard_Insert.Value;

        private static readonly Lazy<string> _pr_Hazard_GetById = new Lazy<string>(() => "pr_Hazard_GetById");
        public static string pr_Hazard_GetById => _pr_Hazard_GetById.Value;

        private static readonly Lazy<string> _pr_Hazard_GetAll = new Lazy<string>(() => "pr_Hazard_GetAll");
        public static string pr_Hazard_GetAll => _pr_Hazard_GetAll.Value;

        private static readonly Lazy<string> _pr_Hazard_Update = new Lazy<string>(() => "pr_Hazard_Update");
        public static string pr_Hazard_Update => _pr_Hazard_Update.Value;

        private static readonly Lazy<string> _pr_Hazard_Delete = new Lazy<string>(() => "pr_Hazard_Delete");
        public static string pr_Hazard_Delete => _pr_Hazard_Delete.Value;
        #endregion

        #region SMS Hazard Location CRUD Operations
        /// <summary>
        /// SMS Hazard Location CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_HazardLocation_Insert = new Lazy<string>(() => "pr_HazardLocation_Insert");
        public static string pr_HazardLocation_Insert => _pr_HazardLocation_Insert.Value;

        private static readonly Lazy<string> _pr_HazardLocation_GetById = new Lazy<string>(() => "pr_HazardLocation_GetById");
        public static string pr_HazardLocation_GetById => _pr_HazardLocation_GetById.Value;

        private static readonly Lazy<string> _pr_HazardLocation_GetAll = new Lazy<string>(() => "pr_HazardLocation_GetAll");
        public static string pr_HazardLocation_GetAll => _pr_HazardLocation_GetAll.Value;

        private static readonly Lazy<string> _pr_HazardLocation_GetByHazardCode = new Lazy<string>(() => "pr_HazardLocation_GetByHazardCode");
        public static string pr_HazardLocation_GetByHazardCode => _pr_HazardLocation_GetByHazardCode.Value;

        private static readonly Lazy<string> _pr_HazardLocation_Update = new Lazy<string>(() => "pr_HazardLocation_Update");
        public static string pr_HazardLocation_Update => _pr_HazardLocation_Update.Value;

        private static readonly Lazy<string> _pr_HazardLocation_Delete = new Lazy<string>(() => "pr_HazardLocation_Delete");
        public static string pr_HazardLocation_Delete => _pr_HazardLocation_Delete.Value;

        private static readonly Lazy<string> _pr_HazardLocation_GetNearby = new Lazy<string>(() => "pr_HazardLocation_GetNearby");
        public static string pr_HazardLocation_GetNearby => _pr_HazardLocation_GetNearby.Value;
        #endregion

        #region SMS Investigation CRUD Operations
        /// <summary>
        /// SMS Investigation CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Investigation_Insert = new Lazy<string>(() => "pr_Investigation_Insert");
        public static string pr_Investigation_Insert => _pr_Investigation_Insert.Value;

        private static readonly Lazy<string> _pr_Investigation_GetById = new Lazy<string>(() => "pr_Investigation_GetById");
        public static string pr_Investigation_GetById => _pr_Investigation_GetById.Value;

        private static readonly Lazy<string> _pr_Investigation_GetByCode = new Lazy<string>(() => "pr_Investigation_GetByCode");
        public static string pr_Investigation_GetByCode => _pr_Investigation_GetByCode.Value;

        private static readonly Lazy<string> _pr_Investigation_GetAll = new Lazy<string>(() => "pr_Investigation_GetAll");
        public static string pr_Investigation_GetAll => _pr_Investigation_GetAll.Value;

        private static readonly Lazy<string> _pr_Investigation_GetByHazardCode = new Lazy<string>(() => "pr_Investigation_GetByHazardCode");
        public static string pr_Investigation_GetByHazardCode => _pr_Investigation_GetByHazardCode.Value;

        private static readonly Lazy<string> _pr_Investigation_GetByInvestigator = new Lazy<string>(() => "pr_Investigation_GetByInvestigator");
        public static string pr_Investigation_GetByInvestigator => _pr_Investigation_GetByInvestigator.Value;

        private static readonly Lazy<string> _pr_Investigation_GetByStatus = new Lazy<string>(() => "pr_Investigation_GetByStatus");
        public static string pr_Investigation_GetByStatus => _pr_Investigation_GetByStatus.Value;

        private static readonly Lazy<string> _pr_Investigation_Update = new Lazy<string>(() => "pr_Investigation_Update");
        public static string pr_Investigation_Update => _pr_Investigation_Update.Value;

        private static readonly Lazy<string> _pr_Investigation_Delete = new Lazy<string>(() => "pr_Investigation_Delete");
        public static string pr_Investigation_Delete => _pr_Investigation_Delete.Value;

        private static readonly Lazy<string> _pr_Investigation_UpdateStatus = new Lazy<string>(() => "pr_Investigation_UpdateStatus");
        public static string pr_Investigation_UpdateStatus => _pr_Investigation_UpdateStatus.Value;

        private static readonly Lazy<string> _pr_Investigation_RecordDecision = new Lazy<string>(() => "pr_Investigation_RecordDecision");
        public static string pr_Investigation_RecordDecision => _pr_Investigation_RecordDecision.Value;

        private static readonly Lazy<string> _pr_Investigation_Complete = new Lazy<string>(() => "pr_Investigation_Complete");
        public static string pr_Investigation_Complete => _pr_Investigation_Complete.Value;

        #endregion

        #region SMS Interview CRUD Operations
        /// <summary>
        /// SMS Interview CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Interview_Insert = new Lazy<string>(() => "pr_Interview_Insert");
        public static string pr_Interview_Insert => _pr_Interview_Insert.Value;

        private static readonly Lazy<string> _pr_Interview_GetById = new Lazy<string>(() => "pr_Interview_GetById");
        public static string pr_Interview_GetById => _pr_Interview_GetById.Value;

        private static readonly Lazy<string> _pr_Interview_GetByCode = new Lazy<string>(() => "pr_Interview_GetByCode");
        public static string pr_Interview_GetByCode => _pr_Interview_GetByCode.Value;

        private static readonly Lazy<string> _pr_Interview_GetAll = new Lazy<string>(() => "pr_Interview_GetAll");
        public static string pr_Interview_GetAll => _pr_Interview_GetAll.Value;

        private static readonly Lazy<string> _pr_Interview_GetByInvestigation = new Lazy<string>(() => "pr_Interview_GetByInvestigation");
        public static string pr_Interview_GetByInvestigation => _pr_Interview_GetByInvestigation.Value;

        private static readonly Lazy<string> _pr_Interview_GetByInvestigator = new Lazy<string>(() => "pr_Interview_GetByInvestigator");
        public static string pr_Interview_GetByInvestigator => _pr_Interview_GetByInvestigator.Value;

        private static readonly Lazy<string> _pr_Interview_GetByStatus = new Lazy<string>(() => "pr_Interview_GetByStatus");
        public static string pr_Interview_GetByStatus => _pr_Interview_GetByStatus.Value;

        private static readonly Lazy<string> _pr_Interview_Update = new Lazy<string>(() => "pr_Interview_Update");
        public static string pr_Interview_Update => _pr_Interview_Update.Value;

        private static readonly Lazy<string> _pr_Interview_Delete = new Lazy<string>(() => "pr_Interview_Delete");
        public static string pr_Interview_Delete => _pr_Interview_Delete.Value;

        private static readonly Lazy<string> _pr_Interview_UpdateStatus = new Lazy<string>(() => "pr_Interview_UpdateStatus");
        public static string pr_Interview_UpdateStatus => _pr_Interview_UpdateStatus.Value;

        private static readonly Lazy<string> _pr_Interview_Schedule = new Lazy<string>(() => "pr_Interview_Schedule");
        public static string pr_Interview_Schedule => _pr_Interview_Schedule.Value;

        private static readonly Lazy<string> _pr_Interview_Complete = new Lazy<string>(() => "pr_Interview_Complete");
        public static string pr_Interview_Complete => _pr_Interview_Complete.Value;
        #endregion

        #region SMS Risk Analysis CRUD Operations
        /// <summary>
        /// SMS Risk Analysis CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_RiskAnalysis_Insert = new Lazy<string>(() => "pr_RiskAnalysis_Insert");
        public static string pr_RiskAnalysis_Insert => _pr_RiskAnalysis_Insert.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_GetById = new Lazy<string>(() => "pr_RiskAnalysis_GetById");
        public static string pr_RiskAnalysis_GetById => _pr_RiskAnalysis_GetById.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_GetAll = new Lazy<string>(() => "pr_RiskAnalysis_GetAll");
        public static string pr_RiskAnalysis_GetAll => _pr_RiskAnalysis_GetAll.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_Update = new Lazy<string>(() => "pr_RiskAnalysis_Update");
        public static string pr_RiskAnalysis_Update => _pr_RiskAnalysis_Update.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_Delete = new Lazy<string>(() => "pr_RiskAnalysis_Delete");
        public static string pr_RiskAnalysis_Delete => _pr_RiskAnalysis_Delete.Value;
        #endregion

        #region SMS Risk Assessment CRUD Operations
        /// <summary>
        /// SMS Risk Assessment CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_RiskAssessment_Insert = new Lazy<string>(() => "pr_RiskAssessment_Insert");
        public static string pr_RiskAssessment_Insert => _pr_RiskAssessment_Insert.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_GetById = new Lazy<string>(() => "pr_RiskAssessment_GetById");
        public static string pr_RiskAssessment_GetById => _pr_RiskAssessment_GetById.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_GetAll = new Lazy<string>(() => "pr_RiskAssessment_GetAll");
        public static string pr_RiskAssessment_GetAll => _pr_RiskAssessment_GetAll.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_Update = new Lazy<string>(() => "pr_RiskAssessment_Update");
        public static string pr_RiskAssessment_Update => _pr_RiskAssessment_Update.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_Delete = new Lazy<string>(() => "pr_RiskAssessment_Delete");
        public static string pr_RiskAssessment_Delete => _pr_RiskAssessment_Delete.Value;
        #endregion

        #region SMS Mitigation CRUD Operations
        /// <summary>
        /// SMS Mitigation CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Mitigation_Insert = new Lazy<string>(() => "pr_Mitigation_Insert");
        public static string pr_Mitigation_Insert => _pr_Mitigation_Insert.Value;

        private static readonly Lazy<string> _pr_Mitigation_GetById = new Lazy<string>(() => "pr_Mitigation_GetById");
        public static string pr_Mitigation_GetById => _pr_Mitigation_GetById.Value;

        private static readonly Lazy<string> _pr_Mitigation_GetAll = new Lazy<string>(() => "pr_Mitigation_GetAll");
        public static string pr_Mitigation_GetAll => _pr_Mitigation_GetAll.Value;

        private static readonly Lazy<string> _pr_Mitigation_Update = new Lazy<string>(() => "pr_Mitigation_Update");
        public static string pr_Mitigation_Update => _pr_Mitigation_Update.Value;

        private static readonly Lazy<string> _pr_Mitigation_Delete = new Lazy<string>(() => "pr_Mitigation_Delete");
        public static string pr_Mitigation_Delete => _pr_Mitigation_Delete.Value;
        #endregion

        #region SMS Mitigation Assignment CRUD Operations
        /// <summary>
        /// SMS Mitigation Assignment CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_MitigationAssignment_Insert = new Lazy<string>(() => "pr_MitigationAssignment_Insert");
        public static string pr_MitigationAssignment_Insert => _pr_MitigationAssignment_Insert.Value;

        private static readonly Lazy<string> _pr_MitigationAssignment_GetById = new Lazy<string>(() => "pr_MitigationAssignment_GetById");
        public static string pr_MitigationAssignment_GetById => _pr_MitigationAssignment_GetById.Value;

        private static readonly Lazy<string> _pr_MitigationAssignment_GetAll = new Lazy<string>(() => "pr_MitigationAssignment_GetAll");
        public static string pr_MitigationAssignment_GetAll => _pr_MitigationAssignment_GetAll.Value;

        private static readonly Lazy<string> _pr_MitigationAssignment_Update = new Lazy<string>(() => "pr_MitigationAssignment_Update");
        public static string pr_MitigationAssignment_Update => _pr_MitigationAssignment_Update.Value;

        private static readonly Lazy<string> _pr_MitigationAssignment_Delete = new Lazy<string>(() => "pr_MitigationAssignment_Delete");
        public static string pr_MitigationAssignment_Delete => _pr_MitigationAssignment_Delete.Value;
        #endregion

        #region SMS Report Validation CRUD Operations
        /// <summary>
        /// SMS Report Validation CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_ReportValidation_Insert = new Lazy<string>(() => "pr_ReportValidation_Insert");
        public static string pr_ReportValidation_Insert => _pr_ReportValidation_Insert.Value;

        private static readonly Lazy<string> _pr_ReportValidation_GetById = new Lazy<string>(() => "pr_ReportValidation_GetById");
        public static string pr_ReportValidation_GetById => _pr_ReportValidation_GetById.Value;

        private static readonly Lazy<string> _pr_ReportValidation_GetAll = new Lazy<string>(() => "pr_ReportValidation_GetAll");
        public static string pr_ReportValidation_GetAll => _pr_ReportValidation_GetAll.Value;

        private static readonly Lazy<string> _pr_ReportValidation_Update = new Lazy<string>(() => "pr_ReportValidation_Update");
        public static string pr_ReportValidation_Update => _pr_ReportValidation_Update.Value;

        private static readonly Lazy<string> _pr_ReportValidation_Delete = new Lazy<string>(() => "pr_ReportValidation_Delete");
        public static string pr_ReportValidation_Delete => _pr_ReportValidation_Delete.Value;
        #endregion

        #region SMS Scoring Panel CRUD Operations
        /// <summary>
        /// SMS Scoring Panel CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_ScoringPanel_Insert = new Lazy<string>(() => "pr_ScoringPanel_Insert");
        public static string pr_ScoringPanel_Insert => _pr_ScoringPanel_Insert.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_GetById = new Lazy<string>(() => "pr_ScoringPanel_GetById");
        public static string pr_ScoringPanel_GetById => _pr_ScoringPanel_GetById.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_GetAll = new Lazy<string>(() => "pr_ScoringPanel_GetAll");
        public static string pr_ScoringPanel_GetAll => _pr_ScoringPanel_GetAll.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_Update = new Lazy<string>(() => "pr_ScoringPanel_Update");
        public static string pr_ScoringPanel_Update => _pr_ScoringPanel_Update.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_Delete = new Lazy<string>(() => "pr_ScoringPanel_Delete");
        public static string pr_ScoringPanel_Delete => _pr_ScoringPanel_Delete.Value;
        #endregion

        #region SMS Airport Shared Dataset CRUD Operations
        /// <summary>
        /// SMS Airport Shared Dataset CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_AirportSharedDataset_Insert = new Lazy<string>(() => "pr_AirportSharedDataset_Insert");
        public static string pr_AirportSharedDataset_Insert => _pr_AirportSharedDataset_Insert.Value;

        private static readonly Lazy<string> _pr_AirportSharedDataset_GetById = new Lazy<string>(() => "pr_AirportSharedDataset_GetById");
        public static string pr_AirportSharedDataset_GetById => _pr_AirportSharedDataset_GetById.Value;

        private static readonly Lazy<string> _pr_AirportSharedDataset_GetAll = new Lazy<string>(() => "pr_AirportSharedDataset_GetAll");
        public static string pr_AirportSharedDataset_GetAll => _pr_AirportSharedDataset_GetAll.Value;

        private static readonly Lazy<string> _pr_AirportSharedDataset_Update = new Lazy<string>(() => "pr_AirportSharedDataset_Update");
        public static string pr_AirportSharedDataset_Update => _pr_AirportSharedDataset_Update.Value;

        private static readonly Lazy<string> _pr_AirportSharedDataset_Delete = new Lazy<string>(() => "pr_AirportSharedDataset_Delete");
        public static string pr_AirportSharedDataset_Delete => _pr_AirportSharedDataset_Delete.Value;
        #endregion

        #region Utility Procedures

        /// <summary>
        /// Utility and System Operations
        /// </summary>
        private static readonly Lazy<string> _pr_GenerateFormattedCode = new Lazy<string>(() => "pr_GenerateFormattedCode");
        public static string pr_GenerateFormattedCode => _pr_GenerateFormattedCode.Value;

        private static readonly Lazy<string> _pr_SMS_CheckUsernameAvailability = new Lazy<string>(() => "pr_SMS_CheckUsernameAvailability");
        public static string pr_SMS_CheckUsernameAvailability => _pr_SMS_CheckUsernameAvailability.Value;

        private static readonly Lazy<string> _pr_SMS_GetUserStatisticsSummary = new Lazy<string>(() => "pr_SMS_GetUserStatisticsSummary");
        public static string pr_SMS_GetUserStatisticsSummary => _pr_SMS_GetUserStatisticsSummary.Value;

        private static readonly Lazy<string> _pr_TruncateAllTables = new Lazy<string>(() => "pr_TruncateAllTables");
        public static string pr_TruncateAllTables => _pr_TruncateAllTables.Value;

        #endregion

        #region Hazard File Stored Procedures

        /// <summary>
        /// Hazard File Management CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_HazardFile_Insert = new Lazy<string>(() => "pr_HazardFile_Insert");
        public static string pr_HazardFile_Insert => _pr_HazardFile_Insert.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetById = new Lazy<string>(() => "pr_HazardFile_GetById");
        public static string pr_HazardFile_GetById => _pr_HazardFile_GetById.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetByCode = new Lazy<string>(() => "pr_HazardFile_GetByCode");
        public static string pr_HazardFile_GetByCode => _pr_HazardFile_GetByCode.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetByHazardCode = new Lazy<string>(() => "pr_HazardFile_GetByHazardCode");
        public static string pr_HazardFile_GetByHazardCode => _pr_HazardFile_GetByHazardCode.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetByReportCode = new Lazy<string>(() => "pr_HazardFile_GetByReportCode");
        public static string pr_HazardFile_GetByReportCode => _pr_HazardFile_GetByReportCode.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetFileData = new Lazy<string>(() => "pr_HazardFile_GetFileData");
        public static string pr_HazardFile_GetFileData => _pr_HazardFile_GetFileData.Value;

        private static readonly Lazy<string> _pr_HazardFile_Update = new Lazy<string>(() => "pr_HazardFile_Update");
        public static string pr_HazardFile_Update => _pr_HazardFile_Update.Value;

        private static readonly Lazy<string> _pr_HazardFile_Deactivate = new Lazy<string>(() => "pr_HazardFile_Deactivate");
        public static string pr_HazardFile_Deactivate => _pr_HazardFile_Deactivate.Value;

        private static readonly Lazy<string> _pr_HazardFile_Reactivate = new Lazy<string>(() => "pr_HazardFile_Reactivate");
        public static string pr_HazardFile_Reactivate => _pr_HazardFile_Reactivate.Value;

        private static readonly Lazy<string> _pr_HazardFile_Search = new Lazy<string>(() => "pr_HazardFile_Search");
        public static string pr_HazardFile_Search => _pr_HazardFile_Search.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetStatistics = new Lazy<string>(() => "pr_HazardFile_GetStatistics");
        public static string pr_HazardFile_GetStatistics => _pr_HazardFile_GetStatistics.Value;

        #endregion
    }
}