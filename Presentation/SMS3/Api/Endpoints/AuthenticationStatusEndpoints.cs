//-----------------------------------------------------------------------
// <copyright file="AuthenticationStatusEndpoints.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Minimal API endpoints for authentication system diagnostics.
//                  Development-only endpoints for testing authentication strategies.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.FeatureManagement;
using SMS_Application.Interfaces;

namespace SMS3.Api.Endpoints;

/// <summary>
/// Authentication diagnostics endpoints using Minimal API pattern
/// Development-only endpoints for testing authentication strategies
/// </summary>
public static class AuthenticationStatusEndpoints
{
    /// <summary>
    /// Maps authentication diagnostic endpoints if enabled via feature flag
    /// </summary>
    public static WebApplication MapAuthenticationStatusEndpoints(this WebApplication app)
    {
        // Check if authentication diagnostics are enabled via feature management
        var featureManager = app.Services.GetRequiredService<IFeatureManager>();
        
        // Only register diagnostics endpoints if the feature is enabled AND in development
        if (app.Environment.IsDevelopment() && 
            featureManager.IsEnabledAsync("EnableAuthenticationLogging").GetAwaiter().GetResult())
        {
            var authGroup = app.MapGroup("/api/auth-diagnostics")
                .WithTags("AuthenticationDiagnostics");

            // Authentication system status
            authGroup.MapGet("/status", GetAuthenticationStatus)
                .WithName("GetAuthenticationStatus")
                .WithSummary("Get comprehensive authentication system status")
                .WithDescription("Development-only endpoint to check authentication strategy status");

            // Test user instantiation
            authGroup.MapGet("/test-user/{userCode}/{userType}", TestUserInstantiation)
                .WithName("TestUserInstantiation")
                .WithSummary("Test complete user instantiation for debugging")
                .WithDescription("Development-only endpoint to validate user loading with CQRS");

            // Strategy validation
            authGroup.MapGet("/validate-strategies", ValidateStrategies)
                .WithName("ValidateStrategies")
                .WithSummary("Validate all authentication strategies")
                .WithDescription("Development-only endpoint to test all strategy implementations");
        }

        return app;
    }

    /// <summary>
    /// Get comprehensive authentication system status
    /// </summary>
    private static async Task<IResult> GetAuthenticationStatus(
        IAuthenticationStrategyManager strategyManager,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("?? Authentication status requested");

            var status = await strategyManager.GetStrategyStatusAsync();
            var isAuthenticated = await strategyManager.IsUserAuthenticatedAsync();
            var currentUserId = await strategyManager.GetCurrentUserIdAsync();
            var currentUserName = await strategyManager.GetCurrentUserDisplayNameAsync();

            var result = new
            {
                Timestamp = DateTime.UtcNow,
                IsAuthenticated = isAuthenticated,
                CurrentUserId = currentUserId,
                CurrentUserName = currentUserName,
                StrategyStatus = status,
                ValidationResults = await strategyManager.ValidateAllStrategiesAsync()
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Error getting authentication status");
            return Results.Problem(
                detail: ex.Message,
                title: "Authentication Status Error",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Test complete user instantiation for a specific user
    /// </summary>
    private static async Task<IResult> TestUserInstantiation(
        string userCode,
        string userType,
        IUserInstantiationService userInstantiationService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("?? Testing user instantiation for {UserCode} ({UserType})", userCode, userType);

            var smsUserType = SMS_Domain.Enums.SMSUserType.FromValue(userType);
            var result = await userInstantiationService.GetCompleteUserAsync(userCode, smsUserType);

            if (result.IsSuccess)
            {
                var user = result.Value.User;
                var completenessScore = userInstantiationService.IsUserComplete(user) ? 100 : 0;
                var missingComponents = userInstantiationService.GetMissingComponents(user);

                var response = new
                {
                    Success = true,
                    UserCode = user.Code,
                    DisplayName = user.DisplayName,
                    UserType = result.Value.UserType.Value,
                    IsComplete = completenessScore == 100,
                    CompletenessScore = completenessScore,
                    MissingComponents = missingComponents,
                    RoleCode = user.UserRole?.Code,
                    RoleName = user.UserRole?.Name,
                    PermissionCount = user.UserRole?.Permissions?.Count ?? 0,
                    Permissions = user.UserRole?.Permissions?.Select(p => new
                    {
                        Module = p.SMSModule,
                        Create = p.Create,
                        Read = p.Read,
                        Update = p.Update,
                        Delete = p.Delete
                    }).ToList()
                };

                return Results.Ok(response);
            }
            else
            {
                return Results.NotFound(new { Error = result.Error?.Message });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Error testing user instantiation for {UserCode} ({UserType})", userCode, userType);
            return Results.Problem(
                detail: ex.Message,
                title: "User Instantiation Test Error",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Validate all authentication strategies
    /// </summary>
    private static async Task<IResult> ValidateStrategies(
        IAuthenticationStrategyManager strategyManager,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("?? Validating all authentication strategies");

            var strategies = strategyManager.GetAllStrategies().ToList();
            var validationResults = await strategyManager.ValidateAllStrategiesAsync();

            var result = new
            {
                Timestamp = DateTime.UtcNow,
                TotalStrategies = strategies.Count,
                AvailableStrategies = strategies.Where(s => s.IsAvailable).Count(),
                StrategyDetails = strategies.Select(s => new
                {
                    Name = s.StrategyName,
                    Method = s.Method.ToString(),
                    IsAvailable = s.IsAvailable,
                    StorageInfo = s.GetStorageInfo(),
                    IsValid = validationResults.GetValueOrDefault(s.Method, false)
                }).ToList(),
                ValidationResults = validationResults
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Error validating authentication strategies");
            return Results.Problem(
                detail: ex.Message,
                title: "Strategy Validation Error",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}