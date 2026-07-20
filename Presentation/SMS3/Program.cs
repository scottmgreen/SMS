using SMS_Application.Configuration;
using System.Net;

using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Configuration.Extensions;

using SMS_Shared.Configuration;

using SMS3.Api.Extensions;
using SMS3.Components;
using SMS3.Configuration;
using SMS3.EventHandlers;
using SMS_Domain.Events;
using SMS_Application.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;

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
        builder.Services.AddHttpClient();
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
        // PRESENTATION LAYER SERVICES 
        // This must be after AddApiVersioning
        builder.Services.AddPresentationServices(builder.Configuration);
        
        // Configure IIS options
        builder.Services.Configure<IISServerOptions>(options =>
        {
            options.AutomaticAuthentication = false;
            options.AllowSynchronousIO = true;
        });
        var app = builder.Build();

        // Ensure original scheme/protocol is honored when running behind IIS/reverse proxies
        // to avoid HTTPS redirection loops (ERR_TOO_MANY_REDIRECTS).
        var forwardedHeadersConfig = app.Configuration.GetSection("ForwardedHeaders");
        var trustAllForwarders = forwardedHeadersConfig.GetValue<bool>("TrustAllProxies", false);
        var configuredForwardLimit = forwardedHeadersConfig.GetValue<int?>("ForwardLimit");

        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = configuredForwardLimit.HasValue && configuredForwardLimit.Value > 0
                ? configuredForwardLimit.Value
                : 1,
            RequireHeaderSymmetry = forwardedHeadersConfig.GetValue<bool>("RequireHeaderSymmetry", true)
        };

        forwardedHeadersOptions.KnownNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();

        if (trustAllForwarders)
        {
            forwardedHeadersOptions.ForwardLimit = null;
            forwardedHeadersOptions.RequireHeaderSymmetry = false;
        }
        else
        {
            var knownProxies = forwardedHeadersConfig.GetSection("KnownProxies").Get<string[]>() ?? [];
            var knownNetworks = forwardedHeadersConfig.GetSection("KnownNetworks").Get<string[]>() ?? [];

            foreach (var proxy in knownProxies)
            {
                if (IPAddress.TryParse(proxy, out var proxyIp))
                {
                    forwardedHeadersOptions.KnownProxies.Add(proxyIp);
                }
            }

            foreach (var cidr in knownNetworks)
            {
                var parts = cidr.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 2 &&
                    IPAddress.TryParse(parts[0], out var networkIp) &&
                    int.TryParse(parts[1], out var prefixLength))
                {
                    forwardedHeadersOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(networkIp, prefixLength));
                }
            }

            if (forwardedHeadersOptions.KnownProxies.Count == 0 && forwardedHeadersOptions.KnownNetworks.Count == 0)
            {
                forwardedHeadersOptions.KnownProxies.Add(IPAddress.Loopback);
                forwardedHeadersOptions.KnownProxies.Add(IPAddress.IPv6Loopback);
            }
        }

        app.UseForwardedHeaders(forwardedHeadersOptions);

        // Emergency scheme normalization for IIS/ARR reverse-proxy environments.
        // Some deployments send X-ARR-SSL (without X-Forwarded-Proto), which can cause
        // the app to see HTTP internally and trigger redirect/cookie loops.
        app.Use((context, next) =>
        {
            if (!context.Request.IsHttps)
            {
                var forwardedProto = context.Request.Headers["X-Forwarded-Proto"].ToString();
                var arrSsl = context.Request.Headers["X-ARR-SSL"].ToString();

                if (forwardedProto.Equals("https", StringComparison.OrdinalIgnoreCase) ||
                    !string.IsNullOrWhiteSpace(arrSsl))
                {
                    context.Request.Scheme = "https";
                }
            }

            return next();
        });

        // Temporary diagnostics to troubleshoot IIS/proxy HTTPS redirect loops.
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/" || context.Request.Path == string.Empty)
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("SMS3.ProtocolDiagnostics");
                logger.LogInformation(
                    "Protocol diagnostics: Scheme={Scheme}, IsHttps={IsHttps}, Host={Host}, X-Forwarded-Proto={XForwardedProto}, X-Forwarded-For={XForwardedFor}",
                    context.Request.Scheme,
                    context.Request.IsHttps,
                    context.Request.Host.Value,
                    context.Request.Headers["X-Forwarded-Proto"].ToString(),
                    context.Request.Headers["X-Forwarded-For"].ToString());
            }

            await next();
        });
        
        // SET UP SERVICE LOCATOR FOR SECURE NAVIGATION - Using existing ServiceLocator**
        SMS3.Components.Shared.UIHelpers.ServiceLocator.Current = app.Services;

        // EXPLICIT PROTOCOL CONFIGURATION - For middleware decisions**
        var masterProtocolConfig = app.Configuration.GetSection("MasterProtocol");
        var explicitProtocol = masterProtocolConfig.GetValue<string>("Protocol", "HTTP");
        var forceEverywhere = masterProtocolConfig.GetValue<bool>("ForceProtocolEverywhere", true);
        var isHttps = !string.IsNullOrEmpty(explicitProtocol) && explicitProtocol.Equals("HTTPS", StringComparison.OrdinalIgnoreCase);

        // SECURITY MIDDLEWARE - Must be first to add headers to all responses
        // CONDITIONAL: Only apply security headers if enabled in configuration
        if (isHttps || app.Configuration.GetValue<bool>("FeatureManagement:EnableSecurityHeaders", false))
        {
            app.UseSecurityHeaders();
        }

        // REQUEST VALIDATION MIDDLEWARE - Validate and sanitize all requests**
        app.UseMiddleware<SMS3.Middleware.RequestValidationMiddleware>();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        if (app.Configuration.GetValue<bool>("FeatureManagement:ExternalApiEnabled", true) && ApiServicesExtensions.IsSwaggerEnabled(app))
        {
            SMS3.Api.Extensions.ApiServicesExtensions.UseSMSSwagger(app);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Use app-level HTTPS redirection only when explicitly enabled.
        // In IIS/reverse-proxy deployments this is commonly handled upstream,
        // and forcing it here can create redirect loops.
        var enforceHttpsRedirection = app.Configuration.GetValue<bool>("FeatureManagement:EnforceHttpsRedirection", false);
        if (enforceHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        
        // SESSION MUST BE BEFORE ROUTING AND AUTHENTICATION**
        app.UseSession();
        
        app.UseRouting();

        // AUTHENTICATION & AUTHORIZATION MIDDLEWARE**
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        // BLAZOR UI
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        // EVENTBUS SUBSCRIPTIONS 
        app.InitializeEventBus(); // Application layer handlers (Domain + Integration events)

        // Ensure Blazor Presentation UI notification handler is explicitly wired
        // so UINotificationEvent events can dispatch to Radzen popup notifications.
        using (var scope = app.Services.CreateScope())
        {
            var eventBus = scope.ServiceProvider.GetRequiredService<IBaseEventBus>();
            eventBus.SubscribeUI<UINotificationEvent, UIEventHandler>();
            eventBus.SubscribeUI<SPIDashboardRefreshEvent, SPIDashboardRefreshEventHandler>();
        }

        app.Run();
    }
}