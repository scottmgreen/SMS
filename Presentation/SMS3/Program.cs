using SMS_Application.Configuration;
using Microsoft.AspNetCore.Http.Features;
using System.Net;

using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Configuration.Extensions;
using SMS_Infrastructure.Interfaces;

using SMS_Shared.Configuration;

using SMS3.Api.Extensions;
using SMS3.Components;
using SMS3.Configuration.Extensions;
using SMS3.Configuration;
using SMS3.EventHandlers;
using SMS3.Hubs;
using SMS3.Security;
using SMS_Domain.Events;
using SMS_Application.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SMS3;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ?? ENSURE LOGS DIRECTORY EXISTS - Simple directory creation
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
        builder.Services.AddSignalR();
        builder.Services.AddRadzenComponents();
        builder.Services.AddHttpClient();

        var dataProtectionSection = builder.Configuration.GetSection("DataProtection");
        var keyRingPath = dataProtectionSection.GetValue<string>("KeyRingPath");
        var dataProtectionApplicationName = dataProtectionSection.GetValue<string>("ApplicationName", "PDXSMS");

        if (!string.IsNullOrWhiteSpace(keyRingPath))
        {
            Directory.CreateDirectory(keyRingPath);
            builder.Services
                .AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
                .SetApplicationName(dataProtectionApplicationName ?? "PDXSMS");
        }
        else
        {
            builder.Services
                .AddDataProtection()
                .SetApplicationName(dataProtectionApplicationName ?? "PDXSMS");
        }

        // Register mock email sender for development/testing
        builder.Services.AddScoped<IEmailSender, SMS3.Components.Pages.SMSSystem.Services.MockEmailSender>();
        
        // ?? FEATURE MANAGEMENT - Must be registered early**
        builder.Services.AddSharedServices(builder.Configuration);

        // ?? REGISTER APPLICATION SERVICES EARLY - Need SecurityFeatureService**
        builder.Services.AddApplicationServices(builder.Configuration);

        // ?? PRESENTATION AUTHENTICATION SERVICES - Centralized authentication registration**
        builder.Services.AddPresentationAuthenticationServices(builder.Configuration);

        // ?? INFRASTRUCTURE SERVICES also includes AddHttpContextAccessor() registration
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // Initialize SQL-driven organization smart-enum values at startup
        using (var preloadScope = builder.Services.BuildServiceProvider().CreateScope())
        {
            var organizationRepository = preloadScope.ServiceProvider.GetService<ISMSOrganizationRepository>();
            if (organizationRepository is not null)
            {
                await organizationRepository.GetAllAsync();
            }

            var companyRepository = preloadScope.ServiceProvider.GetService<ISMSCompanyRepository>();
            if (companyRepository is not null)
            {
                await companyRepository.GetAllAsync();
            }

            var jobTitleRepository = preloadScope.ServiceProvider.GetService<ISMSJobTitleRepository>();
            if (jobTitleRepository is not null)
            {
                await jobTitleRepository.GetAllAsync();
            }
        }

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

        // Ensure the session cookie is established on the initial HTTP pipeline before
        // interactive Blazor events attempt to write session data.
        app.Use(async (context, next) =>
        {
            if (!context.Request.Cookies.ContainsKey("SMS_MachineId") && !context.Response.HasStarted)
            {
                context.Response.Cookies.Append("SMS_MachineId", Guid.NewGuid().ToString("N"), new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    MaxAge = TimeSpan.FromDays(365)
                });
            }

            await next();
        });
        
        app.UseRouting();

        // AUTHENTICATION & AUTHORIZATION MIDDLEWARE**
        app.UseAuthentication();

        // Cleanup stale cookie-auth tickets (e.g., app recycle with in-memory ticket store)
        // so clients are returned to a clean anonymous state instead of repeated auth-failure churn.
        app.Use(async (context, next) =>
        {
            if (context.Request.Cookies.ContainsKey("SMS_Auth")
                && context.User?.Identity?.IsAuthenticated != true
                && !context.Response.HasStarted)
            {
                context.Response.Cookies.Delete("SMS_Auth", new CookieOptions
                {
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    HttpOnly = true,
                    IsEssential = true
                });
            }

            await next();
        });

        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapPost("/auth/login", async (
            HttpContext context,
            SMS_Application.Interfaces.IAuthenticationService authenticationService,
            ISMSSessionService sessionService,
            CancellationToken cancellationToken) =>
        {
            var form = await context.Request.ReadFormAsync(cancellationToken);
            var username = form["username"].ToString();
            var password = form["password"].ToString();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Results.Redirect("/login?error=Username%20and%20password%20are%20required.");
            }

            var authResult = await authenticationService.AuthenticateAsync(username, password, cancellationToken);
            if (!authResult.IsSuccess || authResult.User is null)
            {
                var encoded = Uri.EscapeDataString(authResult.ErrorMessage ?? "Invalid username or password.");
                return Results.Redirect($"/login?error={encoded}");
            }

            if (authResult.User.TwoFactorEnabled)
            {
                await sessionService.ClearPending2FAUserAsync();
                await sessionService.StorePending2FAUserAsync(authResult.User, authResult.UserType);
                return Results.Redirect("/verify-2fa");
            }

            await sessionService.CreateSMSSessionAsync(authResult.User, authResult.UserType);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, authResult.User.Code),
                new(ClaimTypes.Name, authResult.User.DisplayName ?? authResult.User.Code),
                new("SMS_UserCode", authResult.User.Code),
                new("IsAuthenticated", "true")
            };

            var session = context.Features.Get<ISessionFeature>()?.Session;
            var sessionRoleCode = session?.GetString("SMS_UserRoleCode");
            var sessionRoleName = session?.GetString("SMS_UserRoleName");
            var sessionPermissions = session?.GetString("SMS_UserPermissions");

            if (!string.IsNullOrWhiteSpace(sessionRoleCode))
            {
                claims.Add(new Claim("SMS_UserRoleCode", sessionRoleCode));
            }

            if (!string.IsNullOrWhiteSpace(sessionRoleName))
            {
                claims.Add(new Claim("SMS_UserRoleName", sessionRoleName));
            }

            if (!string.IsNullOrWhiteSpace(sessionPermissions))
            {
                claims.Add(new Claim("SMS_UserPermissions", sessionPermissions));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true
                });

            return Results.Redirect("/");
        }).AllowAnonymous();

        app.MapGet("/auth/logout", async (
            HttpContext context,
            ISMSSessionService sessionService) =>
        {
            try
            {
                await sessionService.ClearSMSSessionAsync();
            }
            catch
            {
                // Continue with cookie sign-out even when session clear has transient failures.
            }

            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/pdx");
        }).AllowAnonymous();

        app.MapPost("/auth/establish-session", async (
            AuthSessionRequest request,
            SMS_Application.Interfaces.IAuthenticationService authenticationService,
            ISMSSessionService sessionService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new AuthSessionResponse(false, false, "Username and password are required."));
            }

            var authResult = await authenticationService.AuthenticateAsync(request.Username, request.Password, cancellationToken);
            if (!authResult.IsSuccess || authResult.User is null)
            {
                return Results.Ok(new AuthSessionResponse(false, false, authResult.ErrorMessage ?? "Invalid username or password."));
            }

            if (authResult.User.TwoFactorEnabled)
            {
                await sessionService.ClearPending2FAUserAsync();
                await sessionService.StorePending2FAUserAsync(authResult.User, authResult.UserType);
                return Results.Ok(new AuthSessionResponse(true, true, string.Empty));
            }

            await sessionService.CreateSMSSessionAsync(authResult.User, authResult.UserType);
            return Results.Ok(new AuthSessionResponse(true, false, string.Empty));
        }).AllowAnonymous();

        app.MapGet("/auth/current-user", (HttpContext context) =>
        {
            var session = context.Features.Get<ISessionFeature>()?.Session;
            if (session is null)
            {
                return Results.Ok(new CurrentUserSessionResponse(false, null));
            }

            var isAuthenticated = session.GetString("IsAuthenticated") == "true";
            if (!isAuthenticated)
            {
                return Results.Ok(new CurrentUserSessionResponse(false, null));
            }

            var keys = new[]
            {
                "IsAuthenticated",
                "SMS_UserCode",
                "SMS_UserType",
                "SMS_DisplayName",
                "SMS_Email",
                "SMS_FirstName",
                "SMS_LastName",
                "SMS_LoginTime",
                "SMS_TwoFactorEnabled",
                "SMS_UserRoleCode",
                "SMS_UserRoleName",
                "SMS_UserPermissions"
            };

            var payload = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var key in keys)
            {
                var value = session.GetString(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    payload[key] = value;
                }
            }

            return Results.Ok(new CurrentUserSessionResponse(true, payload));
        }).AllowAnonymous();

        // BLAZOR UI
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        app.MapHub<LiveUpdatesHub>("/hubs/live-updates");

        // EVENTBUS SUBSCRIPTIONS 
        app.InitializeEventBus(); // Application layer handlers (Domain + Integration events)

        app.Run();
    }

    private sealed record AuthSessionRequest(string Username, string Password);
    private sealed record AuthSessionResponse(bool IsSuccess, bool RequiresTwoFactor, string ErrorMessage);
    private sealed record CurrentUserSessionResponse(bool IsAuthenticated, Dictionary<string, string>? AuthValues);
}
