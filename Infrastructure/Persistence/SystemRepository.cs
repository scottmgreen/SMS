using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;

using Microsoft.FeatureManagement;

namespace SMS_Infrastructure.Persistence;

public sealed class SystemRepository : BaseRepository<SystemRepository, AuditLogEntry>
{
    private readonly ILogger<SystemRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;
    private readonly IFeatureManager _featureManager;

    public SystemRepository(ILogger<SystemRepository> logger, ILogSupport logsupport, IConfiguration configuration, IFeatureManager featureManager) : base(logger, logsupport, configuration)
    {
        _configuration = configuration;
        _featureManager = featureManager;
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader}");
    }

    public async Task<Result<bool>> AddAuditLogEntryAsync(AuditLogEntry auditLogEntry, CancellationToken ct = default)
    {

        try
        {
            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.cn_spAddAuditLogEntry}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spAddAuditLogEntry, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };


            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserID, auditLogEntry.UserID));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMessageType, auditLogEntry.MessageType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSeverity, auditLogEntry.Severity));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmModule, auditLogEntry.Module));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFunction, auditLogEntry.Function));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmDescription, auditLogEntry.Description));
            //cmd.Parameters.Add(DataAccess.Parameter("@ReturnVal", course.Id));


            await sql.OpenAsync(ct).ConfigureAwait(false);
            int rowsAffected = (int)await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            bool success = rowsAffected > 0;

            if (success)
                return Result<bool>.Success(true);
            else
                return Result<bool>.Failure<bool>(DomainErrors.SystemError.AuditLogEntryError);// No rows were affected
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SystemError.AuditLogEntryError);
        }
    }

    public Task<Result<int>> GetAdminPasscodeAsync()
    {
        _logger.LogInfrastructureGetItem($"{_logheader} Get Admin Passcode", null);
        return Task.FromResult<Result<int>>(Convert.ToInt32(_configuration["AdminPasscode"]));
    }

    public async Task<Result<bool>> GetFeatureEnabledAsync(string featurename)
    {
        var result = await _featureManager.IsEnabledAsync(featurename);

        return Task.FromResult<Result<bool>>(result).Result;
    }


}

