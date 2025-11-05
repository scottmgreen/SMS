namespace SMS_Domain.Errors;

/// <summary>
/// Contains the domain errors for Safety Management System (SMS).
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// Contains system-level errors.
    /// </summary>
    public static class SystemError
    {
        public static Error AuditLogEntryError => new Error("SystemError.AuditLogEntryError", "Audit Log Entry Error.");
        public static Error FileExistsError => new Error("SystemError.FileExistsError", "File Does Not Exist Error.");
        // public static Error VideoPlaybackError => new Error("SystemError.VideoPlaybackError", "Video Playback Error.");
        // public static Error LessonQuizError => new Error("SystemError.LessonQuizError", "Lesson Quiz Error.");
        // public static Error CustomPageError => new Error("SystemError.CustomPageError", "Custom Page Error.");
    }

    /// <summary>
    /// Contains base user-related errors.
    /// </summary>
    public static class BaseUserError
    {
        public static Error NullOrEmpty => new Error("BaseUser.NullOrEmpty", "The user is required.");
        public static Error UserNotFound => new Error("BaseUser.UserNotFound", "The user was not found.");
        public static Error InvalidUserType => new Error("BaseUser.InvalidUserType", "The user type is invalid.");
        public static Error CreateFailed => new Error("BaseUser.CreateFailed", "Failed to create the user.");
        public static Error UpdateFailed => new Error("BaseUser.UpdateFailed", "Failed to update the user.");
        public static Error DeleteFailed => new Error("BaseUser.DeleteFailed", "Failed to delete the user.");
        public static Error UserAlreadyExists => new Error("BaseUser.UserAlreadyExists", "A user with this identifier already exists.");
        public static Error InvalidOperation => new Error("BaseUser.InvalidOperation", "The operation is not valid for this user.");
        public static Error InactiveUser => new Error("BaseUser.InactiveUser", "The user account is inactive.");
        public static Error LockedUser => new Error("BaseUser.LockedUser", "The user account is locked.");
    }

    /// <summary>
    /// Contains user name errors.
    /// </summary>
    public static class UserNameError
    {
        public static Error NullOrEmpty => new Error("UserName.NullOrEmpty", "The user name is required.");
        public static Error TooShort => new Error("UserName.TooShort", "The user name must be at least 3 characters long.");
        public static Error TooLong => new Error("UserName.TooLong", "The user name cannot exceed 50 characters.");
        public static Error InvalidFormat => new Error("UserName.InvalidFormat", "The user name contains invalid characters. Only letters, numbers, dots, hyphens, underscores, and @ symbols are allowed.");
        public static Error AlreadyExists => new Error("UserName.AlreadyExists", "A user with this username already exists.");
        public static Error Reserved => new Error("UserName.Reserved", "This username is reserved and cannot be used.");
    }

    /// <summary>
    /// Contains password errors.
    /// </summary>
    public static class PasswordError
    {
        public static Error NullOrEmpty => new Error("Password.NullOrEmpty", "The password is required.");
        public static Error TooShort => new Error("Password.TooShort", "The password must be at least 8 characters long.");
        public static Error TooLong => new Error("Password.TooLong", "The password cannot exceed 128 characters.");
        public static Error MissingUpperCase => new Error("Password.MissingUpperCase", "The password must contain at least one uppercase letter.");
        public static Error MissingLowerCase => new Error("Password.MissingLowerCase", "The password must contain at least one lowercase letter.");
        public static Error MissingDigit => new Error("Password.MissingDigit", "The password must contain at least one digit.");
        public static Error MissingSpecialChar => new Error("Password.MissingSpecialChar", "The password must contain at least one special character.");
        public static Error InvalidHash => new Error("Password.InvalidHash", "The password hash is invalid.");
        public static Error VerificationFailed => new Error("Password.VerificationFailed", "Password verification failed.");
        public static Error Expired => new Error("Password.Expired", "The password has expired and must be changed.");
        public static Error RecentlyUsed => new Error("Password.RecentlyUsed", "This password has been used recently and cannot be reused.");
    }

    /// <summary>
    /// Contains SMS application user errors.
    /// </summary>
    public static class SMSApplicationUserError
    {
        public static Error NullOrEmpty => new Error("SMSApplicationUser.NullOrEmpty", "The SMS Application User is required.");
        public static Error CodeRequired => new Error("SMSApplicationUser.CodeRequired", "The SMS Application User Code is required.");
        public static Error UserNameRequired => new Error("SMSApplicationUser.UserNameRequired", "The Username is required.");
        public static Error PasswordRequired => new Error("SMSApplicationUser.PasswordRequired", "The Password is required.");
        public static Error ApplicationRoleRequired => new Error("SMSApplicationUser.ApplicationRoleRequired", "The Application Role is required.");
        public static Error PermissionLevelRequired => new Error("SMSApplicationUser.PermissionLevelRequired", "The Permission Level is required.");
        public static Error InvalidCode => new Error("SMSApplicationUser.InvalidCode", "The SMS Application User Code is invalid.");
        public static Error InvalidApplicationRole => new Error("SMSApplicationUser.InvalidApplicationRole", "The Application Role is invalid.");
        public static Error InvalidPermissionLevel => new Error("SMSApplicationUser.InvalidPermissionLevel", "The Permission Level is invalid.");
        public static Error NotFound => new Error("SMSApplicationUser.NotFound", "The SMS Application User was not found.");
        public static Error CreateFailed => new Error("SMSApplicationUser.CreateFailed", "Failed to create the SMS Application User.");
        public static Error UpdateFailed => new Error("SMSApplicationUser.UpdateFailed", "Failed to update the SMS Application User.");
        public static Error DeleteFailed => new Error("SMSApplicationUser.DeleteFailed", "Failed to delete the SMS Application User.");
        public static Error LoginFailed => new Error("SMSApplicationUser.LoginFailed", "Login failed for SMS Application User.");
        public static Error PasswordUpdateFailed => new Error("SMSApplicationUser.PasswordUpdateFailed", "Failed to update SMS Application User password.");
    }

    /// <summary>
    /// Contains SMS organizational user errors.
    /// </summary>
    public static class SMSOrganizationalUserError
    {
        public static Error NullOrEmpty => new Error("SMSOrganizationalUser.NullOrEmpty", "The SMS Organizational User is required.");
        public static Error CodeRequired => new Error("SMSOrganizationalUser.CodeRequired", "The SMS Organizational User Code is required.");
        public static Error UserNameRequired => new Error("SMSOrganizationalUser.UserNameRequired", "The Username is required.");
        public static Error PasswordRequired => new Error("SMSOrganizationalUser.PasswordRequired", "The Password is required.");
        public static Error DepartmentRequired => new Error("SMSOrganizationalUser.DepartmentRequired", "The Department is required.");
        public static Error PositionRequired => new Error("SMSOrganizationalUser.PositionRequired", "The Position is required.");
        public static Error OrganizationLevelRequired => new Error("SMSOrganizationalUser.OrganizationLevelRequired", "The Organization Level is required.");
        public static Error InvalidCode => new Error("SMSOrganizationalUser.InvalidCode", "The SMS Organizational User Code is invalid.");
        public static Error InvalidDepartment => new Error("SMSOrganizationalUser.InvalidDepartment", "The Department is invalid.");
        public static Error InvalidPosition => new Error("SMSOrganizationalUser.InvalidPosition", "The Position is invalid.");
        public static Error InvalidOrganizationLevel => new Error("SMSOrganizationalUser.InvalidOrganizationLevel", "The Organization Level is invalid.");
        public static Error NotFound => new Error("SMSOrganizationalUser.NotFound", "The SMS Organizational User was not found.");
        public static Error CreateFailed => new Error("SMSOrganizationalUser.CreateFailed", "Failed to create the SMS Organizational User.");
        public static Error UpdateFailed => new Error("SMSOrganizationalUser.UpdateFailed", "Failed to update the SMS Organizational User.");
        public static Error DeleteFailed => new Error("SMSOrganizationalUser.DeleteFailed", "Failed to delete the SMS Organizational User.");
        public static Error LoginFailed => new Error("SMSOrganizationalUser.LoginFailed", "Login failed for SMS Organizational User.");
        public static Error PasswordUpdateFailed => new Error("SMSOrganizationalUser.PasswordUpdateFailed", "Failed to update SMS Organizational User password.");
    }

    /// <summary>
    /// Contains SMS stakeholder user errors.
    /// </summary>
    public static class SMSStakeholderUserError
    {
        public static Error NullOrEmpty => new Error("SMSStakeholderUser.NullOrEmpty", "The SMS Stakeholder User is required.");
        public static Error CodeRequired => new Error("SMSStakeholderUser.CodeRequired", "The SMS Stakeholder User Code is required.");
        public static Error UserNameRequired => new Error("SMSStakeholderUser.UserNameRequired", "The Username is required.");
        public static Error PasswordRequired => new Error("SMSStakeholderUser.PasswordRequired", "The Password is required.");
        public static Error StakeholderTypeRequired => new Error("SMSStakeholderUser.StakeholderTypeRequired", "The Stakeholder Type is required.");
        public static Error OrganizationRequired => new Error("SMSStakeholderUser.OrganizationRequired", "The Organization is required.");
        public static Error AccessLevelRequired => new Error("SMSStakeholderUser.AccessLevelRequired", "The Access Level is required.");
        public static Error InvalidCode => new Error("SMSStakeholderUser.InvalidCode", "The SMS Stakeholder User Code is invalid.");
        public static Error InvalidStakeholderType => new Error("SMSStakeholderUser.InvalidStakeholderType", "The Stakeholder Type is invalid.");
        public static Error InvalidOrganization => new Error("SMSStakeholderUser.InvalidOrganization", "The Organization is invalid.");
        public static Error InvalidAccessLevel => new Error("SMSStakeholderUser.InvalidAccessLevel", "The Access Level is invalid.");
        public static Error NotFound => new Error("SMSStakeholderUser.NotFound", "The SMS Stakeholder User was not found.");
        public static Error CreateFailed => new Error("SMSStakeholderUser.CreateFailed", "Failed to create the SMS Stakeholder User.");
        public static Error UpdateFailed => new Error("SMSStakeholderUser.UpdateFailed", "Failed to update the SMS Stakeholder User.");
        public static Error DeleteFailed => new Error("SMSStakeholderUser.DeleteFailed", "Failed to delete the SMS Stakeholder User.");
        public static Error LoginFailed => new Error("SMSStakeholderUser.LoginFailed", "Login failed for SMS Stakeholder User.");
        public static Error PasswordUpdateFailed => new Error("SMSStakeholderUser.PasswordUpdateFailed", "Failed to update SMS Stakeholder User password.");
    }

    /// <summary>
    /// Contains SMS role-related errors.
    /// </summary>
    public static class SMSRoleError
    {
        public static Error NullOrEmpty => new Error("SMSRole.NullOrEmpty", "The SMS Role is required.");
        public static Error InvalidRole => new Error("SMSRole.InvalidRole", "The SMS Role is not valid.");
        public static Error RoleNotFound => new Error("SMSRole.RoleNotFound", "The specified SMS Role was not found.");
        public static Error InsufficientAuthority => new Error("SMSRole.InsufficientAuthority", "The user does not have sufficient authority for this operation.");
        public static Error RoleAssignmentFailed => new Error("SMSRole.RoleAssignmentFailed", "Failed to assign the SMS Role to the user.");
        public static Error RoleRemovalFailed => new Error("SMSRole.RoleRemovalFailed", "Failed to remove the SMS Role from the user.");
        public static Error DuplicateRoleAssignment => new Error("SMSRole.DuplicateRoleAssignment", "The user already has this SMS Role assigned.");
        public static Error ExpiredRoleAssignment => new Error("SMSRole.ExpiredRoleAssignment", "The SMS Role assignment has expired.");
        public static Error InactiveRoleAssignment => new Error("SMSRole.InactiveRoleAssignment", "The SMS Role assignment is inactive.");
        public static Error InvalidAuthorityLevel => new Error("SMSRole.InvalidAuthorityLevel", "The authority level is invalid.");
        public static Error InvalidRoleCategory => new Error("SMSRole.InvalidRoleCategory", "The role category is invalid.");
        public static Error BulkAssignmentFailed => new Error("SMSRole.BulkAssignmentFailed", "Failed to complete bulk role assignment.");
        public static Error ValidationFailed => new Error("SMSRole.ValidationFailed", "SMS Role validation failed.");
    }

    /// <summary>
    /// Contains SMS user role assignment errors.
    /// </summary>
    public static class SMSUserRoleError
    {
        public static Error NullOrEmpty => new Error("SMSUserRole.NullOrEmpty", "The SMS User Role assignment is required.");
        public static Error UserIDRequired => new Error("SMSUserRole.UserIDRequired", "The User ID is required.");
        public static Error UserTypeRequired => new Error("SMSUserRole.UserTypeRequired", "The User Type is required.");
        public static Error DepartmentRequired => new Error("SMSUserRole.DepartmentRequired", "The Department is required.");
        public static Error AssignedByRequired => new Error("SMSUserRole.AssignedByRequired", "The AssignedBy field is required.");
        public static Error InvalidUserType => new Error("SMSUserRole.InvalidUserType", "The User Type is invalid.");
        public static Error InvalidDepartment => new Error("SMSUserRole.InvalidDepartment", "The Department is invalid.");
        public static Error InvalidEffectiveDate => new Error("SMSUserRole.InvalidEffectiveDate", "The Effective Date is invalid.");
        public static Error InvalidExpirationDate => new Error("SMSUserRole.InvalidExpirationDate", "The Expiration Date is invalid.");
        public static Error NotFound => new Error("SMSUserRole.NotFound", "The SMS User Role assignment was not found.");
        public static Error CreateFailed => new Error("SMSUserRole.CreateFailed", "Failed to create the SMS User Role assignment.");
        public static Error UpdateFailed => new Error("SMSUserRole.UpdateFailed", "Failed to update the SMS User Role assignment.");
        public static Error DeactivationFailed => new Error("SMSUserRole.DeactivationFailed", "Failed to deactivate the SMS User Role assignment.");
        public static Error ReactivationFailed => new Error("SMSUserRole.ReactivationFailed", "Failed to reactivate the SMS User Role assignment.");
        public static Error ExtensionFailed => new Error("SMSUserRole.ExtensionFailed", "Failed to extend the SMS User Role assignment.");
        public static Error AlreadyDeactivated => new Error("SMSUserRole.AlreadyDeactivated", "The SMS User Role assignment is already deactivated.");
        public static Error AlreadyActive => new Error("SMSUserRole.AlreadyActive", "The SMS User Role assignment is already active.");
    }

    /// <summary>
    /// Contains SMS department-related errors.
    /// </summary>
    public static class SMSDepartmentError
    {
        public static Error NullOrEmpty => new Error("SMSDepartment.NullOrEmpty", "The SMS Department is required.");
        public static Error InvalidDepartment => new Error("SMSDepartment.InvalidDepartment", "The SMS Department is not valid.");
        public static Error DepartmentNotFound => new Error("SMSDepartment.DepartmentNotFound", "The specified SMS Department was not found.");
        public static Error ResponsibilityNotFound => new Error("SMSDepartment.ResponsibilityNotFound", "The specified responsibility is not assigned to this department.");
    }

    /// <summary>
    /// Contains committee-related errors.
    /// </summary>
    public static class CommitteeError
    {
        public static Error NullOrEmpty => new Error("Committee.NullOrEmpty", "The Committee is required.");
        public static Error InvalidUserId => new Error("Committee.InvalidUserId", "The User ID is invalid.");
        public static Error InvalidMembershipType => new Error("Committee.InvalidMembershipType", "The Membership Type is invalid.");
        public static Error UserAlreadyMember => new Error("Committee.UserAlreadyMember", "The user is already a member of this committee.");
        public static Error MaxMembersExceeded => new Error("Committee.MaxMembersExceeded", "The committee has reached its maximum member limit.");
        public static Error MemberNotFound => new Error("Committee.MemberNotFound", "The committee member was not found.");
        public static Error MembershipCreationFailed => new Error("Committee.MembershipCreationFailed", "Failed to create committee membership.");
        public static Error MemberRemovalFailed => new Error("Committee.MemberRemovalFailed", "Failed to remove committee member.");
        public static Error MembershipUpdateFailed => new Error("Committee.MembershipUpdateFailed", "Failed to update committee membership.");
        public static Error MembershipTypeDoesNotSupportVoting => new Error("Committee.MembershipTypeDoesNotSupportVoting", "This membership type does not support voting rights.");
        public static Error VotingRightsUpdateFailed => new Error("Committee.VotingRightsUpdateFailed", "Failed to update voting rights.");
        public static Error InvalidExpirationDate => new Error("Committee.InvalidExpirationDate", "The expiration date must be in the future.");
        public static Error ExpirationUpdateFailed => new Error("Committee.ExpirationUpdateFailed", "Failed to update expiration date.");
        public static Error MembershipExtensionFailed => new Error("Committee.MembershipExtensionFailed", "Failed to extend membership.");
        public static Error CannotReactivateExpiredMembership => new Error("Committee.CannotReactivateExpiredMembership", "Cannot reactivate an expired membership.");
        public static Error MembershipReactivationFailed => new Error("Committee.MembershipReactivationFailed", "Failed to reactivate membership.");
    }

    /// <summary>
    /// Contains meeting-related errors.
    /// </summary>
    public static class MeetingError
    {
        public static Error NullOrEmpty => new Error("Meeting.NullOrEmpty", "The Meeting is required.");
        public static Error InvalidMeetingDate => new Error("Meeting.InvalidMeetingDate", "The meeting date must be in the future.");
        public static Error InvalidFacilitator => new Error("Meeting.InvalidFacilitator", "The facilitator ID is invalid.");
        public static Error MeetingCreationFailed => new Error("Meeting.MeetingCreationFailed", "Failed to create the meeting.");
        public static Error MeetingUpdateFailed => new Error("Meeting.MeetingUpdateFailed", "Failed to update the meeting.");
        public static Error CannotModifyCompletedMeeting => new Error("Meeting.CannotModifyCompletedMeeting", "Cannot modify a completed meeting.");
        public static Error InvalidAgenda => new Error("Meeting.InvalidAgenda", "The agenda cannot be empty.");
        public static Error AgendaUpdateFailed => new Error("Meeting.AgendaUpdateFailed", "Failed to update the agenda.");
        public static Error InvalidMinutes => new Error("Meeting.InvalidMinutes", "The minutes cannot be empty.");
        public static Error CannotSetMinutesForNonActiveMeeting => new Error("Meeting.CannotSetMinutesForNonActiveMeeting", "Cannot set minutes for a non-active meeting.");
        public static Error MinutesUpdateFailed => new Error("Meeting.MinutesUpdateFailed", "Failed to update the minutes.");
        public static Error MinutesNotPendingApproval => new Error("Meeting.MinutesNotPendingApproval", "The minutes are not pending approval.");
        public static Error NoMinutesToApprove => new Error("Meeting.NoMinutesToApprove", "There are no minutes to approve.");
        public static Error MinutesApprovalFailed => new Error("Meeting.MinutesApprovalFailed", "Failed to approve the minutes.");
        public static Error CannotStartNonScheduledMeeting => new Error("Meeting.CannotStartNonScheduledMeeting", "Cannot start a non-scheduled meeting.");
        public static Error MeetingStartFailed => new Error("Meeting.MeetingStartFailed", "Failed to start the meeting.");
        public static Error CannotEndNonActiveMeeting => new Error("Meeting.CannotEndNonActiveMeeting", "Cannot end a non-active meeting.");
        public static Error MeetingEndFailed => new Error("Meeting.MeetingEndFailed", "Failed to end the meeting.");
        public static Error CannotCancelCompletedMeeting => new Error("Meeting.CannotCancelCompletedMeeting", "Cannot cancel a completed meeting.");
        public static Error MeetingCancellationFailed => new Error("Meeting.MeetingCancellationFailed", "Failed to cancel the meeting.");
        public static Error CannotPostponeCompletedMeeting => new Error("Meeting.CannotPostponeCompletedMeeting", "Cannot postpone a completed meeting.");
        public static Error MeetingPostponeFailed => new Error("Meeting.MeetingPostponeFailed", "Failed to postpone the meeting.");
        public static Error InvalidUserId => new Error("Meeting.InvalidUserId", "The user ID is invalid.");
        public static Error AttendeeAlreadyExists => new Error("Meeting.AttendeeAlreadyExists", "The attendee already exists for this meeting.");
        public static Error AttendeeAddFailed => new Error("Meeting.AttendeeAddFailed", "Failed to add attendee to the meeting.");
        public static Error InvalidAgendaItemTitle => new Error("Meeting.InvalidAgendaItemTitle", "The agenda item title cannot be empty.");
        public static Error AgendaItemAddFailed => new Error("Meeting.AgendaItemAddFailed", "Failed to add agenda item to the meeting.");
    }

    /// <summary>
    /// Contains approval workflow errors.
    /// </summary>
    public static class ApprovalError
    {
        public static Error CannotApproveNonPendingRequest => new Error("Approval.CannotApproveNonPendingRequest", "Cannot approve a non-pending request.");
        public static Error InvalidApprover => new Error("Approval.InvalidApprover", "The approver ID is invalid.");
        public static Error ApprovalProcessFailed => new Error("Approval.ApprovalProcessFailed", "The approval process failed.");
        public static Error CannotRejectNonPendingRequest => new Error("Approval.CannotRejectNonPendingRequest", "Cannot reject a non-pending request.");
        public static Error InvalidRejecter => new Error("Approval.InvalidRejecter", "The rejecter ID is invalid.");
        public static Error RejectionReasonRequired => new Error("Approval.RejectionReasonRequired", "A rejection reason is required.");
        public static Error RejectionProcessFailed => new Error("Approval.RejectionProcessFailed", "The rejection process failed.");
        public static Error CannotEscalateCompletedRequest => new Error("Approval.CannotEscalateCompletedRequest", "Cannot escalate a completed request.");
        public static Error InvalidEscalationTarget => new Error("Approval.InvalidEscalationTarget", "The escalation target ID is invalid.");
        public static Error EscalationReasonRequired => new Error("Approval.EscalationReasonRequired", "An escalation reason is required.");
        public static Error EscalationProcessFailed => new Error("Approval.EscalationProcessFailed", "The escalation process failed.");
        public static Error CannotStartReviewOnNonPendingRequest => new Error("Approval.CannotStartReviewOnNonPendingRequest", "Cannot start review on a non-pending request.");
        public static Error ReviewStartFailed => new Error("Approval.ReviewStartFailed", "Failed to start the review process.");
        public static Error CannotExpireCompletedRequest => new Error("Approval.CannotExpireCompletedRequest", "Cannot expire a completed request.");
        public static Error ExpirationProcessFailed => new Error("Approval.ExpirationProcessFailed", "The expiration process failed.");
    }

    /// <summary>
    /// Contains authorization and workflow errors.
    /// </summary>
    public static class WorkflowError
    {
        public static Error InvalidRiskLevel => new Error("Workflow.InvalidRiskLevel", "The risk level is invalid.");
        public static Error NoApproverFound => new Error("Workflow.NoApproverFound", "No approver found for the specified authority level.");
        public static Error ApprovalRequired => new Error("Workflow.ApprovalRequired", "Approval is required for this operation.");
        public static Error UnauthorizedOperation => new Error("Workflow.UnauthorizedOperation", "User is not authorized to perform this operation.");
        public static Error EscalationRequired => new Error("Workflow.EscalationRequired", "This item requires escalation to a higher authority level.");
        public static Error WorkflowViolation => new Error("Workflow.WorkflowViolation", "The operation violates the established workflow rules.");
    }

    /// <summary>
    /// Contains airport shared dataset-related errors.
    /// </summary>
    public static class AirportSharedDatasetError
    {
        public static Error NullOrEmpty => new Error("AirportSharedDataset.NullOrEmpty", "The Airport Shared Dataset is required.");
        public static Error CodeRequired => new Error("AirportSharedDataset.CodeRequired", "The Airport Shared Dataset Code is required.");
        public static Error ReportIDRequired => new Error("AirportSharedDataset.ReportIDRequired", "The Report ID is required - cannot create dataset without parent report.");
        public static Error HazardCodeRequired => new Error("AirportSharedDataset.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidCode => new Error("AirportSharedDataset.InvalidCode", "The Airport Shared Dataset Code is invalid.");
        public static Error InvalidReportID => new Error("AirportSharedDataset.InvalidReportID", "The Report ID is invalid.");
        public static Error InvalidNarrative => new Error("AirportSharedDataset.InvalidNarrative", "The narrative exceeds maximum length.");
        public static Error NotFound => new Error("AirportSharedDataset.NotFound", "The Airport Shared Dataset was not found.");
        public static Error CreateFailed => new Error("AirportSharedDataset.CreateFailed", "Failed to create the Airport Shared Dataset.");
        public static Error UpdateFailed => new Error("AirportSharedDataset.UpdateFailed", "Failed to update the Airport Shared Dataset.");
        public static Error DeleteFailed => new Error("AirportSharedDataset.DeleteFailed", "Failed to delete the Airport Shared Dataset.");
    }
    /// <summary>
    /// Contains hazard-related errors.
    /// </summary>
    public static class HazardError
    {
        public static Error NullOrEmpty => new Error("Hazard.NullOrEmpty", "The Hazard is required.");
        public static Error CodeRequired => new Error("Hazard.CodeRequired", "The Hazard Code is required.");
        public static Error ReportCodeRequired => new Error("Hazard.ReportCodeRequired", "The Report Code is required.");
        public static Error InvalidCode => new Error("Hazard.InvalidCode", "The Hazard Code is invalid.");
        public static Error NotFound => new Error("Hazard.NotFound", "The Hazard was not found.");
        public static Error CreateFailed => new Error("Hazard.CreateFailed", "Failed to create the Hazard.");
        public static Error UpdateFailed => new Error("Hazard.UpdateFailed", "Failed to update the Hazard.");
        public static Error DeleteFailed => new Error("Hazard.DeleteFailed", "Failed to delete the Hazard.");
    }

    /// <summary>
    /// Contains report-related errors.
    /// </summary>
    public static class ReportError
    {
        public static Error NullOrEmpty => new Error("Report.NullOrEmpty", "The Report is required.");
        public static Error CodeRequired => new Error("Report.CodeRequired", "The Report Code is required.");
        public static Error InvalidStatus => new Error("Report.InvalidStatus", "The Report Status is invalid.");
        public static Error InvalidStage => new Error("Report.InvalidStage", "The Report Stage is invalid.");
        public static Error NotFound => new Error("Report.NotFound", "The Report was not found.");
        public static Error CreateFailed => new Error("Report.CreateFailed", "Failed to create the Report.");
        public static Error UpdateFailed => new Error("Report.UpdateFailed", "Failed to update the Report.");
        public static Error DeleteFailed => new Error("Report.DeleteFailed", "Failed to delete the Report.");
    }

    /// <summary>
    /// Contains investigation-related errors.
    /// </summary>
    public static class InvestigationError
    {
        public static Error NullOrEmpty => new Error("Investigation.NullOrEmpty", "The Investigation is required.");
        public static Error ReportCodeRequired => new Error("Investigation.ReportCodeRequired", "The Report Code is required.");
        public static Error NotFound => new Error("Investigation.NotFound", "The Investigation was not found.");
        public static Error InvalidCode => new Error("Investigation.InvalidCode", "The Investigation Code is invalid.");
        public static Error CreateFailed => new Error("Investigation.CreateFailed", "Failed to create the Investigation.");
        public static Error UpdateFailed => new Error("Investigation.UpdateFailed", "Failed to update the Investigation.");
        public static Error DeleteFailed => new Error("Investigation.DeleteFailed", "Failed to delete the Investigation.");
    }

    /// <summary>
    /// Contains interview-related errors.
    /// </summary>
    public static class InterviewError
    {
        public static Error NullOrEmpty => new Error("Interview.NullOrEmpty", "The Interview is required.");
        public static Error InvestigationCodeRequired => new Error("Interview.InvestigationCodeRequired", "The Investigation Code is required.");
        public static Error PersonInterviewedRequired => new Error("Interview.PersonInterviewedRequired", "The Person Interviewed is required.");
        public static Error NotFound => new Error("Interview.NotFound", "The Interview was not found.");
        public static Error CreateFailed => new Error("Interview.CreateFailed", "Failed to create the Interview.");
        public static Error UpdateFailed => new Error("Interview.UpdateFailed", "Failed to update the Interview.");
        public static Error DeleteFailed => new Error("Interview.DeleteFailed", "Failed to delete the Interview.");
    }

    /// <summary>
    /// Contains risk analysis-related errors.
    /// </summary>
    public static class RiskAnalysisError
    {
        public static Error NullOrEmpty => new Error("RiskAnalysis.NullOrEmpty", "The Risk Analysis is required.");
        public static Error HazardCodeRequired => new Error("RiskAnalysis.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidStatus => new Error("RiskAnalysis.InvalidStatus", "The Risk Analysis Status is invalid.");
        public static Error InvalidStage => new Error("RiskAnalysis.InvalidStage", "The Risk Analysis Stage is invalid.");
        public static Error NotFound => new Error("RiskAnalysis.NotFound", "The Risk Analysis was not found.");
        public static Error CreateFailed => new Error("RiskAnalysis.CreateFailed", "Failed to create the Risk Analysis.");
        public static Error UpdateFailed => new Error("RiskAnalysis.UpdateFailed", "Failed to update the Risk Analysis.");
        public static Error DeleteFailed => new Error("RiskAnalysis.DeleteFailed", "Failed to delete the Risk Analysis.");
    }

    /// <summary>
    /// Contains risk assessment-related errors.
    /// </summary>
    public static class RiskAssessmentError
    {
        public static Error NullOrEmpty => new Error("RiskAssessment.NullOrEmpty", "The Risk Assessment is required.");
        public static Error HazardCodeRequired => new Error("RiskAssessment.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidAssessmentType => new Error("RiskAssessment.InvalidAssessmentType", "The Assessment Type is invalid.");
        public static Error InvalidStatus => new Error("RiskAssessment.InvalidStatus", "The Risk Assessment Status is invalid.");
        public static Error NotFound => new Error("RiskAssessment.NotFound", "The Risk Assessment was not found.");
        public static Error CreateFailed => new Error("RiskAssessment.CreateFailed", "Failed to create the Risk Assessment.");
        public static Error UpdateFailed => new Error("RiskAssessment.UpdateFailed", "Failed to update the Risk Assessment.");
        public static Error DeleteFailed => new Error("RiskAssessment.DeleteFailed", "Failed to delete the Risk Assessment.");
    }

    /// <summary>
    /// Contains mitigation-related errors.
    /// </summary>
    public static class MitigationError
    {
        public static Error NullOrEmpty => new Error("Mitigation.NullOrEmpty", "The Mitigation is required.");
        public static Error HazardCodeRequired => new Error("Mitigation.HazardCodeRequired", "The Hazard Code is required.");
        public static Error NotFound => new Error("Mitigation.NotFound", "The Mitigation was not found.");
        public static Error InvalidCode => new Error("Mitigation.InvalidCode", "The Mitigation Code is invalid.");
        public static Error CreateFailed => new Error("Mitigation.CreateFailed", "Failed to create the Mitigation.");
        public static Error UpdateFailed => new Error("Mitigation.UpdateFailed", "Failed to update the Mitigation.");
        public static Error DeleteFailed => new Error("Mitigation.DeleteFailed", "Failed to delete the Mitigation.");
    }

    /// <summary>
    /// Contains mitigation assignment-related errors.
    /// </summary>
    public static class MitigationAssignmentError
    {
        public static Error NullOrEmpty => new Error("MitigationAssignment.NullOrEmpty", "The Mitigation Assignment is required.");
        public static Error MitigationCodeRequired => new Error("MitigationAssignment.MitigationCodeRequired", "The Mitigation Code is required.");
        public static Error DepartmentCodeRequired => new Error("MitigationAssignment.DepartmentCodeRequired", "The Department Code is required.");
        public static Error NotFound => new Error("MitigationAssignment.NotFound", "The Mitigation Assignment was not found.");
        public static Error CreateFailed => new Error("MitigationAssignment.CreateFailed", "Failed to create the Mitigation Assignment.");
        public static Error UpdateFailed => new Error("MitigationAssignment.UpdateFailed", "Failed to update the Mitigation Assignment.");
        public static Error DeleteFailed => new Error("MitigationAssignment.DeleteFailed", "Failed to delete the Mitigation Assignment.");
    }

    /// <summary>
    /// Contains scoring panel-related errors.
    /// </summary>
    public static class ScoringPanelError
    {
        public static Error NullOrEmpty => new Error("ScoringPanel.NullOrEmpty", "The Scoring Panel is required.");
        public static Error HazardCodeRequired => new Error("ScoringPanel.HazardCodeRequired", "The Hazard Code is required.");
        public static Error SMSUserCodeRequired => new Error("ScoringPanel.SMSUserCodeRequired", "The SMS User Code is required.");
        public static Error InvalidLikelihood => new Error("ScoringPanel.InvalidLikelihood", "The Likelihood value is invalid.");
        public static Error InvalidSeverity => new Error("ScoringPanel.InvalidSeverity", "The Severity value is invalid.");
        public static Error InvalidScore => new Error("ScoringPanel.InvalidScore", "The Score value is invalid.");
        public static Error NotFound => new Error("ScoringPanel.NotFound", "The Scoring Panel was not found.");
        public static Error CreateFailed => new Error("ScoringPanel.CreateFailed", "Failed to create the Scoring Panel.");
        public static Error UpdateFailed => new Error("ScoringPanel.UpdateFailed", "Failed to update the Scoring Panel.");
        public static Error DeleteFailed => new Error("ScoringPanel.DeleteFailed", "Failed to delete the Scoring Panel.");
    }

    /// <summary>
    /// Contains report validation-related errors.
    /// </summary>
    public static class ReportValidationError
    {
        public static Error NullOrEmpty => new Error("ReportValidation.NullOrEmpty", "The Report Validation is required.");
        public static Error ReportCodeRequired => new Error("ReportValidation.ReportCodeRequired", "The Report Code is required.");
        public static Error ValidationDecisionRequired => new Error("ReportValidation.ValidationDecisionRequired", "The Validation Decision is required.");
        public static Error InvalidStatus => new Error("ReportValidation.InvalidStatus", "The Validation Status is invalid.");
        public static Error NotFound => new Error("ReportValidation.NotFound", "The Report Validation was not found.");
        public static Error CreateFailed => new Error("ReportValidation.CreateFailed", "Failed to create the Report Validation.");
        public static Error UpdateFailed => new Error("ReportValidation.UpdateFailed", "Failed to update the Report Validation.");
        public static Error DeleteFailed => new Error("ReportValidation.DeleteFailed", "Failed to delete the Report Validation.");
    }

    // Training/Course-related errors (commented out as they don't relate to SMS POCO classes)
    /*
    public static class MachineError
    {
        public static Error CurrentStateNullOrEmpty => new Error("Machine.CurrentStateNullOrEmpty", "The Current State is required.");
    }

    public static class TraineeError
    {
        public static Error NullOrEmpty => new Error("Trainee.NullOrEmpty", "The Trainee is required.");
    }

    public static class TrainingStationError
    {
        public static Error NullOrEmpty => new Error("TrainingStation.NullOrEmpty", "The Training Station can not be null.");
    }

    public static class TrainingSessionError
    {
        public static Error NullOrEmpty => new Error("TrainingSession.NullOrEmpty", "The Training Session can not be null.");
    }

    public static class CourseError
    {
        public static Error NullOrEmpty => new Error("Course.NullOrEmpty", "The Course is required.");
        public static Error NoLessonsFound => new Error("Course.NoLessonsFound", "The course must have at least one lesson.");
        public static Error InValidCourse => new Error("Course.InValidCourse", "Cannot start CourseMachine without a valid Course with lessons.");
        public static Error CourseCheck => new Error("Course.CourseCheck", "Course Check Error");
    }

    public static class LessonError
    {
        public static Error NullOrEmpty => new Error("Lesson.NullOrEmpty", "The Course is required.");
        public static Error NoLessonPagesFound => new Error("Lesson.NoLessonPagesFound", "The Lesson must have at least one LessonPage.");
        public static Error InValidLesson => new Error("Lesson.InValidLesson", "Cannot start LessonMachine without a valid Lesson with LessonPages.");
    }

    public static class LessonQuizError
    {
        public static Error NullOrEmpty => new Error("LessonQuiz.NullOrEmpty", "The Lesson Quiz is required.");
        public static Error NoQuestionPoolsFound => new Error("LessonQuiz.NoQuestionPoolsFound", "The Lesson Quiz must have at least one QuestionPool.");
        public static Error AnswerNotRecorded => new Error("LessonQuiz.AnswerNotRecorded", "The Answer was not recorded.");
    }

    public static class TrainingLogEntryError
    {
        public static Error NullOrEmptyParam => new Error("TrainingLogEntry.NullOrEmptyParam", "The Training Log Params are required.");
        public static Error CourseCompletion => new Error("TrainingLogEntry.CourseCompletion", "The Course Completion was not recorded.");
        public static Error AddTrainingLogEntry => new Error("TrainingLogEntry.AddTrainingLogEntry", "The Add Training Log failed.");
        public static Error DeleteTrainingLog => new Error("TrainingLogEntry.DeleteTrainingLog", "The Delete Training Log failed.");
    }
    */

    /// <summary>
    /// Contains the notification errors.
    /// </summary>
    public static class NotificationError
    {
        public static Error AlreadySent => new Error("Notification.AlreadySent", "The notification has already been sent.");
    }

    /// <summary>
    /// Contains the name errors.
    /// </summary>
    public static class NameError
    {
        public static Error NullOrEmpty => new Error("Name.NullOrEmpty", "The name is required.");
        public static Error LongerThanAllowed => new Error("Name.LongerThanAllowed", "The name is longer than allowed.");
    }

    // Personal information errors (commented out as they don't relate to SMS POCO classes)
    
    public static class FirstNameError
    {
        public static Error NullOrEmpty => new Error("FirstName.NullOrEmpty", "The first name is required.");
        public static Error LongerThanAllowed => new Error("FirstName.LongerThanAllowed", "The first name is longer than allowed.");
        public static Error ContainsSpecialCharactersOrNumbers => new Error("FirstName.ContainsSpecialCharactersOrNumbers", "The first name must not contain special characters or numeric values");
    }

    public static class LastNameError
    {
        public static Error NullOrEmpty => new Error("LastName.NullOrEmpty", "The last name is required.");
        public static Error LongerThanAllowed => new Error("LastName.LongerThanAllowed", "The last name is longer than allowed.");
        public static Error ContainsSpecialCharactersOrNumbers => new Error("LastName.ContainsSpecialCharactersOrNumbers", "The last name must not contain special characters or numeric values");
    }
    /*
    public static class UPIDError
    {
        public static Error NullOrEmpty => new Error("UPID.NullOrEmpty", "The UPID is required.");
        public static Error NonNumericCharacters => new Error("UPID.NonNumericCharacters", "Non numeric characters are not allowed");
        public static Error RequiredLength => new Error("UPID.RequiredLength", "The UPID is not of the required length.");
        public static Error OutOfAllowedRange => new Error("UPID.OutOfAllowedRange", "The UPID is not within the allowed range.");
        public static Error InvalidUPID => new Error("UPID.InvalidUPID", "The UPID is invalid.");
        public static Error MismatchUPID => new Error("UPID.MismatchUPID", "The UPIDs provided do not match.");
    }

    public static class YearOfBirthError
    {
        public static Error NullOrEmpty => new Error("YearOfBirth.NullOrEmpty", "The YearOfBirth is required.");
        public static Error NonNumericCharacters => new Error("YearOfBirth.NonNumericCharacters", "Non numeric characters are not allowed");
        public static Error RequiredLength => new Error("YearOfBirth.RequiredLength", "The YearOfBirth is not of the required length.");
        public static Error OutOfRange => new Error("YearOfBirth.OutOfRange", "The YearOfBirth is out of the allowable range.");
        public static Error MismatchYearOfBirth => new Error("YearOfBirth.MismatchYearOfBirth", "The Year of Birth provided does not match.");
    }

    public static class URLError
    {
        public static Error NullOrEmpty => new Error("URLError.NullOrEmpty", "The URL is required.");
        public static Error NumericCharacters => new Error("URLError.NumericCharacters", "Numeric characters are not allowed");
        public static Error InValid => new Error("URLError.InValid", "The URL is invalid.");
    }

    public static class SubscribeToEmailNewsletterError
    {
        public static Error NullOrEmpty => new Error("SubscribeToEmailNewsletterError.NullOrEmpty", "The Subscribe To EmailNewsletter option is required.");
        public static Error InValid => new Error("SubscribeToEmailNewsletterError.InValid", "The Subscribe To Email Newsletter option is invalid.";
    }

    public static class SubscribeToTextNewsletterError
    {
        public static Error NullOrEmpty => new Error("SubscribeToTextNewsletterError.NullOrEmpty", "The Subscribe To Text Newsletter option is required.");
        public static Error InValid => new Error("SubscribeToTextNewsletterError.InValid", "The Subscribe To Text Newsletter option is invalid.";
    }

    public static class SubscribeToOperationalTextsError
    {
        public static Error NullOrEmpty => new Error("SubscribeToOperationalTextsError.NullOrEmpty", "The Subscribe To Operational Texts option is required.");
        public static Error InValid => new Error("SubscribeToOperationalTextsError.InValid", "The Subscribe To Operational Texts option is invalid.";
    }
    */

    /// <summary>
    /// Contains general errors.
    /// </summary>
    public static class GeneralError
    {
        public static Error UnProcessableRequest => new Error(
            "General.UnProcessableRequest",
            "The server could not process the request.");

        public static Error ServerError => new Error("General.ServerError", "The server encountered an unrecoverable error.");
    }
}