// **************************************************
// NEW FIELD CONSTANTS FOR RISK ASSESSMENT WIZARD STEPS 1-5
// Add these to your FieldNames.cs file
// **************************************************

/// <summary>
/// Risk Assessment table (tbld_RiskAssessments) - UPDATED FOR STEPS 1-5
/// </summary>

// Core Assessment Fields (updated sizes)
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

// Step 1 - System Description Fields
private static readonly Lazy<string> _fRiskAssessmentSystemDescription = new Lazy<string>(() => "fldv_SystemDescription");
public static string fRiskAssessmentSystemDescription => _fRiskAssessmentSystemDescription.Value;

private static readonly Lazy<string> _fRiskAssessmentSystemBoundaries = new Lazy<string>(() => "fldv_SystemBoundaries");
public static string fRiskAssessmentSystemBoundaries => _fRiskAssessmentSystemBoundaries.Value;

private static readonly Lazy<string> _fRiskAssessmentSystemPurpose = new Lazy<string>(() => "fldv_SystemPurpose");
public static string fRiskAssessmentSystemPurpose => _fRiskAssessmentSystemPurpose.Value;

private static readonly Lazy<string> _fRiskAssessmentPersonnelFactors = new Lazy<string>(() => "fldv_PersonnelFactors");
public static string fRiskAssessmentPersonnelFactors => _fRiskAssessmentPersonnelFactors.Value;

private static readonly Lazy<string> _fRiskAssessmentEquipmentFactors = new Lazy<string>(() => "fldv_EquipmentFactors");
public static string fRiskAssessmentEquipmentFactors => _fRiskAssessmentEquipmentFactors.Value;

private static readonly Lazy<string> _fRiskAssessmentProcedureFactors = new Lazy<string>(() => "fldv_ProcedureFactors");
public static string fRiskAssessmentProcedureFactors => _fRiskAssessmentProcedureFactors.Value;

private static readonly Lazy<string> _fRiskAssessmentResourceFactors = new Lazy<string>(() => "fldv_ResourceFactors");
public static string fRiskAssessmentResourceFactors => _fRiskAssessmentResourceFactors.Value;

private static readonly Lazy<string> _fRiskAssessmentEnvironmentFactors = new Lazy<string>(() => "fldv_EnvironmentFactors");
public static string fRiskAssessmentEnvironmentFactors => _fRiskAssessmentEnvironmentFactors.Value;

// Step 3 - Risk Analysis Fields
private static readonly Lazy<string> _fRiskAssessmentRiskAnalysisMethod = new Lazy<string>(() => "fldv_RiskAnalysisMethod");
public static string fRiskAssessmentRiskAnalysisMethod => _fRiskAssessmentRiskAnalysisMethod.Value;

private static readonly Lazy<string> _fRiskAssessmentRiskCriteria = new Lazy<string>(() => "fldv_RiskCriteria");
public static string fRiskAssessmentRiskCriteria => _fRiskAssessmentRiskCriteria.Value;

// Step 4 - Risk Assessment Fields
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

// Step 5 - Implementation Fields
private static readonly Lazy<string> _fRiskAssessmentImplementationStrategy = new Lazy<string>(() => "fldv_ImplementationStrategy");
public static string fRiskAssessmentImplementationStrategy => _fRiskAssessmentImplementationStrategy.Value;

private static readonly Lazy<string> _fRiskAssessmentOverallTargetDate = new Lazy<string>(() => "fldd_OverallTargetDate");
public static string fRiskAssessmentOverallTargetDate => _fRiskAssessmentOverallTargetDate.Value;

private static readonly Lazy<string> _fRiskAssessmentImplementationNotes = new Lazy<string>(() => "fldv_ImplementationNotes");
public static string fRiskAssessmentImplementationNotes => _fRiskAssessmentImplementationNotes.Value;

// Progress Tracking Fields
private static readonly Lazy<string> _fRiskAssessmentCompletedSteps = new Lazy<string>(() => "fldv_CompletedSteps");
public static string fRiskAssessmentCompletedSteps => _fRiskAssessmentCompletedSteps.Value;

private static readonly Lazy<string> _fRiskAssessmentCompletionPercentage = new Lazy<string>(() => "fldi_CompletionPercentage");
public static string fRiskAssessmentCompletionPercentage => _fRiskAssessmentCompletionPercentage.Value;

/// <summary>
/// Report Validations - Additional Step Fields for Multi-Hazard Support
/// </summary>

// Step 2 - Hazard Data Fields
private static readonly Lazy<string> _fReportValidationHazardIds = new Lazy<string>(() => "fldv_HazardIds");
public static string fReportValidationHazardIds => _fReportValidationHazardIds.Value;

private static readonly Lazy<string> _fReportValidationHazardDescriptions = new Lazy<string>(() => "fldv_HazardDescriptions");
public static string fReportValidationHazardDescriptions => _fReportValidationHazardDescriptions.Value;

private static readonly Lazy<string> _fReportValidationHazardCategories = new Lazy<string>(() => "fldv_HazardCategories");
public static string fReportValidationHazardCategories => _fReportValidationHazardCategories.Value;

private static readonly Lazy<string> _fReportValidationValidHazardCount = new Lazy<string>(() => "fldi_ValidHazardCount");
public static string fReportValidationValidHazardCount => _fReportValidationValidHazardCount.Value;

// Step 3 - Multi-Hazard Risk Analysis Fields
private static readonly Lazy<string> _fReportValidationHazardAnalysesData = new Lazy<string>(() => "fldv_HazardAnalysesData");
public static string fReportValidationHazardAnalysesData => _fReportValidationHazardAnalysesData.Value;

private static readonly Lazy<string> _fReportValidationHazardWorstOutcomes = new Lazy<string>(() => "fldv_HazardWorstOutcomes");
public static string fReportValidationHazardWorstOutcomes => _fReportValidationHazardWorstOutcomes.Value;

private static readonly Lazy<string> _fReportValidationHazardRootCauses = new Lazy<string>(() => "fldv_HazardRootCauses");
public static string fReportValidationHazardRootCauses => _fReportValidationHazardRootCauses.Value;

// Step 4 - Panel Scoring Fields
private static readonly Lazy<string> _fReportValidationHazardPanelMembers = new Lazy<string>(() => "fldv_HazardPanelMembers");
public static string fReportValidationHazardPanelMembers => _fReportValidationHazardPanelMembers.Value;

private static readonly Lazy<string> _fReportValidationHazardPanelScores = new Lazy<string>(() => "fldv_HazardPanelScores");
public static string fReportValidationHazardPanelScores => _fReportValidationHazardPanelScores.Value;

private static readonly Lazy<string> _fReportValidationHazardAverageScores = new Lazy<string>(() => "fldv_HazardAverageScores");
public static string fReportValidationHazardAverageScores => _fReportValidationHazardAverageScores.Value;

// Step 5 - Mitigation Fields
private static readonly Lazy<string> _fReportValidationSavedMitigationStrategies = new Lazy<string>(() => "fldv_SavedMitigationStrategies");
public static string fReportValidationSavedMitigationStrategies => _fReportValidationSavedMitigationStrategies.Value;

private static readonly Lazy<string> _fReportValidationHazardMitigationStrategyIds = new Lazy<string>(() => "fldv_HazardMitigationStrategyIds");
public static string fReportValidationHazardMitigationStrategyIds => _fReportValidationHazardMitigationStrategyIds.Value;

private static readonly Lazy<string> _fReportValidationMonitoringRequirements = new Lazy<string>(() => "fldv_MonitoringRequirements");
public static string fReportValidationMonitoringRequirements => _fReportValidationMonitoringRequirements.Value;

// Progress Tracking for Report Validations
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