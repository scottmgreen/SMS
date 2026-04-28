//-----------------------------------------------------------------------
// <copyright file="AddAuditLogEntryCommandHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer component providing functionality for the SMS safety management system.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------



namespace SMS_Application.Messaging.CommandHandlers;

public class AddAuditLogEntryCommandHandler : BaseCommandBundle, IBaseRequestHandler<AddAuditLogEntryCommand, Result<bool>>
{
    public AddAuditLogEntryCommandHandler(SystemDataService dataService)
    {
        _dataService = dataService;
    }
    private readonly SystemDataService _dataService;
    public Task<Result<bool>> HandleAsync(AddAuditLogEntryCommand request, CancellationToken ct = default)
    {
        var result = _dataService.AddAuditLogEntryAsync(request.AuditLogEntry, ct);
        return result;
    }


}

