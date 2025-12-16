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

    private static readonly Lazy<string> _fSMSOrganizationalUserIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSOrganizationalUserIsActive => _fSMSOrganizationalUserIsActive.Value;

    private static readonly Lazy<string> _fSMSOrganizationalUserLastLoginDate = new Lazy<string>(() => "fldd_LastLoginDate");
    public static string fSMSOrganizationalUserLastLoginDate => _fSMSOrganizationalUserLastLoginDate.Value;

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
    /// SMS Stakeholder User Group Memberships table (tbld_SMSStakeholderUserGroups) - Junction table
    /// </summary>
    private static readonly Lazy<string> _fSMSStakeholderUserGroupUserCode = new Lazy<string>(() => "fldv_UserCode");
    public static string fSMSStakeholderUserGroupUserCode => _fSMSStakeholderUserGroupUserCode.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserGroupGroupCode = new Lazy<string>(() => "fldv_GroupCode");
    public static string fSMSStakeholderUserGroupGroupCode => _fSMSStakeholderUserGroupGroupCode.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserGroupAssignedDate = new Lazy<string>(() => "fldd_AssignedDate");
    public static string fSMSStakeholderUserGroupAssignedDate => _fSMSStakeholderUserGroupAssignedDate.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserGroupAssignedBy = new Lazy<string>(() => "fldv_AssignedBy");
    public static string fSMSStakeholderUserGroupAssignedBy => _fSMSStakeholderUserGroupAssignedBy.Value;

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

    private static readonly Lazy<string> _fHazardDescription = new Lazy<string>(() => "fldc_Description");
    public static string fHazardDescription => _fHazardDescription.Value;

    private static readonly Lazy<string> _fHazardReportCode = new Lazy<string>(() => "fldv_ReportCode");
    public static string fHazardReportCode => _fHazardReportCode.Value;

    private static readonly Lazy<string> _fHazardScoringPanelRiskMatrixCode = new Lazy<string>(() => "fldv_RiskMatrixCode");
    public static string fHazardScoringPanelRiskMatrixCode => _fHazardScoringPanelRiskMatrixCode.Value;

    private static readonly Lazy<string> _fHazardAverageScore = new Lazy<string>(() => "fldv_AverageScore");
    public static string fHazardAverageScore => _fHazardAverageScore.Value;

    // Additional Hazard fields for comprehensive SMS support
    private static readonly Lazy<string> _fHazardType = new Lazy<string>(() => "fldv_HazardType");
    public static string fHazardType => _fHazardType.Value;

    private static readonly Lazy<string> _fHazardReportedBy = new Lazy<string>(() => "fldv_ReportedBy");
    public static string fHazardReportedBy => _fHazardReportedBy.Value;

    private static readonly Lazy<string> _fHazardReportedOn = new Lazy<string>(() => "fldd_ReportedOn");
    public static string fHazardReportedOn => _fHazardReportedOn.Value;

    private static readonly Lazy<string> _fHazardReportingDepartment = new Lazy<string>(() => "fldv_ReportingDepartment");
    public static string fHazardReportingDepartment => _fHazardReportingDepartment.Value;

    private static readonly Lazy<string> _fHazardIsConfidential = new Lazy<string>(() => "fldb_IsConfidential");
    public static string fHazardIsConfidential => _fHazardIsConfidential.Value;

    private static readonly Lazy<string> _fHazardIsAnonymous = new Lazy<string>(() => "fldb_IsAnonymous");
    public static string fHazardIsAnonymous => _fHazardIsAnonymous.Value;

    private static readonly Lazy<string> _fHazardCategory = new Lazy<string>(() => "fldv_Category");
    public static string fHazardCategory => _fHazardCategory.Value;

    private static readonly Lazy<string> _fHazardFiveMComponent = new Lazy<string>(() => "fldv_FiveMComponent");
    public static string fHazardFiveMComponent => _fHazardFiveMComponent.Value;

    private static readonly Lazy<string> _fHazardStatus = new Lazy<string>(() => "fldv_Status");
    public static string fHazardStatus => _fHazardStatus.Value;

    private static readonly Lazy<string> _fHazardPriority = new Lazy<string>(() => "fldv_Priority");
    public static string fHazardPriority => _fHazardPriority.Value;

    private static readonly Lazy<string> _fHazardRiskLevel = new Lazy<string>(() => "fldv_RiskLevel");
    public static string fHazardRiskLevel => _fHazardRiskLevel.Value;

    private static readonly Lazy<string> _fHazardWorstCredibleOutcome = new Lazy<string>(() => "fldc_WorstCredibleOutcome");
    public static string fHazardWorstCredibleOutcome => _fHazardWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _fHazardRootCause = new Lazy<string>(() => "fldc_RootCause");
    public static string fHazardRootCause => _fHazardRootCause.Value;

    private static readonly Lazy<string> _fHazardCurrentMitigations = new Lazy<string>(() => "fldc_CurrentMitigations");
    public static string fHazardCurrentMitigations => _fHazardCurrentMitigations.Value;

    private static readonly Lazy<string> _fHazardProposedMitigations = new Lazy<string>(() => "fldc_ProposedMitigations");
    public static string fHazardProposedMitigations => _fHazardProposedMitigations.Value;

    private static readonly Lazy<string> _fHazardMitigationTargetDate = new Lazy<string>(() => "fldd_MitigationTargetDate");
    public static string fHazardMitigationTargetDate => _fHazardMitigationTargetDate.Value;

    private static readonly Lazy<string> _fHazardMitigationOwner = new Lazy<string>(() => "fldv_MitigationOwner");
    public static string fHazardMitigationOwner => _fHazardMitigationOwner.Value;

    private static readonly Lazy<string> _fHazardRequiresInvestigation = new Lazy<string>(() => "fldb_RequiresInvestigation");
    public static string fHazardRequiresInvestigation => _fHazardRequiresInvestigation.Value;

    private static readonly Lazy<string> _fHazardInvestigationCompletedDate = new Lazy<string>(() => "fldd_InvestigationCompletedDate");
    public static string fHazardInvestigationCompletedDate => _fHazardInvestigationCompletedDate.Value;

    private static readonly Lazy<string> _fHazardInvestigationNotes = new Lazy<string>(() => "fldc_InvestigationNotes");
    public static string fHazardInvestigationNotes => _fHazardInvestigationNotes.Value;

    private static readonly Lazy<string> _fHazardAdditionalComments = new Lazy<string>(() => "fldc_AdditionalComments");
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

    // Interview field names
    private static readonly Lazy<string> _fInterviewCode = new Lazy<string>(() => "fldv_InterviewCode");
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

    /// <summary>
    /// Report Validations table (tbld_ReportValidations) - UPDATED FOR STEPS 1-5
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

    private static readonly Lazy<string> _fReportValidationValidatedBy = new Lazy<string>(() => "fldv_ValidatedBy");
    public static string fReportValidationValidatedBy => _fReportValidationValidatedBy.Value;

    private static readonly Lazy<string> _fReportValidationValidatedDate = new Lazy<string>(() => "fldd_ValidatedDate");
    public static string fReportValidationValidatedDate => _fReportValidationValidatedDate.Value;

    private static readonly Lazy<string> _fReportValidationComments = new Lazy<string>(() => "fldv_ValidationComments");
    public static string fReportValidationComments => _fReportValidationComments.Value;

    private static readonly Lazy<string> _fReportValidationType = new Lazy<string>(() => "fldv_ValidationType");
    public static string fReportValidationType => _fReportValidationType.Value;

    /// <summary>
    /// Report Validations - Step 2 Hazard Data Fields (Multi-Hazard Support)
    /// </summary>
    private static readonly Lazy<string> _fReportValidationHazardIds = new Lazy<string>(() => "fldv_HazardIds");
    public static string fReportValidationHazardIds => _fReportValidationHazardIds.Value;

    private static readonly Lazy<string> _fReportValidationHazardDescriptions = new Lazy<string>(() => "fldv_HazardDescriptions");
    public static string fReportValidationHazardDescriptions => _fReportValidationHazardDescriptions.Value;

    private static readonly Lazy<string> _fReportValidationHazardCategories = new Lazy<string>(() => "fldv_HazardCategories");
    public static string fReportValidationHazardCategories => _fReportValidationHazardCategories.Value;

    private static readonly Lazy<string> _fReportValidationValidHazardCount = new Lazy<string>(() => "fldi_ValidHazardCount");
    public static string fReportValidationValidHazardCount => _fReportValidationValidHazardCount.Value;

    /// <summary>
    /// Report Validations - Step 3 Risk Analysis Fields (Multi-Hazard Support)
    /// </summary>
    private static readonly Lazy<string> _fReportValidationHazardAnalysesData = new Lazy<string>(() => "fldv_HazardAnalysesData");
    public static string fReportValidationHazardAnalysesData => _fReportValidationHazardAnalysesData.Value;

    private static readonly Lazy<string> _fReportValidationHazardWorstOutcomes = new Lazy<string>(() => "fldv_HazardWorstOutcomes");
    public static string fReportValidationHazardWorstOutcomes => _fReportValidationHazardWorstOutcomes.Value;

    private static readonly Lazy<string> _fReportValidationHazardRootCauses = new Lazy<string>(() => "fldv_HazardRootCauses");
    public static string fReportValidationHazardRootCauses => _fReportValidationHazardRootCauses.Value;

    /// <summary>
    /// Report Validations - Step 4 Panel Scoring Fields (Multi-Hazard Support)
    /// </summary>
    private static readonly Lazy<string> _fReportValidationHazardPanelMembers = new Lazy<string>(() => "fldv_HazardPanelMembers");
    public static string fReportValidationHazardPanelMembers => _fReportValidationHazardPanelMembers.Value;

    private static readonly Lazy<string> _fReportValidationHazardPanelScores = new Lazy<string>(() => "fldv_HazardPanelScores");
    public static string fReportValidationHazardPanelScores => _fReportValidationHazardPanelScores.Value;

    private static readonly Lazy<string> _fReportValidationHazardAverageScores = new Lazy<string>(() => "fldv_HazardAverageScores");
    public static string fReportValidationHazardAverageScores => _fReportValidationHazardAverageScores.Value;

    /// <summary>
    /// Report Validations - Step 5 Mitigation Fields (Multi-Hazard Support)
    /// </summary>
    private static readonly Lazy<string> _fReportValidationSavedMitigationStrategies = new Lazy<string>(() => "fldv_SavedMitigationStrategies");
    public static string fReportValidationSavedMitigationStrategies => _fReportValidationSavedMitigationStrategies.Value;

    private static readonly Lazy<string> _fReportValidationHazardMitigationStrategyIds = new Lazy<string>(() => "fldv_HazardMitigationStrategyIds");
    public static string fReportValidationHazardMitigationStrategyIds => _fReportValidationHazardMitigationStrategyIds.Value;

    private static readonly Lazy<string> _fReportValidationMonitoringRequirements = new Lazy<string>(() => "fldv_MonitoringRequirements");
    public static string fReportValidationMonitoringRequirements => _fReportValidationMonitoringRequirements.Value;

    /// <summary>
    /// Report Validations - Progress Tracking Fields (Steps 1-5 Support)
    /// </summary>
    private static readonly Lazy<string> _fReportValidationCurrentStep = new Lazy<string>(() => "fldi_CurrentStep");
    public static string fReportValidationCurrentStep => _fReportValidationCurrentStep.Value;

    private static readonly Lazy<string> _fReportValidationCompletedSteps = new Lazy<string>(() => "fldv_CompletedSteps");
    public static string fReportValidationCompletedSteps => _fReportValidationCompletedSteps.Value;

    private static readonly Lazy<string> _fReportValidationCompletionPercentage = new Lazy<string>(() => "fldi_CompletionPercentage");
    public static string fReportValidationCompletionPercentage => _fReportValidationCompletionPercentage.Value;

    private static readonly Lazy<string> _fReportValidationAssessmentType = new Lazy<string>(() => "fldv_AssessmentType");
    public static string fReportValidationAssessmentType => _fReportValidationAssessmentType.Value;

    private static readonly Lazy<string> _fReportValidationAssessmentCategory = new Lazy<string>(() => "fldv_AssessmentCategory");
    public static string fReportValidationAssessmentCategory => _fReportValidationAssessmentCategory.Value;

    private static readonly Lazy<string> _fReportValidationParentAssessmentId = new Lazy<string>(() => "fldv_ParentAssessmentId");
    public static string fReportValidationParentAssessmentId => _fReportValidationParentAssessmentId.Value;

    /// <summary>
    /// Reports table (tbld_Reports)
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

    /// <summary>
    /// Risk Analysis table (tbld_RiskAnalysis)
    /// </summary>
    private static readonly Lazy<string> _fRiskAnalysisCode = new Lazy<string>(() => "fldv_Code");
    public static string fRiskAnalysisCode => _fRiskAnalysisCode.Value;

    private static readonly Lazy<string> _fRiskAnalysisName = new Lazy<string>(() => "fldv_Name");
    public static string fRiskAnalysisName => _fRiskAnalysisName.Value;

    private static readonly Lazy<string> _fRiskAnalysisDescription = new Lazy<string>(() => "fldv_Description");
    public static string fRiskAnalysisDescription => _fRiskAnalysisDescription.Value;

    private static readonly Lazy<string> _fRiskAnalysisHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fRiskAnalysisHazardCode => _fRiskAnalysisHazardCode.Value;

    private static readonly Lazy<string> _fRiskAnalysisStatus = new Lazy<string>(() => "fldv_Status");
    public static string fRiskAnalysisStatus => _fRiskAnalysisStatus.Value;

    private static readonly Lazy<string> _fRiskAnalysisStage = new Lazy<string>(() => "fldv_Stage");
    public static string fRiskAnalysisStage => _fRiskAnalysisStage.Value;

    private static readonly Lazy<string> _fRiskAnalysisWorstCredibleOutcome = new Lazy<string>(() => "fldv_WorstCredibleOutcome");
    public static string fRiskAnalysisWorstCredibleOutcome => _fRiskAnalysisWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _fRiskAnalysisRootCause = new Lazy<string>(() => "fldv_RootCause");
    public static string fRiskAnalysisRootCause => _fRiskAnalysisRootCause.Value;

    /// <summary>
    /// Mitigation table (tbld_Mitigations)
    /// </summary>
    private static readonly Lazy<string> _fMitigationCode = new Lazy<string>(() => "fldv_Code");
    public static string fMitigationCode => _fMitigationCode.Value;

    private static readonly Lazy<string> _fMitigationHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fMitigationHazardCode => _fMitigationHazardCode.Value;

    /// <summary>
    /// Mitigation Assignment table (tbld_MitigationAssignments)
    /// </summary>
    private static readonly Lazy<string> _fMitigationAssignmentCode = new Lazy<string>(() => "fldv_Code");
    public static string fMitigationAssignmentCode => _fMitigationAssignmentCode.Value;

    private static readonly Lazy<string> _fMitigationAssignmentMitigationCode = new Lazy<string>(() => "fldv_MitigationCode");
    public static string fMitigationAssignmentMitigationCode => _fMitigationAssignmentMitigationCode.Value;

    private static readonly Lazy<string> _fMitigationAssignmentDepartmentCode = new Lazy<string>(() => "fldv_DepartmentCode");
    public static string fMitigationAssignmentDepartmentCode => _fMitigationAssignmentDepartmentCode.Value;

    /// <summary>
    /// Scoring Panel table (tbld_ScoringPanels)
    /// </summary>
    private static readonly Lazy<string> _fScoringPanelCode = new Lazy<string>(() => "fldv_Code");
    public static string fScoringPanelCode => _fScoringPanelCode.Value;

    private static readonly Lazy<string> _fScoringPanelHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fScoringPanelHazardCode => _fScoringPanelHazardCode.Value;

    private static readonly Lazy<string> _fScoringPanelRiskAssessmentCode = new Lazy<string>(() => "fldv_RiskAssessmentCode");
    public static string fScoringPanelRiskAssessmentCode => _fScoringPanelRiskAssessmentCode.Value;

    private static readonly Lazy<string> _fScoringPanelSMSUserCode = new Lazy<string>(() => "fldv_SMSUserCode");
    public static string fScoringPanelSMSUserCode => _fScoringPanelSMSUserCode.Value;

    private static readonly Lazy<string> _fScoringPanelLikelihood = new Lazy<string>(() => "fldv_Likelyhood");
    public static string fScoringPanelLikelihood => _fScoringPanelLikelihood.Value;

    private static readonly Lazy<string> _fScoringPanelSeverity = new Lazy<string>(() => "fldv_Severity");
    public static string fScoringPanelSeverity => _fScoringPanelSeverity.Value;

    private static readonly Lazy<string> _fScoringPanelScore = new Lazy<string>(() => "fldv_Score");
    public static string fScoringPanelScore => _fScoringPanelScore.Value;

    private static readonly Lazy<string> _fScoringPanelRationale = new Lazy<string>(() => "fldv_ScoreRationale");
    public static string fScoringPanelRationale => _fScoringPanelRationale.Value;

    /// <summary>
    /// SMS User Role table (tbld_SMSUserRoles)
    /// </summary>
    //private static readonly Lazy<string> _fSMSUserRoleCode = new Lazy<string>(() => "fldv_Code");
    //public static string fSMSUserRoleCode => _fSMSUserRoleCode.Value;

    private static readonly Lazy<string> _fSMSUserRoleUserId = new Lazy<string>(() => "fldv_UserId");
    public static string fSMSUserRoleUserId => _fSMSUserRoleUserId.Value;

    private static readonly Lazy<string> _fSMSUserRoleUserType = new Lazy<string>(() => "fldv_UserType");
    public static string fSMSUserRoleUserType => _fSMSUserRoleUserType.Value;

    private static readonly Lazy<string> _fSMSUserRoleSMSRoleCode = new Lazy<string>(() => "fldv_SMSRoleCode");
    public static string fSMSUserRoleSMSRoleCode => _fSMSUserRoleSMSRoleCode.Value;

    private static readonly Lazy<string> _fSMSUserRoleDepartment = new Lazy<string>(() => "fldv_Department");
    public static string fSMSUserRoleDepartment => _fSMSUserRoleDepartment.Value;

    private static readonly Lazy<string> _fSMSUserRoleEffectiveDate = new Lazy<string>(() => "fldd_EffectiveDate");
    public static string fSMSUserRoleEffectiveDate => _fSMSUserRoleEffectiveDate.Value;

    private static readonly Lazy<string> _fSMSUserRoleExpirationDate = new Lazy<string>(() => "fldd_ExpirationDate");
    public static string fSMSUserRoleExpirationDate => _fSMSUserRoleExpirationDate.Value;

    private static readonly Lazy<string> _fSMSUserRoleIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSUserRoleIsActive => _fSMSUserRoleIsActive.Value;

    private static readonly Lazy<string> _fSMSUserRoleAssignedBy = new Lazy<string>(() => "fldv_AssignedBy");
    public static string fSMSUserRoleAssignedBy => _fSMSUserRoleAssignedBy.Value;

    private static readonly Lazy<string> _fSMSUserRoleAssignedDate = new Lazy<string>(() => "fldd_AssignedDate");
    public static string fSMSUserRoleAssignedDate => _fSMSUserRoleAssignedDate.Value;

    private static readonly Lazy<string> _fSMSUserRoleDeactivatedBy = new Lazy<string>(() => "fldv_DeactivatedBy");
    public static string fSMSUserRoleDeactivatedBy => _fSMSUserRoleDeactivatedBy.Value;

    private static readonly Lazy<string> _fSMSUserRoleDeactivatedDate = new Lazy<string>(() => "fldd_DeactivatedDate");
    public static string fSMSUserRoleDeactivatedDate => _fSMSUserRoleDeactivatedDate.Value;

    /// <summary>
    /// Risk Assessment table (tbld_RiskAssessments) - UPDATED FOR STEPS 1-5
    /// </summary>
    private static readonly Lazy<string> _fRiskAssessmentCode = new Lazy<string>(() => "fldv_Code");
    public static string fRiskAssessmentCode => _fRiskAssessmentCode.Value;

    private static readonly Lazy<string> _fRiskAssessmentName = new Lazy<string>(() => "fldv_Name");
    public static string fRiskAssessmentName => _fRiskAssessmentName.Value;

    private static readonly Lazy<string> _fRiskAssessmentDescription = new Lazy<string>(() => "fldv_Description");
    public static string fRiskAssessmentDescription => _fRiskAssessmentDescription.Value;

    private static readonly Lazy<string> _fRiskAssessmentHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fRiskAssessmentHazardCode => _fRiskAssessmentHazardCode.Value;

    private static readonly Lazy<string> _fRiskAssessmentType = new Lazy<string>(() => "fldv_AssessmentType");
    public static string fRiskAssessmentType => _fRiskAssessmentType.Value;

    private static readonly Lazy<string> _fRiskAssessmentStatus = new Lazy<string>(() => "fldv_Status");
    public static string fRiskAssessmentStatus => _fRiskAssessmentStatus.Value;

    private static readonly Lazy<string> _fRiskAssessmentStage = new Lazy<string>(() => "fldv_Stage");
    public static string fRiskAssessmentStage => _fRiskAssessmentStage.Value;

    // Core Assessment Fields (NEW - Steps 1-5 Support)
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

    /// <summary>
    /// Risk Assessment - Step 1 System Description Fields
    /// </summary>
    private static readonly Lazy<string> _fRiskAssessmentSystemDescription = new Lazy<string>(() => "fldv_SystemDescription");
    public static string fRiskAssessmentSystemDescription => _fRiskAssessmentSystemDescription.Value;

    private static readonly Lazy<string> _fRiskAssessmentSystemBoundaries = new Lazy<string>(() => "fldv_SystemBoundaries");
    public static string fRiskAssessmentSystemBoundaries => _fRiskAssessmentSystemBoundaries.Value;

    private static readonly Lazy<string> _fRiskAssessmentSystemPurpose = new Lazy<string>(() => "fldv_SystemPurpose");
    public static string fRiskAssessmentSystemPurpose => _fRiskAssessmentSystemPurpose.Value;

    // 5M Framework Fields - CORRECTED naming
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

    private static readonly Lazy<string> _fRiskAssessmentFiveMOperationalEnvironment = new Lazy<string>(() => "fldv_EnvironmentFactors");
    public static string fRiskAssessmentFiveMOperationalEnvironment => _fRiskAssessmentFiveMOperationalEnvironment.Value;

    /// <summary>
    /// Risk Assessment - Step 3 Risk Analysis Fields
    /// </summary>
    private static readonly Lazy<string> _fRiskAssessmentRiskAnalysisMethod = new Lazy<string>(() => "fldv_RiskAnalysisMethod");
    public static string fRiskAssessmentRiskAnalysisMethod => _fRiskAssessmentRiskAnalysisMethod.Value;

    private static readonly Lazy<string> _fRiskAssessmentRiskCriteria = new Lazy<string>(() => "fldv_RiskCriteria");
    public static string fRiskAssessmentRiskCriteria => _fRiskAssessmentRiskCriteria.Value;

    /// <summary>
    /// Risk Assessment - Step 4 Risk Assessment Fields
    /// </summary>
    private static readonly Lazy<string> _fRiskAssessmentTolerabilityFramework = new Lazy<string>(() => "fldv_TolerabilityFramework");
    public static string fRiskAssessmentTolerabilityFramework => _fRiskAssessmentTolerabilityFramework.Value;

    private static readonly Lazy<string> _fRiskAssessmentRiskAcceptanceCriteria = new Lazy<string>(() => "fldv_RiskAcceptanceCriteria");
    public static string fRiskAssessmentRiskAcceptanceCriteria => _fRiskAssessmentRiskAcceptanceCriteria.Value;

    private static readonly Lazy<string> _fRiskAssessmentFinalSeverityScore = new Lazy<string>(() => "fldi_FinalSeverityScore");
    public static string fRiskAssessmentFinalSeverityScore => _fRiskAssessmentFinalSeverityScore.Value;

    private static readonly Lazy<string> _fRiskAssessmentFinalLikelihoodScore = new Lazy<string>(() => "fldi_FinalLikelihoodScore");
    public static string fRiskAssessmentFinalLikelihoodScore => _fRiskAssessmentFinalLikelihoodScore.Value;

    private static readonly Lazy<string> _fRiskAssessmentFinalRiskLevel = new Lazy<string>(() => "fldv_FinalRiskLevel");
    public static string fRiskAssessmentFinalRiskLevel => _fRiskAssessmentFinalRiskLevel.Value;

    private static readonly Lazy<string> _fRiskAssessmentRiskTolerability = new Lazy<string>(() => "fldv_RiskTolerability");
    public static string fRiskAssessmentRiskTolerability => _fRiskAssessmentRiskTolerability.Value;

    private static readonly Lazy<string> _fRiskAssessmentAssessmentRationale = new Lazy<string>(() => "fldv_AssessmentRationale");
    public static string fRiskAssessmentAssessmentRationale => _fRiskAssessmentAssessmentRationale.Value;

    /// <summary>
    /// Risk Assessment - Step 5 Implementation Fields
    /// </summary>
    private static readonly Lazy<string> _fRiskAssessmentImplementationStrategy = new Lazy<string>(() => "fldv_ImplementationStrategy");
    public static string fRiskAssessmentImplementationStrategy => _fRiskAssessmentImplementationStrategy.Value;

    private static readonly Lazy<string> _fRiskAssessmentOverallTargetDate = new Lazy<string>(() => "fldd_OverallTargetDate");
    public static string fRiskAssessmentOverallTargetDate => _fRiskAssessmentOverallTargetDate.Value;

    private static readonly Lazy<string> _fRiskAssessmentImplementationNotes = new Lazy<string>(() => "fldv_ImplementationNotes");
    public static string fRiskAssessmentImplementationNotes => _fRiskAssessmentImplementationNotes.Value;

    /// <summary>
    /// Risk Assessment - Progress Tracking Fields
    /// </summary>
    private static readonly Lazy<string> _fRiskAssessmentCompletedSteps = new Lazy<string>(() => "fldv_CompletedSteps");
    public static string fRiskAssessmentCompletedSteps => _fRiskAssessmentCompletedSteps.Value;

    private static readonly Lazy<string> _fRiskAssessmentCompletionPercentage = new Lazy<string>(() => "fldi_CompletionPercentage");
    public static string fRiskAssessmentCompletionPercentage => _fRiskAssessmentCompletionPercentage.Value;
}