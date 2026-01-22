using System.ComponentModel.DataAnnotations;

namespace SMS3.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private AuthenticationService AuthService { get; set; } = default!;

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

            // **SMART LOGIN**: Discover user type and authenticate
            var (user, userType) = await DiscoverAndAuthenticateUserAsync(model.Username, model.Password);

            if (user != null)
            {
                Logger.LogInformation("Smart Login successful for user: {Username}, Type: {UserType}", model.Username, userType.Value);

                // Set authentication in service
                AuthService.SetAuthentication(user, userType);
                Logger.LogInformation("Authentication set in service for user: {Username}", model.Username);

                // Navigate to dashboard
                Navigation.NavigateTo("/");
            }
            else
            {
                ErrorMessage = "Invalid username or password";
                Logger.LogWarning("Smart Login failed for user: {Username}", model.Username);
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

    /// <summary>
    /// Smart Login: Try each user type sequentially using the same pattern as other login implementations
    /// This matches the logic used in Login.cshtml.cs, LoginController.cs, and SMS_Blazor Login.razor
    /// </summary>
    private async Task<(BaseUser? user, SMSUserType userType)> DiscoverAndAuthenticateUserAsync(string email, string password)
    {
        // Try SMSApplicationUser first
        try
        {
            Logger.LogDebug("Checking Application User: {Email}", email);
            var appQuery = new GetSMSApplicationUserByUserNameQuery(email);
            var appResult = await Mediator.SendAsync(appQuery, CancellationToken.None);

            if (appResult.IsSuccess && appResult.Value != null && appResult.Value.Authenticate(password))
            {
                appResult.Value.RecordLogin();
                Logger.LogInformation("Application User authenticated successfully: {Email}", email);
                return (appResult.Value, SMSUserType.Application);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug("Application user auth failed: {Error}", ex.Message);
        }

        // Try SMSOrganizationalUser
        try
        {
            Logger.LogDebug("Checking Organizational User: {Email}", email);
            var orgQuery = new GetSMSOrganizationalUserByUserNameQuery(email);
            var orgResult = await Mediator.SendAsync(orgQuery, CancellationToken.None);

            if (orgResult.IsSuccess && orgResult.Value != null && orgResult.Value.Authenticate(password))
            {
                orgResult.Value.RecordLogin();
                Logger.LogInformation("Organizational User authenticated successfully: {Email}", email);
                return (orgResult.Value, SMSUserType.Organizational);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug("Organizational user auth failed: {Error}", ex.Message);
        }

        // Try SMSStakeholderUser
        try
        {
            Logger.LogDebug("Checking Stakeholder User: {Email}", email);
            var stakeholderQuery = new GetSMSStakeholderUserByUserNameQuery(email);
            var stakeholderResult = await Mediator.SendAsync(stakeholderQuery, CancellationToken.None);

            if (stakeholderResult.IsSuccess && stakeholderResult.Value != null && stakeholderResult.Value.Authenticate(password))
            {
                stakeholderResult.Value.RecordLogin();
                Logger.LogInformation("Stakeholder User authenticated successfully: {Email}", email);
                return (stakeholderResult.Value, SMSUserType.Stakeholder);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug("Stakeholder user auth failed: {Error}", ex.Message);
        }

        // No user found or authentication failed
        Logger.LogWarning("Smart Login failed - user not found or invalid credentials: {Email}", email);
        return (null, SMSUserType.Application); // Default fallback
    }

    public class LoginFormModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}