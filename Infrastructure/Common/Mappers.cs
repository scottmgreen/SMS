using System.Reflection;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Common;
using Microsoft.Data.SqlClient;
using System.Data;

using SMS_Domain.Enums;
using SMS_Domain.Models;
using SMS_Domain.ValueObjects;

using SMS_Infrastructure.Common;

using SMS_Shared.Common;

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

    private static void PopulateEntityFromReader<T>(this SqlDataReader reader, T entity) where T : new()
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
            SMSApplicationUserID applicationuserId = new SMSApplicationUserID(reader.GetString(FieldNames.fSMSApplicationUserCode));
            SMSApplicationUser applicationUser = new SMSApplicationUser(applicationuserId);
                        
            applicationUser.Code = reader.GetString(FieldNames.fSMSApplicationUserCode);
            applicationUser.FirstName = FirstName.Create(reader.GetString(FieldNames.fSMSApplicationUserFirstName)).Value;
            applicationUser.LastName = LastName.Create(reader.GetString(FieldNames.fSMSApplicationUserLastName)).Value;
            applicationUser.UserName= UserName.Create(reader.GetString(FieldNames.fSMSApplicationUserUserName)).Value;
            applicationUser.Password = Password.FromHash(reader.GetString(FieldNames.fSMSApplicationUserPassword), reader.GetDateTime(FieldNames.fCreatedDate),false);
            applicationUser.SMSUserType = reader.GetString(FieldNames.fSMSApplicationUserTypeCode);
            applicationUser.UserRole = new SMSUserRole(new SMSUserRoleID(reader.GetString(FieldNames.fSMSUserRoleCode)));
            applicationUser.IsActive = reader.GetBoolean(FieldNames.fSMSApplicationUserIsActive);
            applicationUser.LastLoginDate = reader.IsDBNull(FieldNames.fSMSApplicationUserLastLoginDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSApplicationUserLastLoginDate);
            applicationUser.CreatedBy = reader.GetString(FieldNames.fCreatedBy);
            applicationUser.CreatedDate = reader.GetDateTime(FieldNames.fCreatedDate);

            return applicationUser;
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
            SMSOrganizationalUserID userId = reader.GetString(FieldNames.fSMSOrganizationalUserCode);
            SMSOrganizationalUser orgUser = new SMSOrganizationalUser(userId);
            var createdDate = reader.GetDateTime(FieldNames.fCreatedDate);
            orgUser.Code = reader.GetString(FieldNames.fSMSOrganizationalUserCode);
            orgUser.FirstName = FirstName.Create(reader.GetString(FieldNames.fSMSOrganizationalUserFirstName)).Value;
            orgUser.LastName = LastName.Create(reader.GetString(FieldNames.fSMSOrganizationalUserLastName)).Value;
            orgUser.UserName = UserName.Create(reader.GetString(FieldNames.fSMSOrganizationalUserUserName)).Value;
            orgUser.Password = Password.FromHash(reader.GetString(FieldNames.fSMSOrganizationalUserPassword), createdDate);

            orgUser.Department = reader.GetString(FieldNames.fSMSOrganizationalUserDepartment);
            orgUser.Position = reader.GetString(FieldNames.fSMSOrganizationalUserPosition);
            orgUser.OrganizationLevel = reader.GetString(FieldNames.fSMSOrganizationalUserOrganizationLevel);
            
            // New SMS role fields - with null checking for backward compatibility
            if (reader.HasColumn(FieldNames.fSMSOrganizationalUserSMSRole))
            {
                orgUser.SMSRole = reader.GetValue<string>(FieldNames.fSMSOrganizationalUserSMSRole);
            }
            
            if (reader.HasColumn(FieldNames.fSMSOrganizationalUserAuthorityLevel))
            {
                orgUser.AuthorityLevel = reader.GetValue<string>(FieldNames.fSMSOrganizationalUserAuthorityLevel);
            }
            
            if (reader.HasColumn(FieldNames.fSMSOrganizationalUserRiskApprovalAuthority))
            {
                orgUser.RiskApprovalAuthority = reader.GetValue<string>(FieldNames.fSMSOrganizationalUserRiskApprovalAuthority);
            }
            
            orgUser.IsActive = reader.GetBoolean(FieldNames.fSMSOrganizationalUserIsActive);
            orgUser.LastLoginDate = reader.IsDBNull(FieldNames.fSMSOrganizationalUserLastLoginDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSOrganizationalUserLastLoginDate);
            orgUser.CreatedBy = reader.GetString(FieldNames.fCreatedBy);
            orgUser.CreatedDate = createdDate;

            return orgUser;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to SMSOrganizationalUser: {ex.Message}", ex);
        }
    }
    /// <summary>
    /// Maps SqlDataReader to SMSUserRole entity
    /// </summary>
    public static SMSUserRole MapToSMSUserRole(SqlDataReader reader)
    {
        SMSUserRoleID userRoleId = new(reader.GetValue<string>(FieldNames.fSMSRoleCode)?.Trim() ?? string.Empty);
        SMSUserRole userRole = new(userRoleId);

        userRole.Code = reader.GetValue<string>(FieldNames.fSMSRoleCode)?.Trim() ?? string.Empty;
        userRole.Name = reader.GetValue<string>(FieldNames.fSMSUserRoleName)?.Trim();

        return userRole;
    }

    /// <summary>
    /// Maps SqlDataReader to SMSUserRolePermission entity
    /// </summary>
    public static SMSUserRolePermission MapToSMSUserRolePermission(SqlDataReader reader)
    {
        SMSUserRolePermissionID permissionId = new(reader.GetValue<string>(FieldNames.fSMSUserRolePermissionCode));
        SMSUserRolePermission permission = new(permissionId);

        permission.Code = reader.GetValue<string>(FieldNames.fSMSUserRolePermissionCode)?.Trim() ?? string.Empty;
        permission.SMSUserRoleCode = reader.GetValue<string>(FieldNames.fSMSUserRolePermissionSMSUserRoleCode)?.Trim();
        permission.SMSModule = reader.GetValue<string>(FieldNames.fSMSUserRolePermissionModule)?.Trim();
        permission.Create = reader.GetValue<bool>(FieldNames.fSMSUserRolePermissionCreate);
        permission.Read = reader.GetValue<bool>(FieldNames.fSMSUserRolePermissionRead);
        permission.Update = reader.GetValue<bool>(FieldNames.fSMSUserRolePermissionUpdate);
        permission.Delete = reader.GetValue<bool>(FieldNames.fSMSUserRolePermissionDelete);

        return permission;
    }
    /// <summary>
    /// Maps SqlDataReader to SMSStakeholderUser entity
    /// </summary>
    public static SMSStakeholderUser MapToSMSStakeholderUser(SqlDataReader reader)
    {
        try
        {
            SMSStakeholderUserID stakeholderid = new SMSStakeholderUserID(reader.GetString(FieldNames.fSMSStakeholderUserCode));
            SMSStakeholderUser stakeholderuser = new SMSStakeholderUser(stakeholderid);
            // Extract database values
            FirstName firstName = FirstName.Create(reader.GetString(FieldNames.fSMSStakeholderUserFirstName)).Value;
            LastName lastName = LastName.Create(reader.GetString(FieldNames.fSMSStakeholderUserLastName)).Value;
            UserName userName = UserName.Create(reader.GetString(FieldNames.fSMSStakeholderUserUserName)).Value;
            Password password = Password.FromHash(reader.GetString(FieldNames.fSMSStakeholderUserPassword), reader.GetDateTime(FieldNames.fCreatedDate));
            stakeholderuser.Code = reader.GetString(FieldNames.fSMSStakeholderUserCode);
            stakeholderuser.FirstName = firstName;
            stakeholderuser.LastName = lastName;
            stakeholderuser.UserName = userName;
            stakeholderuser.Password = password;    

            stakeholderuser.StakeholderType = reader.GetString(FieldNames.fSMSStakeholderUserStakeholderTypeCode);
            stakeholderuser.UserRole = new SMSUserRole(new SMSUserRoleID(reader.GetString(FieldNames.fSMSUserRoleCode)));
            stakeholderuser.Organization = reader.GetString(FieldNames.fSMSStakeholderUserOrganization);

            var smsUserRole = reader.GetString(FieldNames.fSMSUserRoleCode);
            stakeholderuser.IsActive = reader.GetBoolean(FieldNames.fSMSStakeholderUserIsActive);
            stakeholderuser.LastLoginDate = reader.IsDBNull(FieldNames.fSMSStakeholderUserLastLoginDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSStakeholderUserLastLoginDate);
            stakeholderuser.CreatedBy = reader.GetString(FieldNames.fCreatedBy);
            stakeholderuser.CreatedDate = reader.GetDateTime(FieldNames.fCreatedDate);
            
            return stakeholderuser;
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
        dataset.ReportCode = reader.GetValue<string>(FieldNames.fAirportSharedDatasetReportCode).ToString();
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
    /// NOTE: This mapper only handles basic Hazard data from tbld_Hazards
    /// HazardLocation must be populated separately via Application Service layer
    /// </summary>
    public static Hazard MapToHazard(SqlDataReader reader)
    {
        HazardID hazardID = new(reader.GetValue<string>(FieldNames.fHazardCode));
        Hazard hazard = new(hazardID);

        // Core properties - Basic mapping from tbld_Hazards
        hazard.Code = reader.GetValue<string>(FieldNames.fHazardCode) ?? string.Empty;
        hazard.Name = reader.GetValue<string>(FieldNames.fHazardName);
        hazard.Description = reader.GetValue<string>(FieldNames.fHazardDescription).Trim() ?? string.Empty;
        hazard.Category = reader.GetValue<string>(FieldNames.fHazardCategory).Trim();
        hazard.ReportCode = reader.GetValue<string>(FieldNames.fHazardReportCode) ?? string.Empty;
        hazard.RiskMatrixCode = reader.GetValue<string>(FieldNames.fHazardScoringPanelRiskMatrixCode);
        
        // ✅ FIXED: HazardAverageScore is actually a decimal in database, not string
        var averageScore = reader.IsDBNull(FieldNames.fHazardAverageScore) ? (decimal?)null : reader.GetDecimal(FieldNames.fHazardAverageScore);
        hazard.AverageScore = averageScore?.ToString() ?? string.Empty;
        
        // NOTE: HazardLocation is NOT populated here - it must be populated at the Application Service layer
        // to maintain proper separation of concerns and avoid circular dependencies

        // Enhanced properties - if enhanced fields exist in the database
        try
        {
            // Enhanced Classification Fields
            
            var hazardType = reader.GetValue<string>(FieldNames.fHazardType);
            if (!string.IsNullOrEmpty(hazardType))
            {
                hazard.HazardType = hazardType;
            }

            // Reporting Information
            var reportedBy = reader.GetValue<string>(FieldNames.fHazardReportedBy);
            if (!string.IsNullOrEmpty(reportedBy))
            {
                hazard.ReportedBy = reportedBy;
            }

            var reportedOn = reader.IsDBNull(FieldNames.fHazardReportedOn) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fHazardReportedOn);
            hazard.ReportedOn = reportedOn;

            var reportingDepartment = reader.GetValue<string>(FieldNames.fHazardReportingDepartment);
            if (!string.IsNullOrEmpty(reportingDepartment))
            {
                hazard.ReportingDepartment = reportingDepartment;
            }

            // Privacy and Confidentiality
            var isConfidential = reader.IsDBNull(FieldNames.fHazardIsConfidential) ? false : reader.GetBoolean(FieldNames.fHazardIsConfidential);
            hazard.IsConfidential = isConfidential;

            var isAnonymous = reader.IsDBNull(FieldNames.fHazardIsAnonymous) ? false : reader.GetBoolean(FieldNames.fHazardIsAnonymous);
            hazard.IsAnonymous = isAnonymous;

            // Status and Priority (with enum parsing)
            var statusValue = reader.GetValue<string>(FieldNames.fHazardStatus)?.Trim(); // ✅ FIXED: Trim whitespace
            if (!string.IsNullOrEmpty(statusValue))
            {
                hazard.Status = HazardStatus.FromValue(statusValue) ?? HazardStatus.Active;
            }

            var priorityValue = reader.GetValue<string>(FieldNames.fHazardPriority)?.Trim(); // ✅ FIXED: Trim whitespace
            if (!string.IsNullOrEmpty(priorityValue))
            {
                hazard.Priority = HazardPriority.FromValue(priorityValue) ?? HazardPriority.Medium;
            }

            // Risk Level
            var riskLevel = reader.GetValue<string>(FieldNames.fHazardRiskLevel);
            if (!string.IsNullOrEmpty(riskLevel))
            {
                hazard.RiskLevel = riskLevel;
            }

            // Step 3 Risk Analysis Fields
            var worstCredibleOutcome = reader.GetValue<string>(FieldNames.fHazardWorstCredibleOutcome);
            if (!string.IsNullOrEmpty(worstCredibleOutcome))
            {
                hazard.WorstCredibleOutcome = worstCredibleOutcome;
            }

            var rootCause = reader.GetValue<string>(FieldNames.fHazardRootCause);
            if (!string.IsNullOrEmpty(rootCause))
            {
                hazard.RootCause = rootCause;
            }

            // Investigation Properties
            var requiresInvestigation = reader.IsDBNull(FieldNames.fHazardRequiresInvestigation) ? false : reader.GetBoolean(FieldNames.fHazardRequiresInvestigation);
            hazard.RequiresInvestigation = requiresInvestigation;

            var investigationNotes = reader.GetValue<string>(FieldNames.fHazardInvestigationNotes);
            if (!string.IsNullOrEmpty(investigationNotes))
            {
                hazard.InvestigationNotes = investigationNotes;
            }

            // Additional Properties
            var additionalComments = reader.GetValue<string>(FieldNames.fHazardAdditionalComments);
            if (!string.IsNullOrEmpty(additionalComments))
            {
                hazard.AdditionalComments = additionalComments;
            }

            var locationArea = reader.GetValue<string>(FieldNames.fHazardLocationArea);
            if (!string.IsNullOrEmpty(locationArea))
            {
                hazard.LocationArea = locationArea;
            }

            var locationSubArea = reader.GetValue<string>(FieldNames.fHazardLocationSubArea);
            if (!string.IsNullOrEmpty(locationSubArea))
            {
                hazard.LocationSubArea = locationSubArea;
            }

            // 5M Component (Smart Enum)
            var fiveMComponentValue = reader.GetValue<string>(FieldNames.fHazardFiveMComponent);
            if (!string.IsNullOrEmpty(fiveMComponentValue))
            {
                hazard.FiveMComponent = FiveMComponent.FromValue(fiveMComponentValue) ?? FiveMComponent.FromName(fiveMComponentValue);
            }
        }
        catch (Exception)
        {
            // Enhanced fields might not exist in older database schemas
            // Continue with basic mapping - enhanced fields will have default values
        }

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
        report.Status = reader.GetValue<string>(FieldNames.fReportStatus)?.Trim(); // ✅ FIXED: Trim whitespace
        report.Stage = reader.GetValue<string>(FieldNames.fReportStage)?.Trim();   // ✅ FIXED: Trim whitespace

        return report;
    }

    /// <summary>
    /// Maps SqlDataReader to Investigation entity with enhanced properties
    /// </summary>
    public static Investigation MapToInvestigation(SqlDataReader reader)
    {
        InvestigationID investigationId = new InvestigationID(reader.GetValue<string>(FieldNames.fInvestigationCode));
        Investigation investigation = new Investigation(investigationId);
        
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
        investigation.Status = InvestigationStatus.FromValue(statusValue) ?? InvestigationStatus.InProgress;
        
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
        interview.Status = InterviewStatus.FromValue(statusValue) ?? InterviewStatus.Planned;
        
        interview.InterviewDate = reader.GetValue<DateTime?>(FieldNames.fInterviewDate);
        interview.DurationMinutes = reader.GetValue<int?>(FieldNames.fInterviewDurationMinutes);
        interview.InterviewLocation = reader.GetValue<string>(FieldNames.fInterviewLocation);
        
        var typeValue = reader.GetValue<string>(FieldNames.fInterviewType);
        interview.Type = InterviewType.FromValue(typeValue) ?? InterviewType.Witness;
        
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
        riskAnalysis.HazardCode = reader.GetValue<string>(FieldNames.fRiskAnalysisHazardCode);
        riskAnalysis.RiskAssessmentCode = reader.GetValue<string>(FieldNames.fRiskAnalysisRiskAssessmentCode);
        riskAnalysis.WorstCredibleOutcome = reader.GetValue<string>(FieldNames.fRiskAnalysisWorstCredibleOutcome);
        riskAnalysis.RootCause = reader.GetValue<string>(FieldNames.fRiskAnalysisRootCause);
        riskAnalysis.AdditionalComments = reader.GetValue<string>(FieldNames.fRiskAnalysisAdditionalComments);

        return riskAnalysis;
    }

    /// <summary>
    /// Maps SqlDataReader to RiskAssessment entity - UPDATED FOR SMARTENUM SUPPORT
    /// Following the preferred pattern: direct field mapping with GetValue<T>
    /// </summary>
    public static RiskAssessment MapToRiskAssessment(SqlDataReader reader)
    {
        // ✅ Clean pattern: Direct field extraction and entity creation
        var code = reader.GetValue<string>(FieldNames.fRiskAssessmentCode);
        var riskAssessmentId = new RiskAssessmentID(code);
        var riskAssessment = new RiskAssessment(riskAssessmentId);

        // ✅ SIMPLIFIED: Direct property assignment - NO REFLECTION!
        riskAssessment.Name = reader.GetValue<string>(FieldNames.fRiskAssessmentName);
        riskAssessment.Description = reader.GetValue<string>(FieldNames.fRiskAssessmentDescription);
        riskAssessment.HazardCode = reader.GetValue<string>(FieldNames.fRiskAssessmentHazardCode);
        riskAssessment.Code = code;

        // ✅ SmartEnum parsing for AssessmentType
        var assessmentTypeValue = reader.GetValue<string>(FieldNames.fRiskAssessmentType)?.Trim();
        if (!string.IsNullOrEmpty(assessmentTypeValue))
        {
            riskAssessment.AssessmentType = RiskAssessmentType.FromValue(assessmentTypeValue) ?? RiskAssessmentType.Initial;
        }

        // ✅ SmartEnum parsing for Status
        var statusValue = reader.GetValue<string>(FieldNames.fRiskAssessmentStatus)?.Trim();
        if (!string.IsNullOrEmpty(statusValue))
        {
            riskAssessment.Status = RiskAssessmentStatus.FromValue(statusValue) ?? RiskAssessmentStatus.Created;
        }

        // ✅ Direct assignment for simple properties
        riskAssessment.Stage = reader.GetValue<string>(FieldNames.fRiskAssessmentStage);
        riskAssessment.LeadAssessorId = reader.GetValue<string>(FieldNames.fRiskAssessmentLeadAssessorId);
        riskAssessment.PrimaryHazardId = reader.GetValue<string>(FieldNames.fRiskAssessmentPrimaryHazardId);

        // ✅ SmartEnum parsing for Category
        var categoryValue = reader.GetValue<string>(FieldNames.fRiskAssessmentCategory)?.Trim();
        if (!string.IsNullOrEmpty(categoryValue))
        {
            riskAssessment.RiskAssessmentCategory = RiskAssessmentCategory.FromValue(categoryValue) ?? RiskAssessmentCategory.Technical;
        }

        // ✅ Integer fields with null handling
        riskAssessment.CurrentStep = reader.IsDBNull(FieldNames.fRiskAssessmentCurrentStep) ? 1 : reader.GetValue<int>(FieldNames.fRiskAssessmentCurrentStep);

        // ✅ DateTime fields with null handling  
        riskAssessment.CompletedDate = reader.IsDBNull(FieldNames.fRiskAssessmentCompletedDate) ? null : reader.GetValue<DateTime?>(FieldNames.fRiskAssessmentCompletedDate);
        riskAssessment.CompletedBy = reader.GetValue<string>(FieldNames.fRiskAssessmentCompletedBy);
        riskAssessment.ParentAssessmentId = reader.GetValue<string>(FieldNames.fRiskAssessmentParentAssessmentId);

        // ✅ Step 1 - System Description Fields (now with public setters!)
        riskAssessment.SystemDescription = reader.GetValue<string>(FieldNames.fRiskAssessmentSystemDescription) ?? string.Empty;
        riskAssessment.SystemBoundaries = reader.GetValue<string>(FieldNames.fRiskAssessmentSystemBoundaries) ?? string.Empty;
        riskAssessment.SystemPurpose = reader.GetValue<string>(FieldNames.fRiskAssessmentSystemPurpose) ?? string.Empty;

        // ✅ 5M Framework Fields - Direct assignment!
        riskAssessment.FiveMPersonnel = reader.GetValue<string>(FieldNames.fRiskAssessmentFiveMPersonnel) ?? string.Empty;
        riskAssessment.FiveMEquipment = reader.GetValue<string>(FieldNames.fRiskAssessmentFiveMEquipment) ?? string.Empty;
        riskAssessment.FiveMProcedures = reader.GetValue<string>(FieldNames.fRiskAssessmentFiveMProcedures) ?? string.Empty;
        riskAssessment.FiveMResources = reader.GetValue<string>(FieldNames.fRiskAssessmentFiveMResources) ?? string.Empty;
        riskAssessment.FiveMPhysicalEnvironment = reader.GetValue<string>(FieldNames.fRiskAssessmentFiveMPhysicalEnvironment) ?? string.Empty;
        riskAssessment.FiveMOperationalEnvironment = reader.GetValue<string>(FieldNames.fRiskAssessmentFiveMOperationalEnvironment) ?? string.Empty;

        // ✅ Step 3 - Risk Analysis Fields
        riskAssessment.RiskAnalysisMethod = reader.GetValue<string>(FieldNames.fRiskAssessmentRiskAnalysisMethod) ?? "SMS Risk Matrix";
        riskAssessment.RiskCriteria = reader.GetValue<string>(FieldNames.fRiskAssessmentRiskCriteria) ?? string.Empty;

        // ✅ Step 4 - Risk Assessment Fields
        riskAssessment.TolerabilityFramework = reader.GetValue<string>(FieldNames.fRiskAssessmentTolerabilityFramework) ?? "PDX-SMS Default";
        riskAssessment.RiskAcceptanceCriteria = reader.GetValue<string>(FieldNames.fRiskAssessmentRiskAcceptanceCriteria) ?? string.Empty;
        riskAssessment.FinalSeverityScore = reader.IsDBNull(FieldNames.fRiskAssessmentFinalSeverityScore) ? null : reader.GetValue<int?>(FieldNames.fRiskAssessmentFinalSeverityScore);
        riskAssessment.FinalLikelihoodScore = reader.IsDBNull(FieldNames.fRiskAssessmentFinalLikelihoodScore) ? null : reader.GetValue<int?>(FieldNames.fRiskAssessmentFinalLikelihoodScore);
        riskAssessment.FinalRiskLevel = reader.GetValue<string>(FieldNames.fRiskAssessmentFinalRiskLevel);
        riskAssessment.RiskTolerability = reader.GetValue<string>(FieldNames.fRiskAssessmentRiskTolerability) ?? "ALARP";
        riskAssessment.AssessmentRationale = reader.GetValue<string>(FieldNames.fRiskAssessmentAssessmentRationale);

        // ✅ Step 5 - Implementation Fields
        riskAssessment.ImplementationStrategy = reader.GetValue<string>(FieldNames.fRiskAssessmentImplementationStrategy) ?? string.Empty;
        riskAssessment.OverallTargetDate = reader.IsDBNull(FieldNames.fRiskAssessmentOverallTargetDate) ? null : reader.GetValue<DateTime?>(FieldNames.fRiskAssessmentOverallTargetDate);
        riskAssessment.ImplementationNotes = reader.GetValue<string>(FieldNames.fRiskAssessmentImplementationNotes) ?? string.Empty;

        return riskAssessment;
    }

    /// <summary>
    /// Maps SqlDataReader to Mitigation entity - COMPLETE SCHEMA MAPPING
    /// </summary>
    public static Mitigation MapToMitigation(SqlDataReader reader)
    {
        MitigationID mitigationID = new(reader.GetValue<string>(FieldNames.fMitigationCode).ToString());
        Mitigation mitigation = new(mitigationID);

        // Basic Properties
        mitigation.Code = reader.GetValue<string>(FieldNames.fMitigationCode);
        mitigation.HazardCode = reader.GetValue<string>(FieldNames.fMitigationHazardCode);
        mitigation.Name = reader.GetValue<string>(FieldNames.fMitigationName);
        mitigation.Description = reader.GetValue<string>(FieldNames.fMitigationDescription);
        mitigation.Type = reader.GetValue<string>(FieldNames.fMitigationType);
        mitigation.Status = reader.GetValue<string>(FieldNames.fMitigationStatus);
        mitigation.Priority = reader.GetValue<string>(FieldNames.fMitigationPriority);
        mitigation.RiskAssessmentCode = reader.GetValue<string>(FieldNames.fMitigationRiskAssessmentCode);

        // Date Properties
        mitigation.TargetDate = reader.IsDBNull(FieldNames.fMitigationTargetDate) ? null : reader.GetValue<DateTime?>(FieldNames.fMitigationTargetDate);
        mitigation.ImplementationDate = reader.IsDBNull(FieldNames.fMitigationImplementationDate) ? null : reader.GetValue<DateTime?>(FieldNames.fMitigationImplementationDate);
        mitigation.CompletionDate = reader.IsDBNull(FieldNames.fMitigationCompletionDate) ? null : reader.GetValue<DateTime?>(FieldNames.fMitigationCompletionDate);

        // Assignment Properties
        mitigation.AssignedDepartment = reader.GetValue<string>(FieldNames.fMitigationAssignedDepartment);
        mitigation.AssignedTo = reader.GetValue<string>(FieldNames.fMitigationAssignedTo);
        mitigation.ApprovedBy = reader.GetValue<string>(FieldNames.fMitigationApprovedBy);
        mitigation.ApprovedDate = reader.IsDBNull(FieldNames.fMitigationApprovedDate) ? null : reader.GetValue<DateTime?>(FieldNames.fMitigationApprovedDate);

        // Progress Properties - ✅ FIXED: Progress is int (not nullable)
        mitigation.Progress = reader.IsDBNull(FieldNames.fMitigationProgress) ? 0 : reader.GetValue<int>(FieldNames.fMitigationProgress);
        mitigation.ProgressNotes = reader.GetValue<string>(FieldNames.fMitigationProgressNotes);
        mitigation.LastProgressUpdate = reader.IsDBNull(FieldNames.fMitigationLastProgressUpdate) ? null : reader.GetValue<DateTime?>(FieldNames.fMitigationLastProgressUpdate);
        mitigation.ProgressUpdatedBy = reader.GetValue<string>(FieldNames.fMitigationProgressUpdatedBy);

        // Cost and Resource Properties
        mitigation.EstimatedCost = reader.IsDBNull(FieldNames.fMitigationEstimatedCost) ? null : reader.GetValue<decimal?>(FieldNames.fMitigationEstimatedCost);
        mitigation.ActualCost = reader.IsDBNull(FieldNames.fMitigationActualCost) ? null : reader.GetValue<decimal?>(FieldNames.fMitigationActualCost);
        mitigation.ResourceRequirements = reader.GetValue<string>(FieldNames.fMitigationResourceRequirements);
        mitigation.EstimatedHours = reader.IsDBNull(FieldNames.fMitigationEstimatedHours) ? null : reader.GetValue<int?>(FieldNames.fMitigationEstimatedHours);
        mitigation.ActualHours = reader.IsDBNull(FieldNames.fMitigationActualHours) ? null : reader.GetValue<int?>(FieldNames.fMitigationActualHours);

        // Effectiveness Properties
        mitigation.EffectivenessRating = reader.GetValue<string>(FieldNames.fMitigationEffectivenessRating);
        mitigation.EffectivenessNotes = reader.GetValue<string>(FieldNames.fMitigationEffectivenessNotes);
        mitigation.EffectivenessReviewDate = reader.IsDBNull(FieldNames.fMitigationEffectivenessReviewDate) ? null : reader.GetValue<DateTime?>(FieldNames.fMitigationEffectivenessReviewDate);
        mitigation.EffectivenessReviewedBy = reader.GetValue<string>(FieldNames.fMitigationEffectivenessReviewedBy);

        // Monitoring Properties
        mitigation.MonitoringRequirements = reader.GetValue<string>(FieldNames.fMitigationMonitoringRequirements);
        mitigation.MonitoringFrequency = reader.GetValue<string>(FieldNames.fMitigationMonitoringFrequency);

        // Risk Reduction Properties
        mitigation.ExpectedSeverityReduction = reader.IsDBNull(FieldNames.fMitigationExpectedSeverityReduction) ? null : reader.GetValue<int?>(FieldNames.fMitigationExpectedSeverityReduction);
        mitigation.ExpectedLikelihoodReduction = reader.IsDBNull(FieldNames.fMitigationExpectedLikelihoodReduction) ? null : reader.GetValue<int?>(FieldNames.fMitigationExpectedLikelihoodReduction);
        mitigation.ActualSeverityReduction = reader.IsDBNull(FieldNames.fMitigationActualSeverityReduction) ? null : reader.GetValue<int?>(FieldNames.fMitigationActualSeverityReduction);
        mitigation.ActualLikelihoodReduction = reader.IsDBNull(FieldNames.fMitigationActualLikelihoodReduction) ? null : reader.GetValue<int?>(FieldNames.fMitigationActualLikelihoodReduction);
        mitigation.ResidualRiskLevel = reader.GetValue<string>(FieldNames.fMitigationResidualRiskLevel);

        // Dependency Properties - ✅ FIXED: bool properties are not nullable in entity
        mitigation.Prerequisites = reader.GetValue<string>(FieldNames.fMitigationPrerequisites);
        mitigation.Dependencies = reader.GetValue<string>(FieldNames.fMitigationDependencies);
        mitigation.HasDependencies = reader.IsDBNull(FieldNames.fMitigationHasDependencies) ? false : reader.GetValue<bool>(FieldNames.fMitigationHasDependencies);
        mitigation.IsPrerequisite = reader.IsDBNull(FieldNames.fMitigationIsPrerequisite) ? false : reader.GetValue<bool>(FieldNames.fMitigationIsPrerequisite);

        // Plan Properties
        mitigation.ImplementationPlan = reader.GetValue<string>(FieldNames.fMitigationImplementationPlan);
        mitigation.CommunicationPlan = reader.GetValue<string>(FieldNames.fMitigationCommunicationPlan);
        mitigation.TrainingRequirements = reader.GetValue<string>(FieldNames.fMitigationTrainingRequirements);
        mitigation.DocumentationUpdates = reader.GetValue<string>(FieldNames.fMitigationDocumentationUpdates);

        // Testing Properties
        mitigation.TestingProcedure = reader.GetValue<string>(FieldNames.fMitigationTestingProcedure);
        mitigation.TestingCompletedDate = reader.IsDBNull(FieldNames.fMitigationTestingCompletedDate) ? null : reader.GetValue<DateTime?>(FieldNames.fMitigationTestingCompletedDate);
        mitigation.TestingResults = reader.GetValue<string>(FieldNames.fMitigationTestingResults);

        // Validation Properties - ✅ FIXED: ValidationRequired is bool (not nullable)
        mitigation.ValidationRequired = reader.IsDBNull(FieldNames.fMitigationValidationRequired) ? false : reader.GetValue<bool>(FieldNames.fMitigationValidationRequired);
        mitigation.ValidationDate = reader.IsDBNull(FieldNames.fMitigationValidationDate) ? null : reader.GetValue<DateTime?>(FieldNames.fMitigationValidationDate);
        mitigation.ValidatedBy = reader.GetValue<string>(FieldNames.fMitigationValidatedBy);

        // Documentation Properties
        mitigation.Notes = reader.GetValue<string>(FieldNames.fMitigationNotes);
        mitigation.LessonsLearned = reader.GetValue<string>(FieldNames.fMitigationLessonsLearned);
        mitigation.RecommendationsForFuture = reader.GetValue<string>(FieldNames.fMitigationRecommendationsForFuture);

        // Audit Properties (inherited from BaseAuditableEntity)
        mitigation.CreatedBy = reader.GetValue<string>(FieldNames.fCreatedBy) ?? "SYSTEM";
        mitigation.CreatedDate = reader.IsDBNull(FieldNames.fCreatedDate) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fCreatedDate);
        mitigation.UpdatedBy = reader.GetValue<string>(FieldNames.fUpdatedBy);
        mitigation.UpdatedDate = reader.IsDBNull(FieldNames.fUpdatedDate) ? null : reader.GetValue<DateTime?>(FieldNames.fUpdatedDate);

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
        try
        {
            var code = reader.GetValue<string>(FieldNames.fReportValidationCode) ?? string.Empty;
            ReportValidationID reportValidationID = new(code);
            ReportValidation reportValidation = new(reportValidationID);

            reportValidation.Code = code;
            reportValidation.ReportCode = reader.GetValue<string>(FieldNames.fReportValidationReportCode);
            reportValidation.ValidationDecision = reader.GetValue<string>(FieldNames.fReportValidationDecision);
            reportValidation.Status = reader.GetValue<string>(FieldNames.fReportValidationStatus);
            reportValidation.Stage = reader.GetValue<string>(FieldNames.fReportValidationStage);
            reportValidation.ValidationType = reader.GetValue<string>(FieldNames.fReportValidationType);
            reportValidation.ValidationComments =  reader.GetValue<string>(FieldNames.fReportValidationComments);
            reportValidation.ValidatedBy = reader.GetValue<string>(FieldNames.fReportValidationValidatedBy);
            // Set audit properties using reflection since they have private setters
            var baseEntityType = typeof(BaseAuditableEntity);
            var createdBy = reader.GetValue<string>(FieldNames.fCreatedBy) ?? "SYSTEM";
            var createdDate = reader.IsDBNull(FieldNames.fCreatedDate) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fCreatedDate);
            var updatedBy = reader.GetValue<string>(FieldNames.fUpdatedBy);
            var updatedDate = reader.IsDBNull(FieldNames.fUpdatedDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fUpdatedDate);

            baseEntityType.GetProperty("CreatedBy")?.SetValue(reportValidation, createdBy);
            baseEntityType.GetProperty("CreatedDate")?.SetValue(reportValidation, createdDate);
            baseEntityType.GetProperty("UpdatedBy")?.SetValue(reportValidation, updatedBy);
            baseEntityType.GetProperty("UpdatedDate")?.SetValue(reportValidation, updatedDate);

            return reportValidation;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to ReportValidation: {ex.Message}", ex);
        }
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
        scoringPanel.RiskAssessmentCode = reader.GetValue<string>(FieldNames.fScoringPanelRiskAssessmentCode);
        scoringPanel.SMSUserCode = reader.GetValue<string>(FieldNames.fScoringPanelSMSUserCode);
        scoringPanel.Likelihood = reader.GetValue<int?>(FieldNames.fScoringPanelLikelihood);
        scoringPanel.Severity = reader.GetValue<int?>(FieldNames.fScoringPanelSeverity);
        scoringPanel.Score = reader.GetValue<decimal?>(FieldNames.fScoringPanelScore);
        scoringPanel.Rationale = reader.GetValue<string>(FieldNames.fScoringPanelRationale);

        return scoringPanel;
    }

    /// <summary>
    /// Maps SqlDataReader to SMSApplicationUserRole entity - CORRECTED to match actual database schema
    /// </summary>
    //public static SMSApplicationUserRole MapToSMSUserRole(SqlDataReader reader)
    //{
    //    try
    //    {
    //        // Extract database values - using ACTUAL database field names from tbld_SMSUserRoles
    //        var code = reader.GetString(FieldNames.fSMSUserRoleCode);
    //        var userId = reader.GetString(FieldNames.fSMSUserRoleUserId);
    //        var userType = reader.GetString(FieldNames.fSMSUserRoleUserType);
    //        var smsRoleCode = reader.GetString(FieldNames.fSMSUserRoleSMSRoleCode);  // ✅ CORRECTED: Using actual FK field
    //        var department = reader.GetString(FieldNames.fSMSUserRoleDepartment);
    //        var effectiveDate = reader.GetDateTime(FieldNames.fSMSUserRoleEffectiveDate);
    //        var expirationDate = reader.IsDBNull(FieldNames.fSMSUserRoleExpirationDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSUserRoleExpirationDate);
    //        var isActive = reader.GetBoolean(FieldNames.fSMSUserRoleIsActive);
    //        var assignedBy = reader.GetString(FieldNames.fSMSUserRoleAssignedBy);
    //        var assignedDate = reader.GetDateTime(FieldNames.fSMSUserRoleAssignedDate);  // ✅ CORRECTED: Added missing field
    //        var deactivatedBy = reader.IsDBNull(FieldNames.fSMSUserRoleDeactivatedBy) ? null : reader.GetString(FieldNames.fSMSUserRoleDeactivatedBy);
    //        var deactivatedDate = reader.IsDBNull(FieldNames.fSMSUserRoleDeactivatedDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fSMSUserRoleDeactivatedDate);
    //        var createdBy = reader.GetString(FieldNames.fCreatedBy);
    //        var createdDate = reader.GetDateTime(FieldNames.fCreatedDate);

    //        // ✅ CORRECTED: Get the SMS Role using the SMSRoleCode (foreign key relationship)
    //        // Note: This requires looking up the role from tbld_SMSRoles table using smsRoleCode
    //        // For now, we'll create a basic role structure based on the FK
    //        var role = SMSRole.GetAllValues().FirstOrDefault(r => r.Value == smsRoleCode);
    //        if (role == null)
    //        {
    //            // If role not found in enum, we need role data from tbld_SMSRoles table
    //            // This is expected since roles are stored separately and linked via FK
    //            throw new InvalidOperationException($"SMS Role with code '{smsRoleCode}' not found in SMSRole enumeration. Role data should be joined from tbld_SMSRoles table.");
    //        }

    //        // Create the entity using the factory method
    //        var userRole = SMSApplicationUserRole.Create(
    //            userId,
    //            userType,
    //            role,
    //            department,
    //            assignedBy,
    //            effectiveDate,
    //            expirationDate,
    //            null); // ✅ CORRECTED: No AssignmentNotes field in database

    //        // Set the code from database
    //        var userRoleId = new SMSApplicationUserRoleID(code);
    //        var idProperty = typeof(SMSApplicationUserRole).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    //        idProperty?.SetValue(userRole, userRoleId);

    //        // Set additional properties that aren't part of creation
    //        if (!isActive && deactivatedBy != null)
    //        {
    //            userRole.Deactivate(deactivatedBy, "Role deactivated");  // ✅ CORRECTED: No DeactivationReason field in database
    //        }

    //        // Set audit fields using reflection since they might be private setters
    //        var baseEntity = typeof(BaseAuditableEntity);
    //        baseEntity.GetProperty("CreatedBy")?.SetValue(userRole, createdBy);
    //        baseEntity.GetProperty("CreatedDate")?.SetValue(userRole, createdDate);

    //        return userRole;
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new InvalidOperationException($"Error mapping SqlDataReader to SMSUserRole: {ex.Message}", ex);
    //    }
    //}

    /// <summary>
    /// Maps a SqlDataReader to an SMSStakeholderGroup entity
    /// </summary>
    /// <param name="reader">The SqlDataReader containing SMS stakeholder group data</param>
    /// <returns>A new SMSStakeholderGroup entity</returns>
    public static SMSStakeholderGroup MapToSMSStakeholderGroup(SqlDataReader reader)
    {
        try
        {
            var code = reader.GetValue<string>(FieldNames.fSMSStakeholderGroupCode) ?? string.Empty;
            SMSStakeholderGroupID id = new SMSStakeholderGroupID(code); 
            SMSStakeholderGroup stakeholderGroup = new SMSStakeholderGroup(id);
            stakeholderGroup.Code = code;
            stakeholderGroup.Name = reader.GetValue<string>(FieldNames.fSMSStakeholderGroupName) ?? string.Empty;
            stakeholderGroup.CreatedBy = reader.GetValue<string>(FieldNames.fCreatedBy) ?? "SYSTEM";
            stakeholderGroup.CreatedDate = reader.IsDBNull(FieldNames.fCreatedDate) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fCreatedDate);
            stakeholderGroup.Description = reader.GetValue<string>(FieldNames.fSMSStakeholderGroupDescription) ?? string.Empty;
            stakeholderGroup.IsActive = reader.IsDBNull(FieldNames.fSMSStakeholderGroupIsActive) ? true : reader.GetBoolean(FieldNames.fSMSStakeholderGroupIsActive);


            return stakeholderGroup;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to SMSStakeholderGroup: {ex.Message}", ex);
        }
    }

    public static SMSApplicationGroup MapToSMSApplicationGroup(SqlDataReader reader)
    {
        try
        {
            var code = reader.GetValue<string>(FieldNames.fSMSApplicationGroupCode) ?? string.Empty;
            SMSApplicationGroupID id = new SMSApplicationGroupID(code);
            SMSApplicationGroup applicationGroup = new SMSApplicationGroup(id);
            
            applicationGroup.Code = code;
            applicationGroup.Name = reader.GetValue<string>(FieldNames.fSMSApplicationGroupName) ?? string.Empty;
            applicationGroup.Description = reader.GetValue<string>(FieldNames.fSMSApplicationGroupDescription) ?? string.Empty;
            applicationGroup.IsActive = reader.IsDBNull(FieldNames.fSMSApplicationGroupIsActive) ? true : reader.GetBoolean(FieldNames.fSMSApplicationGroupIsActive);
            applicationGroup.CreatedBy = reader.GetValue<string>(FieldNames.fCreatedBy) ?? "SYSTEM";
            applicationGroup.CreatedDate = reader.IsDBNull(FieldNames.fCreatedDate) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fCreatedDate);
            applicationGroup.UpdatedBy = reader.GetValue<string>(FieldNames.fUpdatedBy);
            applicationGroup.UpdatedDate = reader.IsDBNull(FieldNames.fUpdatedDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fUpdatedDate);

            return applicationGroup;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to SMSApplicationGroup: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps SqlDataReader to SMSOrganizationalGroup entity
    /// </summary>
    public static SMSOrganizationalGroup MapToSMSOrganizationalGroup(SqlDataReader reader)
    {
        try
        {
            var code = reader.GetValue<string>(FieldNames.fSMSOrganizationalGroupCode) ?? string.Empty;
            SMSOrganizationalGroupID id = new SMSOrganizationalGroupID(code);
            SMSOrganizationalGroup organizationalGroup = new SMSOrganizationalGroup(id);
            
            organizationalGroup.Code = code;
            organizationalGroup.Name = reader.GetValue<string>(FieldNames.fSMSOrganizationalGroupName) ?? string.Empty;
            organizationalGroup.Description = reader.GetValue<string>(FieldNames.fSMSOrganizationalGroupDescription) ?? string.Empty;
            organizationalGroup.GroupType = reader.GetValue<string>(FieldNames.fSMSOrganizationalGroupGroupType) ?? "Department";
            organizationalGroup.AuthorityLevel = reader.GetValue<string>(FieldNames.fSMSOrganizationalGroupAuthorityLevel) ?? "Standard";
            organizationalGroup.IsActive = reader.IsDBNull(FieldNames.fSMSOrganizationalGroupIsActive) ? true : reader.GetBoolean(FieldNames.fSMSOrganizationalGroupIsActive);
            organizationalGroup.CreatedBy = reader.GetValue<string>(FieldNames.fCreatedBy) ?? "SYSTEM";
            organizationalGroup.CreatedDate = reader.IsDBNull(FieldNames.fCreatedDate) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fCreatedDate);
            organizationalGroup.UpdatedBy = reader.GetValue<string>(FieldNames.fUpdatedBy);
            organizationalGroup.UpdatedDate = reader.IsDBNull(FieldNames.fUpdatedDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fUpdatedDate);

            return organizationalGroup;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error mapping SqlDataReader to SMSOrganizationalGroup: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps SqlDataReader to HazardFile entity
    /// </summary>
    public static HazardFile MapToHazardFile(SqlDataReader reader)
    {
        var code = reader.GetValue<string>(FieldNames.fHazardFileCode);
        HazardFileID hazardFileID = new(code);
        HazardFile hazardFile = new(hazardFileID);

        hazardFile.Code = code ?? string.Empty;
        hazardFile.HazardCode = reader.GetValue<string>(FieldNames.fHazardFileHazardCode) ?? string.Empty;
        hazardFile.ReportCode = reader.GetValue<string>(FieldNames.fHazardFileReportCode);
        hazardFile.FileName = reader.GetValue<string>(FieldNames.fHazardFileFileName) ?? string.Empty;
        hazardFile.FileType = reader.GetValue<string>(FieldNames.fHazardFileFileType) ?? string.Empty;
        hazardFile.ContentType = reader.GetValue<string>(FieldNames.fHazardFileContentType) ?? string.Empty;
        hazardFile.FileSizeBytes = reader.GetValue<long>(FieldNames.fHazardFileFileSizeBytes);
        hazardFile.FileHash = reader.GetValue<string>(FieldNames.fHazardFileFileHash);
        hazardFile.StorageType = reader.GetValue<string>(FieldNames.fHazardFileStorageType) ?? "FileSystem";
        hazardFile.FilePath = reader.GetValue<string>(FieldNames.fHazardFileFilePath);
        hazardFile.FileData = reader.GetValue<byte[]>(FieldNames.fHazardFileFileData);
        hazardFile.Description = reader.GetValue<string>(FieldNames.fHazardFileDescription);
        hazardFile.Category = reader.GetValue<string>(FieldNames.fHazardFileCategory);
        hazardFile.IsConfidential = reader.IsDBNull(FieldNames.fHazardFileIsConfidential) ? false : reader.GetBoolean(FieldNames.fHazardFileIsConfidential);
        hazardFile.UploadedBy = reader.GetValue<string>(FieldNames.fHazardFileUploadedBy) ?? string.Empty;
        hazardFile.UploadedDate = reader.IsDBNull(FieldNames.fHazardFileUploadedDate) ? DateTime.UtcNow : reader.GetDateTime(FieldNames.fHazardFileUploadedDate);
        hazardFile.Tags = reader.GetValue<string>(FieldNames.fHazardFileTags);
        hazardFile.IsActive = reader.IsDBNull(FieldNames.fHazardFileIsActive) ? true : reader.GetBoolean(FieldNames.fHazardFileIsActive);
        hazardFile.InactiveReason = reader.GetValue<string>(FieldNames.fHazardFileInactiveReason);
        hazardFile.InactiveDate = reader.IsDBNull(FieldNames.fHazardFileInactiveDate) ? (DateTime?)null : reader.GetDateTime(FieldNames.fHazardFileInactiveDate);
        hazardFile.InactiveBy = reader.GetValue<string>(FieldNames.fHazardFileInactiveBy);

        return hazardFile;
    }

    #endregion
}