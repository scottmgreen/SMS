//-----------------------------------------------------------------------
// <copyright file="DomainErrors.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Comprehensive error catalog defining structured error handling for SMS domain operations with business-meaningful error codes and messages.
//                  Domain error definitions providing structured error handling
//                  with business-meaningful error codes and messages.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Errors;

/// <summary>
/// Contains the domain errors for Safety Management System (SMS).
/// All error classes follow the convention: DomainErrors.<EntityName>Error
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
    /// Contains SMS application group errors.
    /// </summary>
    public static class SMSApplicationGroupError
    {
        public static Error NullOrEmpty => new Error("SMSApplicationGroup.NullOrEmpty", "The SMS Application Group is required.");
        public static Error CodeRequired => new Error("SMSApplicationGroup.CodeRequired", "The SMS Application Group Code is required.");
        public static Error GroupNameRequired => new Error("SMSApplicationGroup.GroupNameRequired", "The Group Name is required.");
        public static Error CreatedByRequired => new Error("SMSApplicationGroup.CreatedByRequired", "The Created By field is required.");
        public static Error InvalidCode => new Error("SMSApplicationGroup.InvalidCode", "The SMS Application Group Code is invalid.");
        public static Error InvalidGroupName => new Error("SMSApplicationGroup.InvalidGroupName", "The Group Name is invalid or too short.");
        public static Error InvalidDescription => new Error("SMSApplicationGroup.InvalidDescription", "The Description exceeds maximum length.");
        public static Error GroupNameTooLong => new Error("SMSApplicationGroup.GroupNameTooLong", "The Group Name cannot exceed 255 characters.");
        public static Error DescriptionTooLong => new Error("SMSApplicationGroup.DescriptionTooLong", "The Description cannot exceed 1000 characters.");
        public static Error NotFound => new Error("SMSApplicationGroup.NotFound", "The SMS Application Group was not found.");
        public static Error CreateFailed => new Error("SMSApplicationGroup.CreateFailed", "Failed to create the SMS Application Group.");
        public static Error UpdateFailed => new Error("SMSApplicationGroup.UpdateFailed", "Failed to update the SMS Application Group.");
        public static Error DeleteFailed => new Error("SMSApplicationGroup.DeleteFailed", "Failed to delete the SMS Application Group.");
        public static Error ActivationFailed => new Error("SMSApplicationGroup.ActivationFailed", "Failed to activate the SMS Application Group.");
        public static Error DeactivationFailed => new Error("SMSApplicationGroup.DeactivationFailed", "Failed to deactivate the SMS Application Group.");
        public static Error AlreadyActive => new Error("SMSApplicationGroup.AlreadyActive", "The SMS Application Group is already active.");
        public static Error AlreadyInactive => new Error("SMSApplicationGroup.AlreadyInactive", "The SMS Application Group is already inactive.");
        public static Error CannotDeleteActiveGroup => new Error("SMSApplicationGroup.CannotDeleteActiveGroup", "Cannot delete an active stakeholder group. Deactivate it first.");
        public static Error GroupHasMembers => new Error("SMSApplicationGroup.GroupHasMembers", "Cannot delete a group that has assigned members.");
        public static Error DuplicateGroupName => new Error("SMSApplicationGroup.DuplicateGroupName", "A stakeholder group with this name already exists.");
        public static Error DuplicateCode => new Error("SMSApplicationGroup.DuplicateCode", "A stakeholder group with this code already exists.");

        // User-Group Assignment Errors
        public static Error UserCodeRequired => new Error("SMSApplicationGroup.UserCodeRequired", "The User Code is required for group assignment.");
        public static Error AssignedByRequired => new Error("SMSApplicationGroup.AssignedByRequired", "The Assigned By field is required.");
        public static Error UserNotFound => new Error("SMSApplicationGroup.UserNotFound", "The specified user was not found.");
        public static Error UserAlreadyInGroup => new Error("SMSApplicationGroup.UserAlreadyInGroup", "The user is already assigned to this group.");
        public static Error UserNotInGroup => new Error("SMSApplicationGroup.UserNotInGroup", "The user is not assigned to this group.");
        public static Error AssignmentFailed => new Error("SMSApplicationGroup.AssignmentFailed", "Failed to assign user to the stakeholder group.");
        public static Error RemovalFailed => new Error("SMSApplicationGroup.RemovalFailed", "Failed to remove user from the stakeholder group.");
        public static Error ClearGroupsFailed => new Error("SMSApplicationGroup.ClearGroupsFailed", "Failed to clear all group memberships for the user.");
        public static Error CannotAssignToInactiveGroup => new Error("SMSApplicationGroup.CannotAssignToInactiveGroup", "Cannot assign users to an inactive stakeholder group.");
        public static Error CompanyNotAllowed => new Error("SMSApplicationGroup.CompanyNotAllowed", "The user's company is not allowed for this group.");
        public static Error InvalidUserType => new Error("SMSApplicationGroup.InvalidUserType", "Only stakeholder users can be assigned to stakeholder groups.");
        public static Error MaxMembersExceeded => new Error("SMSApplicationGroup.MaxMembersExceeded", "The stakeholder group has reached its maximum member limit.");
        public static Error MinMembersRequired => new Error("SMSApplicationGroup.MinMembersRequired", "The stakeholder group must have at least one member.");
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
    /// Contains SMS stakeholder group errors.
    /// </summary>
    public static class SMSStakeholderGroupError
    {
        public static Error NullOrEmpty => new Error("SMSStakeholderGroup.NullOrEmpty", "The SMS Stakeholder Group is required.");
        public static Error CodeRequired => new Error("SMSStakeholderGroup.CodeRequired", "The SMS Stakeholder Group Code is required.");
        public static Error GroupNameRequired => new Error("SMSStakeholderGroup.GroupNameRequired", "The Group Name is required.");
        public static Error CreatedByRequired => new Error("SMSStakeholderGroup.CreatedByRequired", "The Created By field is required.");
        public static Error InvalidCode => new Error("SMSStakeholderGroup.InvalidCode", "The SMS Stakeholder Group Code is invalid.");
        public static Error InvalidGroupName => new Error("SMSStakeholderGroup.InvalidGroupName", "The Group Name is invalid or too short.");
        public static Error InvalidDescription => new Error("SMSStakeholderGroup.InvalidDescription", "The Description exceeds maximum length.");
        public static Error GroupNameTooLong => new Error("SMSStakeholderGroup.GroupNameTooLong", "The Group Name cannot exceed 255 characters.");
        public static Error DescriptionTooLong => new Error("SMSStakeholderGroup.DescriptionTooLong", "The Description cannot exceed 1000 characters.");
        public static Error NotFound => new Error("SMSStakeholderGroup.NotFound", "The SMS Stakeholder Group was not found.");
        public static Error CreateFailed => new Error("SMSStakeholderGroup.CreateFailed", "Failed to create the SMS Stakeholder Group.");
        public static Error UpdateFailed => new Error("SMSStakeholderGroup.UpdateFailed", "Failed to update the SMS Stakeholder Group.");
        public static Error DeleteFailed => new Error("SMSStakeholderGroup.DeleteFailed", "Failed to delete the SMS Stakeholder Group.");
        public static Error ActivationFailed => new Error("SMSStakeholderGroup.ActivationFailed", "Failed to activate the SMS Stakeholder Group.");
        public static Error DeactivationFailed => new Error("SMSStakeholderGroup.DeactivationFailed", "Failed to deactivate the SMS Stakeholder Group.");
        public static Error AlreadyActive => new Error("SMSStakeholderGroup.AlreadyActive", "The SMS Stakeholder Group is already active.");
        public static Error AlreadyInactive => new Error("SMSStakeholderGroup.AlreadyInactive", "The SMS Stakeholder Group is already inactive.");
        public static Error CannotDeleteActiveGroup => new Error("SMSStakeholderGroup.CannotDeleteActiveGroup", "Cannot delete an active stakeholder group. Deactivate it first.");
        public static Error GroupHasMembers => new Error("SMSStakeholderGroup.GroupHasMembers", "Cannot delete a group that has assigned members.");
        public static Error DuplicateGroupName => new Error("SMSStakeholderGroup.DuplicateGroupName", "A stakeholder group with this name already exists.");
        public static Error DuplicateCode => new Error("SMSStakeholderGroup.DuplicateCode", "A stakeholder group with this code already exists.");

        // User-Group Assignment Errors
        public static Error UserCodeRequired => new Error("SMSStakeholderGroup.UserCodeRequired", "The User Code is required for group assignment.");
        public static Error AssignedByRequired => new Error("SMSStakeholderGroup.AssignedByRequired", "The Assigned By field is required.");
        public static Error UserNotFound => new Error("SMSStakeholderGroup.UserNotFound", "The specified user was not found.");
        public static Error UserAlreadyInGroup => new Error("SMSStakeholderGroup.UserAlreadyInGroup", "The user is already assigned to this group.");
        public static Error UserNotInGroup => new Error("SMSStakeholderGroup.UserNotInGroup", "The user is not assigned to this group.");
        public static Error AssignmentFailed => new Error("SMSStakeholderGroup.AssignmentFailed", "Failed to assign user to the stakeholder group.");
        public static Error RemovalFailed => new Error("SMSStakeholderGroup.RemovalFailed", "Failed to remove user from the stakeholder group.");
        public static Error ClearGroupsFailed => new Error("SMSStakeholderGroup.ClearGroupsFailed", "Failed to clear all group memberships for the user.");
        public static Error CannotAssignToInactiveGroup => new Error("SMSStakeholderGroup.CannotAssignToInactiveGroup", "Cannot assign users to an inactive stakeholder group.");
        public static Error CompanyNotAllowed => new Error("SMSStakeholderGroup.CompanyNotAllowed", "The user's company is not allowed for this group.");
        public static Error InvalidUserType => new Error("SMSStakeholderGroup.InvalidUserType", "Only stakeholder users can be assigned to stakeholder groups.");
        public static Error MaxMembersExceeded => new Error("SMSStakeholderGroup.MaxMembersExceeded", "The stakeholder group has reached its maximum member limit.");
        public static Error MinMembersRequired => new Error("SMSStakeholderGroup.MinMembersRequired", "The stakeholder group must have at least one member.");
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
        public static readonly Error DeleteFailed = new Error(
            "SMSUserRole.DeleteFailed",
            "Failed to delete SMS user role.");
    }

    /// <summary>
    /// Contains SMS department-related errors.
    /// </summary>
    public static class SMSDepartmentError
    {
        public static Error NullOrEmpty => new Error("SMSOrganization.NullOrEmpty", "The SMS Department is required.");
        public static Error InvalidDepartment => new Error("SMSOrganization.InvalidDepartment", "The SMS Department is not valid.");
        public static Error DepartmentNotFound => new Error("SMSOrganization.DepartmentNotFound", "The specified SMS Department was not found.");
        public static Error ResponsibilityNotFound => new Error("SMSOrganization.ResponsibilityNotFound", "The specified responsibility is not assigned to this department.");
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
        public static Error InvalidCode => new Error("Hazard.InvalidCode", "The Hazard Code is invalid or description too short.");
        public static Error SubmittedByRequired => new Error("Hazard.SubmittedByRequired", "The SubmittedBy field is required.");
        public static Error NotFound => new Error("Hazard.NotFound", "The Hazard was not found.");
        public static Error CreateFailed => new Error("Hazard.CreateFailed", "Failed to create the Hazard.");
        public static Error UpdateFailed => new Error("Hazard.UpdateFailed", "Failed to update the Hazard.");
        public static Error DeleteFailed => new Error("Hazard.DeleteFailed", "Failed to delete the Hazard.");
    }

    /// <summary>
    /// Contains interview-related errors.
    /// </summary>
    public static class InterviewError
    {
        public static Error NullOrEmpty => new Error("Interview.NullOrEmpty", "The Interview is required.");
        public static Error NotFound => new Error("Interview.NotFound", "The Interview was not found.");
        public static Error CreateFailed => new Error("Interview.CreateFailed", "Failed to create the Interview.");
        public static Error UpdateFailed => new Error("Interview.UpdateFailed", "Failed to update the Interview.");
        public static Error DeleteFailed => new Error("Interview.DeleteFailed", "Failed to delete the Interview.");
        public static Error CodeRequired => new Error("Interview.CodeRequired", "The Interview Code is required.");
        public static Error InvestigationCodeRequired => new Error("Interview.InvestigationCodeRequired", "The Investigation Code is required.");
        public static Error PersonInterviewedRequired => new Error("Interview.PersonInterviewedRequired", "The person being interviewed is required.");
        public static Error InvestigatorRequired => new Error("Interview.InvestigatorRequired", "An assigned investigator is required.");
        public static Error CannotModifyCompleted => new Error("Interview.CannotModifyCompleted", "Cannot modify a completed interview.");
        public static Error CannotComplete => new Error("Interview.CannotComplete", "Cannot complete interview in current status.");
        public static Error MustBeScheduled => new Error("Interview.MustBeScheduled", "Interview must be scheduled before starting.");
        public static Error InterviewDateMustBeFuture => new Error("Interview.InterviewDateMustBeFuture", "Interview date must be in the future.");
        public static Error CancellationReasonRequired => new Error("Interview.CancellationReasonRequired", "A cancellation reason is required.");
    }

    /// <summary>
    /// Contains investigation-related errors.
    /// </summary>
    public static class InvestigationError
    {
        public static Error NullOrEmpty => new Error("Investigation.NullOrEmpty", "The Investigation is required.");
        public static Error NotFound => new Error("Investigation.NotFound", "The Investigation was not found.");
        public static Error CreateFailed => new Error("Investigation.CreateFailed", "Failed to create the Investigation.");
        public static Error UpdateFailed => new Error("Investigation.UpdateFailed", "Failed to update the Investigation.");
        public static Error DeleteFailed => new Error("Investigation.DeleteFailed", "Failed to delete the Investigation.");
        public static Error CodeRequired => new Error("Investigation.CodeRequired", "The Investigation Code is required.");
        public static Error ReportCodeRequired => new Error("Investigation.ReportCodeRequired", "The Report Code is required.");
        public static Error HazardCodeRequired => new Error("Investigation.HazardCodeRequired", "The Hazard Code is required.");
        public static Error AssignedInvestigatorRequired => new Error("Investigation.AssignedInvestigatorRequired", "An assigned investigator is required.");
        public static Error InvalidStatus => new Error("Investigation.InvalidStatus", "The Investigation Status is invalid.");
        public static Error CannotModifyCompleted => new Error("Investigation.CannotModifyCompleted", "Cannot modify a completed investigation.");
        public static Error CannotComplete => new Error("Investigation.CannotComplete", "Cannot complete investigation in current status.");
        public static Error MustBeAssigned => new Error("Investigation.MustBeAssigned", "Investigation must be assigned before starting.");
        public static Error PlanRequired => new Error("Investigation.PlanRequired", "Investigation plan is required before starting.");
        public static Error ObjectivesRequired => new Error("Investigation.ObjectivesRequired", "Investigation objectives are required.");
        public static Error DecisionRequired => new Error("Investigation.DecisionRequired", "Investigation decision is required for completion.");
        public static Error DecisionMakerRequired => new Error("Investigation.DecisionMakerRequired", "Decision maker is required.");
        public static Error InvalidDecisionType => new Error("Investigation.InvalidDecisionType", "The decision type is invalid.");
        public static Error CompletionFailed => new Error("Investigation.CompletionFailed", "Failed to complete the investigation.");
        public static Error StatusUpdateFailed => new Error("Investigation.StatusUpdateFailed", "Failed to update investigation status.");
        public static Error AssignmentFailed => new Error("Investigation.AssignmentFailed", "Failed to assign investigator.");
        public static Error InvestigatorRequired => new Error("Investigation.InvestigatorRequired", "An investigator is required.");
        public static Error DecisionTypeRequired => new Error("Investigation.DecisionTypeRequired", "Decision type is required.");
        public static Error DecisionRationaleRequired => new Error("Investigation.DecisionRationaleRequired", "Decision rationale is required.");
        public static Error AlreadyCompleted => new Error("Investigation.AlreadyCompleted", "Investigation is already completed.");
        public static Error NotCompleted => new Error("Investigation.NotCompleted", "Investigation is not completed.");
        public static Error ReasonRequired => new Error("Investigation.ReasonRequired", "A reason is required.");
        public static Error InvalidCode => new Error("Investigation.InvalidCode", "The investigation code is invalid or malformed.");
        public static Error InvalidInvestigator => new Error("Investigation.InvalidInvestigator", "The assigned investigator is invalid or not authorized.");
        public static Error MissingRecommendations => new Error("Investigation.MissingRecommendations", "Investigation recommendations are required before completion.");
    }

    /// <summary>
    /// Contains mitigation-related errors.
    /// </summary>
    public static class MitigationError
    {
        public static Error NullOrEmpty => new Error("Mitigation.NullOrEmpty", "The Mitigation is required.");
        public static Error CodeRequired => new Error("Mitigation.CodeRequired", "The Mitigation Code is required.");
        public static Error InvalidCode => new Error("Mitigation.InvalidCode", "The Mitigation Code is invalid.");
        public static Error InvalidName => new Error("Mitigation.InvalidName", "The Mitigation Name is required.");
        public static Error InvalidHazardCode => new Error("Mitigation.InvalidHazardCode", "The Hazard Code is required.");
        public static Error NotFound => new Error("Mitigation.NotFound", "The Mitigation was not found.");
        public static Error CreateFailed => new Error("Mitigation.CreateFailed", "Failed to create the Mitigation.");
        public static Error UpdateFailed => new Error("Mitigation.UpdateFailed", "Failed to update the Mitigation.");
        public static Error DeleteFailed => new Error("Mitigation.DeleteFailed", "Failed to delete the Mitigation.");
        public static Error InvalidType => new Error("Mitigation.InvalidType", "The Mitigation Type is invalid.");
        public static Error InvalidStatus => new Error("Mitigation.InvalidStatus", "The Mitigation Status is invalid.");
        public static Error InvalidAssignment => new Error("Mitigation.InvalidAssignment", "The Mitigation assignment is invalid.");
        public static Error InvalidDueDate => new Error("Mitigation.InvalidDueDate", "The Mitigation due date is invalid.");
        public static Error InvalidProgress => new Error("Mitigation.InvalidProgress", "The progress percentage must be between 0 and 100.");
        public static Error InvalidApprover => new Error("Mitigation.InvalidApprover", "The approver is required.");
        public static Error InvalidCancellationReason => new Error("Mitigation.InvalidCancellationReason", "A cancellation reason is required.");
        public static Error AlreadyCompleted => new Error("Mitigation.AlreadyCompleted", "The Mitigation is already completed.");
        public static Error CannotModifyCompleted => new Error("Mitigation.CannotModifyCompleted", "Cannot modify a completed Mitigation.");

        // Enhanced validation errors for comprehensive schema
        public static Error InvalidTargetDate => new Error("Mitigation.InvalidTargetDate", "The target date cannot be in the past.");
        public static Error InvalidEstimatedCost => new Error("Mitigation.InvalidEstimatedCost", "The estimated cost must be greater than or equal to 0.");
        public static Error InvalidActualCost => new Error("Mitigation.InvalidActualCost", "The actual cost must be greater than or equal to 0.");
        public static Error InvalidEstimatedHours => new Error("Mitigation.InvalidEstimatedHours", "The estimated hours must be greater than 0.");
        public static Error InvalidActualHours => new Error("Mitigation.InvalidActualHours", "The actual hours must be greater than or equal to 0.");
        public static Error InvalidSeverityReduction => new Error("Mitigation.InvalidSeverityReduction", "Severity reduction must be between 1 and 5.");
        public static Error InvalidLikelihoodReduction => new Error("Mitigation.InvalidLikelihoodReduction", "Likelihood reduction must be between 1 and 5.");
        public static Error InvalidEffectivenessRating => new Error("Mitigation.InvalidEffectivenessRating", "The effectiveness rating is invalid.");
        public static Error InvalidMonitoringFrequency => new Error("Mitigation.InvalidMonitoringFrequency", "The monitoring frequency is invalid.");
        public static Error ValidationRequiredWithoutValidator => new Error("Mitigation.ValidationRequiredWithoutValidator", "Validation is required but no validator is assigned.");
        public static Error CannotValidateWithoutTesting => new Error("Mitigation.CannotValidateWithoutTesting", "Cannot validate mitigation without completed testing.");
        public static Error DependenciesNotMet => new Error("Mitigation.DependenciesNotMet", "Cannot proceed - required dependencies are not completed.");
        public static Error PrerequisitesNotMet => new Error("Mitigation.PrerequisitesNotMet", "Cannot proceed - required prerequisites are not satisfied.");
    }

    /// <summary>
    /// Contains report-related errors.
    /// </summary>
    public static class ReportError
    {
        public static Error NullOrEmpty => new Error("Report.NullOrEmpty", "The Report is required.");
        public static Error CodeRequired => new Error("Report.CodeRequired", "The Report Code is required.");
        public static Error InvalidCode => new Error("Report.InvalidCode", "The Report Code is invalid.");
        public static Error NotFound => new Error("Report.NotFound", "The Report was not found.");
        public static Error CreateFailed => new Error("Report.CreateFailed", "Failed to create the Report.");
        public static Error UpdateFailed => new Error("Report.UpdateFailed", "Failed to update the Report.");
        public static Error DeleteFailed => new Error("Report.DeleteFailed", "Failed to delete the Report.");
        public static Error InvalidReportType => new Error("Report.InvalidReportType", "The Report Type is invalid.");
        public static Error InvalidStatus => new Error("Report.InvalidStatus", "The Report Status is invalid.");
        public static Error InvalidReporter => new Error("Report.InvalidReporter", "The Reporter information is invalid.");
        public static Error InvalidIncidentDate => new Error("Report.InvalidIncidentDate", "The Incident Date is invalid.");
        public static Error MissingDescription => new Error("Report.MissingDescription", "The Report description is required.");
        public static Error CannotModifySubmitted => new Error("Report.CannotModifySubmitted", "Cannot modify a submitted Report.");
        public static Error AlreadySubmitted => new Error("Report.AlreadySubmitted", "The Report has already been submitted.");
    }

    /// <summary>
    /// Contains risk analysis-related errors.
    /// </summary>
    public static class RiskAnalysisError
    {
        public static Error NullOrEmpty => new Error("RiskAnalysis.NullOrEmpty", "The Risk Analysis is required.");
        public static Error CodeRequired => new Error("RiskAnalysis.CodeRequired", "The Risk Analysis Code is required.");
        public static Error InvalidCode => new Error("RiskAnalysis.InvalidCode", "The Risk Analysis Code is invalid.");
        public static Error NotFound => new Error("RiskAnalysis.NotFound", "The Risk Analysis was not found.");
        public static Error CreateFailed => new Error("RiskAnalysis.CreateFailed", "Failed to create the Risk Analysis.");
        public static Error UpdateFailed => new Error("RiskAnalysis.UpdateFailed", "Failed to update the Risk Analysis.");
        public static Error DeleteFailed => new Error("RiskAnalysis.DeleteFailed", "Failed to delete the Risk Analysis.");
        public static Error InvalidAnalysisType => new Error("RiskAnalysis.InvalidAnalysisType", "The Risk Analysis Type is invalid.");
        public static Error InvalidStatus => new Error("RiskAnalysis.InvalidStatus", "The Risk Analysis Status is invalid.");
        public static Error InvalidAnalyst => new Error("RiskAnalysis.InvalidAnalyst", "The Risk Analyst information is invalid.");
        public static Error MissingWorstOutcome => new Error("RiskAnalysis.MissingWorstOutcome", "The worst credible outcome is required.");
        public static Error MissingRootCause => new Error("RiskAnalysis.MissingRootCause", "The root cause analysis is required.");
        public static Error IncompleteAnalysis => new Error("RiskAnalysis.IncompleteAnalysis", "The Risk Analysis is incomplete.");
        public static Error CannotModifyCompleted => new Error("RiskAnalysis.CannotModifyCompleted", "Cannot modify a completed Risk Analysis.");
    }

    /// <summary>
    /// Contains scoring panel-related errors.
    /// </summary>
    public static class ScoringPanelError
    {
        public static Error NullOrEmpty => new Error("ScoringPanel.NullOrEmpty", "The Scoring Panel is required.");
        public static Error CodeRequired => new Error("ScoringPanel.CodeRequired", "The Scoring Panel Code is required.");
        public static Error InvalidCode => new Error("ScoringPanel.InvalidCode", "The Scoring Panel Code is invalid.");
        public static Error NotFound => new Error("ScoringPanel.NotFound", "The Scoring Panel was not found.");
        public static Error CreateFailed => new Error("ScoringPanel.CreateFailed", "Failed to create the Scoring Panel.");
        public static Error UpdateFailed => new Error("ScoringPanel.UpdateFailed", "Failed to update the Scoring Panel.");
        public static Error DeleteFailed => new Error("ScoringPanel.DeleteFailed", "Failed to delete the Scoring Panel.");
        public static Error InvalidPanelType => new Error("ScoringPanel.InvalidPanelType", "The Scoring Panel Type is invalid.");
        public static Error InvalidStatus => new Error("ScoringPanel.InvalidStatus", "The Scoring Panel Status is invalid.");
        public static Error InvalidMember => new Error("ScoringPanel.InvalidMember", "The Panel Member information is invalid.");
        public static Error MemberAlreadyExists => new Error("ScoringPanel.MemberAlreadyExists", "The member is already part of this scoring panel.");
        public static Error MemberNotFound => new Error("ScoringPanel.MemberNotFound", "The panel member was not found.");
        public static Error InsufficientMembers => new Error("ScoringPanel.InsufficientMembers", "The scoring panel requires at least 3 members.");
        public static Error MaxMembersExceeded => new Error("ScoringPanel.MaxMembersExceeded", "The scoring panel has reached its maximum member limit.");
        public static Error InvalidScore => new Error("ScoringPanel.InvalidScore", "The score value is invalid.");
        public static Error ScoreOutOfRange => new Error("ScoringPanel.ScoreOutOfRange", "The score must be between 1 and 5.");
        public static Error DuplicateScore => new Error("ScoringPanel.DuplicateScore", "This member has already submitted a score for this item.");
        public static Error CannotModifyCompletedPanel => new Error("ScoringPanel.CannotModifyCompletedPanel", "Cannot modify a completed scoring panel.");
        public static Error PanelNotActive => new Error("ScoringPanel.PanelNotActive", "The scoring panel is not active.");
        public static Error ScoringNotComplete => new Error("ScoringPanel.ScoringNotComplete", "All panel members must submit scores before completion.");
    }

    /// <summary>
    /// Contains mitigation assignment-related errors.
    /// </summary>
    public static class MitigationAssignmentError
    {
        public static Error NullOrEmpty => new Error("MitigationAssignment.NullOrEmpty", "The Mitigation Assignment is required.");
        public static Error CodeRequired => new Error("MitigationAssignment.CodeRequired", "The Mitigation Assignment Code is required.");
        public static Error InvalidCode => new Error("MitigationAssignment.InvalidCode", "The Mitigation Assignment Code is invalid.");
        public static Error NotFound => new Error("MitigationAssignment.NotFound", "The Mitigation Assignment was not found.");
        public static Error CreateFailed => new Error("MitigationAssignment.CreateFailed", "Failed to create the Mitigation Assignment.");
        public static Error UpdateFailed => new Error("MitigationAssignment.UpdateFailed", "Failed to update the Mitigation Assignment.");
        public static Error DeleteFailed => new Error("MitigationAssignment.DeleteFailed", "Failed to delete the Mitigation Assignment.");
        public static Error InvalidAssignmentType => new Error("MitigationAssignment.InvalidAssignmentType", "The Mitigation Assignment Type is invalid.");
        public static Error InvalidStatus => new Error("MitigationAssignment.InvalidStatus", "The Mitigation Assignment Status is invalid.");
        public static Error InvalidAssignee => new Error("MitigationAssignment.InvalidAssignee", "The Assignee information is invalid.");
        public static Error InvalidAssigner => new Error("MitigationAssignment.InvalidAssigner", "The Assigner information is invalid.");
        public static Error InvalidDueDate => new Error("MitigationAssignment.InvalidDueDate", "The Due Date is invalid or in the past.");
        public static Error InvalidPriority => new Error("MitigationAssignment.InvalidPriority", "The Priority level is invalid.");
        public static Error AlreadyAssigned => new Error("MitigationAssignment.AlreadyAssigned", "This mitigation is already assigned to the specified user.");
        public static Error AssigneeNotFound => new Error("MitigationAssignment.AssigneeNotFound", "The assigned user was not found.");
        public static Error CannotModifyCompleted => new Error("MitigationAssignment.CannotModifyCompleted", "Cannot modify a completed Mitigation Assignment.");
        public static Error CannotReassign => new Error("MitigationAssignment.CannotReassign", "Cannot reassign a mitigation that is in progress or completed.");
        public static Error InsufficientPermissions => new Error("MitigationAssignment.InsufficientPermissions", "User does not have permission to assign mitigations.");
        public static Error WorkflowViolation => new Error("MitigationAssignment.WorkflowViolation", "The assignment violates workflow rules.");
        public static Error DependencyNotMet => new Error("MitigationAssignment.DependencyNotMet", "Cannot assign mitigation until dependencies are resolved.");
        public static Error InvalidDepartment => new Error("MitigationAssignment.InvalidDepartment", "The assigned department is invalid.");
        public static Error InvalidProgress => new Error("MitigationAssignment.InvalidProgress", "The progress percentage must be between 0 and 100.");
    }

    /// <summary>
    /// Contains risk assessment-related errors - MISSION CRITICAL.
    /// </summary>
    public static class RiskAssessmentError
    {
        // Core Risk Assessment Errors
        public static Error NullOrEmpty => new Error("RiskAssessment.NullOrEmpty", "The Risk Assessment is required.");
        public static Error NotFound => new Error("RiskAssessment.NotFound", "The Risk Assessment was not found.");
        public static Error CreateFailed => new Error("RiskAssessment.CreateFailed", "Failed to create the Risk Assessment.");
        public static Error UpdateFailed => new Error("RiskAssessment.UpdateFailed", "Failed to update the Risk Assessment.");
        public static Error DeleteFailed => new Error("RiskAssessment.DeleteFailed", "Failed to delete the Risk Assessment.");
        public static Error DeletionFailed => new Error("RiskAssessment.DeletionFailed", "Failed to delete the Risk Assessment.");
        public static Error CreationFailed => new Error("RiskAssessment.CreationFailed", "Risk assessment creation failed due to invalid data.");

        // ID and Basic Validation Errors
        public static Error InvalidId => new Error("RiskAssessment.InvalidId", "The Risk Assessment ID is invalid.");
        public static Error InvalidIdFormat => new Error("RiskAssessment.InvalidIdFormat", "The Risk Assessment ID format is invalid. Expected format: RA-YYYYMMDD-XXXXXXXX");
        public static Error InvalidName => new Error("RiskAssessment.InvalidName", "The Risk Assessment name is required and cannot be empty.");
        public static Error InvalidLeadAssessor => new Error("RiskAssessment.InvalidLeadAssessor", "The Lead Assessor ID is required and cannot be empty.");

        // Status and Workflow Errors
        public static Error InvalidStatus => new Error("RiskAssessment.InvalidStatus", "The Risk Assessment Status is invalid.");
        public static Error InvalidStage => new Error("RiskAssessment.InvalidStage", "The Stage is invalid or required.");
        public static Error InvalidStep => new Error("RiskAssessment.InvalidStep", "The step number must be between 1 and 5.");
        public static Error CannotModifyCompleted => new Error("RiskAssessment.CannotModifyCompleted", "Cannot modify a completed Risk Assessment.");
        public static Error IncompleteSteps => new Error("RiskAssessment.IncompleteSteps", "Cannot complete Risk Assessment - not all steps are completed.");

        // Hazard-Related Errors
        public static Error InvalidHazard => new Error("RiskAssessment.InvalidHazard", "The hazard ID is invalid or empty.");
        public static Error HazardNotFound => new Error("RiskAssessment.HazardNotFound", "The hazard is not part of this Risk Assessment.");
        public static Error InvalidStakeholder => new Error("RiskAssessment.InvalidStakeholder", "The stakeholder ID is invalid or empty.");
        public static Error InvalidParentAssessment => new Error("RiskAssessment.InvalidParentAssessment", "The parent assessment ID is required for residual risk assessments.");

        // Panel Scoring Errors
        public static Error InvalidPanelMember => new Error("RiskAssessment.InvalidPanelMember", "The panel member ID is invalid or empty.");
        public static Error InvalidSeverityScore => new Error("RiskAssessment.InvalidSeverityScore", "Severity score must be between 1 and 5.");
        public static Error InvalidLikelihoodScore => new Error("RiskAssessment.InvalidLikelihoodScore", "Likelihood score must be between 1 and 5.");
        public static Error InvalidRiskScore => new Error("RiskAssessment.InvalidRiskScore", "Both severity and likelihood scores must be between 1 and 5.");

        // Mitigation and Assessment Errors
        public static Error InvalidMitigationStrategy => new Error("RiskAssessment.InvalidMitigationStrategy", "The mitigation strategy is invalid or empty.");
        public static Error InvalidAssignment => new Error("RiskAssessment.InvalidAssignment", "The mitigation assignment is invalid - department is required.");

        // Legacy Support (for existing RiskAssessmentError references)
        public static Error HazardCodeRequired => new Error("RiskAssessment.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidAssessmentType => new Error("RiskAssessment.InvalidAssessmentType", "The Assessment Type is invalid.");
    }

    /// <summary>
    /// Contains mitigation strategy-related errors - MISSION CRITICAL.
    /// </summary>
    public static class MitigationStrategyError
    {
        public static Error NullOrEmpty => new Error("MitigationStrategy.NullOrEmpty", "The Mitigation Strategy is required.");
        public static Error NotFound => new Error("MitigationStrategy.NotFound", "The Mitigation Strategy was not found.");
        public static Error CreateFailed => new Error("MitigationStrategy.CreateFailed", "Failed to create the Mitigation Strategy.");
        public static Error UpdateFailed => new Error("MitigationStrategy.UpdateFailed", "Failed to update the Mitigation Strategy.");
        public static Error DeleteFailed => new Error("MitigationStrategy.DeleteFailed", "Failed to delete the Mitigation Strategy.");

        // Validation Errors
        public static Error InvalidId => new Error("MitigationStrategy.InvalidId", "The Mitigation Strategy ID is invalid.");
        public static Error InvalidIdFormat => new Error("MitigationStrategy.InvalidIdFormat", "The Mitigation Strategy ID format is invalid. Expected format: MS-YYYYMMDD-XXXXXXXX");
        public static Error InvalidDescription => new Error("MitigationStrategy.InvalidDescription", "The description is required and cannot be empty.");
        public static Error InvalidHazardId => new Error("MitigationStrategy.InvalidHazardId", "The hazard ID is required and cannot be empty.");
        public static Error InvalidProgress => new Error("MitigationStrategy.InvalidProgress", "Progress percentage must be between 0 and 100.");
        public static Error InvalidCompletedBy => new Error("MitigationStrategy.InvalidCompletedBy", "CompletedBy is required when marking as completed.");
    }

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

    /// <summary>
    /// Contains first name errors.
    /// </summary>
    public static class FirstNameError
    {
        public static Error NullOrEmpty => new Error("FirstName.NullOrEmpty", "The first name is required.");
        public static Error LongerThanAllowed => new Error("FirstName.LongerThanAllowed", "The first name is longer than allowed.");
        public static Error ContainsSpecialCharactersOrNumbers => new Error("FirstName.ContainsSpecialCharactersOrNumbers", "The first name must not contain special characters or numeric values");
    }

    /// <summary>
    /// Contains last name errors.
    /// </summary>
    public static class LastNameError
    {
        public static Error NullOrEmpty => new Error("LastName.NullOrEmpty", "The last name is required.");
        public static Error LongerThanAllowed => new Error("LastName.LongerThanAllowed", "The last name is longer than allowed.");
        public static Error ContainsSpecialCharactersOrNumbers => new Error("LastName.ContainsSpecialCharactersOrNumbers", "The last name must not contain special characters or numeric values");
    }

    /// <summary>
    /// Contains general errors.
    /// </summary>
    public static class GeneralError
    {
        public static Error UnProcessableRequest => new Error("General.UnProcessableRequest", "The server could not process the request.");
        public static Error ServerError => new Error("General.ServerError", "The server encountered an unrecoverable error.");
        public static Error InvalidParameters => new Error("General.InvalidParameters", "One or more parameters are invalid or missing.");
    }

    /// <summary>
    /// Contains hazard location-related errors.
    /// </summary>
    public static class HazardLocationError
    {
        public static Error NullOrEmpty => new Error("HazardLocation.NullOrEmpty", "The Hazard Location is required.");
        public static Error CodeRequired => new Error("HazardLocation.CodeRequired", "The Hazard Location Code is required.");
        public static Error HazardCodeRequired => new Error("HazardLocation.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidCode => new Error("HazardLocation.InvalidCode", "The Hazard Location Code is invalid.");
        public static Error InvalidLatitude => new Error("HazardLocation.InvalidLatitude", "Latitude must be between -90 and 90 degrees.");
        public static Error InvalidLongitude => new Error("HazardLocation.InvalidLongitude", "Longitude must be between -180 and 180 degrees.");
        public static Error InvalidSource => new Error("HazardLocation.InvalidSource", "Source must be GPS, Manual, Import, Survey, or Estimated.");
        public static Error CoordinatesRequired => new Error("HazardLocation.CoordinatesRequired", "Coordinates are required for location validation.");
        public static Error ValidatedByRequired => new Error("HazardLocation.ValidatedByRequired", "ValidatedBy is required when validating location.");
        public static Error NotFound => new Error("HazardLocation.NotFound", "The Hazard Location was not found.");
        public static Error CreateFailed => new Error("HazardLocation.CreateFailed", "Failed to create the Hazard Location.");
        public static Error UpdateFailed => new Error("HazardLocation.UpdateFailed", "Failed to update the Hazard Location.");
        public static Error DeleteFailed => new Error("HazardLocation.DeleteFailed", "Failed to delete the Hazard Location.");
    }

    /// <summary>
    /// Contains hazard file-related errors.
    /// </summary>
    public static class HazardFileError
    {
        public static Error NullOrEmpty => new Error("HazardFile.NullOrEmpty", "The Hazard File is required.");
        public static Error HazardCodeRequired => new Error("HazardFile.HazardCodeRequired", "The Hazard Code is required.");
        public static Error FileNameRequired => new Error("HazardFile.FileNameRequired", "The File Name is required.");
        public static Error FileTypeRequired => new Error("HazardFile.FileTypeRequired", "The File Type is required.");
        public static Error FileDataRequired => new Error("HazardFile.FileDataRequired", "The File Data is required for database storage.");
        public static Error UploadedByRequired => new Error("HazardFile.UploadedByRequired", "The UploadedBy field is required.");
        public static Error UploaderRequired => new Error("HazardFile.UploaderRequired", "An uploader is required for file creation.");
        public static Error InvalidFileSize => new Error("HazardFile.InvalidFileSize", "The file size must be greater than 0.");
        public static Error CannotModifyInactive => new Error("HazardFile.CannotModifyInactive", "Cannot modify an inactive file.");
        public static Error AlreadyInactive => new Error("HazardFile.AlreadyInactive", "The file is already inactive.");
        public static Error AlreadyActive => new Error("HazardFile.AlreadyActive", "The file is already active.");
        public static Error DeactivationReasonRequired => new Error("HazardFile.DeactivationReasonRequired", "A deactivation reason is required.");
        public static Error DeactivatedByRequired => new Error("HazardFile.DeactivatedByRequired", "The DeactivatedBy field is required.");
        public static Error NotFound => new Error("HazardFile.NotFound", "The Hazard File was not found.");
        public static Error CreateFailed => new Error("HazardFile.CreateFailed", "Failed to create the Hazard File.");
        public static Error UpdateFailed => new Error("HazardFile.UpdateFailed", "Failed to update the Hazard File.");
        public static Error DeleteFailed => new Error("HazardFile.DeleteFailed", "Failed to delete the Hazard File.");
        public static Error UnsupportedFileType => new Error("HazardFile.UnsupportedFileType", "The file type is not supported.");
        public static Error FileSizeExceedsLimit => new Error("HazardFile.FileSizeExceedsLimit", "The file size exceeds the maximum allowed limit.");
        public static Error StorageError => new Error("HazardFile.StorageError", "An error occurred while storing the file.");
    }

    /// <summary>
    /// Contains report validation-related errors.
    /// </summary>
    public static class ReportValidationError
    {
        public static Error NullOrEmpty => new Error("ReportValidation.NullOrEmpty", "The Report Validation is required.");
        public static Error CodeRequired => new Error("ReportValidation.CodeRequired", "The Report Validation Code is required.");
        public static Error ReportCodeRequired => new Error("ReportValidation.ReportCodeRequired", "The Report Code is required.");
        public static Error ValidatedByRequired => new Error("ReportValidation.ValidatedByRequired", "The ValidatedBy field is required.");
        public static Error ValidationDecisionRequired => new Error("ReportValidation.ValidationDecisionRequired", "The Validation Decision is required.");
        public static Error ValidationCommentsRequired => new Error("ReportValidation.ValidationCommentsRequired", "Validation Comments are required (minimum 5 characters).");
        public static Error InvalidCode => new Error("ReportValidation.InvalidCode", "The Report Validation Code is invalid.");
        public static Error InvalidValidationDecision => new Error("ReportValidation.InvalidValidationDecision", "The Validation Decision is invalid.");
        public static Error InvalidValidationType => new Error("ReportValidation.InvalidValidationType", "The Validation Type is invalid.");
        public static Error AssessorRequiredForSmsRisk => new Error("ReportValidation.AssessorRequiredForSmsRisk", "Assigned Assessor is required for SMS Risk validations.");
        public static Error CannotModifyCompleted => new Error("ReportValidation.CannotModifyCompleted", "Cannot modify a completed Report Validation.");
        public static Error NotFound => new Error("ReportValidation.NotFound", "The Report Validation was not found.");
        public static Error CreateFailed => new Error("ReportValidation.CreateFailed", "Failed to create the Report Validation.");
        public static Error UpdateFailed => new Error("ReportValidation.UpdateFailed", "Failed to update the Report Validation.");
        public static Error DeleteFailed => new Error("ReportValidation.DeleteFailed", "Failed to delete the Report Validation.");
        public static Error AlreadyExists => new Error("ReportValidation.AlreadyExists", "A validation for this report already exists.");
        public static Error StatusUpdateFailed => new Error("ReportValidation.StatusUpdateFailed", "Failed to update the validation status.");
        public static Error WorkflowViolation => new Error("ReportValidation.WorkflowViolation", "The validation operation violates workflow rules.");
    }

    /// <summary>
    /// Contains SMS organizational group errors.
    /// </summary>
    public static class SMSOrganizationalGroupError
    {
        public static Error NullOrEmpty => new Error("SMSOrganizationalGroup.NullOrEmpty", "The SMS Organizational Group is required.");
        public static Error CodeRequired => new Error("SMSOrganizationalGroup.CodeRequired", "The SMS Organizational Group Code is required.");
        public static Error GroupNameRequired => new Error("SMSOrganizationalGroup.GroupNameRequired", "The Group Name is required.");
        public static Error CreatedByRequired => new Error("SMSOrganizationalGroup.CreatedByRequired", "The Created By field is required.");
        public static Error InvalidCode => new Error("SMSOrganizationalGroup.InvalidCode", "The SMS Organizational Group Code is invalid.");
        public static Error InvalidGroupName => new Error("SMSOrganizationalGroup.InvalidGroupName", "The Group Name is invalid or too short.");
        public static Error InvalidDescription => new Error("SMSOrganizationalGroup.InvalidDescription", "The Description exceeds maximum length.");
        public static Error GroupNameTooLong => new Error("SMSOrganizationalGroup.GroupNameTooLong", "The Group Name cannot exceed 255 characters.");
        public static Error DescriptionTooLong => new Error("SMSOrganizationalGroup.DescriptionTooLong", "The Description cannot exceed 1000 characters.");
        public static Error InvalidGroupType => new Error("SMSOrganizationalGroup.InvalidGroupType", "The Group Type is invalid.");
        public static Error InvalidAuthorityLevel => new Error("SMSOrganizationalGroup.InvalidAuthorityLevel", "The Authority Level is invalid.");
        public static Error NotFound => new Error("SMSOrganizationalGroup.NotFound", "The SMS Organizational Group was not found.");
        public static Error CreateFailed => new Error("SMSOrganizationalGroup.CreateFailed", "Failed to create the SMS Organizational Group.");
        public static Error UpdateFailed => new Error("SMSOrganizationalGroup.UpdateFailed", "Failed to update the SMS Organizational Group.");
        public static Error DeleteFailed => new Error("SMSOrganizationalGroup.DeleteFailed", "Failed to delete the SMS Organizational Group.");
        public static Error ActivationFailed => new Error("SMSOrganizationalGroup.ActivationFailed", "Failed to activate the SMS Organizational Group.");
        public static Error DeactivationFailed => new Error("SMSOrganizationalGroup.DeactivationFailed", "Failed to deactivate the SMS Organizational Group.");
        public static Error AlreadyActive => new Error("SMSOrganizationalGroup.AlreadyActive", "The SMS Organizational Group is already active.");
        public static Error AlreadyInactive => new Error("SMSOrganizationalGroup.AlreadyInactive", "The SMS Organizational Group is already inactive.");
        public static Error CannotDeleteActiveGroup => new Error("SMSOrganizationalGroup.CannotDeleteActiveGroup", "Cannot delete an active organizational group. Deactivate it first.");
        public static Error GroupHasMembers => new Error("SMSOrganizationalGroup.GroupHasMembers", "Cannot delete a group that has assigned members.");
        public static Error DuplicateGroupName => new Error("SMSOrganizationalGroup.DuplicateGroupName", "An organizational group with this name already exists.");
        public static Error DuplicateCode => new Error("SMSOrganizationalGroup.DuplicateCode", "An organizational group with this code already exists.");

        // User-Group Assignment Errors
        public static Error UserCodeRequired => new Error("SMSOrganizationalGroup.UserCodeRequired", "The User Code is required for group assignment.");
        public static Error AssignedByRequired => new Error("SMSOrganizationalGroup.AssignedByRequired", "The Assigned By field is required.");
        public static Error UserNotFound => new Error("SMSOrganizationalGroup.UserNotFound", "The specified user was not found.");
        public static Error UserAlreadyInGroup => new Error("SMSOrganizationalGroup.UserAlreadyInGroup", "The user is already assigned to this group.");
        public static Error UserNotInGroup => new Error("SMSOrganizationalGroup.UserNotInGroup", "The user is not assigned to this group.");
        public static Error AssignmentFailed => new Error("SMSOrganizationalGroup.AssignmentFailed", "Failed to assign user to the organizational group.");
        public static Error RemovalFailed => new Error("SMSOrganizationalGroup.RemovalFailed", "Failed to remove user from the organizational group.");
        public static Error ClearGroupsFailed => new Error("SMSOrganizationalGroup.ClearGroupsFailed", "Failed to clear all group memberships for the user.");
        public static Error CannotAssignToInactiveGroup => new Error("SMSOrganizationalGroup.CannotAssignToInactiveGroup", "Cannot assign users to an inactive organizational group.");
        public static Error CompanyNotAllowed => new Error("SMSOrganizationalGroup.CompanyNotAllowed", "The user's company is not allowed for this group.");
        public static Error InvalidUserType => new Error("SMSOrganizationalGroup.InvalidUserType", "Only organizational users can be assigned to organizational groups.");
        public static Error MaxMembersExceeded => new Error("SMSOrganizationalGroup.MaxMembersExceeded", "The organizational group has reached its maximum member limit.");
        public static Error MinMembersRequired => new Error("SMSOrganizationalGroup.MinMembersRequired", "The organizational group must have at least one member.");

        // Enhanced SMS Role-specific Errors
        public static Error InvalidSMSRole => new Error("SMSOrganizationalGroup.InvalidSMSRole", "The SMS Role assignment is invalid.");
        public static Error InvalidRoleAuthorityLevel => new Error("SMSOrganizationalGroup.InvalidRoleAuthorityLevel", "The role authority level is invalid.");
        public static Error InvalidRiskApprovalAuthority => new Error("SMSOrganizationalGroup.InvalidRiskApprovalAuthority", "The risk approval authority level is invalid.");
        public static Error InsufficientAuthority => new Error("SMSOrganizationalGroup.InsufficientAuthority", "User does not have sufficient authority for this operation.");
        public static Error ConflictingRoleAssignment => new Error("SMSOrganizationalGroup.ConflictingRoleAssignment", "The SMS role assignment conflicts with existing group assignments.");
        public static Error RoleAssignmentFailed => new Error("SMSOrganizationalGroup.RoleAssignmentFailed", "Failed to assign SMS role to the organizational group member.");
        public static Error RoleRemovalFailed => new Error("SMSOrganizationalGroup.RoleRemovalFailed", "Failed to remove SMS role from the organizational group member.");
        public static Error GroupTypeAuthorityMismatch => new Error("SMSOrganizationalGroup.GroupTypeAuthorityMismatch", "The group type does not match the required authority level.");

        // Department and Organization Hierarchy Errors
        public static Error InvalidDepartmentAssignment => new Error("SMSOrganizationalGroup.InvalidDepartmentAssignment", "The department assignment is invalid for this group type.");
        public static Error OrganizationLevelMismatch => new Error("SMSOrganizationalGroup.OrganizationLevelMismatch", "The organization level does not match the group's authority level.");
        public static Error HierarchyViolation => new Error("SMSOrganizationalGroup.HierarchyViolation", "The assignment violates organizational hierarchy rules.");
        public static Error DepartmentNotFound => new Error("SMSOrganizationalGroup.DepartmentNotFound", "The specified department was not found.");
        public static Error PositionNotAuthorized => new Error("SMSOrganizationalGroup.PositionNotAuthorized", "The position is not authorized for this group type.");
    }

    /// <summary>
    /// Contains Safety Performance Indicator (SPI) related errors.
    /// </summary>
    public static class SPIError
    {
        public static Error NullOrEmpty => new Error("SPI.NullOrEmpty", "The Safety Performance Indicator is required.");
        public static Error CodeRequired => new Error("SPI.CodeRequired", "The SPI Code is required.");
        public static Error InvalidCode => new Error("SPI.InvalidCode", "The SPI Code is invalid.");
        public static Error InvalidName => new Error("SPI.InvalidName", "The SPI Name is required and cannot be empty.");
        public static Error InvalidDescription => new Error("SPI.InvalidDescription", "The SPI Description is required and cannot be empty.");
        public static Error InvalidIndicatorType => new Error("SPI.InvalidIndicatorType", "The SPI Indicator Type is invalid.");
        public static Error InvalidMeasurementUnit => new Error("SPI.InvalidMeasurementUnit", "The measurement unit is required.");
        public static Error InvalidMeasurementFrequency => new Error("SPI.InvalidMeasurementFrequency", "The measurement frequency is invalid.");
        public static Error InvalidTargetValue => new Error("SPI.InvalidTargetValue", "The target value must be greater than 0.");
        public static Error InvalidThresholdValues => new Error("SPI.InvalidThresholdValues", "Warning threshold must be less than critical threshold.");
        public static Error InvalidDataSource => new Error("SPI.InvalidDataSource", "The data source information is required.");
        public static Error InvalidReviewDate => new Error("SPI.InvalidReviewDate", "The review date must be in the future.");
        public static Error InvalidDataPoint => new Error("SPI.InvalidDataPoint", "The data point value is invalid.");
        public static Error InvalidPeriod => new Error("SPI.InvalidPeriod", "The measurement period is invalid for the frequency.");

        public static Error NotFound => new Error("SPI.NotFound", "The Safety Performance Indicator was not found.");
        public static Error CreateFailed => new Error("SPI.CreateFailed", "Failed to create the Safety Performance Indicator.");
        public static Error UpdateFailed => new Error("SPI.UpdateFailed", "Failed to update the Safety Performance Indicator.");
        public static Error DeleteFailed => new Error("SPI.DeleteFailed", "Failed to delete the Safety Performance Indicator.");
        public static Error StatusUpdateFailed => new Error("SPI.StatusUpdateFailed", "Failed to update the SPI status.");
        public static Error TargetUpdateFailed => new Error("SPI.TargetUpdateFailed", "Failed to update the SPI targets.");
        public static Error DataPointAddFailed => new Error("SPI.DataPointAddFailed", "Failed to add data point to the SPI.");
        public static Error ReviewScheduleFailed => new Error("SPI.ReviewScheduleFailed", "Failed to schedule SPI review.");
        public static Error ReviewCompletionFailed => new Error("SPI.ReviewCompletionFailed", "Failed to complete SPI review.");

        public static Error CannotModifyInactive => new Error("SPI.CannotModifyInactive", "Cannot modify an inactive Safety Performance Indicator.");
        public static Error AlreadyExists => new Error("SPI.AlreadyExists", "A Safety Performance Indicator with this code already exists.");
        public static Error DependentDataExists => new Error("SPI.DependentDataExists", "Cannot delete SPI that has associated data points.");
        public static Error DuplicateDataPoint => new Error("SPI.DuplicateDataPoint", "A data point for this period already exists.");
        public static Error ThresholdExceeded => new Error("SPI.ThresholdExceeded", "The data value exceeds the critical threshold.");
        public static Error InsufficientData => new Error("SPI.InsufficientData", "Insufficient data points for trend analysis.");
        public static Error AlertConfigurationFailed => new Error("SPI.AlertConfigurationFailed", "Failed to configure SPI alerts.");
        public static Error DataValidationFailed => new Error("SPI.DataValidationFailed", "Data validation failed for the SPI measurement.");

        // NEW: SPI Automation Errors
        public static Error AutomationFailed => new Error("SPI.AutomationFailed", "Failed to execute automated SPI calculation.");
        public static Error EventProcessingFailed => new Error("SPI.EventProcessingFailed", "Failed to process SPI automation event.");
        public static Error CalculationServiceUnavailable => new Error("SPI.CalculationServiceUnavailable", "SPI calculation service is currently unavailable.");
        public static Error BackgroundJobFailed => new Error("SPI.BackgroundJobFailed", "SPI background calculation job failed.");
        public static Error DataSourceQueryFailed => new Error("SPI.DataSourceQueryFailed", "Failed to query data source for SPI calculation.");
    }

    /// <summary>
    /// Contains SMS Audit Plan related errors.
    /// </summary>
    public static class SMSAuditPlanError
    {
        public static Error NullOrEmpty => new Error("SMSAuditPlan.NullOrEmpty", "The SMS Audit Plan is required.");
        public static Error CodeRequired => new Error("SMSAuditPlan.CodeRequired", "The SMS Audit Plan Code is required.");
        public static Error NameRequired => new Error("SMSAuditPlan.NameRequired", "The SMS Audit Plan Name is required.");
        public static Error AuditTypeRequired => new Error("SMSAuditPlan.AuditTypeRequired", "The Audit Type is required.");
        public static Error LeadAuditorRequired => new Error("SMSAuditPlan.LeadAuditorRequired", "The Lead Auditor is required.");
        public static Error InvalidPlannedDates => new Error("SMSAuditPlan.InvalidPlannedDates", "The planned start date must be before the planned end date.");
        public static Error InvalidStatus => new Error("SMSAuditPlan.InvalidStatus", "The SMS Audit Plan Status is invalid.");
        public static Error InvalidAuditType => new Error("SMSAuditPlan.InvalidAuditType", "The Audit Type is invalid.");
        public static Error InvalidCode => new Error("SMSAuditPlan.InvalidCode", "The SMS Audit Plan Code is invalid.");
        public static Error NotFound => new Error("SMSAuditPlan.NotFound", "The SMS Audit Plan was not found.");
        public static Error CreateFailed => new Error("SMSAuditPlan.CreateFailed", "Failed to create the SMS Audit Plan.");
        public static Error UpdateFailed => new Error("SMSAuditPlan.UpdateFailed", "Failed to update the SMS Audit Plan.");
        public static Error DeleteFailed => new Error("SMSAuditPlan.DeleteFailed", "Failed to delete the SMS Audit Plan.");
        public static Error AlreadyApproved => new Error("SMSAuditPlan.AlreadyApproved", "The SMS Audit Plan is already approved.");
        public static Error ApprovalRequired => new Error("SMSAuditPlan.ApprovalRequired", "The SMS Audit Plan must be approved before scheduling.");
        public static Error CannotModifyApproved => new Error("SMSAuditPlan.CannotModifyApproved", "Cannot modify an approved SMS Audit Plan.");
        public static Error SchedulingFailed => new Error("SMSAuditPlan.SchedulingFailed", "Failed to schedule audit from the audit plan.");
        public static Error InvalidScheduleDate => new Error("SMSAuditPlan.InvalidScheduleDate", "Cannot schedule audit in the past.");
        public static Error StatusUpdateFailed => new Error("SMSAuditPlan.StatusUpdateFailed", "Failed to update the SMS Audit Plan status.");
    }

    /// <summary>
    /// Contains SMS Audit related errors.
    /// </summary>
    public static class SMSAuditError
    {
        public static Error NullOrEmpty => new Error("SMSAudit.NullOrEmpty", "The SMS Audit is required.");
        public static Error CodeRequired => new Error("SMSAudit.CodeRequired", "The SMS Audit Code is required.");
        public static Error NameRequired => new Error("SMSAudit.NameRequired", "The SMS Audit Name is required.");
        public static Error AuditTypeRequired => new Error("SMSAudit.AuditTypeRequired", "The Audit Type is required.");
        public static Error LeadAuditorRequired => new Error("SMSAudit.LeadAuditorRequired", "The Lead Auditor is required.");
        public static Error InvalidScheduledDates => new Error("SMSAudit.InvalidScheduledDates", "The scheduled start date must be before the scheduled end date.");
        public static Error InvalidActualDates => new Error("SMSAudit.InvalidActualDates", "The actual start date must be before the actual end date.");
        public static Error InvalidStatus => new Error("SMSAudit.InvalidStatus", "The SMS Audit Status is invalid.");
        public static Error InvalidAuditType => new Error("SMSAudit.InvalidAuditType", "The Audit Type is invalid.");
        public static Error InvalidCurrentPhase => new Error("SMSAudit.InvalidCurrentPhase", "The Current Phase is invalid.");
        public static Error InvalidCode => new Error("SMSAudit.InvalidCode", "The SMS Audit Code is invalid.");
        public static Error NotFound => new Error("SMSAudit.NotFound", "The SMS Audit was not found.");
        public static Error CreateFailed => new Error("SMSAudit.CreateFailed", "Failed to create the SMS Audit.");
        public static Error UpdateFailed => new Error("SMSAudit.UpdateFailed", "Failed to update the SMS Audit.");
        public static Error DeleteFailed => new Error("SMSAudit.DeleteFailed", "Failed to delete the SMS Audit.");
        public static Error StartFailed => new Error("SMSAudit.StartFailed", "Failed to start the SMS Audit.");
        public static Error CompletionFailed => new Error("SMSAudit.CompletionFailed", "Failed to complete the SMS Audit.");
        public static Error CannotStartNonScheduled => new Error("SMSAudit.CannotStartNonScheduled", "Cannot start a non-scheduled audit.");
        public static Error CannotCompleteNonStarted => new Error("SMSAudit.CannotCompleteNonStarted", "Cannot complete an audit that has not been started.");
        public static Error CannotModifyCompleted => new Error("SMSAudit.CannotModifyCompleted", "Cannot modify a completed SMS Audit.");
        public static Error AlreadyStarted => new Error("SMSAudit.AlreadyStarted", "The SMS Audit has already been started.");
        public static Error AlreadyCompleted => new Error("SMSAudit.AlreadyCompleted", "The SMS Audit has already been completed.");
        public static Error AddFindingFailed => new Error("SMSAudit.AddFindingFailed", "Failed to add finding to the SMS Audit.");
        public static Error StatusUpdateFailed => new Error("SMSAudit.StatusUpdateFailed", "Failed to update the SMS Audit status.");
    }

    /// <summary>
    /// Contains SMS Audit Finding related errors.
    /// </summary>
    public static class SMSAuditFindingError
    {
        public static Error NullOrEmpty => new Error("SMSAuditFinding.NullOrEmpty", "The SMS Audit Finding is required.");
        public static Error CodeRequired => new Error("SMSAuditFinding.CodeRequired", "The SMS Audit Finding Code is required.");
        public static Error AuditCodeRequired => new Error("SMSAuditFinding.AuditCodeRequired", "The Audit Code is required.");
        public static Error DescriptionRequired => new Error("SMSAuditFinding.DescriptionRequired", "The Finding Description is required.");
        public static Error SeverityRequired => new Error("SMSAuditFinding.SeverityRequired", "The Severity is required.");
        public static Error ResponsiblePersonRequired => new Error("SMSAuditFinding.ResponsiblePersonRequired", "The Responsible Person is required for corrective actions.");
        public static Error TargetDateRequired => new Error("SMSAuditFinding.TargetDateRequired", "The Target Completion Date is required for corrective actions.");
        public static Error InvalidSeverity => new Error("SMSAuditFinding.InvalidSeverity", "The Severity level is invalid.");
        public static Error InvalidStatus => new Error("SMSAuditFinding.InvalidStatus", "The SMS Audit Finding Status is invalid.");
        public static Error InvalidFindingType => new Error("SMSAuditFinding.InvalidFindingType", "The Finding Type is invalid.");
        public static Error InvalidTargetDate => new Error("SMSAuditFinding.InvalidTargetDate", "The Target Completion Date cannot be in the past.");
        public static Error InvalidCode => new Error("SMSAuditFinding.InvalidCode", "The SMS Audit Finding Code is invalid.");
        public static Error NotFound => new Error("SMSAuditFinding.NotFound", "The SMS Audit Finding was not found.");
        public static Error CreateFailed => new Error("SMSAuditFinding.CreateFailed", "Failed to create the SMS Audit Finding.");
        public static Error UpdateFailed => new Error("SMSAuditFinding.UpdateFailed", "Failed to update the SMS Audit Finding.");
        public static Error DeleteFailed => new Error("SMSAuditFinding.DeleteFailed", "Failed to delete the SMS Audit Finding.");
        public static Error AssignmentFailed => new Error("SMSAuditFinding.AssignmentFailed", "Failed to assign corrective action.");
        public static Error CompletionFailed => new Error("SMSAuditFinding.CompletionFailed", "Failed to complete corrective action.");
        public static Error VerificationFailed => new Error("SMSAuditFinding.VerificationFailed", "Failed to verify the finding.");
        public static Error CannotModifyCompleted => new Error("SMSAuditFinding.CannotModifyCompleted", "Cannot modify a completed finding.");
        public static Error CannotVerifyIncomplete => new Error("SMSAuditFinding.CannotVerifyIncomplete", "Cannot verify an incomplete finding.");
        public static Error CannotAssignToOpen => new Error("SMSAuditFinding.CannotAssignToOpen", "Can only assign corrective actions to open findings.");
        public static Error StatusUpdateFailed => new Error("SMSAuditFinding.StatusUpdateFailed", "Failed to update the finding status.");
    }

    /// <summary>
    /// Contains SMS Audit Evidence related errors.
    /// </summary>
    public static class SMSAuditEvidenceError
    {
        public static Error NullOrEmpty => new Error("SMSAuditEvidence.NullOrEmpty", "The SMS Audit Evidence is required.");
        public static Error CodeRequired => new Error("SMSAuditEvidence.CodeRequired", "The SMS Audit Evidence Code is required.");
        public static Error AuditCodeRequired => new Error("SMSAuditEvidence.AuditCodeRequired", "The Audit Code is required.");
        public static Error TitleRequired => new Error("SMSAuditEvidence.TitleRequired", "The Evidence Title is required.");
        public static Error DescriptionRequired => new Error("SMSAuditEvidence.DescriptionRequired", "The Evidence Description is required.");
        public static Error EvidenceTypeRequired => new Error("SMSAuditEvidence.EvidenceTypeRequired", "The Evidence Type is required.");
        public static Error CollectedByRequired => new Error("SMSAuditEvidence.CollectedByRequired", "The Collected By field is required.");
        public static Error InvalidEvidenceType => new Error("SMSAuditEvidence.InvalidEvidenceType", "The Evidence Type is invalid.");
        public static Error InvalidConfidentialityLevel => new Error("SMSAuditEvidence.InvalidConfidentialityLevel", "The Confidentiality Level is invalid.");
        public static Error InvalidRetentionPeriod => new Error("SMSAuditEvidence.InvalidRetentionPeriod", "The Retention Period must be greater than 0.");
        public static Error InvalidFileSize => new Error("SMSAuditEvidence.InvalidFileSize", "The File Size must be greater than 0 for file evidence.");
        public static Error FileDataRequired => new Error("SMSAuditEvidence.FileDataRequired", "File data is required for file-based evidence.");
        public static Error InvalidCode => new Error("SMSAuditEvidence.InvalidCode", "The SMS Audit Evidence Code is invalid.");
        public static Error NotFound => new Error("SMSAuditEvidence.NotFound", "The SMS Audit Evidence was not found.");
        public static Error CreateFailed => new Error("SMSAuditEvidence.CreateFailed", "Failed to create the SMS Audit Evidence.");
        public static Error UpdateFailed => new Error("SMSAuditEvidence.UpdateFailed", "Failed to update the SMS Audit Evidence.");
        public static Error DeleteFailed => new Error("SMSAuditEvidence.DeleteFailed", "Failed to delete the SMS Audit Evidence.");
        public static Error VerificationFailed => new Error("SMSAuditEvidence.VerificationFailed", "Failed to verify the evidence.");
        public static Error ArchivingFailed => new Error("SMSAuditEvidence.ArchivingFailed", "Failed to archive the evidence.");
        public static Error LinkingFailed => new Error("SMSAuditEvidence.LinkingFailed", "Failed to link evidence to finding.");
        public static Error CannotModifyArchived => new Error("SMSAuditEvidence.CannotModifyArchived", "Cannot modify archived evidence.");
        public static Error AlreadyArchived => new Error("SMSAuditEvidence.AlreadyArchived", "The evidence is already archived.");
        public static Error AlreadyVerified => new Error("SMSAuditEvidence.AlreadyVerified", "The evidence is already verified.");
        public static Error StorageLocationRequired => new Error("SMSAuditEvidence.StorageLocationRequired", "The Storage Location is required for external file references.");
    }

    /// <summary>
    /// Contains SMS Audit Checklist Item related errors.
    /// </summary>
    public static class SMSAuditChecklistItemError
    {
        public static Error NullOrEmpty => new Error("SMSAuditChecklistItem.NullOrEmpty", "The SMS Audit Checklist Item is required.");
        public static Error CodeRequired => new Error("SMSAuditChecklistItem.CodeRequired", "The SMS Audit Checklist Item Code is required.");
        public static Error AuditCodeRequired => new Error("SMSAuditChecklistItem.AuditCodeRequired", "The Audit Code is required.");
        public static Error CategoryRequired => new Error("SMSAuditChecklistItem.CategoryRequired", "The Category is required.");
        public static Error DescriptionRequired => new Error("SMSAuditChecklistItem.DescriptionRequired", "The Description is required.");
        public static Error CreatedByRequired => new Error("SMSAuditChecklistItem.CreatedByRequired", "The Created By field is required.");
        public static Error InvalidItemNumber => new Error("SMSAuditChecklistItem.InvalidItemNumber", "The Item Number must be greater than zero.");
        public static Error InvalidStatus => new Error("SMSAuditChecklistItem.InvalidStatus", "The Status is invalid.");
        public static Error InvalidCode => new Error("SMSAuditChecklistItem.InvalidCode", "The SMS Audit Checklist Item Code is invalid.");
        public static Error NotFound => new Error("SMSAuditChecklistItem.NotFound", "The SMS Audit Checklist Item was not found.");
        public static Error CreateFailed => new Error("SMSAuditChecklistItem.CreateFailed", "Failed to create the SMS Audit Checklist Item.");
        public static Error UpdateFailed => new Error("SMSAuditChecklistItem.UpdateFailed", "Failed to update the SMS Audit Checklist Item.");
        public static Error DeleteFailed => new Error("SMSAuditChecklistItem.DeleteFailed", "Failed to delete the SMS Audit Checklist Item.");
        public static Error AlreadyCompleted => new Error("SMSAuditChecklistItem.AlreadyCompleted", "Cannot change status of completed item.");
        public static Error NotApplicable => new Error("SMSAuditChecklistItem.NotApplicable", "Cannot mark N/A item as in progress.");
        public static Error Skipped => new Error("SMSAuditChecklistItem.Skipped", "Cannot complete skipped item.");
        public static Error RequiredItem => new Error("SMSAuditChecklistItem.RequiredItem", "Cannot mark required item as N/A or skip.");
        public static Error ReasonRequired => new Error("SMSAuditChecklistItem.ReasonRequired", "A reason is required for this action.");
        public static Error NotCompleted => new Error("SMSAuditChecklistItem.NotCompleted", "Can only review completed items.");
        public static Error CannotUpdateCompleted => new Error("SMSAuditChecklistItem.CannotUpdateCompleted", "Cannot update completed checklist item.");
        public static Error CannotResetRequired => new Error("SMSAuditChecklistItem.CannotResetRequired", "Cannot reset required item from N/A.");
        public static Error StatusUpdateFailed => new Error("SMSAuditChecklistItem.StatusUpdateFailed", "Failed to update checklist item status.");
        public static Error CompletionFailed => new Error("SMSAuditChecklistItem.CompletionFailed", "Failed to complete checklist item.");
        public static Error ReviewFailed => new Error("SMSAuditChecklistItem.ReviewFailed", "Failed to review checklist item.");
    }

    /// <summary>
    /// Contains Hazard Report Tracking related errors.
    /// </summary>
    public static class HazardReportTrackingError
    {
        public static Error NullOrEmpty => new Error("HazardReportTracking.NullOrEmpty", "The Hazard Report Tracking is required.");
        public static Error CodeRequired => new Error("HazardReportTracking.CodeRequired", "The Tracking Code is required.");
        public static Error HazardCodeRequired => new Error("HazardReportTracking.HazardCodeRequired", "The Hazard Code is required.");
        public static Error ReportCodeRequired => new Error("HazardReportTracking.ReportCodeRequired", "The Report Code is required.");
        public static Error TrackingCodeRequired => new Error("HazardReportTracking.TrackingCodeRequired", "The Tracking Code is required.");
        public static Error InvalidCode => new Error("HazardReportTracking.InvalidCode", "The Tracking Code is invalid.");
        public static Error InvalidTrackingCodeFormat => new Error("HazardReportTracking.InvalidTrackingCodeFormat", "The Tracking Code format is invalid. Expected format: HRT-YYYYMMDD-HHMMSS-XXXX");
        public static Error InvalidHazardCode => new Error("HazardReportTracking.InvalidHazardCode", "The Hazard Code is invalid.");
        public static Error InvalidReportCode => new Error("HazardReportTracking.InvalidReportCode", "The Report Code is invalid.");
        public static Error NotFound => new Error("HazardReportTracking.NotFound", "The Hazard Report Tracking record was not found.");
        public static Error CreateFailed => new Error("HazardReportTracking.CreateFailed", "Failed to create the Hazard Report Tracking record.");
        public static Error UpdateFailed => new Error("HazardReportTracking.UpdateFailed", "Failed to update the Hazard Report Tracking record.");
        public static Error DeleteFailed => new Error("HazardReportTracking.DeleteFailed", "Failed to delete the Hazard Report Tracking record.");
        public static Error TrackingCodeGenerationFailed => new Error("HazardReportTracking.TrackingCodeGenerationFailed", "Failed to generate unique tracking code.");
        public static Error DuplicateTrackingCode => new Error("HazardReportTracking.DuplicateTrackingCode", "A tracking record with this code already exists.");
        public static Error TrackingCodeExpired => new Error("HazardReportTracking.TrackingCodeExpired", "The tracking code has expired and is no longer valid.");
        public static Error InvalidStatusTransition => new Error("HazardReportTracking.InvalidStatusTransition", "The status transition is not allowed.");
        public static Error StatusUpdateFailed => new Error("HazardReportTracking.StatusUpdateFailed", "Failed to update the tracking status.");
        public static Error ProcessingStageUpdateFailed => new Error("HazardReportTracking.ProcessingStageUpdateFailed", "Failed to update the processing stage.");
        public static Error RelatedHazardNotFound => new Error("HazardReportTracking.RelatedHazardNotFound", "The related hazard was not found.");
        public static Error RelatedReportNotFound => new Error("HazardReportTracking.RelatedReportNotFound", "The related report was not found.");
        public static Error TrackingCodeNotActive => new Error("HazardReportTracking.TrackingCodeNotActive", "The tracking code is not active for status updates.");
        public static Error CannotModifyCompletedTracking => new Error("HazardReportTracking.CannotModifyCompletedTracking", "Cannot modify a completed tracking record.");
        public static Error InvalidProcessingStage => new Error("HazardReportTracking.InvalidProcessingStage", "The processing stage is invalid.");
        public static Error InvalidCurrentStatus => new Error("HazardReportTracking.InvalidCurrentStatus", "The current status is invalid.");
    }
}

