//-----------------------------------------------------------------------
// <copyright file="ISMSOrganizationalUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS User management service handling user lifecycle and authentication operations.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

public interface ISMSOrganizationalUserService
{
    Task<Result<bool>> AuthenticateSMSOrganizationalUserAsync(string userName, string plainTextPassword, CancellationToken ct = default);
    Task<Result<SMSOrganizationalUser>> CreateSMSOrganizationalUserAsync(SMSOrganizationalUser user, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetAllSMSOrganizationalUsersAsync(CancellationToken ct = default);
    Task<Result<SMSOrganizationalUser>> GetSMSOrganizationalUserByCodeAsync(string id, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetSMSOrganizationalUsersByDepartmentAsync(string department, CancellationToken ct = default);
    Task<Result<SMSOrganizationalUser>> UpdateSMSOrganizationalUserAsync(SMSOrganizationalUser user, CancellationToken ct = default);
}
