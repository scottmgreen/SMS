//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS risk analysis business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// RISK ANALYSIS COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateRiskAnalysisCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateRiskAnalysisCommand, Result<RiskAnalysis>>
{
    private readonly IRiskAnalysisService _riskAnalysisService;
    private readonly ILogger<CreateRiskAnalysisCommandHandler> _logger;

    public CreateRiskAnalysisCommandHandler(IRiskAnalysisService riskAnalysisService, ILogger<CreateRiskAnalysisCommandHandler> logger)
    {
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(CreateRiskAnalysisCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAnalysis is null)
            {
                _logger.LogApplicationError("CreateRiskAnalysisCommand received with null RiskAnalysis", ApplicationEventIds.Error, null);
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing CreateRiskAnalysisCommand for Code: {Code}", request.RiskAnalysis.Code);

            var result = await _riskAnalysisService.CreateRiskAnalysisAsync(request.RiskAnalysis, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully created RiskAnalysis with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create RiskAnalysis with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateRiskAnalysisCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating RiskAnalysis", ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.CreateFailed);
        }
    }
}

public class UpdateRiskAnalysisCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateRiskAnalysisCommand, Result<RiskAnalysis>>
{
    private readonly IRiskAnalysisService _riskAnalysisService;
    private readonly ILogger<UpdateRiskAnalysisCommandHandler> _logger;

    public UpdateRiskAnalysisCommandHandler(IRiskAnalysisService riskAnalysisService, ILogger<UpdateRiskAnalysisCommandHandler> logger)
    {
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(UpdateRiskAnalysisCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAnalysis is null)
            {
                _logger.LogApplicationError("UpdateRiskAnalysisCommand received with null RiskAnalysis", ApplicationEventIds.Error, null);
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing UpdateRiskAnalysisCommand for ID: {Id}, Code: {Code}",
                request.RiskAnalysis.Id, request.RiskAnalysis.Code);

            var result = await _riskAnalysisService.UpdateRiskAnalysisAsync(request.RiskAnalysis, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully updated RiskAnalysis with ID: {Id}", request.RiskAnalysis.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update RiskAnalysis with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateRiskAnalysisCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating RiskAnalysis with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.UpdateFailed);
        }
    }
}

public class DeleteRiskAnalysisCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteRiskAnalysisCommand, Result<bool>>
{
    private readonly IRiskAnalysisService _riskAnalysisService;
    private readonly ILogger<DeleteRiskAnalysisCommandHandler> _logger;

    public DeleteRiskAnalysisCommandHandler(IRiskAnalysisService riskAnalysisService, ILogger<DeleteRiskAnalysisCommandHandler> logger)
    {
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteRiskAnalysisCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAnalysisId is null)
            {
                _logger.LogApplicationError("DeleteRiskAnalysisCommand received with null RiskAnalysisId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing DeleteRiskAnalysisCommand for ID: {Id}", request.RiskAnalysisId);

            var result = await _riskAnalysisService.DeleteRiskAnalysisAsync(request.RiskAnalysisId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully deleted RiskAnalysis with ID: {Id}", request.RiskAnalysisId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete RiskAnalysis with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteRiskAnalysisCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting RiskAnalysis with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAnalysisError.DeleteFailed);
        }
    }
}
