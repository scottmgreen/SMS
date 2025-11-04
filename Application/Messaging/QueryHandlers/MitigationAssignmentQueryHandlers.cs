using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// MITIGATION ASSIGNMENT QUERY HANDLERS
// =============================================

public class GetMitigationAssignmentByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetMitigationAssignmentByIdQuery, Result<MitigationAssignment>>
{
    private readonly MitigationAssignmentDataService _mitigationAssignmentDataService;
    private readonly ILogger<GetMitigationAssignmentByIdQueryHandler> _logger;

    public GetMitigationAssignmentByIdQueryHandler(MitigationAssignmentDataService mitigationAssignmentDataService, ILogger<GetMitigationAssignmentByIdQueryHandler> logger)
    {
        _mitigationAssignmentDataService = mitigationAssignmentDataService ?? throw new ArgumentNullException(nameof(mitigationAssignmentDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<MitigationAssignment>> HandleAsync(GetMitigationAssignmentByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetMitigationAssignmentByIdQuery for ID: {Id}", request.MitigationAssignmentId);
            var result = await _mitigationAssignmentDataService.GetMitigationAssignmentByIdAsync(request.MitigationAssignmentId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetMitigationAssignmentByIdQuery for ID: {Id}", request.MitigationAssignmentId);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NotFound);
        }
    }
}

public class GetAllMitigationAssignmentsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllMitigationAssignmentsQuery, Result<List<MitigationAssignment>>>
{
    private readonly MitigationAssignmentDataService _mitigationAssignmentDataService;
    private readonly ILogger<GetAllMitigationAssignmentsQueryHandler> _logger;

    public GetAllMitigationAssignmentsQueryHandler(MitigationAssignmentDataService mitigationAssignmentDataService, ILogger<GetAllMitigationAssignmentsQueryHandler> logger)
    {
        _mitigationAssignmentDataService = mitigationAssignmentDataService ?? throw new ArgumentNullException(nameof(mitigationAssignmentDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<MitigationAssignment>>> HandleAsync(GetAllMitigationAssignmentsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllMitigationAssignmentsQuery");
            var result = await _mitigationAssignmentDataService.GetAllMitigationAssignmentsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllMitigationAssignmentsQuery");
            return Result<List<MitigationAssignment>>.Failure<List<MitigationAssignment>>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
        }
    }
}