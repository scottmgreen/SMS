using SMS_Domain.Entities;
using SMS_Domain.Interfaces;

using System.Data;
using System.Reflection;

namespace SMS_Infrastructure.Common;

public static class Mappers
{
    #region Generic Helper Methods

    public static List<T> LoadCollection<T>(this SqlDataReader reader) where T : new()
    {
        List<T> collection = new List<T>();

        while (reader.Read())
        {
            T entity = reader.CreateEntityFromReader<T>();
            collection.Add(entity);
        }

        return collection;
    }

    private static T CreateEntityFromReader<T>(this SqlDataReader reader) where T : new()
    {
        T entity = new T();
        reader.PopulateEntityFromReader(entity);
        return entity;
    }

    private static void PopulateEntityFromReader<T>(this SqlDataReader reader, T entity)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            object columnValue = reader.GetValue(i);

            PropertyInfo property = typeof(T).GetProperty(columnName);
            if (property != null && columnValue != DBNull.Value)
            {
                property.SetValue(entity, columnValue, null);
            }
        }
    }

    public static T GetValue<T>(this IDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value == DBNull.Value ? default : (T)value;
    }

    #endregion

    #region SMS Entity Mappers

    /// <summary>
    /// Maps SqlDataReader to AirportSharedDataset entity using updated field names
    /// </summary>
    public static AirportSharedDataset MapToAirportSharedDataset(SqlDataReader reader)
    {
        AirportSharedDatasetID datasetID = new(reader.GetValue<string>(FieldNames.fAirportSharedDatasetCode).ToString());
        AirportSharedDataset dataset = new(datasetID);

        dataset.Code = reader.GetValue<string>(FieldNames.fAirportSharedDatasetCode) ?? string.Empty;
        dataset.ReportID = reader.GetValue<string>(FieldNames.fAirportSharedDatasetReportCode).ToString();
        dataset.HazardCode = reader.GetValue<string>(FieldNames.fAirportSharedDatasetHazardCode);
        dataset.PrivateNarrative = reader.GetValue<string>(FieldNames.fPrivateNarrative);
        dataset.SharedNarrative = reader.GetValue<string>(FieldNames.fSharedNarrative);
        dataset.LocationArea = reader.GetValue<string>(FieldNames.fLocationArea);
        dataset.LocationSubArea = reader.GetValue<string>(FieldNames.fLocationSubArea);
        dataset.LocationOther = reader.GetValue<string>(FieldNames.fLocationOther);
        dataset.Weather = reader.GetValue<string>(FieldNames.fWeather);
        dataset.TriggeringEvent = reader.GetValue<string>(FieldNames.fTriggeringEvent);
        dataset.AircraftInvolved = reader.GetValue<bool>(FieldNames.fAircraftInvolved);
        dataset.PoweredEquipmentInvolved = reader.GetValue<bool>(FieldNames.fPoweredEquipmentInvolved);
        dataset.NonPoweredEquipmentInvolved = reader.GetValue<bool>(FieldNames.fNonPoweredEquipmentInvolved);
        dataset.PedestrianInvolved = reader.GetValue<bool>(FieldNames.fPedestrianInvolved);
        dataset.OtherInvolved = reader.GetValue<bool>(FieldNames.fOtherInvolved);
        dataset.OtherDescription = reader.GetValue<string>(FieldNames.fOtherDescription);
        dataset.PropertyDamage = reader.GetValue<bool>(FieldNames.fPropertyDamage);
        dataset.PropertyDamageComments = reader.GetValue<string>(FieldNames.fPropertyDamageComments);
        dataset.PersonalInjury = reader.GetValue<bool>(FieldNames.fPersonalInjury);
        dataset.PersonalInjuryComments = reader.GetValue<string>(FieldNames.fPersonalInjuryComments);
        dataset.Fatality = reader.GetValue<bool>(FieldNames.fFatality);
        dataset.FatalityComments = reader.GetValue<string>(FieldNames.fFatalityComments);
        dataset.OtherIssues = reader.GetValue<bool>(FieldNames.fOtherIssues);
        dataset.OtherIssuesDescription = reader.GetValue<string>(FieldNames.fOtherIssuesDescription);
        dataset.AirlineCompanyOperator = reader.GetValue<string>(FieldNames.fAirlineCompanyOperator);
        dataset.OperatorsAuthorized = reader.GetValue<string>(FieldNames.fOperatorsAuthorized);
        dataset.FlightDelay = reader.GetValue<string>(FieldNames.fFlightDelay);
        dataset.FlightDelayDetails = reader.GetValue<string>(FieldNames.fFlightDelayDetails);
        dataset.EquipmentRemovedFromService = reader.GetValue<string>(FieldNames.fEquipmentRemovedFromService);
        dataset.EquipmentRemovalDetails = reader.GetValue<string>(FieldNames.fEquipmentRemovalDetails);
        dataset.PoliceReport = reader.GetValue<string>(FieldNames.fPoliceReport);
        dataset.PoliceReportDetails = reader.GetValue<string>(FieldNames.fPoliceReportDetails);
        dataset.ContributingFactors = reader.GetValue<string>(FieldNames.fContributingFactors);
        dataset.FactorsOtherDescription = reader.GetValue<string>(FieldNames.fFactorsOtherDescription);

        return dataset;
    }
    /// <summary>
    /// Maps SqlDataReader to Hazard entity
    /// </summary>
    public static Hazard MapToHazard(SqlDataReader reader)
    {
        HazardID hazardID = new(reader.GetValue<string>(FieldNames.fHazardCode));
        Hazard hazard = new(hazardID);

        hazard.Code = reader.GetValue<string>(FieldNames.fHazardCode) ?? string.Empty;
        hazard.Name = reader.GetValue<string>(FieldNames.fHazardName);
        hazard.Description = reader.GetValue<string>(FieldNames.fHazardDescription);
        hazard.ReportCode = reader.GetValue<string>(FieldNames.fHazardReportCode) ?? string.Empty;
        hazard.ScoringPanelCode = reader.GetValue<string>(FieldNames.fHazardScoringPanelCode);
        hazard.AverageScore = reader.GetValue<string>(FieldNames.fHazardAverageScore);

        return hazard;
    }

    /// <summary>
    /// Maps SqlDataReader to Report entity
    /// </summary>
    public static Report MapToReport(SqlDataReader reader)
    {
        ReportID reportID = new(reader.GetValue<string>(FieldNames.fReportCode).ToString());
        Report report = new(reportID);

        report.Code = reader.GetValue<string>(FieldNames.fReportCode) ?? string.Empty;
        report.Name = reader.GetValue<string>(FieldNames.fReportName);
        report.Description = reader.GetValue<string>(FieldNames.fReportDescription);
        report.Status = reader.GetValue<string>(FieldNames.fReportStatus);
        report.Stage = reader.GetValue<string>(FieldNames.fReportStage);

        return report;
    }

    /// <summary>
    /// Maps SqlDataReader to Investigation entity
    /// </summary>
    public static Investigation MapToInvestigation(SqlDataReader reader)
    {
        InvestigationID investigationID = new(reader.GetValue<string>(FieldNames.fInvestigationCode).ToString());
        Investigation investigation = new(investigationID);

        investigation.Code = reader.GetValue<string>(FieldNames.fInvestigationCode);
        investigation.ReportCode = reader.GetValue<string>(FieldNames.fInvestigationReportCode);
        investigation.InvestigationNotes = reader.GetValue<string>(FieldNames.fInvestigationNotes);

        return investigation;
    }

    /// <summary>
    /// Maps SqlDataReader to Interview entity
    /// </summary>
    public static Interview MapToInterview(SqlDataReader reader)
    {
        InterviewID interviewID = new(reader.GetValue<string>(FieldNames.fInterviewCode).ToString());
        Interview interview = new(interviewID);

        interview.Code = reader.GetValue<string>(FieldNames.fInterviewCode);
        interview.InvestigationCode = reader.GetValue<string>(FieldNames.fInterviewInvestigationCode);
        interview.SMSInvestigatorCode = reader.GetValue<string>(FieldNames.fInterviewSMSInvestigatorCode);
        interview.PersonInterviewed = reader.GetValue<string>(FieldNames.fInterviewPersonInterviewed);
        interview.PersonInterviewedNotes = reader.GetValue<string>(FieldNames.fInterviewPersonInterviewedNotes);
        interview.InvestigatorNotes = reader.GetValue<string>(FieldNames.fInterviewInvestigatorNotes);

        return interview;
    }

    /// <summary>
    /// Maps SqlDataReader to RiskAnalysis entity
    /// </summary>
    public static RiskAnalysis MapToRiskAnalysis(SqlDataReader reader)
    {
        RiskAnalysisID riskAnalysisID = new(reader.GetValue<string>(FieldNames.fRiskAnalysisCode).ToString());
        RiskAnalysis riskAnalysis = new(riskAnalysisID);

        riskAnalysis.Code = reader.GetValue<string>(FieldNames.fRiskAnalysisCode);
        riskAnalysis.Name = reader.GetValue<string>(FieldNames.fRiskAnalysisName);
        riskAnalysis.Description = reader.GetValue<string>(FieldNames.fRiskAnalysisDescription);
        riskAnalysis.HazardCode = reader.GetValue<string>(FieldNames.fRiskAnalysisHazardCode);
        riskAnalysis.Status = reader.GetValue<string>(FieldNames.fRiskAnalysisStatus);
        riskAnalysis.Stage = reader.GetValue<string>(FieldNames.fRiskAnalysisStage);
        riskAnalysis.WorstCredibleOutcome = reader.GetValue<string>(FieldNames.fRiskAnalysisWorstCredibleOutcome);
        riskAnalysis.RootCause = reader.GetValue<string>(FieldNames.fRiskAnalysisRootCause);

        return riskAnalysis;
    }

    /// <summary>
    /// Maps SqlDataReader to RiskAssessment entity
    /// </summary>
    public static RiskAssessment MapToRiskAssessment(SqlDataReader reader)
    {
        RiskAssessmentID riskAssessmentID = new(reader.GetValue<string>(FieldNames.fRiskAssessmentCode).ToString());
        RiskAssessment riskAssessment = new(riskAssessmentID);

        riskAssessment.Code = reader.GetValue<string>(FieldNames.fRiskAssessmentCode);
        riskAssessment.Name = reader.GetValue<string>(FieldNames.fRiskAssessmentName);
        riskAssessment.Description = reader.GetValue<string>(FieldNames.fRiskAssessmentDescription);
        riskAssessment.HazardCode = reader.GetValue<string>(FieldNames.fRiskAssessmentHazardCode);
        riskAssessment.AssessmentType = reader.GetValue<string>(FieldNames.fRiskAssessmentType);
        riskAssessment.Status = reader.GetValue<string>(FieldNames.fRiskAssessmentStatus);
        riskAssessment.Stage = reader.GetValue<string>(FieldNames.fRiskAssessmentStage);

        return riskAssessment;
    }

    /// <summary>
    /// Maps SqlDataReader to Mitigation entity
    /// </summary>
    public static Mitigation MapToMitigation(SqlDataReader reader)
    {
        MitigationID mitigationID = new(reader.GetValue<string>(FieldNames.fMitigationCode).ToString());
        Mitigation mitigation = new(mitigationID);

        mitigation.Code = reader.GetValue<string>(FieldNames.fMitigationCode);
        mitigation.HazardCode = reader.GetValue<string>(FieldNames.fMitigationHazardCode);

        return mitigation;
    }

    /// <summary>
    /// Maps SqlDataReader to MitigationAssignment entity
    /// </summary>
    public static MitigationAssignment MapToMitigationAssignment(SqlDataReader reader)
    {
        MitigationAssignmentID mitigationAssignmentID = new(reader.GetValue<string>(FieldNames.fMitigationAssignmentCode).ToString());
        MitigationAssignment mitigationAssignment = new(mitigationAssignmentID);

        mitigationAssignment.Code = reader.GetValue<string>(FieldNames.fMitigationAssignmentCode);
        mitigationAssignment.MitigationCode = reader.GetValue<string>(FieldNames.fMitigationAssignmentMitigationCode);
        mitigationAssignment.DepartmentCode = reader.GetValue<string>(FieldNames.fMitigationAssignmentDepartmentCode);

        return mitigationAssignment;
    }

    /// <summary>
    /// Maps SqlDataReader to ReportValidation entity
    /// </summary>
    public static ReportValidation MapToReportValidation(SqlDataReader reader)
    {
        ReportValidationID reportValidationID = new(reader.GetValue<string>(FieldNames.fReportValidationCode).ToString());
        ReportValidation reportValidation = new(reportValidationID);

        reportValidation.Code = reader.GetValue<string>(FieldNames.fReportValidationCode);
        reportValidation.ReportCode = reader.GetValue<string>(FieldNames.fReportValidationReportCode);
        reportValidation.ValidationDecision = reader.GetValue<string>(FieldNames.fReportValidationDecision);
        reportValidation.Status = reader.GetValue<string>(FieldNames.fReportValidationStatus);
        reportValidation.Stage = reader.GetValue<string>(FieldNames.fReportValidationStage);

        return reportValidation;
    }

    /// <summary>
    /// Maps SqlDataReader to ScoringPanel entity
    /// </summary>
    public static ScoringPanel MapToScoringPanel(SqlDataReader reader)
    {
        ScoringPanelID scoringPanelID = new(reader.GetValue<string>(FieldNames.fScoringPanelCode).ToString());
        ScoringPanel scoringPanel = new(scoringPanelID);

        scoringPanel.Code = reader.GetValue<string>(FieldNames.fScoringPanelCode);
        scoringPanel.HazardCode = reader.GetValue<string>(FieldNames.fScoringPanelHazardCode);
        scoringPanel.SMSUserCode = reader.GetValue<string>(FieldNames.fScoringPanelSMSUserCode);
        scoringPanel.Likelihood = reader.GetValue<string>(FieldNames.fScoringPanelLikelihood);
        scoringPanel.Severity = reader.GetValue<string>(FieldNames.fScoringPanelSeverity);
        scoringPanel.Score = reader.GetValue<string>(FieldNames.fScoringPanelScore);

        return scoringPanel;
    }

    #endregion
}