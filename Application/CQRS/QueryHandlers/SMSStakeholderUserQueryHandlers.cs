//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for SMS Stakeholder User operations using CQRS pattern with MediatR.
//                  Application layer query handlers providing clean separation
//                  between presentation and business logic with validation.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

/// <summary>
/// Query Handlers for SMS Stakeholder User operations
/// </summary>
public class SMSStakeholderUserQueryHandlers
{
    // =============================================
    // SMS STAKEHOLDER USER QUERY HANDLERS
    // =============================================

    public class GetAllSMSStakeholderUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSStakeholderUsersQuery, Result<IEnumerable<SMSStakeholderUser>>>
    {
        private readonly SMSStakeholderUserService _dataService;
        private readonly ILogger<GetAllSMSStakeholderUsersQueryHandler> _logger;

        public GetAllSMSStakeholderUsersQueryHandler(SMSStakeholderUserService dataService, ILogger<GetAllSMSStakeholderUsersQueryHandler> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetAllSMSStakeholderUsersQuery request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Processing GetAllSMSStakeholderUsersQuery");
                var result = await _dataService.GetAllSMSStakeholderUsersAsync(ct);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully retrieved {Count} SMS Stakeholder Users", result.Value?.Count() ?? 0);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve SMS Stakeholder Users");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error processing GetAllSMSStakeholderUsersQuery", ApplicationEventIds.Error, ex);
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
    }


    public class GetSMSStakeholderUserByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUserByCodeQuery, Result<SMSStakeholderUser>>
    {
        private readonly SMSStakeholderUserDataService _dataService;
        private readonly ILogger<GetSMSStakeholderUserByCodeQueryHandler> _logger;

        public GetSMSStakeholderUserByCodeQueryHandler(SMSStakeholderUserDataService dataService, ILogger<GetSMSStakeholderUserByCodeQueryHandler> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<SMSStakeholderUser>> HandleAsync(GetSMSStakeholderUserByCodeQuery request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Processing GetSMSStakeholderUserByCodeQuery for Code: {UserCode}", request.UserCode);
                var result = await _dataService.GetByCodeAsync(request.UserCode, ct);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully retrieved SMS Stakeholder User with Code: {UserCode}", request.UserCode);
                }
                else
                {
                    _logger.LogWarning("SMS Stakeholder User not found with Code: {UserCode}", request.UserCode);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error processing GetSMSStakeholderUserByCodeQuery for Code: {UserCode}", ApplicationEventIds.Error, ex);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
    }

    public class GetSMSStakeholderUserByUserNameQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUserByUserNameQuery, Result<SMSStakeholderUser>>
    {
        private readonly SMSStakeholderUserDataService _dataService;
        private readonly ILogger<GetSMSStakeholderUserByUserNameQueryHandler> _logger;

        public GetSMSStakeholderUserByUserNameQueryHandler(SMSStakeholderUserDataService dataService, ILogger<GetSMSStakeholderUserByUserNameQueryHandler> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<SMSStakeholderUser>> HandleAsync(GetSMSStakeholderUserByUserNameQuery request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Processing GetSMSStakeholderUserByUserNameQuery for UserName: {UserName}", request.UserName);

                // Get all users and filter by username
                var allUsersResult = await _dataService.GetAllAsync(ct);
                if (allUsersResult.IsFailure)
                {
                    return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(allUsersResult.Error);
                }

                var user = allUsersResult.Value?.FirstOrDefault(u => u.UserName.Value.Equals(request.UserName, StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    _logger.LogInformation("Successfully retrieved SMS Stakeholder User with UserName: {UserName}", request.UserName);
                    return Result<SMSStakeholderUser>.Success(user);
                }
                else
                {
                    _logger.LogWarning("SMS Stakeholder User not found with UserName: {UserName}", request.UserName);
                    return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
                }
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error processing GetSMSStakeholderUserByUserNameQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
    }

    public class GetActiveSMSStakeholderUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetActiveSMSStakeholderUsersQuery, Result<IEnumerable<SMSStakeholderUser>>>
    {
        private readonly SMSStakeholderUserDataService _dataService;
        private readonly ILogger<GetActiveSMSStakeholderUsersQueryHandler> _logger;

        public GetActiveSMSStakeholderUsersQueryHandler(SMSStakeholderUserDataService dataService, ILogger<GetActiveSMSStakeholderUsersQueryHandler> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetActiveSMSStakeholderUsersQuery request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Processing GetActiveSMSStakeholderUsersQuery");
                var result = await _dataService.GetActiveAsync(ct);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully retrieved {Count} active SMS Stakeholder Users", result.Value?.Count() ?? 0);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve active SMS Stakeholder Users");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error processing GetActiveSMSStakeholderUsersQuery", ApplicationEventIds.Error, ex);
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
    }

    public class GetSMSStakeholderUsersByTypeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUsersByTypeQuery, Result<IEnumerable<SMSStakeholderUser>>>
    {
        private readonly SMSStakeholderUserDataService _dataService;
        private readonly ILogger<GetSMSStakeholderUsersByTypeQueryHandler> _logger;

        public GetSMSStakeholderUsersByTypeQueryHandler(SMSStakeholderUserDataService dataService, ILogger<GetSMSStakeholderUsersByTypeQueryHandler> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetSMSStakeholderUsersByTypeQuery request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Processing GetSMSStakeholderUsersByTypeQuery for Type: {StakeholderType}", request.StakeholderType);

                // Filter users by stakeholder type through service
                var allUsersResult = await _dataService.GetAllAsync(ct);
                if (allUsersResult.IsFailure)
                {
                    return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(allUsersResult.Error);
                }

                var filteredUsers = allUsersResult.Value?.Where(u => u.StakeholderType.Equals(request.StakeholderType, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<SMSStakeholderUser>();

                _logger.LogInformation("Successfully retrieved {Count} SMS Stakeholder Users for Type: {StakeholderType}", filteredUsers.Count(), request.StakeholderType);
                return Result<IEnumerable<SMSStakeholderUser>>.Success(filteredUsers);
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error processing GetSMSStakeholderUsersByTypeQuery for Type: {StakeholderType}", ApplicationEventIds.Error, ex);
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
    }

    public class GetSMSStakeholderUsersByOrganizationQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUsersByOrganizationQuery, Result<IEnumerable<SMSStakeholderUser>>>
    {
        private readonly SMSStakeholderUserDataService _dataService;
        private readonly ILogger<GetSMSStakeholderUsersByOrganizationQueryHandler> _logger;

        public GetSMSStakeholderUsersByOrganizationQueryHandler(SMSStakeholderUserDataService dataService, ILogger<GetSMSStakeholderUsersByOrganizationQueryHandler> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetSMSStakeholderUsersByOrganizationQuery request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Processing GetSMSStakeholderUsersByOrganizationQuery for Organization: {Organization}", request.Organization);

                // Filter users by organization through service
                var allUsersResult = await _dataService.GetAllAsync(ct);
                if (allUsersResult.IsFailure)
                {
                    return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(allUsersResult.Error);
                }

                var filteredUsers = allUsersResult.Value?.Where(u => u.Organization.Equals(request.Organization, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<SMSStakeholderUser>();

                _logger.LogInformation("Successfully retrieved {Count} SMS Stakeholder Users for Organization: {Organization}", filteredUsers.Count(), request.Organization);
                return Result<IEnumerable<SMSStakeholderUser>>.Success(filteredUsers);
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error processing GetSMSStakeholderUsersByOrganizationQuery for Organization: {Organization}", ApplicationEventIds.Error, ex);
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
    }

    /// <summary>
    /// Query handler for getting SMS stakeholder users by group code
    /// </summary>
    public class GetSMSStakeholderUsersByGroupCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderUsersByGroupCodeQuery, Result<IEnumerable<SMSStakeholderUser>>>
    {
        private readonly SMSStakeholderUserDataService _dataService;
        private readonly ILogger<GetSMSStakeholderUsersByGroupCodeQueryHandler> _logger;

        public GetSMSStakeholderUsersByGroupCodeQueryHandler(SMSStakeholderUserDataService dataService, ILogger<GetSMSStakeholderUsersByGroupCodeQueryHandler> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetSMSStakeholderUsersByGroupCodeQuery request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Processing GetSMSStakeholderUsersByGroupCodeQuery for GroupCode: {GroupCode}", request.GroupCode);

                // Call the proper data service method to get users by group code
                var result = await _dataService.GetSMSStakeholderUsersByGroupCodeAsync(request.GroupCode, ct);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully retrieved {Count} SMS Stakeholder Users for GroupCode: {GroupCode}",
                        result.Value?.Count() ?? 0, request.GroupCode);
                }
                else
                {
                    _logger.LogApplicationError("Failed to retrieve SMS Stakeholder Users for GroupCode {GroupCode}: {Error}",
                        ApplicationEventIds.Error, null);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error processing GetSMSStakeholderUsersByGroupCodeQuery for GroupCode: {GroupCode}", ApplicationEventIds.Error, ex);
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
    }

    public class ValidateSMSStakeholderUsersCredentialsQueryHandler : BaseQueryBundle, IRequestHandler<ValidateSMSStakeholderUserCredentialsQuery, Result<bool>>
    {
        private readonly SMSStakeholderUserDataService _dataService;
        private readonly ILogger<ValidateSMSStakeholderUsersCredentialsQueryHandler> _logger;

        public ValidateSMSStakeholderUsersCredentialsQueryHandler(SMSStakeholderUserDataService dataService, ILogger<ValidateSMSStakeholderUsersCredentialsQueryHandler> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> HandleAsync(ValidateSMSStakeholderUserCredentialsQuery request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Processing ValidateSMSStakeholderUsersCredentialsQuery for UserName: {UserName}", request.UserName);

                var userResult = await _dataService.GetByUserNameAsync(request.UserName);
                if (userResult.IsFailure)
                {
                    _logger.LogWarning("User not found for credential validation: {UserName}", request.UserName);
                    return Result<bool>.Success(false);
                }

                var user = userResult.Value;
                var isValid = user.IsActive && user.Authenticate(request.Password);

                user.UpdatedBy = "SYSTEM";
                user.LastLoginDate = DateTime.UtcNow;
                await _dataService.UpdateAsync(user, ct);


                _logger.LogInformation("Credential validation for {UserName}: {IsValid}", request.UserName, isValid);

                return Result<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error processing ValidateSMSOrganizationalUserCredentialsQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
            }
        }
    }
}
