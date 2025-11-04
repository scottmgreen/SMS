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
    /// Airport Shared Dataset field names - Using "fldv_", "fldd_", "fldi_" conventions
    /// </summary>
    public static readonly string fAirportSharedDatasetCode = "fldv_Code";
    public static readonly string fAirportSharedDatasetReportCode = "fldv_ReportCode";
    public static readonly string fAirportSharedDatasetHazardCode = "fldv_HazardCode";
    public static readonly string fPrivateNarrative = "fldv_PrivateNarrative";
    public static readonly string fSharedNarrative = "fldv_SharedNarrative";
    public static readonly string fLocationArea = "fldv_LocationArea";
    public static readonly string fLocationSubArea = "fldv_LocationSubArea";
    public static readonly string fLocationOther = "fldv_LocationOther";
    public static readonly string fWeather = "fldv_Weather";
    public static readonly string fTriggeringEvent = "fldv_TriggeringEvent";
    public static readonly string fAircraftInvolved = "fldb_AircraftInvolved";
    public static readonly string fPoweredEquipmentInvolved = "fldb_PoweredEquipmentInvolved";
    public static readonly string fNonPoweredEquipmentInvolved = "fldb_NonPoweredEquipmentInvolved";
    public static readonly string fPedestrianInvolved = "fldb_PedestrianInvolved";
    public static readonly string fOtherInvolved = "fldb_OtherInvolved";
    public static readonly string fOtherDescription = "fldv_OtherDescription";
    public static readonly string fPropertyDamage = "fldb_PropertyDamage";
    public static readonly string fPropertyDamageComments = "fldv_PropertyDamageComments";
    public static readonly string fPersonalInjury = "fldb_PersonalInjury";
    public static readonly string fPersonalInjuryComments = "fldv_PersonalInjuryComments";
    public static readonly string fFatality = "fldb_Fatality";
    public static readonly string fFatalityComments = "fldv_FatalityComments";
    public static readonly string fOtherIssues = "fldb_OtherIssues";
    public static readonly string fOtherIssuesDescription = "fldv_OtherIssuesDescription";
    public static readonly string fAirlineCompanyOperator = "fldv_AirlineCompanyOperator";
    public static readonly string fOperatorsAuthorized = "fldv_OperatorsAuthorized";
    public static readonly string fFlightDelay = "fldv_FlightDelay";
    public static readonly string fFlightDelayDetails = "fldv_FlightDelayDetails";
    public static readonly string fEquipmentRemovedFromService = "fldv_EquipmentRemovedFromService";
    public static readonly string fEquipmentRemovalDetails = "fldv_EquipmentRemovalDetails";
    public static readonly string fPoliceReport = "fldv_PoliceReport";
    public static readonly string fPoliceReportDetails = "fldv_PoliceReportDetails";
    public static readonly string fContributingFactors = "fldv_ContributingFactors";
    public static readonly string fFactorsOtherDescription = "fldv_FactorsOtherDescription";
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