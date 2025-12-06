using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Shared.Common;
using SMS_Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SMS3.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;

    private LoginFormModel LoginModel { get; set; } = new();
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsLoading { get; set; } = false;

    protected override void OnInitialized()
    {
        // If already authenticated, redirect to home
        if (SessionService.IsAuthenticated())
        {
            Logger.LogInformation("User already authenticated, redirecting to home");
            Navigation.NavigateTo("/");
        }
    }

    private async Task HandleLoginAsync(LoginFormModel model)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            StateHasChanged();

            Logger.LogInformation("Attempting login for user: {Username}", model.Username);

            // First validate credentials
            var credentialsQuery = new ValidateSMSApplicationUserCredentialsQuery(model.Username, model.Password);
            var credentialsResult = await Mediator.SendAsync(credentialsQuery, CancellationToken.None);

            if (credentialsResult.IsSuccess && credentialsResult.Value)
            {
                // Get user details
                var userQuery = new GetSMSApplicationUserByUserNameQuery(model.Username);
                var userResult = await Mediator.SendAsync(userQuery, CancellationToken.None);

                if (userResult.IsSuccess && userResult.Value != null)
                {
                    var user = userResult.Value;
                    Logger.LogInformation("Login successful for user: {Username}", model.Username);

                    try
                    {
                        // **BLAZOR FIX**: Create session before navigation
                        await SessionService.CreateSMSSessionAsync(user, SMSUserType.Application);
                        
                        // **BLAZOR FIX**: Use ForceLoad to trigger a full page reload which will establish the session properly
                        Navigation.NavigateTo("/", forceLoad: true);
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("response has started"))
                    {
                        // Handle the response started case - the session will be established on the next request
                        Logger.LogInformation("Response started - redirecting to complete session setup");
                        Navigation.NavigateTo("/", forceLoad: true);
                    }
                }
                else
                {
                    ErrorMessage = "Failed to retrieve user details";
                    Logger.LogWarning("Failed to retrieve user details for: {Username}", model.Username);
                }
            }
            else
            {
                ErrorMessage = "Invalid username or password";
                Logger.LogWarning("Login failed for user: {Username}", model.Username);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during login for user: {Username}", model.Username);
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