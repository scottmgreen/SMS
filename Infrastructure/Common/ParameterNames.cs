//-----------------------------------------------------------------------
// <copyright file="ParameterNames.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Parameter name constants providing standardized stored procedure parameter naming and validation.
//                  Infrastructure utility providing shared functionality
//                  for data access and external system integration.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Common;

public static class ParameterNames
{
    /// <summary>
    /// SMS User Role parameters (SMS role assignment related)
    /// </summary>
    private static readonly Lazy<string> _pmRoleValue = new Lazy<string>(() => "@pRoleValue");
    public static string pmRoleValue => _pmRoleValue.Value;

    private static readonly Lazy<string> _pmRoleName = new Lazy<string>(() => "@pName");
    public static string pmRoleName => _pmRoleName.Value;

    private static readonly Lazy<string> _pmRoleCategory = new Lazy<string>(() => "@pRoleCategory");
    public static string pmRoleCategory => _pmRoleCategory.Value;

    private static readonly Lazy<string> _pmEffectiveDate = new Lazy<string>(() => "@pEffectiveDate");
    public static string pmEffectiveDate => _pmEffectiveDate.Value;

    private static readonly Lazy<string> _pmExpirationDate = new Lazy<string>(() => "@pExpirationDate");
    public static string pmExpirationDate => _pmExpirationDate.Value;

    private static readonly Lazy<string> _pmAssignedBy = new Lazy<string>(() => "@pAssignedBy");
    public static string pmAssignedBy => _pmAssignedBy.Value;

    private static readonly Lazy<string> _pmAssignmentNotes = new Lazy<string>(() => "@pAssignmentNotes");
    public static string pmAssignmentNotes => _pmAssignmentNotes.Value;

    private static readonly Lazy<string> _pmDeactivationReason = new Lazy<string>(() => "@pDeactivationReason");
    public static string pmDeactivationReason => _pmDeactivationReason.Value;

    private static readonly Lazy<string> _pmDeactivatedDate = new Lazy<string>(() => "@pDeactivatedDate");
    public static string pmDeactivatedDate => _pmDeactivatedDate.Value;

    private static readonly Lazy<string> _pmDeactivatedBy = new Lazy<string>(() => "@pDeactivatedBy");
    public static string pmDeactivatedBy => _pmDeactivatedBy.Value;

    private static readonly Lazy<string> _pmCutoffDate = new Lazy<string>(() => "@pCutoffDate");
    public static string pmCutoffDate => _pmCutoffDate.Value;


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
    /// SMS Application User parameters
    /// </summary>
    private static readonly Lazy<string> _pmSMSApplicationUserId = new Lazy<string>(() => "@pID");
    public static string pmSMSApplicationUserId => _pmSMSApplicationUserId.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSApplicationUserCode => _pmSMSApplicationUserCode.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserFirstName = new Lazy<string>(() => "@pFirstName");
    public static string pmSMSApplicationUserFirstName => _pmSMSApplicationUserFirstName.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserLastName = new Lazy<string>(() => "@pLastName");
    public static string pmSMSApplicationUserLastName => _pmSMSApplicationUserLastName.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserUserName = new Lazy<string>(() => "@pUserName");
    public static string pmSMSApplicationUserUserName => _pmSMSApplicationUserUserName.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserPassword = new Lazy<string>(() => "@pPassword");
    public static string pmSMSApplicationUserPassword => _pmSMSApplicationUserPassword.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserType = new Lazy<string>(() => "@pApplicationUserType");
    public static string pmSMSApplicationUserType => _pmSMSApplicationUserType.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserRole = new Lazy<string>(() => "@pSMSUserRole");
    public static string pmSMSApplicationUserRole => _pmSMSApplicationUserRole.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserIsActive = new Lazy<string>(() => "@pIsActive");
    public static string pmSMSApplicationUserIsActive => _pmSMSApplicationUserIsActive.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserLastLoginDate = new Lazy<string>(() => "@pLastLoginDate");
    public static string pmSMSApplicationUserLastLoginDate => _pmSMSApplicationUserLastLoginDate.Value;

    private static readonly Lazy<string> _pmSMSApplicationUserLoginDate = new Lazy<string>(() => "@pLoginDate");
    public static string pmSMSApplicationUserLoginDate => _pmSMSApplicationUserLoginDate.Value;

    /// <summary>
    /// SMS Organizational User parameters
    /// </summary>
    private static readonly Lazy<string> _pmSMSOrganizationalUserId = new Lazy<string>(() => "@pID");
    public static string pmSMSOrganizationalUserId => _pmSMSOrganizationalUserId.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserCode = new Lazy<string>(() => "@pUserCode");
    public static string pmSMSOrganizationalUserCode => _pmSMSOrganizationalUserCode.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserFirstName = new Lazy<string>(() => "@pFirstName");
    public static string pmSMSOrganizationalUserFirstName => _pmSMSOrganizationalUserFirstName.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserLastName = new Lazy<string>(() => "@pLastName");
    public static string pmSMSOrganizationalUserLastName => _pmSMSOrganizationalUserLastName.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserUserName = new Lazy<string>(() => "@pUserName");
    public static string pmSMSOrganizationalUserUserName => _pmSMSOrganizationalUserUserName.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserPassword = new Lazy<string>(() => "@pPassword");
    public static string pmSMSOrganizationalUserPassword => _pmSMSOrganizationalUserPassword.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserDepartment = new Lazy<string>(() => "@pDepartment");
    public static string pmSMSOrganizationalUserDepartment => _pmSMSOrganizationalUserDepartment.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserPosition = new Lazy<string>(() => "@pPosition");
    public static string pmSMSOrganizationalUserPosition => _pmSMSOrganizationalUserPosition.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserOrganizationLevel = new Lazy<string>(() => "@pOrganizationLevel");
    public static string pmSMSOrganizationalUserOrganizationLevel => _pmSMSOrganizationalUserOrganizationLevel.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserSMSRole = new Lazy<string>(() => "@pSMSRole");
    public static string pmSMSOrganizationalUserSMSRole => _pmSMSOrganizationalUserSMSRole.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserAuthorityLevel = new Lazy<string>(() => "@pAuthorityLevel");
    public static string pmSMSOrganizationalUserAuthorityLevel => _pmSMSOrganizationalUserAuthorityLevel.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserRiskApprovalAuthority = new Lazy<string>(() => "@pRiskApprovalAuthority");
    public static string pmSMSOrganizationalUserRiskApprovalAuthority => _pmSMSOrganizationalUserRiskApprovalAuthority.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserIsActive = new Lazy<string>(() => "@pIsActive");
    public static string pmSMSOrganizationalUserIsActive => _pmSMSOrganizationalUserIsActive.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserLastLoginDate = new Lazy<string>(() => "@pLastLoginDate");
    public static string pmSMSOrganizationalUserLastLoginDate => _pmSMSOrganizationalUserLastLoginDate.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalUserLoginDate = new Lazy<string>(() => "@pLoginDate");
    public static string pmSMSOrganizationalUserLoginDate => _pmSMSOrganizationalUserLoginDate.Value;

    /// <summary>
    /// SMS Stakeholder User parameters
    /// </summary>
    private static readonly Lazy<string> _pmSMSStakeholderUserId = new Lazy<string>(() => "@pID");
    public static string pmSMSStakeholderUserId => _pmSMSStakeholderUserId.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserCodeForAssignment = new Lazy<string>(() => "@pUserCode");
    public static string pmSMSStakeholderUserCodeForAssignment => _pmSMSStakeholderUserCodeForAssignment.Value;


    private static readonly Lazy<string> _pmSMSStakeholderUserCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSStakeholderUserCode => _pmSMSStakeholderUserCode.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserFirstName = new Lazy<string>(() => "@pFirstName");
    public static string pmSMSStakeholderUserFirstName => _pmSMSStakeholderUserFirstName.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserLastName = new Lazy<string>(() => "@pLastName");
    public static string pmSMSStakeholderUserLastName => _pmSMSStakeholderUserLastName.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserUserName = new Lazy<string>(() => "@pUserName");
    public static string pmSMSStakeholderUserUserName => _pmSMSStakeholderUserUserName.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserPassword = new Lazy<string>(() => "@pPassword");
    public static string pmSMSStakeholderUserPassword => _pmSMSStakeholderUserPassword.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserStakeholderType = new Lazy<string>(() => "@pStakeholderType");
    public static string pmSMSStakeholderUserStakeholderType => _pmSMSStakeholderUserStakeholderType.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserOrganization = new Lazy<string>(() => "@pOrganization");
    public static string pmSMSStakeholderUserOrganization => _pmSMSStakeholderUserOrganization.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserRole = new Lazy<string>(() => "@pSMSUserRole");
    public static string pmSMSStakeholderUserRole => _pmSMSStakeholderUserRole.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserIsActive = new Lazy<string>(() => "@pIsActive");
    public static string pmSMSStakeholderUserIsActive => _pmSMSStakeholderUserIsActive.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserLastLoginDate = new Lazy<string>(() => "@pLastLoginDate");
    public static string pmSMSStakeholderUserLastLoginDate => _pmSMSStakeholderUserLastLoginDate.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserLoginDate = new Lazy<string>(() => "@pLoginDate");
    public static string pmSMSStakeholderUserLoginDate => _pmSMSStakeholderUserLoginDate.Value;

    private static readonly Lazy<string> _pmSMSStakeholderUserIsPOPEmployee = new Lazy<string>(() => "@pIsPOPEmployee");
    public static string pmSMSStakeholderUserIsPOPEmployee => _pmSMSStakeholderUserIsPOPEmployee.Value;

    /// <summary>
    /// Airport Shared Dataset parameters
    /// </summary>

    private static readonly Lazy<string> _pmAirportSharedDatasetCode = new Lazy<string>(() => "@pCode");
    public static string pmAirportSharedDatasetCode => _pmAirportSharedDatasetCode.Value;

    private static readonly Lazy<string> _pmAirportSharedDatasetReportCode = new Lazy<string>(() => "@pReportCode");
    public static string pmAirportSharedDatasetReportCode => _pmAirportSharedDatasetReportCode.Value;

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

    private static readonly Lazy<string> _pmHazardInitialRiskMatrixCode = new Lazy<string>(() => "@pHazardInitialRiskMatrixCode");
    public static string pmHazardInitialRiskMatrixCode => _pmHazardInitialRiskMatrixCode.Value;

    private static readonly Lazy<string> _pmHazardInitialAverageScore = new Lazy<string>(() => "@pHazardInitialAverageScore");
    public static string pmHazardInitialAverageScore => _pmHazardInitialAverageScore.Value;


    private static readonly Lazy<string> _pmHazardResidualRiskMatrixCode = new Lazy<string>(() => "@pHazardResidualRiskMatrixCode");
    public static string pmHazardResidualRiskMatrixCode => _pmHazardResidualRiskMatrixCode.Value;

    private static readonly Lazy<string> _pmHazardResidualAverageScore = new Lazy<string>(() => "@pHazardResidualAverageScore");
    public static string pmHazardResidualAverageScore => _pmHazardResidualAverageScore.Value;






    // Additional Hazard parameters for comprehensive SMS support


 
    private static readonly Lazy<string> _pmHazardType = new Lazy<string>(() => "@pHazardType");
    public static string pmHazardType => _pmHazardType.Value;

    //IsInitialHazard
    private static readonly Lazy<string> _pmHazardIsInitialHazard = new Lazy<string>(() => "@pIsInitialHazard");
    public static string pmHazardIsInitialHazard => _pmHazardIsInitialHazard.Value;


    private static readonly Lazy<string> _pmHazardCategory = new Lazy<string>(() => "@pHazardCategory");
    public static string pmHazardCategory => _pmHazardCategory.Value;


    private static readonly Lazy<string> _pmHazardStatus = new Lazy<string>(() => "@pStatus");
    public static string pmHazardStatus => _pmHazardStatus.Value;

    private static readonly Lazy<string> _pmHazardPriority = new Lazy<string>(() => "@pHazardPriority");
    public static string pmHazardPriority => _pmHazardPriority.Value;

    private static readonly Lazy<string> _pmHazardRiskLevel = new Lazy<string>(() => "@pHazardRiskLevel");
    public static string pmHazardRiskLevel => _pmHazardRiskLevel.Value;

    private static readonly Lazy<string> _pmHazardInitialWorstCredibleOutcome = new Lazy<string>(() => "@pHazardInitialWorstCredibleOutcome");
    public static string pmHazardInitialWorstCredibleOutcome => _pmHazardInitialWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _pmHazardInitialRootCause = new Lazy<string>(() => "@pHazardInitialRootCause");
    public static string pmHazardInitialRootCause => _pmHazardInitialRootCause.Value;

    private static readonly Lazy<string> _pmHazardCurrentMitigations = new Lazy<string>(() => "@pHazardCurrentMitigations");
    public static string pmHazardCurrentMitigations => _pmHazardCurrentMitigations.Value;

    private static readonly Lazy<string> _pmHazardProposedMitigations = new Lazy<string>(() => "@pHazardProposedMitigations");
    public static string pmHazardProposedMitigations => _pmHazardProposedMitigations.Value;

    private static readonly Lazy<string> _pmHazardMitigationTargetDate = new Lazy<string>(() => "@pHazardMitigationTargetDate");
    public static string pmHazardMitigationTargetDate => _pmHazardMitigationTargetDate.Value;

    private static readonly Lazy<string> _pmHazardMitigationOwner = new Lazy<string>(() => "@pHazardMitigationOwner");
    public static string pmHazardMitigationOwner => _pmHazardMitigationOwner.Value;

    private static readonly Lazy<string> _pmHazardRequiresInvestigation = new Lazy<string>(() => "@pHazardRequiresInvestigation");
    public static string pmHazardRequiresInvestigation => _pmHazardRequiresInvestigation.Value;

    private static readonly Lazy<string> _pmHazardInvestigationCompletedDate = new Lazy<string>(() => "@pHazardInvestigationCompletedDate");
    public static string pmHazardInvestigationCompletedDate => _pmHazardInvestigationCompletedDate.Value;

    private static readonly Lazy<string> _pmHazardInvestigationNotes = new Lazy<string>(() => "@pHazardInvestigationNotes");
    public static string pmHazardInvestigationNotes => _pmHazardInvestigationNotes.Value;

    private static readonly Lazy<string> _pmHazardAdditionalComments = new Lazy<string>(() => "@pHazardAdditionalComments");
    public static string pmHazardAdditionalComments => _pmHazardAdditionalComments.Value;

    private static readonly Lazy<string> _pmHazardLocation = new Lazy<string>(() => "@pHazardLocation");
    public static string pmHazardLocation => _pmHazardLocation.Value;

    private static readonly Lazy<string> _pmHazardLocationArea = new Lazy<string>(() => "@pHazardLocationArea");
    public static string pmHazardLocationArea => _pmHazardLocationArea.Value;

    private static readonly Lazy<string> _pmHazardLocationSubArea = new Lazy<string>(() => "@pHazardLocationSubArea");
    public static string pmHazardLocationSubArea => _pmHazardLocationSubArea.Value;

    /// <summary>
    /// Hazard Location parameters (for pr_HazardLocation_* stored procedures)
    /// These are distinct from AirportSharedDataset location parameters
    /// </summary>
    private static readonly Lazy<string> _pmHazardLocationId = new Lazy<string>(() => "@pHazardLocationID");
    public static string pmHazardLocationId => _pmHazardLocationId.Value;

    private static readonly Lazy<string> _pmHazardLocationCode = new Lazy<string>(() => "@pCode");
    public static string pmHazardLocationCode => _pmHazardLocationCode.Value;

    private static readonly Lazy<string> _pmHazardLocationHazardCode = new Lazy<string>(() => "@pHazardCode");
    public static string pmHazardLocationHazardCode => _pmHazardLocationHazardCode.Value;

    private static readonly Lazy<string> _pmHazardLocationLatitude = new Lazy<string>(() => "@pLatitude");
    public static string pmHazardLocationLatitude => _pmHazardLocationLatitude.Value;

    private static readonly Lazy<string> _pmHazardLocationLongitude = new Lazy<string>(() => "@pLongitude");
    public static string pmHazardLocationLongitude => _pmHazardLocationLongitude.Value;

    private static readonly Lazy<string> _pmHazardLocationDescription = new Lazy<string>(() => "@pDescription");
    public static string pmHazardLocationDescription => _pmHazardLocationDescription.Value;

    private static readonly Lazy<string> _pmHazardLocationDateSelected = new Lazy<string>(() => "@pDateSelected");
    public static string pmHazardLocationDateSelected => _pmHazardLocationDateSelected.Value;

    private static readonly Lazy<string> _pmHazardLocationMapSVG = new Lazy<string>(() => "@pLocationMapSVG");
    public static string pmHazardLocationMapSVG => _pmHazardLocationMapSVG.Value;

    // HazardLocation has its own location area/subarea parameters (different from AirportSharedDataset)
    private static readonly Lazy<string> _pmHazardLocationLocationArea = new Lazy<string>(() => "@pLocationArea");
    public static string pmHazardLocationLocationArea => _pmHazardLocationLocationArea.Value;

    private static readonly Lazy<string> _pmHazardLocationLocationSubArea = new Lazy<string>(() => "@pLocationSubArea");
    public static string pmHazardLocationLocationSubArea => _pmHazardLocationLocationSubArea.Value;

    private static readonly Lazy<string> _pmHazardLocationName = new Lazy<string>(() => "@pLocationName");
    public static string pmHazardLocationName => _pmHazardLocationName.Value;

    private static readonly Lazy<string> _pmHazardLocationAccuracyMeters = new Lazy<string>(() => "@pAccuracyMeters");
    public static string pmHazardLocationAccuracyMeters => _pmHazardLocationAccuracyMeters.Value;

    private static readonly Lazy<string> _pmHazardLocationElevationFeet = new Lazy<string>(() => "@pElevationFeet");
    public static string pmHazardLocationElevationFeet => _pmHazardLocationElevationFeet.Value;

    private static readonly Lazy<string> _pmHazardLocationSource = new Lazy<string>(() => "@pSource");
    public static string pmHazardLocationSource => _pmHazardLocationSource.Value;

    private static readonly Lazy<string> _pmHazardLocationStatus = new Lazy<string>(() => "@pStatus");
    public static string pmHazardLocationStatus => _pmHazardLocationStatus.Value;

    private static readonly Lazy<string> _pmHazardLocationIsValidated = new Lazy<string>(() => "@pIsValidated");
    public static string pmHazardLocationIsValidated => _pmHazardLocationIsValidated.Value;

    private static readonly Lazy<string> _pmHazardLocationValidatedDate = new Lazy<string>(() => "@pValidatedDate");
    public static string pmHazardLocationValidatedDate => _pmHazardLocationValidatedDate.Value;

    private static readonly Lazy<string> _pmHazardLocationValidatedBy = new Lazy<string>(() => "@pValidatedBy");
    public static string pmHazardLocationValidatedBy => _pmHazardLocationValidatedBy.Value;

    private static readonly Lazy<string> _pmHazardLocationNotes = new Lazy<string>(() => "@pNotes");
    public static string pmHazardLocationNotes => _pmHazardLocationNotes.Value;

    private static readonly Lazy<string> _pmHazardLocationTags = new Lazy<string>(() => "@pTags");
    public static string pmHazardLocationTags => _pmHazardLocationTags.Value;

    private static readonly Lazy<string> _pmHazardLocationAirportGrid = new Lazy<string>(() => "@pAirportGrid");
    public static string pmHazardLocationAirportGrid => _pmHazardLocationAirportGrid.Value;

    private static readonly Lazy<string> _pmHazardLocationRunwayReference = new Lazy<string>(() => "@pRunwayReference");
    public static string pmHazardLocationRunwayReference => _pmHazardLocationRunwayReference.Value;

    private static readonly Lazy<string> _pmHazardLocationTaxiwayReference = new Lazy<string>(() => "@pTaxiwayReference");
    public static string pmHazardLocationTaxiwayReference => _pmHazardLocationTaxiwayReference.Value;

    // Proximity search parameters
    private static readonly Lazy<string> _pmHazardLocationRadiusMeters = new Lazy<string>(() => "@pRadiusMeters");
    public static string pmHazardLocationRadiusMeters => _pmHazardLocationRadiusMeters.Value;

    private static readonly Lazy<string> _pmHazardLocationMaxResults = new Lazy<string>(() => "@pMaxResults");
    public static string pmHazardLocationMaxResults => _pmHazardLocationMaxResults.Value;

    // Common parameters for HazardLocation operations
    private static readonly Lazy<string> _pmHazardLocationIncludeInactive = new Lazy<string>(() => "@pIncludeInactive");
    public static string pmHazardLocationIncludeInactive => _pmHazardLocationIncludeInactive.Value;

    private static readonly Lazy<string> _pmHazardLocationSoftDelete = new Lazy<string>(() => "@pSoftDelete");
    public static string pmHazardLocationSoftDelete => _pmHazardLocationSoftDelete.Value;

    private static readonly Lazy<string> _pmHazardLocationDeletedBy = new Lazy<string>(() => "@pDeletedBy");
    public static string pmHazardLocationDeletedBy => _pmHazardLocationDeletedBy.Value;

    /// <summary>
    /// SMS User Role parameters - CORRECTED to match actual database schema
    /// </summary>
    private static readonly Lazy<string> _pmSMSUserRoleId = new Lazy<string>(() => "@pID");
    public static string pmSMSUserRoleId => _pmSMSUserRoleId.Value;

    private static readonly Lazy<string> _pmSMSUserRoleCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSUserRoleCode => _pmSMSUserRoleCode.Value;

    private static readonly Lazy<string> _pmSMSUserRoleUserId = new Lazy<string>(() => "@pUserID");
    public static string pmSMSUserRoleUserId => _pmSMSUserRoleUserId.Value;

    private static readonly Lazy<string> _pmSMSUserRoleUserType = new Lazy<string>(() => "@pUserType");
    public static string pmSMSUserRoleUserType => _pmSMSUserRoleUserType.Value;

    private static readonly Lazy<string> _pmSMSUserRoleSMSRoleCode = new Lazy<string>(() => "@pSMSRoleCode");
    public static string pmSMSUserRoleSMSRoleCode => _pmSMSUserRoleSMSRoleCode.Value;

    private static readonly Lazy<string> _pmSMSUserRoleDepartment = new Lazy<string>(() => "@pDepartment");
    public static string pmSMSUserRoleDepartment => _pmSMSUserRoleDepartment.Value;

    private static readonly Lazy<string> _pmSMSUserRoleEffectiveDate = new Lazy<string>(() => "@pEffectiveDate");
    public static string pmSMSUserRoleEffectiveDate => _pmSMSUserRoleEffectiveDate.Value;

    private static readonly Lazy<string> _pmSMSUserRoleExpirationDate = new Lazy<string>(() => "@pExpirationDate");
    public static string pmSMSUserRoleExpirationDate => _pmSMSUserRoleExpirationDate.Value;

    private static readonly Lazy<string> _pmSMSUserRoleIsActive = new Lazy<string>(() => "@pIsActive");
    public static string pmSMSUserRoleIsActive => _pmSMSUserRoleIsActive.Value;

    private static readonly Lazy<string> _pmSMSUserRoleAssignedBy = new Lazy<string>(() => "@pAssignedBy");
    public static string pmSMSUserRoleAssignedBy => _pmSMSUserRoleAssignedBy.Value;

    private static readonly Lazy<string> _pmSMSUserRoleAssignedDate = new Lazy<string>(() => "@pAssignedDate");
    public static string pmSMSUserRoleAssignedDate => _pmSMSUserRoleAssignedDate.Value;

    private static readonly Lazy<string> _pmSMSUserRoleDeactivatedBy = new Lazy<string>(() => "@pDeactivatedBy");
    public static string pmSMSUserRoleDeactivatedBy => _pmSMSUserRoleDeactivatedBy.Value;

    private static readonly Lazy<string> _pmSMSUserRoleDeactivatedDate = new Lazy<string>(() => "@pDeactivatedDate");
    public static string pmSMSUserRoleDeactivatedDate => _pmSMSUserRoleDeactivatedDate.Value;

    private static readonly Lazy<string> _pmSMSUserRoleCutoffDate = new Lazy<string>(() => "@pCutoffDate");
    public static string pmSMSUserRoleCutoffDate => _pmSMSUserRoleCutoffDate.Value;

    private static readonly Lazy<string> _pmSMSUserRoleAuthorityLevel = new Lazy<string>(() => "@pAuthorityLevel");
    public static string pmSMSUserRoleAuthorityLevel => _pmSMSUserRoleAuthorityLevel.Value;

    /// <summary>
    /// SMS User Role Permission parameters
    /// </summary>
    private static readonly Lazy<string> _pmSMSUserRolePermissionCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSUserRolePermissionCode => _pmSMSUserRolePermissionCode.Value;

    private static readonly Lazy<string> _pmSMSUserRolePermissionSMSUserRoleCode = new Lazy<string>(() => "@pSMSUserRoleCode");
    public static string pmSMSUserRolePermissionSMSUserRoleCode => _pmSMSUserRolePermissionSMSUserRoleCode.Value;

    private static readonly Lazy<string> _pmSMSUserRolePermissionModule = new Lazy<string>(() => "@pModule");
    public static string pmSMSUserRolePermissionModule => _pmSMSUserRolePermissionModule.Value;

    private static readonly Lazy<string> _pmSMSUserRolePermissionCreate = new Lazy<string>(() => "@pCreate");
    public static string pmSMSUserRolePermissionCreate => _pmSMSUserRolePermissionCreate.Value;

    private static readonly Lazy<string> _pmSMSUserRolePermissionRead = new Lazy<string>(() => "@pRead");
    public static string pmSMSUserRolePermissionRead => _pmSMSUserRolePermissionRead.Value;

    private static readonly Lazy<string> _pmSMSUserRolePermissionUpdate = new Lazy<string>(() => "@pUpdate");
    public static string pmSMSUserRolePermissionUpdate => _pmSMSUserRolePermissionUpdate.Value;

    private static readonly Lazy<string> _pmSMSUserRolePermissionDelete = new Lazy<string>(() => "@pDelete");
    public static string pmSMSUserRolePermissionDelete => _pmSMSUserRolePermissionDelete.Value;

    /// <summary>
    /// Risk Assessment parameters (UPDATED FOR STEPS 1-5)
    /// </summary>
    private static readonly Lazy<string> _pmRiskAssessmentHazardCode = new Lazy<string>(() => "@pHazardCode");
    public static string pmRiskAssessmentHazardCode => _pmRiskAssessmentHazardCode.Value;

    private static readonly Lazy<string> _pmRiskAssessmentCode = new Lazy<string>(() => "@pCode");
    public static string pmRiskAssessmentCode => _pmRiskAssessmentCode.Value;

    private static readonly Lazy<string> _pmRiskAssessmentName = new Lazy<string>(() => "@pRiskAssessmentName");
    public static string pmRiskAssessmentName => _pmRiskAssessmentName.Value;

    private static readonly Lazy<string> _pmRiskAssessmentDescription = new Lazy<string>(() => "@pRiskAssessmentDescription");
    public static string pmRiskAssessmentDescription => _pmRiskAssessmentDescription.Value;

    
    private static readonly Lazy<string> _pmRiskAssessmentType = new Lazy<string>(() => "@pRiskAssessmentType");
    public static string pmRiskAssessmentType => _pmRiskAssessmentType.Value;

    private static readonly Lazy<string> _pmRiskAssessmentStatus = new Lazy<string>(() => "@pRiskAssessmentStatus");
    public static string pmRiskAssessmentStatus => _pmRiskAssessmentStatus.Value;

    private static readonly Lazy<string> _pmRiskAssessmentStage = new Lazy<string>(() => "@pRiskAssessmentStage");
    public static string pmRiskAssessmentStage => _pmRiskAssessmentStage.Value;

    // New Core Fields
    private static readonly Lazy<string> _pmLeadAssessorId = new Lazy<string>(() => "@pLeadAssessorId");
    public static string pmLeadAssessorId => _pmLeadAssessorId.Value;

    private static readonly Lazy<string> _pmPrimaryHazardId = new Lazy<string>(() => "@pPrimaryHazardId");
    public static string pmPrimaryHazardId => _pmPrimaryHazardId.Value;

    private static readonly Lazy<string> _pmRiskAssessmentCategory = new Lazy<string>(() => "@pRiskAssessmentCategory");
    public static string pmRiskAssessmentCategory => _pmRiskAssessmentCategory.Value;

    private static readonly Lazy<string> _pmCurrentStep = new Lazy<string>(() => "@pCurrentStep");
    public static string pmCurrentStep => _pmCurrentStep.Value;

    private static readonly Lazy<string> _pmCompletedDate = new Lazy<string>(() => "@pCompletedDate");
    public static string pmCompletedDate => _pmCompletedDate.Value;

    private static readonly Lazy<string> _pmCompletedBy = new Lazy<string>(() => "@pCompletedBy");
    public static string pmCompletedBy => _pmCompletedBy.Value;

    private static readonly Lazy<string> _pmParentAssessmentId = new Lazy<string>(() => "@pParentAssessmentId");
    public static string pmParentAssessmentId => _pmParentAssessmentId.Value;

    /// <summary>
    /// Risk Assessment - Step 1 Parameters
    /// </summary>
    private static readonly Lazy<string> _pmSystemDescription = new Lazy<string>(() => "@pSystemDescription");
    public static string pmSystemDescription => _pmSystemDescription.Value;

    private static readonly Lazy<string> _pmSystemBoundaries = new Lazy<string>(() => "@pSystemBoundaries");
    public static string pmSystemBoundaries => _pmSystemBoundaries.Value;

    private static readonly Lazy<string> _pmSystemPurpose = new Lazy<string>(() => "@pSystemPurpose");
    public static string pmSystemPurpose => _pmSystemPurpose.Value;

    private static readonly Lazy<string> _pmPersonnelFactors = new Lazy<string>(() => "@pPersonnelFactors");
    public static string pmPersonnelFactors => _pmPersonnelFactors.Value;

    private static readonly Lazy<string> _pmEquipmentFactors = new Lazy<string>(() => "@pEquipmentFactors");
    public static string pmEquipmentFactors => _pmEquipmentFactors.Value;

    private static readonly Lazy<string> _pmProcedureFactors = new Lazy<string>(() => "@pProcedureFactors");
    public static string pmProcedureFactors => _pmProcedureFactors.Value;

    private static readonly Lazy<string> _pmResourceFactors = new Lazy<string>(() => "@pResourceFactors");
    public static string pmResourceFactors => _pmResourceFactors.Value;

    private static readonly Lazy<string> _pmEnvironmentFactors = new Lazy<string>(() => "@pEnvironmentFactors");
    public static string pmEnvironmentFactors => _pmEnvironmentFactors.Value;

    private static readonly Lazy<string> _pmSelectedStakeholderGroups = new Lazy<string>(() => "@pSelectedStakeholderGroups");
    public static string pmSelectedStakeholderGroups => _pmSelectedStakeholderGroups.Value;

    private static readonly Lazy<string> _pmSelectedIndividualStakeholders = new Lazy<string>(() => "@pSelectedIndividualStakeholders");
    public static string pmSelectedIndividualStakeholders => _pmSelectedIndividualStakeholders.Value;

    /// <summary>
    /// Risk Assessment - Step 3 Parameters
    /// </summary>
    //private static readonly Lazy<string> _pmRiskAnalysisMethod = new Lazy<string>(() => "@pRiskAnalysisMethod");
    //public static string pmRiskAnalysisMethod => _pmRiskAnalysisMethod.Value;

    //private static readonly Lazy<string> _pmRiskCriteria = new Lazy<string>(() => "@pRiskCriteria");
    //public static string pmRiskCriteria => _pmRiskCriteria.Value;

    /// <summary>
    /// Risk Assessment - Step 4 Parameters
    /// </summary>
    private static readonly Lazy<string> _pmTolerabilityFramework = new Lazy<string>(() => "@pTolerabilityFramework");
    public static string pmTolerabilityFramework => _pmTolerabilityFramework.Value;

    private static readonly Lazy<string> _pmRiskAcceptanceCriteria = new Lazy<string>(() => "@pRiskAcceptanceCriteria");
    public static string pmRiskAcceptanceCriteria => _pmRiskAcceptanceCriteria.Value;

    private static readonly Lazy<string> _pmFinalSeverityScore = new Lazy<string>(() => "@pFinalSeverityScore");
    public static string pmFinalSeverityScore => _pmFinalSeverityScore.Value;

    private static readonly Lazy<string> _pmFinalLikelihoodScore = new Lazy<string>(() => "@pFinalLikelihoodScore");
    public static string pmFinalLikelihoodScore => _pmFinalLikelihoodScore.Value;

    private static readonly Lazy<string> _pmFinalRiskLevel = new Lazy<string>(() => "@pFinalRiskLevel");
    public static string pmFinalRiskLevel => _pmFinalRiskLevel.Value;

    

    /// <summary>
    /// Risk Assessment - Step 5 Parameters
    /// </summary>
    

    /// <summary>
    /// Risk Assessment - Progress Tracking Parameters
    /// </summary>
    private static readonly Lazy<string> _pmCompletedSteps = new Lazy<string>(() => "@pCompletedSteps");
    public static string pmCompletedSteps => _pmCompletedSteps.Value;

    private static readonly Lazy<string> _pmCompletionPercentage = new Lazy<string>(() => "@pCompletionPercentage");
    public static string pmCompletionPercentage => _pmCompletionPercentage.Value;

    /// <summary>
    /// Report parameters (restored from accidental removal)
    /// </summary>
    private static readonly Lazy<string> _pmReportId = new Lazy<string>(() => "@pReportID");
    public static string pmReportId => _pmReportId.Value;

    private static readonly Lazy<string> _pmReportCode = new Lazy<string>(() => "@pReportCode");
    public static string pmReportCode => _pmReportCode.Value;

    private static readonly Lazy<string> _pmReportName = new Lazy<string>(() => "@pReportName");
    public static string pmReportName => _pmReportName.Value;

    private static readonly Lazy<string> _pmIncidentDateTime = new Lazy<string>(() => "@pIncidentDateTime");
    public static string pmIncidentDateTime => _pmIncidentDateTime.Value;

    private static readonly Lazy<string> _pmSubmittedBy = new Lazy<string>(() => "@pSubmittedBy");
    public static string pmSubmittedBy => _pmSubmittedBy.Value;

    private static readonly Lazy<string> _pmSubmittedDate = new Lazy<string>(() => "@pSubmittedDate");
    public static string pmSubmittedDate => _pmSubmittedDate.Value;

    private static readonly Lazy<string> _pmSubmittingDepartment = new Lazy<string>(() => "@pSubmittingDepartment");
    public static string pmSubmittingDepartment => _pmSubmittingDepartment.Value;

    private static readonly Lazy<string> _pmSubmittingDepartmentJobFunction = new Lazy<string>(() => "@pSubmittingDepartmentJobFunction");
    public static string pmSubmittingDepartmentJobFunction => _pmSubmittingDepartmentJobFunction.Value;


    private static readonly Lazy<string> _pmReportContactName = new Lazy<string>(() => "@pReportContactName");
    public static string pmReportContactName => _pmReportContactName.Value;

    private static readonly Lazy<string> _pmReportContactCell = new Lazy<string>(() => "@pReportContactCell");
    public static string pmReportContactCell => _pmReportContactCell.Value;

    private static readonly Lazy<string> _pmReportContactEmail = new Lazy<string>(() => "@pReportContactEmail");
    public static string pmReportContactEmail => _pmReportContactEmail.Value;


    private static readonly Lazy<string> _pmReportDescription = new Lazy<string>(() => "@pReportDescription");
    public static string pmReportDescription => _pmReportDescription.Value;

    private static readonly Lazy<string> _pmReportStatus = new Lazy<string>(() => "@pReportStatus");
    public static string pmReportStatus => _pmReportStatus.Value;

    private static readonly Lazy<string> _pmReportStage = new Lazy<string>(() => "@pReportStage");
    public static string pmReportStage => _pmReportStage.Value;

    /// <summary>
    /// Mitigation parameters (restored from accidental removal)
    /// </summary>
    private static readonly Lazy<string> _pmMitigationId = new Lazy<string>(() => "@pMitigationID");
    public static string pmMitigationId => _pmMitigationId.Value;

    private static readonly Lazy<string> _pmMitigationCode = new Lazy<string>(() => "@pMitigationCode");
    public static string pmMitigationCode => _pmMitigationCode.Value;

    private static readonly Lazy<string> _pmMitigationHazardCode = new Lazy<string>(() => "@pMitigationHazardCode");
    public static string pmMitigationHazardCode => _pmMitigationHazardCode.Value;

    /// <summary>
    /// Interview parameters (restored from accidental removal)
    /// </summary>
    private static readonly Lazy<string> _pmInterviewId = new Lazy<string>(() => "@pInterviewID");
    public static string pmInterviewId => _pmInterviewId.Value;

    private static readonly Lazy<string> _pmInterviewCode = new Lazy<string>(() => "@pInterviewCode");
    public static string pmInterviewCode => _pmInterviewCode.Value;

    private static readonly Lazy<string> _pmInterviewInvestigationCode = new Lazy<string>(() => "@pInvestigationCode");
    public static string pmInterviewInvestigationCode => _pmInterviewInvestigationCode.Value;

    private static readonly Lazy<string> _pmInterviewSMSInvestigatorCode = new Lazy<string>(() => "@pSMSInvestigatorCode");
    public static string pmInterviewSMSInvestigatorCode => _pmInterviewSMSInvestigatorCode.Value;

    private static readonly Lazy<string> _pmInterviewPersonInterviewed = new Lazy<string>(() => "@pPersonInterviewed");
    public static string pmInterviewPersonInterviewed => _pmInterviewPersonInterviewed.Value;

    private static readonly Lazy<string> _pmInterviewPersonRole = new Lazy<string>(() => "@pPersonInterviewedRole");
    public static string pmInterviewPersonRole => _pmInterviewPersonRole.Value;

    private static readonly Lazy<string> _pmInterviewPersonDepartment = new Lazy<string>(() => "@pPersonInterviewedDepartment");
    public static string pmInterviewPersonDepartment => _pmInterviewPersonDepartment.Value;

    private static readonly Lazy<string> _pmInterviewPersonInterviewedNotes = new Lazy<string>(() => "@pPersonInterviewedNotes");
    public static string pmInterviewPersonInterviewedNotes => _pmInterviewPersonInterviewedNotes.Value;

    private static readonly Lazy<string> _pmInterviewInvestigatorNotes = new Lazy<string>(() => "@pInvestigatorNotes");
    public static string pmInterviewInvestigatorNotes => _pmInterviewInvestigatorNotes.Value;

    private static readonly Lazy<string> _pmInterviewStatus = new Lazy<string>(() => "@pStatus");
    public static string pmInterviewStatus => _pmInterviewStatus.Value;

    private static readonly Lazy<string> _pmInterviewDate = new Lazy<string>(() => "@pInterviewDate");
    public static string pmInterviewDate => _pmInterviewDate.Value;

    private static readonly Lazy<string> _pmInterviewDurationMinutes = new Lazy<string>(() => "@pDurationMinutes");
    public static string pmInterviewDurationMinutes => _pmInterviewDurationMinutes.Value;

    private static readonly Lazy<string> _pmInterviewLocation = new Lazy<string>(() => "@pInterviewLocation");
    public static string pmInterviewLocation => _pmInterviewLocation.Value;

    private static readonly Lazy<string> _pmInterviewType = new Lazy<string>(() => "@pType");
    public static string pmInterviewType => _pmInterviewType.Value;

    private static readonly Lazy<string> _pmInterviewIsConfidential = new Lazy<string>(() => "@pIsConfidential");
    public static string pmInterviewIsConfidential => _pmInterviewIsConfidential.Value;

    private static readonly Lazy<string> _pmInterviewPreparationNotes = new Lazy<string>(() => "@pPreparationNotes");
    public static string pmInterviewPreparationNotes => _pmInterviewPreparationNotes.Value;

    private static readonly Lazy<string> _pmInterviewQuestionsToAsk = new Lazy<string>(() => "@pQuestionsToAsk");
    public static string pmInterviewQuestionsToAsk => _pmInterviewQuestionsToAsk.Value;

    private static readonly Lazy<string> _pmInterviewBackgroundInformation = new Lazy<string>(() => "@pBackgroundInformation");
    public static string pmInterviewBackgroundInformation => _pmInterviewBackgroundInformation.Value;

    private static readonly Lazy<string> _pmInterviewKeyFindings = new Lazy<string>(() => "@pKeyFindings");
    public static string pmInterviewKeyFindings => _pmInterviewKeyFindings.Value;

    private static readonly Lazy<string> _pmInterviewFollowUpRequired = new Lazy<string>(() => "@pFollowUpRequired");
    public static string pmInterviewFollowUpRequired => _pmInterviewFollowUpRequired.Value;

    private static readonly Lazy<string> _pmInterviewAdditionalWitnesses = new Lazy<string>(() => "@pAdditionalWitnesses");
    public static string pmInterviewAdditionalWitnesses => _pmInterviewAdditionalWitnesses.Value;

    private static readonly Lazy<string> _pmInterviewCompletedDate = new Lazy<string>(() => "@pCompletedDate");
    public static string pmInterviewCompletedDate => _pmInterviewCompletedDate.Value;

    /// <summary>
    /// Investigation parameters (restored from accidental removal)
    /// </summary>
    private static readonly Lazy<string> _pmInvestigationId = new Lazy<string>(() => "@pInvestigationID");
    public static string pmInvestigationId => _pmInvestigationId.Value;

    private static readonly Lazy<string> _pmInvestigationCode = new Lazy<string>(() => "@pInvestigationCode");
    public static string pmInvestigationCode => _pmInvestigationCode.Value;

    private static readonly Lazy<string> _pmInvestigationReportCode = new Lazy<string>(() => "@pReportCode");
    public static string pmInvestigationReportCode => _pmInvestigationReportCode.Value;

    private static readonly Lazy<string> _pmInvestigationHazardCode = new Lazy<string>(() => "@pHazardCode");
    public static string pmInvestigationHazardCode => _pmInvestigationHazardCode.Value;

    private static readonly Lazy<string> _pmInvestigationNotes = new Lazy<string>(() => "@pInvestigationNotes");
    public static string pmInvestigationNotes => _pmInvestigationNotes.Value;

    private static readonly Lazy<string> _pmInvestigationAssignedInvestigatorId = new Lazy<string>(() => "@pAssignedInvestigatorId");
    public static string pmInvestigationAssignedInvestigatorId => _pmInvestigationAssignedInvestigatorId.Value;

    /// <summary>
    /// Risk Analysis parameters (restored from accidental removal)
    /// </summary>
    private static readonly Lazy<string> _pmRiskAnalysisId = new Lazy<string>(() => "@pRiskAnalysisID");
    public static string pmRiskAnalysisId => _pmRiskAnalysisId.Value;

    private static readonly Lazy<string> _pmRiskAnalysisCode = new Lazy<string>(() => "@pRiskAnalysisCode");
    public static string pmRiskAnalysisCode => _pmRiskAnalysisCode.Value;

    

        private static readonly Lazy<string> _pmRiskAnalysisType = new Lazy<string>(() => "@pRiskAnalysisType");
    public static string pmRiskAnalysisType => _pmRiskAnalysisType.Value;


    private static readonly Lazy<string> _pmRiskAnalysisHazardCode = new Lazy<string>(() => "@pRiskAnalysisHazardCode");
    public static string pmRiskAnalysisHazardCode => _pmRiskAnalysisHazardCode.Value;

    private static readonly Lazy<string> _pmRiskAnalysisRiskAssessmentCode = new Lazy<string>(() => "@pRiskAnalysisRiskAssessmentCode");
    public static string pmRiskAnalysisRiskAssessmentCode => _pmRiskAnalysisRiskAssessmentCode.Value;

    private static readonly Lazy<string> _pmRiskAnalysisInitialWorstCredibleOutcome = new Lazy<string>(() => "@pRiskAnalysisInitialWorstCredibleOutcome");
    public static string pmRiskAnalysisInitialWorstCredibleOutcome => _pmRiskAnalysisInitialWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _pmRiskAnalysisInitialRootCause = new Lazy<string>(() => "@pRiskAnalysisInitialRootCause");
    public static string pmRiskAnalysisInitialRootCause => _pmRiskAnalysisInitialRootCause.Value;

    private static readonly Lazy<string> _pmRiskAnalysisInitialAdditionalComments = new Lazy<string>(() => "@pRiskAnalysisInitialAdditionalComments");
    public static string pmRiskAnalysisInitialAdditionalComments => _pmRiskAnalysisInitialAdditionalComments.Value;

    private static readonly Lazy<string> _pmRiskAnalysisResidualWorstCredibleOutcome = new Lazy<string>(() => "@pRiskAnalysisResidualWorstCredibleOutcome");
    public static string pmRiskAnalysisResidualWorstCredibleOutcome => _pmRiskAnalysisResidualWorstCredibleOutcome.Value;

    private static readonly Lazy<string> _pmRiskAnalysisResidualRootCause = new Lazy<string>(() => "@pRiskAnalysisResidualRootCause");
    public static string pmRiskAnalysisResidualRootCause => _pmRiskAnalysisResidualRootCause.Value;

    private static readonly Lazy<string> _pmRiskAnalysisResidualAdditionalComments = new Lazy<string>(() => "@pRiskAnalysisResidualAdditionalComments");
    public static string pmRiskAnalysisResidualAdditionalComments => _pmRiskAnalysisResidualAdditionalComments.Value;













    /// <summary>
    /// Scoring Panel parameters (restored from accidental removal)
    /// </summary>
    private static readonly Lazy<string> _pmScoringPanelId = new Lazy<string>(() => "@pScoringPanelID");
    public static string pmScoringPanelId => _pmScoringPanelId.Value;

    private static readonly Lazy<string> _pmScoringPanelCode = new Lazy<string>(() => "@pScoringPanelCode");
    public static string pmScoringPanelCode => _pmScoringPanelCode.Value;

    private static readonly Lazy<string> _pmScoringPanelRiskAssessmentCode = new Lazy<string>(() => "@pScoringPanelRiskAssessmentCode");
    public static string pmScoringPanelRiskAssessmentCode => _pmScoringPanelRiskAssessmentCode.Value;

    private static readonly Lazy<string> _pmScoringPanelHazardCode = new Lazy<string>(() => "@pScoringPanelHazardCode");
    public static string pmScoringPanelHazardCode => _pmScoringPanelHazardCode.Value;

    private static readonly Lazy<string> _pmScoringPanelSMSUserCode = new Lazy<string>(() => "@pScoringPanelSMSUserCode");
    public static string pmScoringPanelSMSUserCode => _pmScoringPanelSMSUserCode.Value;

    private static readonly Lazy<string> _pmScoringPanelInitialLikelihood = new Lazy<string>(() => "@pScoringPanelInitialLikelihood");
    public static string pmScoringPanelInitialLikelihood => _pmScoringPanelInitialLikelihood.Value;

    private static readonly Lazy<string> _pmScoringPanelInitialSeverity = new Lazy<string>(() => "@pScoringPanelInitialSeverity");
    public static string pmScoringPanelInitialSeverity => _pmScoringPanelInitialSeverity.Value;

    private static readonly Lazy<string> _pmScoringPanelInitialScore = new Lazy<string>(() => "@pScoringPanelInitialScore");
    public static string pmScoringPanelInitialScore => _pmScoringPanelInitialScore.Value;

    private static readonly Lazy<string> _pmScoringPanelInitialRationale = new Lazy<string>(() => "@pScoringPanelInitialRationale");
    public static string pmScoringPanelInitialRationale => _pmScoringPanelInitialRationale.Value;


    private static readonly Lazy<string> _pmScoringPanelResidualLikelihood = new Lazy<string>(() => "@pScoringPanelResidualLikelihood");
    public static string pmScoringPanelResidualLikelihood => _pmScoringPanelResidualLikelihood.Value;

    private static readonly Lazy<string> _pmScoringPanelResidualSeverity = new Lazy<string>(() => "@pScoringPanelResidualSeverity");
    public static string pmScoringPanelResidualSeverity => _pmScoringPanelResidualSeverity.Value;

    private static readonly Lazy<string> _pmScoringPanelResidualScore = new Lazy<string>(() => "@pScoringPanelResidualScore");
    public static string pmScoringPanelResidualScore => _pmScoringPanelResidualScore.Value;

    private static readonly Lazy<string> _pmScoringPanelResidualRationale = new Lazy<string>(() => "@pScoringPanelResidualRationale");
    public static string pmScoringPanelResidualRationale => _pmScoringPanelResidualRationale.Value;








    /// <summary>
    /// Report Validation parameters (restored from accidental removal)
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

    private static readonly Lazy<string> _pmReportValidationValidatedBy = new Lazy<string>(() => "@pReportValidationValidatedBy");
    public static string pmReportValidationValidatedBy => _pmReportValidationValidatedBy.Value;

    private static readonly Lazy<string> _pmReportValidationValidatedDate = new Lazy<string>(() => "@pReportValidationValidatedDate");
    public static string pmReportValidationValidatedDate => _pmReportValidationValidatedDate.Value;

    private static readonly Lazy<string> _pmReportValidationComments = new Lazy<string>(() => "@pReportValidationComments");
    public static string pmReportValidationComments => _pmReportValidationComments.Value;

    private static readonly Lazy<string> _pmReportValidationType = new Lazy<string>(() => "@pReportValidationType");
    public static string pmReportValidationType => _pmReportValidationType.Value;

    /// <summary>
    /// Mitigation Assignment parameters (restored from accidental removal) 
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
    /// Investigation additional parameters (missing)
    /// </summary>
    private static readonly Lazy<string> _pmInvestigationStatus = new Lazy<string>(() => "@pInvestigationStatus");
    public static string pmInvestigationStatus => _pmInvestigationStatus.Value;

    private static readonly Lazy<string> _pmInvestigationCompletedDate = new Lazy<string>(() => "@pInvestigationCompletedDate");
    public static string pmInvestigationCompletedDate => _pmInvestigationCompletedDate.Value;

    private static readonly Lazy<string> _pmInvestigationPlan = new Lazy<string>(() => "@pInvestigationPlan");
    public static string pmInvestigationPlan => _pmInvestigationPlan.Value;

    private static readonly Lazy<string> _pmInvestigationObjectives = new Lazy<string>(() => "@pInvestigationObjectives");
    public static string pmInvestigationObjectives => _pmInvestigationObjectives.Value;

    private static readonly Lazy<string> _pmInvestigationDecisionType = new Lazy<string>(() => "@pInvestigationDecisionType");
    public static string pmInvestigationDecisionType => _pmInvestigationDecisionType.Value;

    private static readonly Lazy<string> _pmInvestigationDecisionRationale = new Lazy<string>(() => "@pInvestigationDecisionRationale");
    public static string pmInvestigationDecisionRationale => _pmInvestigationDecisionRationale.Value;

    private static readonly Lazy<string> _pmInvestigationDecisionMaker = new Lazy<string>(() => "@pInvestigationDecisionMaker");
    public static string pmInvestigationDecisionMaker => _pmInvestigationDecisionMaker.Value;

    private static readonly Lazy<string> _pmInvestigationDecisionDate = new Lazy<string>(() => "@pInvestigationDecisionDate");
    public static string pmInvestigationDecisionDate => _pmInvestigationDecisionDate.Value;

    private static readonly Lazy<string> _pmInvestigationNextSteps = new Lazy<string>(() => "@pInvestigationNextSteps");
    public static string pmInvestigationNextSteps => _pmInvestigationNextSteps.Value;

    private static readonly Lazy<string> _pmInvestigationReferralDetails = new Lazy<string>(() => "@pInvestigationReferralDetails");
    public static string pmInvestigationReferralDetails => _pmInvestigationReferralDetails.Value;

    /// <summary>
    /// Hazard File parameters (missing)
    /// </summary>
    private static readonly Lazy<string> _pmHazardFileId = new Lazy<string>(() => "@pID");
    public static string pmHazardFileId => _pmHazardFileId.Value;

    private static readonly Lazy<string> _pmHazardFileCode = new Lazy<string>(() => "@pCode");
    public static string pmHazardFileCode => _pmHazardFileCode.Value;

    private static readonly Lazy<string> _pmHazardFileHazardCode = new Lazy<string>(() => "@pHazardCode");
    public static string pmHazardFileHazardCode => _pmHazardFileHazardCode.Value;

    private static readonly Lazy<string> _pmHazardFileReportCode = new Lazy<string>(() => "@pReportCode");
    public static string pmHazardFileReportCode => _pmHazardFileReportCode.Value;

    private static readonly Lazy<string> _pmHazardFileFileName = new Lazy<string>(() => "@pFileName");
    public static string pmHazardFileFileName => _pmHazardFileFileName.Value;

    private static readonly Lazy<string> _pmHazardFileFileType = new Lazy<string>(() => "@pFileType");
    public static string pmHazardFileFileType => _pmHazardFileFileType.Value;

    private static readonly Lazy<string> _pmHazardFileContentType = new Lazy<string>(() => "@pContentType");
    public static string pmHazardFileContentType => _pmHazardFileContentType.Value;

    private static readonly Lazy<string> _pmHazardFileFileSizeBytes = new Lazy<string>(() => "@pFileSizeBytes");
    public static string pmHazardFileFileSizeBytes => _pmHazardFileFileSizeBytes.Value;

    private static readonly Lazy<string> _pmHazardFileFileHash = new Lazy<string>(() => "@pFileHash");
    public static string pmHazardFileFileHash => _pmHazardFileFileHash.Value;

    private static readonly Lazy<string> _pmHazardFileStorageType = new Lazy<string>(() => "@pStorageType");
    public static string pmHazardFileStorageType => _pmHazardFileStorageType.Value;

    private static readonly Lazy<string> _pmHazardFileFilePath = new Lazy<string>(() => "@pFilePath");
    public static string pmHazardFileFilePath => _pmHazardFileFilePath.Value;

    private static readonly Lazy<string> _pmHazardFileFileData = new Lazy<string>(() => "@pFileData");
    public static string pmHazardFileFileData => _pmHazardFileFileData.Value;

    private static readonly Lazy<string> _pmHazardFileDescription = new Lazy<string>(() => "@pDescription");
    public static string pmHazardFileDescription => _pmHazardFileDescription.Value;

    private static readonly Lazy<string> _pmHazardFileCategory = new Lazy<string>(() => "@pCategory");
    public static string pmHazardFileCategory => _pmHazardFileCategory.Value;

    private static readonly Lazy<string> _pmHazardFileIsConfidential = new Lazy<string>(() => "@pIsConfidential");
    public static string pmHazardFileIsConfidential => _pmHazardFileIsConfidential.Value;

    private static readonly Lazy<string> _pmHazardFileUploadedBy = new Lazy<string>(() => "@pUploadedBy");
    public static string pmHazardFileUploadedBy => _pmHazardFileUploadedBy.Value;

    private static readonly Lazy<string> _pmHazardFileUploadedDate = new Lazy<string>(() => "@pUploadedDate");
    public static string pmHazardFileUploadedDate => _pmHazardFileUploadedDate.Value;

    private static readonly Lazy<string> _pmHazardFileTags = new Lazy<string>(() => "@pTags");
    public static string pmHazardFileTags => _pmHazardFileTags.Value;

    private static readonly Lazy<string> _pmHazardFileIsActive = new Lazy<string>(() => "@pIsActive");
    public static string pmHazardFileIsActive => _pmHazardFileIsActive.Value;

    private static readonly Lazy<string> _pmHazardFileInactiveReason = new Lazy<string>(() => "@pInactiveReason");
    public static string pmHazardFileInactiveReason => _pmHazardFileInactiveReason.Value;

    private static readonly Lazy<string> _pmHazardFileInactiveDate = new Lazy<string>(() => "@pInactiveDate");
    public static string pmHazardFileInactiveDate => _pmHazardFileInactiveDate.Value;

    private static readonly Lazy<string> _pmHazardFileInactiveBy = new Lazy<string>(() => "@pInactiveBy");
    public static string pmHazardFileInactiveBy => _pmHazardFileInactiveBy.Value;

    /// <summary>
    /// Additional Hazard File search and utility parameters
    /// </summary>
    private static readonly Lazy<string> _pmHazardFileIncludeFileData = new Lazy<string>(() => "@pIncludeFileData");
    public static string pmHazardFileIncludeFileData => _pmHazardFileIncludeFileData.Value;

    private static readonly Lazy<string> _pmHazardFileSearchText = new Lazy<string>(() => "@pSearchText");
    public static string pmHazardFileSearchText => _pmHazardFileSearchText.Value;

    private static readonly Lazy<string> _pmHazardFileDateFrom = new Lazy<string>(() => "@pDateFrom");
    public static string pmHazardFileDateFrom => _pmHazardFileDateFrom.Value;

    private static readonly Lazy<string> _pmHazardFileDateTo = new Lazy<string>(() => "@pDateTo");
    public static string pmHazardFileDateTo => _pmHazardFileDateTo.Value;

    private static readonly Lazy<string> _pmHazardFileIncludeConfidential = new Lazy<string>(() => "@pIncludeConfidential");
    public static string pmHazardFileIncludeConfidential => _pmHazardFileIncludeConfidential.Value;

    private static readonly Lazy<string> _pmHazardFileMaxResults = new Lazy<string>(() => "@pMaxResults");
    public static string pmHazardFileMaxResults => _pmHazardFileMaxResults.Value;

    #region SMS Stakeholder Groups Parameters

    /// <summary>
    /// SMS Stakeholder Groups table parameters
    /// </summary>
    private static readonly Lazy<string> _pmSMSStakeholderGroupCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSStakeholderGroupCode => _pmSMSStakeholderGroupCode.Value;

    private static readonly Lazy<string> _pmSMSStakeholderGroupName = new Lazy<string>(() => "@pName");
    public static string pmSMSStakeholderGroupName => _pmSMSStakeholderGroupName.Value;

    private static readonly Lazy<string> _pmSMSStakeholderGroupDescription = new Lazy<string>(() => "@pDescription");
    public static string pmSMSStakeholderGroupDescription => _pmSMSStakeholderGroupDescription.Value;

    private static readonly Lazy<string> _pmSMSStakeholderGroupIsActive = new Lazy<string>(() => "@pIsActive");
    public static string pmSMSStakeholderGroupIsActive => _pmSMSStakeholderGroupIsActive.Value;

    #endregion

    #region SMS Application Groups Parameters

    /// <summary>
    /// SMS Application Groups table parameters

    /// </summary>
    private static readonly Lazy<string> _pmSMSApplicationGroupUserCode = new Lazy<string>(() => "@pUserCode");
    public static string pmSMSApplicationGroupUserCode => _pmSMSApplicationGroupUserCode.Value;

    private static readonly Lazy<string> _pmSMSApplicationGroupCode = new Lazy<string>(() => "@pGroupCode");
    public static string pmSMSApplicationGroupCode => _pmSMSApplicationGroupCode.Value;

    private static readonly Lazy<string> _pmSMSApplicationGroupName = new Lazy<string>(() => "@pName");
    public static string pmSMSApplicationGroupName => _pmSMSApplicationGroupName.Value;

    private static readonly Lazy<string> _pmSMSApplicationGroupDescription = new Lazy<string>(() => "@pDescription");
    public static string pmSMSApplicationGroupDescription => _pmSMSApplicationGroupDescription.Value;

    private static readonly Lazy<string> _pmSMSApplicationGroupIsActive = new Lazy<string>(() => "@pIsActive");
    public static string pmSMSApplicationGroupIsActive => _pmSMSApplicationGroupIsActive.Value;

    private static readonly Lazy<string> _pmSMSApplicationGroupClearedBy = new Lazy<string>(() => "@pClearedBy");
    public static string pmSMSApplicationGroupClearedBy => _pmSMSApplicationGroupClearedBy.Value;
    #endregion

    #region SMS Organizational Groups Parameters

    /// <summary>
    /// SMS Organizational Groups table parameters
    /// </summary>
    private static readonly Lazy<string> _pmSMSOrganizationalGroupCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSOrganizationalGroupCode => _pmSMSOrganizationalGroupCode.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalGroupName = new Lazy<string>(() => "@pName");
    public static string pmSMSOrganizationalGroupName => _pmSMSOrganizationalGroupName.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalGroupDescription = new Lazy<string>(() => "@pDescription");
    public static string pmSMSOrganizationalGroupDescription => _pmSMSOrganizationalGroupDescription.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalGroupGroupType = new Lazy<string>(() => "@pGroupType");
    public static string pmSMSOrganizationalGroupGroupType => _pmSMSOrganizationalGroupGroupType.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalGroupAuthorityLevel = new Lazy<string>(() => "@pAuthorityLevel");
    public static string pmSMSOrganizationalGroupAuthorityLevel => _pmSMSOrganizationalGroupAuthorityLevel.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalGroupIsActive = new Lazy<string>(() => "@pIsActive");
    public static string pmSMSOrganizationalGroupIsActive => _pmSMSOrganizationalGroupIsActive.Value;

    /// <summary>
    /// SMS Organizational User Group Assignment parameters
    /// </summary>
    private static readonly Lazy<string> _pmSMSOrganizationalGroupUserCode = new Lazy<string>(() => "@pUserCode");
    public static string pmSMSOrganizationalGroupUserCode => _pmSMSOrganizationalGroupUserCode.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalGroupCodeForAssignment = new Lazy<string>(() => "@pGroupCode");
    public static string pmSMSOrganizationalGroupCodeForAssignment => _pmSMSOrganizationalGroupCodeForAssignment.Value;

    private static readonly Lazy<string> _pmSMSOrganizationalGroupClearedBy = new Lazy<string>(() => "@pClearedBy");
    public static string pmSMSOrganizationalGroupClearedBy => _pmSMSOrganizationalGroupClearedBy.Value;

    #endregion

    #region Common Group Assignment Parameters

    /// <summary>
    /// Common parameters used across group assignment operations
    /// </summary>
    private static readonly Lazy<string> _pmAssignedDate = new Lazy<string>(() => "@pAssignedDate");
    public static string pmAssignedDate => _pmAssignedDate.Value;

    #endregion

    /// <summary>
    /// Common parameter aliases for consistency
    /// </summary>
    private static readonly Lazy<string> _pmCode = new Lazy<string>(() => "@pCode");
    public static string pmCode => _pmCode.Value;

    private static readonly Lazy<string> _pmUserId = new Lazy<string>(() => "@pUserID");
    public static string pmUserId => _pmUserId.Value;

    // ✅ ADDITIONAL MISSING PARAMETERS - ADDED TO ALIGN WITH STORED PROCEDURES 

    /// <summary>
    /// NEW: Missing output parameters used in Mitigation stored procedures
    /// </summary>
    private static readonly Lazy<string> _pmNewID = new Lazy<string>(() => "@pNewID");
    public static string pmNewID => _pmNewID.Value;

    private static readonly Lazy<string> _pmNewMitigationCode = new Lazy<string>(() => "@pNewMitigationCode");
    public static string pmNewMitigationCode => _pmNewMitigationCode.Value;

    private static readonly Lazy<string> _pmNewMitigationAssignmentCode = new Lazy<string>(() => "@pNewMitigationAssignmentCode");
    public static string pmNewMitigationAssignmentCode => _pmNewMitigationAssignmentCode.Value;

    /// <summary>
    /// NEW: Corrected Mitigation parameter IDs to match stored procedures (@pID not @pMitigationID)
    /// </summary>
    private static readonly Lazy<string> _pmMitigationIdCorrected = new Lazy<string>(() => "@pID");
    public static string pmMitigationIdCorrected => _pmMitigationIdCorrected.Value;

    private static readonly Lazy<string> _pmMitigationAssignmentIdCorrected = new Lazy<string>(() => "@pID");
    public static string pmMitigationAssignmentIdCorrected => _pmMitigationAssignmentIdCorrected.Value;

    /// <summary>
    /// ENHANCED MITIGATION PARAMETERS - All comprehensive mitigation fields
    /// </summary>

    // Core Mitigation Fields
    private static readonly Lazy<string> _pmMitigationName = new Lazy<string>(() => "@pMitigationName");
    public static string pmMitigationName => _pmMitigationName.Value;

    private static readonly Lazy<string> _pmMitigationDescription = new Lazy<string>(() => "@pMitigationDescription");
    public static string pmMitigationDescription => _pmMitigationDescription.Value;

    private static readonly Lazy<string> _pmMitigationType = new Lazy<string>(() => "@pMitigationType");
    public static string pmMitigationType => _pmMitigationType.Value;

    private static readonly Lazy<string> _pmMitigationStatus = new Lazy<string>(() => "@pMitigationStatus");
    public static string pmMitigationStatus => _pmMitigationStatus.Value;

    private static readonly Lazy<string> _pmMitigationPriority = new Lazy<string>(() => "@pMitigationPriority");
    public static string pmMitigationPriority => _pmMitigationPriority.Value;

    private static readonly Lazy<string> _pmMitigationRiskAssessmentCode = new Lazy<string>(() => "@pMitigationRiskAssessmentCode");
    public static string pmMitigationRiskAssessmentCode => _pmMitigationRiskAssessmentCode.Value;

    // Timeline Fields
    private static readonly Lazy<string> _pmMitigationTargetDate = new Lazy<string>(() => "@pMitigationTargetDate");
    public static string pmMitigationTargetDate => _pmMitigationTargetDate.Value;

    private static readonly Lazy<string> _pmMitigationCompletionDate = new Lazy<string>(() => "@pMitigationCompletionDate");
    public static string pmMitigationCompletionDate => _pmMitigationCompletionDate.Value;

    // Assignment Fields
    private static readonly Lazy<string> _pmMitigationAssignedDepartment = new Lazy<string>(() => "@pMitigationAssignedDepartment");
    public static string pmMitigationAssignedDepartment => _pmMitigationAssignedDepartment.Value;

    private static readonly Lazy<string> _pmMitigationAssignedTo = new Lazy<string>(() => "@pMitigationAssignedTo");
    public static string pmMitigationAssignedTo => _pmMitigationAssignedTo.Value;

    private static readonly Lazy<string> _pmMitigationApprovedBy = new Lazy<string>(() => "@pMitigationApprovedBy");
    public static string pmMitigationApprovedBy => _pmMitigationApprovedBy.Value;

    private static readonly Lazy<string> _pmMitigationApprovedDate = new Lazy<string>(() => "@pMitigationApprovedDate");
    public static string pmMitigationApprovedDate => _pmMitigationApprovedDate.Value;

    // Progress Fields
    private static readonly Lazy<string> _pmMitigationProgress = new Lazy<string>(() => "@pMitigationProgress");
    public static string pmMitigationProgress => _pmMitigationProgress.Value;

    private static readonly Lazy<string> _pmMitigationProgressNotes = new Lazy<string>(() => "@pMitigationProgressNotes");
    public static string pmMitigationProgressNotes => _pmMitigationProgressNotes.Value;

    private static readonly Lazy<string> _pmMitigationLastProgressUpdate = new Lazy<string>(() => "@pMitigationLastProgressUpdate");
    public static string pmMitigationLastProgressUpdate => _pmMitigationLastProgressUpdate.Value;

    private static readonly Lazy<string> _pmMitigationProgressUpdatedBy = new Lazy<string>(() => "@pMitigationProgressUpdatedBy");
    public static string pmMitigationProgressUpdatedBy => _pmMitigationProgressUpdatedBy.Value;

    // Cost and Resource Fields
    private static readonly Lazy<string> _pmMitigationEstimatedCost = new Lazy<string>(() => "@pMitigationEstimatedCost");
    public static string pmMitigationEstimatedCost => _pmMitigationEstimatedCost.Value;

    private static readonly Lazy<string> _pmMitigationActualCost = new Lazy<string>(() => "@pMitigationActualCost");
    public static string pmMitigationActualCost => _pmMitigationActualCost.Value;

    private static readonly Lazy<string> _pmMitigationResourceRequirements = new Lazy<string>(() => "@pMitigationResourceRequirements");
    public static string pmMitigationResourceRequirements => _pmMitigationResourceRequirements.Value;

    private static readonly Lazy<string> _pmMitigationEstimatedHours = new Lazy<string>(() => "@pMitigationEstimatedHours");
    public static string pmMitigationEstimatedHours => _pmMitigationEstimatedHours.Value;

    private static readonly Lazy<string> _pmMitigationActualHours = new Lazy<string>(() => "@pMitigationActualHours");
    public static string pmMitigationActualHours => _pmMitigationActualHours.Value;

    // Effectiveness Fields
    private static readonly Lazy<string> _pmMitigationEffectivenessRating = new Lazy<string>(() => "@pMitigationEffectivenessRating");
    public static string pmMitigationEffectivenessRating => _pmMitigationEffectivenessRating.Value;

    private static readonly Lazy<string> _pmMitigationEffectivenessNotes = new Lazy<string>(() => "@pMitigationEffectivenessNotes");
    public static string pmMitigationEffectivenessNotes => _pmMitigationEffectivenessNotes.Value;

    private static readonly Lazy<string> _pmMitigationEffectivenessReviewDate = new Lazy<string>(() => "@pMitigationEffectivenessReviewDate");
    public static string pmMitigationEffectivenessReviewDate => _pmMitigationEffectivenessReviewDate.Value;

    private static readonly Lazy<string> _pmMitigationEffectivenessReviewedBy = new Lazy<string>(() => "@pMitigationEffectivenessReviewedBy");
    public static string pmMitigationEffectivenessReviewedBy => _pmMitigationEffectivenessReviewedBy.Value;

    // Monitoring Fields
    private static readonly Lazy<string> _pmMitigationMonitoringRequirements = new Lazy<string>(() => "@pMitigationMonitoringRequirements");
    public static string pmMitigationMonitoringRequirements => _pmMitigationMonitoringRequirements.Value;

    private static readonly Lazy<string> _pmMitigationMonitoringFrequency = new Lazy<string>(() => "@pMitigationMonitoringFrequency");
    public static string pmMitigationMonitoringFrequency => _pmMitigationMonitoringFrequency.Value;

    // Risk Reduction Fields
    private static readonly Lazy<string> _pmMitigationExpectedSeverityReduction = new Lazy<string>(() => "@pMitigationExpectedSeverityReduction");
    public static string pmMitigationExpectedSeverityReduction => _pmMitigationExpectedSeverityReduction.Value;

    private static readonly Lazy<string> _pmMitigationExpectedLikelihoodReduction = new Lazy<string>(() => "@pMitigationExpectedLikelihoodReduction");
    public static string pmMitigationExpectedLikelihoodReduction => _pmMitigationExpectedLikelihoodReduction.Value;

    private static readonly Lazy<string> _pmMitigationActualSeverityReduction = new Lazy<string>(() => "@pMitigationActualSeverityReduction");
    public static string pmMitigationActualSeverityReduction => _pmMitigationActualSeverityReduction.Value;

    private static readonly Lazy<string> _pmMitigationActualLikelihoodReduction = new Lazy<string>(() => "@pMitigationActualLikelihoodReduction");
    public static string pmMitigationActualLikelihoodReduction => _pmMitigationActualLikelihoodReduction.Value;

    private static readonly Lazy<string> _pmMitigationResidualRiskLevel = new Lazy<string>(() => "@pMitigationResidualRiskLevel");
    public static string pmMitigationResidualRiskLevel => _pmMitigationResidualRiskLevel.Value;

    // Dependency Fields
    private static readonly Lazy<string> _pmMitigationPrerequisites = new Lazy<string>(() => "@pMitigationPrerequisites");
    public static string pmMitigationPrerequisites => _pmMitigationPrerequisites.Value;

    private static readonly Lazy<string> _pmMitigationDependencies = new Lazy<string>(() => "@pMitigationDependencies");
    public static string pmMitigationDependencies => _pmMitigationDependencies.Value;

    private static readonly Lazy<string> _pmMitigationHasDependencies = new Lazy<string>(() => "@pMitigationHasDependencies");
    public static string pmMitigationHasDependencies => _pmMitigationHasDependencies.Value;

    private static readonly Lazy<string> _pmMitigationIsPrerequisite = new Lazy<string>(() => "@pMitigationIsPrerequisite");
    public static string pmMitigationIsPrerequisite => _pmMitigationIsPrerequisite.Value;

    // Planning Fields
    private static readonly Lazy<string> _pmMitigationImplementationPlan = new Lazy<string>(() => "@pMitigationImplementationPlan");
    public static string pmMitigationImplementationPlan => _pmMitigationImplementationPlan.Value;

    private static readonly Lazy<string> _pmMitigationCommunicationPlan = new Lazy<string>(() => "@pMitigationCommunicationPlan");
    public static string pmMitigationCommunicationPlan => _pmMitigationCommunicationPlan.Value;

    private static readonly Lazy<string> _pmMitigationTrainingRequirements = new Lazy<string>(() => "@pMitigationTrainingRequirements");
    public static string pmMitigationTrainingRequirements => _pmMitigationTrainingRequirements.Value;

    private static readonly Lazy<string> _pmMitigationDocumentationUpdates = new Lazy<string>(() => "@pMitigationDocumentationUpdates");
    public static string pmMitigationDocumentationUpdates => _pmMitigationDocumentationUpdates.Value;

    // Testing Fields
    private static readonly Lazy<string> _pmMitigationTestingProcedure = new Lazy<string>(() => "@pMitigationTestingProcedure");
    public static string pmMitigationTestingProcedure => _pmMitigationTestingProcedure.Value;

    private static readonly Lazy<string> _pmMitigationTestingCompletedDate = new Lazy<string>(() => "@pMitigationTestingCompletedDate");
    public static string pmMitigationTestingCompletedDate => _pmMitigationTestingCompletedDate.Value;

    private static readonly Lazy<string> _pmMitigationTestingResults = new Lazy<string>(() => "@pMitigationTestingResults");
    public static string pmMitigationTestingResults => _pmMitigationTestingResults.Value;

    // Validation Fields
    private static readonly Lazy<string> _pmMitigationValidationRequired = new Lazy<string>(() => "@pMitigationValidationRequired");
    public static string pmMitigationValidationRequired => _pmMitigationValidationRequired.Value;

    private static readonly Lazy<string> _pmMitigationValidationDate = new Lazy<string>(() => "@pMitigationValidationDate");
    public static string pmMitigationValidationDate => _pmMitigationValidationDate.Value;

    private static readonly Lazy<string> _pmMitigationValidatedBy = new Lazy<string>(() => "@pMitigationValidatedBy");
    public static string pmMitigationValidatedBy => _pmMitigationValidatedBy.Value;

    // Additional Fields
    private static readonly Lazy<string> _pmMitigationNotes = new Lazy<string>(() => "@pMitigationNotes");
    public static string pmMitigationNotes => _pmMitigationNotes.Value;

    private static readonly Lazy<string> _pmMitigationLessonsLearned = new Lazy<string>(() => "@pMitigationLessonsLearned");
    public static string pmMitigationLessonsLearned => _pmMitigationLessonsLearned.Value;

    private static readonly Lazy<string> _pmMitigationRecommendationsForFuture = new Lazy<string>(() => "@pMitigationRecommendationsForFuture");
    public static string pmMitigationRecommendationsForFuture => _pmMitigationRecommendationsForFuture.Value;

    /// <summary>
    /// Safety Performance Indicator parameters
    /// </summary>
    private static readonly Lazy<string> _pmSPIId = new Lazy<string>(() => "@pID");
    public static string pmSPIId => _pmSPIId.Value;

    private static readonly Lazy<string> _pmSPICode = new Lazy<string>(() => "@pCode");
    public static string pmSPICode => _pmSPICode.Value;

    private static readonly Lazy<string> _pmSPIName = new Lazy<string>(() => "@pName");
    public static string pmSPIName => _pmSPIName.Value;

    private static readonly Lazy<string> _pmSPIDescription = new Lazy<string>(() => "@pDescription");
    public static string pmSPIDescription => _pmSPIDescription.Value;

    private static readonly Lazy<string> _pmSPIIndicatorType = new Lazy<string>(() => "@pIndicatorType");
    public static string pmSPIIndicatorType => _pmSPIIndicatorType.Value;

    private static readonly Lazy<string> _pmSPIStatus = new Lazy<string>(() => "@pStatus");
    public static string pmSPIStatus => _pmSPIStatus.Value;

    private static readonly Lazy<string> _pmSPIMeasurementUnit = new Lazy<string>(() => "@pMeasurementUnit");
    public static string pmSPIMeasurementUnit => _pmSPIMeasurementUnit.Value;

    private static readonly Lazy<string> _pmSPIMeasurementFrequency = new Lazy<string>(() => "@pMeasurementFrequency");
    public static string pmSPIMeasurementFrequency => _pmSPIMeasurementFrequency.Value;

    private static readonly Lazy<string> _pmSPICalculationMethod = new Lazy<string>(() => "@pCalculationMethod");
    public static string pmSPICalculationMethod => _pmSPICalculationMethod.Value;

    private static readonly Lazy<string> _pmSPIDataSource = new Lazy<string>(() => "@pDataSource");
    public static string pmSPIDataSource => _pmSPIDataSource.Value;

    private static readonly Lazy<string> _pmSPITargetValue = new Lazy<string>(() => "@pTargetValue");
    public static string pmSPITargetValue => _pmSPITargetValue.Value;

    private static readonly Lazy<string> _pmSPIAcceptableRange = new Lazy<string>(() => "@pAcceptableRange");
    public static string pmSPIAcceptableRange => _pmSPIAcceptableRange.Value;

    private static readonly Lazy<string> _pmSPIWarningThreshold = new Lazy<string>(() => "@pWarningThreshold");
    public static string pmSPIWarningThreshold => _pmSPIWarningThreshold.Value;

    private static readonly Lazy<string> _pmSPICriticalThreshold = new Lazy<string>(() => "@pCriticalThreshold");
    public static string pmSPICriticalThreshold => _pmSPICriticalThreshold.Value;

    private static readonly Lazy<string> _pmSPIResponsibleDepartment = new Lazy<string>(() => "@pResponsibleDepartment");
    public static string pmSPIResponsibleDepartment => _pmSPIResponsibleDepartment.Value;

    private static readonly Lazy<string> _pmSPIDataOwner = new Lazy<string>(() => "@pDataOwner");
    public static string pmSPIDataOwner => _pmSPIDataOwner.Value;

    private static readonly Lazy<string> _pmSPIReviewAuthority = new Lazy<string>(() => "@pReviewAuthority");
    public static string pmSPIReviewAuthority => _pmSPIReviewAuthority.Value;

    private static readonly Lazy<string> _pmSPINextReviewDate = new Lazy<string>(() => "@pNextReviewDate");
    public static string pmSPINextReviewDate => _pmSPINextReviewDate.Value;

    private static readonly Lazy<string> _pmSPILastReviewDate = new Lazy<string>(() => "@pLastReviewDate");
    public static string pmSPILastReviewDate => _pmSPILastReviewDate.Value;

    private static readonly Lazy<string> _pmSPILastReviewNotes = new Lazy<string>(() => "@pLastReviewNotes");
    public static string pmSPILastReviewNotes => _pmSPILastReviewNotes.Value;

    private static readonly Lazy<string> _pmSPIAlertsEnabled = new Lazy<string>(() => "@pAlertsEnabled");
    public static string pmSPIAlertsEnabled => _pmSPIAlertsEnabled.Value;

    private static readonly Lazy<string> _pmSPIAlertRecipients = new Lazy<string>(() => "@pAlertRecipients");
    public static string pmSPIAlertRecipients => _pmSPIAlertRecipients.Value;

    /// <summary>
    /// SPI Data Point parameters
    /// </summary>
    private static readonly Lazy<string> _pmSPIDataPointSPIId = new Lazy<string>(() => "@pSPIId");
    public static string pmSPIDataPointSPIId => _pmSPIDataPointSPIId.Value;

    private static readonly Lazy<string> _pmSPIDataPointValue = new Lazy<string>(() => "@pValue");
    public static string pmSPIDataPointValue => _pmSPIDataPointValue.Value;

    private static readonly Lazy<string> _pmSPIDataPointMeasurementDate = new Lazy<string>(() => "@pMeasurementDate");
    public static string pmSPIDataPointMeasurementDate => _pmSPIDataPointMeasurementDate.Value;

    private static readonly Lazy<string> _pmSPIDataPointPeriod = new Lazy<string>(() => "@pPeriod");
    public static string pmSPIDataPointPeriod => _pmSPIDataPointPeriod.Value;

    private static readonly Lazy<string> _pmSPIDataPointDataSource = new Lazy<string>(() => "@pDataSource");
    public static string pmSPIDataPointDataSource => _pmSPIDataPointDataSource.Value;

    private static readonly Lazy<string> _pmSPIDataPointEnteredBy = new Lazy<string>(() => "@pEnteredBy");
    public static string pmSPIDataPointEnteredBy => _pmSPIDataPointEnteredBy.Value;

    private static readonly Lazy<string> _pmSPIDataPointEnteredDate = new Lazy<string>(() => "@pEnteredDate");
    public static string pmSPIDataPointEnteredDate => _pmSPIDataPointEnteredDate.Value;

    private static readonly Lazy<string> _pmSPIDataPointNotes = new Lazy<string>(() => "@pNotes");
    public static string pmSPIDataPointNotes => _pmSPIDataPointNotes.Value;

    private static readonly Lazy<string> _pmSPIDataPointIsVerified = new Lazy<string>(() => "@pIsVerified");
    public static string pmSPIDataPointIsVerified => _pmSPIDataPointIsVerified.Value;

    private static readonly Lazy<string> _pmSPIDataPointVerifiedBy = new Lazy<string>(() => "@pVerifiedBy");
    public static string pmSPIDataPointVerifiedBy => _pmSPIDataPointVerifiedBy.Value;

    private static readonly Lazy<string> _pmSPIDataPointVerifiedDate = new Lazy<string>(() => "@pVerifiedDate");
    public static string pmSPIDataPointVerifiedDate => _pmSPIDataPointVerifiedDate.Value;

    /// <summary>
    /// SMS Audit Management parameters
    /// </summary>

    // SMS Audit Plan parameters
    private static readonly Lazy<string> _pmSMSAuditPlanId = new Lazy<string>(() => "@pID");
    public static string pmSMSAuditPlanId => _pmSMSAuditPlanId.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSAuditPlanCode => _pmSMSAuditPlanCode.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanName = new Lazy<string>(() => "@pName");
    public static string pmSMSAuditPlanName => _pmSMSAuditPlanName.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanDescription = new Lazy<string>(() => "@pDescription");
    public static string pmSMSAuditPlanDescription => _pmSMSAuditPlanDescription.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanAuditType = new Lazy<string>(() => "@pAuditType");
    public static string pmSMSAuditPlanAuditType => _pmSMSAuditPlanAuditType.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanScope = new Lazy<string>(() => "@pScope");
    public static string pmSMSAuditPlanScope => _pmSMSAuditPlanScope.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanObjectives = new Lazy<string>(() => "@pObjectives");
    public static string pmSMSAuditPlanObjectives => _pmSMSAuditPlanObjectives.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanPlannedStartDate = new Lazy<string>(() => "@pPlannedStartDate");
    public static string pmSMSAuditPlanPlannedStartDate => _pmSMSAuditPlanPlannedStartDate.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanPlannedEndDate = new Lazy<string>(() => "@pPlannedEndDate");
    public static string pmSMSAuditPlanPlannedEndDate => _pmSMSAuditPlanPlannedEndDate.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanLeadAuditor = new Lazy<string>(() => "@pLeadAuditor");
    public static string pmSMSAuditPlanLeadAuditor => _pmSMSAuditPlanLeadAuditor.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanAuditorTeam = new Lazy<string>(() => "@pAuditorTeam");
    public static string pmSMSAuditPlanAuditorTeam => _pmSMSAuditPlanAuditorTeam.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanResponsibleDepartment = new Lazy<string>(() => "@pResponsibleDepartment");
    public static string pmSMSAuditPlanResponsibleDepartment => _pmSMSAuditPlanResponsibleDepartment.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanStatus = new Lazy<string>(() => "@pStatus");
    public static string pmSMSAuditPlanStatus => _pmSMSAuditPlanStatus.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanPriority = new Lazy<string>(() => "@pPriority");
    public static string pmSMSAuditPlanPriority => _pmSMSAuditPlanPriority.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanRecurrencePattern = new Lazy<string>(() => "@pRecurrencePattern");
    public static string pmSMSAuditPlanRecurrencePattern => _pmSMSAuditPlanRecurrencePattern.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanRequiresApproval = new Lazy<string>(() => "@pRequiresApproval");
    public static string pmSMSAuditPlanRequiresApproval => _pmSMSAuditPlanRequiresApproval.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanApprovedBy = new Lazy<string>(() => "@pApprovedBy");
    public static string pmSMSAuditPlanApprovedBy => _pmSMSAuditPlanApprovedBy.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanApprovedDate = new Lazy<string>(() => "@pApprovedDate");
    public static string pmSMSAuditPlanApprovedDate => _pmSMSAuditPlanApprovedDate.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanExpectedDurationHours = new Lazy<string>(() => "@pExpectedDurationHours");
    public static string pmSMSAuditPlanExpectedDurationHours => _pmSMSAuditPlanExpectedDurationHours.Value;

    private static readonly Lazy<string> _pmSMSAuditPlanNotes = new Lazy<string>(() => "@pNotes");
    public static string pmSMSAuditPlanNotes => _pmSMSAuditPlanNotes.Value;

    // SMS Audit parameters
    private static readonly Lazy<string> _pmSMSAuditId = new Lazy<string>(() => "@pID");
    public static string pmSMSAuditId => _pmSMSAuditId.Value;

    private static readonly Lazy<string> _pmSMSAuditCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSAuditCode => _pmSMSAuditCode.Value;

    private static readonly Lazy<string> _pmSMSAuditName = new Lazy<string>(() => "@pName");
    public static string pmSMSAuditName => _pmSMSAuditName.Value;

    private static readonly Lazy<string> _pmSMSAuditDescription = new Lazy<string>(() => "@pDescription");
    public static string pmSMSAuditDescription => _pmSMSAuditDescription.Value;

    private static readonly Lazy<string> _pmSMSAuditAuditPlanCode = new Lazy<string>(() => "@pAuditPlanCode");
    public static string pmSMSAuditAuditPlanCode => _pmSMSAuditAuditPlanCode.Value;

    private static readonly Lazy<string> _pmSMSAuditAuditType = new Lazy<string>(() => "@pAuditType");
    public static string pmSMSAuditAuditType => _pmSMSAuditAuditType.Value;

    private static readonly Lazy<string> _pmSMSAuditScope = new Lazy<string>(() => "@pScope");
    public static string pmSMSAuditScope => _pmSMSAuditScope.Value;

    private static readonly Lazy<string> _pmSMSAuditObjectives = new Lazy<string>(() => "@pObjectives");
    public static string pmSMSAuditObjectives => _pmSMSAuditObjectives.Value;

    private static readonly Lazy<string> _pmSMSAuditScheduledStartDate = new Lazy<string>(() => "@pScheduledStartDate");
    public static string pmSMSAuditScheduledStartDate => _pmSMSAuditScheduledStartDate.Value;

    private static readonly Lazy<string> _pmSMSAuditScheduledEndDate = new Lazy<string>(() => "@pScheduledEndDate");
    public static string pmSMSAuditScheduledEndDate => _pmSMSAuditScheduledEndDate.Value;

    private static readonly Lazy<string> _pmSMSAuditActualStartDate = new Lazy<string>(() => "@pActualStartDate");
    public static string pmSMSAuditActualStartDate => _pmSMSAuditActualStartDate.Value;

    private static readonly Lazy<string> _pmSMSAuditActualEndDate = new Lazy<string>(() => "@pActualEndDate");
    public static string pmSMSAuditActualEndDate => _pmSMSAuditActualEndDate.Value;

    private static readonly Lazy<string> _pmSMSAuditLeadAuditor = new Lazy<string>(() => "@pLeadAuditor");
    public static string pmSMSAuditLeadAuditor => _pmSMSAuditLeadAuditor.Value;

    private static readonly Lazy<string> _pmSMSAuditAuditorTeam = new Lazy<string>(() => "@pAuditorTeam");
    public static string pmSMSAuditAuditorTeam => _pmSMSAuditAuditorTeam.Value;

    private static readonly Lazy<string> _pmSMSAuditResponsibleDepartment = new Lazy<string>(() => "@pResponsibleDepartment");
    public static string pmSMSAuditResponsibleDepartment => _pmSMSAuditResponsibleDepartment.Value;

    private static readonly Lazy<string> _pmSMSAuditStatus = new Lazy<string>(() => "@pStatus");
    public static string pmSMSAuditStatus => _pmSMSAuditStatus.Value;

    private static readonly Lazy<string> _pmSMSAuditPriority = new Lazy<string>(() => "@pPriority");
    public static string pmSMSAuditPriority => _pmSMSAuditPriority.Value;

    private static readonly Lazy<string> _pmSMSAuditContactPerson = new Lazy<string>(() => "@pContactPerson");
    public static string pmSMSAuditContactPerson => _pmSMSAuditContactPerson.Value;

    private static readonly Lazy<string> _pmSMSAuditLocation = new Lazy<string>(() => "@pAuditLocation");
    public static string pmSMSAuditLocation => _pmSMSAuditLocation.Value;

    private static readonly Lazy<string> _pmSMSAuditExecutiveSummary = new Lazy<string>(() => "@pExecutiveSummary");
    public static string pmSMSAuditExecutiveSummary => _pmSMSAuditExecutiveSummary.Value;

    private static readonly Lazy<string> _pmSMSAuditNotes = new Lazy<string>(() => "@pNotes");
    public static string pmSMSAuditNotes => _pmSMSAuditNotes.Value;

    // SMS Audit Finding parameters
    private static readonly Lazy<string> _pmSMSAuditFindingId = new Lazy<string>(() => "@pID");
    public static string pmSMSAuditFindingId => _pmSMSAuditFindingId.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSAuditFindingCode => _pmSMSAuditFindingCode.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingAuditCode = new Lazy<string>(() => "@pAuditCode");
    public static string pmSMSAuditFindingAuditCode => _pmSMSAuditFindingAuditCode.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingTitle = new Lazy<string>(() => "@pTitle");
    public static string pmSMSAuditFindingTitle => _pmSMSAuditFindingTitle.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingDescription = new Lazy<string>(() => "@pDescription");
    public static string pmSMSAuditFindingDescription => _pmSMSAuditFindingDescription.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingSeverity = new Lazy<string>(() => "@pSeverity");
    public static string pmSMSAuditFindingSeverity => _pmSMSAuditFindingSeverity.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingCategory = new Lazy<string>(() => "@pCategory");
    public static string pmSMSAuditFindingCategory => _pmSMSAuditFindingCategory.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingStatus = new Lazy<string>(() => "@pStatus");
    public static string pmSMSAuditFindingStatus => _pmSMSAuditFindingStatus.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingDiscoveredDate = new Lazy<string>(() => "@pDiscoveredDate");
    public static string pmSMSAuditFindingDiscoveredDate => _pmSMSAuditFindingDiscoveredDate.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingResponsiblePerson = new Lazy<string>(() => "@pResponsiblePerson");
    public static string pmSMSAuditFindingResponsiblePerson => _pmSMSAuditFindingResponsiblePerson.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingTargetResolutionDate = new Lazy<string>(() => "@pTargetResolutionDate");
    public static string pmSMSAuditFindingTargetResolutionDate => _pmSMSAuditFindingTargetResolutionDate.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingActualResolutionDate = new Lazy<string>(() => "@pActualResolutionDate");
    public static string pmSMSAuditFindingActualResolutionDate => _pmSMSAuditFindingActualResolutionDate.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingCorrectiveAction = new Lazy<string>(() => "@pCorrectiveAction");
    public static string pmSMSAuditFindingCorrectiveAction => _pmSMSAuditFindingCorrectiveAction.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingRootCauseAnalysis = new Lazy<string>(() => "@pRootCauseAnalysis");
    public static string pmSMSAuditFindingRootCauseAnalysis => _pmSMSAuditFindingRootCauseAnalysis.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingVerificationRequired = new Lazy<string>(() => "@pVerificationRequired");
    public static string pmSMSAuditFindingVerificationRequired => _pmSMSAuditFindingVerificationRequired.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingVerifiedBy = new Lazy<string>(() => "@pVerifiedBy");
    public static string pmSMSAuditFindingVerifiedBy => _pmSMSAuditFindingVerifiedBy.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingVerificationDate = new Lazy<string>(() => "@pVerificationDate");
    public static string pmSMSAuditFindingVerificationDate => _pmSMSAuditFindingVerificationDate.Value;

    private static readonly Lazy<string> _pmSMSAuditFindingNotes = new Lazy<string>(() => "@pNotes");
    public static string pmSMSAuditFindingNotes => _pmSMSAuditFindingNotes.Value;

    // SMS Audit Evidence parameters
    private static readonly Lazy<string> _pmSMSAuditEvidenceId = new Lazy<string>(() => "@pID");
    public static string pmSMSAuditEvidenceId => _pmSMSAuditEvidenceId.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceCode = new Lazy<string>(() => "@pCode");
    public static string pmSMSAuditEvidenceCode => _pmSMSAuditEvidenceCode.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceAuditCode = new Lazy<string>(() => "@pAuditCode");
    public static string pmSMSAuditEvidenceAuditCode => _pmSMSAuditEvidenceAuditCode.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceFindingCode = new Lazy<string>(() => "@pFindingCode");
    public static string pmSMSAuditEvidenceFindingCode => _pmSMSAuditEvidenceFindingCode.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceTitle = new Lazy<string>(() => "@pTitle");
    public static string pmSMSAuditEvidenceTitle => _pmSMSAuditEvidenceTitle.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceDescription = new Lazy<string>(() => "@pDescription");
    public static string pmSMSAuditEvidenceDescription => _pmSMSAuditEvidenceDescription.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceType = new Lazy<string>(() => "@pEvidenceType");
    public static string pmSMSAuditEvidenceType => _pmSMSAuditEvidenceType.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceSource = new Lazy<string>(() => "@pSource");
    public static string pmSMSAuditEvidenceSource => _pmSMSAuditEvidenceSource.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceCollectedBy = new Lazy<string>(() => "@pCollectedBy");
    public static string pmSMSAuditEvidenceCollectedBy => _pmSMSAuditEvidenceCollectedBy.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceCollectionDate = new Lazy<string>(() => "@pCollectionDate");
    public static string pmSMSAuditEvidenceCollectionDate => _pmSMSAuditEvidenceCollectionDate.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceFilePath = new Lazy<string>(() => "@pFilePath");
    public static string pmSMSAuditEvidenceFilePath => _pmSMSAuditEvidenceFilePath.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceFileSize = new Lazy<string>(() => "@pFileSize");
    public static string pmSMSAuditEvidenceFileSize => _pmSMSAuditEvidenceFileSize.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceContentType = new Lazy<string>(() => "@pContentType");
    public static string pmSMSAuditEvidenceContentType => _pmSMSAuditEvidenceContentType.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceStorageLocation = new Lazy<string>(() => "@pStorageLocation");
    public static string pmSMSAuditEvidenceStorageLocation => _pmSMSAuditEvidenceStorageLocation.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceConfidentialityLevel = new Lazy<string>(() => "@pConfidentialityLevel");
    public static string pmSMSAuditEvidenceConfidentialityLevel => _pmSMSAuditEvidenceConfidentialityLevel.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceRetentionPeriodMonths = new Lazy<string>(() => "@pRetentionPeriodMonths");
    public static string pmSMSAuditEvidenceRetentionPeriodMonths => _pmSMSAuditEvidenceRetentionPeriodMonths.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceRetentionReason = new Lazy<string>(() => "@pRetentionReason");
    public static string pmSMSAuditEvidenceRetentionReason => _pmSMSAuditEvidenceRetentionReason.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceIsVerified = new Lazy<string>(() => "@pIsVerified");
    public static string pmSMSAuditEvidenceIsVerified => _pmSMSAuditEvidenceIsVerified.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceVerifiedBy = new Lazy<string>(() => "@pVerifiedBy");
    public static string pmSMSAuditEvidenceVerifiedBy => _pmSMSAuditEvidenceVerifiedBy.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceVerificationDate = new Lazy<string>(() => "@pVerificationDate");
    public static string pmSMSAuditEvidenceVerificationDate => _pmSMSAuditEvidenceVerificationDate.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceIsArchived = new Lazy<string>(() => "@pIsArchived");
    public static string pmSMSAuditEvidenceIsArchived => _pmSMSAuditEvidenceIsArchived.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceArchivedDate = new Lazy<string>(() => "@pArchivedDate");
    public static string pmSMSAuditEvidenceArchivedDate => _pmSMSAuditEvidenceArchivedDate.Value;

    private static readonly Lazy<string> _pmSMSAuditEvidenceNotes = new Lazy<string>(() => "@pNotes");
    public static string pmSMSAuditEvidenceNotes => _pmSMSAuditEvidenceNotes.Value;

    // Common filter parameters for SMS Audit Management
    private static readonly Lazy<string> _pmStatusFilter = new Lazy<string>(() => "@pStatusFilter");
    public static string pmStatusFilter => _pmStatusFilter.Value;

    private static readonly Lazy<string> _pmAuditTypeFilter = new Lazy<string>(() => "@pAuditTypeFilter");
    public static string pmAuditTypeFilter => _pmAuditTypeFilter.Value;

    private static readonly Lazy<string> _pmDepartmentFilter = new Lazy<string>(() => "@pDepartmentFilter");
    public static string pmDepartmentFilter => _pmDepartmentFilter.Value;

    private static readonly Lazy<string> _pmAuditorFilter = new Lazy<string>(() => "@pAuditorFilter");
    public static string pmAuditorFilter => _pmAuditorFilter.Value;

    private static readonly Lazy<string> _pmStartDateFrom = new Lazy<string>(() => "@pStartDateFrom");
    public static string pmStartDateFrom => _pmStartDateFrom.Value;

    private static readonly Lazy<string> _pmStartDateTo = new Lazy<string>(() => "@pStartDateTo");
    public static string pmStartDateTo => _pmStartDateTo.Value;

    private static readonly Lazy<string> _pmIncludeFindings = new Lazy<string>(() => "@pIncludeFindings");
    public static string pmIncludeFindings => _pmIncludeFindings.Value;

    private static readonly Lazy<string> _pmIncludeEvidence = new Lazy<string>(() => "@pIncludeEvidence");
    public static string pmIncludeEvidence => _pmIncludeEvidence.Value;

    private static readonly Lazy<string> _pmSeverityFilter = new Lazy<string>(() => "@pSeverityFilter");
    public static string pmSeverityFilter => _pmSeverityFilter.Value;

    private static readonly Lazy<string> _pmEvidenceTypeFilter = new Lazy<string>(() => "@pEvidenceTypeFilter");
    public static string pmEvidenceTypeFilter => _pmEvidenceTypeFilter.Value;

    private static readonly Lazy<string> _pmConfidentialityLevel = new Lazy<string>(() => "@pConfidentialityLevel");
    public static string pmConfidentialityLevel => _pmConfidentialityLevel.Value;

    private static readonly Lazy<string> _pmRetentionReasonFilter = new Lazy<string>(() => "@pRetentionReasonFilter");
    public static string pmRetentionReasonFilter => _pmRetentionReasonFilter.Value;

    private static readonly Lazy<string> _pmStartDate = new Lazy<string>(() => "@pStartDate");
    public static string pmStartDate => _pmStartDate.Value;

    private static readonly Lazy<string> _pmEndDate = new Lazy<string>(() => "@pEndDate");
    public static string pmEndDate => _pmEndDate.Value;

    private static readonly Lazy<string> _pmScheduledBy = new Lazy<string>(() => "@pScheduledBy");
    public static string pmScheduledBy => _pmScheduledBy.Value;

    private static readonly Lazy<string> _pmDeletedBy = new Lazy<string>(() => "@pDeletedBy");
    public static string pmDeletedBy => _pmDeletedBy.Value;

    private static readonly Lazy<string> _pmDeleteReason = new Lazy<string>(() => "@pDeleteReason");
    public static string pmDeleteReason => _pmDeleteReason.Value;

    #region HazardReportTracking Parameter Names

    /// <summary>
    /// HazardReportTracking table parameter names - Following established naming convention
    /// </summary>
    private static readonly Lazy<string> _pmHazardReportTrackingHazardCode = new Lazy<string>(() => "@pHazardCode");
    public static string pmHazardReportTrackingHazardCode => _pmHazardReportTrackingHazardCode.Value;

    private static readonly Lazy<string> _pmHazardReportTrackingReportCode = new Lazy<string>(() => "@pReportCode");
    public static string pmHazardReportTrackingReportCode => _pmHazardReportTrackingReportCode.Value;

    private static readonly Lazy<string> _pmHazardReportTrackingTrackingCode = new Lazy<string>(() => "@pTrackingCode");
    public static string pmHazardReportTrackingTrackingCode => _pmHazardReportTrackingTrackingCode.Value;

    #endregion
}
