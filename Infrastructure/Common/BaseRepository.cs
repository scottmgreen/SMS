using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace SMS_Infrastructure.Common;


public abstract class BaseRepository<TEntity, TModel> : IBaseRepository<TEntity, TModel>
 where TEntity : class
 where TModel : class
{
    private readonly ILogger<TEntity> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly string _logheader;
    public string ConnectionString => _connectionString;
    public string LogHeader => _logheader;
    public ILogger<TEntity> Logger => _logger;
    public IConfiguration Configuration => _configuration;
    private readonly ILogSupport _logsupport;

    public BaseRepository(ILogger<TEntity> logger, ILogSupport logsupport, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        // Use DI-injected configuration
        _connectionString = _configuration.GetConnectionString("DefaultConnectionString") ?? throw new InvalidOperationException("Database connection string is missing.");
        _logsupport = logsupport;
        _logheader = _logsupport.GenerateLogHeader();
                
    }

    //public abstract Task<List<TModel>> GetAllAsync();
    //public abstract Task<TModel> GetByIdAsync(string id);
    //public abstract Task<TModel> AddAsync(TModel model);
    //public abstract Task<bool> UpdateAsync(TModel model);
    //public abstract Task<bool> DeleteAsync(TModel model);
}
