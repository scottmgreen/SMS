using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS USER ROLE COMMAND HANDLERS
// =============================================

public class CreateSMSUserRoleCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSUserRoleCommand, Result<SMSUserRole>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<CreateSMSUserRoleCommandHandler> _logger;

    public CreateSMSUserRoleCommandHandler(SMSUserRoleDataService userRoleDataService, ILogger<CreateSMSUserRoleCommandHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSUserRole>> HandleAsync(CreateSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSUserRole is null)
            {
                _logger.LogApplicationError("CreateSMSUserRoleCommand received with null request or user role", ApplicationEventIds.Error, null);
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateSMSUserRoleCommand for Code: {Code}", request.SMSUserRole.Code);

            var result = await _userRoleDataService.CreateSMSUserRoleAsync(request.SMSUserRole);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS User Role with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS User Role with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating SMS User Role", ApplicationEventIds.Error, ex);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.CreateFailed);
        }
    }
}

public class UpdateSMSUserRoleCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSUserRoleCommand, Result<SMSUserRole>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<UpdateSMSUserRoleCommandHandler> _logger;

    public UpdateSMSUserRoleCommandHandler(SMSUserRoleDataService userRoleDataService, ILogger<UpdateSMSUserRoleCommandHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSUserRole>> HandleAsync(UpdateSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSUserRole is null)
            {
                _logger.LogApplicationError("UpdateSMSUserRoleCommand received with null request or user role", ApplicationEventIds.Error, null);
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSUserRoleCommand for ID: {Id}", request.SMSUserRole.Id);

            var result = await _userRoleDataService.UpdateSMSUserRoleAsync(request.SMSUserRole);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS User Role with ID: {Id}", request.SMSUserRole.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS User Role with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }
}

public class DeleteSMSUserRoleCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSUserRoleCommand, Result<bool>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<DeleteSMSUserRoleCommandHandler> _logger;

    public DeleteSMSUserRoleCommandHandler(SMSUserRoleDataService userRoleDataService, ILogger<DeleteSMSUserRoleCommandHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteSMSUserRoleCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteSMSUserRoleCommand for ID: {Id}", request.SMSUserRoleId);

            var result = await _userRoleDataService.DeleteSMSUserRoleAsync(request.SMSUserRoleId.Value);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS User Role with ID: {Id}", request.SMSUserRoleId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS User Role with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.DeleteFailed);
        }
    }
}

public class AssignRoleToUserCommandHandler : BaseCommandBundle, IRequestHandler<AssignRoleToUserCommand, Result<SMSUserRole>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<AssignRoleToUserCommandHandler> _logger;

    public AssignRoleToUserCommandHandler(SMSUserRoleDataService userRoleDataService, ILogger<AssignRoleToUserCommandHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSUserRole>> HandleAsync(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("AssignRoleToUserCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing AssignRoleToUserCommand - User: {UserId}, Role: {RoleCode}", 
                request.UserId, request.RoleCode);

            // Create a new user role assignment
            var userRoleId = new SMSUserRoleID($"UR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
            var userRole = new SMSUserRole(userRoleId)
            {
                Code = userRoleId.Value,
                Name = $"{request.RoleCode} Assignment for {request.UserId}"
                // Note: You'll need to set other properties based on your domain model
            };

            var result = await _userRoleDataService.CreateSMSUserRoleAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully assigned role {RoleCode} to user {UserId}", 
                    request.RoleCode, request.UserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign role {RoleCode} to user {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AssignRoleToUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while assigning role to user", ApplicationEventIds.Error, ex);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.CreateFailed);
        }
    }
}

//public class RemoveRoleFromUserCommandHandler : BaseCommandBundle, IRequestHandler<RemoveRoleFromUserCommand, Result<bool>>
//{
//    private readonly SMSUserRoleDataService _userRoleDataService;
//    private readonly ILogger<RemoveRoleFromUserCommandHandler> _logger;

//    public RemoveRoleFromUserCommandHandler(SMSUserRoleDataService userRoleDataService, ILogger<RemoveRoleFromUserCommandHandler> logger)
//    {
//        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<Result<bool>> HandleAsync(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (request is null)
//            {
//                _logger.LogError("RemoveRoleFromUserCommand received with null request");
//                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
//            }

//            _logger.LogInformation("Processing RemoveRoleFromUserCommand - User: {UserId}, Role: {RoleCode}", 
//                request.UserId, request.RoleCode);

//            // Get user's roles to find the specific assignment to remove
//            var userRolesResult = await _userRoleDataService.GetSMSUserRolesByUserIdAsync(request.UserId);
//            if (userRolesResult.IsFailure)
//            {
//                return Result<bool>.Failure<bool>(userRolesResult.Error);
//            }

//            var roleToRemove = userRolesResult.Value.FirstOrDefault(ur => 
//                string.Equals(ur.Code, request.RoleCode, StringComparison.OrdinalIgnoreCase));

//            if (roleToRemove == null)
//            {
//                _logger.LogWarning("Role {RoleCode} not found for user {UserId}", request.RoleCode, request.UserId);
//                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NotFound);
//            }

//            var result = await _userRoleDataService.DeleteSMSUserRoleAsync(roleToRemove.Id.Value);

//            if (result.IsSuccess)
//            {
//                _logger.LogInformation("Successfully removed role {RoleCode} from user {UserId}", 
//                    request.RoleCode, request.UserId);
//            }
//            else
//            {
//                _logger.LogError("Failed to remove role {RoleCode} from user {UserId}. Error: {Error}",
//                    request.RoleCode, request.UserId, result.Error?.Message);
//            }

//            return result;
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogWarning("RemoveRoleFromUserCommand operation was cancelled");
//            throw;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Unexpected error occurred while removing role from user");
//            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.DeleteFailed);
//        }
//    }
//}

public class ActivateSMSUserRoleCommandHandler : BaseCommandBundle, IRequestHandler<ActivateSMSUserRoleCommand, Result<bool>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<ActivateSMSUserRoleCommandHandler> _logger;

    public ActivateSMSUserRoleCommandHandler(SMSUserRoleDataService userRoleDataService, ILogger<ActivateSMSUserRoleCommandHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ActivateSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("ActivateSMSUserRoleCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing ActivateSMSUserRoleCommand for ID: {Id}", request.UserRoleId);

            // Get the user role to activate
            var userRoleResult = await _userRoleDataService.GetSMSUserRoleByIdAsync(request.UserRoleId);
            if (userRoleResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userRoleResult.Error);
            }

            var userRole = userRoleResult.Value;
            
            // Note: You'll need to add activation logic to your domain model
            // userRole.Activate(request.ActivatedBy);

            var result = await _userRoleDataService.UpdateSMSUserRoleAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully activated SMS User Role with ID: {Id}", request.UserRoleId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to activate SMS User Role with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ActivateSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while activating SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }
}

public class DeactivateSMSUserRoleCommandHandler : BaseCommandBundle, IRequestHandler<DeactivateSMSUserRoleCommand, Result<bool>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<DeactivateSMSUserRoleCommandHandler> _logger;

    public DeactivateSMSUserRoleCommandHandler(SMSUserRoleDataService userRoleDataService, ILogger<DeactivateSMSUserRoleCommandHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeactivateSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeactivateSMSUserRoleCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeactivateSMSUserRoleCommand for ID: {Id}", request.UserRoleId);

            // Get the user role to deactivate
            var userRoleResult = await _userRoleDataService.GetSMSUserRoleByIdAsync(request.UserRoleId);
            if (userRoleResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userRoleResult.Error);
            }

            var userRole = userRoleResult.Value;
            
            // Note: You'll need to add deactivation logic to your domain model
            // userRole.Deactivate(request.DeactivatedBy, request.DeactivationReason);

            var result = await _userRoleDataService.UpdateSMSUserRoleAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deactivated SMS User Role with ID: {Id}", request.UserRoleId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to deactivate SMS User Role with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeactivateSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deactivating SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }
}