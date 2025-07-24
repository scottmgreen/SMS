

namespace CBT3_Application.Messaging.CommandHandlers;

public class AddAuditLogEntryCommandHandler : BaseCommandBundle, IRequestHandler<AddAuditLogEntryCommand, Result<bool>>
{
    public AddAuditLogEntryCommandHandler(SystemDataService dataService)
    {
        _dataService = dataService;
    }
    private readonly SystemDataService _dataService;
    public Task<Result<bool>> HandleAsync(AddAuditLogEntryCommand request, CancellationToken ct = default)
    {
        var result = _dataService.AddAuditLogEntryAsync(request.AuditLogEntry,ct);
        return result;
    }

    
}
