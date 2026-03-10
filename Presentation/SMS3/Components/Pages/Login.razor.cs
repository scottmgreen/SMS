using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SMS_Application.Interfaces;
using SMS_Application.Services;

namespace SMS3.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = default!;

    private LoginFormModel LoginModel { get; set; } = new();
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsLoading { get; set; } = false;

    private async Task HandleLoginAsync(LoginFormModel model)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            StateHasChanged();

            Logger.LogInformation("Attempting Smart Login for user: {Username}", model.Username);

            var authResult = await AuthenticationService.AuthenticateAsync(model.Username, model.Password, CancellationToken.None);

            if (authResult.IsSuccess && authResult.User != null)
            {
                Logger.LogInformation("Authentication successful for user: {Username}, Type: {UserType}", model.Username, authResult.UserType.Value);

                // 🔐 Record successful authentication audit with simplified data
                try
                {
                    var authSuccessCommand = new RecordAuthenticationSuccessCommand(
                        model.Username,
                        authResult.UserType,
                        authResult.User.DisplayName,
                        "Server", // Simplified IP address
                        "Blazor", // Simplified user agent
                        Guid.NewGuid().ToString() // Session ID
                    );
                    var auditResult = await Mediator.SendAsync(authSuccessCommand, CancellationToken.None);
                    Logger.LogInformation("✅ Authentication success audit recorded for user: {Username}, Result: {IsSuccess}", 
                        model.Username, auditResult.IsSuccess);
                }
                catch (Exception auditEx)
                {
                    Logger.LogError(auditEx, "❌ Failed to record authentication success audit for {Username}", model.Username);
                    // Continue with login even if audit fails
                }

                // Use static authentication service - NO HttpContext dependency
                StaticCurrentUserService.SetAuthenticationState(authResult.User, authResult.UserType);
                Logger.LogInformation("✅ Authentication state stored statically for user: {Username}", model.Username);
                
                // Navigate to home page
                Logger.LogInformation("Navigating to home page after successful authentication");
                Navigation.NavigateTo("/", forceLoad: false);
                return;
            }
            else
            {
                // 🔐 Record failed authentication audit with simplified data
                try
                {
                    var authFailureCommand = new RecordAuthenticationFailureCommand(
                        model.Username,
                        "Invalid username or password",
                        "Server", // Simplified IP address
                        "Blazor", // Simplified user agent
                        1
                    );
                    var auditResult = await Mediator.SendAsync(authFailureCommand, CancellationToken.None);
                    Logger.LogInformation("✅ Authentication failure audit recorded for user: {Username}, Result: {IsSuccess}", 
                        model.Username, auditResult.IsSuccess);
                }
                catch (Exception auditEx)
                {
                    Logger.LogError(auditEx, "❌ Failed to record authentication failure audit for {Username}", model.Username);
                    // Continue - don't block user from seeing error message
                }

                ErrorMessage = authResult.ErrorMessage ?? "Invalid username or password";
                Logger.LogWarning("Authentication failed for user: {Username}", model.Username);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during Smart Login for user: {Username}", model.Username);
            ErrorMessage = "An error occurred during login. Please try again.";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    public class LoginFormModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}