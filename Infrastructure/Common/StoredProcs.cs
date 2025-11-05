namespace SMS_Infrastructure.Common
{
    public static class StoredProcs
    {
        /// <summary>
        /// System Operations
        /// </summary>
        private static readonly Lazy<string> _cn_spAddAuditLogEntry = new Lazy<string>(() => "sp_AddAuditLogEntry");
        public static string cn_spAddAuditLogEntry => _cn_spAddAuditLogEntry.Value;

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
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_GetActiveUsers = new(() => "pr_SMSApplicationUser_GetActiveUsers");
        public static string pr_SMSApplicationUser_GetActiveUsers => _pr_SMSApplicationUser_GetActiveUsers.Value;
        
        private static readonly Lazy<string> _pr_SMSApplicationUser_RecordLogin = new(() => "pr_SMSApplicationUser_RecordLogin");
        public static string pr_SMSApplicationUser_RecordLogin => _pr_SMSApplicationUser_RecordLogin.Value;
        
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
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetActiveUsers = new(() => "pr_SMSOrganizationalUser_GetActiveUsers");
        public static string pr_SMSOrganizationalUser_GetActiveUsers => _pr_SMSOrganizationalUser_GetActiveUsers.Value;
        
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_RecordLogin = new(() => "pr_SMSOrganizationalUser_RecordLogin");
        public static string pr_SMSOrganizationalUser_RecordLogin => _pr_SMSOrganizationalUser_RecordLogin.Value;
        
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
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetActiveUsers = new(() => "pr_SMSStakeholderUser_GetActiveUsers");
        public static string pr_SMSStakeholderUser_GetActiveUsers => _pr_SMSStakeholderUser_GetActiveUsers.Value;
        
        private static readonly Lazy<string> _pr_SMSStakeholderUser_RecordLogin = new(() => "pr_SMSStakeholderUser_RecordLogin");
        public static string pr_SMSStakeholderUser_RecordLogin => _pr_SMSStakeholderUser_RecordLogin.Value;
        
        #endregion

        /// <summary>
        /// SMS User Role Management CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSUserRole_Insert = new Lazy<string>(() => "pr_SMSUserRole_Insert");
        public static string pr_SMSUserRole_Insert => _pr_SMSUserRole_Insert.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetById = new Lazy<string>(() => "pr_SMSUserRole_GetById");
        public static string pr_SMSUserRole_GetById => _pr_SMSUserRole_GetById.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByUserId = new Lazy<string>(() => "pr_SMSUserRole_GetByUserId");
        public static string pr_SMSUserRole_GetByUserId => _pr_SMSUserRole_GetByUserId.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetActiveByUserId = new Lazy<string>(() => "pr_SMSUserRole_GetActiveByUserId");
        public static string pr_SMSUserRole_GetActiveByUserId => _pr_SMSUserRole_GetActiveByUserId.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByRoleValue = new Lazy<string>(() => "pr_SMSUserRole_GetByRoleValue");
        public static string pr_SMSUserRole_GetByRoleValue => _pr_SMSUserRole_GetByRoleValue.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByCategory = new Lazy<string>(() => "pr_SMSUserRole_GetByCategory");
        public static string pr_SMSUserRole_GetByCategory => _pr_SMSUserRole_GetByCategory.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByDepartment = new Lazy<string>(() => "pr_SMSUserRole_GetByDepartment");
        public static string pr_SMSUserRole_GetByDepartment => _pr_SMSUserRole_GetByDepartment.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByAuthorityLevel = new Lazy<string>(() => "pr_SMSUserRole_GetByAuthorityLevel");
        public static string pr_SMSUserRole_GetByAuthorityLevel => _pr_SMSUserRole_GetByAuthorityLevel.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetExpiringRoles = new Lazy<string>(() => "pr_SMSUserRole_GetExpiringRoles");
        public static string pr_SMSUserRole_GetExpiringRoles => _pr_SMSUserRole_GetExpiringRoles.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetAll = new Lazy<string>(() => "pr_SMSUserRole_GetAll");
        public static string pr_SMSUserRole_GetAll => _pr_SMSUserRole_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_Update = new Lazy<string>(() => "pr_SMSUserRole_Update");
        public static string pr_SMSUserRole_Update => _pr_SMSUserRole_Update.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_Deactivate = new Lazy<string>(() => "pr_SMSUserRole_Deactivate");
        public static string pr_SMSUserRole_Deactivate => _pr_SMSUserRole_Deactivate.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_Reactivate = new Lazy<string>(() => "pr_SMSUserRole_Reactivate");
        public static string pr_SMSUserRole_Reactivate => _pr_SMSUserRole_Reactivate.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_ExtendAssignment = new Lazy<string>(() => "pr_SMSUserRole_ExtendAssignment");
        public static string pr_SMSUserRole_ExtendAssignment => _pr_SMSUserRole_ExtendAssignment.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_BulkInsert = new Lazy<string>(() => "pr_SMSUserRole_BulkInsert");
        public static string pr_SMSUserRole_BulkInsert => _pr_SMSUserRole_BulkInsert.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetStatsReport = new Lazy<string>(() => "pr_SMSUserRole_GetStatsReport");
        public static string pr_SMSUserRole_GetStatsReport => _pr_SMSUserRole_GetStatsReport.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_ValidateUserAuthorization = new Lazy<string>(() => "pr_SMSUserRole_ValidateUserAuthorization");
        public static string pr_SMSUserRole_ValidateUserAuthorization => _pr_SMSUserRole_ValidateUserAuthorization.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetUserMaxAuthorityLevel = new Lazy<string>(() => "pr_SMSUserRole_GetUserMaxAuthorityLevel");
        public static string pr_SMSUserRole_GetUserMaxAuthorityLevel => _pr_SMSUserRole_GetUserMaxAuthorityLevel.Value;

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

        /// <summary>
        /// SMS Report CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Report_Insert = new Lazy<string>(() => "pr_Report_Insert");
        public static string pr_Report_Insert => _pr_Report_Insert.Value;

        private static readonly Lazy<string> _pr_Report_GetById = new Lazy<string>(() => "pr_Report_GetById");
        public static string pr_Report_GetById => _pr_Report_GetById.Value;

        private static readonly Lazy<string> _pr_Report_GetAll = new Lazy<string>(() => "pr_Report_GetAll");
        public static string pr_Report_GetAll => _pr_Report_GetAll.Value;

        private static readonly Lazy<string> _pr_Report_Update = new Lazy<string>(() => "pr_Report_Update");
        public static string pr_Report_Update => _pr_Report_Update.Value;

        private static readonly Lazy<string> _pr_Report_Delete = new Lazy<string>(() => "pr_Report_Delete");
        public static string pr_Report_Delete => _pr_Report_Delete.Value;

        /// <summary>
        /// SMS Investigation CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Investigation_Insert = new Lazy<string>(() => "pr_Investigation_Insert");
        public static string pr_Investigation_Insert => _pr_Investigation_Insert.Value;

        private static readonly Lazy<string> _pr_Investigation_GetById = new Lazy<string>(() => "pr_Investigation_GetById");
        public static string pr_Investigation_GetById => _pr_Investigation_GetById.Value;

        private static readonly Lazy<string> _pr_Investigation_GetAll = new Lazy<string>(() => "pr_Investigation_GetAll");
        public static string pr_Investigation_GetAll => _pr_Investigation_GetAll.Value;

        private static readonly Lazy<string> _pr_Investigation_Update = new Lazy<string>(() => "pr_Investigation_Update");
        public static string pr_Investigation_Update => _pr_Investigation_Update.Value;

        private static readonly Lazy<string> _pr_Investigation_Delete = new Lazy<string>(() => "pr_Investigation_Delete");
        public static string pr_Investigation_Delete => _pr_Investigation_Delete.Value;

        /// <summary>
        /// SMS Interview CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Interview_Insert = new Lazy<string>(() => "pr_Interview_Insert");
        public static string pr_Interview_Insert => _pr_Interview_Insert.Value;

        private static readonly Lazy<string> _pr_Interview_GetById = new Lazy<string>(() => "pr_Interview_GetById");
        public static string pr_Interview_GetById => _pr_Interview_GetById.Value;

        private static readonly Lazy<string> _pr_Interview_GetAll = new Lazy<string>(() => "pr_Interview_GetAll");
        public static string pr_Interview_GetAll => _pr_Interview_GetAll.Value;

        private static readonly Lazy<string> _pr_Interview_Update = new Lazy<string>(() => "pr_Interview_Update");
        public static string pr_Interview_Update => _pr_Interview_Update.Value;

        private static readonly Lazy<string> _pr_Interview_Delete = new Lazy<string>(() => "pr_Interview_Delete");
        public static string pr_Interview_Delete => _pr_Interview_Delete.Value;

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
    }
}