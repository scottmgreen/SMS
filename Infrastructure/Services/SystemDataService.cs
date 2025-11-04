
using SMS_Domain.Interfaces;

using SMS_Infrastructure.Repositories;

namespace SMS_Infrastructure.Services
{
    public class SystemDataService : BaseDataService<SystemDataService>
    {
        private readonly ILogger<SystemDataService> _logger;
        private readonly string _logheader;
        private SystemRepository _repo;

        public SystemDataService(ILogger<SystemDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, SystemRepository repo) : base(logger, serviceScopeFactory, configuration)
        {
            _logger = base.Logger;
            _logheader = base.LogHeader;

            _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader}  {repo.GetType().Name} ");
            _repo = repo;
        }

      
        public Task<Result<bool>> AddAuditLogEntryAsync(AuditLogEntry auditlogentry, CancellationToken ct = default)
        {
            return _repo.AddAuditLogEntryAsync(auditlogentry,ct);
        }

        public Task<Result<int>> GetAdminPasscodeAsync()
        {
            return _repo.GetAdminPasscodeAsync();
        }

        public Task<Result<bool>> GetFeatureEnabledAsync(string featurename)
        {
            return _repo.GetFeatureEnabledAsync(featurename);
        }
    }
}
