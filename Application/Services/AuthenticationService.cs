//-----------------------------------------------------------------------
// <copyright file="AuthenticationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Authentication service providing presentation-layer independent authentication logic.
//                  Moved from Login.razor.cs to achieve clean architecture separation.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Authentication service providing presentation-layer independent authentication logic.
/// Implements smart user discovery and authentication across all user types.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IMediator mediator, ILogger<AuthenticationService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Smart authentication: Try each user type sequentially using CQRS queries.
    /// This implements the same logic that was previously in Login.razor.cs but in the Application layer.
    /// </summary>
    public async Task<AuthenticationResult> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthenticationResult.Failure("Email and password are required");
        }

        _logger.LogInformation("Attempting smart authentication for user: {Email}", email);

        // Try SMSApplicationUser first
        var applicationResult = await TryAuthenticateApplicationUserAsync(email, password, cancellationToken);
        if (applicationResult.IsSuccess)
        {
            _logger.LogInformation("Application User authenticated successfully: {Email}", email);
            return applicationResult;
        }

        // Try SMSOrganizationalUser
        var organizationalResult = await TryAuthenticateOrganizationalUserAsync(email, password, cancellationToken);
        if (organizationalResult.IsSuccess)
        {
            _logger.LogInformation("Organizational User authenticated successfully: {Email}", email);
            return organizationalResult;
        }

        // Try SMSStakeholderUser
        var stakeholderResult = await TryAuthenticateStakeholderUserAsync(email, password, cancellationToken);
        if (stakeholderResult.IsSuccess)
        {
            _logger.LogInformation("Stakeholder User authenticated successfully: {Email}", email);
            return stakeholderResult;
        }

        // No user found or authentication failed
        _logger.LogWarning("Authentication failed - user not found or invalid credentials: {Email}", email);
        return AuthenticationResult.Failure("Invalid email or password");
    }

    /// <summary>
    /// Try to authenticate as SMSApplicationUser
    /// </summary>
    private async Task<AuthenticationResult> TryAuthenticateApplicationUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Checking Application User: {Email}", email);
            var query = new GetSMSApplicationUserByUserNameQuery(email);
            var result = await _mediator.SendAsync(query, cancellationToken);

            if (result.IsSuccess && result.Value != null && result.Value.Authenticate(password))
            {
                result.Value.RecordLogin();
                return AuthenticationResult.Success(result.Value, SMSUserType.Application);
            }

            return AuthenticationResult.Failure("Application user authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Application user authentication failed: {Error}", ex.Message);
            return AuthenticationResult.Failure($"Application user authentication error: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to authenticate as SMSOrganizationalUser
    /// </summary>
    private async Task<AuthenticationResult> TryAuthenticateOrganizationalUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Checking Organizational User: {Email}", email);
            var query = new GetSMSOrganizationalUserByUserNameQuery(email);
            var result = await _mediator.SendAsync(query, cancellationToken);

            if (result.IsSuccess && result.Value != null && result.Value.Authenticate(password))
            {
                result.Value.RecordLogin();
                return AuthenticationResult.Success(result.Value, SMSUserType.Organizational);
            }

            return AuthenticationResult.Failure("Organizational user authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Organizational user authentication failed: {Error}", ex.Message);
            return AuthenticationResult.Failure($"Organizational user authentication error: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to authenticate as SMSStakeholderUser
    /// </summary>
    private async Task<AuthenticationResult> TryAuthenticateStakeholderUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Checking Stakeholder User: {Email}", email);
            var query = new GetSMSStakeholderUserByUserNameQuery(email);
            var result = await _mediator.SendAsync(query, cancellationToken);

            if (result.IsSuccess && result.Value != null && result.Value.Authenticate(password))
            {
                result.Value.RecordLogin();
                return AuthenticationResult.Success(result.Value, SMSUserType.Stakeholder);
            }

            return AuthenticationResult.Failure("Stakeholder user authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Stakeholder user authentication failed: {Error}", ex.Message);
            return AuthenticationResult.Failure($"Stakeholder user authentication error: {ex.Message}");
        }
    }
}