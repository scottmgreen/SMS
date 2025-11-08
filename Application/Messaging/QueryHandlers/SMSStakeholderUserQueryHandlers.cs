using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;

using SMS_Infrastructure.Interfaces;

using SMS_Shared.Common;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SMS STAKEHOLDER USER QUERY HANDLERS
// =============================================

public class GetAllSMSStakeholderUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSStakeholderUsersQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetAllSMSStakeholderUsersQueryHandler> _logger;

    public GetAllSMSStakeholderUsersQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetAllSMSStakeholderUsersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetAllSMSStakeholderUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSStakeholderUsersQuery");
            var result = await _repository.GetAllAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Stakeholder Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Stakeholder Users: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllSMSStakeholderUsersQuery");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetSMSStakeholderUserByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUserByIdQuery, Result<SMSStakeholderUser>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetSMSStakeholderUserByIdQueryHandler> _logger;

    public GetSMSStakeholderUserByIdQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetSMSStakeholderUserByIdQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(GetSMSStakeholderUserByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSStakeholderUserByIdQuery for ID: {UserId}", request.UserId);
            var result = await _repository.GetByIdAsync(request.UserId);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Stakeholder User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogWarning("SMS Stakeholder User not found with ID: {UserId}", request.UserId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSStakeholderUserByIdQuery for ID: {UserId}", request.UserId);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetSMSStakeholderUserByUserNameQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUserByUserNameQuery, Result<SMSStakeholderUser>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetSMSStakeholderUserByUserNameQueryHandler> _logger;

    public GetSMSStakeholderUserByUserNameQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetSMSStakeholderUserByUserNameQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(GetSMSStakeholderUserByUserNameQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSStakeholderUserByUserNameQuery for UserName: {UserName}", request.UserName);
            var result = await _repository.GetByUserNameAsync(request.UserName);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Stakeholder User with UserName: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("SMS Stakeholder User not found with UserName: {UserName}", request.UserName);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSStakeholderUserByUserNameQuery for UserName: {UserName}", request.UserName);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetActiveSMSStakeholderUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetActiveSMSStakeholderUsersQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetActiveSMSStakeholderUsersQueryHandler> _logger;

    public GetActiveSMSStakeholderUsersQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetActiveSMSStakeholderUsersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetActiveSMSStakeholderUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetActiveSMSStakeholderUsersQuery");
            var result = await _repository.GetActiveUsersAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} active SMS Stakeholder Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve active SMS Stakeholder Users: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetActiveSMSStakeholderUsersQuery");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetSMSStakeholderUsersByTypeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUsersByTypeQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetSMSStakeholderUsersByTypeQueryHandler> _logger;

    public GetSMSStakeholderUsersByTypeQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetSMSStakeholderUsersByTypeQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetSMSStakeholderUsersByTypeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSStakeholderUsersByTypeQuery for Type: {StakeholderType}", request.StakeholderType);
            var result = await _repository.GetByStakeholderTypeAsync(request.StakeholderType);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Stakeholder Users for Type: {StakeholderType}", 
                    result.Value?.Count() ?? 0, request.StakeholderType);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Stakeholder Users for Type: {StakeholderType}: {Error}", 
                    request.StakeholderType, result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSStakeholderUsersByTypeQuery for Type: {StakeholderType}", request.StakeholderType);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetSMSStakeholderUsersByOrganizationQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUsersByOrganizationQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetSMSStakeholderUsersByOrganizationQueryHandler> _logger;

    public GetSMSStakeholderUsersByOrganizationQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetSMSStakeholderUsersByOrganizationQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetSMSStakeholderUsersByOrganizationQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSStakeholderUsersByOrganizationQuery for Organization: {Organization}", request.Organization);
            var result = await _repository.GetByOrganizationAsync(request.Organization);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Stakeholder Users for Organization: {Organization}", 
                    result.Value?.Count() ?? 0, request.Organization);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Stakeholder Users for Organization: {Organization}: {Error}", 
                    request.Organization, result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSStakeholderUsersByOrganizationQuery for Organization: {Organization}", request.Organization);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetAirlineStakeholdersQueryHandler : BaseQueryBundle, IRequestHandler<GetAirlineStakeholdersQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetAirlineStakeholdersQueryHandler> _logger;

    public GetAirlineStakeholdersQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetAirlineStakeholdersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetAirlineStakeholdersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAirlineStakeholdersQuery");
            var result = await _repository.GetAirlineStakeholdersAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} airline stakeholders", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve airline stakeholders: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAirlineStakeholdersQuery");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetGroundHandlerStakeholdersQueryHandler : BaseQueryBundle, IRequestHandler<GetGroundHandlerStakeholdersQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetGroundHandlerStakeholdersQueryHandler> _logger;

    public GetGroundHandlerStakeholdersQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetGroundHandlerStakeholdersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetGroundHandlerStakeholdersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetGroundHandlerStakeholdersQuery");
            var result = await _repository.GetGroundHandlerStakeholdersAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} ground handler stakeholders", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve ground handler stakeholders: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetGroundHandlerStakeholdersQuery");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetContractorStakeholdersQueryHandler : BaseQueryBundle, IRequestHandler<GetContractorStakeholdersQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetContractorStakeholdersQueryHandler> _logger;

    public GetContractorStakeholdersQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetContractorStakeholdersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetContractorStakeholdersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetContractorStakeholdersQuery");
            var result = await _repository.GetContractorStakeholdersAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} contractor stakeholders", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve contractor stakeholders: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetContractorStakeholdersQuery");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class GetStakeholdersRequiringAOAAccessQueryHandler : BaseQueryBundle, IRequestHandler<GetStakeholdersRequiringAOAAccessQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetStakeholdersRequiringAOAAccessQueryHandler> _logger;

    public GetStakeholdersRequiringAOAAccessQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetStakeholdersRequiringAOAAccessQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetStakeholdersRequiringAOAAccessQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetStakeholdersRequiringAOAAccessQuery");
            var result = await _repository.GetUsersRequiringAOAAccessAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} stakeholders requiring AOA access", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve stakeholders requiring AOA access: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetStakeholdersRequiringAOAAccessQuery");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

public class CheckSMSStakeholderUserNameExistsQueryHandler : BaseQueryBundle, IRequestHandler<CheckSMSStakeholderUserNameExistsQuery, Result<bool>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<CheckSMSStakeholderUserNameExistsQueryHandler> _logger;

    public CheckSMSStakeholderUserNameExistsQueryHandler(ISMSStakeholderUserRepository repository, ILogger<CheckSMSStakeholderUserNameExistsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(CheckSMSStakeholderUserNameExistsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing CheckSMSStakeholderUserNameExistsQuery for UserName: {UserName}", request.UserName);
            var result = await _repository.UserNameExistsAsync(request.UserName);
            
            _logger.LogInformation("Username {UserName} exists: {Exists}", request.UserName, result.Value);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CheckSMSStakeholderUserNameExistsQuery for UserName: {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetStakeholderTypeStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetStakeholderTypeStatisticsQuery, Result<Dictionary<string, int>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetStakeholderTypeStatisticsQueryHandler> _logger;

    public GetStakeholderTypeStatisticsQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetStakeholderTypeStatisticsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Dictionary<string, int>>> HandleAsync(GetStakeholderTypeStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetStakeholderTypeStatisticsQuery");
            var result = await _repository.GetStakeholderTypeStatisticsAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved stakeholder type statistics for {Count} types", result.Value?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve stakeholder type statistics: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetStakeholderTypeStatisticsQuery");
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetStakeholderOrganizationStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetStakeholderOrganizationStatisticsQuery, Result<Dictionary<string, int>>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetStakeholderOrganizationStatisticsQueryHandler> _logger;

    public GetStakeholderOrganizationStatisticsQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetStakeholderOrganizationStatisticsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Dictionary<string, int>>> HandleAsync(GetStakeholderOrganizationStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetStakeholderOrganizationStatisticsQuery");
            var result = await _repository.GetOrganizationStatisticsAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved organization statistics for {Count} organizations", result.Value?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve organization statistics: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetStakeholderOrganizationStatisticsQuery");
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetSMSStakeholderUserStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUserStatisticsQuery, Result<UserStatistics>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<GetSMSStakeholderUserStatisticsQueryHandler> _logger;

    public GetSMSStakeholderUserStatisticsQueryHandler(ISMSStakeholderUserRepository repository, ILogger<GetSMSStakeholderUserStatisticsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UserStatistics>> HandleAsync(GetSMSStakeholderUserStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSStakeholderUserStatisticsQuery");
            var result = await _repository.GetUserStatisticsAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Stakeholder User statistics");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Stakeholder User statistics: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSStakeholderUserStatisticsQuery");
            return Result<UserStatistics>.Failure<UserStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}