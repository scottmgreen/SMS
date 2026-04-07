//-----------------------------------------------------------------------
// <copyright file="QueryAuditPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Pipeline behavior for auditing query (read) operations
//                  Tracks all data access for compliance and security monitoring
//                  UPDATED: Now uses feature flags to control audit verbosity
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Application.Common;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Pipeline behavior that audits all query operations implementing IReadQuery
/// Provides comprehensive audit trail for data access operations
/// UPDATED: Now respects EnableQueryAudit feature flag to reduce chattiness
/// </summary>
public class QueryAuditPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    private readonly IQueryAccessAuditService _queryAuditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<QueryAuditPipeline<TRequest, TResult>> _logger;

    public QueryAuditPipeline(
        IQueryAccessAuditService queryAuditService,
        ICurrentUserService currentUserService,
        IFeatureManager featureManager,
        ILogger<QueryAuditPipeline<TRequest, TResult>> logger)
    {
        _queryAuditService = queryAuditService ?? throw new ArgumentNullException(nameof(queryAuditService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResult> HandleAsync(
        TRequest request,
        RequestPipelineDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var queryType = request.GetType().Name;
        var currentUserId = _currentUserService.UserDisplayName;

        // Check if query auditing is enabled via feature flag
        var isQueryAuditEnabled = await _featureManager.IsEnabledAsync("EnableQueryAudit");

        // Only audit queries if feature is enabled AND request implements IReadQuery
        if (isQueryAuditEnabled && EntityInformationExtractor.IsReadQuery(request))
        {
            var readQuery = (IReadQuery)request;
            var resourceIdentifier = EntityInformationExtractor.GetResourceIdentifier(request);
            var accessType = EntityInformationExtractor.GetActionType(request);
            
            _logger.LogDebug("?? Query Audit: Processing read query {QueryType} by user {UserId}", 
                queryType, currentUserId);

            try
            {
                // Execute the query first
                var result = await next().ConfigureAwait(false);

                // Log the query access AFTER successful execution
                if (result.IsSuccess)
                {
                    await _queryAuditService.LogQueryAccessAsync(
                        currentUserId,
                        queryType,
                        resourceIdentifier,
                        accessType,
                        cancellationToken);

                    _logger.LogDebug("? Query Audit: Successfully audited {QueryType} access by {UserId}", 
                        queryType, currentUserId);
                }
                else
                {
                    // Still log failed queries for security monitoring
                    await _queryAuditService.LogQueryAccessAsync(
                        currentUserId,
                        queryType,
                        resourceIdentifier,
                        $"{accessType}_FAILED",
                        $"Query failed: {result.Error?.Message}",
                        cancellationToken);

                    _logger.LogWarning("?? Query Audit: Logged failed query {QueryType} by {UserId}", 
                        queryType, currentUserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Log query exceptions for security monitoring
                await _queryAuditService.LogQueryAccessAsync(
                    currentUserId,
                    queryType,
                    resourceIdentifier,
                    $"{accessType}_ERROR",
                    $"Query exception: {ex.Message}",
                    cancellationToken);

                _logger.LogError(ex, "? Query Audit: Logged query exception for {QueryType} by {UserId}", 
                    queryType, currentUserId);

                throw; // Re-throw the original exception
            }
        }
        else
        {
            // Query auditing disabled or not a read query - just execute without audit logging
            _logger.LogTrace("?? Query Audit: Skipping audit - Feature: {Enabled}, IsReadQuery: {IsRead}", 
                isQueryAuditEnabled, EntityInformationExtractor.IsReadQuery(request));
            return await next().ConfigureAwait(false);
        }
    }
}