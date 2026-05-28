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
using SMS_Application.Queries;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Authentication service providing presentation-layer independent authentication logic.
/// Implements smart user discovery and authentication across all user types.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IBaseMediator _mediator;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IBaseMediator mediator, ILogger<AuthenticationService> logger)
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

        _logger.LogApplicationInformation("Attempting smart authentication for user: {Email}",
            ApplicationEventIds.Information,
            email);

        // Try SMSApplicationUser first
        var applicationResult = await TryAuthenticateApplicationUserAsync(email, password, cancellationToken);
        if (applicationResult.IsSuccess)
        {
            _logger.LogApplicationInformation("Application User authenticated successfully: {Email}",
                ApplicationEventIds.Information,
                email);
            return applicationResult;
        }

        // Try SMSOrganizationalUser
        var organizationalResult = await TryAuthenticateOrganizationalUserAsync(email, password, cancellationToken);
        if (organizationalResult.IsSuccess)
        {
            _logger.LogApplicationInformation("Organizational User authenticated successfully: {Email}",
                ApplicationEventIds.Information,
                email);
            return organizationalResult;
        }

        // Try SMSStakeholderUser
        var stakeholderResult = await TryAuthenticateStakeholderUserAsync(email, password, cancellationToken);
        if (stakeholderResult.IsSuccess)
        {
            _logger.LogApplicationInformation("Stakeholder User authenticated successfully: {Email}",
                ApplicationEventIds.Information,
                email);
            return stakeholderResult;
        }

        // No user found or authentication failed
        _logger.LogApplicationWarning("Authentication failed - user not found or invalid credentials: {Email}",
            ApplicationEventIds.Warning,
            email);
        return AuthenticationResult.Failure("Invalid email or password");
    }

    /// <summary>
    /// Try to authenticate as SMSApplicationUser - ? PIPELINE APPROACH with audit logging
    /// </summary>
    private async Task<AuthenticationResult> TryAuthenticateApplicationUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogApplicationDebug("Checking Application User: {Email}",
                ApplicationEventIds.Debug,
                email);
            var query = new GetSMSApplicationUserByUserNameQuery(email);
            var result = await _mediator.SendAsync(query, cancellationToken);

            if (result.IsSuccess && result.Value != null && result.Value.Authenticate(password))
            {
                result.Value.RecordLogin();
                
                // ? PIPELINE APPROACH: Record authentication success
                try
                {
                    var authSuccessCommand = new RecordAuthenticationSuccessCommand(
                        email,
                        SMSUserType.Application,
                        result.Value.DisplayName,
                        "Server", // Service layer doesn't have access to HTTP context
                        "Application Service",
                        Guid.NewGuid().ToString()
                    );
                    var auditResult = await _mediator.SendAsync(authSuccessCommand, cancellationToken);
                    _logger.LogApplicationInformation("Authentication success audit recorded for Application User: {Email}",
                        ApplicationEventIds.Information,
                        email);
                }
                catch (Exception auditEx)
                {
                    _logger.LogApplicationError("Failed to record authentication audit for Application User: {Email}",
                        ApplicationEventIds.Error,
                        auditEx,
                        email);
                    // Don't fail authentication due to audit failure
                }

                return AuthenticationResult.Success(result.Value, SMSUserType.Application);
            }

            return AuthenticationResult.Failure("Application user authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogApplicationDebug("Application user authentication failed: {Error}",
                ApplicationEventIds.Debug,
                ex.Message);
            return AuthenticationResult.Failure($"Application user authentication error: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to authenticate as SMSOrganizationalUser - ? PIPELINE APPROACH with audit logging
    /// </summary>
    private async Task<AuthenticationResult> TryAuthenticateOrganizationalUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogApplicationDebug("Checking Organizational User: {Email}",
                ApplicationEventIds.Debug,
                email);
            var query = new GetSMSOrganizationalUserByUserNameQuery(email);
            var result = await _mediator.SendAsync(query, cancellationToken);

            if (result.IsSuccess && result.Value != null && result.Value.Authenticate(password))
            {
                result.Value.RecordLogin();
                
                // ? PIPELINE APPROACH: Record authentication success
                try
                {
                    var authSuccessCommand = new RecordAuthenticationSuccessCommand(
                        email,
                        SMSUserType.Organizational,
                        result.Value.DisplayName,
                        "Server",
                        "Application Service",
                        Guid.NewGuid().ToString()
                    );
                    var auditResult = await _mediator.SendAsync(authSuccessCommand, cancellationToken);
                    _logger.LogApplicationInformation("Authentication success audit recorded for Organizational User: {Email}",
                        ApplicationEventIds.Information,
                        email);
                }
                catch (Exception auditEx)
                {
                    _logger.LogApplicationError("Failed to record authentication audit for Organizational User: {Email}",
                        ApplicationEventIds.Error,
                        auditEx,
                        email);
                    // Don't fail authentication due to audit failure
                }

                return AuthenticationResult.Success(result.Value, SMSUserType.Organizational);
            }

            return AuthenticationResult.Failure("Organizational user authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogApplicationDebug("Organizational user authentication failed: {Error}",
                ApplicationEventIds.Debug,
                ex.Message);
            return AuthenticationResult.Failure($"Organizational user authentication error: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to authenticate as SMSStakeholderUser - ? PIPELINE APPROACH with audit logging
    /// </summary>
    private async Task<AuthenticationResult> TryAuthenticateStakeholderUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogApplicationDebug("Checking Stakeholder User: {Email}",
                ApplicationEventIds.Debug,
                email);
            var query = new GetSMSStakeholderUserByUserNameQuery(email);
            var result = await _mediator.SendAsync(query, cancellationToken);

            if (result.IsSuccess && result.Value != null && result.Value.Authenticate(password))
            {
                result.Value.RecordLogin();
                
                // ? PIPELINE APPROACH: Record authentication success
                try
                {
                    var authSuccessCommand = new RecordAuthenticationSuccessCommand(
                        email,
                        SMSUserType.Stakeholder,
                        result.Value.DisplayName,
                        "Server",
                        "Application Service",
                        Guid.NewGuid().ToString()
                    );
                    var auditResult = await _mediator.SendAsync(authSuccessCommand, cancellationToken);
                    _logger.LogApplicationInformation("Authentication success audit recorded for Stakeholder User: {Email}",
                        ApplicationEventIds.Information,
                        email);
                }
                catch (Exception auditEx)
                {
                    _logger.LogApplicationError("Failed to record authentication audit for Stakeholder User: {Email}",
                        ApplicationEventIds.Error,
                        auditEx,
                        email);
                    // Don't fail authentication due to audit failure
                }

                return AuthenticationResult.Success(result.Value, SMSUserType.Stakeholder);
            }

            return AuthenticationResult.Failure("Stakeholder user authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogApplicationDebug("Stakeholder user authentication failed: {Error}",
                ApplicationEventIds.Debug,
                ex.Message);
            return AuthenticationResult.Failure($"Stakeholder user authentication error: {ex.Message}");
        }
    }
}
