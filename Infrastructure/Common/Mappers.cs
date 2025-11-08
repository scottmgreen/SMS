using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using Microsoft.Data.SqlClient;
using SMS_Infrastructure.Common;
using SMS_Domain.ValueObjects;
using SMS_Domain.Models;
using System.Reflection;

namespace SMS_Infrastructure.Common;

public static partial class Mappers
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
    /// Maps SqlDataReader to SMSApplicationUser entity
    /// </summary>
    public static SMSApplicationUser MapToSMSApplicationUser(SqlDataReader reader)
    {
        try
        {
            // Extract database values
            var code = reader.GetString(FieldNames.fSMSApplicationUserCode);
            var firstName = reader.GetString(FieldNames.fSMSApplicationUserFirstName);
            var lastName = reader.GetString(FieldNames.fSMSApplicationUserLastName);
            var userName = reader.GetString(FieldNames.fSMSApplicationUserUserName);
            var hashedPassword = reader.GetString(FieldNames.fSMSApplicationUserPassword);
            var applicationRole = reader.GetString(FieldNames.fSMSApplicationUserApplicationRole);
            var permissionLevel = reader.GetString(FieldNames.fSMSApplicationUserPermissionLevel);
            var isActive = reader.GetBoolean(FieldNames.fSMSApplicationUserIsActive);
            var lastLoginDate = reader.IsDBNull(FieldNames.fSMSApplicationUserLastLoginDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSApplicationUserLastLoginDate);
            var createdBy = reader.GetString(FieldNames.fCreatedBy);
            var createdDate = reader.GetDateTime(FieldNames.fCreatedDate);

            // Create value objects
            var firstNameVO = FirstName.Create(firstName).Value;
            var lastNameVO = LastName.Create(lastName).Value;
            var userNameVO = UserName.Create(userName).Value;
            var passwordVO = Password.FromHash(hashedPassword, createdDate);

            // Create entity using factory method
            var user = SMSApplicationUser.Create(
                code,
                firstNameVO,
                lastNameVO,
                userNameVO,
                passwordVO,
                applicationRole,
                permissionLevel,
                createdBy);

            // Set additional properties that aren't part of creation
            if (!isActive)
                user.Deactivate();

            if (lastLoginDate.HasValue)
                user.RecordLogin(); // This will set to current time, but we'll need to adjust

            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to SMSApplicationUser: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps SqlDataReader to SMSOrganizationalUser entity
    /// </summary>
    public static SMSOrganizationalUser MapToSMSOrganizationalUser(SqlDataReader reader)
    {
        try
        {
            // Extract database values
            var code = reader.GetString(FieldNames.fSMSOrganizationalUserCode);
            var firstName = reader.GetString(FieldNames.fSMSOrganizationalUserFirstName);
            var lastName = reader.GetString(FieldNames.fSMSOrganizationalUserLastName);
            var userName = reader.GetString(FieldNames.fSMSOrganizationalUserUserName);
            var hashedPassword = reader.GetString(FieldNames.fSMSOrganizationalUserPassword);
            var department = reader.GetString(FieldNames.fSMSOrganizationalUserDepartment);
            var position = reader.GetString(FieldNames.fSMSOrganizationalUserPosition);
            var organizationLevel = reader.GetString(FieldNames.fSMSOrganizationalUserOrganizationLevel);
            var isActive = reader.GetBoolean(FieldNames.fSMSOrganizationalUserIsActive);
            var lastLoginDate = reader.IsDBNull(FieldNames.fSMSOrganizationalUserLastLoginDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSOrganizationalUserLastLoginDate);
            var createdBy = reader.GetString(FieldNames.fCreatedBy);
            var createdDate = reader.GetDateTime(FieldNames.fCreatedDate);

            // Create value objects
            var firstNameVO = FirstName.Create(firstName).Value;
            var lastNameVO = LastName.Create(lastName).Value;
            var userNameVO = UserName.Create(userName).Value;
            var passwordVO = Password.FromHash(hashedPassword, createdDate);

            // Create entity using factory method
            var user = SMSOrganizationalUser.Create(
                code,
                firstNameVO,
                lastNameVO,
                userNameVO,
                passwordVO,
                department,
                position,
                organizationLevel,
                createdBy);

            // Set additional properties that aren't part of creation
            if (!isActive)
                user.Deactivate();

            if (lastLoginDate.HasValue)
                user.RecordLogin();

            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to SMSOrganizationalUser: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps SqlDataReader to SMSStakeholderUser entity
    /// </summary>
    public static SMSStakeholderUser MapToSMSStakeholderUser(SqlDataReader reader)
    {
        try
        {
            // Extract database values
            var code = reader.GetString(FieldNames.fSMSStakeholderUserCode);
            var firstName = reader.GetString(FieldNames.fSMSStakeholderUserFirstName);
            var lastName = reader.GetString(FieldNames.fSMSStakeholderUserLastName);
            var userName = reader.GetString(FieldNames.fSMSStakeholderUserUserName);
            var hashedPassword = reader.GetString(FieldNames.fSMSStakeholderUserPassword);
            var stakeholderType = reader.GetString(FieldNames.fSMSStakeholderUserStakeholderType);
            var organization = reader.GetString(FieldNames.fSMSStakeholderUserOrganization);
            var accessLevel = reader.GetString(FieldNames.fSMSStakeholderUserAccessLevel);
            var isActive = reader.GetBoolean(FieldNames.fSMSStakeholderUserIsActive);
            var lastLoginDate = reader.IsDBNull(FieldNames.fSMSStakeholderUserLastLoginDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSStakeholderUserLastLoginDate);
            var createdBy = reader.GetString(FieldNames.fCreatedBy);
            var createdDate = reader.GetDateTime(FieldNames.fCreatedDate);

            // Create value objects
            var firstNameVO = FirstName.Create(firstName).Value;
            var lastNameVO = LastName.Create(lastName).Value;
            var userNameVO = UserName.Create(userName).Value;
            var passwordVO = Password.FromHash(hashedPassword, createdDate);

            // Create entity using factory method
            var user = SMSStakeholderUser.Create(
                code,
                firstNameVO,
                lastNameVO,
                userNameVO,
                passwordVO,
                stakeholderType,
                organization,
                accessLevel,
                createdBy);

            // Set additional properties that aren't part of creation
            if (!isActive)
                user.Deactivate();

            if (lastLoginDate.HasValue)
                user.RecordLogin();

            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to SMSStakeholderUser: {ex.Message}", ex);
        }
    }
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

        // Core properties
        hazard.Code = reader.GetValue<string>(FieldNames.fHazardCode) ?? string.Empty;
        hazard.Name = reader.GetValue<string>(FieldNames.fHazardName);
        hazard.Description = reader.GetValue<string>(FieldNames.fHazardDescription) ?? string.Empty;
        hazard.ReportCode = reader.GetValue<string>(FieldNames.fHazardReportCode) ?? string.Empty;
        hazard.ScoringPanelCode = reader.GetValue<string>(FieldNames.fHazardScoringPanelCode);
        hazard.AverageScore = reader.GetValue<string>(FieldNames.fHazardAverageScore);

        // Enhanced properties
        hazard.Category = reader.GetValue<string>(FieldNames.fHazardCategory) ?? string.Empty;
        hazard.FiveMComponent = reader.GetValue<string>(FieldNames.fHazardFiveMComponent);
        hazard.HazardType = reader.GetValue<string>(FieldNames.fHazardType);
        
        // Reporting information
        hazard.ReportedBy = reader.GetValue<string>(FieldNames.fHazardReportedBy) ?? string.Empty;
        hazard.ReportedOn = reader.IsDBNull(FieldNames.fHazardReportedOn) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fHazardReportedOn);
        hazard.ReportingDepartment = reader.GetValue<string>(FieldNames.fHazardReportingDepartment);

        // Privacy and confidentiality
        hazard.IsConfidential = reader.IsDBNull(FieldNames.fHazardIsConfidential) ? false : reader.GetBoolean(FieldNames.fHazardIsConfidential);
        hazard.IsAnonymous = reader.IsDBNull(FieldNames.fHazardIsAnonymous) ? false : reader.GetBoolean(FieldNames.fHazardIsAnonymous);

        // Status and priority (with enum parsing)
        var statusValue = reader.GetValue<string>(FieldNames.fHazardStatus);
        hazard.Status = string.IsNullOrEmpty(statusValue) ? HazardStatus.Active : 
            (HazardStatus.FromValue(statusValue) ?? HazardStatus.Active);

        var priorityValue = reader.GetValue<string>(FieldNames.fHazardPriority);
        hazard.Priority = string.IsNullOrEmpty(priorityValue) ? HazardPriority.Medium :
            (HazardPriority.FromValue(priorityValue) ?? HazardPriority.Medium);

        // Risk assessment properties
        hazard.RiskLevel = reader.GetValue<string>(FieldNames.fHazardRiskLevel);
        hazard.WorstCredibleOutcome = reader.GetValue<string>(FieldNames.fHazardWorstCredibleOutcome);
        hazard.RootCause = reader.GetValue<string>(FieldNames.fHazardRootCause);

        // Mitigation properties
        hazard.CurrentMitigations = reader.GetValue<string>(FieldNames.fHazardCurrentMitigations);
        hazard.ProposedMitigations = reader.GetValue<string>(FieldNames.fHazardProposedMitigations);
        hazard.MitigationTargetDate = reader.IsDBNull(FieldNames.fHazardMitigationTargetDate) ? null : reader.GetDateTime(FieldNames.fHazardMitigationTargetDate);
        hazard.MitigationOwner = reader.GetValue<string>(FieldNames.fHazardMitigationOwner);

        // Investigation properties
        hazard.RequiresInvestigation = reader.IsDBNull(FieldNames.fHazardRequiresInvestigation) ? false : reader.GetBoolean(FieldNames.fHazardRequiresInvestigation);
        hazard.InvestigationCompletedDate = reader.IsDBNull(FieldNames.fHazardInvestigationCompletedDate) ? null : reader.GetDateTime(FieldNames.fHazardInvestigationCompletedDate);
        hazard.InvestigationNotes = reader.GetValue<string>(FieldNames.fHazardInvestigationNotes);

        // Additional properties
        hazard.AdditionalComments = reader.GetValue<string>(FieldNames.fHazardAdditionalComments);

        // Location properties (legacy)
        hazard.Location = reader.GetValue<string>(FieldNames.fHazardLocation);
        hazard.LocationArea = reader.GetValue<string>(FieldNames.fHazardLocationArea);
        hazard.LocationSubArea = reader.GetValue<string>(FieldNames.fHazardLocationSubArea);

        return hazard;
    }

    /// <summary>
    /// Maps SqlDataReader to HazardLocation entity
    /// </summary>
    public static HazardLocation MapToHazardLocation(SqlDataReader reader)
    {
        var hazardLocationCode = reader.GetValue<string>(FieldNames.fHazardLocationCode);
        HazardLocationID hazardLocationID = new(hazardLocationCode);
        HazardLocation hazardLocation = new(hazardLocationID);

        hazardLocation.Code = hazardLocationCode ?? string.Empty;
        hazardLocation.HazardCode = reader.GetValue<string>(FieldNames.fHazardLocationHazardCode) ?? string.Empty;
        hazardLocation.Latitude = reader.IsDBNull(FieldNames.fHazardLocationLatitude) ? null : reader.GetDecimal(FieldNames.fHazardLocationLatitude);
        hazardLocation.Longitude = reader.IsDBNull(FieldNames.fHazardLocationLongitude) ? null : reader.GetDecimal(FieldNames.fHazardLocationLongitude);
        hazardLocation.Description = reader.GetValue<string>(FieldNames.fHazardLocationDescription);
        hazardLocation.DateSelected = reader.GetDateTime(FieldNames.fHazardLocationDateSelected);
        hazardLocation.LocationMapSVG = reader.GetValue<string>(FieldNames.fHazardLocationMapSVG);
        hazardLocation.LocationArea = reader.GetValue<string>(FieldNames.fHazardLocationArea);
        hazardLocation.LocationSubArea = reader.GetValue<string>(FieldNames.fHazardLocationSubArea);
        hazardLocation.LocationName = reader.GetValue<string>(FieldNames.fHazardLocationName);
        hazardLocation.AccuracyMeters = reader.IsDBNull(FieldNames.fHazardLocationAccuracyMeters) ? null : reader.GetDecimal(FieldNames.fHazardLocationAccuracyMeters);
        hazardLocation.ElevationFeet = reader.IsDBNull(FieldNames.fHazardLocationElevationFeet) ? null : reader.GetDecimal(FieldNames.fHazardLocationElevationFeet);
        hazardLocation.Source = reader.GetValue<string>(FieldNames.fHazardLocationSource) ?? "Manual";
        
        // Map status enum
        var statusValue = reader.GetValue<string>(FieldNames.fHazardLocationStatus);
        hazardLocation.Status = Enum.TryParse<HazardLocationStatus>(statusValue, out var status) ? status : HazardLocationStatus.Active;
        
        hazardLocation.IsValidated = reader.GetBoolean(FieldNames.fHazardLocationIsValidated);
        hazardLocation.ValidatedDate = reader.IsDBNull(FieldNames.fHazardLocationValidatedDate) ? null : reader.GetDateTime(FieldNames.fHazardLocationValidatedDate);
        hazardLocation.ValidatedBy = reader.GetValue<string>(FieldNames.fHazardLocationValidatedBy);
        hazardLocation.Notes = reader.GetValue<string>(FieldNames.fHazardLocationNotes);
        hazardLocation.Tags = reader.GetValue<string>(FieldNames.fHazardLocationTags);
        hazardLocation.AirportGrid = reader.GetValue<string>(FieldNames.fHazardLocationAirportGrid);
        hazardLocation.RunwayReference = reader.GetValue<string>(FieldNames.fHazardLocationRunwayReference);
        hazardLocation.TaxiwayReference = reader.GetValue<string>(FieldNames.fHazardLocationTaxiwayReference);

        return hazardLocation;
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
    /// Maps SqlDataReader to Investigation entity with enhanced properties
    /// </summary>
    public static Investigation MapToInvestigation(SqlDataReader reader)
    {
        var investigationId = new InvestigationID(reader.GetValue<string>(FieldNames.fInvestigationCode));
        var investigation = new Investigation(investigationId);
        
        // Map basic properties using reflection to access private setters
        var investigationType = typeof(Investigation);
        
        // Core properties
        investigation.Code = reader.GetValue<string>(FieldNames.fInvestigationCode);
        investigation.ReportCode = reader.GetValue<string>(FieldNames.fInvestigationReportCode);
        investigation.HazardCode = reader.GetValue<string>(FieldNames.fInvestigationHazardCode);
        investigation.InvestigationNotes = reader.GetValue<string>(FieldNames.fInvestigationNotes);
        
        // Management properties
        investigation.AssignedInvestigatorId = reader.GetValue<string>(FieldNames.fInvestigationAssignedInvestigatorId) ?? string.Empty;
        
        var statusValue = reader.GetValue<string>(FieldNames.fInvestigationStatus);
        if (Enum.TryParse<InvestigationStatus>(statusValue, out var status))
        {
            investigation.Status = status;
        }
        
        investigation.CompletedDate = reader.GetValue<DateTime?>(FieldNames.fInvestigationCompletedDate);
        investigation.InvestigationPlan = reader.GetValue<string>(FieldNames.fInvestigationPlan);
        investigation.InvestigationObjectives = reader.GetValue<string>(FieldNames.fInvestigationObjectives);
        
        // Decision properties
        investigation.DecisionType = reader.GetValue<string>(FieldNames.fInvestigationDecisionType);
        investigation.DecisionRationale = reader.GetValue<string>(FieldNames.fInvestigationDecisionRationale);
        investigation.DecisionMaker = reader.GetValue<string>(FieldNames.fInvestigationDecisionMaker);
        investigation.DecisionDate = reader.GetValue<DateTime?>(FieldNames.fInvestigationDecisionDate);
        investigation.NextSteps = reader.GetValue<string>(FieldNames.fInvestigationNextSteps);
        investigation.ReferralDetails = reader.GetValue<string>(FieldNames.fInvestigationReferralDetails);
        
        // Audit properties
        investigation.CreatedBy = reader.GetValue<string>(FieldNames.fCreatedBy) ?? "SYSTEM";
        investigation.CreatedDate = reader.GetValue<DateTime>(FieldNames.fCreatedDate);
        investigation.UpdatedBy = reader.GetValue<string>(FieldNames.fUpdatedBy);
        investigation.UpdatedDate = reader.GetValue<DateTime?>(FieldNames.fUpdatedDate);
        
        return investigation;
    }

    /// <summary>
    /// Maps SqlDataReader to Interview entity with enhanced properties
    /// </summary>
    public static Interview MapToInterview(SqlDataReader reader)
    {
        var interviewId = new InterviewID(reader.GetValue<string>(FieldNames.fInterviewCode));
        var interview = new Interview(interviewId);
        
        // Core properties
        interview.Code = reader.GetValue<string>(FieldNames.fInterviewCode);
        interview.InvestigationCode = reader.GetValue<string>(FieldNames.fInterviewInvestigationCode);
        interview.SMSInvestigatorCode = reader.GetValue<string>(FieldNames.fInterviewSMSInvestigatorCode);
        
        // Interview details
        interview.PersonInterviewed = reader.GetValue<string>(FieldNames.fInterviewPersonInterviewed) ?? string.Empty;
        interview.PersonInterviewedRole = reader.GetValue<string>(FieldNames.fInterviewPersonRole);
        interview.PersonInterviewedDepartment = reader.GetValue<string>(FieldNames.fInterviewPersonDepartment);
        interview.PersonInterviewedNotes = reader.GetValue<string>(FieldNames.fInterviewPersonInterviewedNotes);
        interview.InvestigatorNotes = reader.GetValue<string>(FieldNames.fInterviewInvestigatorNotes);
        
        // Status and scheduling
        var statusValue = reader.GetValue<string>(FieldNames.fInterviewStatus);
        if (Enum.TryParse<InterviewStatus>(statusValue, out var status))
        {
            interview.Status = status;
        }
        
        interview.InterviewDate = reader.GetValue<DateTime?>(FieldNames.fInterviewDate);
        interview.DurationMinutes = reader.GetValue<int?>(FieldNames.fInterviewDurationMinutes);
        interview.InterviewLocation = reader.GetValue<string>(FieldNames.fInterviewLocation);
        
        var typeValue = reader.GetValue<string>(FieldNames.fInterviewType);
        if (Enum.TryParse<InterviewType>(typeValue, out var interviewType))
        {
            interview.Type = interviewType;
        }
        
        interview.IsConfidential = reader.GetValue<bool>(FieldNames.fInterviewIsConfidential);
        
        // Preparation and results
        interview.PreparationNotes = reader.GetValue<string>(FieldNames.fInterviewPreparationNotes);
        interview.QuestionsToAsk = reader.GetValue<string>(FieldNames.fInterviewQuestionsToAsk);
        interview.BackgroundInformation = reader.GetValue<string>(FieldNames.fInterviewBackgroundInformation);
        interview.KeyFindings = reader.GetValue<string>(FieldNames.fInterviewKeyFindings);
        interview.FollowUpRequired = reader.GetValue<string>(FieldNames.fInterviewFollowUpRequired);
        interview.AdditionalWitnesses = reader.GetValue<string>(FieldNames.fInterviewAdditionalWitnesses);
        interview.CompletedDate = reader.GetValue<DateTime?>(FieldNames.fInterviewCompletedDate);
        
        // Audit properties
        interview.CreatedBy = reader.GetValue<string>(FieldNames.fCreatedBy) ?? "SYSTEM";
        interview.CreatedDate = reader.GetValue<DateTime>(FieldNames.fCreatedDate);
        interview.UpdatedBy = reader.GetValue<string>(FieldNames.fUpdatedBy);
        interview.UpdatedDate = reader.GetValue<DateTime?>(FieldNames.fUpdatedDate);
        
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
        // Extract values from database
        var code = reader.GetValue<string>(FieldNames.fRiskAssessmentCode);
        var name = reader.GetValue<string>(FieldNames.fRiskAssessmentName);
        var description = reader.GetValue<string>(FieldNames.fRiskAssessmentDescription);
        var hazardCode = reader.GetValue<string>(FieldNames.fRiskAssessmentHazardCode);
        var assessmentType = reader.GetValue<string>(FieldNames.fRiskAssessmentType);
        var status = reader.GetValue<string>(FieldNames.fRiskAssessmentStatus);
        var stage = reader.GetValue<string>(FieldNames.fRiskAssessmentStage);

        // Create RiskAssessmentId
        var riskAssessmentId = new RiskAssessmentID(code);
        
        // Create entity using the public constructor (for database mapping)
        var riskAssessment = new RiskAssessment(riskAssessmentId);

        // Set properties using reflection since they have private setters
        // This is acceptable for database mapping scenarios
        var riskAssessmentType = typeof(RiskAssessment);
        
        riskAssessmentType.GetProperty("Name")?.SetValue(riskAssessment, name);
        riskAssessmentType.GetProperty("Description")?.SetValue(riskAssessment, description);
        riskAssessmentType.GetProperty("HazardCode")?.SetValue(riskAssessment, hazardCode);
        
        // For Code property, we need to use reflection since it has a private setter
        riskAssessmentType.GetProperty("Code")?.SetValue(riskAssessment, code);

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

    /// <summary>
    /// Maps SqlDataReader to SMSUserRole entity - CORRECTED to match actual database schema
    /// </summary>
    public static SMSUserRole MapToSMSUserRole(SqlDataReader reader)
    {
        try
        {
            // Extract database values - using ACTUAL database field names from tbld_SMSUserRoles
            var code = reader.GetString(FieldNames.fSMSUserRoleCode);
            var userId = reader.GetString(FieldNames.fSMSUserRoleUserId);
            var userType = reader.GetString(FieldNames.fSMSUserRoleUserType);
            var smsRoleCode = reader.GetString(FieldNames.fSMSUserRoleSMSRoleCode);  // ✅ CORRECTED: Using actual FK field
            var department = reader.GetString(FieldNames.fSMSUserRoleDepartment);
            var effectiveDate = reader.GetDateTime(FieldNames.fSMSUserRoleEffectiveDate);
            var expirationDate = reader.IsDBNull(FieldNames.fSMSUserRoleExpirationDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSUserRoleExpirationDate);
            var isActive = reader.GetBoolean(FieldNames.fSMSUserRoleIsActive);
            var assignedBy = reader.GetString(FieldNames.fSMSUserRoleAssignedBy);
            var assignedDate = reader.GetDateTime(FieldNames.fSMSUserRoleAssignedDate);  // ✅ CORRECTED: Added missing field
            var deactivatedBy = reader.IsDBNull(FieldNames.fSMSUserRoleDeactivatedBy) ? null : reader.GetString(FieldNames.fSMSUserRoleDeactivatedBy);
            var deactivatedDate = reader.IsDBNull(FieldNames.fSMSUserRoleDeactivatedDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSUserRoleDeactivatedDate);
            var createdBy = reader.GetString(FieldNames.fCreatedBy);
            var createdDate = reader.GetDateTime(FieldNames.fCreatedDate);

            // ✅ CORRECTED: Get the SMS Role using the SMSRoleCode (foreign key relationship)
            // Note: This requires looking up the role from tbld_SMSRoles table using smsRoleCode
            // For now, we'll create a basic role structure based on the FK
            var role = SMSRole.GetAllValues().FirstOrDefault(r => r.Value == smsRoleCode);
            if (role == null)
            {
                // If role not found in enum, we need role data from tbld_SMSRoles table
                // This is expected since roles are stored separately and linked via FK
                throw new InvalidOperationException($"SMS Role with code '{smsRoleCode}' not found in SMSRole enumeration. Role data should be joined from tbld_SMSRoles table.");
            }

            // Create the entity using the factory method
            var userRole = SMSUserRole.Create(
                userId,
                userType,
                role,
                department,
                assignedBy,
                effectiveDate,
                expirationDate,
                null); // ✅ CORRECTED: No AssignmentNotes field in database

            // Set the code from database
            var userRoleId = new SMSUserRoleID(code);
            var idProperty = typeof(SMSUserRole).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            idProperty?.SetValue(userRole, userRoleId);

            // Set additional properties that aren't part of creation
            if (!isActive && deactivatedBy != null)
            {
                userRole.Deactivate(deactivatedBy, "Role deactivated");  // ✅ CORRECTED: No DeactivationReason field in database
            }

            // Set audit fields using reflection since they might be private setters
            var baseEntityType = typeof(BaseAuditableEntity);
            baseEntityType.GetProperty("CreatedBy")?.SetValue(userRole, createdBy);
            baseEntityType.GetProperty("CreatedDate")?.SetValue(userRole, createdDate);

            return userRole;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to SMSUserRole: {ex.Message}", ex);
        }
    }

    #endregion

    #region HazardFile Mappers

    /// <summary>
    /// Maps SqlDataReader to HazardFile entity
    /// </summary>
    public static HazardFile MapToHazardFile(SqlDataReader reader)
    {
        try
        {
            var code = reader.GetValue<string>(FieldNames.fHazardFileCode) ?? string.Empty;
            var hazardCode = reader.GetValue<string>(FieldNames.fHazardFileHazardCode) ?? string.Empty;
            var fileName = reader.GetValue<string>(FieldNames.fHazardFileFileName) ?? string.Empty;
            var fileType = reader.GetValue<string>(FieldNames.fHazardFileFileType) ?? string.Empty;
            var fileSizeBytes = reader.GetValue<long>(FieldNames.fHazardFileFileSizeBytes);
            var uploadedBy = reader.GetValue<string>(FieldNames.fHazardFileUploadedBy) ?? string.Empty;

            // Determine storage type and create appropriate HazardFile
            var storageTypeValue = reader.GetValue<string>(FieldNames.fHazardFileStorageType) ?? "FileSystem";
            var storageType = HazardFileStorageType.FromValue(storageTypeValue) ?? HazardFileStorageType.FileSystem;

            Result<HazardFile> hazardFileResult;

            switch (storageType.Value) // Use .Value to get the string value for comparison
            {
                case "DATABASE":
                    var fileData = reader.GetValue<byte[]>(FieldNames.fHazardFileFileData);
                    hazardFileResult = HazardFile.CreateForDatabase(hazardCode, fileName, fileType, fileData ?? Array.Empty<byte>(), uploadedBy);
                    break;

                case "CLOUD":
                    var cloudPath = reader.GetValue<string>(FieldNames.fHazardFileFilePath) ?? string.Empty;
                    hazardFileResult = HazardFile.CreateForCloud(hazardCode, fileName, fileType, fileSizeBytes, cloudPath, uploadedBy);
                    break;

                default: // FileSystem
                    var filePath = reader.GetValue<string>(FieldNames.fHazardFileFilePath) ?? string.Empty;
                    hazardFileResult = HazardFile.CreateForFileSystem(hazardCode, fileName, fileType, fileSizeBytes, filePath, uploadedBy);
                    break;
            }

            if (hazardFileResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to create HazardFile: {hazardFileResult.Error?.Message}");
            }

            var hazardFile = hazardFileResult.Value;

            // Map additional properties using reflection since they have private setters
            var hazardFileType = typeof(HazardFile);

            // Set Code using reflection if needed (it should already be set by factory)
            SetPrivateProperty(hazardFileType, hazardFile, "Code", code);

            // Set other properties
            var reportCode = reader.GetValue<string>(FieldNames.fHazardFileReportCode);
            SetPrivateProperty(hazardFileType, hazardFile, "ReportCode", reportCode);

            var contentType = reader.GetValue<string>(FieldNames.fHazardFileContentType) ?? string.Empty;
            SetPrivateProperty(hazardFileType, hazardFile, "ContentType", contentType);

            var fileHash = reader.GetValue<string>(FieldNames.fHazardFileFileHash);
            SetPrivateProperty(hazardFileType, hazardFile, "FileHash", fileHash);

            var description = reader.GetValue<string>(FieldNames.fHazardFileDescription);
            SetPrivateProperty(hazardFileType, hazardFile, "Description", description);

            var categoryValue = reader.GetValue<string>(FieldNames.fHazardFileCategory);
            if (!string.IsNullOrEmpty(categoryValue))
            {
                var category = HazardFileCategory.FromValue(categoryValue);
                SetPrivateProperty(hazardFileType, hazardFile, "Category", category);
            }

            var isConfidential = reader.IsDBNull(FieldNames.fHazardFileIsConfidential) ? false : reader.GetBoolean(FieldNames.fHazardFileIsConfidential);
            SetPrivateProperty(hazardFileType, hazardFile, "IsConfidential", isConfidential);

            var uploadedDate = reader.IsDBNull(FieldNames.fHazardFileUploadedDate) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fHazardFileUploadedDate);
            SetPrivateProperty(hazardFileType, hazardFile, "UploadedDate", uploadedDate);

            var tags = reader.GetValue<string>(FieldNames.fHazardFileTags);
            SetPrivateProperty(hazardFileType, hazardFile, "Tags", tags);

            var isActive = reader.IsDBNull(FieldNames.fHazardFileIsActive) ? true : reader.GetBoolean(FieldNames.fHazardFileIsActive);
            SetPrivateProperty(hazardFileType, hazardFile, "IsActive", isActive);

            var inactiveReason = reader.GetValue<string>(FieldNames.fHazardFileInactiveReason);
            SetPrivateProperty(hazardFileType, hazardFile, "InactiveReason", inactiveReason);

            var inactiveDate = reader.IsDBNull(FieldNames.fHazardFileInactiveDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fHazardFileInactiveDate);
            SetPrivateProperty(hazardFileType, hazardFile, "InactiveDate", inactiveDate);

            var inactiveBy = reader.GetValue<string>(FieldNames.fHazardFileInactiveBy);
            SetPrivateProperty(hazardFileType, hazardFile, "InactiveBy", inactiveBy);

            // Set audit fields
            var createdBy = reader.GetValue<string>(FieldNames.fCreatedBy) ?? "SYSTEM";
            var createdDate = reader.IsDBNull(FieldNames.fCreatedDate) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fCreatedDate);
            var updatedBy = reader.GetValue<string>(FieldNames.fUpdatedBy);
            var updatedDate = reader.IsDBNull(FieldNames.fUpdatedDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fUpdatedDate);

            var baseEntityType = typeof(BaseAuditableEntity);
            baseEntityType.GetProperty("CreatedBy")?.SetValue(hazardFile, createdBy);
            baseEntityType.GetProperty("CreatedDate")?.SetValue(hazardFile, createdDate);
            baseEntityType.GetProperty("UpdatedBy")?.SetValue(hazardFile, updatedBy);
            baseEntityType.GetProperty("UpdatedDate")?.SetValue(hazardFile, updatedDate);

            return hazardFile;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to HazardFile: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps SqlDataReader to HazardFileStatistics
    /// </summary>
    public static HazardFileStatistics MapToHazardFileStatistics(SqlDataReader reader)
    {
        return new HazardFileStatistics
        {
            HazardCode = reader.GetValue<string>("HazardCode") ?? string.Empty,
            TotalFiles = reader.GetValue<int>("TotalFiles"),
            TotalSizeBytes = reader.GetValue<long>("TotalSizeBytes"),
            PhotoCount = reader.GetValue<int>("PhotoCount"),
            DocumentCount = reader.GetValue<int>("DocumentCount"),
            VideoCount = reader.GetValue<int>("VideoCount"),
            ConfidentialCount = reader.GetValue<int>("ConfidentialCount"),
            FirstUploadDate = reader.IsDBNull("FirstUploadDate") ? null : reader.GetDateTime("FirstUploadDate"),
            LastUploadDate = reader.IsDBNull("LastUploadDate") ? null : reader.GetDateTime("LastUploadDate")
        };
    }

    /// <summary>
    /// Helper method to set private properties using reflection
    /// </summary>
    private static void SetPrivateProperty(Type type, object instance, string propertyName, object? value)
    {
        var property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(instance, value);
        }
    }

    #endregion
}