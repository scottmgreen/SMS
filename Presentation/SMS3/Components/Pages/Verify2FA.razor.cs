using SMS_Application.Services;
using SMS_Application.Interfaces;

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;


namespace SMS3.Components.Pages;

public partial class Verify2FA : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private ILogger<Verify2FA> _logger { get; set; } = default!;
    [Inject] private ISMSSessionService _sessionService { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; } = default!;
    [Inject] private TwoFactorAuthService _twoFactorAuthService { get; set; } = default!;
    [Inject] private SessionTimerService _sessionTimerService { get; set; } = default!;
    
    // ?? SMART REPOSITORY INJECTION - All three user repositories
    [Inject] private ISMSApplicationUserRepository _applicationUserRepository { get; set; } = default!;
    [Inject] private ISMSOrganizationalUserRepository _organizationalUserRepository { get; set; } = default!;
    [Inject] private ISMSStakeholderUserRepository _stakeholderUserRepository { get; set; } = default!;
    
    [Inject] private TwoFactorSessionTimerService _twoFactorTimer { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery] public string? User { get; set; }

    private TwoFactorFormModel _twoFactorModel { get; set; } = new();
    private SetupFormModel _setupModel { get; set; } = new();
    private string _errorMessage { get; set; } = string.Empty;
    private bool _isLoading { get; set; } = false;
    private string _backupCode { get; set; } = string.Empty;
    
    // Page state
    private bool _showSetupPage { get; set; } = false;
    private bool _showBackupMethods { get; set; } = false;
    private bool _showBackupCodeEntry { get; set; } = false;
    
    // QR Code data for Radzen component
    private string _qrCodeUrl { get; set; } = string.Empty;
    private string _manualEntryCode { get; set; } = string.Empty;
    private string _userDisplayName { get; set; } = string.Empty;
    private string _userSecretKey { get; set; } = string.Empty;
    
    // Timer for TOTP countdown
    private Timer? _timeRemainingTimer;
    private int _timeRemainingInWindow { get; set; } = 30;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _logger.LogInformation("2FA Verification page initialized for user: {User}", User);
            _logger.LogInformation("Current URL: {CurrentUrl}", _navigation.Uri);

            // Add delay to ensure session is fully available
            await Task.Delay(200);

            // ?? CRITICAL: Check for session contamination and clean slate verification
            var initialPendingCheck = _sessionService.GetPending2FAUser();
            if (initialPendingCheck is not null)
            {
                _logger.LogInformation("Found existing pending 2FA user: {ExistingUser} - verifying it's not contaminated", 
                    initialPendingCheck.Value.User.UserName.Value);
                
                // If we have a query parameter user and it doesn't match the pending user, clear contaminated data
                if (!string.IsNullOrEmpty(User) && initialPendingCheck.Value.User.UserName.Value != User)
                {
                    _logger.LogError("SESSION CONTAMINATION DETECTED: Pending user {PendingUser} does not match URL user {UrlUser}",
                        initialPendingCheck.Value.User.UserName.Value, User);
                    
                    // Clear contaminated data
                    await _sessionService.ClearPending2FAUserAsync();
                    await Task.Delay(200);
                    
                    _errorMessage = "Session contamination detected. Please log in again.";
                    _logger.LogError("Redirecting to login due to session contamination");
                    _navigation.NavigateTo("/login", forceLoad: true);
                    return;
                }
            }

            // Get pending 2FA user from session
            var pendingUserTuple = _sessionService.GetPending2FAUser();
            _logger.LogInformation("First attempt to get pending user: {Found}", pendingUserTuple is not null ? "FOUND" : "NOT FOUND");
            
            if (pendingUserTuple is null)
            {
                _logger.LogWarning("No pending 2FA user found in session - checking again in 500ms");
                
                // Try again after a short delay in case of timing issues
                await Task.Delay(500);
                pendingUserTuple = _sessionService.GetPending2FAUser();
                _logger.LogInformation("Second attempt to get pending user: {Found}", pendingUserTuple is not null ? "FOUND" : "NOT FOUND");
                
                if (pendingUserTuple is null)
                {
                    _logger.LogError("No pending 2FA user found in session after retry");
                    
                    // Try one more time with longer delay
                    await Task.Delay(1000);
                    pendingUserTuple = _sessionService.GetPending2FAUser();
                    _logger.LogInformation("Third attempt to get pending user: {Found}", pendingUserTuple is not null ? "FOUND" : "NOT FOUND");
                    
                    if (pendingUserTuple is null)
                    {
                        _errorMessage = "Session expired or invalid. Please log in again.";
                        _logger.LogError("Final attempt failed - redirecting to login");
                        
                        // Use JavaScript redirect instead of Blazor navigation to avoid NavigationException
                        await Task.Delay(100);
                        StateHasChanged();
                        
                        // Don't use Navigation.NavigateTo here as it causes NavigationException
                        // Instead, we'll let the user see the error and provide a link
                        return;
                    }
                }
            }

            var (pendingUser, userType) = pendingUserTuple.Value;
            _logger.LogInformation("Retrieved pending 2FA user: {UserCode} ({UserType})", pendingUser.Code, userType.Value);
            
            _userDisplayName = pendingUser.UserName;
            _userSecretKey = pendingUser.TwoFactorSecretKey ?? string.Empty;

            // Check if user needs to set up 2FA (no secret key)
            if (string.IsNullOrEmpty(_userSecretKey))
            {
                _logger.LogInformation("User {User} needs to set up 2FA - showing setup page", pendingUser.Code);
                _showSetupPage = true;
                await GenerateQRCodeAsync(pendingUser);
            }
            else
            {
                _logger.LogInformation("User {User} has 2FA already set up - showing verification page", pendingUser.Code);
                _showSetupPage = false;
                StartTotpTimer();
            }
            
            // ?? START 2FA SESSION TIMER - User has 10 minutes to complete 2FA verification
            _twoFactorTimer.StartTimer();
            _logger.LogInformation("2FA session timer started - user has {TimeoutMinutes} minutes to complete verification", 10);
            
            // Force a state change to ensure UI updates
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing 2FA verification page");
            _errorMessage = "An error occurred loading the 2FA page. Please try logging in again.";
            await Task.Delay(2000); // Show error message longer
            _navigation.NavigateTo("/login", forceLoad: true);
        }
    }

    /// <summary>
    /// Generate QR code URL for 2FA setup using Radzen component
    /// </summary>
    private async Task GenerateQRCodeAsync(BaseUser user)
    {
        try
        {
            _logger.LogInformation("Generating QR code for 2FA setup for user: {User}", user.Code);

            // Generate new secret key for setup
            _userSecretKey = _twoFactorAuthService.GenerateSecretKey();
            _manualEntryCode = _userSecretKey;

            // Generate QR code URL for Radzen component
            _qrCodeUrl = _twoFactorAuthService.GenerateQrCodeUri(
                userEmail: user.UserName.Value,
                secretKey: _userSecretKey
            );

            _logger.LogInformation("QR code URL generated successfully for user: {User}", user.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for user: {User}", user.Code);
            _errorMessage = "Failed to generate QR code. Please contact IT support.";
        }
        await Task.CompletedTask; // Ensure async method
    }

    /// <summary>
    /// Complete 2FA setup after user scans QR code
    /// </summary>
    private async Task CompleteSetupAsync(SetupFormModel model)
    {
        try
        {
            _isLoading = true;
            _errorMessage = string.Empty;
            StateHasChanged();

            var pendingUserTuple = _sessionService.GetPending2FAUser();
            if (pendingUserTuple is null)
            {
                _errorMessage = "Session expired. Please log in again.";
                _navigation.NavigateTo("/login", forceLoad: true);
                return;
            }

            var (pendingUser, userType) = pendingUserTuple.Value;
            _logger.LogInformation("Completing 2FA setup for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // Validate the verification code
            var isValid = _twoFactorAuthService.ValidateTotpCode(_userSecretKey, model.Code);
            if (!isValid)
            {
                _logger.LogWarning("Invalid 2FA setup verification code for user: {User} ({UserType})", pendingUser.Code, userType.Value);
                _errorMessage = "Invalid verification code. Please check your authenticator app and try again.";
                _setupModel.Code = string.Empty;
                return;
            }

            // Generate backup codes
            var backupCodes = _twoFactorAuthService.GenerateBackupCodes();
            var backupCodesJson = JsonSerializer.Serialize(backupCodes);

            // ?? SMART REPOSITORY USAGE - Save 2FA setup using the correct repository based on user type
            var setupResult = await Setup2FAAsync(pendingUser.Code, _userSecretKey, backupCodesJson, userType);

            if (setupResult.IsFailure)
            {
                _logger.LogError("Failed to save 2FA setup for user: {User} ({UserType}) - Error: {Error}", 
                    pendingUser.Code, userType.Value, setupResult.Error?.Message);
                _errorMessage = "Failed to save 2FA setup. Please try again.";
                return;
            }

            _logger.LogInformation("2FA setup completed successfully for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // ?? CRITICAL FIX: Refresh user data from database after 2FA setup
            _logger.LogInformation("Refreshing user data after 2FA setup to ensure complete user object...");
            var refreshedUserResult = await RefreshUserFromDatabaseAsync(pendingUser.Code, userType);
            if (refreshedUserResult.IsFailure || refreshedUserResult.Value is null)
            {
                _logger.LogError("Failed to refresh user data after 2FA setup - using original user object");
                // Use original user, but manually update the 2FA fields
                pendingUser.TwoFactorSecretKey = _userSecretKey;
                pendingUser.TwoFactorEnabled = true;
                pendingUser.BackupCodes = backupCodesJson;
                pendingUser.TwoFactorSetupDate = DateTime.UtcNow;
            }
            else
            {
                _logger.LogInformation("User data refreshed successfully after 2FA setup");
                pendingUser = refreshedUserResult.Value; // Use refreshed user with complete 2FA data
            }

            // Complete login process with refreshed user data
            await CompleteLoginAsync(pendingUser, userType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing 2FA setup");
            _errorMessage = "An error occurred during setup. Please try again.";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handle 2FA verification form submission
    /// </summary>
    private async Task HandleVerificationAsync(TwoFactorFormModel model)
    {
        try
        {
            _isLoading = true;
            _errorMessage = string.Empty;
            StateHasChanged();

            var pendingUserTuple = _sessionService.GetPending2FAUser();
            if (pendingUserTuple is null)
            {
                _errorMessage = "Session expired. Please log in again.";
                _navigation.NavigateTo("/login", forceLoad: true);
                return;
            }

            var (pendingUser, userType) = pendingUserTuple.Value;
            _logger.LogInformation("Verifying 2FA code for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // Validate TOTP code
            var isValid = _twoFactorAuthService.ValidateTotpCode(_userSecretKey, model.Code);
            if (isValid)
            {
                _logger.LogInformation("2FA verification successful for user: {User} ({UserType})", pendingUser.Code, userType.Value);

                // ?? SMART REPOSITORY USAGE - Reset failed attempts using the correct repository
                var resetResult = await Reset2FAFailedAttemptsAsync(pendingUser.Code, userType);
                if (resetResult.IsFailure)
                {
                    _logger.LogWarning("Failed to reset 2FA attempts for user: {User} ({UserType}) - continuing with login", 
                        pendingUser.Code, userType.Value);
                }

                // Complete login
                await CompleteLoginAsync(pendingUser, userType);
            }
            else
            {
                _logger.LogWarning("Invalid 2FA code for user: {User} ({UserType})", pendingUser.Code, userType.Value);

                // Increment failed attempts
                var failedAttempts = pendingUser.FailedTwoFactorAttempts + 1;
                DateTime? lockoutUntil = null;

                if (failedAttempts >= 5)
                {
                    lockoutUntil = DateTime.UtcNow.AddMinutes(15);
                    _logger.LogWarning("User {User} ({UserType}) locked out due to too many failed 2FA attempts", 
                        pendingUser.Code, userType.Value);
                }

                // ?? SMART REPOSITORY USAGE - Update failed attempts using the correct repository
                var updateResult = await Update2FAFailedAttemptsAsync(pendingUser.Code, failedAttempts, lockoutUntil, userType);
                if (updateResult.IsFailure)
                {
                    _logger.LogError("Failed to update 2FA failed attempts for user: {User} ({UserType}) - Error: {Error}",
                        pendingUser.Code, userType.Value, updateResult.Error?.Message);
                }

                _errorMessage = lockoutUntil.HasValue 
                    ? "Too many failed attempts. Account locked for 15 minutes."
                    : "Invalid verification code. Please try again.";
                
                _twoFactorModel.Code = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification");
            _errorMessage = "An error occurred during verification. Please try again.";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Complete login process after successful 2FA
    /// </summary>
    private async Task CompleteLoginAsync(BaseUser user, SMSUserType userType)
    {
        try
        {
            _logger.LogInformation("Completing login process for user: {User}", user.Code);

            // ?? STOP 2FA TIMER
            _twoFactorTimer.StopTimer();

            // ?? CRITICAL: Clear pending 2FA data FIRST
            _logger.LogInformation("??? Clearing pending 2FA user data...");
            await _sessionService.ClearPending2FAUserAsync();
            
            // Verify clearing was successful
            await Task.Delay(200);
            var verifyCleared = _sessionService.GetPending2FAUser();
            if (verifyCleared is not null)
            {
                _logger.LogWarning("Pending 2FA data not fully cleared, forcing additional cleanup...");
                await _sessionService.ClearPending2FAUserAsync();
                await Task.Delay(300);
            }
            _logger.LogInformation("Pending 2FA user data cleared successfully");

            // ?? CRITICAL FIX: Create full SMS session with multiple retry attempts and fallback strategies
            _logger.LogInformation("Creating full SMS session...");
            var sessionCreated = false;
            var maxAttempts = 5;
            Exception? lastException = null;
            
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    _logger.LogInformation("Session creation attempt {Attempt} of {MaxAttempts}", attempt, maxAttempts);
                    
                    // Add progressive delay to allow system state to settle
                    if (attempt > 1)
                    {
                        var delayMs = 200 * attempt;
                        _logger.LogInformation("Waiting {DelayMs}ms before retry...", delayMs);
                        await Task.Delay(delayMs);
                    }
                    
                    await _sessionService.CreateSMSSessionAsync(user, userType);
                    sessionCreated = true;
                    _logger.LogInformation("Full SMS session created successfully on attempt {Attempt}", attempt);
                    break;
                }
                catch (Exception sessionEx)
                {
                    lastException = sessionEx;
                    _logger.LogWarning(sessionEx, "Session creation failed on attempt {Attempt}: {Error}", attempt, sessionEx.Message);
                    
                    // If this isn't the last attempt, continue trying
                    if (attempt < maxAttempts)
                    {
                        _logger.LogInformation("Will retry session creation...");
                    }
                }
            }
            
            if (!sessionCreated)
            {
                _logger.LogError(lastException, "CRITICAL: Failed to create SMS session after {MaxAttempts} attempts", maxAttempts);

                // Do not navigate when session creation failed.
                // This previously caused users to land on Home without an authenticated NavMenu state.
                var isAuthenticatedAfterRetries = _sessionService.IsAuthenticated();
                if (!isAuthenticatedAfterRetries)
                {
                    _errorMessage = "2FA verification succeeded, but sign-in session could not be established. Please try signing in again.";
                    _logger.LogError("Blocking navigation because no authenticated session exists after 2FA completion");
                    return;
                }

                _logger.LogWarning("Session creation retries failed, but authentication state is present; continuing to navigation");
            }

            // Start session timer regardless of session creation success
            // The timer service should handle missing sessions gracefully
            try
            {
                _sessionTimerService.StartTimer();
                _logger.LogInformation("Session timer started");
            }
            catch (Exception timerEx)
            {
                _logger.LogWarning(timerEx, "Failed to start session timer, continuing anyway");
            }

            _logger.LogInformation("Login completed for user: {User} (SessionCreated: {SessionCreated})", user.Code, sessionCreated);

            // Force multiple state changes to ensure UI updates
            await InvokeAsync(StateHasChanged);
            await Task.Delay(50);
            await InvokeAsync(StateHasChanged);
            
            // Navigate with a small delay to ensure state changes are processed
            await Task.Delay(100);
            _logger.LogInformation("Navigating to home page without force reload to preserve Blazor auth state...");
            _navigation.NavigateTo("/", forceLoad: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing login process for user: {UserCode}", user.Code);
            _errorMessage = "Login verification successful, but there was an error completing the process. You may already be logged in.";
            
            // Force navigation anyway - user might still be authenticated
            await Task.Delay(2000);
            _logger.LogInformation("Navigating to home page after error without force reload...");
            _navigation.NavigateTo("/", forceLoad: false);
        }
    }

    #region Helper Methods

    /// <summary>
    /// ?? SMART REPOSITORY RESOLVER - Returns the correct repository based on SMSUserType
    /// </summary>
    private object GetUserRepositoryForType(SMSUserType userType)
    {
        if (userType == SMSUserType.Application)
            return _applicationUserRepository;
        else if (userType == SMSUserType.Organizational)
            return _organizationalUserRepository;
        else if (userType == SMSUserType.Stakeholder)
            return _stakeholderUserRepository;
        else
            throw new ArgumentException($"Unknown user type: {userType.Value}");
    }

    /// <summary>
    /// ?? SMART 2FA SETUP - Saves 2FA setup using the correct repository
    /// </summary>
    private async Task<Result> Setup2FAAsync(string userCode, string secretKey, string backupCodesJson, SMSUserType userType)
    {
        _logger.LogInformation("Setting up 2FA for user {UserCode} with type {UserType}", userCode, userType.Value);
        var updatedBy = ResolveAuditActor(userCode);
        
        if (userType == SMSUserType.Application)
            return await _applicationUserRepository.Setup2FAAsync(userCode, secretKey, backupCodesJson, updatedBy);
        else if (userType == SMSUserType.Organizational)
            return await _organizationalUserRepository.Setup2FAAsync(userCode, secretKey, backupCodesJson, updatedBy);
        else if (userType == SMSUserType.Stakeholder)
            return await _stakeholderUserRepository.Setup2FAAsync(userCode, secretKey, backupCodesJson, updatedBy);
        else
            return Result.Failure(DomainErrors.BaseUserError.InvalidUserType);
    }

    /// <summary>
    /// ?? SMART 2FA RESET - Resets failed attempts using the correct repository
    /// </summary>
    private async Task<Result> Reset2FAFailedAttemptsAsync(string userCode, SMSUserType userType)
    {
        _logger.LogInformation("Resetting 2FA failed attempts for user {UserCode} with type {UserType}", userCode, userType.Value);
        var updatedBy = ResolveAuditActor(userCode);
        
        if (userType == SMSUserType.Application)
            return await _applicationUserRepository.Reset2FAFailedAttemptsAsync(userCode, updatedBy);
        else if (userType == SMSUserType.Organizational)
            return await _organizationalUserRepository.Reset2FAFailedAttemptsAsync(userCode, updatedBy);
        else if (userType == SMSUserType.Stakeholder)
            return await _stakeholderUserRepository.Reset2FAFailedAttemptsAsync(userCode, updatedBy);
        else
            return Result.Failure(DomainErrors.BaseUserError.InvalidUserType);
    }

    /// <summary>
    /// ?? SMART 2FA UPDATE - Updates failed attempts using the correct repository
    /// </summary>
    private async Task<Result> Update2FAFailedAttemptsAsync(string userCode, int failedAttempts, DateTime? lockoutUntil, SMSUserType userType)
    {
        _logger.LogInformation("Updating 2FA failed attempts for user {UserCode} with type {UserType} - Attempts: {Attempts}", 
            userCode, userType.Value, failedAttempts);
        var updatedBy = ResolveAuditActor(userCode);
        
        if (userType == SMSUserType.Application)
            return await _applicationUserRepository.Update2FAFailedAttemptsAsync(userCode, failedAttempts, lockoutUntil, updatedBy);
        else if (userType == SMSUserType.Organizational)
            return await _organizationalUserRepository.Update2FAFailedAttemptsAsync(userCode, failedAttempts, lockoutUntil, updatedBy);
        else if (userType == SMSUserType.Stakeholder)
            return await _stakeholderUserRepository.Update2FAFailedAttemptsAsync(userCode, failedAttempts, lockoutUntil, updatedBy);
        else
            return Result.Failure(DomainErrors.BaseUserError.InvalidUserType);
    }

    private string ResolveAuditActor(string fallbackUserCode)
    {
        var userCode = _currentUserService.UserCode;
        return string.IsNullOrWhiteSpace(userCode) ? fallbackUserCode : userCode;
    }

    /// <summary>
    /// ?? REFRESH USER - Retrieves fresh user data from database after 2FA setup
    /// </summary>
    private async Task<Result<BaseUser>> RefreshUserFromDatabaseAsync(string userCode, SMSUserType userType)
    {
        try
        {
            _logger.LogInformation("Refreshing user {UserCode} with type {UserType} from database", userCode, userType.Value);
            
            if (userType == SMSUserType.Application)
            {
                var result = await _applicationUserRepository.GetByCodeAsync(userCode);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Application user refreshed successfully from database");
                    return Result<BaseUser>.Success((BaseUser)result.Value);
                }
                return Result<BaseUser>.Failure<BaseUser>(result.Error);
            }
            else if (userType == SMSUserType.Organizational)
            {
                var result = await _organizationalUserRepository.GetByCodeAsync(userCode);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Organizational user refreshed successfully from database");
                    return Result<BaseUser>.Success((BaseUser)result.Value);
                }
                return Result<BaseUser>.Failure<BaseUser>(result.Error);
            }
            else if (userType == SMSUserType.Stakeholder)
            {
                var result = await _stakeholderUserRepository.GetByCodeAsync(userCode);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Stakeholder user refreshed successfully from database");
                    return Result<BaseUser>.Success((BaseUser)result.Value);
                }
                return Result<BaseUser>.Failure<BaseUser>(result.Error);
            }
            else
            {
                return Result<BaseUser>.Failure<BaseUser>(DomainErrors.BaseUserError.InvalidUserType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing user {UserCode} from database", userCode);
            return Result<BaseUser>.Failure<BaseUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    private void ShowBackupOptions()
    {
        _showBackupMethods = !_showBackupMethods;
        _showBackupCodeEntry = false;
        _logger.LogInformation("Backup options {Status}", _showBackupMethods ? "shown" : "hidden");
    }

    private void ShowBackupCodeInput()
    {
        _showBackupCodeEntry = true;
        _logger.LogInformation("Backup code input shown");
    }

    private async Task VerifyBackupCode()
    {
        // TODO: Implement backup code verification
        _logger.LogInformation("Backup code verification requested");
        _errorMessage = "Backup code verification is not yet implemented. Please use your authenticator app.";
        await Task.CompletedTask;
    }

    private void ContactSupport()
    {
        _logger.LogInformation("IT support contact requested");
        _errorMessage = "Please contact IT Support at it-support@flypdx.com for assistance with 2FA.";
    }

    private void StartTotpTimer()
    {
        _timeRemainingInWindow = _twoFactorAuthService.GetTimeRemainingInWindow();
        
        _timeRemainingTimer = new Timer(async _ =>
        {
            var newTimeRemaining = _twoFactorAuthService.GetTimeRemainingInWindow();
            
            // Update every second as requested - keep the smooth countdown
            _timeRemainingInWindow = newTimeRemaining;
            await InvokeAsync(StateHasChanged);
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private string GetTimeRemaining()
    {
        return $"{_timeRemainingInWindow}s";
    }

    private string GetCurrentTotpCode()
    {
        if (string.IsNullOrEmpty(_userSecretKey)) return "000000";
        return _twoFactorAuthService.GetCurrentTotpCode(_userSecretKey);
    }

    private async Task CopyToClipboard()
    {
        // TODO: Implement clipboard copy functionality using IJSRuntime
        _logger.LogInformation("Copy to clipboard requested for manual entry code");
        await Task.CompletedTask;
    }

    #endregion

    #region TwoFactorTimeoutModal Event Handlers

    /// <summary>
    /// Handle "Start Over" button from TwoFactorTimeoutModal
    /// </summary>
    private async Task HandleStartOver()
    {
        try
        {
            _logger.LogInformation("User chose to start over from 2FA timeout modal");
            
            // Stop the 2FA timer
            _twoFactorTimer.StopTimer();
            
            // Clear pending 2FA session
            await _sessionService.ClearPending2FAUserAsync();
            
            // Navigate back to login
            _navigation.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling start over from 2FA timeout modal");
            // Fallback - force navigate to login
            _navigation.NavigateTo("/login", forceLoad: true);
        }
    }

    /// <summary>
    /// Handle "Dismiss Warning" from TwoFactorTimeoutModal  
    /// </summary>
    private async Task HandleDismissWarning()
    {
        try
        {
            _logger.LogInformation("User dismissed 2FA timeout warning - continuing with verification");
            
            // User dismissed the warning but wants to continue
            // The timer continues running in the background
            // No action needed - just log the event
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling dismiss warning from 2FA timeout modal");
        }
    }

    #endregion

    public void Dispose()
    {
        _timeRemainingTimer?.Dispose();
        _twoFactorTimer?.StopTimer(); // ?? Ensure 2FA timer is stopped on page disposal
    }

    public class TwoFactorFormModel
    {
        [Required(ErrorMessage = "Verification code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }

    public class SetupFormModel
    {
        [Required(ErrorMessage = "Verification code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }
}


