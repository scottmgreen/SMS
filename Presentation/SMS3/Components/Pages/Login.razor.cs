namespace SMS3.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string ErrorMessage { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("error", out var errorValue))
        {
            ErrorMessage = errorValue.ToString();
        }
    }
}


