using Microsoft.AspNetCore.Mvc;

using SMS_Application.Configuration;
using SMS_Infrastructure.Configuration;
using SMS_Shared.Configuration;

using SMS3.Components;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration;
using SMS3.Security;
using SMS3.Api.Endpoints;
using Infrastructure.Configuration.Extensions;

namespace SMS3;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        
        // **PRESENTATION LAYER SERVICES - Centralized registration**
        builder.Services.AddPresentationServices(builder.Configuration);
        builder.Services.AddPresentationMiddleware();
        builder.Services.ConfigurePresentationOptions(builder.Configuration);

        // **🔐 SYSTEM-WIDE SECURE ROUTING SERVICES**
        builder.Services.AddSingleton<ISecureRoutingService, SecureRoutingService>();
                
        // Register SMS Services
        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

        var app = builder.Build();
        
        // **🔐 SET UP SERVICE LOCATOR FOR SECURE NAVIGATION - Using existing ServiceLocator**
        SMS3.Components.Shared.UIHelpers.ServiceLocator.Current = app.Services;

        // ⚡ SECURITY MIDDLEWARE - Must be first to add headers to all responses
        app.UseSecurityHeaders();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
            
            // **SWAGGER/OpenAPI UI - Feature flag controlled**
            app.UseSMSSwagger();
        }

        // **🔐 SECURE ROUTING SERVICE - No middleware needed for Blazor Server**
       

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAntiforgery();

        // ============================================================================
        // EXTERNAL API ENDPOINTS - Feature flag controlled
        // ============================================================================
        app.MapPDXSMSApiEndpoints();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}