using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Infrastructure.Interfaces;
using System.Text.Json;
using SMS_Domain.Enums;
using SMS_Domain.Errors;


namespace SMS3.Components.Pages;

public partial class Verify2FA : ComponentBase
{
    [Inject] private IBaseMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Verify2FA> Logger { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private TwoFactorAuthService TwoFactorAuthService { get; set; } = default!;
    [Inject] private SessionTimerService SessionTimerService { get; set; } = default!;
    
    // ?? SMART REPOSITORY INJECTION - All three user repositories
    [Inject] private ISMSApplicationUserRepository ApplicationUserRepository { get; set; } = default!;
    [Inject] private ISMSOrganizationalUserRepository OrganizationalUserRepository { get; set; } = default!;
    [Inject] private ISMSStakeholderUserRepository StakeholderUserRepository { get; set; } = default!;
    
    [Inject] private TwoFactorSessionTimerService TwoFactorTimer { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery] public string? User { get; set; }

    private TwoFactorFormModel TwoFactorModel { get; set; } = new();
    private SetupFormModel SetupModel { get; set; } = new();
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
            Logger.LogInformation("2FA Verification page initialized for user: {User}", User);
            Logger.LogInformation("Current URL: {CurrentUrl}", Navigation.Uri);

            // Add delay to ensure session is fully available
            await Task.Delay(200);

            // ?? CRITICAL: Check for session contamination and clean slate verification
            var initialPendingCheck = SessionService.GetPending2FAUser();
            if (initialPendingCheck is not null)
            {
                Logger.LogInformation("Found existing pending 2FA user: {ExistingUser} - verifying it's not contaminated", 
                    initialPendingCheck.Value.User.UserName.Value);
                
                // If we have a query parameter user and it doesn't match the pending user, clear contaminated data
                if (!string.IsNullOrEmpty(User) && initialPendingCheck.Value.User.UserName.Value != User)
                {
                    Logger.LogError("SESSION CONTAMINATION DETECTED: Pending user {PendingUser} does not match URL user {UrlUser}",
                        initialPendingCheck.Value.User.UserName.Value, User);
                    
                    // Clear contaminated data
                    await SessionService.ClearPending2FAUserAsync();
                    await Task.Delay(200);
                    
                    _errorMessage = "Session contamination detected. Please log in again.";
                    Logger.LogError("Redirecting to login due to session contamination");
                    Navigation.NavigateTo("/login", forceLoad: true);
                    return;
                }
            }

            // Get pending 2FA user from session
            var pendingUserTuple = SessionService.GetPending2FAUser();
            Logger.LogInformation("First attempt to get pending user: {Found}", pendingUserTuple is not null ? "FOUND" : "NOT FOUND");
            
            if (pendingUserTuple is null)
            {
                Logger.LogWarning("No pending 2FA user found in session - checking again in 500ms");
                
                // Try again after a short delay in case of timing issues
                await Task.Delay(500);
                pendingUserTuple = SessionService.GetPending2FAUser();
                Logger.LogInformation("Second attempt to get pending user: {Found}", pendingUserTuple is not null ? "FOUND" : "NOT FOUND");
                
                if (pendingUserTuple is null)
                {
                    Logger.LogError("No pending 2FA user found in session after retry");
                    
                    // Try one more time with longer delay
                    await Task.Delay(1000);
                    pendingUserTuple = SessionService.GetPending2FAUser();
                    Logger.LogInformation("Third attempt to get pending user: {Found}", pendingUserTuple is not null ? "FOUND" : "NOT FOUND");
                    
                    if (pendingUserTuple is null)
                    {
                        _errorMessage = "Session expired or invalid. Please log in again.";
                        Logger.LogError("Final attempt failed - redirecting to login");
                        
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
            Logger.LogInformation("Retrieved pending 2FA user: {UserCode} ({UserType})", pendingUser.Code, userType.Value);
            
            _userDisplayName = pendingUser.UserName;
            _userSecretKey = pendingUser.TwoFactorSecretKey ?? string.Empty;

            // Check if user needs to set up 2FA (no secret key)
            if (string.IsNullOrEmpty(_userSecretKey))
            {
                Logger.LogInformation("User {User} needs to set up 2FA - showing setup page", pendingUser.Code);
                _showSetupPage = true;
                await GenerateQRCodeAsync(pendingUser);
            }
            else
            {
                Logger.LogInformation("User {User} has 2FA already set up - showing verification page", pendingUser.Code);
                _showSetupPage = false;
                StartTotpTimer();
            }
            
            // ?? START 2FA SESSION TIMER - User has 10 minutes to complete 2FA verification
            TwoFactorTimer.StartTimer();
            Logger.LogInformation("2FA session timer started - user has {TimeoutMinutes} minutes to complete verification", 10);
            
            // Force a state change to ensure UI updates
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing 2FA verification page");
            _errorMessage = "An error occurred loading the 2FA page. Please try logging in again.";
            await Task.Delay(2000); // Show error message longer
            Navigation.NavigateTo("/login", forceLoad: true);
        }
    }

    /// <summary>
    /// Generate QR code URL for 2FA setup using Radzen component
    /// </summary>
    private async Task GenerateQRCodeAsync(BaseUser user)
    {
        try
        {
            Logger.LogInformation("Generating QR code for 2FA setup for user: {User}", user.Code);

            // Generate new secret key for setup
            _userSecretKey = TwoFactorAuthService.GenerateSecretKey();
            _manualEntryCode = _userSecretKey;

            // Generate QR code URL for Radzen component
            _qrCodeUrl = TwoFactorAuthService.GenerateQrCodeUri(
                userEmail: user.UserName.Value,
                secretKey: _userSecretKey
            );

            Logger.LogInformation("QR code URL generated successfully for user: {User}", user.Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating QR code for user: {User}", user.Code);
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

            var pendingUserTuple = SessionService.GetPending2FAUser();
            if (pendingUserTuple is null)
            {
                _errorMessage = "Session expired. Please log in again.";
                Navigation.NavigateTo("/login", forceLoad: true);
                return;
            }

            var (pendingUser, userType) = pendingUserTuple.Value;
            Logger.LogInformation("Completing 2FA setup for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // Validate the verification code
            var isValid = TwoFactorAuthService.ValidateTotpCode(_userSecretKey, model.Code);
            if (!isValid)
            {
                Logger.LogWarning("Invalid 2FA setup verification code for user: {User} ({UserType})", pendingUser.Code, userType.Value);
                _errorMessage = "Invalid verification code. Please check your authenticator app and try again.";
                SetupModel.Code = string.Empty;
                return;
            }

            // Generate backup codes
            var backupCodes = TwoFactorAuthService.GenerateBackupCodes();
            var backupCodesJson = JsonSerializer.Serialize(backupCodes);

            // ?? SMART REPOSITORY USAGE - Save 2FA setup using the correct repository based on user type
            var setupResult = await Setup2FAAsync(pendingUser.Code, _userSecretKey, backupCodesJson, userType);

            if (setupResult.IsFailure)
            {
                Logger.LogError("Failed to save 2FA setup for user: {User} ({UserType}) - Error: {Error}", 
                    pendingUser.Code, userType.Value, setupResult.Error?.Message);
                _errorMessage = "Failed to save 2FA setup. Please try again.";
                return;
            }

            Logger.LogInformation("2FA setup completed successfully for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // ?? CRITICAL FIX: Refresh user data from database after 2FA setup
            Logger.LogInformation("Refreshing user data after 2FA setup to ensure complete user object...");
            var refreshedUserResult = await RefreshUserFromDatabaseAsync(pendingUser.Code, userType);
            if (refreshedUserResult.IsFailure || refreshedUserResult.Value is null)
            {
                Logger.LogError("Failed to refresh user data after 2FA setup - using original user object");
                // Use original user, but manually update the 2FA fields
                pendingUser.TwoFactorSecretKey = _userSecretKey;
                pendingUser.TwoFactorEnabled = true;
                pendingUser.BackupCodes = backupCodesJson;
                pendingUser.TwoFactorSetupDate = DateTime.UtcNow;
            }
            else
            {
                Logger.LogInformation("User data refreshed successfully after 2FA setup");
                pendingUser = refreshedUserResult.Value; // Use refreshed user with complete 2FA data
            }

            // Complete login process with refreshed user data
            await CompleteLoginAsync(pendingUser, userType);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing 2FA setup");
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

            var pendingUserTuple = SessionService.GetPending2FAUser();
            if (pendingUserTuple is null)
            {
                _errorMessage = "Session expired. Please log in again.";
                Navigation.NavigateTo("/login", forceLoad: true);
                return;
            }

            var (pendingUser, userType) = pendingUserTuple.Value;
            Logger.LogInformation("Verifying 2FA code for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // Validate TOTP code
            var isValid = TwoFactorAuthService.ValidateTotpCode(_userSecretKey, model.Code);
            if (isValid)
            {
                Logger.LogInformation("2FA verification successful for user: {User} ({UserType})", pendingUser.Code, userType.Value);

                // ?? SMART REPOSITORY USAGE - Reset failed attempts using the correct repository
                var resetResult = await Reset2FAFailedAttemptsAsync(pendingUser.Code, userType);
                if (resetResult.IsFailure)
                {
                    Logger.LogWarning("Failed to reset 2FA attempts for user: {User} ({UserType}) - continuing with login", 
                        pendingUser.Code, userType.Value);
                }

                // Complete login
                await CompleteLoginAsync(pendingUser, userType);
            }
            else
            {
                Logger.LogWarning("Invalid 2FA code for user: {User} ({UserType})", pendingUser.Code, userType.Value);

                // Increment failed attempts
                var failedAttempts = pendingUser.FailedTwoFactorAttempts + 1;
                DateTime? lockoutUntil = null;

                if (failedAttempts >= 5)
                {
                    lockoutUntil = DateTime.UtcNow.AddMinutes(15);
                    Logger.LogWarning("User {User} ({UserType}) locked out due to too many failed 2FA attempts", 
                        pendingUser.Code, userType.Value);
                }

                // ?? SMART REPOSITORY USAGE - Update failed attempts using the correct repository
                var updateResult = await Update2FAFailedAttemptsAsync(pendingUser.Code, failedAttempts, lockoutUntil, userType);
                if (updateResult.IsFailure)
                {
                    Logger.LogError("Failed to update 2FA failed attempts for user: {User} ({UserType}) - Error: {Error}",
                        pendingUser.Code, userType.Value, updateResult.Error?.Message);
                }

                _errorMessage = lockoutUntil.HasValue 
                    ? "Too many failed attempts. Account locked for 15 minutes."
                    : "Invalid verification code. Please try again.";
                
                TwoFactorModel.Code = string.Empty;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during 2FA verification");
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
            Logger.LogInformation("Completing login process for user: {User}", user.Code);

            // ?? STOP 2FA TIMER
            TwoFactorTimer.StopTimer();

            // ?? CRITICAL: Clear pending 2FA data FIRST
            Logger.LogInformation("??? Clearing pending 2FA user data...");
            await SessionService.ClearPending2FAUserAsync();
            
            // Verify clearing was successful
            await Task.Delay(200);
            var verifyCleared = SessionService.GetPending2FAUser();
            if (verifyCleared is not null)
            {
                Logger.LogWarning("Pending 2FA data not fully cleared, forcing additional cleanup...");
                await SessionService.ClearPending2FAUserAsync();
                await Task.Delay(300);
            }
            Logger.LogInformation("Pending 2FA user data cleared successfully");

            // ?? CRITICAL FIX: Create full SMS session with multiple retry attempts and fallback strategies
            Logger.LogInformation("Creating full SMS session...");
            var sessionCreated = false;
            var maxAttempts = 5;
            Exception? lastException = null;
            
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    Logger.LogInformation("Session creation attempt {Attempt} of {MaxAttempts}", attempt, maxAttempts);
                    
                    // Add progressive delay to allow system state to settle
                    if (attempt > 1)
                    {
                        var delayMs = 200 * attempt;
                        Logger.LogInformation("Waiting {DelayMs}ms before retry...", delayMs);
                        await Task.Delay(delayMs);
                    }
                    
                    await SessionService.CreateSMSSessionAsync(user, userType);
                    sessionCreated = true;
                    Logger.LogInformation("Full SMS session created successfully on attempt {Attempt}", attempt);
                    break;
                }
                catch (Exception sessionEx)
                {
                    lastException = sessionEx;
                    Logger.LogWarning(sessionEx, "Session creation failed on attempt {Attempt}: {Error}", attempt, sessionEx.Message);
                    
                    // If this isn't the last attempt, continue trying
                    if (attempt < maxAttempts)
                    {
                        Logger.LogInformation("Will retry session creation...");
                    }
                }
            }
            
            if (!sessionCreated)
            {
                Logger.LogError(lastException, "CRITICAL: Failed to create SMS session after {MaxAttempts} attempts", maxAttempts);

                // Do not navigate when session creation failed.
                // This previously caused users to land on Home without an authenticated NavMenu state.
                var isAuthenticatedAfterRetries = SessionService.IsAuthenticated();
                if (!isAuthenticatedAfterRetries)
                {
                    _errorMessage = "2FA verification succeeded, but sign-in session could not be established. Please try signing in again.";
                    Logger.LogError("Blocking navigation because no authenticated session exists after 2FA completion");
                    return;
                }

                Logger.LogWarning("Session creation retries failed, but authentication state is present; continuing to navigation");
            }

            // Start session timer regardless of session creation success
            // The timer service should handle missing sessions gracefully
            try
            {
                SessionTimerService.StartTimer();
                Logger.LogInformation("Session timer started");
            }
            catch (Exception timerEx)
            {
                Logger.LogWarning(timerEx, "Failed to start session timer, continuing anyway");
            }

            Logger.LogInformation("Login completed for user: {User} (SessionCreated: {SessionCreated})", user.Code, sessionCreated);

            // Force multiple state changes to ensure UI updates
            await InvokeAsync(StateHasChanged);
            await Task.Delay(50);
            await InvokeAsync(StateHasChanged);
            
            // Navigate with a small delay to ensure state changes are processed
            await Task.Delay(100);
            Logger.LogInformation("Navigating to home page without force reload to preserve Blazor auth state...");
            Navigation.NavigateTo("/", forceLoad: false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing login process for user: {UserCode}", user.Code);
            _errorMessage = "Login verification successful, but there was an error completing the process. You may already be logged in.";
            
            // Force navigation anyway - user might still be authenticated
            await Task.Delay(2000);
            Logger.LogInformation("Navigating to home page after error without force reload...");
            Navigation.NavigateTo("/", forceLoad: false);
        }
    }

    #region Helper Methods

    /// <summary>
    /// ?? SMART REPOSITORY RESOLVER - Returns the correct repository based on SMSUserType
    /// </summary>
    private object GetUserRepositoryForType(SMSUserType userType)
    {
        if (userType == SMSUserType.Application)
            return ApplicationUserRepository;
        else if (userType == SMSUserType.Organizational)
            return OrganizationalUserRepository;
        else if (userType == SMSUserType.Stakeholder)
            return StakeholderUserRepository;
        else
            throw new ArgumentException($"Unknown user type: {userType.Value}");
    }

    /// <summary>
    /// ?? SMART 2FA SETUP - Saves 2FA setup using the correct repository
    /// </summary>
    private async Task<Result> Setup2FAAsync(string userCode, string secretKey, string backupCodesJson, SMSUserType userType)
    {
        Logger.LogInformation("Setting up 2FA for user {UserCode} with type {UserType}", userCode, userType.Value);
        
        if (userType == SMSUserType.Application)
            return await ApplicationUserRepository.Setup2FAAsync(userCode, secretKey, backupCodesJson);
        else if (userType == SMSUserType.Organizational)
            return await OrganizationalUserRepository.Setup2FAAsync(userCode, secretKey, backupCodesJson);
        else if (userType == SMSUserType.Stakeholder)
            return await StakeholderUserRepository.Setup2FAAsync(userCode, secretKey, backupCodesJson);
        else
            return Result.Failure(DomainErrors.BaseUserError.InvalidUserType);
    }

    /// <summary>
    /// ?? SMART 2FA RESET - Resets failed attempts using the correct repository
    /// </summary>
    private async Task<Result> Reset2FAFailedAttemptsAsync(string userCode, SMSUserType userType)
    {
        Logger.LogInformation("Resetting 2FA failed attempts for user {UserCode} with type {UserType}", userCode, userType.Value);
        
        if (userType == SMSUserType.Application)
            return await ApplicationUserRepository.Reset2FAFailedAttemptsAsync(userCode);
        else if (userType == SMSUserType.Organizational)
            return await OrganizationalUserRepository.Reset2FAFailedAttemptsAsync(userCode);
        else if (userType == SMSUserType.Stakeholder)
            return await StakeholderUserRepository.Reset2FAFailedAttemptsAsync(userCode);
        else
            return Result.Failure(DomainErrors.BaseUserError.InvalidUserType);
    }

    /// <summary>
    /// ?? SMART 2FA UPDATE - Updates failed attempts using the correct repository
    /// </summary>
    private async Task<Result> Update2FAFailedAttemptsAsync(string userCode, int failedAttempts, DateTime? lockoutUntil, SMSUserType userType)
    {
        Logger.LogInformation("Updating 2FA failed attempts for user {UserCode} with type {UserType} - Attempts: {Attempts}", 
            userCode, userType.Value, failedAttempts);
        
        if (userType == SMSUserType.Application)
            return await ApplicationUserRepository.Update2FAFailedAttemptsAsync(userCode, failedAttempts, lockoutUntil);
        else if (userType == SMSUserType.Organizational)
            return await OrganizationalUserRepository.Update2FAFailedAttemptsAsync(userCode, failedAttempts, lockoutUntil);
        else if (userType == SMSUserType.Stakeholder)
            return await StakeholderUserRepository.Update2FAFailedAttemptsAsync(userCode, failedAttempts, lockoutUntil);
        else
            return Result.Failure(DomainErrors.BaseUserError.InvalidUserType);
    }

    /// <summary>
    /// ?? REFRESH USER - Retrieves fresh user data from database after 2FA setup
    /// </summary>
    private async Task<Result<BaseUser>> RefreshUserFromDatabaseAsync(string userCode, SMSUserType userType)
    {
        try
        {
            Logger.LogInformation("Refreshing user {UserCode} with type {UserType} from database", userCode, userType.Value);
            
            if (userType == SMSUserType.Application)
            {
                var result = await ApplicationUserRepository.GetByCodeAsync(userCode);
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Application user refreshed successfully from database");
                    return Result<BaseUser>.Success((BaseUser)result.Value);
                }
                return Result<BaseUser>.Failure<BaseUser>(result.Error);
            }
            else if (userType == SMSUserType.Organizational)
            {
                var result = await OrganizationalUserRepository.GetByCodeAsync(userCode);
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Organizational user refreshed successfully from database");
                    return Result<BaseUser>.Success((BaseUser)result.Value);
                }
                return Result<BaseUser>.Failure<BaseUser>(result.Error);
            }
            else if (userType == SMSUserType.Stakeholder)
            {
                var result = await StakeholderUserRepository.GetByCodeAsync(userCode);
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Stakeholder user refreshed successfully from database");
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
            Logger.LogError(ex, "Error refreshing user {UserCode} from database", userCode);
            return Result<BaseUser>.Failure<BaseUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    private void ShowBackupOptions()
    {
        _showBackupMethods = !_showBackupMethods;
        _showBackupCodeEntry = false;
        Logger.LogInformation("Backup options {Status}", _showBackupMethods ? "shown" : "hidden");
    }

    private void ShowBackupCodeInput()
    {
        _showBackupCodeEntry = true;
        Logger.LogInformation("Backup code input shown");
    }

    private async Task VerifyBackupCode()
    {
        // TODO: Implement backup code verification
        Logger.LogInformation("Backup code verification requested");
        _errorMessage = "Backup code verification is not yet implemented. Please use your authenticator app.";
        await Task.CompletedTask;
    }

    private void ContactSupport()
    {
        Logger.LogInformation("IT support contact requested");
        _errorMessage = "Please contact IT Support at it-support@flypdx.com for assistance with 2FA.";
    }

    private void StartTotpTimer()
    {
        _timeRemainingInWindow = TwoFactorAuthService.GetTimeRemainingInWindow();
        
        _timeRemainingTimer = new Timer(async _ =>
        {
            var newTimeRemaining = TwoFactorAuthService.GetTimeRemainingInWindow();
            
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
        return TwoFactorAuthService.GetCurrentTotpCode(_userSecretKey);
    }

    private async Task CopyToClipboard()
    {
        // TODO: Implement clipboard copy functionality using IJSRuntime
        Logger.LogInformation("Copy to clipboard requested for manual entry code");
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
            Logger.LogInformation("User chose to start over from 2FA timeout modal");
            
            // Stop the 2FA timer
            TwoFactorTimer.StopTimer();
            
            // Clear pending 2FA session
            await SessionService.ClearPending2FAUserAsync();
            
            // Navigate back to login
            Navigation.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling start over from 2FA timeout modal");
            // Fallback - force navigate to login
            Navigation.NavigateTo("/login", forceLoad: true);
        }
    }

    /// <summary>
    /// Handle "Dismiss Warning" from TwoFactorTimeoutModal  
    /// </summary>
    private async Task HandleDismissWarning()
    {
        try
        {
            Logger.LogInformation("User dismissed 2FA timeout warning - continuing with verification");
            
            // User dismissed the warning but wants to continue
            // The timer continues running in the background
            // No action needed - just log the event
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling dismiss warning from 2FA timeout modal");
        }
    }

    #endregion

    public void Dispose()
    {
        _timeRemainingTimer?.Dispose();
        TwoFactorTimer?.StopTimer(); // ?? Ensure 2FA timer is stopped on page disposal
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


