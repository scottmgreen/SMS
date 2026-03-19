using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using Infrastructure.Interfaces;
using System.Text.Json;
using SMS_Domain.Enums;
using SMS_Domain.Errors;

namespace SMS3.Components.Pages;

public partial class Verify2FA : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Verify2FA> Logger { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private TwoFactorAuthService TwoFactorAuthService { get; set; } = default!;
    [Inject] private SessionTimerService SessionTimerService { get; set; } = default!;
    
    // 🔧 SMART REPOSITORY INJECTION - All three user repositories
    [Inject] private ISMSApplicationUserRepository ApplicationUserRepository { get; set; } = default!;
    [Inject] private ISMSOrganizationalUserRepository OrganizationalUserRepository { get; set; } = default!;
    [Inject] private Infrastructure.Interfaces.ISMSStakeholderUserRepository StakeholderUserRepository { get; set; } = default!;
    
    [Inject] private TwoFactorSessionTimerService TwoFactorTimer { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery] public string? User { get; set; }

    private TwoFactorFormModel TwoFactorModel { get; set; } = new();
    private SetupFormModel SetupModel { get; set; } = new();
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsLoading { get; set; } = false;
    private string BackupCode { get; set; } = string.Empty;
    
    // Page state
    private bool ShowSetupPage { get; set; } = false;
    private bool ShowBackupMethods { get; set; } = false;
    private bool ShowBackupCodeEntry { get; set; } = false;
    
    // QR Code data for Radzen component
    private string QRCodeUrl { get; set; } = string.Empty;
    private string ManualEntryCode { get; set; } = string.Empty;
    private string UserDisplayName { get; set; } = string.Empty;
    private string UserSecretKey { get; set; } = string.Empty;
    
    // Timer for TOTP countdown
    private Timer? _timeRemainingTimer;
    private int TimeRemainingInWindow { get; set; } = 30;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("🔐 2FA Verification page initialized for user: {User}", User);
            Logger.LogInformation("🔐 Current URL: {CurrentUrl}", Navigation.Uri);

            // Add delay to ensure session is fully available
            await Task.Delay(200);

            // Get pending 2FA user from session
            var pendingUserTuple = SessionService.GetPending2FAUser();
            Logger.LogInformation("🔐 First attempt to get pending user: {Found}", pendingUserTuple != null ? "FOUND" : "NOT FOUND");
            
            if (pendingUserTuple == null)
            {
                Logger.LogWarning("⚠️ No pending 2FA user found in session - checking again in 500ms");
                
                // Try again after a short delay in case of timing issues
                await Task.Delay(500);
                pendingUserTuple = SessionService.GetPending2FAUser();
                Logger.LogInformation("🔐 Second attempt to get pending user: {Found}", pendingUserTuple != null ? "FOUND" : "NOT FOUND");
                
                if (pendingUserTuple == null)
                {
                    Logger.LogError("❌ No pending 2FA user found in session after retry");
                    
                    // Try one more time with longer delay
                    await Task.Delay(1000);
                    pendingUserTuple = SessionService.GetPending2FAUser();
                    Logger.LogInformation("🔐 Third attempt to get pending user: {Found}", pendingUserTuple != null ? "FOUND" : "NOT FOUND");
                    
                    if (pendingUserTuple == null)
                    {
                        ErrorMessage = "Session expired or invalid. Please log in again.";
                        Logger.LogError("❌ Final attempt failed - redirecting to login");
                        
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
            Logger.LogInformation("🔐 Retrieved pending 2FA user: {UserCode} ({UserType})", pendingUser.Code, userType.Value);
            
            UserDisplayName = pendingUser.UserName;
            UserSecretKey = pendingUser.TwoFactorSecretKey ?? string.Empty;

            // Check if user needs to set up 2FA (no secret key)
            if (string.IsNullOrEmpty(UserSecretKey))
            {
                Logger.LogInformation("🔐 User {User} needs to set up 2FA - showing setup page", pendingUser.Code);
                ShowSetupPage = true;
                await GenerateQRCodeAsync(pendingUser);
            }
            else
            {
                Logger.LogInformation("🔐 User {User} has 2FA already set up - showing verification page", pendingUser.Code);
                ShowSetupPage = false;
                StartTotpTimer();
            }
            
            // 🔐 START 2FA SESSION TIMER - User has 10 minutes to complete 2FA verification
            TwoFactorTimer.StartTimer();
            Logger.LogInformation("🔐 2FA session timer started - user has {TimeoutMinutes} minutes to complete verification", 10);
            
            // Force a state change to ensure UI updates
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error initializing 2FA verification page");
            ErrorMessage = "An error occurred loading the 2FA page. Please try logging in again.";
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
            Logger.LogInformation("🔐 Generating QR code for 2FA setup for user: {User}", user.Code);

            // Generate new secret key for setup
            UserSecretKey = TwoFactorAuthService.GenerateSecretKey();
            ManualEntryCode = UserSecretKey;

            // Generate QR code URL for Radzen component
            QRCodeUrl = TwoFactorAuthService.GenerateQrCodeUri(
                userEmail: user.UserName.Value,
                secretKey: UserSecretKey
            );

            Logger.LogInformation("✅ QR code URL generated successfully for user: {User}", user.Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error generating QR code for user: {User}", user.Code);
            ErrorMessage = "Failed to generate QR code. Please contact IT support.";
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
            IsLoading = true;
            ErrorMessage = string.Empty;
            StateHasChanged();

            var pendingUserTuple = SessionService.GetPending2FAUser();
            if (pendingUserTuple == null)
            {
                ErrorMessage = "Session expired. Please log in again.";
                Navigation.NavigateTo("/login", forceLoad: true);
                return;
            }

            var (pendingUser, userType) = pendingUserTuple.Value;
            Logger.LogInformation("🔐 Completing 2FA setup for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // Validate the verification code
            var isValid = TwoFactorAuthService.ValidateTotpCode(UserSecretKey, model.Code);
            if (!isValid)
            {
                Logger.LogWarning("❌ Invalid 2FA setup verification code for user: {User} ({UserType})", pendingUser.Code, userType.Value);
                ErrorMessage = "Invalid verification code. Please check your authenticator app and try again.";
                SetupModel.Code = string.Empty;
                return;
            }

            // Generate backup codes
            var backupCodes = TwoFactorAuthService.GenerateBackupCodes();
            var backupCodesJson = JsonSerializer.Serialize(backupCodes);

            // 🔧 SMART REPOSITORY USAGE - Save 2FA setup using the correct repository based on user type
            var setupResult = await Setup2FAAsync(pendingUser.Code, UserSecretKey, backupCodesJson, userType);

            if (setupResult.IsFailure)
            {
                Logger.LogError("❌ Failed to save 2FA setup for user: {User} ({UserType}) - Error: {Error}", 
                    pendingUser.Code, userType.Value, setupResult.Error?.Message);
                ErrorMessage = "Failed to save 2FA setup. Please try again.";
                return;
            }

            Logger.LogInformation("✅ 2FA setup completed successfully for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // Complete login process
            await CompleteLoginAsync(pendingUser, userType);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error completing 2FA setup");
            ErrorMessage = "An error occurred during setup. Please try again.";
        }
        finally
        {
            IsLoading = false;
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
            IsLoading = true;
            ErrorMessage = string.Empty;
            StateHasChanged();

            var pendingUserTuple = SessionService.GetPending2FAUser();
            if (pendingUserTuple == null)
            {
                ErrorMessage = "Session expired. Please log in again.";
                Navigation.NavigateTo("/login", forceLoad: true);
                return;
            }

            var (pendingUser, userType) = pendingUserTuple.Value;
            Logger.LogInformation("🔐 Verifying 2FA code for user: {User} ({UserType})", pendingUser.Code, userType.Value);

            // Validate TOTP code
            var isValid = TwoFactorAuthService.ValidateTotpCode(UserSecretKey, model.Code);
            if (isValid)
            {
                Logger.LogInformation("✅ 2FA verification successful for user: {User} ({UserType})", pendingUser.Code, userType.Value);

                // 🔧 SMART REPOSITORY USAGE - Reset failed attempts using the correct repository
                var resetResult = await Reset2FAFailedAttemptsAsync(pendingUser.Code, userType);
                if (resetResult.IsFailure)
                {
                    Logger.LogWarning("⚠️ Failed to reset 2FA attempts for user: {User} ({UserType}) - continuing with login", 
                        pendingUser.Code, userType.Value);
                }

                // Complete login
                await CompleteLoginAsync(pendingUser, userType);
            }
            else
            {
                Logger.LogWarning("❌ Invalid 2FA code for user: {User} ({UserType})", pendingUser.Code, userType.Value);

                // Increment failed attempts
                var failedAttempts = pendingUser.FailedTwoFactorAttempts + 1;
                DateTime? lockoutUntil = null;

                if (failedAttempts >= 5)
                {
                    lockoutUntil = DateTime.UtcNow.AddMinutes(15);
                    Logger.LogWarning("🔒 User {User} ({UserType}) locked out due to too many failed 2FA attempts", 
                        pendingUser.Code, userType.Value);
                }

                // 🔧 SMART REPOSITORY USAGE - Update failed attempts using the correct repository
                var updateResult = await Update2FAFailedAttemptsAsync(pendingUser.Code, failedAttempts, lockoutUntil, userType);
                if (updateResult.IsFailure)
                {
                    Logger.LogError("❌ Failed to update 2FA failed attempts for user: {User} ({UserType}) - Error: {Error}",
                        pendingUser.Code, userType.Value, updateResult.Error?.Message);
                }

                ErrorMessage = lockoutUntil.HasValue 
                    ? "Too many failed attempts. Account locked for 15 minutes."
                    : "Invalid verification code. Please try again.";
                
                TwoFactorModel.Code = string.Empty;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error during 2FA verification");
            ErrorMessage = "An error occurred during verification. Please try again.";
        }
        finally
        {
            IsLoading = false;
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
            Logger.LogInformation("🔐 Completing login process for user: {User}", user.Code);

            // 🔐 STOP 2FA TIMER - User successfully completed 2FA
            TwoFactorTimer.StopTimer();

            // Clear pending 2FA data
            await SessionService.ClearPending2FAUserAsync();

            // Create full SMS session
            await SessionService.CreateSMSSessionAsync(user, userType);

            // Start session timer
            SessionTimerService.StartTimer();

            Logger.LogInformation("✅ Login completed successfully for user: {User}", user.Code);

            // Navigate to home page
            Navigation.NavigateTo("/", forceLoad: false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error completing login process");
            ErrorMessage = "Login completed but there was an error setting up your session.";
            
            // Still try to navigate
            Navigation.NavigateTo("/", forceLoad: true);
        }
    }

    #region Helper Methods

    /// <summary>
    /// 🔧 SMART REPOSITORY RESOLVER - Returns the correct repository based on SMSUserType
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
    /// 🔧 SMART 2FA SETUP - Saves 2FA setup using the correct repository
    /// </summary>
    private async Task<Result> Setup2FAAsync(string userCode, string secretKey, string backupCodesJson, SMSUserType userType)
    {
        Logger.LogInformation("🔐 Setting up 2FA for user {UserCode} with type {UserType}", userCode, userType.Value);
        
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
    /// 🔧 SMART 2FA RESET - Resets failed attempts using the correct repository
    /// </summary>
    private async Task<Result> Reset2FAFailedAttemptsAsync(string userCode, SMSUserType userType)
    {
        Logger.LogInformation("🔐 Resetting 2FA failed attempts for user {UserCode} with type {UserType}", userCode, userType.Value);
        
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
    /// 🔧 SMART 2FA UPDATE - Updates failed attempts using the correct repository
    /// </summary>
    private async Task<Result> Update2FAFailedAttemptsAsync(string userCode, int failedAttempts, DateTime? lockoutUntil, SMSUserType userType)
    {
        Logger.LogInformation("🔐 Updating 2FA failed attempts for user {UserCode} with type {UserType} - Attempts: {Attempts}", 
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

    private void ShowBackupOptions()
    {
        ShowBackupMethods = !ShowBackupMethods;
        ShowBackupCodeEntry = false;
        Logger.LogInformation("Backup options {Status}", ShowBackupMethods ? "shown" : "hidden");
    }

    private void ShowBackupCodeInput()
    {
        ShowBackupCodeEntry = true;
        Logger.LogInformation("Backup code input shown");
    }

    private async Task VerifyBackupCode()
    {
        // TODO: Implement backup code verification
        Logger.LogInformation("🔐 Backup code verification requested");
        ErrorMessage = "Backup code verification is not yet implemented. Please use your authenticator app.";
        await Task.CompletedTask;
    }

    private void ContactSupport()
    {
        Logger.LogInformation("📞 IT support contact requested");
        ErrorMessage = "Please contact IT Support at it-support@flypdx.com for assistance with 2FA.";
    }

    private void StartTotpTimer()
    {
        TimeRemainingInWindow = TwoFactorAuthService.GetTimeRemainingInWindow();
        
        _timeRemainingTimer = new Timer(async _ =>
        {
            TimeRemainingInWindow = TwoFactorAuthService.GetTimeRemainingInWindow();
            await InvokeAsync(StateHasChanged);
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private string GetTimeRemaining()
    {
        return $"{TimeRemainingInWindow}s";
    }

    private string GetCurrentTotpCode()
    {
        if (string.IsNullOrEmpty(UserSecretKey)) return "000000";
        return TwoFactorAuthService.GetCurrentTotpCode(UserSecretKey);
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
            Logger.LogInformation("🔐 User chose to start over from 2FA timeout modal");
            
            // Stop the 2FA timer
            TwoFactorTimer.StopTimer();
            
            // Clear pending 2FA session
            await SessionService.ClearPending2FAUserAsync();
            
            // Navigate back to login
            Navigation.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "🔐 Error handling start over from 2FA timeout modal");
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
            Logger.LogInformation("🔐 User dismissed 2FA timeout warning - continuing with verification");
            
            // User dismissed the warning but wants to continue
            // The timer continues running in the background
            // No action needed - just log the event
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "🔐 Error handling dismiss warning from 2FA timeout modal");
        }
    }

    #endregion

    public void Dispose()
    {
        _timeRemainingTimer?.Dispose();
        TwoFactorTimer?.StopTimer(); // 🔐 Ensure 2FA timer is stopped on page disposal
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