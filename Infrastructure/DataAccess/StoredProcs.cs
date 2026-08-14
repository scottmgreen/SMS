//-----------------------------------------------------------------------
// <copyright file="StoredProcs.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Stored procedure name constants providing centralized database procedure reference management with lazy initialization.
//                  Infrastructure utility providing shared functionality
//                  for data access and external system integration.
// </copyright>
//-----------------------------------------------------------------------

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

        private static readonly Lazy<string> _pr_SMSApplicationUser_GetByCode = new(() => "pr_SMSApplicationUser_GetByCode");
        public static string pr_SMSApplicationUser_GetByCode => _pr_SMSApplicationUser_GetByCode.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_GetByUserName = new(() => "pr_SMSApplicationUser_GetByUserName");
        public static string pr_SMSApplicationUser_GetByUserName => _pr_SMSApplicationUser_GetByUserName.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_GetByRole = new(() => "pr_SMSApplicationUser_GetByRole");
        public static string pr_SMSApplicationUser_GetByRole => _pr_SMSApplicationUser_GetByRole.Value;

        //private static readonly Lazy<string> _pr_SMSApplicationUser_GetByPermissionLevel = new(() => "pr_SMSApplicationUser_GetByPermissionLevel");
        //public static string pr_SMSApplicationUser_GetByPermissionLevel => _pr_SMSApplicationUser_GetByPermissionLevel.Value;

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

        // 🔐 Two-Factor Authentication Procedures for SMS Application Users
        private static readonly Lazy<string> _pr_SMSApplicationUser_Setup2FA = new(() => "pr_SMSApplicationUser_Setup2FA");
        public static string pr_SMSApplicationUser_Setup2FA => _pr_SMSApplicationUser_Setup2FA.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_Update2FAFailedAttempts = new(() => "pr_SMSApplicationUser_Update2FAFailedAttempts");
        public static string pr_SMSApplicationUser_Update2FAFailedAttempts => _pr_SMSApplicationUser_Update2FAFailedAttempts.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_Reset2FAFailedAttempts = new(() => "pr_SMSApplicationUser_Reset2FAFailedAttempts");
        public static string pr_SMSApplicationUser_Reset2FAFailedAttempts => _pr_SMSApplicationUser_Reset2FAFailedAttempts.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUser_Disable2FA = new(() => "pr_SMSApplicationUser_Disable2FA");
        public static string pr_SMSApplicationUser_Disable2FA => _pr_SMSApplicationUser_Disable2FA.Value;

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

        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetByCode = new(() => "pr_SMSOrganizationalUser_GetByCode");
        public static string pr_SMSOrganizationalUser_GetByCode => _pr_SMSOrganizationalUser_GetByCode.Value;

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

        // 🔐 Two-Factor Authentication Procedures for SMS Organizational Users
        private static readonly Lazy<string> _pr_SMSOrganizationalUser_Setup2FA = new(() => "pr_SMSOrganizationalUser_Setup2FA");
        public static string pr_SMSOrganizationalUser_Setup2FA => _pr_SMSOrganizationalUser_Setup2FA.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUser_Update2FAFailedAttempts = new(() => "pr_SMSOrganizationalUser_Update2FAFailedAttempts");
        public static string pr_SMSOrganizationalUser_Update2FAFailedAttempts => _pr_SMSOrganizationalUser_Update2FAFailedAttempts.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUser_Reset2FAFailedAttempts = new(() => "pr_SMSOrganizationalUser_Reset2FAFailedAttempts");
        public static string pr_SMSOrganizationalUser_Reset2FAFailedAttempts => _pr_SMSOrganizationalUser_Reset2FAFailedAttempts.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUser_Disable2FA = new(() => "pr_SMSOrganizationalUser_Disable2FA");
        public static string pr_SMSOrganizationalUser_Disable2FA => _pr_SMSOrganizationalUser_Disable2FA.Value;

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

        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetByCode = new(() => "pr_SMSStakeholderUser_GetByCode");
        public static string pr_SMSStakeholderUser_GetByCode => _pr_SMSStakeholderUser_GetByCode.Value;

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

        private static readonly Lazy<string> _pr_SMSStakeholderUser_GetByGroupCode = new(() => "pr_SMSStakeholderUser_GetByGroupCode");
        public static string pr_SMSStakeholderUser_GetByGroupCode => _pr_SMSStakeholderUser_GetByGroupCode.Value;

        // 🔐 Two-Factor Authentication Procedures for SMS Stakeholder Users
        private static readonly Lazy<string> _pr_SMSStakeholderUser_Setup2FA = new(() => "pr_SMSStakeholderUser_Setup2FA");
        public static string pr_SMSStakeholderUser_Setup2FA => _pr_SMSStakeholderUser_Setup2FA.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUser_Update2FAFailedAttempts = new(() => "pr_SMSStakeholderUser_Update2FAFailedAttempts");
        public static string pr_SMSStakeholderUser_Update2FAFailedAttempts => _pr_SMSStakeholderUser_Update2FAFailedAttempts.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUser_Reset2FAFailedAttempts = new(() => "pr_SMSStakeholderUser_Reset2FAFailedAttempts");
        public static string pr_SMSStakeholderUser_Reset2FAFailedAttempts => _pr_SMSStakeholderUser_Reset2FAFailedAttempts.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUser_Disable2FA = new(() => "pr_SMSStakeholderUser_Disable2FA");
        public static string pr_SMSStakeholderUser_Disable2FA => _pr_SMSStakeholderUser_Disable2FA.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserTitle_GetAll = new(() => "pr_SMSStakeholderUserTitle_GetAll");
        public static string pr_SMSStakeholderUserTitle_GetAll => _pr_SMSStakeholderUserTitle_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSOrginization_GetAll = new(() => "pr_SMSOrginization_GetAll");
        public static string pr_SMSOrginization_GetAll => _pr_SMSOrginization_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSCompanies_GetAll = new(() => "pr_SMSCompanies_GetAll");
        public static string pr_SMSCompanies_GetAll => _pr_SMSCompanies_GetAll.Value;

        #endregion

        #region SMS Stakeholder Groups Stored Procedures

        /// <summary>
        /// Stored procedures for SMS Stakeholder Groups operations
        /// </summary>







        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_GetByCode = new(() => "pr_SMSStakeholderUserGroup_GetByCode");
        public static string pr_SMSStakeholderUserGroup_GetByCode => _pr_SMSStakeholderUserGroup_GetByCode.Value;

        /// <summary>
        /// Stored procedures for SMS Stakeholder User-Group membership operations (Junction Table)
        /// </summary>
        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_Insert = new(() => "pr_SMSStakeholderUserGroup_Insert");
        public static string pr_SMSStakeholderUserGroup_Insert => _pr_SMSStakeholderUserGroup_Insert.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_Update = new(() => "pr_SMSStakeholderUserGroup_Update");
        public static string pr_SMSStakeholderUserGroup_Update => _pr_SMSStakeholderUserGroup_Update.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_Delete = new(() => "pr_SMSStakeholderUserGroup_Delete");
        public static string pr_SMSStakeholderUserGroup_Delete => _pr_SMSStakeholderUserGroup_Delete.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_GetAll = new(() => "pr_SMSStakeholderUserGroup_GetAll");
        public static string pr_SMSStakeholderUserGroup_GetAll => _pr_SMSStakeholderUserGroup_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_GetByUserID = new(() => "pr_SMSStakeholderUserGroup_GetGroupsByUserCode");
        public static string pr_SMSStakeholderUserGroup_GetByUserID => _pr_SMSStakeholderUserGroup_GetByUserID.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_GetByGroupCode = new(() => "pr_SMSStakeholderUserGroup_GetByGroupCode");
        public static string pr_SMSStakeholderUserGroup_GetByGroupCode => _pr_SMSStakeholderUserGroup_GetByGroupCode.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_Assign = new(() => "pr_SMSStakeholderUserGroup_Assign");
        public static string pr_SMSStakeholderUserGroup_Assign => _pr_SMSStakeholderUserGroup_Assign.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_Remove = new(() => "pr_SMSStakeholderUserGroup_Remove");
        public static string pr_SMSStakeholderUserGroup_Remove => _pr_SMSStakeholderUserGroup_Remove.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_GetUsersByGroup = new(() => "pr_SMSStakeholderUserGroup_GetUsersByGroup");
        public static string pr_SMSStakeholderUserGroup_GetUsersByGroup => _pr_SMSStakeholderUserGroup_GetUsersByGroup.Value;

        private static readonly Lazy<string> _pr_SMSStakeholderUserGroup_ClearUserGroups = new(() => "pr_SMSStakeholderUserGroup_ClearUserGroups");
        public static string pr_SMSStakeholderUserGroup_ClearUserGroups => _pr_SMSStakeholderUserGroup_ClearUserGroups.Value;

        #endregion

        private static readonly Lazy<string> _pr_Tracking_GetByReportCode = new(() => "pr_Tracking_GetByReportCode");
        public static string pr_Tracking_GetByReportCode => _pr_Tracking_GetByReportCode.Value;


        #region SMS Application Groups Stored Procedures

        /// <summary>
        /// Stored procedures for SMS Application Groups operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSApplicationGroup_Insert = new(() => "pr_SMSApplicationUserGroup_Insert");
        public static string pr_SMSApplicationGroup_Insert => _pr_SMSApplicationGroup_Insert.Value;

        private static readonly Lazy<string> _pr_SMSApplicationGroup_Update = new(() => "pr_SMSApplicationUserGroup_Update");
        public static string pr_SMSApplicationGroup_Update => _pr_SMSApplicationGroup_Update.Value;

        private static readonly Lazy<string> _pr_SMSApplicationGroup_Delete = new(() => "pr_SMSApplicationUserGroup_Delete");
        public static string pr_SMSApplicationGroup_Delete => _pr_SMSApplicationGroup_Delete.Value;

        private static readonly Lazy<string> _pr_SMSApplicationGroup_GetAll = new(() => "pr_SMSApplicationUserGroup_GetAll");
        public static string pr_SMSApplicationGroup_GetAll => _pr_SMSApplicationGroup_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSApplicationGroups_GetByUserCode = new(() => "pr_SMSApplicationUserGroup_GetGroupsByUserCode");
        public static string pr_SMSApplicationGroups_GetByUserCode => _pr_SMSApplicationGroups_GetByUserCode.Value;

        private static readonly Lazy<string> _pr_SMSApplicationGroup_GetByCode = new(() => "pr_SMSApplicationUserGroup_GetByCode");
        public static string pr_SMSApplicationGroup_GetByCode => _pr_SMSApplicationGroup_GetByCode.Value;

        /// <summary>
        /// Stored procedures for SMS Application User-Group membership operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSApplicationUserGroup_Assign = new(() => "pr_SMSApplicationUserGroup_Assign");
        public static string pr_SMSApplicationUserGroup_Assign => _pr_SMSApplicationUserGroup_Assign.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUserGroup_Remove = new(() => "pr_SMSApplicationUserGroup_Remove");
        public static string pr_SMSApplicationUserGroup_Remove => _pr_SMSApplicationUserGroup_Remove.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUserGroup_GetUsersByGroup = new(() => "pr_SMSApplicationUserGroup_GetUsersByGroup");
        public static string pr_SMSApplicationUserGroup_GetUsersByGroup => _pr_SMSApplicationUserGroup_GetUsersByGroup.Value;

        private static readonly Lazy<string> _pr_SMSApplicationUserGroup_ClearUserGroups = new(() => "pr_SMSApplicationUserGroup_ClearUserGroups");
        public static string pr_SMSApplicationUserGroup_ClearUserGroups => _pr_SMSApplicationUserGroup_ClearUserGroups.Value;


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

        private static readonly Lazy<string> _pr_SMSUserRole_GetByCode = new Lazy<string>(() => "pr_SMSUserRole_GetByCode");
        public static string pr_SMSUserRole_GetByCode => _pr_SMSUserRole_GetByCode.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByApplicationUserId = new Lazy<string>(() => "pr_SMSUserRole_GetByApplicationUserCode");
        public static string pr_SMSUserRole_GetByApplicationUserId => _pr_SMSUserRole_GetByApplicationUserId.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetByStakeholderUserId = new Lazy<string>(() => "pr_SMSUserRole_GetByStakeholderUserCode");
        public static string pr_SMSUserRole_GetByStakeholderUserId => _pr_SMSUserRole_GetByStakeholderUserId.Value;

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

        private static readonly Lazy<string> _pr_SMSUserRole_GetByUserId = new Lazy<string>(() => "pr_SMSUserRole_GetByUserId");
        public static string pr_SMSUserRole_GetByUserId => _pr_SMSUserRole_GetByUserId.Value;

        private static readonly Lazy<string> _pr_SMSUserRole_GetActiveByUserId = new Lazy<string>(() => "pr_SMSUserRole_GetActiveByUserId");
        public static string pr_SMSUserRole_GetActiveByUserId => _pr_SMSUserRole_GetActiveByUserId.Value;

        /// <summary>
        /// SMS User Role Permission stored procedures
        /// </summary>
        private static readonly Lazy<string> _pr_SMSUserRolePermission_Update = new Lazy<string>(() => "pr_SMSUserRolePermission_Update");
        public static string pr_SMSUserRolePermission_Update => _pr_SMSUserRolePermission_Update.Value;

        private static readonly Lazy<string> _pr_SMSUserRolePermission_Insert = new Lazy<string>(() => "pr_SMSUserRolePermission_Insert");
        public static string pr_SMSUserRolePermission_Insert => _pr_SMSUserRolePermission_Insert.Value;

        #endregion

        #region Report Management Procedures

        /// <summary>
        /// Report Management Operations
        /// </summary>
        
        private static readonly Lazy<string> _pr_Report_GetAll = new Lazy<string>(() => "pr_Report_GetAll");
        public static string pr_Report_GetAll => _pr_Report_GetAll.Value;

        private static readonly Lazy<string> _pr_Report_GetByCode = new Lazy<string>(() => "pr_Report_GetByCode");
        public static string pr_Report_GetByCode => _pr_Report_GetByCode.Value;

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

        private static readonly Lazy<string> _pr_Hazard_GetByCode = new Lazy<string>(() => "pr_Hazard_GetByCode");
        public static string pr_Hazard_GetByCode => _pr_Hazard_GetByCode.Value;

        private static readonly Lazy<string> _pr_Hazard_GetByReportCode = new Lazy<string>(() => "pr_Hazard_GetByReportCode");
        public static string pr_Hazard_GetByReportCode => _pr_Hazard_GetByReportCode.Value;

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

        private static readonly Lazy<string> _pr_HazardLocation_GetByCode = new Lazy<string>(() => "pr_HazardLocation_GetByCode");
        public static string pr_HazardLocation_GetByCode => _pr_HazardLocation_GetByCode.Value;

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

        //private static readonly Lazy<string> _pr_Investigation_GetById = new Lazy<string>(() => "pr_Investigation_GetById");
        //public static string pr_Investigation_GetById => _pr_Investigation_GetById.Value;

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

        private static readonly Lazy<string> _pr_Investigation_Update = new Lazy<string>(() => "pr_Investigation_Update_Enhanced");
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
        /// 
        /// pr_Interview_Insert_Enhanced
        /// </summary>
        private static readonly Lazy<string> _pr_Interview_Insert_Enhanced = new Lazy<string>(() => "pr_Interview_Insert_Enhanced");
        public static string pr_Interview_Insert_Enhanced => _pr_Interview_Insert_Enhanced.Value;

        private static readonly Lazy<string> _pr_Interview_Update_Enhanced = new Lazy<string>(() => "pr_Interview_Update_Enhanced");
        public static string pr_Interview_Update_Enhanced => _pr_Interview_Update_Enhanced.Value;



        private static readonly Lazy<string> _pr_Interview_Insert = new Lazy<string>(() => "pr_Interview_Insert");
        public static string pr_Interview_Insert => _pr_Interview_Insert.Value;

        //private static readonly Lazy<string> _pr_Interview_GetById = new Lazy<string>(() => "pr_Interview_GetById");
        //public static string pr_Interview_GetById => _pr_Interview_GetById.Value;

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

        private static readonly Lazy<string> _pr_RiskAnalysis_GetByCode = new Lazy<string>(() => "pr_RiskAnalysis_GetByCode");
        public static string pr_RiskAnalysis_GetByCode => _pr_RiskAnalysis_GetByCode.Value;


        private static readonly Lazy<string> _pr_RiskAnalysis_GetByHazardCode = new Lazy<string>(() => "pr_RiskAnalysis_GetByHazardCode");
        public static string pr_RiskAnalysis_GetByHazardCode => _pr_RiskAnalysis_GetByHazardCode.Value;



        private static readonly Lazy<string> _pr_RiskAnalysis_GetAll = new Lazy<string>(() => "pr_RiskAnalysis_GetAll");
        public static string pr_RiskAnalysis_GetAll => _pr_RiskAnalysis_GetAll.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_Update = new Lazy<string>(() => "pr_RiskAnalysis_Update");
        public static string pr_RiskAnalysis_Update => _pr_RiskAnalysis_Update.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_Delete = new Lazy<string>(() => "pr_RiskAnalysis_Delete");
        public static string pr_RiskAnalysis_Delete => _pr_RiskAnalysis_Delete.Value;
        #endregion

        #region SMS Risk Assessment CRUD Operations
        /// <summary>
        /// SMS Risk Assessment CRUD Operations - UPDATED FOR STEPS 1-5
        /// </summary>
        private static readonly Lazy<string> _pr_RiskAssessment_Insert = new Lazy<string>(() => "pr_RiskAssessment_Insert");
        public static string pr_RiskAssessment_Insert => _pr_RiskAssessment_Insert.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_GetByCode = new Lazy<string>(() => "pr_RiskAssessment_GetByCode");
        public static string pr_RiskAssessment_GetByCode => _pr_RiskAssessment_GetByCode.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_GetByHazardCode = new Lazy<string>(() => "pr_RiskAssessment_GetByHazardCode");
        public static string pr_RiskAssessment_GetByHazardCode => _pr_RiskAssessment_GetByHazardCode.Value;





        private static readonly Lazy<string> _pr_RiskAssessment_GetAll = new Lazy<string>(() => "pr_RiskAssessment_GetAll");
        public static string pr_RiskAssessment_GetAll => _pr_RiskAssessment_GetAll.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_Update = new Lazy<string>(() => "pr_RiskAssessment_Update");
        public static string pr_RiskAssessment_Update => _pr_RiskAssessment_Update.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_Delete = new Lazy<string>(() => "pr_RiskAssessment_Delete");
        public static string pr_RiskAssessment_Delete => _pr_RiskAssessment_Delete.Value;

        /// <summary>
        /// Risk Assessment Step-Specific Update Procedures (NEW)
        /// </summary>
        private static readonly Lazy<string> _pr_RiskAssessment_UpdateStep1 = new Lazy<string>(() => "pr_RiskAssessment_UpdateStep1");
        public static string pr_RiskAssessment_UpdateStep1 => _pr_RiskAssessment_UpdateStep1.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_UpdateStep3 = new Lazy<string>(() => "pr_RiskAssessment_UpdateStep3");
        public static string pr_RiskAssessment_UpdateStep3 => _pr_RiskAssessment_UpdateStep3.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_UpdateStep4 = new Lazy<string>(() => "pr_RiskAssessment_UpdateStep4");
        public static string pr_RiskAssessment_UpdateStep4 => _pr_RiskAssessment_UpdateStep4.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_UpdateStep5 = new Lazy<string>(() => "pr_RiskAssessment_UpdateStep5");
        public static string pr_RiskAssessment_UpdateStep5 => _pr_RiskAssessment_UpdateStep5.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_UpdateProgress = new Lazy<string>(() => "pr_RiskAssessment_UpdateProgress");
        public static string pr_RiskAssessment_UpdateProgress => _pr_RiskAssessment_UpdateProgress.Value;
        #endregion

        #region SMS Mitigation CRUD Operations
        /// <summary>
        /// SMS Mitigation CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Mitigation_Insert = new Lazy<string>(() => "pr_Mitigation_Insert");
        public static string pr_Mitigation_Insert => _pr_Mitigation_Insert.Value;

        private static readonly Lazy<string> _pr_Mitigation_GetByCode = new Lazy<string>(() => "pr_Mitigation_GetByCode");
        public static string pr_Mitigation_GetByCode => _pr_Mitigation_GetByCode.Value;

        private static readonly Lazy<string> _pr_Mitigation_GetAll = new Lazy<string>(() => "pr_Mitigation_GetAll");
        public static string pr_Mitigation_GetAll => _pr_Mitigation_GetAll.Value;

        private static readonly Lazy<string> _pr_Mitigation_Update = new Lazy<string>(() => "pr_Mitigation_Update");
        public static string pr_Mitigation_Update => _pr_Mitigation_Update.Value;

        private static readonly Lazy<string> _pr_Mitigation_Delete = new Lazy<string>(() => "pr_Mitigation_Delete");
        public static string pr_Mitigation_Delete => _pr_Mitigation_Delete.Value;

        /// <summary>
        /// SMS Mitigation Query Operations - Enhanced for Steps 1-5
        /// </summary>
        private static readonly Lazy<string> _pr_Mitigation_GetByHazardCode = new Lazy<string>(() => "pr_Mitigation_GetByHazardCode");
        public static string pr_Mitigation_GetByHazardCode => _pr_Mitigation_GetByHazardCode.Value;
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

        private static readonly Lazy<string> _pr_ReportValidation_GetByCode = new Lazy<string>(() => "pr_ReportValidation_GetByCode");
        public static string pr_ReportValidation_GetByCode => _pr_ReportValidation_GetByCode.Value;

        private static readonly Lazy<string> _pr_ReportValidation_GetByReportCode = new Lazy<string>(() => "pr_ReportValidation_GetByReportCode");
        public static string pr_ReportValidation_GetByReportCode => _pr_ReportValidation_GetByReportCode.Value;



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

        private static readonly Lazy<string> _pr_ScoringPanel_GetByCode = new Lazy<string>(() => "pr_ScoringPanel_GetByCode");
        public static string pr_ScoringPanel_GetByCode => _pr_ScoringPanel_GetByCode.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_GetAll = new Lazy<string>(() => "pr_ScoringPanel_GetAll");
        public static string pr_ScoringPanel_GetAll => _pr_ScoringPanel_GetAll.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_GetByHazardCode = new Lazy<string>(() => "pr_ScoringPanel_GetByHazardCode");
        public static string pr_ScoringPanel_GetByHazardCode => _pr_ScoringPanel_GetByHazardCode.Value;

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

        private static readonly Lazy<string> _pr_AirportSharedDataset_GetByCode = new Lazy<string>(() => "pr_AirportSharedDataset_GetByCode");
        public static string pr_AirportSharedDataset_GetByCode => _pr_AirportSharedDataset_GetByCode.Value;

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


        private static readonly Lazy<string> _pr_HazardFile_GetByCode = new Lazy<string>(() => "pr_HazardFile_GetByCode");
        public static string pr_HazardFile_GetByCode => _pr_HazardFile_GetByCode.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetByHazardCode = new Lazy<string>(() => "pr_HazardFile_GetByHazardCode");
        public static string pr_HazardFile_GetByHazardCode => _pr_HazardFile_GetByHazardCode.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetByReportCode = new Lazy<string>(() => "pr_HazardFile_GetByReportCode");
        public static string pr_HazardFile_GetByReportCode => _pr_HazardFile_GetByReportCode.Value;

        private static readonly Lazy<string> _pr_HazardFile_GetActiveFiles = new Lazy<string>(() => "pr_HazardFile_GetActiveFiles");
        public static string pr_HazardFile_GetActiveFiles => _pr_HazardFile_GetActiveFiles.Value;

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

        #region SMS Organizational Group Stored Procedures

        /// <summary>
        /// Stored procedures for SMS Organizational Group operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSOrganizationalGroup_Insert = new(() => "pr_SMSOrganizationalUserGroup_Insert");
        public static string pr_SMSOrganizationalGroup_Insert => _pr_SMSOrganizationalGroup_Insert.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalGroup_Update = new(() => "pr_SMSOrganizationalUserGroup_Update");
        public static string pr_SMSOrganizationalGroup_Update => _pr_SMSOrganizationalGroup_Update.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalGroup_Delete = new(() => "pr_SMSOrganizationalUserGroup_Delete");
        public static string pr_SMSOrganizationalGroup_Delete => _pr_SMSOrganizationalGroup_Delete.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalGroup_GetAll = new(() => "pr_SMSOrganizationalUserGroup_GetAll");
        public static string pr_SMSOrganizationalGroup_GetAll => _pr_SMSOrganizationalGroup_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalGroup_GetByCode = new(() => "pr_SMSOrganizationalUserGroup_GetByCode");
        public static string pr_SMSOrganizationalGroup_GetByCode => _pr_SMSOrganizationalGroup_GetByCode.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalGroups_GetByUserCode = new(() => "pr_SMSOrganizationalUserGroup_GetByUserCode");
        public static string pr_SMSOrganizationalGroups_GetByUserCode => _pr_SMSOrganizationalGroups_GetByUserCode.Value;

        /// <summary>
        /// Stored procedures for SMS Organizational User-Group membership operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSOrganizationalUserGroup_Assign = new(() => "pr_SMSOrganizationalUserGroup_Assign");
        public static string pr_SMSOrganizationalUserGroup_Assign => _pr_SMSOrganizationalUserGroup_Assign.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUserGroup_Remove = new(() => "pr_SMSOrganizationalUserGroup_Remove");
        public static string pr_SMSOrganizationalUserGroup_Remove => _pr_SMSOrganizationalUserGroup_Remove.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUserGroup_GetUsersByGroup = new(() => "pr_SMSOrganizationalUserGroup_GetUsersByGroup");
        public static string pr_SMSOrganizationalUserGroup_GetUsersByGroup => _pr_SMSOrganizationalUserGroup_GetUsersByGroup.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUserGroup_ClearUserGroups = new(() => "pr_SMSOrganizationalUserGroup_ClearUserGroups");
        public static string pr_SMSOrganizationalUserGroup_ClearUserGroups => _pr_SMSOrganizationalUserGroup_ClearUserGroups.Value;

        private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetByGroupCode = new(() => "pr_SMSOrganizationalUser_GetByGroupCode");
        public static string pr_SMSOrganizationalUser_GetByGroupCode => _pr_SMSOrganizationalUser_GetByGroupCode.Value;

        #endregion

        #region SMS Safety Performance Indicator CRUD Operations
        /// <summary>
        /// SMS Safety Performance Indicator CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SafetyPerformanceIndicator_Insert = new(() => "pr_SafetyPerformanceIndicator_Insert");
        public static string pr_SafetyPerformanceIndicator_Insert => _pr_SafetyPerformanceIndicator_Insert.Value;

        //private static readonly Lazy<string> _pr_SafetyPerformanceIndicator_GetByCode = new(() => "pr_SafetyPerformanceIndicator_GetById");
        //public static string pr_SafetyPerformanceIndicator_GetById => _pr_SafetyPerformanceIndicator_GetById.Value;

        private static readonly Lazy<string> _pr_SafetyPerformanceIndicator_GetByCode = new(() => "pr_SafetyPerformanceIndicator_GetByCode");
        public static string pr_SafetyPerformanceIndicator_GetByCode => _pr_SafetyPerformanceIndicator_GetByCode.Value;

        private static readonly Lazy<string> _pr_SafetyPerformanceIndicator_GetAll = new(() => "pr_SafetyPerformanceIndicator_GetAll");
        public static string pr_SafetyPerformanceIndicator_GetAll => _pr_SafetyPerformanceIndicator_GetAll.Value;

        private static readonly Lazy<string> _pr_SafetyPerformanceIndicator_GetByType = new(() => "pr_SafetyPerformanceIndicator_GetByType");
        public static string pr_SafetyPerformanceIndicator_GetByType => _pr_SafetyPerformanceIndicator_GetByType.Value;

        private static readonly Lazy<string> _pr_SafetyPerformanceIndicator_GetByDepartment = new(() => "pr_SafetyPerformanceIndicator_GetByDepartment");
        public static string pr_SafetyPerformanceIndicator_GetByDepartment => _pr_SafetyPerformanceIndicator_GetByDepartment.Value;

        private static readonly Lazy<string> _pr_SafetyPerformanceIndicator_Update = new(() => "pr_SafetyPerformanceIndicator_Update");
        public static string pr_SafetyPerformanceIndicator_Update => _pr_SafetyPerformanceIndicator_Update.Value;

        private static readonly Lazy<string> _pr_SafetyPerformanceIndicator_Delete = new(() => "pr_SafetyPerformanceIndicator_Delete");
        public static string pr_SafetyPerformanceIndicator_Delete => _pr_SafetyPerformanceIndicator_Delete.Value;

        /// <summary>
        /// SMS Safety Performance Indicator Data Points Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SPIDataPoint_Insert = new(() => "pr_SPIDataPoint_Insert");
        public static string pr_SPIDataPoint_Insert => _pr_SPIDataPoint_Insert.Value;

        private static readonly Lazy<string> _pr_SPIDataPoint_GetBySPICode = new(() => "pr_SPIDataPoint_GetBySPICode");
        public static string pr_SPIDataPoint_GetBySPICode => _pr_SPIDataPoint_GetBySPICode.Value;

        private static readonly Lazy<string> _pr_SPIDataPoint_Update = new(() => "pr_SPIDataPoint_Update");
        public static string pr_SPIDataPoint_Update => _pr_SPIDataPoint_Update.Value;

        private static readonly Lazy<string> _pr_SPIDataPoint_Delete = new(() => "pr_SPIDataPoint_Delete");
        public static string pr_SPIDataPoint_Delete => _pr_SPIDataPoint_Delete.Value;
        #endregion

        #region SMS Audit Management CRUD Operations
        /// <summary>
        /// SMS Audit Plan CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSAuditPlan_Insert = new(() => "pr_SMSAuditPlan_Insert");
        public static string pr_SMSAuditPlan_Insert => _pr_SMSAuditPlan_Insert.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_GetById = new(() => "pr_SMSAuditPlan_GetById");
        public static string pr_SMSAuditPlan_GetById => _pr_SMSAuditPlan_GetById.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_GetByCode = new(() => "pr_SMSAuditPlan_GetByCode");
        public static string pr_SMSAuditPlan_GetByCode => _pr_SMSAuditPlan_GetByCode.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_GetAll = new(() => "pr_SMSAuditPlan_GetAll");
        public static string pr_SMSAuditPlan_GetAll => _pr_SMSAuditPlan_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_GetByType = new(() => "pr_SMSAuditPlan_GetByType");
        public static string pr_SMSAuditPlan_GetByType => _pr_SMSAuditPlan_GetByType.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_GetByDepartment = new(() => "pr_SMSAuditPlan_GetByDepartment");
        public static string pr_SMSAuditPlan_GetByDepartment => _pr_SMSAuditPlan_GetByDepartment.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_GetRequiringApproval = new(() => "pr_SMSAuditPlan_GetRequiringApproval");
        public static string pr_SMSAuditPlan_GetRequiringApproval => _pr_SMSAuditPlan_GetRequiringApproval.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_GetCalendarData = new(() => "pr_SMSAuditPlan_GetCalendarData");
        public static string pr_SMSAuditPlan_GetCalendarData => _pr_SMSAuditPlan_GetCalendarData.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_Update = new(() => "pr_SMSAuditPlan_Update");
        public static string pr_SMSAuditPlan_Update => _pr_SMSAuditPlan_Update.Value;

        private static readonly Lazy<string> _pr_SMSAuditPlan_Delete = new(() => "pr_SMSAuditPlan_Delete");
        public static string pr_SMSAuditPlan_Delete => _pr_SMSAuditPlan_Delete.Value;

        /// <summary>
        /// SMS Audit CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSAudit_Insert = new(() => "pr_SMSAudit_Insert");
        public static string pr_SMSAudit_Insert => _pr_SMSAudit_Insert.Value;

        private static readonly Lazy<string> _pr_SMSAudit_GetById = new(() => "pr_SMSAudit_GetById");
        public static string pr_SMSAudit_GetById => _pr_SMSAudit_GetById.Value;

        private static readonly Lazy<string> _pr_SMSAudit_GetByCode = new(() => "pr_SMSAudit_GetByCode");
        public static string pr_SMSAudit_GetByCode => _pr_SMSAudit_GetByCode.Value;

        private static readonly Lazy<string> _pr_SMSAudit_GetAll = new(() => "pr_SMSAudit_GetAll");
        public static string pr_SMSAudit_GetAll => _pr_SMSAudit_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSAudit_GetByPlan = new(() => "pr_SMSAudit_GetByPlan");
        public static string pr_SMSAudit_GetByPlan => _pr_SMSAudit_GetByPlan.Value;

        private static readonly Lazy<string> _pr_SMSAudit_GetByStatus = new(() => "pr_SMSAudit_GetByStatus");
        public static string pr_SMSAudit_GetByStatus => _pr_SMSAudit_GetByStatus.Value;

        private static readonly Lazy<string> _pr_SMSAudit_GetByAuditor = new(() => "pr_SMSAudit_GetByAuditor");
        public static string pr_SMSAudit_GetByAuditor => _pr_SMSAudit_GetByAuditor.Value;

        private static readonly Lazy<string> _pr_SMSAudit_GetOverdue = new(() => "pr_SMSAudit_GetOverdue");
        public static string pr_SMSAudit_GetOverdue => _pr_SMSAudit_GetOverdue.Value;

        private static readonly Lazy<string> _pr_SMSAudit_GetExecutionDashboard = new(() => "pr_SMSAudit_GetExecutionDashboard");
        public static string pr_SMSAudit_GetExecutionDashboard => _pr_SMSAudit_GetExecutionDashboard.Value;

        private static readonly Lazy<string> _pr_SMSAudit_ScheduleFromPlan = new(() => "pr_SMSAudit_ScheduleFromPlan");
        public static string pr_SMSAudit_ScheduleFromPlan => _pr_SMSAudit_ScheduleFromPlan.Value;

        private static readonly Lazy<string> _pr_SMSAudit_Update = new(() => "pr_SMSAudit_Update");
        public static string pr_SMSAudit_Update => _pr_SMSAudit_Update.Value;

        private static readonly Lazy<string> _pr_SMSAudit_Delete = new(() => "pr_SMSAudit_Delete");
        public static string pr_SMSAudit_Delete => _pr_SMSAudit_Delete.Value;

        /// <summary>
        /// SMS Audit Finding CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSAuditFinding_Insert = new(() => "pr_SMSAuditFinding_Insert");
        public static string pr_SMSAuditFinding_Insert => _pr_SMSAuditFinding_Insert.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetById = new(() => "pr_SMSAuditFinding_GetById");
        public static string pr_SMSAuditFinding_GetById => _pr_SMSAuditFinding_GetById.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetByCode = new(() => "pr_SMSAuditFinding_GetByCode");
        public static string pr_SMSAuditFinding_GetByCode => _pr_SMSAuditFinding_GetByCode.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetAll = new(() => "pr_SMSAuditFinding_GetAll");
        public static string pr_SMSAuditFinding_GetAll => _pr_SMSAuditFinding_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetByAudit = new(() => "pr_SMSAuditFinding_GetByAudit");
        public static string pr_SMSAuditFinding_GetByAudit => _pr_SMSAuditFinding_GetByAudit.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetBySeverity = new(() => "pr_SMSAuditFinding_GetBySeverity");
        public static string pr_SMSAuditFinding_GetBySeverity => _pr_SMSAuditFinding_GetBySeverity.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetByStatus = new(() => "pr_SMSAuditFinding_GetByStatus");
        public static string pr_SMSAuditFinding_GetByStatus => _pr_SMSAuditFinding_GetByStatus.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetByResponsiblePerson = new(() => "pr_SMSAuditFinding_GetByResponsiblePerson");
        public static string pr_SMSAuditFinding_GetByResponsiblePerson => _pr_SMSAuditFinding_GetByResponsiblePerson.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetOverdue = new(() => "pr_SMSAuditFinding_GetOverdue");
        public static string pr_SMSAuditFinding_GetOverdue => _pr_SMSAuditFinding_GetOverdue.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetRequiringVerification = new(() => "pr_SMSAuditFinding_GetRequiringVerification");
        public static string pr_SMSAuditFinding_GetRequiringVerification => _pr_SMSAuditFinding_GetRequiringVerification.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_GetStatistics = new(() => "pr_SMSAuditFinding_GetStatistics");
        public static string pr_SMSAuditFinding_GetStatistics => _pr_SMSAuditFinding_GetStatistics.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_Update = new(() => "pr_SMSAuditFinding_Update");
        public static string pr_SMSAuditFinding_Update => _pr_SMSAuditFinding_Update.Value;

        private static readonly Lazy<string> _pr_SMSAuditFinding_Delete = new(() => "pr_SMSAuditFinding_Delete");
        public static string pr_SMSAuditFinding_Delete => _pr_SMSAuditFinding_Delete.Value;

        /// <summary>
        /// SMS Audit Evidence CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSAuditEvidence_Insert = new(() => "pr_SMSAuditEvidence_Insert");
        public static string pr_SMSAuditEvidence_Insert => _pr_SMSAuditEvidence_Insert.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetById = new(() => "pr_SMSAuditEvidence_GetById");
        public static string pr_SMSAuditEvidence_GetById => _pr_SMSAuditEvidence_GetById.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetByCode = new(() => "pr_SMSAuditEvidence_GetByCode");
        public static string pr_SMSAuditEvidence_GetByCode => _pr_SMSAuditEvidence_GetByCode.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetAll = new(() => "pr_SMSAuditEvidence_GetAll");
        public static string pr_SMSAuditEvidence_GetAll => _pr_SMSAuditEvidence_GetAll.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetByAudit = new(() => "pr_SMSAuditEvidence_GetByAudit");
        public static string pr_SMSAuditEvidence_GetByAudit => _pr_SMSAuditEvidence_GetByAudit.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetByFinding = new(() => "pr_SMSAuditEvidence_GetByFinding");
        public static string pr_SMSAuditEvidence_GetByFinding => _pr_SMSAuditEvidence_GetByFinding.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetByType = new(() => "pr_SMSAuditEvidence_GetByType");
        public static string pr_SMSAuditEvidence_GetByType => _pr_SMSAuditEvidence_GetByType.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetByCollector = new(() => "pr_SMSAuditEvidence_GetByCollector");
        public static string pr_SMSAuditEvidence_GetByCollector => _pr_SMSAuditEvidence_GetByCollector.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetUnverified = new(() => "pr_SMSAuditEvidence_GetUnverified");
        public static string pr_SMSAuditEvidence_GetUnverified => _pr_SMSAuditEvidence_GetUnverified.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetArchived = new(() => "pr_SMSAuditEvidence_GetArchived");
        public static string pr_SMSAuditEvidence_GetArchived => _pr_SMSAuditEvidence_GetArchived.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetStatistics = new(() => "pr_SMSAuditEvidence_GetStatistics");
        public static string pr_SMSAuditEvidence_GetStatistics => _pr_SMSAuditEvidence_GetStatistics.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_UploadFile = new(() => "pr_SMSAuditEvidence_UploadFile");
        public static string pr_SMSAuditEvidence_UploadFile => _pr_SMSAuditEvidence_UploadFile.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_DownloadFile = new(() => "pr_SMSAuditEvidence_DownloadFile");
        public static string pr_SMSAuditEvidence_DownloadFile => _pr_SMSAuditEvidence_DownloadFile.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_Update = new(() => "pr_SMSAuditEvidence_Update");
        public static string pr_SMSAuditEvidence_Update => _pr_SMSAuditEvidence_Update.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_Delete = new(() => "pr_SMSAuditEvidence_Delete");
        public static string pr_SMSAuditEvidence_Delete => _pr_SMSAuditEvidence_Delete.Value;

        /// <summary>
        /// SMS Audit Utility Operations
        /// </summary>
        private static readonly Lazy<string> _pr_SMSAudit_UpdateFindingsSummary = new(() => "pr_SMSAudit_UpdateFindingsSummary");
        public static string pr_SMSAudit_UpdateFindingsSummary => _pr_SMSAudit_UpdateFindingsSummary.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_Archive = new(() => "pr_SMSAuditEvidence_Archive");
        public static string pr_SMSAuditEvidence_Archive => _pr_SMSAuditEvidence_Archive.Value;

        private static readonly Lazy<string> _pr_SMSAuditEvidence_GetRetentionReview = new(() => "pr_SMSAuditEvidence_GetRetentionReview");
        public static string pr_SMSAuditEvidence_GetRetentionReview => _pr_SMSAuditEvidence_GetRetentionReview.Value;
        #endregion

        #region HazardReportTracking CRUD Operations
        /// <summary>
        /// HazardReportTracking CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_HazardReportTracking_Insert = new(() => "pr_HazardReportTracking_Insert");
        public static string pr_HazardReportTracking_Insert => _pr_HazardReportTracking_Insert.Value;

        private static readonly Lazy<string> _pr_HazardReportTracking_GetByTrackingCode = new(() => "pr_HazardReportTracking_GetByTrackingCode");
        public static string pr_HazardReportTracking_GetByTrackingCode => _pr_HazardReportTracking_GetByTrackingCode.Value;

        private static readonly Lazy<string> _pr_HazardReportTracking_GetAll = new(() => "pr_HazardReportTracking_GetAll");
        public static string pr_HazardReportTracking_GetAll => _pr_HazardReportTracking_GetAll.Value;

        private static readonly Lazy<string> _pr_HazardReportTracking_GetByHazardCode = new(() => "pr_HazardReportTracking_GetByHazardCode");
        public static string pr_HazardReportTracking_GetByHazardCode => _pr_HazardReportTracking_GetByHazardCode.Value;

        private static readonly Lazy<string> _pr_HazardReportTracking_GetByReportCode = new(() => "pr_HazardReportTracking_GetByReportCode");
        public static string pr_HazardReportTracking_GetByReportCode => _pr_HazardReportTracking_GetByReportCode.Value;

        private static readonly Lazy<string> _pr_HazardReportTracking_Update = new(() => "pr_HazardReportTracking_Update");
        public static string pr_HazardReportTracking_Update => _pr_HazardReportTracking_Update.Value;

        private static readonly Lazy<string> _pr_HazardReportTracking_Delete = new(() => "pr_HazardReportTracking_Delete");
        public static string pr_HazardReportTracking_Delete => _pr_HazardReportTracking_Delete.Value;
        #endregion

        #region EventQueue Operations
        /// <summary>
        /// EventQueue Operations
        /// </summary>
        private static readonly Lazy<string> _pr_EventQueue_Insert = new(() => "pr_EventQueue_Insert");
        public static string pr_EventQueue_Insert => _pr_EventQueue_Insert.Value;

        private static readonly Lazy<string> _pr_EventQueue_LeaseBatch = new(() => "pr_EventQueue_LeaseBatch");
        public static string pr_EventQueue_LeaseBatch => _pr_EventQueue_LeaseBatch.Value;

        private static readonly Lazy<string> _pr_EventQueue_MarkFailed = new(() => "pr_EventQueue_MarkFailed");
        public static string pr_EventQueue_MarkFailed => _pr_EventQueue_MarkFailed.Value;

        private static readonly Lazy<string> _pr_EventQueue_MarkProcessed = new(() => "pr_EventQueue_MarkProcessed");
        public static string pr_EventQueue_MarkProcessed => _pr_EventQueue_MarkProcessed.Value;

        private static readonly Lazy<string> _pr_EventQueue_GetByCode = new(() => "pr_EventQueue_GetByCode");
        public static string pr_EventQueue_GetByCode => _pr_EventQueue_GetByCode.Value;

        private static readonly Lazy<string> _pr_EventQueue_GetPending = new(() => "pr_EventQueue_GetPending");
        public static string pr_EventQueue_GetPending => _pr_EventQueue_GetPending.Value;

        private static readonly Lazy<string> _pr_EventQueue_GetByStatus = new(() => "pr_EventQueue_GetByStatus");
        public static string pr_EventQueue_GetByStatus => _pr_EventQueue_GetByStatus.Value;

        private static readonly Lazy<string> _pr_EventQueue_Cancel = new(() => "pr_EventQueue_Cancel");
        public static string pr_EventQueue_Cancel => _pr_EventQueue_Cancel.Value;

        private static readonly Lazy<string> _pr_EventQueue_ClearCompleted = new(() => "pr_EventQueue_ClearCompleted");
        public static string pr_EventQueue_ClearCompleted => _pr_EventQueue_ClearCompleted.Value;

        private static readonly Lazy<string> _pr_EventQueue_ClearAll = new(() => "pr_EventQueue_ClearAll");
        public static string pr_EventQueue_ClearAll => _pr_EventQueue_ClearAll.Value;

        private static readonly Lazy<string> _pr_EventQueue_RecoverExpiredLocks = new(() => "pr_EventQueue_RecoverExpiredLocks");
        public static string pr_EventQueue_RecoverExpiredLocks => _pr_EventQueue_RecoverExpiredLocks.Value;

        private static readonly Lazy<string> _pr_EventQueue_GetStats = new(() => "pr_EventQueue_GetStats");
        public static string pr_EventQueue_GetStats => _pr_EventQueue_GetStats.Value;
        #endregion
    }
}
