using Radzen;

using SMS_Application.Configuration;
using SMS_Blazor.Components;
using SMS_Blazor.Configuration;

using SMS_Infrastructure.Configuration;
using SMS_Shared.Configuration;

namespace SMS_Blazor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        try
        {
            // Configure logging first
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Add core services
            builder.Services.AddHttpContextAccessor(); // Critical for session access

            // Add Blazor services
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add MVC controllers for login handling
            builder.Services.AddControllers();

            // Add Radzen services
            builder.Services.AddRadzenComponents();

            // **FIX**: Add distributed cache BEFORE session
            builder.Services.AddDistributedMemoryCache();
            
            // Configure session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Add SMS Backend Configuration
            builder.Services.AddSharedServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.ConfigureSMSSession();

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
            app.UseRouting();
            app.UseSession();
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
        catch (Exception ex)
        {
            // Log startup errors
            var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();
            logger?.LogCritical(ex, "Application failed to start");
            throw;
        }
    }
}
