//-----------------------------------------------------------------------
// <copyright file="FieldNames.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Field name constants providing consistent database column references and query parameter naming.
//                  Infrastructure utility providing shared functionality
//                  for data access and external system integration.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Common;

public static class FieldNames
{
    /// <summary>
    /// Common ID field used across all tables
    /// </summary>
    private static readonly Lazy<string> _fId = new Lazy<string>(() => "fldi_ID");
    public static string fId => _fId.Value;

    /// <summary>
    /// Audit fields - inherited from BaseAuditableEntity
    /// </summary>
    private static readonly Lazy<string> _fCreatedBy = new Lazy<string>(() => "fldv_CreatedBy");
    public static string fCreatedBy => _fCreatedBy.Value;

    private static readonly Lazy<string> _fCreatedDate = new Lazy<string>(() => "fldd_CreatedDate");
    public static string fCreatedDate => _fCreatedDate.Value;

    private static readonly Lazy<string> _fUpdatedBy = new Lazy<string>(() => "fldv_UpdatedBy");
    public static string fUpdatedBy => _fUpdatedBy.Value;

    private static readonly Lazy<string> _fUpdatedDate = new Lazy<string>(() => "fldd_UpdatedDate");
    public static string fUpdatedDate => _fUpdatedDate.Value;

    /// <summary>
    /// UserRole
    /// </summary>
    private static readonly Lazy<string> _fSMSRoleCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSRoleCode => _fSMSRoleCode.Value;

    private static readonly Lazy<string> _fSMSUserRoleCode = new Lazy<string>(() => "fldv_SMSUserRoleCode");
    public static string fSMSUserRoleCode => _fSMSUserRoleCode.Value;

    private static readonly Lazy<string> _fSMSUserRoleName = new Lazy<string>(() => "fldv_Name");
    public static string fSMSUserRoleName => _fSMSUserRoleName.Value;


    private static readonly Lazy<string> _fSMSUserRolePermissionCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSUserRolePermissionCode => _fSMSUserRolePermissionCode.Value;

    private static readonly Lazy<string> _fSMSUserRolePermissionSMSUserRoleCode = new Lazy<string>(() => "fldv_SMSUserRoleCode");
    public static string fSMSUserRolePermissionSMSUserRoleCode => _fSMSUserRoleCode.Value;

    private static readonly Lazy<string> _fSMSUserRolePermissionModule = new Lazy<string>(() => "fldv_Module");
    public static string fSMSUserRolePermissionModule => _fSMSUserRolePermissionModule.Value;

    private static readonly Lazy<string> _fSMSUserRolePermissionCreate = new Lazy<string>(() => "fldb_Create");
    public static string fSMSUserRolePermissionCreate => _fSMSUserRolePermissionCreate.Value;

    private static readonly Lazy<string> _fSMSUserRolePermissionRead = new Lazy<string>(() => "fldb_Read");
    public static string fSMSUserRolePermissionRead => _fSMSUserRolePermissionRead.Value;

    private static readonly Lazy<string> _fSMSUserRolePermissionUpdate = new Lazy<string>(() => "fldb_Update");
    public static string fSMSUserRolePermissionUpdate => _fSMSUserRolePermissionUpdate.Value;

    private static readonly Lazy<string> _fSMSUserRolePermissionDelete = new Lazy<string>(() => "fldb_Delete");
    public static string fSMSUserRolePermissionDelete => _fSMSUserRolePermissionDelete.Value;







    /// <summary>
    /// SMS Application Users table (tbld_SMSApplicationUsers)
    /// </summary>
    private static readonly Lazy<string> _fSMSApplicationUserCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSApplicationUserCode => _fSMSApplicationUserCode.Value;

    private static readonly Lazy<string> _fSMSApplicationUserFirstName = new Lazy<string>(() => "fldv_FirstName");
    public static string fSMSApplicationUserFirstName => _fSMSApplicationUserFirstName.Value;

    private static readonly Lazy<string> _fSMSApplicationUserLastName = new Lazy<string>(() => "fldv_LastName");
    public static string fSMSApplicationUserLastName => _fSMSApplicationUserLastName.Value;

    private static readonly Lazy<string> _fSMSApplicationUserUserName = new Lazy<string>(() => "fldv_UserName");
    public static string fSMSApplicationUserUserName => _fSMSApplicationUserUserName.Value;

    private static readonly Lazy<string> _fSMSApplicationUserPassword = new Lazy<string>(() => "fldv_Password");
    public static string fSMSApplicationUserPassword => _fSMSApplicationUserPassword.Value;

    private static readonly Lazy<string> _fSMSApplicationUserTypeCode = new Lazy<string>(() => "fldv_ApplicationUserTypeCode");
    public static string fSMSApplicationUserTypeCode => _fSMSApplicationUserTypeCode.Value;

    private static readonly Lazy<string> _fSMSApplicationUserApplicationRole = new Lazy<string>(() => "fldv_SMSUserRoleCode");
    public static string fSMSApplicationUserApplicationRole => _fSMSApplicationUserApplicationRole.Value;

    private static readonly Lazy<string> _fSMSApplicationUserIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSApplicationUserIsActive => _fSMSApplicationUserIsActive.Value;

    private static readonly Lazy<string> _fSMSApplicationUserLastLoginDate = new Lazy<string>(() => "fldd_LastLoginDate");
    public static string fSMSApplicationUserLastLoginDate => _fSMSApplicationUserLastLoginDate.Value;

    // 🔐 Two-Factor Authentication Fields for SMS Application Users
    private static readonly Lazy<string> _fSMSApplicationUserTwoFactorSecretKey = new Lazy<string>(() => "fldv_TwoFactorSecretKey");
    public static string fSMSApplicationUserTwoFactorSecretKey => _fSMSApplicationUserTwoFactorSecretKey.Value;

    private static readonly Lazy<string> _fSMSApplicationUserTwoFactorEnabled = new Lazy<string>(() => "fldb_TwoFactorEnabled");
    public static string fSMSApplicationUserTwoFactorEnabled => _fSMSApplicationUserTwoFactorEnabled.Value;

    private static readonly Lazy<string> _fSMSApplicationUserBackupCodes = new Lazy<string>(() => "fldv_BackupCodes");
    public static string fSMSApplicationUserBackupCodes => _fSMSApplicationUserBackupCodes.Value;

    private static readonly Lazy<string> _fSMSApplicationUserTwoFactorSetupDate = new Lazy<string>(() => "fldd_TwoFactorSetupDate");
    public static string fSMSApplicationUserTwoFactorSetupDate => _fSMSApplicationUserTwoFactorSetupDate.Value;

    private static readonly Lazy<string> _fSMSApplicationUserFailedTwoFactorAttempts = new Lazy<string>(() => "fldi_FailedTwoFactorAttempts");
    public static string fSMSApplicationUserFailedTwoFactorAttempts => _fSMSApplicationUserFailedTwoFactorAttempts.Value;

    private static readonly Lazy<string> _fSMSApplicationUserTwoFactorLockedUntil = new Lazy<string>(() => "fldd_TwoFactorLockedUntil");
    public static string fSMSApplicationUserTwoFactorLockedUntil => _fSMSApplicationUserTwoFactorLockedUntil.Value;

    /// <summary>
    /// SMS Organizational Users table (tbld_SMSOrganizationalUsers)
    /// </summary>
    private static readonly Lazy<string> _fSMSOrganizationalUserCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSOrganizationalUserCode => _fSMSOrganizationalUserCode.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserFirstName = new Lazy<string>(() => "fldv_FirstName");
    public static string fSMSOrganizationalUserFirstName => _fSMSOrganizationalUserFirstName.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserLastName = new Lazy<string>(() => "fldv_LastName");
    public static string fSMSOrganizationalUserLastName => _fSMSOrganizationalUserLastName.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserUserName = new Lazy<string>(() => "fldv_UserName");
    public static string fSMSOrganizationalUserUserName => _fSMSOrganizationalUserUserName.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserPassword = new Lazy<string>(() => "fldv_Password");
    public static string fSMSOrganizationalUserPassword => _fSMSOrganizationalUserPassword.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserDepartment = new Lazy<string>(() => "fldv_Department");
    public static string fSMSOrganizationalUserDepartment => _fSMSOrganizationalUserDepartment.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserPosition = new Lazy<string>(() => "fldv_Position");
    public static string fSMSOrganizationalUserPosition => _fSMSOrganizationalUserPosition.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserOrganizationLevel = new Lazy<string>(() => "fldv_OrganizationLevel");
    public static string fSMSOrganizationalUserOrganizationLevel => _fSMSOrganizationalUserOrganizationLevel.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserSMSRole = new Lazy<string>(() => "fldv_SMSRole");
    public static string fSMSOrganizationalUserSMSRole => _fSMSOrganizationalUserSMSRole.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserAuthorityLevel = new Lazy<string>(() => "fldv_AuthorityLevel");
    public static string fSMSOrganizationalUserAuthorityLevel => _fSMSOrganizationalUserAuthorityLevel.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserRiskApprovalAuthority = new Lazy<string>(() => "fldv_RiskApprovalAuthority");
    public static string fSMSOrganizationalUserRiskApprovalAuthority => _fSMSOrganizationalUserRiskApprovalAuthority.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSOrganizationalUserIsActive => _fSMSOrganizationalUserIsActive.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserLastLoginDate = new Lazy<string>(() => "fldd_LastLoginDate");
    public static string fSMSOrganizationalUserLastLoginDate => _fSMSOrganizationalUserLastLoginDate.Value;

    // 🔐 Two-Factor Authentication Fields for SMS Organizational Users
    private static readonly Lazy<string> _fSMSOrganizationalUserTwoFactorSecretKey = new Lazy<string>(() => "fldv_TwoFactorSecretKey");
    public static string fSMSOrganizationalUserTwoFactorSecretKey => _fSMSOrganizationalUserTwoFactorSecretKey.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserTwoFactorEnabled = new Lazy<string>(() => "fldb_TwoFactorEnabled");
    public static string fSMSOrganizationalUserTwoFactorEnabled => _fSMSOrganizationalUserTwoFactorEnabled.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserBackupCodes = new Lazy<string>(() => "fldv_BackupCodes");
    public static string fSMSOrganizationalUserBackupCodes => _fSMSOrganizationalUserBackupCodes.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserTwoFactorSetupDate = new Lazy<string>(() => "fldd_TwoFactorSetupDate");
    public static string fSMSOrganizationalUserTwoFactorSetupDate => _fSMSOrganizationalUserTwoFactorSetupDate.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserFailedTwoFactorAttempts = new Lazy<string>(() => "fldi_FailedTwoFactorAttempts");
    public static string fSMSOrganizationalUserFailedTwoFactorAttempts => _fSMSOrganizationalUserFailedTwoFactorAttempts.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserTwoFactorLockedUntil = new Lazy<string>(() => "fldd_TwoFactorLockedUntil");
    public static string fSMSOrganizationalUserTwoFactorLockedUntil => _fSMSOrganizationalUserTwoFactorLockedUntil.Value;

    /// <summary>
    /// SMS Stakeholder Users table (tbld_SMSStakeholderUsers)
    /// </summary>
    private static readonly Lazy<string> _fSMSStakeholderUserCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSStakeholderUserCode => _fSMSStakeholderUserCode.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserFirstName = new Lazy<string>(() => "fldv_FirstName");
    public static string fSMSStakeholderUserFirstName => _fSMSStakeholderUserFirstName.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserLastName = new Lazy<string>(() => "fldv_LastName");
    public static string fSMSStakeholderUserLastName => _fSMSStakeholderUserLastName.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserUserName = new Lazy<string>(() => "fldv_UserName");
    public static string fSMSStakeholderUserUserName => _fSMSStakeholderUserUserName.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserPassword = new Lazy<string>(() => "fldv_Password");
    public static string fSMSStakeholderUserPassword => _fSMSStakeholderUserPassword.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserStakeholderTypeCode = new Lazy<string>(() => "fldv_StakeholderTypeCode");
    public static string fSMSStakeholderUserStakeholderTypeCode => _fSMSStakeholderUserStakeholderTypeCode.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserOrganization = new Lazy<string>(() => "fldv_Organization");
    public static string fSMSStakeholderUserOrganization => _fSMSStakeholderUserOrganization.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSStakeholderUserIsActive => _fSMSStakeholderUserIsActive.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserLastLoginDate = new Lazy<string>(() => "fldd_LastLoginDate");
    public static string fSMSStakeholderUserLastLoginDate => _fSMSStakeholderUserLastLoginDate.Value;

    private static readonly Lazy<string> _fSMSStakeholderIsPOPEmployee = new Lazy<string>(() => "fldb_IsPOPEmployee");
    public static string fSMSStakeholderIsPOPEmployee => _fSMSStakeholderIsPOPEmployee.Value;

    // 🔐 Two-Factor Authentication Fields for SMS Stakeholder Users
    private static readonly Lazy<string> _fSMSStakeholderUserTwoFactorSecretKey = new Lazy<string>(() => "fldv_TwoFactorSecretKey");
    public static string fSMSStakeholderUserTwoFactorSecretKey => _fSMSStakeholderUserTwoFactorSecretKey.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserTwoFactorEnabled = new Lazy<string>(() => "fldb_TwoFactorEnabled");
    public static string fSMSStakeholderUserTwoFactorEnabled => _fSMSStakeholderUserTwoFactorEnabled.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserBackupCodes = new Lazy<string>(() => "fldv_BackupCodes");
    public static string fSMSStakeholderUserBackupCodes => _fSMSStakeholderUserBackupCodes.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserTwoFactorSetupDate = new Lazy<string>(() => "fldd_TwoFactorSetupDate");
    public static string fSMSStakeholderUserTwoFactorSetupDate => _fSMSStakeholderUserTwoFactorSetupDate.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserFailedTwoFactorAttempts = new Lazy<string>(() => "fldi_FailedTwoFactorAttempts");
    public static string fSMSStakeholderUserFailedTwoFactorAttempts => _fSMSStakeholderUserFailedTwoFactorAttempts.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserTwoFactorLockedUntil = new Lazy<string>(() => "fldd_TwoFactorLockedUntil");
    public static string fSMSStakeholderUserTwoFactorLockedUntil => _fSMSStakeholderUserTwoFactorLockedUntil.Value;

    /// <summary>
    /// SMS Stakeholder Groups table (tbld_SMSStakeholderGroups)
    /// </summary>
    private static readonly Lazy<string> _fSMSStakeholderGroupCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSStakeholderGroupCode => _fSMSStakeholderGroupCode.Value;

    private static readonly Lazy<string> _fSMSStakeholderGroupName = new Lazy<string>(() => "fldv_GroupName");
    public static string fSMSStakeholderGroupName => _fSMSStakeholderGroupName.Value;

    private static readonly Lazy<string> _fSMSStakeholderGroupDescription = new Lazy<string>(() => "fldv_Description");
    public static string fSMSStakeholderGroupDescription => _fSMSStakeholderGroupDescription.Value;

    private static readonly Lazy<string> _fSMSStakeholderGroupIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSStakeholderGroupIsActive => _fSMSStakeholderGroupIsActive.Value;

    /// <summary>
    /// SMS Application Groups table (tbld_SMSStakeholderGroups)
    /// </summary>
    private static readonly Lazy<string> _fSMSApplicationGroupCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSApplicationGroupCode => _fSMSApplicationGroupCode.Value;

    private static readonly Lazy<string> _fSMSApplicationGroupName = new Lazy<string>(() => "fldv_GroupName");
    public static string fSMSApplicationGroupName => _fSMSApplicationGroupName.Value;

    private static readonly Lazy<string> _fSMSApplicationGroupDescription = new Lazy<string>(() => "fldv_Description");
    public static string fSMSApplicationGroupDescription => _fSMSApplicationGroupDescription.Value;

    private static readonly Lazy<string> _fSMSApplicationGroupIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSApplicationGroupIsActive => _fSMSApplicationGroupIsActive.Value;

    /// <summary>
    /// SMS Organizational Groups table (tbld_SMSOrganizationalGroups)
    /// </summary>
    private static readonly Lazy<string> _fSMSOrganizationalGroupCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSOrganizationalGroupCode => _fSMSOrganizationalGroupCode.Value;

    private static readonly Lazy<string> _fSMSOrganizationalGroupName = new Lazy<string>(() => "fldv_GroupName");
    public static string fSMSOrganizationalGroupName => _fSMSOrganizationalGroupName.Value;

    private static readonly Lazy<string> _fSMSOrganizationalGroupDescription = new Lazy<string>(() => "fldv_Description");
    public static string fSMSOrganizationalGroupDescription => _fSMSOrganizationalGroupDescription.Value;

    private static readonly Lazy<string> _fSMSOrganizationalGroupGroupType = new Lazy<string>(() => "fldv_GroupType");
    public static string fSMSOrganizationalGroupGroupType => _fSMSOrganizationalGroupGroupType.Value;

    private static readonly Lazy<string> _fSMSOrganizationalGroupAuthorityLevel = new Lazy<string>(() => "fldv_AuthorityLevel");
    public static string fSMSOrganizationalGroupAuthorityLevel => _fSMSOrganizationalGroupAuthorityLevel.Value;

    private static readonly Lazy<string> _fSMSOrganizationalGroupIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSOrganizationalGroupIsActive => _fSMSOrganizationalGroupIsActive.Value;

    /// <summary>
    /// SMS Application User Group Memberships table (tbld_SMSApplicationUserGroups) - Junction table
    /// </summary>
    private static readonly Lazy<string> _fSMSApplicationUserGroupUserCode = new Lazy<string>(() => "fldv_UserCode");
    public static string fSMSApplicationUserGroupUserCode => _fSMSApplicationUserGroupUserCode.Value;

    private static readonly Lazy<string> _fSMSApplicationUserGroupGroupCode = new Lazy<string>(() => "fldv_GroupCode");
    public static string fSMSApplicationUserGroupGroupCode => _fSMSApplicationUserGroupGroupCode.Value;

    private static readonly Lazy<string> _fSMSApplicationUserGroupAssignedDate = new Lazy<string>(() => "fldd_AssignedDate");
    public static string fSMSApplicationUserGroupAssignedDate => _fSMSApplicationUserGroupAssignedDate.Value;

    private static readonly Lazy<string> _fSMSApplicationUserGroupAssignedBy = new Lazy<string>(() => "fldv_AssignedBy");
    public static string fSMSApplicationUserGroupAssignedBy => _fSMSApplicationUserGroupAssignedBy.Value;

    /// <summary>
    /// Airport Shared Dataset field names - Using "fldv_", "fldd_", "fldi_" conventions
    /// </summary>
    private static readonly Lazy<string> _fAirportSharedDatasetCode = new Lazy<string>(() => "fldv_Code");
    public static string fAirportSharedDatasetCode => _fAirportSharedDatasetCode.Value;

    private static readonly Lazy<string> _fAirportSharedDatasetReportCode = new Lazy<string>(() => "fldv_ReportCode");
    public static string fAirportSharedDatasetReportCode => _fAirportSharedDatasetReportCode.Value;

    private static readonly Lazy<string> _fAirportSharedDatasetHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fAirportSharedDatasetHazardCode => _fAirportSharedDatasetHazardCode.Value;

    private static readonly Lazy<string> _fPrivateNarrative = new Lazy<string>(() => "fldv_PrivateNarrative");
    public static string fPrivateNarrative => _fPrivateNarrative.Value;

    private static readonly Lazy<string> _fSharedNarrative = new Lazy<string>(() => "fldv_SharedNarrative");
    public static string fSharedNarrative => _fSharedNarrative.Value;

    private static readonly Lazy<string> _fLocationArea = new Lazy<string>(() => "fldv_LocationArea");
    public static string fLocationArea => _fLocationArea.Value;

    private static readonly Lazy<string> _fLocationSubArea = new Lazy<string>(() => "fldv_LocationSubArea");
    public static string fLocationSubArea => _fLocationSubArea.Value;

    private static readonly Lazy<string> _fLocationOther = new Lazy<string>(() => "fldv_LocationOther");
    public static string fLocationOther => _fLocationOther.Value;

    private static readonly Lazy<string> _fWeather = new Lazy<string>(() => "fldv_Weather");
    public static string fWeather => _fWeather.Value;

    private static readonly Lazy<string> _fTriggeringEvent = new Lazy<string>(() => "fldv_TriggeringEvent");
    public static string fTriggeringEvent => _fTriggeringEvent.Value;

    private static readonly Lazy<string> _fAircraftInvolved = new Lazy<string>(() => "fldb_AircraftInvolved");
    public static string fAircraftInvolved => _fAircraftInvolved.Value;

    private static readonly Lazy<string> _fPoweredEquipmentInvolved = new Lazy<string>(() => "fldb_PoweredEquipmentInvolved");
    public static string fPoweredEquipmentInvolved => _fPoweredEquipmentInvolved.Value;

    private static readonly Lazy<string> _fNonPoweredEquipmentInvolved = new Lazy<string>(() => "fldb_NonPoweredEquipmentInvolved");
    public static string fNonPoweredEquipmentInvolved => _fNonPoweredEquipmentInvolved.Value;

    private static readonly Lazy<string> _fPedestrianInvolved = new Lazy<string>(() => "fldb_PedestrianInvolved");
    public static string fPedestrianInvolved => _fPedestrianInvolved.Value;

    private static readonly Lazy<string> _fOtherInvolved = new Lazy<string>(() => "fldb_OtherInvolved");
    public static string fOtherInvolved => _fOtherInvolved.Value;

    private static readonly Lazy<string> _fOtherDescription = new Lazy<string>(() => "fldv_OtherDescription");
    public static string fOtherDescription => _fOtherDescription.Value;

    private static readonly Lazy<string> _fPropertyDamage = new Lazy<string>(() => "fldb_PropertyDamage");
    public static string fPropertyDamage => _fPropertyDamage.Value;

    private static readonly Lazy<string> _fPropertyDamageComments = new Lazy<string>(() => "fldv_PropertyDamageComments");
    public static string fPropertyDamageComments => _fPropertyDamageComments.Value;

    private static readonly Lazy<string> _fPersonalInjury = new Lazy<string>(() => "fldb_PersonalInjury");
    public static string fPersonalInjury => _fPersonalInjury.Value;

    private static readonly Lazy<string> _fPersonalInjuryComments = new Lazy<string>(() => "fldv_PersonalInjuryComments");
    public static string fPersonalInjuryComments => _fPersonalInjuryComments.Value;

    private static readonly Lazy<string> _fFatality = new Lazy<string>(() => "fldb_Fatality");
    public static string fFatality => _fFatality.Value;

    private static readonly Lazy<string> _fFatalityComments = new Lazy<string>(() => "fldv_FatalityComments");
    public static string fFatalityComments => _fFatalityComments.Value;

    private static readonly Lazy<string> _fOtherIssues = new Lazy<string>(() => "fldb_OtherIssues");
    public static string fOtherIssues => _fOtherIssues.Value;

    private static readonly Lazy<string> _fOtherIssuesDescription = new Lazy<string>(() => "fldv_OtherIssuesDescription");
    public static string fOtherIssuesDescription => _fOtherIssuesDescription.Value;

    private static readonly Lazy<string> _fAirlineCompanyOperator = new Lazy<string>(() => "fldv_AirlineCompanyOperator");
    public static string fAirlineCompanyOperator => _fAirlineCompanyOperator.Value;

    private static readonly Lazy<string> _fOperatorsAuthorized = new Lazy<string>(() => "fldv_OperatorsAuthorized");
    public static string fOperatorsAuthorized => _fOperatorsAuthorized.Value;

    private static readonly Lazy<string> _fFlightDelay = new Lazy<string>(() => "fldv_FlightDelay");
    public static string fFlightDelay => _fFlightDelay.Value;

    private static readonly Lazy<string> _fFlightDelayDetails = new Lazy<string>(() => "fldv_FlightDelayDetails");
    public static string fFlightDelayDetails => _fFlightDelayDetails.Value;

    private static readonly Lazy<string> _fEquipmentRemovedFromService = new Lazy<string>(() => "fldv_EquipmentRemovedFromService");
    public static string fEquipmentRemovedFromService => _fEquipmentRemovedFromService.Value;

    private static readonly Lazy<string> _fEquipmentRemovalDetails = new Lazy<string>(() => "fldv_EquipmentRemovalDetails");
    public static string fEquipmentRemovalDetails => _fEquipmentRemovalDetails.Value;

    private static readonly Lazy<string> _fPoliceReport = new Lazy<string>(() => "fldv_PoliceReport");
    public static string fPoliceReport => _fPoliceReport.Value;

    private static readonly Lazy<string> _fPoliceReportDetails = new Lazy<string>(() => "fldv_PoliceReportDetails");
    public static string fPoliceReportDetails => _fPoliceReportDetails.Value;

    private static readonly Lazy<string> _fContributingFactors = new Lazy<string>(() => "fldv_ContributingFactors");
    public static string fContributingFactors => _fContributingFactors.Value;

    private static readonly Lazy<string> _fFactorsOtherDescription = new Lazy<string>(() => "fldv_FactorsOtherDescription");
    public static string fFactorsOtherDescription => _fFactorsOtherDescription.Value;

    /// <summary>
    /// Hazards table (tbld_Hazards)
    /// </summary>
    private static readonly Lazy<string> _fHazardCode = new Lazy<string>(() => "fldv_Code");
    public static string fHazardCode => _fHazardCode.Value;

    private static readonly Lazy<string> _fHazardName = new Lazy<string>(() => "fldv_Name");
    public static string fHazardName => _fHazardName.Value;

    private static readonly Lazy<string> _fHazardDescription = new Lazy<string>(() => "fldv_Description");
    public static string fHazardDescription => _fHazardDescription.Value;

    private static readonly Lazy<string> _fHazardReportCode = new Lazy<string>(() => "fldv_ReportCode");
    public static string fHazardReportCode => _fHazardReportCode.Value;

    private static readonly Lazy<string> _fHazardInitialRiskMatrixCode = new Lazy<string>(() => "fldv_InitialRiskMatrixCode");
    public static string fHazardInitialRiskMatrixCode => _fHazardInitialRiskMatrixCode.Value;

    private static readonly Lazy<string> _fHazardInitialAverageScore = new Lazy<string>(() => "fldv_InitialAverageScore");
    public static string fHazardInitialAverageScore => _fHazardInitialAverageScore.Value;

    private static readonly Lazy<string> _fHazardResidualRiskMatrixCode = new Lazy<string>(() => "fldv_ResidualRiskMatrixCode");
    public static string fHazardResidualRiskMatrixCode => _fHazardResidualRiskMatrixCode.Value;

    private static readonly Lazy<string> _fHazardResidualAverageScore = new Lazy<string>(() => "fldv_ResidualAverageScore");
    public static string fHazardResidualAverageScore => _fHazardResidualAverageScore.Value;









    // Additional Hazard fields for comprehensive SMS support
    private static readonly Lazy<string> _fHazardType = new Lazy<string>(() => "fldv_HazardType");
    public static string fHazardType => _fHazardType.Value;

    private static readonly Lazy<string> _fHazardCategory = new Lazy<string>(() => "fldv_HazardCategory");
    public static string fHazardCategory => _fHazardCategory.Value;

    //fldb_IsInitialHazard

    private static readonly Lazy<string> _fIsInitialHazard = new Lazy<string>(() => "fldb_IsInitialHazard");
    public static string fIsInitialHazard => _fIsInitialHazard.Value;

    private static readonly Lazy<string> _fHazardStatus = new Lazy<string>(() => "fldv_Status");
    public static string fHazardStatus => _fHazardStatus.Value;

    private static readonly Lazy<string> _fHazardRiskLevel = new Lazy<string>(() => "fldv_RiskLevel");
    public static string fHazardRiskLevel => _fHazardRiskLevel.Value;

    private static readonly Lazy<string> _fHazardInitialWorstCredibleOutcome = new Lazy<string>(() => "fldv_InitialWorstCredibleOutcome");
    public static string fHazardInitialWorstCredibleOutcome => _fHazardInitialWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _fHazardInitialRootCause = new Lazy<string>(() => "fldv_InitialRootCause");
    public static string fHazardInitialRootCause => _fHazardInitialRootCause.Value;

    private static readonly Lazy<string> _fHazardCurrentMitigations = new Lazy<string>(() => "fldv_CurrentMitigations");
    public static string fHazardCurrentMitigations => _fHazardCurrentMitigations.Value;

    private static readonly Lazy<string> _fHazardProposedMitigations = new Lazy<string>(() => "fldv_ProposedMitigations");
    public static string fHazardProposedMitigations => _fHazardProposedMitigations.Value;

    private static readonly Lazy<string> _fHazardMitigationTargetDate = new Lazy<string>(() => "fldd_MitigationTargetDate");
    public static string fHazardMitigationTargetDate => _fHazardMitigationTargetDate.Value;

    private static readonly Lazy<string> _fHazardMitigationOwner = new Lazy<string>(() => "fldv_MitigationOwner");
    public static string fHazardMitigationOwner => _fHazardMitigationOwner.Value;

    private static readonly Lazy<string> _fHazardRequiresInvestigation = new Lazy<string>(() => "fldb_RequiresInvestigation");
    public static string fHazardRequiresInvestigation => _fHazardRequiresInvestigation.Value;

    private static readonly Lazy<string> _fHazardInvestigationCompletedDate = new Lazy<string>(() => "fldd_InvestigationCompletedDate");
    public static string fHazardInvestigationCompletedDate => _fHazardInvestigationCompletedDate.Value;

    private static readonly Lazy<string> _fHazardInvestigationNotes = new Lazy<string>(() => "fldv_InvestigationNotes");
    public static string fHazardInvestigationNotes => _fHazardInvestigationNotes.Value;

    private static readonly Lazy<string> _fHazardAdditionalComments = new Lazy<string>(() => "fldv_AdditionalComments");
    public static string fHazardAdditionalComments => _fHazardAdditionalComments.Value;

    private static readonly Lazy<string> _fHazardLocation = new Lazy<string>(() => "fldv_Location");
    public static string fHazardLocation => _fHazardLocation.Value;

    private static readonly Lazy<string> _fHazardLocationArea = new Lazy<string>(() => "fldv_LocationArea");
    public static string fHazardLocationArea => _fHazardLocationArea.Value;

    private static readonly Lazy<string> _fHazardLocationSubArea = new Lazy<string>(() => "fldv_LocationSubArea");
    public static string fHazardLocationSubArea => _fHazardLocationSubArea.Value;

    /// <summary>
    /// Hazard Locations table (tbld_HazardLocations)
    /// </summary>
    private static readonly Lazy<string> _fHazardLocationCode = new Lazy<string>(() => "fldv_Code");
    public static string fHazardLocationCode => _fHazardLocationCode.Value;

    private static readonly Lazy<string> _fHazardLocationHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fHazardLocationHazardCode => _fHazardLocationHazardCode.Value;

    private static readonly Lazy<string> _fHazardLocationLatitude = new Lazy<string>(() => "fldv_Latitude");
    public static string fHazardLocationLatitude => _fHazardLocationLatitude.Value;

    private static readonly Lazy<string> _fHazardLocationLongitude = new Lazy<string>(() => "fldv_Longitude");
    public static string fHazardLocationLongitude => _fHazardLocationLongitude.Value;

    private static readonly Lazy<string> _fHazardLocationDescription = new Lazy<string>(() => "fldv_Description");
    public static string fHazardLocationDescription => _fHazardLocationDescription.Value;

    private static readonly Lazy<string> _fHazardLocationDateSelected = new Lazy<string>(() => "fldd_DateSelected");
    public static string fHazardLocationDateSelected => _fHazardLocationDateSelected.Value;

    private static readonly Lazy<string> _fHazardLocationMapSVG = new Lazy<string>(() => "fldv_LocationMapSVG");
    public static string fHazardLocationMapSVG => _fHazardLocationMapSVG.Value;

    private static readonly Lazy<string> _fHazardLocationName = new Lazy<string>(() => "fldv_LocationName");
    public static string fHazardLocationName => _fHazardLocationName.Value;

    private static readonly Lazy<string> _fHazardLocationAccuracyMeters = new Lazy<string>(() => "fldv_AccuracyMeters");
    public static string fHazardLocationAccuracyMeters => _fHazardLocationAccuracyMeters.Value;

    private static readonly Lazy<string> _fHazardLocationElevationFeet = new Lazy<string>(() => "fldv_ElevationFeet");
    public static string fHazardLocationElevationFeet => _fHazardLocationElevationFeet.Value;

    private static readonly Lazy<string> _fHazardLocationSource = new Lazy<string>(() => "fldv_Source");
    public static string fHazardLocationSource => _fHazardLocationSource.Value;

    private static readonly Lazy<string> _fHazardLocationStatus = new Lazy<string>(() => "fldv_Status");
    public static string fHazardLocationStatus => _fHazardLocationStatus.Value;

    private static readonly Lazy<string> _fHazardLocationIsValidated = new Lazy<string>(() => "fldb_IsValidated");
    public static string fHazardLocationIsValidated => _fHazardLocationIsValidated.Value;

    private static readonly Lazy<string> _fHazardLocationValidatedDate = new Lazy<string>(() => "fldd_ValidatedDate");
    public static string fHazardLocationValidatedDate => _fHazardLocationValidatedDate.Value;

    private static readonly Lazy<string> _fHazardLocationValidatedBy = new Lazy<string>(() => "fldv_ValidatedBy");
    public static string fHazardLocationValidatedBy => _fHazardLocationValidatedBy.Value;

    private static readonly Lazy<string> _fHazardLocationNotes = new Lazy<string>(() => "fldv_Notes");
    public static string fHazardLocationNotes => _fHazardLocationNotes.Value;

    private static readonly Lazy<string> _fHazardLocationTags = new Lazy<string>(() => "fldv_Tags");
    public static string fHazardLocationTags => _fHazardLocationTags.Value;

    private static readonly Lazy<string> _fHazardLocationAirportGrid = new Lazy<string>(() => "fldv_AirportGrid");
    public static string fHazardLocationAirportGrid => _fHazardLocationAirportGrid.Value;

    private static readonly Lazy<string> _fHazardLocationRunwayReference = new Lazy<string>(() => "fldv_RunwayReference");
    public static string fHazardLocationRunwayReference => _fHazardLocationRunwayReference.Value;

    private static readonly Lazy<string> _fHazardLocationTaxiwayReference = new Lazy<string>(() => "fldv_TaxiwayReference");
    public static string fHazardLocationTaxiwayReference => _fHazardLocationTaxiwayReference.Value;

    /// <summary>
    /// Hazard Files table (tbld_HazardFiles)
    /// </summary>
    private static readonly Lazy<string> _fHazardFileID = new Lazy<string>(() => "fldi_ID");
    public static string fHazardFileID => _fHazardFileID.Value;

    private static readonly Lazy<string> _fHazardFileCode = new Lazy<string>(() => "fldv_Code");
    public static string fHazardFileCode => _fHazardFileCode.Value;

    private static readonly Lazy<string> _fHazardFileHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fHazardFileHazardCode => _fHazardFileHazardCode.Value;

    private static readonly Lazy<string> _fHazardFileReportCode = new Lazy<string>(() => "fldv_ReportCode");
    public static string fHazardFileReportCode => _fHazardFileReportCode.Value;

    private static readonly Lazy<string> _fHazardFileFileName = new Lazy<string>(() => "fldv_FileName");
    public static string fHazardFileFileName => _fHazardFileFileName.Value;

    private static readonly Lazy<string> _fHazardFileFileType = new Lazy<string>(() => "fldv_FileType");
    public static string fHazardFileFileType => _fHazardFileFileType.Value;

    private static readonly Lazy<string> _fHazardFileContentType = new Lazy<string>(() => "fldv_ContentType");
    public static string fHazardFileContentType => _fHazardFileContentType.Value;

    private static readonly Lazy<string> _fHazardFileFileSizeBytes = new Lazy<string>(() => "fldi_FileSizeBytes");
    public static string fHazardFileFileSizeBytes => _fHazardFileFileSizeBytes.Value;

    private static readonly Lazy<string> _fHazardFileFileHash = new Lazy<string>(() => "fldv_FileHash");
    public static string fHazardFileFileHash => _fHazardFileFileHash.Value;

    private static readonly Lazy<string> _fHazardFileStorageType = new Lazy<string>(() => "fldv_StorageType");
    public static string fHazardFileStorageType => _fHazardFileStorageType.Value;

    private static readonly Lazy<string> _fHazardFileFilePath = new Lazy<string>(() => "fldv_FilePath");
    public static string fHazardFileFilePath => _fHazardFileFilePath.Value;

    private static readonly Lazy<string> _fHazardFileFileData = new Lazy<string>(() => "fldb_FileData");
    public static string fHazardFileFileData => _fHazardFileFileData.Value;

    private static readonly Lazy<string> _fHazardFileDescription = new Lazy<string>(() => "fldv_Description");
    public static string fHazardFileDescription => _fHazardFileDescription.Value;

    private static readonly Lazy<string> _fHazardFileCategory = new Lazy<string>(() => "fldv_Category");
    public static string fHazardFileCategory => _fHazardFileCategory.Value;

    private static readonly Lazy<string> _fHazardFileIsConfidential = new Lazy<string>(() => "fldb_IsConfidential");
    public static string fHazardFileIsConfidential => _fHazardFileIsConfidential.Value;

    private static readonly Lazy<string> _fHazardFileUploadedBy = new Lazy<string>(() => "fldv_UploadedBy");
    public static string fHazardFileUploadedBy => _fHazardFileUploadedBy.Value;

    private static readonly Lazy<string> _fHazardFileUploadedDate = new Lazy<string>(() => "fldd_UploadedDate");
    public static string fHazardFileUploadedDate => _fHazardFileUploadedDate.Value;

    private static readonly Lazy<string> _fHazardFileTags = new Lazy<string>(() => "fldv_Tags");
    public static string fHazardFileTags => _fHazardFileTags.Value;

    private static readonly Lazy<string> _fHazardFileIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fHazardFileIsActive => _fHazardFileIsActive.Value;

    private static readonly Lazy<string> _fHazardFileInactiveReason = new Lazy<string>(() => "fldv_InactiveReason");
    public static string fHazardFileInactiveReason => _fHazardFileInactiveReason.Value;

    private static readonly Lazy<string> _fHazardFileInactiveDate = new Lazy<string>(() => "fldd_InactiveDate");
    public static string fHazardFileInactiveDate => _fHazardFileInactiveDate.Value;

    private static readonly Lazy<string> _fHazardFileInactiveBy = new Lazy<string>(() => "fldv_InactiveBy");
    public static string fHazardFileInactiveBy => _fHazardFileInactiveBy.Value;


    #region Safety Performance Indicator Field Names

    /// <summary>
    /// Safety Performance Indicator table field names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _fSPIId = new Lazy<string>(() => "fldi_ID");
    public static string fSPIId => _fSPIId.Value;

    private static readonly Lazy<string> _fSPICode = new Lazy<string>(() => "fldv_Code");
    public static string fSPICode => _fSPICode.Value;

    private static readonly Lazy<string> _fSPIName = new Lazy<string>(() => "fldv_Name");
    public static string fSPIName => _fSPIName.Value;

    private static readonly Lazy<string> _fSPIDescription = new Lazy<string>(() => "fldv_Description");
    public static string fSPIDescription => _fSPIDescription.Value;

    private static readonly Lazy<string> _fSPIIndicatorType = new Lazy<string>(() => "fldv_IndicatorType");
    public static string fSPIIndicatorType => _fSPIIndicatorType.Value;

    private static readonly Lazy<string> _fSPIStatus = new Lazy<string>(() => "fldv_Status");
    public static string fSPIStatus => _fSPIStatus.Value;

    private static readonly Lazy<string> _fSPIMeasurementUnit = new Lazy<string>(() => "fldv_MeasurementUnit");
    public static string fSPIMeasurementUnit => _fSPIMeasurementUnit.Value;

    private static readonly Lazy<string> _fSPIMeasurementFrequency = new Lazy<string>(() => "fldv_MeasurementFrequency");
    public static string fSPIMeasurementFrequency => _fSPIMeasurementFrequency.Value;

    private static readonly Lazy<string> _fSPICalculationMethod = new Lazy<string>(() => "fldv_CalculationMethod");
    public static string fSPICalculationMethod => _fSPICalculationMethod.Value;

    private static readonly Lazy<string> _fSPIDataSource = new Lazy<string>(() => "fldv_DataSource");
    public static string fSPIDataSource => _fSPIDataSource.Value;

    private static readonly Lazy<string> _fSPITargetValue = new Lazy<string>(() => "fldm_TargetValue");
    public static string fSPITargetValue => _fSPITargetValue.Value;

    private static readonly Lazy<string> _fSPIAcceptableRange = new Lazy<string>(() => "fldm_AcceptableRange");
    public static string fSPIAcceptableRange => _fSPIAcceptableRange.Value;

    private static readonly Lazy<string> _fSPIWarningThreshold = new Lazy<string>(() => "fldm_WarningThreshold");
    public static string fSPIWarningThreshold => _fSPIWarningThreshold.Value;

    private static readonly Lazy<string> _fSPICriticalThreshold = new Lazy<string>(() => "fldm_CriticalThreshold");
    public static string fSPICriticalThreshold => _fSPICriticalThreshold.Value;

    private static readonly Lazy<string> _fSPIResponsibleDepartment = new Lazy<string>(() => "fldv_ResponsibleDepartment");
    public static string fSPIResponsibleDepartment => _fSPIResponsibleDepartment.Value;

    private static readonly Lazy<string> _fSPIDataOwner = new Lazy<string>(() => "fldv_DataOwner");
    public static string fSPIDataOwner => _fSPIDataOwner.Value;

    private static readonly Lazy<string> _fSPIReviewAuthority = new Lazy<string>(() => "fldv_ReviewAuthority");
    public static string fSPIReviewAuthority => _fSPIReviewAuthority.Value;

    private static readonly Lazy<string> _fSPINextReviewDate = new Lazy<string>(() => "fldd_NextReviewDate");
    public static string fSPINextReviewDate => _fSPINextReviewDate.Value;

    private static readonly Lazy<string> _fSPILastReviewDate = new Lazy<string>(() => "fldd_LastReviewDate");
    public static string fSPILastReviewDate => _fSPILastReviewDate.Value;

    private static readonly Lazy<string> _fSPILastReviewNotes = new Lazy<string>(() => "fldv_LastReviewNotes");
    public static string fSPILastReviewNotes => _fSPILastReviewNotes.Value;

    private static readonly Lazy<string> _fSPIAlertsEnabled = new Lazy<string>(() => "fldb_AlertsEnabled");
    public static string fSPIAlertsEnabled => _fSPIAlertsEnabled.Value;

    private static readonly Lazy<string> _fSPIAlertRecipients = new Lazy<string>(() => "fldv_AlertRecipients");
    public static string fSPIAlertRecipients => _fSPIAlertRecipients.Value;

    /// <summary>
    /// SPI Data Point table field names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _fSPIDataPointCode = new Lazy<string>(() => "fldv_Code");
    public static string fSPIDataPointCode => _fSPIDataPointCode.Value;

    private static readonly Lazy<string> _fSPIDataPointSPIId = new Lazy<string>(() => "fldv_SPICode");
    public static string fSPIDataPointSPIId => _fSPIDataPointSPIId.Value;

    private static readonly Lazy<string> _fSPIDataPointValue = new Lazy<string>(() => "fldm_Value");
    public static string fSPIDataPointValue => _fSPIDataPointValue.Value;

    private static readonly Lazy<string> _fSPIDataPointMeasurementDate = new Lazy<string>(() => "fldd_MeasurementDate");
    public static string fSPIDataPointMeasurementDate => _fSPIDataPointMeasurementDate.Value;

    private static readonly Lazy<string> _fSPIDataPointPeriod = new Lazy<string>(() => "fldv_Period");
    public static string fSPIDataPointPeriod => _fSPIDataPointPeriod.Value;

    private static readonly Lazy<string> _fSPIDataPointDataSource = new Lazy<string>(() => "fldv_DataSource");
    public static string fSPIDataPointDataSource => _fSPIDataPointDataSource.Value;

    private static readonly Lazy<string> _fSPIDataPointCreatedBy = new Lazy<string>(() => "fldv_CreatedBy");
    public static string fSPIDataPointCreatedBy => _fSPIDataPointCreatedBy.Value;

    private static readonly Lazy<string> _fSPIDataPointCreatedDate = new Lazy<string>(() => "fldd_CreatedDate");
    public static string fSPIDataPointCreatedDate => _fSPIDataPointCreatedDate.Value;

    private static readonly Lazy<string> _fSPIDataPointNotes = new Lazy<string>(() => "fldv_Notes");
    public static string fSPIDataPointNotes => _fSPIDataPointNotes.Value;

    private static readonly Lazy<string> _fSPIDataPointIsVerified = new Lazy<string>(() => "fldb_IsVerified");
    public static string fSPIDataPointIsVerified => _fSPIDataPointIsVerified.Value;

    private static readonly Lazy<string> _fSPIDataPointVerifiedBy = new Lazy<string>(() => "fldv_VerifiedBy");
    public static string fSPIDataPointVerifiedBy => _fSPIDataPointVerifiedBy.Value;

    private static readonly Lazy<string> _fSPIDataPointVerifiedDate = new Lazy<string>(() => "fldd_VerifiedDate");
    public static string fSPIDataPointVerifiedDate => _fSPIDataPointVerifiedDate.Value;

    #endregion

    #region Report Field Names

    /// <summary>
    /// Report field names
    /// </summary>
    private static readonly Lazy<string> _fReportCode = new Lazy<string>(() => "fldv_Code");
    public static string fReportCode => _fReportCode.Value;

    private static readonly Lazy<string> _fReportName = new Lazy<string>(() => "fldv_Name");
    public static string fReportName => _fReportName.Value;

    private static readonly Lazy<string> _fReportDescription = new Lazy<string>(() => "fldv_Description");
    public static string fReportDescription => _fReportDescription.Value;

    private static readonly Lazy<string> _fReportStatus = new Lazy<string>(() => "fldv_Status");
    public static string fReportStatus => _fReportStatus.Value;

    private static readonly Lazy<string> _fReportStage = new Lazy<string>(() => "fldv_Stage");
    public static string fReportStage => _fReportStage.Value;

    private static readonly Lazy<string> _fSubmittedBy = new Lazy<string>(() => "fldv_SubmittedBy");
    public static string fSubmittedBy => _fSubmittedBy.Value;

    private static readonly Lazy<string> _fSubmittedDate = new Lazy<string>(() => "fldd_SubmittedDate");
    public static string fSubmittedDate => _fSubmittedDate.Value;

    private static readonly Lazy<string> _fSubmittingDepartment = new Lazy<string>(() => "fldv_SubmittingDepartment");
    public static string fSubmittingDepartment => _fSubmittingDepartment.Value;

    private static readonly Lazy<string> _fSubmittingDepartmentJobFunction = new Lazy<string>(() => "fldv_SubmittingDepartmentJobFunction");
    public static string fSubmittingDepartmentJobFunction => _fSubmittingDepartmentJobFunction.Value;


    private static readonly Lazy<string> _fReportContactName = new Lazy<string>(() => "fldv_ReportContactName");
    public static string fReportContactName => _fReportContactName.Value;

    private static readonly Lazy<string> _fReportContactCell = new Lazy<string>(() => "fldv_ReportContactCell");
    public static string fReportContactCell => _fReportContactCell.Value;

    private static readonly Lazy<string> _fReportContactEmail = new Lazy<string>(() => "fldv_ReportContactEmail");
    public static string fReportContactEmail => _fReportContactEmail.Value;

    private static readonly Lazy<string> _fReportContactCompany = new Lazy<string>(() => "fldv_ReportContactCompany");
    public static string fReportContactCompany => _fReportContactCompany.Value;

    private static readonly Lazy<string> _fReportIncidentDateTime = new Lazy<string>(() => "fldd_IncidentDateTime");
    public static string fReportIncidentDateTime => _fReportIncidentDateTime.Value;

    






    #endregion

    #region Investigation Field Names

    /// <summary>
    /// Investigation field names
    /// </summary>
    private static readonly Lazy<string> _fInvestigationCode = new Lazy<string>(() => "fldv_Code");
    public static string fInvestigationCode => _fInvestigationCode.Value;

    private static readonly Lazy<string> _fInvestigationReportCode = new Lazy<string>(() => "fldv_ReportCode");
    public static string fInvestigationReportCode => _fInvestigationReportCode.Value;

    private static readonly Lazy<string> _fInvestigationHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fInvestigationHazardCode => _fInvestigationHazardCode.Value;

    private static readonly Lazy<string> _fInvestigationNotes = new Lazy<string>(() => "fldv_InvestigationNotes");
    public static string fInvestigationNotes => _fInvestigationNotes.Value;

    private static readonly Lazy<string> _fInvestigationAssignedInvestigatorId = new Lazy<string>(() => "fldv_AssignedInvestigatorId");
    public static string fInvestigationAssignedInvestigatorId => _fInvestigationAssignedInvestigatorId.Value;

    private static readonly Lazy<string> _fInvestigationStatus = new Lazy<string>(() => "fldv_Status");
    public static string fInvestigationStatus => _fInvestigationStatus.Value;

    private static readonly Lazy<string> _fInvestigationCompletedDate = new Lazy<string>(() => "fldd_CompletedDate");
    public static string fInvestigationCompletedDate => _fInvestigationCompletedDate.Value;

    private static readonly Lazy<string> _fInvestigationPlan = new Lazy<string>(() => "fldv_InvestigationPlan");
    public static string fInvestigationPlan => _fInvestigationPlan.Value;

    private static readonly Lazy<string> _fInvestigationObjectives = new Lazy<string>(() => "fldv_InvestigationObjectives");
    public static string fInvestigationObjectives => _fInvestigationObjectives.Value;

    private static readonly Lazy<string> _fInvestigationDecisionType = new Lazy<string>(() => "fldv_DecisionType");
    public static string fInvestigationDecisionType => _fInvestigationDecisionType.Value;

    private static readonly Lazy<string> _fInvestigationDecisionRationale = new Lazy<string>(() => "fldv_DecisionRationale");
    public static string fInvestigationDecisionRationale => _fInvestigationDecisionRationale.Value;

    private static readonly Lazy<string> _fInvestigationDecisionMaker = new Lazy<string>(() => "fldv_DecisionMaker");
    public static string fInvestigationDecisionMaker => _fInvestigationDecisionMaker.Value;

    private static readonly Lazy<string> _fInvestigationDecisionDate = new Lazy<string>(() => "fldd_DecisionDate");
    public static string fInvestigationDecisionDate => _fInvestigationDecisionDate.Value;

    private static readonly Lazy<string> _fInvestigationNextSteps = new Lazy<string>(() => "fldv_NextSteps");
    public static string fInvestigationNextSteps => _fInvestigationNextSteps.Value;

    private static readonly Lazy<string> _fInvestigationReferralDetails = new Lazy<string>(() => "fldv_ReferralDetails");
    public static string fInvestigationReferralDetails => _fInvestigationReferralDetails.Value;

    #endregion

    #region Interview Field Names

    /// <summary>
    /// Interview field names
    /// </summary>
    private static readonly Lazy<string> _fInterviewCode = new Lazy<string>(() => "fldv_Code");
    public static string fInterviewCode => _fInterviewCode.Value;

    private static readonly Lazy<string> _fInterviewInvestigationCode = new Lazy<string>(() => "fldv_InvestigationCode");
    public static string fInterviewInvestigationCode => _fInterviewInvestigationCode.Value;

    private static readonly Lazy<string> _fInterviewSMSInvestigatorCode = new Lazy<string>(() => "fldv_SMSInvestigatorCode");
    public static string fInterviewSMSInvestigatorCode => _fInterviewSMSInvestigatorCode.Value;

    private static readonly Lazy<string> _fInterviewPersonInterviewed = new Lazy<string>(() => "fldv_PersonInterviewed");
    public static string fInterviewPersonInterviewed => _fInterviewPersonInterviewed.Value;

    private static readonly Lazy<string> _fInterviewPersonRole = new Lazy<string>(() => "fldv_PersonInterviewedRole");
    public static string fInterviewPersonRole => _fInterviewPersonRole.Value;

    private static readonly Lazy<string> _fInterviewPersonDepartment = new Lazy<string>(() => "fldv_PersonInterviewedDepartment");
    public static string fInterviewPersonDepartment => _fInterviewPersonDepartment.Value;

    private static readonly Lazy<string> _fInterviewPersonInterviewedNotes = new Lazy<string>(() => "fldv_PersonInterviewedNotes");
    public static string fInterviewPersonInterviewedNotes => _fInterviewPersonInterviewedNotes.Value;

    private static readonly Lazy<string> _fInterviewInvestigatorNotes = new Lazy<string>(() => "fldv_InvestigatorNotes");
    public static string fInterviewInvestigatorNotes => _fInterviewInvestigatorNotes.Value;

    private static readonly Lazy<string> _fInterviewStatus = new Lazy<string>(() => "fldv_Status");
    public static string fInterviewStatus => _fInterviewStatus.Value;

    private static readonly Lazy<string> _fInterviewDate = new Lazy<string>(() => "fldd_InterviewDate");
    public static string fInterviewDate => _fInterviewDate.Value;

    private static readonly Lazy<string> _fInterviewDurationMinutes = new Lazy<string>(() => "fldi_DurationMinutes");
    public static string fInterviewDurationMinutes => _fInterviewDurationMinutes.Value;

    private static readonly Lazy<string> _fInterviewLocation = new Lazy<string>(() => "fldv_InterviewLocation");
    public static string fInterviewLocation => _fInterviewLocation.Value;

    private static readonly Lazy<string> _fInterviewType = new Lazy<string>(() => "fldv_Type");
    public static string fInterviewType => _fInterviewType.Value;

    private static readonly Lazy<string> _fInterviewIsConfidential = new Lazy<string>(() => "fldb_IsConfidential");
    public static string fInterviewIsConfidential => _fInterviewIsConfidential.Value;

    private static readonly Lazy<string> _fInterviewPreparationNotes = new Lazy<string>(() => "fldv_PreparationNotes");
    public static string fInterviewPreparationNotes => _fInterviewPreparationNotes.Value;

    private static readonly Lazy<string> _fInterviewQuestionsToAsk = new Lazy<string>(() => "fldv_QuestionsToAsk");
    public static string fInterviewQuestionsToAsk => _fInterviewQuestionsToAsk.Value;

    private static readonly Lazy<string> _fInterviewBackgroundInformation = new Lazy<string>(() => "fldv_BackgroundInformation");
    public static string fInterviewBackgroundInformation => _fInterviewBackgroundInformation.Value;

    private static readonly Lazy<string> _fInterviewKeyFindings = new Lazy<string>(() => "fldv_KeyFindings");
    public static string fInterviewKeyFindings => _fInterviewKeyFindings.Value;

    private static readonly Lazy<string> _fInterviewFollowUpRequired = new Lazy<string>(() => "fldv_FollowUpRequired");
    public static string fInterviewFollowUpRequired => _fInterviewFollowUpRequired.Value;

    private static readonly Lazy<string> _fInterviewAdditionalWitnesses = new Lazy<string>(() => "fldv_AdditionalWitnesses");
    public static string fInterviewAdditionalWitnesses => _fInterviewAdditionalWitnesses.Value;

    private static readonly Lazy<string> _fInterviewCompletedDate = new Lazy<string>(() => "fldd_CompletedDate");
    public static string fInterviewCompletedDate => _fInterviewCompletedDate.Value;

    #endregion

    #region Risk Analysis Field Names

    /// <summary>
    /// Risk Analysis field names
    /// </summary>
    private static readonly Lazy<string> _fRiskAnalysisCode = new Lazy<string>(() => "fldv_Code");
    public static string fRiskAnalysisCode => _fRiskAnalysisCode.Value;

    private static readonly Lazy<string> _fRiskAnalysisType = new Lazy<string>(() => "fldv_AssessmentType");
    public static string fRiskAnalysisType => _fRiskAnalysisType.Value;

    private static readonly Lazy<string> _fRiskAnalysisHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fRiskAnalysisHazardCode => _fRiskAnalysisHazardCode.Value;

    private static readonly Lazy<string> _fRiskAnalysisRiskAssessmentCode = new Lazy<string>(() => "fldv_RiskAssessmentCode");
    public static string fRiskAnalysisRiskAssessmentCode => _fRiskAnalysisRiskAssessmentCode.Value;

    private static readonly Lazy<string> _fRiskAnalysisInitialWorstCredibleOutcome = new Lazy<string>(() => "fldv_InitialWorstCredibleOutcome");
    public static string fRiskAnalysisInitialWorstCredibleOutcome => _fRiskAnalysisInitialWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _fRiskAnalysisInitialRootCause = new Lazy<string>(() => "fldv_InitialRootCause");
    public static string fRiskAnalysisInitialRootCause => _fRiskAnalysisInitialRootCause.Value;

    private static readonly Lazy<string> _fRiskAnalysisInitialAdditionalComments = new Lazy<string>(() => "fldv_InitialAdditionalComments");
    public static string fRiskAnalysisInitialAdditionalComments => _fRiskAnalysisInitialAdditionalComments.Value;


    private static readonly Lazy<string> _fRiskAnalysisResidualWorstCredibleOutcome = new Lazy<string>(() => "fldv_ResidualWorstCredibleOutcome");
    public static string fRiskAnalysisResidualWorstCredibleOutcome => _fRiskAnalysisResidualWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _fRiskAnalysisResidualRootCause = new Lazy<string>(() => "fldv_ResidualRootCause");
    public static string fRiskAnalysisResidualRootCause => _fRiskAnalysisResidualRootCause.Value;

    private static readonly Lazy<string> _fRiskAnalysisResidualAdditionalComments = new Lazy<string>(() => "fldv_ResidualAdditionalComments");
    public static string fRiskAnalysisResidualAdditionalComments => _fRiskAnalysisResidualAdditionalComments.Value;




    #endregion

    #region Risk Assessment Field Names

    /// <summary>
    /// Risk Assessment field names
    /// </summary>
    private static readonly Lazy<string> _fRiskAssessmentCode = new Lazy<string>(() => "fldv_Code");
    public static string fRiskAssessmentCode => _fRiskAssessmentCode.Value;

    private static readonly Lazy<string> _fRiskAssessmentName = new Lazy<string>(() => "fldv_Name");
    public static string fRiskAssessmentName => _fRiskAssessmentName.Value;

    private static readonly Lazy<string> _fRiskAssessmentDescription = new Lazy<string>(() => "fldv_Description");
    public static string fRiskAssessmentDescription => _fRiskAssessmentDescription.Value;

    private static readonly Lazy<string> _fRiskAssessmentHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fRiskAssessmentHazardCode => _fRiskAssessmentHazardCode.Value;

    private static readonly Lazy<string> _fRiskAssessmentReportCode = new Lazy<string>(() => "fldv_ReportCode");
    public static string fRiskAssessmentReportCode => _fRiskAssessmentReportCode.Value;





    private static readonly Lazy<string> _fRiskAssessmentType = new Lazy<string>(() => "fldv_AssessmentType");
    public static string fRiskAssessmentType => _fRiskAssessmentType.Value;

    private static readonly Lazy<string> _fRiskAssessmentStatus = new Lazy<string>(() => "fldv_Status");
    public static string fRiskAssessmentStatus => _fRiskAssessmentStatus.Value;

    private static readonly Lazy<string> _fRiskAssessmentStage = new Lazy<string>(() => "fldv_Stage");
    public static string fRiskAssessmentStage => _fRiskAssessmentStage.Value;

    private static readonly Lazy<string> _fRiskAssessmentLeadAssessorId = new Lazy<string>(() => "fldv_LeadAssessorId");
    public static string fRiskAssessmentLeadAssessorId => _fRiskAssessmentLeadAssessorId.Value;

    private static readonly Lazy<string> _fRiskAssessmentPrimaryHazardId = new Lazy<string>(() => "fldv_PrimaryHazardId");
    public static string fRiskAssessmentPrimaryHazardId => _fRiskAssessmentPrimaryHazardId.Value;

    private static readonly Lazy<string> _fRiskAssessmentCategory = new Lazy<string>(() => "fldv_RiskAssessmentCategory");
    public static string fRiskAssessmentCategory => _fRiskAssessmentCategory.Value;

    private static readonly Lazy<string> _fRiskAssessmentCurrentStep = new Lazy<string>(() => "fldi_CurrentStep");
    public static string fRiskAssessmentCurrentStep => _fRiskAssessmentCurrentStep.Value;

    private static readonly Lazy<string> _fRiskAssessmentCompletedDate = new Lazy<string>(() => "fldd_CompletedDate");
    public static string fRiskAssessmentCompletedDate => _fRiskAssessmentCompletedDate.Value;

    private static readonly Lazy<string> _fRiskAssessmentCompletedBy = new Lazy<string>(() => "fldv_CompletedBy");
    public static string fRiskAssessmentCompletedBy => _fRiskAssessmentCompletedBy.Value;

    private static readonly Lazy<string> _fRiskAssessmentParentAssessmentId = new Lazy<string>(() => "fldv_ParentAssessmentId");
    public static string fRiskAssessmentParentAssessmentId => _fRiskAssessmentParentAssessmentId.Value;

    // Step 1 fields
    private static readonly Lazy<string> _fRiskAssessmentSystemDescription = new Lazy<string>(() => "fldv_SystemDescription");
    public static string fRiskAssessmentSystemDescription => _fRiskAssessmentSystemDescription.Value;

    private static readonly Lazy<string> _fRiskAssessmentSystemBoundaries = new Lazy<string>(() => "fldv_SystemBoundaries");
    public static string fRiskAssessmentSystemBoundaries => _fRiskAssessmentSystemBoundaries.Value;

    private static readonly Lazy<string> _fRiskAssessmentSystemPurpose = new Lazy<string>(() => "fldv_SystemPurpose");
    public static string fRiskAssessmentSystemPurpose => _fRiskAssessmentSystemPurpose.Value;

    private static readonly Lazy<string> _fRiskAssessmentFiveMPersonnel = new Lazy<string>(() => "fldv_PersonnelFactors");
    public static string fRiskAssessmentFiveMPersonnel => _fRiskAssessmentFiveMPersonnel.Value;

    private static readonly Lazy<string> _fRiskAssessmentFiveMEquipment = new Lazy<string>(() => "fldv_EquipmentFactors");
    public static string fRiskAssessmentFiveMEquipment => _fRiskAssessmentFiveMEquipment.Value;

    private static readonly Lazy<string> _fRiskAssessmentFiveMProcedures = new Lazy<string>(() => "fldv_ProcedureFactors");
    public static string fRiskAssessmentFiveMProcedures => _fRiskAssessmentFiveMProcedures.Value;

    private static readonly Lazy<string> _fRiskAssessmentFiveMResources = new Lazy<string>(() => "fldv_ResourceFactors");
    public static string fRiskAssessmentFiveMResources => _fRiskAssessmentFiveMResources.Value;

    private static readonly Lazy<string> _fRiskAssessmentFiveMPhysicalEnvironment = new Lazy<string>(() => "fldv_EnvironmentFactors");
    public static string fRiskAssessmentFiveMPhysicalEnvironment => _fRiskAssessmentFiveMPhysicalEnvironment.Value;

    private static readonly Lazy<string> _fRiskAssessmentFiveMOperationalEnvironment = new Lazy<string>(() => "fldv_FiveMOperationalEnvironment");
    public static string fRiskAssessmentFiveMOperationalEnvironment => _fRiskAssessmentFiveMOperationalEnvironment.Value;

    private static readonly Lazy<string> _fRiskAssessmentSelectedIndividualStakeholders = new Lazy<string>(() => "fldv_SelectedIndividualStakeholders");
    public static string fRiskAssessmentSelectedIndividualStakeholders => _fRiskAssessmentSelectedIndividualStakeholders.Value;

    private static readonly Lazy<string> _fRiskAssessmentSelectedStakeholderGroups = new Lazy<string>(() => "fldv_SelectedStakeholderGroups");
    public static string fRiskAssessmentSelectedStakeholderGroups => _fRiskAssessmentSelectedStakeholderGroups.Value;

    // Step 3 fields

    // Step 4 fields

    private static readonly Lazy<string> _fRiskAssessmentFinalSeverityScore = new Lazy<string>(() => "fldi_FinalSeverityScore");
    public static string fRiskAssessmentFinalSeverityScore => _fRiskAssessmentFinalSeverityScore.Value;

    private static readonly Lazy<string> _fRiskAssessmentFinalLikelihoodScore = new Lazy<string>(() => "fldi_FinalLikelihoodScore");
    public static string fRiskAssessmentFinalLikelihoodScore => _fRiskAssessmentFinalLikelihoodScore.Value;

    private static readonly Lazy<string> _fRiskAssessmentFinalRiskLevel = new Lazy<string>(() => "fldv_FinalRiskLevel");
    public static string fRiskAssessmentFinalRiskLevel => _fRiskAssessmentFinalRiskLevel.Value;

    private static readonly Lazy<string> _fRiskAssessmentAdditionalComments = new Lazy<string>(() => "fldv_AdditionalComments");
    public static string fRiskAssessmentAdditionalComments => _fRiskAssessmentAdditionalComments.Value;

    // Step 5 fields


    #endregion

    #region Mitigation Field Names

    /// <summary>
    /// Mitigation field names
    /// </summary>
    private static readonly Lazy<string> _fMitigationCode = new Lazy<string>(() => "fldv_Code");
    public static string fMitigationCode => _fMitigationCode.Value;

    private static readonly Lazy<string> _fMitigationHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fMitigationHazardCode => _fMitigationHazardCode.Value;

    private static readonly Lazy<string> _fMitigationName = new Lazy<string>(() => "fldv_Name");
    public static string fMitigationName => _fMitigationName.Value;

    private static readonly Lazy<string> _fMitigationDescription = new Lazy<string>(() => "fldv_Description");
    public static string fMitigationDescription => _fMitigationDescription.Value;

    private static readonly Lazy<string> _fMitigationType = new Lazy<string>(() => "fldv_Type");
    public static string fMitigationType => _fMitigationType.Value;

    private static readonly Lazy<string> _fMitigationStatus = new Lazy<string>(() => "fldv_Status");
    public static string fMitigationStatus => _fMitigationStatus.Value;

    private static readonly Lazy<string> _fMitigationPriority = new Lazy<string>(() => "fldv_Priority");
    public static string fMitigationPriority => _fMitigationPriority.Value;

    private static readonly Lazy<string> _fMitigationRiskAssessmentCode = new Lazy<string>(() => "fldv_RiskAssessmentCode");
    public static string fMitigationRiskAssessmentCode => _fMitigationRiskAssessmentCode.Value;

    private static readonly Lazy<string> _fMitigationTargetDate = new Lazy<string>(() => "fldd_TargetDate");
    public static string fMitigationTargetDate => _fMitigationTargetDate.Value;

    
    private static readonly Lazy<string> _fMitigationCompletionDate = new Lazy<string>(() => "fldd_CompletionDate");
    public static string fMitigationCompletionDate => _fMitigationCompletionDate.Value;

    private static readonly Lazy<string> _fMitigationAssignedDepartment = new Lazy<string>(() => "fldv_AssignedDepartment");
    public static string fMitigationAssignedDepartment => _fMitigationAssignedDepartment.Value;

    private static readonly Lazy<string> _fMitigationAssignedTo = new Lazy<string>(() => "fldv_AssignedTo");
    public static string fMitigationAssignedTo => _fMitigationAssignedTo.Value;

    private static readonly Lazy<string> _fMitigationApprovedBy = new Lazy<string>(() => "fldv_ApprovedBy");
    public static string fMitigationApprovedBy => _fMitigationApprovedBy.Value;

    private static readonly Lazy<string> _fMitigationApprovedDate = new Lazy<string>(() => "fldd_ApprovedDate");
    public static string fMitigationApprovedDate => _fMitigationApprovedDate.Value;

    private static readonly Lazy<string> _fMitigationProgress = new Lazy<string>(() => "fldi_Progress");
    public static string fMitigationProgress => _fMitigationProgress.Value;

    private static readonly Lazy<string> _fMitigationProgressNotes = new Lazy<string>(() => "fldv_ProgressNotes");
    public static string fMitigationProgressNotes => _fMitigationProgressNotes.Value;

    private static readonly Lazy<string> _fMitigationLastProgressUpdate = new Lazy<string>(() => "fldd_LastProgressUpdate");
    public static string fMitigationLastProgressUpdate => _fMitigationLastProgressUpdate.Value;

    private static readonly Lazy<string> _fMitigationProgressUpdatedBy = new Lazy<string>(() => "fldv_ProgressUpdatedBy");
    public static string fMitigationProgressUpdatedBy => _fMitigationProgressUpdatedBy.Value;

    private static readonly Lazy<string> _fMitigationEstimatedCost = new Lazy<string>(() => "fldd_EstimatedCost");
    public static string fMitigationEstimatedCost => _fMitigationEstimatedCost.Value;

    private static readonly Lazy<string> _fMitigationActualCost = new Lazy<string>(() => "fldd_ActualCost");
    public static string fMitigationActualCost => _fMitigationActualCost.Value;

    private static readonly Lazy<string> _fMitigationResourceRequirements = new Lazy<string>(() => "fldv_ResourceRequirements");
    public static string fMitigationResourceRequirements => _fMitigationResourceRequirements.Value;

    private static readonly Lazy<string> _fMitigationEstimatedHours = new Lazy<string>(() => "fldi_EstimatedHours");
    public static string fMitigationEstimatedHours => _fMitigationEstimatedHours.Value;

    private static readonly Lazy<string> _fMitigationActualHours = new Lazy<string>(() => "fldi_ActualHours");
    public static string fMitigationActualHours => _fMitigationActualHours.Value;

    private static readonly Lazy<string> _fMitigationEffectivenessRating = new Lazy<string>(() => "fldv_EffectivenessRating");
    public static string fMitigationEffectivenessRating => _fMitigationEffectivenessRating.Value;

    private static readonly Lazy<string> _fMitigationEffectivenessNotes = new Lazy<string>(() => "fldv_EffectivenessNotes");
    public static string fMitigationEffectivenessNotes => _fMitigationEffectivenessNotes.Value;

    private static readonly Lazy<string> _fMitigationEffectivenessReviewDate = new Lazy<string>(() => "fldd_EffectivenessReviewDate");
    public static string fMitigationEffectivenessReviewDate => _fMitigationEffectivenessReviewDate.Value;

    private static readonly Lazy<string> _fMitigationEffectivenessReviewedBy = new Lazy<string>(() => "fldv_EffectivenessReviewedBy");
    public static string fMitigationEffectivenessReviewedBy => _fMitigationEffectivenessReviewedBy.Value;

    private static readonly Lazy<string> _fMitigationMonitoringRequirements = new Lazy<string>(() => "fldv_MonitoringRequirements");
    public static string fMitigationMonitoringRequirements => _fMitigationMonitoringRequirements.Value;

    private static readonly Lazy<string> _fMitigationMonitoringFrequency = new Lazy<string>(() => "fldv_MonitoringFrequency");
    public static string fMitigationMonitoringFrequency => _fMitigationMonitoringFrequency.Value;

    private static readonly Lazy<string> _fMitigationExpectedSeverityReduction = new Lazy<string>(() => "fldi_ExpectedSeverityReduction");
    public static string fMitigationExpectedSeverityReduction => _fMitigationExpectedSeverityReduction.Value;

    private static readonly Lazy<string> _fMitigationExpectedLikelihoodReduction = new Lazy<string>(() => "fldi_ExpectedLikelihoodReduction");
    public static string fMitigationExpectedLikelihoodReduction => _fMitigationExpectedLikelihoodReduction.Value;

    private static readonly Lazy<string> _fMitigationActualSeverityReduction = new Lazy<string>(() => "fldi_ActualSeverityReduction");
    public static string fMitigationActualSeverityReduction => _fMitigationActualSeverityReduction.Value;

    private static readonly Lazy<string> _fMitigationActualLikelihoodReduction = new Lazy<string>(() => "fldi_ActualLikelihoodReduction");
    public static string fMitigationActualLikelihoodReduction => _fMitigationActualLikelihoodReduction.Value;

    private static readonly Lazy<string> _fMitigationResidualRiskLevel = new Lazy<string>(() => "fldv_ResidualRiskLevel");
    public static string fMitigationResidualRiskLevel => _fMitigationResidualRiskLevel.Value;

    private static readonly Lazy<string> _fMitigationPrerequisites = new Lazy<string>(() => "fldv_Prerequisites");
    public static string fMitigationPrerequisites => _fMitigationPrerequisites.Value;

    private static readonly Lazy<string> _fMitigationDependencies = new Lazy<string>(() => "fldv_Dependencies");
    public static string fMitigationDependencies => _fMitigationDependencies.Value;

    private static readonly Lazy<string> _fMitigationHasDependencies = new Lazy<string>(() => "fldb_HasDependencies");
    public static string fMitigationHasDependencies => _fMitigationHasDependencies.Value;

    private static readonly Lazy<string> _fMitigationIsPrerequisite = new Lazy<string>(() => "fldb_IsPrerequisite");
    public static string fMitigationIsPrerequisite => _fMitigationIsPrerequisite.Value;

    private static readonly Lazy<string> _fMitigationImplementationPlan = new Lazy<string>(() => "fldv_ImplementationPlan");
    public static string fMitigationImplementationPlan => _fMitigationImplementationPlan.Value;

    private static readonly Lazy<string> _fMitigationCommunicationPlan = new Lazy<string>(() => "fldv_CommunicationPlan");
    public static string fMitigationCommunicationPlan => _fMitigationCommunicationPlan.Value;

    private static readonly Lazy<string> _fMitigationTrainingRequirements = new Lazy<string>(() => "fldv_TrainingRequirements");
    public static string fMitigationTrainingRequirements => _fMitigationTrainingRequirements.Value;

    private static readonly Lazy<string> _fMitigationDocumentationUpdates = new Lazy<string>(() => "fldv_DocumentationUpdates");
    public static string fMitigationDocumentationUpdates => _fMitigationDocumentationUpdates.Value;

    private static readonly Lazy<string> _fMitigationTestingProcedure = new Lazy<string>(() => "fldv_TestingProcedure");
    public static string fMitigationTestingProcedure => _fMitigationTestingProcedure.Value;

    private static readonly Lazy<string> _fMitigationTestingCompletedDate = new Lazy<string>(() => "fldd_TestingCompletedDate");
    public static string fMitigationTestingCompletedDate => _fMitigationTestingCompletedDate.Value;

    private static readonly Lazy<string> _fMitigationTestingResults = new Lazy<string>(() => "fldv_TestingResults");
    public static string fMitigationTestingResults => _fMitigationTestingResults.Value;

    private static readonly Lazy<string> _fMitigationValidationRequired = new Lazy<string>(() => "fldb_ValidationRequired");
    public static string fMitigationValidationRequired => _fMitigationValidationRequired.Value;

    private static readonly Lazy<string> _fMitigationValidationDate = new Lazy<string>(() => "fldd_ValidationDate");
    public static string fMitigationValidationDate => _fMitigationValidationDate.Value;

    private static readonly Lazy<string> _fMitigationValidatedBy = new Lazy<string>(() => "fldv_ValidatedBy");
    public static string fMitigationValidatedBy => _fMitigationValidatedBy.Value;

    private static readonly Lazy<string> _fMitigationNotes = new Lazy<string>(() => "fldv_Notes");
    public static string fMitigationNotes => _fMitigationNotes.Value;

    private static readonly Lazy<string> _fMitigationLessonsLearned = new Lazy<string>(() => "fldv_LessonsLearned");
    public static string fMitigationLessonsLearned => _fMitigationLessonsLearned.Value;

    private static readonly Lazy<string> _fMitigationRecommendationsForFuture = new Lazy<string>(() => "fldv_RecommendationsForFuture");
    public static string fMitigationRecommendationsForFuture => _fMitigationRecommendationsForFuture.Value;

    #endregion

    #region Mitigation Assignment Field Names

    /// <summary>
    /// Mitigation Assignment field names
    /// </summary>
    private static readonly Lazy<string> _fMitigationAssignmentCode = new Lazy<string>(() => "fldv_Code");
    public static string fMitigationAssignmentCode => _fMitigationAssignmentCode.Value;

    private static readonly Lazy<string> _fMitigationAssignmentMitigationCode = new Lazy<string>(() => "fldv_MitigationCode");
    public static string fMitigationAssignmentMitigationCode => _fMitigationAssignmentMitigationCode.Value;

    private static readonly Lazy<string> _fMitigationAssignmentDepartmentCode = new Lazy<string>(() => "fldv_DepartmentCode");
    public static string fMitigationAssignmentDepartmentCode => _fMitigationAssignmentDepartmentCode.Value;

    #endregion

    #region Report Validation Field Names

    /// <summary>
    /// Report Validation field names
    /// </summary>
    private static readonly Lazy<string> _fReportValidationCode = new Lazy<string>(() => "fldv_Code");
    public static string fReportValidationCode => _fReportValidationCode.Value;

    private static readonly Lazy<string> _fReportValidationReportCode = new Lazy<string>(() => "fldv_ReportCode");
    public static string fReportValidationReportCode => _fReportValidationReportCode.Value;

    private static readonly Lazy<string> _fReportValidationDecision = new Lazy<string>(() => "fldv_ValidationDecision");
    public static string fReportValidationDecision => _fReportValidationDecision.Value;

    private static readonly Lazy<string> _fReportValidationStatus = new Lazy<string>(() => "fldv_Status");
    public static string fReportValidationStatus => _fReportValidationStatus.Value;

    private static readonly Lazy<string> _fReportValidationStage = new Lazy<string>(() => "fldv_Stage");
    public static string fReportValidationStage => _fReportValidationStage.Value;

    private static readonly Lazy<string> _fReportValidationType = new Lazy<string>(() => "fldv_ValidationType");
    public static string fReportValidationType => _fReportValidationType.Value;

    private static readonly Lazy<string> _fReportValidationComments = new Lazy<string>(() => "fldv_ValidationComments");
    public static string fReportValidationComments => _fReportValidationComments.Value;

    private static readonly Lazy<string> _fReportValidationValidatedBy = new Lazy<string>(() => "fldv_ValidatedBy");
    public static string fReportValidationValidatedBy => _fReportValidationValidatedBy.Value;

    private static readonly Lazy<string> _fReportValidationValidatedDate = new Lazy<string>(() => "fldd_ValidatedDate");
    public static string fReportValidationValidatedDate => _fReportValidationValidatedDate.Value;

    #endregion

    #region Scoring Panel Field Names

    /// <summary>
    /// Scoring Panel field names
    /// </summary>
    private static readonly Lazy<string> _fScoringPanelCode = new Lazy<string>(() => "fldv_Code");
    public static string fScoringPanelCode => _fScoringPanelCode.Value;

    private static readonly Lazy<string> _fScoringPanelHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fScoringPanelHazardCode => _fScoringPanelHazardCode.Value;

    private static readonly Lazy<string> _fScoringPanelRiskAssessmentCode = new Lazy<string>(() => "fldv_RiskAssessmentCode");
    public static string fScoringPanelRiskAssessmentCode => _fScoringPanelRiskAssessmentCode.Value;

    private static readonly Lazy<string> _fScoringPanelSMSUserCode = new Lazy<string>(() => "fldv_SMSUserCode");
    public static string fScoringPanelSMSUserCode => _fScoringPanelSMSUserCode.Value;

    private static readonly Lazy<string> _fScoringPanelResidualLikelihood = new Lazy<string>(() => "fldi_ResidualLikelyhood");
    public static string fScoringPanelResidualLikelihood => _fScoringPanelResidualLikelihood.Value;

    private static readonly Lazy<string> _fScoringPanelResidualSeverity = new Lazy<string>(() => "fldi_ResidualSeverity");
    public static string fScoringPanelResidualSeverity => _fScoringPanelResidualSeverity.Value;

    private static readonly Lazy<string> _fScoringPanelResidualScore = new Lazy<string>(() => "fldm_ResidualScore");
    public static string fScoringPanelResidualScore => _fScoringPanelResidualScore.Value;

    private static readonly Lazy<string> _fScoringPanelResidualRationale = new Lazy<string>(() => "fldv_ResidualScoreRationale");
    public static string fScoringPanelResidualRationale => _fScoringPanelResidualRationale.Value;


    private static readonly Lazy<string> _fScoringPanelInitialLikelihood = new Lazy<string>(() => "fldi_InitialLikelyhood");
    public static string fScoringPanelInitialLikelihood => _fScoringPanelInitialLikelihood.Value;

    private static readonly Lazy<string> _fScoringPanelInitialSeverity = new Lazy<string>(() => "fldi_InitialSeverity");
    public static string fScoringPanelInitialSeverity => _fScoringPanelInitialSeverity.Value;

    private static readonly Lazy<string> _fScoringPanelInitialScore = new Lazy<string>(() => "fldm_InitialScore");
    public static string fScoringPanelInitialScore => _fScoringPanelInitialScore.Value;

    private static readonly Lazy<string> _fScoringPanelInitialRationale = new Lazy<string>(() => "fldv_InitialScoreRationale");
    public static string fScoringPanelInitialRationale => _fScoringPanelInitialRationale.Value;










    #endregion

    #region SMS Audit Management Field Names

    /// <summary>
    /// SMS Audit Plan table field names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _fSMSAuditPlanCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSAuditPlanCode => _fSMSAuditPlanCode.Value;

    private static readonly Lazy<string> _fSMSAuditPlanName = new Lazy<string>(() => "fldv_Name");
    public static string fSMSAuditPlanName => _fSMSAuditPlanName.Value;

    private static readonly Lazy<string> _fSMSAuditPlanDescription = new Lazy<string>(() => "fldv_Description");
    public static string fSMSAuditPlanDescription => _fSMSAuditPlanDescription.Value;

    private static readonly Lazy<string> _fSMSAuditPlanAuditType = new Lazy<string>(() => "fldv_AuditType");
    public static string fSMSAuditPlanAuditType => _fSMSAuditPlanAuditType.Value;

    private static readonly Lazy<string> _fSMSAuditPlanScope = new Lazy<string>(() => "fldv_Scope");
    public static string fSMSAuditPlanScope => _fSMSAuditPlanScope.Value;

    private static readonly Lazy<string> _fSMSAuditPlanObjectives = new Lazy<string>(() => "fldv_Objectives");
    public static string fSMSAuditPlanObjectives => _fSMSAuditPlanObjectives.Value;

    private static readonly Lazy<string> _fSMSAuditPlanPlannedStartDate = new Lazy<string>(() => "fldd_PlannedStartDate");
    public static string fSMSAuditPlanPlannedStartDate => _fSMSAuditPlanPlannedStartDate.Value;

    private static readonly Lazy<string> _fSMSAuditPlanPlannedEndDate = new Lazy<string>(() => "fldd_PlannedEndDate");
    public static string fSMSAuditPlanPlannedEndDate => _fSMSAuditPlanPlannedEndDate.Value;

    private static readonly Lazy<string> _fSMSAuditPlanLeadAuditor = new Lazy<string>(() => "fldv_LeadAuditor");
    public static string fSMSAuditPlanLeadAuditor => _fSMSAuditPlanLeadAuditor.Value;

    private static readonly Lazy<string> _fSMSAuditPlanAuditorTeam = new Lazy<string>(() => "fldv_AuditorTeam");
    public static string fSMSAuditPlanAuditorTeam => _fSMSAuditPlanAuditorTeam.Value;

    private static readonly Lazy<string> _fSMSAuditPlanResponsibleDepartment = new Lazy<string>(() => "fldv_ResponsibleDepartment");
    public static string fSMSAuditPlanResponsibleDepartment => _fSMSAuditPlanResponsibleDepartment.Value;

    private static readonly Lazy<string> _fSMSAuditPlanStatus = new Lazy<string>(() => "fldv_Status");
    public static string fSMSAuditPlanStatus => _fSMSAuditPlanStatus.Value;

    private static readonly Lazy<string> _fSMSAuditPlanPriority = new Lazy<string>(() => "fldv_Priority");
    public static string fSMSAuditPlanPriority => _fSMSAuditPlanPriority.Value;

    private static readonly Lazy<string> _fSMSAuditPlanRecurrencePattern = new Lazy<string>(() => "fldv_RecurrencePattern");
    public static string fSMSAuditPlanRecurrencePattern => _fSMSAuditPlanRecurrencePattern.Value;

    private static readonly Lazy<string> _fSMSAuditPlanRequiresApproval = new Lazy<string>(() => "fldb_RequiresApproval");
    public static string fSMSAuditPlanRequiresApproval => _fSMSAuditPlanRequiresApproval.Value;

    private static readonly Lazy<string> _fSMSAuditPlanApprovedBy = new Lazy<string>(() => "fldv_ApprovedBy");
    public static string fSMSAuditPlanApprovedBy => _fSMSAuditPlanApprovedBy.Value;

    private static readonly Lazy<string> _fSMSAuditPlanApprovedDate = new Lazy<string>(() => "fldd_ApprovedDate");
    public static string fSMSAuditPlanApprovedDate => _fSMSAuditPlanApprovedDate.Value;

    private static readonly Lazy<string> _fSMSAuditPlanExpectedDurationHours = new Lazy<string>(() => "fldi_ExpectedDurationHours");
    public static string fSMSAuditPlanExpectedDurationHours => _fSMSAuditPlanExpectedDurationHours.Value;

    private static readonly Lazy<string> _fSMSAuditPlanNotes = new Lazy<string>(() => "fldv_Notes");
    public static string fSMSAuditPlanNotes => _fSMSAuditPlanNotes.Value;

    /// <summary>
    /// SMS Audit table field names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _fSMSAuditCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSAuditCode => _fSMSAuditCode.Value;

    private static readonly Lazy<string> _fSMSAuditName = new Lazy<string>(() => "fldv_Name");
    public static string fSMSAuditName => _fSMSAuditName.Value;

    private static readonly Lazy<string> _fSMSAuditDescription = new Lazy<string>(() => "fldv_Description");
    public static string fSMSAuditDescription => _fSMSAuditDescription.Value;

    private static readonly Lazy<string> _fSMSAuditAuditPlanCode = new Lazy<string>(() => "fldv_AuditPlanCode");
    public static string fSMSAuditAuditPlanCode => _fSMSAuditAuditPlanCode.Value;

    private static readonly Lazy<string> _fSMSAuditAuditType = new Lazy<string>(() => "fldv_AuditType");
    public static string fSMSAuditAuditType => _fSMSAuditAuditType.Value;

    private static readonly Lazy<string> _fSMSAuditScope = new Lazy<string>(() => "fldv_Scope");
    public static string fSMSAuditScope => _fSMSAuditScope.Value;

    private static readonly Lazy<string> _fSMSAuditObjectives = new Lazy<string>(() => "fldv_Objectives");
    public static string fSMSAuditObjectives => _fSMSAuditObjectives.Value;

    private static readonly Lazy<string> _fSMSAuditScheduledStartDate = new Lazy<string>(() => "fldd_ScheduledStartDate");
    public static string fSMSAuditScheduledStartDate => _fSMSAuditScheduledStartDate.Value;

    private static readonly Lazy<string> _fSMSAuditScheduledEndDate = new Lazy<string>(() => "fldd_ScheduledEndDate");
    public static string fSMSAuditScheduledEndDate => _fSMSAuditScheduledEndDate.Value;

    private static readonly Lazy<string> _fSMSAuditActualStartDate = new Lazy<string>(() => "fldd_ActualStartDate");
    public static string fSMSAuditActualStartDate => _fSMSAuditActualStartDate.Value;

    private static readonly Lazy<string> _fSMSAuditActualEndDate = new Lazy<string>(() => "fldd_ActualEndDate");
    public static string fSMSAuditActualEndDate => _fSMSAuditActualEndDate.Value;

    private static readonly Lazy<string> _fSMSAuditLeadAuditor = new Lazy<string>(() => "fldv_LeadAuditor");
    public static string fSMSAuditLeadAuditor => _fSMSAuditLeadAuditor.Value;

    private static readonly Lazy<string> _fSMSAuditAuditorTeam = new Lazy<string>(() => "fldv_AuditorTeam");
    public static string fSMSAuditAuditorTeam => _fSMSAuditAuditorTeam.Value;

    private static readonly Lazy<string> _fSMSAuditResponsibleDepartment = new Lazy<string>(() => "fldv_ResponsibleDepartment");
    public static string fSMSAuditResponsibleDepartment => _fSMSAuditResponsibleDepartment.Value;

    private static readonly Lazy<string> _fSMSAuditStatus = new Lazy<string>(() => "fldv_Status");
    public static string fSMSAuditStatus => _fSMSAuditStatus.Value;

    private static readonly Lazy<string> _fSMSAuditPriority = new Lazy<string>(() => "fldv_Priority");
    public static string fSMSAuditPriority => _fSMSAuditPriority.Value;

    private static readonly Lazy<string> _fSMSAuditContactPerson = new Lazy<string>(() => "fldv_ContactPerson");
    public static string fSMSAuditContactPerson => _fSMSAuditContactPerson.Value;

    private static readonly Lazy<string> _fSMSAuditLocation = new Lazy<string>(() => "fldv_AuditLocation");
    public static string fSMSAuditLocation => _fSMSAuditLocation.Value;

    private static readonly Lazy<string> _fSMSAuditTotalFindings = new Lazy<string>(() => "fldi_TotalFindings");
    public static string fSMSAuditTotalFindings => _fSMSAuditTotalFindings.Value;

    private static readonly Lazy<string> _fSMSAuditCriticalFindings = new Lazy<string>(() => "fldi_CriticalFindings");
    public static string fSMSAuditCriticalFindings => _fSMSAuditCriticalFindings.Value;

    private static readonly Lazy<string> _fSMSAuditMajorFindings = new Lazy<string>(() => "fldi_MajorFindings");
    public static string fSMSAuditMajorFindings => _fSMSAuditMajorFindings.Value;

    private static readonly Lazy<string> _fSMSAuditMinorFindings = new Lazy<string>(() => "fldi_MinorFindings");
    public static string fSMSAuditMinorFindings => _fSMSAuditMinorFindings.Value;

    private static readonly Lazy<string> _fSMSAuditObservations = new Lazy<string>(() => "fldi_Observations");
    public static string fSMSAuditObservations => _fSMSAuditObservations.Value;

    private static readonly Lazy<string> _fSMSAuditReportSubmittedDate = new Lazy<string>(() => "fldd_ReportSubmittedDate");
    public static string fSMSAuditReportSubmittedDate => _fSMSAuditReportSubmittedDate.Value;

    private static readonly Lazy<string> _fSMSAuditExecutiveSummary = new Lazy<string>(() => "fldv_ExecutiveSummary");
    public static string fSMSAuditExecutiveSummary => _fSMSAuditExecutiveSummary.Value;

    private static readonly Lazy<string> _fSMSAuditNotes = new Lazy<string>(() => "fldv_Notes");
    public static string fSMSAuditNotes => _fSMSAuditNotes.Value;

    /// <summary>
    /// SMS Audit Finding table field names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _fSMSAuditFindingCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSAuditFindingCode => _fSMSAuditFindingCode.Value;

    private static readonly Lazy<string> _fSMSAuditFindingAuditCode = new Lazy<string>(() => "fldv_AuditCode");
    public static string fSMSAuditFindingAuditCode => _fSMSAuditFindingAuditCode.Value;

    private static readonly Lazy<string> _fSMSAuditFindingTitle = new Lazy<string>(() => "fldv_Title");
    public static string fSMSAuditFindingTitle => _fSMSAuditFindingTitle.Value;

    private static readonly Lazy<string> _fSMSAuditFindingDescription = new Lazy<string>(() => "fldv_Description");
    public static string fSMSAuditFindingDescription => _fSMSAuditFindingDescription.Value;

    private static readonly Lazy<string> _fSMSAuditFindingSeverity = new Lazy<string>(() => "fldv_Severity");
    public static string fSMSAuditFindingSeverity => _fSMSAuditFindingSeverity.Value;

    private static readonly Lazy<string> _fSMSAuditFindingCategory = new Lazy<string>(() => "fldv_Category");
    public static string fSMSAuditFindingCategory => _fSMSAuditFindingCategory.Value;

    private static readonly Lazy<string> _fSMSAuditFindingStatus = new Lazy<string>(() => "fldv_Status");
    public static string fSMSAuditFindingStatus => _fSMSAuditFindingStatus.Value;

    private static readonly Lazy<string> _fSMSAuditFindingDiscoveredDate = new Lazy<string>(() => "fldd_DiscoveredDate");
    public static string fSMSAuditFindingDiscoveredDate => _fSMSAuditFindingDiscoveredDate.Value;

    private static readonly Lazy<string> _fSMSAuditFindingResponsiblePerson = new Lazy<string>(() => "fldv_ResponsiblePerson");
    public static string fSMSAuditFindingResponsiblePerson => _fSMSAuditFindingResponsiblePerson.Value;

    private static readonly Lazy<string> _fSMSAuditFindingTargetResolutionDate = new Lazy<string>(() => "fldd_TargetResolutionDate");
    public static string fSMSAuditFindingTargetResolutionDate => _fSMSAuditFindingTargetResolutionDate.Value;

    private static readonly Lazy<string> _fSMSAuditFindingActualResolutionDate = new Lazy<string>(() => "fldd_ActualResolutionDate");
    public static string fSMSAuditFindingActualResolutionDate => _fSMSAuditFindingActualResolutionDate.Value;

    private static readonly Lazy<string> _fSMSAuditFindingCorrectiveAction = new Lazy<string>(() => "fldv_CorrectiveAction");
    public static string fSMSAuditFindingCorrectiveAction => _fSMSAuditFindingCorrectiveAction.Value;

    private static readonly Lazy<string> _fSMSAuditFindingRootCauseAnalysis = new Lazy<string>(() => "fldv_RootCauseAnalysis");
    public static string fSMSAuditFindingRootCauseAnalysis => _fSMSAuditFindingRootCauseAnalysis.Value;

    private static readonly Lazy<string> _fSMSAuditFindingVerificationRequired = new Lazy<string>(() => "fldb_VerificationRequired");
    public static string fSMSAuditFindingVerificationRequired => _fSMSAuditFindingVerificationRequired.Value;

    private static readonly Lazy<string> _fSMSAuditFindingVerifiedBy = new Lazy<string>(() => "fldv_VerifiedBy");
    public static string fSMSAuditFindingVerifiedBy => _fSMSAuditFindingVerifiedBy.Value;

    private static readonly Lazy<string> _fSMSAuditFindingVerificationDate = new Lazy<string>(() => "fldd_VerificationDate");
    public static string fSMSAuditFindingVerificationDate => _fSMSAuditFindingVerificationDate.Value;

    private static readonly Lazy<string> _fSMSAuditFindingNotes = new Lazy<string>(() => "fldv_Notes");
    public static string fSMSAuditFindingNotes => _fSMSAuditFindingNotes.Value;

    /// <summary>
    /// SMS Audit Evidence table field names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _fSMSAuditEvidenceCode = new Lazy<string>(() => "fldv_Code");
    public static string fSMSAuditEvidenceCode => _fSMSAuditEvidenceCode.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceAuditCode = new Lazy<string>(() => "fldv_AuditCode");
    public static string fSMSAuditEvidenceAuditCode => _fSMSAuditEvidenceAuditCode.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceFindingCode = new Lazy<string>(() => "fldv_FindingCode");
    public static string fSMSAuditEvidenceFindingCode => _fSMSAuditEvidenceFindingCode.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceTitle = new Lazy<string>(() => "fldv_Title");
    public static string fSMSAuditEvidenceTitle => _fSMSAuditEvidenceTitle.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceDescription = new Lazy<string>(() => "fldv_Description");
    public static string fSMSAuditEvidenceDescription => _fSMSAuditEvidenceDescription.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceType = new Lazy<string>(() => "fldv_EvidenceType");
    public static string fSMSAuditEvidenceType => _fSMSAuditEvidenceType.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceSource = new Lazy<string>(() => "fldv_Source");
    public static string fSMSAuditEvidenceSource => _fSMSAuditEvidenceSource.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceCollectedBy = new Lazy<string>(() => "fldv_CollectedBy");
    public static string fSMSAuditEvidenceCollectedBy => _fSMSAuditEvidenceCollectedBy.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceCollectionDate = new Lazy<string>(() => "fldd_CollectionDate");
    public static string fSMSAuditEvidenceCollectionDate => _fSMSAuditEvidenceCollectionDate.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceFilePath = new Lazy<string>(() => "fldv_FilePath");
    public static string fSMSAuditEvidenceFilePath => _fSMSAuditEvidenceFilePath.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceFileSize = new Lazy<string>(() => "fldi_FileSize");
    public static string fSMSAuditEvidenceFileSize => _fSMSAuditEvidenceFileSize.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceContentType = new Lazy<string>(() => "fldv_ContentType");
    public static string fSMSAuditEvidenceContentType => _fSMSAuditEvidenceContentType.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceStorageLocation = new Lazy<string>(() => "fldv_StorageLocation");
    public static string fSMSAuditEvidenceStorageLocation => _fSMSAuditEvidenceStorageLocation.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceConfidentialityLevel = new Lazy<string>(() => "fldv_ConfidentialityLevel");
    public static string fSMSAuditEvidenceConfidentialityLevel => _fSMSAuditEvidenceConfidentialityLevel.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceRetentionPeriodMonths = new Lazy<string>(() => "fldi_RetentionPeriodMonths");
    public static string fSMSAuditEvidenceRetentionPeriodMonths => _fSMSAuditEvidenceRetentionPeriodMonths.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceRetentionReason = new Lazy<string>(() => "fldv_RetentionReason");
    public static string fSMSAuditEvidenceRetentionReason => _fSMSAuditEvidenceRetentionReason.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceIsVerified = new Lazy<string>(() => "fldb_IsVerified");
    public static string fSMSAuditEvidenceIsVerified => _fSMSAuditEvidenceIsVerified.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceVerifiedBy = new Lazy<string>(() => "fldv_VerifiedBy");
    public static string fSMSAuditEvidenceVerifiedBy => _fSMSAuditEvidenceVerifiedBy.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceVerificationDate = new Lazy<string>(() => "fldd_VerificationDate");
    public static string fSMSAuditEvidenceVerificationDate => _fSMSAuditEvidenceVerificationDate.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceIsArchived = new Lazy<string>(() => "fldb_IsArchived");
    public static string fSMSAuditEvidenceIsArchived => _fSMSAuditEvidenceIsArchived.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceArchivedDate = new Lazy<string>(() => "fldd_ArchivedDate");
    public static string fSMSAuditEvidenceArchivedDate => _fSMSAuditEvidenceArchivedDate.Value;

    private static readonly Lazy<string> _fSMSAuditEvidenceNotes = new Lazy<string>(() => "fldv_Notes");
    public static string fSMSAuditEvidenceNotes => _fSMSAuditEvidenceNotes.Value;

    #endregion

    #region HazardReportTracking Field Names

    /// <summary>
    /// HazardReportTracking table field names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _fHazardReportTrackingHazardCode = new(() => "fldv_HazardCode");
    public static string fHazardReportTrackingHazardCode => _fHazardReportTrackingHazardCode.Value;

    private static readonly Lazy<string> _fHazardReportTrackingReportCode = new(() => "fldv_ReportCode");
    public static string fHazardReportTrackingReportCode => _fHazardReportTrackingReportCode.Value;

    private static readonly Lazy<string> _fHazardReportTrackingTrackingCode = new(() => "fldv_Code");
    public static string fHazardReportTrackingTrackingCode => _fHazardReportTrackingTrackingCode.Value;

    #endregion

    #region EventQueue Field Names

    /// <summary>
    /// EventQueue table field names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _fEventQueueCode = new(() => "fldv_Code");
    public static string fEventQueueCode => _fEventQueueCode.Value;

    private static readonly Lazy<string> _fEventQueueGuid = new(() => "fldv_QueueGuid");
    public static string fEventQueueGuid => _fEventQueueGuid.Value;

    private static readonly Lazy<string> _fEventQueueEventCategory = new(() => "fldi_EventCategory");
    public static string fEventQueueEventCategory => _fEventQueueEventCategory.Value;

    private static readonly Lazy<string> _fEventQueueEventType = new(() => "fldv_EventType");
    public static string fEventQueueEventType => _fEventQueueEventType.Value;

    private static readonly Lazy<string> _fEventQueueEventData = new(() => "fldv_EventData");
    public static string fEventQueueEventData => _fEventQueueEventData.Value;

    private static readonly Lazy<string> _fEventQueueReportCode = new(() => "fldv_ReportCode");
    public static string fEventQueueReportCode => _fEventQueueReportCode.Value;

    private static readonly Lazy<string> _fEventQueueTargetSystem = new(() => "fldv_TargetSystem");
    public static string fEventQueueTargetSystem => _fEventQueueTargetSystem.Value;

    private static readonly Lazy<string> _fEventQueuePriority = new(() => "fldi_Priority");
    public static string fEventQueuePriority => _fEventQueuePriority.Value;

    private static readonly Lazy<string> _fEventQueueStatus = new(() => "fldi_Status");
    public static string fEventQueueStatus => _fEventQueueStatus.Value;

    private static readonly Lazy<string> _fEventQueueAttemptCount = new(() => "fldi_AttemptCount");
    public static string fEventQueueAttemptCount => _fEventQueueAttemptCount.Value;

    private static readonly Lazy<string> _fEventQueueMaxAttempts = new(() => "fldi_MaxAttempts");
    public static string fEventQueueMaxAttempts => _fEventQueueMaxAttempts.Value;

    private static readonly Lazy<string> _fEventQueueLastError = new(() => "fldv_LastError");
    public static string fEventQueueLastError => _fEventQueueLastError.Value;

    private static readonly Lazy<string> _fEventQueueQueuedDate = new(() => "fldd_QueuedDate");
    public static string fEventQueueQueuedDate => _fEventQueueQueuedDate.Value;

    private static readonly Lazy<string> _fEventQueueProcessingStartedDate = new(() => "fldd_ProcessingStartedDate");
    public static string fEventQueueProcessingStartedDate => _fEventQueueProcessingStartedDate.Value;

    private static readonly Lazy<string> _fEventQueueProcessedDate = new(() => "fldd_ProcessedDate");
    public static string fEventQueueProcessedDate => _fEventQueueProcessedDate.Value;

    private static readonly Lazy<string> _fEventQueueNextAttemptDate = new(() => "fldd_NextAttemptDate");
    public static string fEventQueueNextAttemptDate => _fEventQueueNextAttemptDate.Value;

    private static readonly Lazy<string> _fEventQueueLockedBy = new(() => "fldv_LockedBy");
    public static string fEventQueueLockedBy => _fEventQueueLockedBy.Value;

    private static readonly Lazy<string> _fEventQueueLockExpiresDate = new(() => "fldd_LockExpiresDate");
    public static string fEventQueueLockExpiresDate => _fEventQueueLockExpiresDate.Value;

    private static readonly Lazy<string> _fEventQueueQueuedBy = new(() => "fldv_QueuedBy");
    public static string fEventQueueQueuedBy => _fEventQueueQueuedBy.Value;

    private static readonly Lazy<string> _fEventQueueCorrelationId = new(() => "fldv_CorrelationId");
    public static string fEventQueueCorrelationId => _fEventQueueCorrelationId.Value;

    private static readonly Lazy<string> _fEventQueueCausationId = new(() => "fldv_CausationId");
    public static string fEventQueueCausationId => _fEventQueueCausationId.Value;

    private static readonly Lazy<string> _fEventQueuePendingCount = new(() => "fldi_PendingCount");
    public static string fEventQueuePendingCount => _fEventQueuePendingCount.Value;

    private static readonly Lazy<string> _fEventQueuePending = new(() => "fldi_Pending");
    public static string fEventQueuePending => _fEventQueuePending.Value;

    private static readonly Lazy<string> _fEventQueueProcessedCount = new(() => "fldi_ProcessedCount");
    public static string fEventQueueProcessedCount => _fEventQueueProcessedCount.Value;

    private static readonly Lazy<string> _fEventQueueProcessed = new(() => "fldi_Processed");
    public static string fEventQueueProcessed => _fEventQueueProcessed.Value;

    private static readonly Lazy<string> _fEventQueueFailedCount = new(() => "fldi_FailedCount");
    public static string fEventQueueFailedCount => _fEventQueueFailedCount.Value;

    private static readonly Lazy<string> _fEventQueueFailed = new(() => "fldi_Failed");
    public static string fEventQueueFailed => _fEventQueueFailed.Value;

    private static readonly Lazy<string> _fEventQueueCancelledCount = new(() => "fldi_CancelledCount");
    public static string fEventQueueCancelledCount => _fEventQueueCancelledCount.Value;

    private static readonly Lazy<string> _fEventQueueCancelled = new(() => "fldi_Cancelled");
    public static string fEventQueueCancelled => _fEventQueueCancelled.Value;

    private static readonly Lazy<string> _fEventQueueTotalCount = new(() => "fldi_TotalCount");
    public static string fEventQueueTotalCount => _fEventQueueTotalCount.Value;

    private static readonly Lazy<string> _fEventQueueCount = new(() => "fldi_Count");
    public static string fEventQueueCount => _fEventQueueCount.Value;

    private static readonly Lazy<string> _fRowsAffected = new(() => "fldi_RowsAffected");
    public static string fRowsAffected => _fRowsAffected.Value;

    private static readonly Lazy<string> _fRemovedCount = new(() => "fldi_RemovedCount");
    public static string fRemovedCount => _fRemovedCount.Value;

    private static readonly Lazy<string> _fRecoveredCount = new(() => "fldi_RecoveredCount");
    public static string fRecoveredCount => _fRecoveredCount.Value;

    #endregion
}
