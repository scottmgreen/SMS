using Radzen;

using SMS.Presentation.Configuration;

using SMS_Application.Configuration;

using SMS_Blazor.Components;

using SMS_Infrastructure.Configuration;

using SMS_Shared.Configuration;

namespace SMS_Blazor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Add Blazor services
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add MVC controllers for login handling
        builder.Services.AddControllers();

        // **CRITICAL**: Use SMS Backend Configuration (not duplicated)
        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.ConfigureSMSSession(); // This handles session + HttpContextAccessor

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Use session (from SMS configuration)
        app.UseSession();

        app.UseRouting();
        app.UseAntiforgery();

        // Map controllers (for login POST handling)
        app.MapControllers();

        // Map Blazor components
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add health check endpoint
        app.MapGet("/health", () => Results.Ok(new { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Environment = app.Environment.EnvironmentName 
        }));

        app.Run();
    }
}
