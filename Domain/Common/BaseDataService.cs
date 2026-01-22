using Microsoft.Extensions.DependencyInjection;

namespace SMS_Domain.Common;

public abstract class BaseDataService<TEntity>
where TEntity : class


{
    // Implement the interface methods here
    private readonly ILogger<TEntity> _logger;
    private readonly string _logheader = "BaseDataService";
    public string LogHeader => _logheader;
    public ILogger<TEntity> Logger => _logger;
    // private UserDetails UserDetails { get; set; }
    private IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;


    protected IConfiguration Configuration => _configuration;


    public BaseDataService(ILogger<TEntity> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        this._logger = logger;
        this._serviceScopeFactory = serviceScopeFactory;
        //using (var scope = _serviceScopeFactory.CreateScope())
        //{
        //    var userDetailsFactory = scope.ServiceProvider.GetRequiredService<IUserDetailsFactory>();
        //    UserDetails = userDetailsFactory.GetUserDetails();
        //}
        //this._logheader = LogSupport.GenerateLogHeader(UserDetails);


        this._configuration = configuration;

    }
    //public abstract Task<List<TModel>> GetAllAsync();
    //public abstract Task<TModel> GetByIdAsync(object id);
    //public abstract Task<TModel> CreateAsync(TModel model);
    //public abstract Task<TModel> UpdateAsync(object id, TModel model);
    //public abstract Task<bool> DeleteAsync(object id);


}
