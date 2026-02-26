//-----------------------------------------------------------------------
// <copyright file="WorkflowPermissions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Value object representing workflowpermissions with access control and authorization logic.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

//using SMS_Domain.Common;
//using SMS_Shared.Common;
//using SMS_Domain.Errors;
//using SMS_Domain.Enums;

//namespace SMS_Domain.ValueObjects;

///// <summary>
///// Workflow permissions for SMS Organizational Users
///// Controls approval authority, escalation rights, and committee participation
///// </summary>
//public sealed class WorkflowPermissions : BaseValueObject
//{
//    // Risk Management Permissions
//    public bool CanApproveLowRisk { get; }
//    public bool CanApproveMediumRisk { get; }
//    public bool CanApproveHighRisk { get; }
//    public bool CanApproveCriticalRisk { get; }

//    // Escalation Permissions
//    public bool CanEscalateWithinDepartment { get; }
//    public bool CanEscalateAcrossDepartments { get; }
//    public bool CanEscalateToExecutiveLevel { get; }

//    // Committee Permissions
//    public bool CanParticipateInCommittees { get; }
//    public bool CanChairCommittees { get; }
//    public bool CanCreateCommittees { get; }
//    public bool CanScheduleMeetings { get; }

//    // Investigation Permissions
//    public bool CanInitiateInvestigations { get; }
//    public bool CanLeadInvestigations { get; }
//    public bool CanCloseInvestigations { get; }

//    // Reporting Permissions
//    public bool CanViewDepartmentReports { get; }
//    public bool CanViewCrossDepartmentReports { get; }
//    public bool CanViewExecutiveReports { get; }

//    private WorkflowPermissions(
//        bool canApproveLowRisk, bool canApproveMediumRisk, bool canApproveHighRisk, bool canApproveCriticalRisk,
//        bool canEscalateWithinDepartment, bool canEscalateAcrossDepartments, bool canEscalateToExecutiveLevel,
//        bool canParticipateInCommittees, bool canChairCommittees, bool canCreateCommittees, bool canScheduleMeetings,
//        bool canInitiateInvestigations, bool canLeadInvestigations, bool canCloseInvestigations,
//        bool canViewDepartmentReports, bool canViewCrossDepartmentReports, bool canViewExecutiveReports)
//    {
//        CanApproveLowRisk = canApproveLowRisk;
//        CanApproveMediumRisk = canApproveMediumRisk;
//        CanApproveHighRisk = canApproveHighRisk;
//        CanApproveCriticalRisk = canApproveCriticalRisk;
//        CanEscalateWithinDepartment = canEscalateWithinDepartment;
//        CanEscalateAcrossDepartments = canEscalateAcrossDepartments;
//        CanEscalateToExecutiveLevel = canEscalateToExecutiveLevel;
//        CanParticipateInCommittees = canParticipateInCommittees;
//        CanChairCommittees = canChairCommittees;
//        CanCreateCommittees = canCreateCommittees;
//        CanScheduleMeetings = canScheduleMeetings;
//        CanInitiateInvestigations = canInitiateInvestigations;
//        CanLeadInvestigations = canLeadInvestigations;
//        CanCloseInvestigations = canCloseInvestigations;
//        CanViewDepartmentReports = canViewDepartmentReports;
//        CanViewCrossDepartmentReports = canViewCrossDepartmentReports;
//        CanViewExecutiveReports = canViewExecutiveReports;
//    }

//    /// <summary>
//    /// Creates workflow permissions based on organization level
//    /// </summary>
//    public static Result<WorkflowPermissions> Create(string organizationLevel) =>
//        Result.Create(organizationLevel, DomainErrors.SMSOrganizationalUserError.OrganizationLevelRequired)
//            .Ensure(o => !string.IsNullOrWhiteSpace(o), DomainErrors.SMSOrganizationalUserError.OrganizationLevelRequired)
//            .Ensure(o => IsValidOrganizationLevel(o), DomainErrors.SMSOrganizationalUserError.InvalidOrganizationLevel)
//            .Map(o => CreateFromLevel(o));

//    /// <summary>
//    /// Creates workflow permissions from individual permission flags
//    /// </summary>
//    public static WorkflowPermissions CreateCustom(
//        bool canApproveLowRisk = false, bool canApproveMediumRisk = false, bool canApproveHighRisk = false, bool canApproveCriticalRisk = false,
//        bool canEscalateWithinDepartment = true, bool canEscalateAcrossDepartments = false, bool canEscalateToExecutiveLevel = false,
//        bool canParticipateInCommittees = true, bool canChairCommittees = false, bool canCreateCommittees = false, bool canScheduleMeetings = false,
//        bool canInitiateInvestigations = false, bool canLeadInvestigations = false, bool canCloseInvestigations = false,
//        bool canViewDepartmentReports = true, bool canViewCrossDepartmentReports = false, bool canViewExecutiveReports = false)
//    {
//        return new WorkflowPermissions(
//            canApproveLowRisk, canApproveMediumRisk, canApproveHighRisk, canApproveCriticalRisk,
//            canEscalateWithinDepartment, canEscalateAcrossDepartments, canEscalateToExecutiveLevel,
//            canParticipateInCommittees, canChairCommittees, canCreateCommittees, canScheduleMeetings,
//            canInitiateInvestigations, canLeadInvestigations, canCloseInvestigations,
//            canViewDepartmentReports, canViewCrossDepartmentReports, canViewExecutiveReports);
//    }

//    private static bool IsValidOrganizationLevel(string level)
//    {
//        var validLevels = new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" };
//        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
//    }

//    private static WorkflowPermissions CreateFromLevel(string level)
//    {
//        return level.ToUpperInvariant() switch
//        {
//            "STAFF" => ForStaff,
//            "SUPERVISOR" => ForSupervisor,
//            "MANAGER" => ForManager,
//            "DIRECTOR" => ForDirector,
//            "EXECUTIVE" => ForExecutive,
//            _ => ForStaff
//        };
//    }

//    // Predefined permission sets based on organization level
//    public static WorkflowPermissions ForStaff => new(
//        canApproveLowRisk: false, canApproveMediumRisk: false, canApproveHighRisk: false, canApproveCriticalRisk: false,
//        canEscalateWithinDepartment: true, canEscalateAcrossDepartments: false, canEscalateToExecutiveLevel: false,
//        canParticipateInCommittees: true, canChairCommittees: false, canCreateCommittees: false, canScheduleMeetings: false,
//        canInitiateInvestigations: true, canLeadInvestigations: false, canCloseInvestigations: false,
//        canViewDepartmentReports: true, canViewCrossDepartmentReports: false, canViewExecutiveReports: false);

//    public static WorkflowPermissions ForSupervisor => new(
//        canApproveLowRisk: true, canApproveMediumRisk: false, canApproveHighRisk: false, canApproveCriticalRisk: false,
//        canEscalateWithinDepartment: true, canEscalateAcrossDepartments: true, canEscalateToExecutiveLevel: false,
//        canParticipateInCommittees: true, canChairCommittees: true, canCreateCommittees: false, canScheduleMeetings: true,
//        canInitiateInvestigations: true, canLeadInvestigations: true, canCloseInvestigations: false,
//        canViewDepartmentReports: true, canViewCrossDepartmentReports: true, canViewExecutiveReports: false);

//    public static WorkflowPermissions ForManager => new(
//        canApproveLowRisk: true, canApproveMediumRisk: true, canApproveHighRisk: false, canApproveCriticalRisk: false,
//        canEscalateWithinDepartment: true, canEscalateAcrossDepartments: true, canEscalateToExecutiveLevel: true,
//        canParticipateInCommittees: true, canChairCommittees: true, canCreateCommittees: true, canScheduleMeetings: true,
//        canInitiateInvestigations: true, canLeadInvestigations: true, canCloseInvestigations: true,
//        canViewDepartmentReports: true, canViewCrossDepartmentReports: true, canViewExecutiveReports: false);

//    public static WorkflowPermissions ForDirector => new(
//        canApproveLowRisk: true, canApproveMediumRisk: true, canApproveHighRisk: true, canApproveCriticalRisk: false,
//        canEscalateWithinDepartment: true, canEscalateAcrossDepartments: true, canEscalateToExecutiveLevel: true,
//        canParticipateInCommittees: true, canChairCommittees: true, canCreateCommittees: true, canScheduleMeetings: true,
//        canInitiateInvestigations: true, canLeadInvestigations: true, canCloseInvestigations: true,
//        canViewDepartmentReports: true, canViewCrossDepartmentReports: true, canViewExecutiveReports: true);

//    public static WorkflowPermissions ForExecutive => new(
//        canApproveLowRisk: true, canApproveMediumRisk: true, canApproveHighRisk: true, canApproveCriticalRisk: true,
//        canEscalateWithinDepartment: true, canEscalateAcrossDepartments: true, canEscalateToExecutiveLevel: true,
//        canParticipateInCommittees: true, canChairCommittees: true, canCreateCommittees: true, canScheduleMeetings: true,
//        canInitiateInvestigations: true, canLeadInvestigations: true, canCloseInvestigations: true,
//        canViewDepartmentReports: true, canViewCrossDepartmentReports: true, canViewExecutiveReports: true);

//    /// <summary>
//    /// Checks if a specific workflow permission is granted
//    /// </summary>
//    public bool HasPermission(string permission)
//    {
//        return permission.ToUpperInvariant() switch
//        {
//            "APPROVE_LOW_RISK" => CanApproveLowRisk,
//            "APPROVE_MEDIUM_RISK" => CanApproveMediumRisk,
//            "APPROVE_HIGH_RISK" => CanApproveHighRisk,
//            "APPROVE_CRITICAL_RISK" => CanApproveCriticalRisk,
//            "ESCALATE_WITHIN_DEPARTMENT" => CanEscalateWithinDepartment,
//            "ESCALATE_ACROSS_DEPARTMENTS" => CanEscalateAcrossDepartments,
//            "ESCALATE_TO_EXECUTIVE" => CanEscalateToExecutiveLevel,
//            "PARTICIPATE_IN_COMMITTEES" => CanParticipateInCommittees,
//            "CHAIR_COMMITTEES" => CanChairCommittees,
//            "CREATE_COMMITTEES" => CanCreateCommittees,
//            "SCHEDULE_MEETINGS" => CanScheduleMeetings,
//            "INITIATE_INVESTIGATIONS" => CanInitiateInvestigations,
//            "LEAD_INVESTIGATIONS" => CanLeadInvestigations,
//            "CLOSE_INVESTIGATIONS" => CanCloseInvestigations,
//            "VIEW_DEPARTMENT_REPORTS" => CanViewDepartmentReports,
//            "VIEW_CROSS_DEPARTMENT_REPORTS" => CanViewCrossDepartmentReports,
//            "VIEW_EXECUTIVE_REPORTS" => CanViewExecutiveReports,
//            _ => false
//        };
//    }

//    /// <summary>
//    /// Checks if user can approve risk at the specified level
//    /// </summary>
//    public bool CanApproveRiskLevel(RiskLevel riskLevel)
//    {
//        return riskLevel.Value.ToUpperInvariant() switch
//        {
//            "LOW" => CanApproveLowRisk,
//            "MEDIUM" => CanApproveMediumRisk,
//            "HIGH" => CanApproveHighRisk,
//            "CRITICAL" => CanApproveCriticalRisk,
//            _ => false
//        };
//    }

//    /// <summary>
//    /// Gets the organization level string representation
//    /// </summary>
//    public string GetOrganizationLevel()
//    {
//        if (CanApproveCriticalRisk) return "Executive";
//        if (CanApproveHighRisk) return "Director";
//        if (CanApproveMediumRisk) return "Manager";
//        if (CanApproveLowRisk) return "Supervisor";
//        return "Staff";
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return CanApproveLowRisk;
//        yield return CanApproveMediumRisk;
//        yield return CanApproveHighRisk;
//        yield return CanApproveCriticalRisk;
//        yield return CanEscalateWithinDepartment;
//        yield return CanEscalateAcrossDepartments;
//        yield return CanEscalateToExecutiveLevel;
//        yield return CanParticipateInCommittees;
//        yield return CanChairCommittees;
//        yield return CanCreateCommittees;
//        yield return CanScheduleMeetings;
//        yield return CanInitiateInvestigations;
//        yield return CanLeadInvestigations;
//        yield return CanCloseInvestigations;
//        yield return CanViewDepartmentReports;
//        yield return CanViewCrossDepartmentReports;
//        yield return CanViewExecutiveReports;
//    }

//    public override string ToString() => $"WorkflowPermissions({GetOrganizationLevel()})";
//}
