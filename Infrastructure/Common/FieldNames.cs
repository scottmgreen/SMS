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

    private static readonly Lazy<string> _fSMSApplicationUserApplicationRole = new Lazy<string>(() => "fldv_ApplicationRole");
    public static string fSMSApplicationUserApplicationRole => _fSMSApplicationUserApplicationRole.Value;

    private static readonly Lazy<string> _fSMSApplicationUserPermissionLevel = new Lazy<string>(() => "fldv_PermissionLevel");
    public static string fSMSApplicationUserPermissionLevel => _fSMSApplicationUserPermissionLevel.Value;

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

    private static readonly Lazy<string> _fSMSStakeholderUserStakeholderType = new Lazy<string>(() => "fldv_StakeholderType");
    public static string fSMSStakeholderUserStakeholderType => _fSMSStakeholderUserStakeholderType.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserOrganization = new Lazy<string>(() => "fldv_Organization");
    public static string fSMSStakeholderUserOrganization => _fSMSStakeholderUserOrganization.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserAccessLevel = new Lazy<string>(() => "fldv_AccessLevel");
    public static string fSMSStakeholderUserAccessLevel => _fSMSStakeholderUserAccessLevel.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserIsActive = new Lazy<string>(() => "fldb_IsActive");
    public static string fSMSStakeholderUserIsActive => _fSMSStakeholderUserIsActive.Value;

    private static readonly Lazy<string> _fSMSStakeholderUserLastLoginDate = new Lazy<string>(() => "fldd_LastLoginDate");
    public static string fSMSStakeholderUserLastLoginDate => _fSMSStakeholderUserLastLoginDate.Value;

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

    private static readonly Lazy<string> _fHazardScoringPanelCode = new Lazy<string>(() => "fldv_ScoringPanelCode");
    public static string fHazardScoringPanelCode => _fHazardScoringPanelCode.Value;

    private static readonly Lazy<string> _fHazardAverageScore = new Lazy<string>(() => "fldv_AverageScore");
    public static string fHazardAverageScore => _fHazardAverageScore.Value;

    /// <summary>
    /// Interviews table (tbld_Interviews)
    /// </summary>
    private static readonly Lazy<string> _fInterviewCode = new Lazy<string>(() => "fldv_Code");
    public static string fInterviewCode => _fInterviewCode.Value;

    private static readonly Lazy<string> _fInterviewInvestigationCode = new Lazy<string>(() => "fldv_InvestigationCode");
    public static string fInterviewInvestigationCode => _fInterviewInvestigationCode.Value;

    private static readonly Lazy<string> _fInterviewSMSInvestigatorCode = new Lazy<string>(() => "fldv_SMSInvestigatorCode");
    public static string fInterviewSMSInvestigatorCode => _fInterviewSMSInvestigatorCode.Value;

    private static readonly Lazy<string> _fInterviewPersonInterviewed = new Lazy<string>(() => "fldv_PersonInterviewed");
    public static string fInterviewPersonInterviewed => _fInterviewPersonInterviewed.Value;

    private static readonly Lazy<string> _fInterviewPersonInterviewedNotes = new Lazy<string>(() => "fldv_PersonInterviewedNotes");
    public static string fInterviewPersonInterviewedNotes => _fInterviewPersonInterviewedNotes.Value;

    private static readonly Lazy<string> _fInterviewInvestigatorNotes = new Lazy<string>(() => "fldv_InvestigatorNotes");
    public static string fInterviewInvestigatorNotes => _fInterviewInvestigatorNotes.Value;

    /// <summary>
    /// Investigations table (tbld_Investigations)
    /// </summary>
    private static readonly Lazy<string> _fInvestigationCode = new Lazy<string>(() => "fldv_Code");
    public static string fInvestigationCode => _fInvestigationCode.Value;

    private static readonly Lazy<string> _fInvestigationReportCode = new Lazy<string>(() => "fldv_ReportCode");
    public static string fInvestigationReportCode => _fInvestigationReportCode.Value;

    private static readonly Lazy<string> _fInvestigationNotes = new Lazy<string>(() => "fldv_InvestigationNotes");
    public static string fInvestigationNotes => _fInvestigationNotes.Value;

    /// <summary>
    /// Mitigation Assignments table (tbld_MitigationAssignments)
    /// </summary>
    private static readonly Lazy<string> _fMitigationAssignmentCode = new Lazy<string>(() => "fldv_Code");
    public static string fMitigationAssignmentCode => _fMitigationAssignmentCode.Value;

    private static readonly Lazy<string> _fMitigationAssignmentMitigationCode = new Lazy<string>(() => "fldv_MitigationCode");
    public static string fMitigationAssignmentMitigationCode => _fMitigationAssignmentMitigationCode.Value;

    private static readonly Lazy<string> _fMitigationAssignmentDepartmentCode = new Lazy<string>(() => "fldv_DepartmentCode");
    public static string fMitigationAssignmentDepartmentCode => _fMitigationAssignmentDepartmentCode.Value;

    /// <summary>
    /// Mitigations table (tbld_Mitigations)
    /// </summary>
    private static readonly Lazy<string> _fMitigationCode = new Lazy<string>(() => "fldv_Code");
    public static string fMitigationCode => _fMitigationCode.Value;

    private static readonly Lazy<string> _fMitigationHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fMitigationHazardCode => _fMitigationHazardCode.Value;

    /// <summary>
    /// Reports table (tbld_Reports)
    /// </summary>
    private static readonly Lazy<string> _fReportCode = new Lazy<string>(() => "fldv_Code");
    public static string fReportCode => _fReportCode.Value;

    private static readonly Lazy<string> _fReportName = new Lazy<string>(() => "fldv_Name");
    public static string fReportName => _fReportName.Value;

    private static readonly Lazy<string> _fReportDescription = new Lazy<string>(() => "fldc_Description");
    public static string fReportDescription => _fReportDescription.Value;

    private static readonly Lazy<string> _fReportStatus = new Lazy<string>(() => "fldv_Status");
    public static string fReportStatus => _fReportStatus.Value;

    private static readonly Lazy<string> _fReportStage = new Lazy<string>(() => "fldv_Stage");
    public static string fReportStage => _fReportStage.Value;

    /// <summary>
    /// Report Validations table (tbld_ReportValidations)
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

    private static readonly Lazy<string> _fRiskAnalysisStatus = new Lazy<string>(() => "dldv_Status");
    public static string fRiskAnalysisStatus => _fRiskAnalysisStatus.Value;

    private static readonly Lazy<string> _fRiskAnalysisStage = new Lazy<string>(() => "fldv_Stage");
    public static string fRiskAnalysisStage => _fRiskAnalysisStage.Value;

    private static readonly Lazy<string> _fRiskAnalysisWorstCredibleOutcome = new Lazy<string>(() => "fldv_WorstCredibleOutcome");
    public static string fRiskAnalysisWorstCredibleOutcome => _fRiskAnalysisWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _fRiskAnalysisRootCause = new Lazy<string>(() => "fldv_RootCause");
    public static string fRiskAnalysisRootCause => _fRiskAnalysisRootCause.Value;

    /// <summary>
    /// Risk Assessments table (tbld_RiskAssessments)
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

    /// <summary>
    /// Scoring Panel table (tbld_ScoringPanel)
    /// </summary>
    private static readonly Lazy<string> _fScoringPanelCode = new Lazy<string>(() => "fldv_Code");
    public static string fScoringPanelCode => _fScoringPanelCode.Value;

    private static readonly Lazy<string> _fScoringPanelHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fScoringPanelHazardCode => _fScoringPanelHazardCode.Value;

    private static readonly Lazy<string> _fScoringPanelSMSUserCode = new Lazy<string>(() => "fldv_SMSUserCode");
    public static string fScoringPanelSMSUserCode => _fScoringPanelSMSUserCode.Value;

    private static readonly Lazy<string> _fScoringPanelLikelihood = new Lazy<string>(() => "fldv_Likelyhood");
    public static string fScoringPanelLikelihood => _fScoringPanelLikelihood.Value;

    private static readonly Lazy<string> _fScoringPanelSeverity = new Lazy<string>(() => "fldv_Severity");
    public static string fScoringPanelSeverity => _fScoringPanelSeverity.Value;

    private static readonly Lazy<string> _fScoringPanelScore = new Lazy<string>(() => "fldv_Score");
    public static string fScoringPanelScore => _fScoringPanelScore.Value;
}