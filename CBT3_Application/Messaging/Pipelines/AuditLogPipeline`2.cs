using Microsoft.FeatureManagement;

namespace CBT3_Application.Messaging.Pipelines;

public class AuditLogPipeline<TRequest, TResult> : IPipeline<TRequest, TResult> where TRequest : IRequest<TResult> where TResult : Result
{

    private readonly SystemService _systemService;
    private readonly IFeatureManager _featureManager;

    public AuditLogPipeline(IFeatureManager featureManager, SystemService systemService)
    {
        _systemService = systemService;
        _featureManager = featureManager;

    }
    public async Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();

        //Execute command Handler
        var result = await next().ConfigureAwait(true); 

        // Log after executing the command handler

        if (await _featureManager.IsEnabledAsync("AuditLogEnabled"))
        {

            AuditLogEntryID logentryId = new(Guid.NewGuid().ToString());
            AuditLogEntry logentry = new(logentryId);
            logentry.UserID = "SYSTEM";
            logentry.Workstation = "localhost";
            logentry.Module = request.GetType().Namespace ?? "";
            logentry.MessageType = "Pipeline";
            logentry.Description = request.GetType().Module.ToString() ?? "";
            logentry.EventDateTime = DateTime.Now.ToString("MM_dd_yyyy_hh:mm:ss");
            logentry.Function = request.GetType().Name;

            if (result.IsSuccess)
            {
                logentry.Severity = "CBT3_ApplicationEventIds.Information";
            }
            else
            {
                logentry.Severity = "CBT3_ApplicationEventIds.Critical";
            }

            _systemService.AddAuditLogEntryAsync(logentry, cancellation);
        }


        return result;
    }
}



