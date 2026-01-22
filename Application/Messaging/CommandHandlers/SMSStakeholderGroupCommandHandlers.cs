using Microsoft.Extensions.Logging;

using SMS_Infrastructure.Persistence;

namespace SMS_Application.Messaging.CommandHandlers;

/// <summary>
/// Command handler for creating SMS stakeholder groups
/// </summary>
public class CreateSMSStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSStakeholderGroupCommand, Result<SMSStakeholderGroup>>
{
    private readonly SMSStakeholderGroupDataService _stakeholderGroupDataService;
    private readonly ILogger<CreateSMSStakeholderGroupCommandHandler> _logger;

    public CreateSMSStakeholderGroupCommandHandler(
        SMSStakeholderGroupDataService stakeholderGroupDataService,
        ILogger<CreateSMSStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupDataService = stakeholderGroupDataService ?? throw new ArgumentNullException(nameof(stakeholderGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderGroup>> HandleAsync(CreateSMSStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing CreateSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup?.Code);

            if (request?.StakeholderGroup == null)
            {
                _logger.LogWarning("CreateSMSStakeholderGroupCommand received with null stakeholder group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            var result = await _stakeholderGroupDataService.CreateAsync(request.StakeholderGroup);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS stakeholder group: {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing CreateSMSStakeholderGroupCommand for group: {GroupCode}",
                ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.CreateFailed);
        }
    }
}

/// <summary>
/// Command handler for updating SMS stakeholder groups
/// </summary>
public class UpdateSMSStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSStakeholderGroupCommand, Result<SMSStakeholderGroup>>
{
    private readonly SMSStakeholderGroupRepository _stakeholderGroupRepository;
    private readonly ILogger<UpdateSMSStakeholderGroupCommandHandler> _logger;

    public UpdateSMSStakeholderGroupCommandHandler(
        SMSStakeholderGroupRepository stakeholderGroupRepository,
        ILogger<UpdateSMSStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupRepository = stakeholderGroupRepository ?? throw new ArgumentNullException(nameof(stakeholderGroupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderGroup>> HandleAsync(UpdateSMSStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing UpdateSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup?.Code);

            if (request?.StakeholderGroup == null)
            {
                _logger.LogWarning("UpdateSMSStakeholderGroupCommand received with null stakeholder group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            var result = await _stakeholderGroupRepository.UpdateAsync(request.StakeholderGroup);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS stakeholder group: {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing UpdateSMSStakeholderGroupCommand for group: {GroupCode}",
                ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.UpdateFailed);
        }
    }
}

/// <summary>
/// Command handler for deleting SMS stakeholder groups
/// </summary>
public class DeleteSMSStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSStakeholderGroupCommand, Result<bool>>
{
    private readonly SMSStakeholderGroupRepository _stakeholderGroupRepository;
    private readonly ILogger<DeleteSMSStakeholderGroupCommandHandler> _logger;

    public DeleteSMSStakeholderGroupCommandHandler(
        SMSStakeholderGroupRepository stakeholderGroupRepository,
        ILogger<DeleteSMSStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupRepository = stakeholderGroupRepository ?? throw new ArgumentNullException(nameof(stakeholderGroupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing DeleteSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup.Code);

            if (string.IsNullOrWhiteSpace(request.StakeholderGroup.Code))
            {
                _logger.LogWarning("DeleteSMSStakeholderGroupCommand received with null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            var result = await _stakeholderGroupRepository.DeleteAsync(request.StakeholderGroup.Code);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS stakeholder group: {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing DeleteSMSStakeholderGroupCommand for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.DeleteFailed);
        }
    }
}

/// <summary>
/// Command handler for assigning users to stakeholder groups
/// </summary>
public class AssignUserToStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<AssignUserToStakeholderGroupCommand, Result<bool>>
{
    private readonly SMSStakeholderGroupRepository _stakeholderGroupRepository;
    private readonly ILogger<AssignUserToStakeholderGroupCommandHandler> _logger;

    public AssignUserToStakeholderGroupCommandHandler(
        SMSStakeholderGroupRepository stakeholderGroupRepository,
        ILogger<AssignUserToStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupRepository = stakeholderGroupRepository ?? throw new ArgumentNullException(nameof(stakeholderGroupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AssignUserToStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing AssignUserToStakeholderGroupCommand for user: {UserCode} to group: {GroupCode}",
                request.UserCode, request.StakeholderGroupID);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.StakeholderGroupID.Value))
            {
                _logger.LogWarning("AssignUserToStakeholderGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupRepository.AssignUserToGroupAsync(request.UserCode, request.StakeholderGroupID.Value, request.AssignedBy);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully assigned user {UserCode} to group {GroupCode}", request.UserCode, request.StakeholderGroupID.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign user {UserCode} to group {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AssignUserToStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing AssignUserToStakeholderGroupCommand for user: {UserCode} to group: {GroupCode}",
                ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.AssignmentFailed);
        }
    }
}

/// <summary>
/// Command handler for removing users from stakeholder groups
/// </summary>
public class RemoveUserFromStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<RemoveUserFromStakeholderGroupCommand, Result<bool>>
{
    private readonly SMSStakeholderGroupRepository _stakeholderGroupRepository;
    private readonly ILogger<RemoveUserFromStakeholderGroupCommandHandler> _logger;

    public RemoveUserFromStakeholderGroupCommandHandler(
        SMSStakeholderGroupRepository stakeholderGroupRepository,
        ILogger<RemoveUserFromStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupRepository = stakeholderGroupRepository ?? throw new ArgumentNullException(nameof(stakeholderGroupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RemoveUserFromStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing RemoveUserFromStakeholderGroupCommand for user: {UserCode} from group: {GroupCode}",
                request.UserCode, request.StakeholderGroupID.Value);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.StakeholderGroupID.Value))
            {
                _logger.LogWarning("RemoveUserFromStakeholderGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupRepository.RemoveUserFromGroupAsync(request.UserCode, request.StakeholderGroupID.Value);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully removed user {UserCode} from group {GroupCode}", request.UserCode, request.StakeholderGroupID.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to remove user {UserCode} from group {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RemoveUserFromStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing RemoveUserFromStakeholderGroupCommand for user: {UserCode} from group: {GroupCode}",
                ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.RemovalFailed);
        }
    }
}

/// <summary>
/// Command handler for clearing all user group memberships
/// </summary>
public class ClearUserStakeholderGroupsCommandHandler : BaseCommandBundle, IRequestHandler<ClearUserStakeholderGroupsCommand, Result<bool>>
{
    private readonly SMSStakeholderGroupRepository _stakeholderGroupRepository;
    private readonly ILogger<ClearUserStakeholderGroupsCommandHandler> _logger;

    public ClearUserStakeholderGroupsCommandHandler(
        SMSStakeholderGroupRepository stakeholderGroupRepository,
        ILogger<ClearUserStakeholderGroupsCommandHandler> logger)
    {
        _stakeholderGroupRepository = stakeholderGroupRepository ?? throw new ArgumentNullException(nameof(stakeholderGroupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ClearUserStakeholderGroupsCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing ClearUserStakeholderGroupsCommand for user: {UserCode}", request.UserCode);

            if (string.IsNullOrWhiteSpace(request.UserCode))
            {
                _logger.LogWarning("ClearUserStakeholderGroupsCommand received with null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupRepository.ClearUserGroupsAsync(request.UserCode);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully cleared all group memberships for user: {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to clear group memberships for user {UserCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ClearUserStakeholderGroupsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing ClearUserStakeholderGroupsCommand for user: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.ClearGroupsFailed);
        }
    }
}