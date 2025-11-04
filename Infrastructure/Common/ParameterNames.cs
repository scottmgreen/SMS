using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;

namespace SMS_Infrastructure.Common;

public static class ParameterNames
{
    /// <summary>
    /// Audit Log Entry parameters
    /// </summary>
    private static readonly Lazy<string> _pmUserID = new Lazy<string>(() => "@pUserID");
    public static string pmUserID => _pmUserID.Value;

    private static readonly Lazy<string> _pmWorkstation = new Lazy<string>(() => "@pWorkstation");
    public static string pmWorkstation => _pmWorkstation.Value;

    private static readonly Lazy<string> _pmEventDateTime = new Lazy<string>(() => "@pEventDateTime");
    public static string pmEventDateTime => _pmEventDateTime.Value;

    private static readonly Lazy<string> _pmMessageType = new Lazy<string>(() => "@pMessageType");
    public static string pmMessageType => _pmMessageType.Value;

    private static readonly Lazy<string> _pmSeverity = new Lazy<string>(() => "@pSeverity");
    public static string pmSeverity => _pmSeverity.Value;

    private static readonly Lazy<string> _pmModule = new Lazy<string>(() => "@pModule");
    public static string pmModule => _pmModule.Value;

    private static readonly Lazy<string> _pmFunction = new Lazy<string>(() => "@pFunction");
    public static string pmFunction => _pmFunction.Value;

    private static readonly Lazy<string> _pmDescription = new Lazy<string>(() => "@pDescription");
    public static string pmDescription => _pmDescription.Value;
    /// <summary>
    /// Common parameters used across all entities
    /// </summary>
    private static readonly Lazy<string> _pmId = new Lazy<string>(() => "@pID");
    public static string pmId => _pmId.Value;

    private static readonly Lazy<string> _pmCreatedBy = new Lazy<string>(() => "@pCreatedBy");
    public static string pmCreatedBy => _pmCreatedBy.Value;

    private static readonly Lazy<string> _pmCreatedDate = new Lazy<string>(() => "@pCreatedDate");
    public static string pmCreatedDate => _pmCreatedDate.Value;

    private static readonly Lazy<string> _pmUpdatedBy = new Lazy<string>(() => "@pUpdatedBy");
    public static string pmUpdatedBy => _pmUpdatedBy.Value;

    private static readonly Lazy<string> _pmUpdatedDate = new Lazy<string>(() => "@pUpdatedDate");
    public static string pmUpdatedDate => _pmUpdatedDate.Value;
    /// <summary>
    /// Airport Shared Dataset parameters
    /// </summary>
    private static readonly Lazy<string> _pmAirportSharedDatasetId = new Lazy<string>(() => "@pAirportSharedDatasetID");
    public static string pmAirportSharedDatasetId => _pmAirportSharedDatasetId.Value;

    private static readonly Lazy<string> _pmAirportSharedDatasetCode = new Lazy<string>(() => "@pCode");
    public static string pmAirportSharedDatasetCode => _pmAirportSharedDatasetCode.Value;

    private static readonly Lazy<string> _pmAirportSharedDatasetReportID = new Lazy<string>(() => "@pReportID");
    public static string pmAirportSharedDatasetReportID => _pmAirportSharedDatasetReportID.Value;

    private static readonly Lazy<string> _pmAirportSharedDatasetHazardCode = new Lazy<string>(() => "@pHazardCode");
    public static string pmAirportSharedDatasetHazardCode => _pmAirportSharedDatasetHazardCode.Value;

    private static readonly Lazy<string> _pmPrivateNarrative = new Lazy<string>(() => "@pPrivateNarrative");
    public static string pmPrivateNarrative => _pmPrivateNarrative.Value;

    private static readonly Lazy<string> _pmSharedNarrative = new Lazy<string>(() => "@pSharedNarrative");
    public static string pmSharedNarrative => _pmSharedNarrative.Value;

    private static readonly Lazy<string> _pmLocationArea = new Lazy<string>(() => "@pLocationArea");
    public static string pmLocationArea => _pmLocationArea.Value;

    private static readonly Lazy<string> _pmLocationSubArea = new Lazy<string>(() => "@pLocationSubArea");
    public static string pmLocationSubArea => _pmLocationSubArea.Value;

    private static readonly Lazy<string> _pmLocationOther = new Lazy<string>(() => "@pLocationOther");
    public static string pmLocationOther => _pmLocationOther.Value;

    private static readonly Lazy<string> _pmWeather = new Lazy<string>(() => "@pWeather");
    public static string pmWeather => _pmWeather.Value;

    private static readonly Lazy<string> _pmTriggeringEvent = new Lazy<string>(() => "@pTriggeringEvent");
    public static string pmTriggeringEvent => _pmTriggeringEvent.Value;

    private static readonly Lazy<string> _pmAircraftInvolved = new Lazy<string>(() => "@pAircraftInvolved");
    public static string pmAircraftInvolved => _pmAircraftInvolved.Value;

    private static readonly Lazy<string> _pmPoweredEquipmentInvolved = new Lazy<string>(() => "@pPoweredEquipmentInvolved");
    public static string pmPoweredEquipmentInvolved => _pmPoweredEquipmentInvolved.Value;

    private static readonly Lazy<string> _pmNonPoweredEquipmentInvolved = new Lazy<string>(() => "@pNonPoweredEquipmentInvolved");
    public static string pmNonPoweredEquipmentInvolved => _pmNonPoweredEquipmentInvolved.Value;

    private static readonly Lazy<string> _pmPedestrianInvolved = new Lazy<string>(() => "@pPedestrianInvolved");
    public static string pmPedestrianInvolved => _pmPedestrianInvolved.Value;

    private static readonly Lazy<string> _pmOtherInvolved = new Lazy<string>(() => "@pOtherInvolved");
    public static string pmOtherInvolved => _pmOtherInvolved.Value;

    private static readonly Lazy<string> _pmOtherDescription = new Lazy<string>(() => "@pOtherDescription");
    public static string pmOtherDescription => _pmOtherDescription.Value;

    private static readonly Lazy<string> _pmPropertyDamage = new Lazy<string>(() => "@pPropertyDamage");
    public static string pmPropertyDamage => _pmPropertyDamage.Value;

    private static readonly Lazy<string> _pmPropertyDamageComments = new Lazy<string>(() => "@pPropertyDamageComments");
    public static string pmPropertyDamageComments => _pmPropertyDamageComments.Value;

    private static readonly Lazy<string> _pmPersonalInjury = new Lazy<string>(() => "@pPersonalInjury");
    public static string pmPersonalInjury => _pmPersonalInjury.Value;

    private static readonly Lazy<string> _pmPersonalInjuryComments = new Lazy<string>(() => "@pPersonalInjuryComments");
    public static string pmPersonalInjuryComments => _pmPersonalInjuryComments.Value;

    private static readonly Lazy<string> _pmFatality = new Lazy<string>(() => "@pFatality");
    public static string pmFatality => _pmFatality.Value;

    private static readonly Lazy<string> _pmFatalityComments = new Lazy<string>(() => "@pFatalityComments");
    public static string pmFatalityComments => _pmFatalityComments.Value;

    private static readonly Lazy<string> _pmOtherIssues = new Lazy<string>(() => "@pOtherIssues");
    public static string pmOtherIssues => _pmOtherIssues.Value;

    private static readonly Lazy<string> _pmOtherIssuesDescription = new Lazy<string>(() => "@pOtherIssuesDescription");
    public static string pmOtherIssuesDescription => _pmOtherIssuesDescription.Value;

    private static readonly Lazy<string> _pmAirlineCompanyOperator = new Lazy<string>(() => "@pAirlineCompanyOperator");
    public static string pmAirlineCompanyOperator => _pmAirlineCompanyOperator.Value;

    private static readonly Lazy<string> _pmOperatorsAuthorized = new Lazy<string>(() => "@pOperatorsAuthorized");
    public static string pmOperatorsAuthorized => _pmOperatorsAuthorized.Value;

    private static readonly Lazy<string> _pmFlightDelay = new Lazy<string>(() => "@pFlightDelay");
    public static string pmFlightDelay => _pmFlightDelay.Value;

    private static readonly Lazy<string> _pmFlightDelayDetails = new Lazy<string>(() => "@pFlightDelayDetails");
    public static string pmFlightDelayDetails => _pmFlightDelayDetails.Value;

    private static readonly Lazy<string> _pmEquipmentRemovedFromService = new Lazy<string>(() => "@pEquipmentRemovedFromService");
    public static string pmEquipmentRemovedFromService => _pmEquipmentRemovedFromService.Value;

    private static readonly Lazy<string> _pmEquipmentRemovalDetails = new Lazy<string>(() => "@pEquipmentRemovalDetails");
    public static string pmEquipmentRemovalDetails => _pmEquipmentRemovalDetails.Value;

    private static readonly Lazy<string> _pmPoliceReport = new Lazy<string>(() => "@pPoliceReport");
    public static string pmPoliceReport => _pmPoliceReport.Value;

    private static readonly Lazy<string> _pmPoliceReportDetails = new Lazy<string>(() => "@pPoliceReportDetails");
    public static string pmPoliceReportDetails => _pmPoliceReportDetails.Value;

    private static readonly Lazy<string> _pmContributingFactors = new Lazy<string>(() => "@pContributingFactors");
    public static string pmContributingFactors => _pmContributingFactors.Value;

    private static readonly Lazy<string> _pmFactorsOtherDescription = new Lazy<string>(() => "@pFactorsOtherDescription");
    public static string pmFactorsOtherDescription => _pmFactorsOtherDescription.Value;
    /// <summary>
    /// Hazard parameters
    /// </summary>
    private static readonly Lazy<string> _pmHazardId = new Lazy<string>(() => "@pHazardID");
    public static string pmHazardId => _pmHazardId.Value;

    private static readonly Lazy<string> _pmHazardCode = new Lazy<string>(() => "@pHazardCode");
    public static string pmHazardCode => _pmHazardCode.Value;

    private static readonly Lazy<string> _pmHazardName = new Lazy<string>(() => "@pHazardName");
    public static string pmHazardName => _pmHazardName.Value;

    private static readonly Lazy<string> _pmHazardDescription = new Lazy<string>(() => "@pHazardDescription");
    public static string pmHazardDescription => _pmHazardDescription.Value;

    private static readonly Lazy<string> _pmHazardReportCode = new Lazy<string>(() => "@pHazardReportCode");
    public static string pmHazardReportCode => _pmHazardReportCode.Value;

    private static readonly Lazy<string> _pmHazardScoringPanelCode = new Lazy<string>(() => "@pHazardScoringPanelCode");
    public static string pmHazardScoringPanelCode => _pmHazardScoringPanelCode.Value;

    private static readonly Lazy<string> _pmHazardAverageScore = new Lazy<string>(() => "@pHazardAverageScore");
    public static string pmHazardAverageScore => _pmHazardAverageScore.Value;

    /// <summary>
    /// Report parameters
    /// </summary>
    private static readonly Lazy<string> _pmReportId = new Lazy<string>(() => "@pReportID");
    public static string pmReportId => _pmReportId.Value;

    private static readonly Lazy<string> _pmReportCode = new Lazy<string>(() => "@pReportCode");
    public static string pmReportCode => _pmReportCode.Value;

    private static readonly Lazy<string> _pmReportName = new Lazy<string>(() => "@pReportName");
    public static string pmReportName => _pmReportName.Value;

    private static readonly Lazy<string> _pmReportDescription = new Lazy<string>(() => "@pReportDescription");
    public static string pmReportDescription => _pmReportDescription.Value;

    private static readonly Lazy<string> _pmReportStatus = new Lazy<string>(() => "@pReportStatus");
    public static string pmReportStatus => _pmReportStatus.Value;

    private static readonly Lazy<string> _pmReportStage = new Lazy<string>(() => "@pReportStage");
    public static string pmReportStage => _pmReportStage.Value;

    /// <summary>
    /// Investigation parameters
    /// </summary>
    private static readonly Lazy<string> _pmInvestigationId = new Lazy<string>(() => "@pInvestigationID");
    public static string pmInvestigationId => _pmInvestigationId.Value;

    private static readonly Lazy<string> _pmInvestigationCode = new Lazy<string>(() => "@pInvestigationCode");
    public static string pmInvestigationCode => _pmInvestigationCode.Value;

    private static readonly Lazy<string> _pmInvestigationReportCode = new Lazy<string>(() => "@pInvestigationReportCode");
    public static string pmInvestigationReportCode => _pmInvestigationReportCode.Value;

    private static readonly Lazy<string> _pmInvestigationNotes = new Lazy<string>(() => "@pInvestigationNotes");
    public static string pmInvestigationNotes => _pmInvestigationNotes.Value;

    /// <summary>
    /// Interview parameters
    /// </summary>
    private static readonly Lazy<string> _pmInterviewId = new Lazy<string>(() => "@pInterviewID");
    public static string pmInterviewId => _pmInterviewId.Value;

    private static readonly Lazy<string> _pmInterviewCode = new Lazy<string>(() => "@pInterviewCode");
    public static string pmInterviewCode => _pmInterviewCode.Value;

    private static readonly Lazy<string> _pmInterviewInvestigationCode = new Lazy<string>(() => "@pInterviewInvestigationCode");
    public static string pmInterviewInvestigationCode => _pmInterviewInvestigationCode.Value;

    private static readonly Lazy<string> _pmInterviewSMSInvestigatorCode = new Lazy<string>(() => "@pInterviewSMSInvestigatorCode");
    public static string pmInterviewSMSInvestigatorCode => _pmInterviewSMSInvestigatorCode.Value;

    private static readonly Lazy<string> _pmInterviewPersonInterviewed = new Lazy<string>(() => "@pInterviewPersonInterviewed");
    public static string pmInterviewPersonInterviewed => _pmInterviewPersonInterviewed.Value;

    private static readonly Lazy<string> _pmInterviewPersonInterviewedNotes = new Lazy<string>(() => "@pInterviewPersonInterviewedNotes");
    public static string pmInterviewPersonInterviewedNotes => _pmInterviewPersonInterviewedNotes.Value;

    private static readonly Lazy<string> _pmInterviewInvestigatorNotes = new Lazy<string>(() => "@pInterviewInvestigatorNotes");
    public static string pmInterviewInvestigatorNotes => _pmInterviewInvestigatorNotes.Value;

    /// <summary>
    /// Risk Analysis parameters
    /// </summary>
    private static readonly Lazy<string> _pmRiskAnalysisId = new Lazy<string>(() => "@pRiskAnalysisID");
    public static string pmRiskAnalysisId => _pmRiskAnalysisId.Value;

    private static readonly Lazy<string> _pmRiskAnalysisCode = new Lazy<string>(() => "@pRiskAnalysisCode");
    public static string pmRiskAnalysisCode => _pmRiskAnalysisCode.Value;

    private static readonly Lazy<string> _pmRiskAnalysisName = new Lazy<string>(() => "@pRiskAnalysisName");
    public static string pmRiskAnalysisName => _pmRiskAnalysisName.Value;

    private static readonly Lazy<string> _pmRiskAnalysisDescription = new Lazy<string>(() => "@pRiskAnalysisDescription");
    public static string pmRiskAnalysisDescription => _pmRiskAnalysisDescription.Value;

    private static readonly Lazy<string> _pmRiskAnalysisHazardCode = new Lazy<string>(() => "@pRiskAnalysisHazardCode");
    public static string pmRiskAnalysisHazardCode => _pmRiskAnalysisHazardCode.Value;

    private static readonly Lazy<string> _pmRiskAnalysisStatus = new Lazy<string>(() => "@pRiskAnalysisStatus");
    public static string pmRiskAnalysisStatus => _pmRiskAnalysisStatus.Value;

    private static readonly Lazy<string> _pmRiskAnalysisStage = new Lazy<string>(() => "@pRiskAnalysisStage");
    public static string pmRiskAnalysisStage => _pmRiskAnalysisStage.Value;

    private static readonly Lazy<string> _pmRiskAnalysisWorstCredibleOutcome = new Lazy<string>(() => "@pRiskAnalysisWorstCredibleOutcome");
    public static string pmRiskAnalysisWorstCredibleOutcome => _pmRiskAnalysisWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _pmRiskAnalysisRootCause = new Lazy<string>(() => "@pRiskAnalysisRootCause");
    public static string pmRiskAnalysisRootCause => _pmRiskAnalysisRootCause.Value;

    /// <summary>
    /// Risk Assessment parameters
    /// </summary>
    private static readonly Lazy<string> _pmRiskAssessmentId = new Lazy<string>(() => "@pRiskAssessmentID");
    public static string pmRiskAssessmentId => _pmRiskAssessmentId.Value;

    private static readonly Lazy<string> _pmRiskAssessmentCode = new Lazy<string>(() => "@pRiskAssessmentCode");
    public static string pmRiskAssessmentCode => _pmRiskAssessmentCode.Value;

    private static readonly Lazy<string> _pmRiskAssessmentName = new Lazy<string>(() => "@pRiskAssessmentName");
    public static string pmRiskAssessmentName => _pmRiskAssessmentName.Value;

    private static readonly Lazy<string> _pmRiskAssessmentDescription = new Lazy<string>(() => "@pRiskAssessmentDescription");
    public static string pmRiskAssessmentDescription => _pmRiskAssessmentDescription.Value;

    private static readonly Lazy<string> _pmRiskAssessmentHazardCode = new Lazy<string>(() => "@pRiskAssessmentHazardCode");
    public static string pmRiskAssessmentHazardCode => _pmRiskAssessmentHazardCode.Value;

    private static readonly Lazy<string> _pmRiskAssessmentType = new Lazy<string>(() => "@pRiskAssessmentType");
    public static string pmRiskAssessmentType => _pmRiskAssessmentType.Value;

    private static readonly Lazy<string> _pmRiskAssessmentStatus = new Lazy<string>(() => "@pRiskAssessmentStatus");
    public static string pmRiskAssessmentStatus => _pmRiskAssessmentStatus.Value;

    private static readonly Lazy<string> _pmRiskAssessmentStage = new Lazy<string>(() => "@pRiskAssessmentStage");
    public static string pmRiskAssessmentStage => _pmRiskAssessmentStage.Value;

    /// <summary>
    /// Mitigation parameters
    /// </summary>
    private static readonly Lazy<string> _pmMitigationId = new Lazy<string>(() => "@pMitigationID");
    public static string pmMitigationId => _pmMitigationId.Value;

    private static readonly Lazy<string> _pmMitigationCode = new Lazy<string>(() => "@pMitigationCode");
    public static string pmMitigationCode => _pmMitigationCode.Value;

    private static readonly Lazy<string> _pmMitigationHazardCode = new Lazy<string>(() => "@pMitigationHazardCode");
    public static string pmMitigationHazardCode => _pmMitigationHazardCode.Value;

    /// <summary>
    /// Mitigation Assignment parameters
    /// </summary>
    private static readonly Lazy<string> _pmMitigationAssignmentId = new Lazy<string>(() => "@pMitigationAssignmentID");
    public static string pmMitigationAssignmentId => _pmMitigationAssignmentId.Value;

    private static readonly Lazy<string> _pmMitigationAssignmentCode = new Lazy<string>(() => "@pMitigationAssignmentCode");
    public static string pmMitigationAssignmentCode => _pmMitigationAssignmentCode.Value;

    private static readonly Lazy<string> _pmMitigationAssignmentMitigationCode = new Lazy<string>(() => "@pMitigationAssignmentMitigationCode");
    public static string pmMitigationAssignmentMitigationCode => _pmMitigationAssignmentMitigationCode.Value;

    private static readonly Lazy<string> _pmMitigationAssignmentDepartmentCode = new Lazy<string>(() => "@pMitigationAssignmentDepartmentCode");
    public static string pmMitigationAssignmentDepartmentCode => _pmMitigationAssignmentDepartmentCode.Value;

    /// <summary>
    /// Report Validation parameters
    /// </summary>
    private static readonly Lazy<string> _pmReportValidationId = new Lazy<string>(() => "@pReportValidationID");
    public static string pmReportValidationId => _pmReportValidationId.Value;

    private static readonly Lazy<string> _pmReportValidationCode = new Lazy<string>(() => "@pReportValidationCode");
    public static string pmReportValidationCode => _pmReportValidationCode.Value;

    private static readonly Lazy<string> _pmReportValidationReportCode = new Lazy<string>(() => "@pReportValidationReportCode");
    public static string pmReportValidationReportCode => _pmReportValidationReportCode.Value;

    private static readonly Lazy<string> _pmReportValidationDecision = new Lazy<string>(() => "@pReportValidationDecision");
    public static string pmReportValidationDecision => _pmReportValidationDecision.Value;

    private static readonly Lazy<string> _pmReportValidationStatus = new Lazy<string>(() => "@pReportValidationStatus");
    public static string pmReportValidationStatus => _pmReportValidationStatus.Value;

    private static readonly Lazy<string> _pmReportValidationStage = new Lazy<string>(() => "@pReportValidationStage");
    public static string pmReportValidationStage => _pmReportValidationStage.Value;

    /// <summary>
    /// Scoring Panel parameters
    /// </summary>
    private static readonly Lazy<string> _pmScoringPanelId = new Lazy<string>(() => "@pScoringPanelID");
    public static string pmScoringPanelId => _pmScoringPanelId.Value;

    private static readonly Lazy<string> _pmScoringPanelCode = new Lazy<string>(() => "@pScoringPanelCode");
    public static string pmScoringPanelCode => _pmScoringPanelCode.Value;

    private static readonly Lazy<string> _pmScoringPanelHazardCode = new Lazy<string>(() => "@pScoringPanelHazardCode");
    public static string pmScoringPanelHazardCode => _pmScoringPanelHazardCode.Value;

    private static readonly Lazy<string> _pmScoringPanelSMSUserCode = new Lazy<string>(() => "@pScoringPanelSMSUserCode");
    public static string pmScoringPanelSMSUserCode => _pmScoringPanelSMSUserCode.Value;

    private static readonly Lazy<string> _pmScoringPanelLikelihood = new Lazy<string>(() => "@pScoringPanelLikelihood");
    public static string pmScoringPanelLikelihood => _pmScoringPanelLikelihood.Value;

    private static readonly Lazy<string> _pmScoringPanelSeverity = new Lazy<string>(() => "@pScoringPanelSeverity");
    public static string pmScoringPanelSeverity => _pmScoringPanelSeverity.Value;

    private static readonly Lazy<string> _pmScoringPanelScore = new Lazy<string>(() => "@pScoringPanelScore");
    public static string pmScoringPanelScore => _pmScoringPanelScore.Value;
}