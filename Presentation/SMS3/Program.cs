using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using SMS_Application.Configuration;
using SMS_Infrastructure.Configuration;
using SMS_Shared.Configuration;
using SMS3.Components;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration;
using SMS3.Security;
using SMS3.Api.Endpoints;
using Infrastructure.Configuration.Extensions;
using SMS_Application.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using SMS_Application.Interfaces;

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
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        
        // **🚀 FEATURE MANAGEMENT - Must be registered early**
        builder.Services.AddSharedServices(builder.Configuration);

        // **🚀 REGISTER APPLICATION SERVICES EARLY - Need SecurityFeatureService**
        builder.Services.AddApplicationServices();

        // **🔐 PRESENTATION AUTHENTICATION SERVICES - Centralized authentication registration**
        // Replaces lines 45-194 with organized extension method maintaining exact same loading sequence
        builder.Services.AddPresentationAuthenticationServices(builder.Configuration);

        // **PRESENTATION LAYER SERVICES - Centralized registration**
        // 🔧 NOTE: AddPresentationServices includes AddHttpContextAccessor() registration
        builder.Services.AddPresentationServices(builder.Configuration);
        builder.Services.AddPresentationMiddleware();
        builder.Services.ConfigurePresentationOptions(builder.Configuration);
                
        // Register SMS Services
        // 🔧 NOTE: AddInfrastructureServices also includes AddHttpContextAccessor() registration
        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);

        var app = builder.Build();
        
        // **🔐 SET UP SERVICE LOCATOR FOR SECURE NAVIGATION - Using existing ServiceLocator**
        SMS3.Components.Shared.UIHelpers.ServiceLocator.Current = app.Services;

        // **🎯 EXPLICIT PROTOCOL CONFIGURATION - For middleware decisions**
        var masterProtocolConfig = app.Configuration.GetSection("MasterProtocol");
        var explicitProtocol = masterProtocolConfig.GetValue<string>("Protocol", "HTTP");
        var forceEverywhere = masterProtocolConfig.GetValue<bool>("ForceProtocolEverywhere", true);
        var isHttps = explicitProtocol.Equals("HTTPS", StringComparison.OrdinalIgnoreCase);

        // ⚡ SECURITY MIDDLEWARE - Must be first to add headers to all responses
        // 🎯 CONDITIONAL: Only apply security headers if enabled in configuration
        if (isHttps || app.Configuration.GetValue<bool>("FeatureManagement:EnableSecurityHeaders", false))
        {
            app.UseSecurityHeaders();
        }

        // **🔐 REQUEST VALIDATION MIDDLEWARE - Validate and sanitize all requests**
        app.UseMiddleware<SMS3.Middleware.RequestValidationMiddleware>();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
            app.UseSMSSwagger();
        }

        // 🎯 EXPLICIT HTTPS redirection based on protocol configuration
        if (isHttps && forceEverywhere)
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        
        // **🔐 CRITICAL FIX: SESSION MUST BE BEFORE ROUTING AND AUTHENTICATION**
        app.UseSession();
        
        app.UseRouting();

        // **🔐 AUTHENTICATION & AUTHORIZATION MIDDLEWARE**
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapPDXSMSApiEndpoints();
        app.MapAuthenticationStatusEndpoints(); // 🎯 NEW: Development diagnostics endpoints
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}