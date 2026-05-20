using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using Infrastructure.Configuration.Extensions;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;

using SMS_Application.Configuration;
using SMS_Application.Interfaces;
using SMS_Application.Services;

using SMS_Infrastructure.Configuration;

using SMS_Shared.Configuration;

using SMS3.Api.Endpoints;
using SMS3.Api.Extensions;
using SMS3.Components;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration;
using SMS3.Security;

namespace SMS3;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 🔧 ENSURE LOGS DIRECTORY EXISTS - Simple directory creation
        try
        {
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            Directory.CreateDirectory(logsDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create logs directory: {ex.Message}");
        }

        // Add services to the container.
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        // Register mock email sender for development/testing
        builder.Services.AddScoped<SMS3.Components.Pages.SMSSystem.Models.IEmailSender, SMS3.Components.Pages.SMSSystem.Services.MockEmailSender>();
        
        // 🚀 FEATURE MANAGEMENT - Must be registered early**
        builder.Services.AddSharedServices(builder.Configuration);

        // 🚀 REGISTER APPLICATION SERVICES EARLY - Need SecurityFeatureService**
        builder.Services.AddApplicationServices(builder.Configuration);

        // 🔐 PRESENTATION AUTHENTICATION SERVICES - Centralized authentication registration**
        builder.Services.AddPresentationAuthenticationServices(builder.Configuration);

        // 🔧 INFRASTRUCTURE SERVICES also includes AddHttpContextAccessor() registration
        builder.Services.AddInfrastructureServices(builder.Configuration);

        if (builder.Configuration.GetValue<bool>("FeatureManagement:ExternalApiEnabled", true))
        {
            // ===========================================================================
            // API SERVICES - External API versioning 
            // ===========================================================================
            builder.Services.AddSMSApiVersioning();
            // ===========================================================================
            // API SERVICES - External API services
            // ===========================================================================
            builder.Services.AddSMSApiServices();
            // ===========================================================================
            // SWAGGER/OpenAPI DOCUMENTATION - Centralized API documentation
            // ===========================================================================
            if(builder.Configuration.GetValue<bool>("FeatureManagement:SwaggerEnabled", true))
            {
                builder.Services.AddSMSSwaggerServices(builder.Configuration);
            }
            
        }
        // PRESENTATION LAYER SERVICES - Centralized registration**
        // This must be after AddApiVersioning
        builder.Services.AddPresentationServices(builder.Configuration);
        
        // Configure IIS options
        builder.Services.Configure<IISServerOptions>(options =>
        {
            options.AutomaticAuthentication = false;
            options.AllowSynchronousIO = true;
        });
        var app = builder.Build();
        
        // 🔐 SET UP SERVICE LOCATOR FOR SECURE NAVIGATION - Using existing ServiceLocator**
        SMS3.Components.Shared.UIHelpers.ServiceLocator.Current = app.Services;

        // 🎯 EXPLICIT PROTOCOL CONFIGURATION - For middleware decisions**
        var masterProtocolConfig = app.Configuration.GetSection("MasterProtocol");
        var explicitProtocol = masterProtocolConfig.GetValue<string>("Protocol", "HTTP");
        var forceEverywhere = masterProtocolConfig.GetValue<bool>("ForceProtocolEverywhere", true);
        var isHttps = !string.IsNullOrEmpty(explicitProtocol) && explicitProtocol.Equals("HTTPS", StringComparison.OrdinalIgnoreCase);

        // ⚡ SECURITY MIDDLEWARE - Must be first to add headers to all responses
        // 🎯 CONDITIONAL: Only apply security headers if enabled in configuration
        if (isHttps || app.Configuration.GetValue<bool>("FeatureManagement:EnableSecurityHeaders", false))
        {
            app.UseSecurityHeaders();
        }

        // 🔐 REQUEST VALIDATION MIDDLEWARE - Validate and sanitize all requests**
        app.UseMiddleware<SMS3.Middleware.RequestValidationMiddleware>();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            if (app.Configuration.GetValue<bool>("FeatureManagement:ExternalApiEnabled", true) && ApiServicesExtensions.IsSwaggerEnabled(app))
            {
                app.UseSMSSwagger();
            }
            
            app.UseDeveloperExceptionPage();
        }

        // 🎯 EXPLICIT HTTPS redirection based on protocol configuration
        if (isHttps && forceEverywhere)
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        
        // 🔐 SESSION MUST BE BEFORE ROUTING AND AUTHENTICATION**
        app.UseSession();
        
        app.UseRouting();

        // 🔐 AUTHENTICATION & AUTHORIZATION MIDDLEWARE**
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        // BLAZOR UI
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        // 🚀 EVENTBUS SUBSCRIPTIONS 
        app.InitializeEventBus(); // Application layer handlers (Domain + Integration events)
        //app.InitializeUIEventHandlers(); // Presentation layer handlers (UI events)

        app.Run();
    }
}