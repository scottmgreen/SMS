using System.Reflection;
using CBT_UI_Dashboard.Components;

using CBT3_Application.Common;
using CBT3_Application.Messaging.CircuitHandlers;

using CBT3_Infrastructure.Configuration;


using CBT3_Shared;

using CBT3_UI_Dashboard;

using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Configuration;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var configuration = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
              .Build();

builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.AddRadzenComponents();

builder.Services.AddScoped<DialogService>();

CBT3_UI_Dashboard.DependencyInjection.Initialize(builder.Services, configuration);
builder.Services.BuildServiceProvider();
builder.Services.AddScoped<CircuitHandler, CBT_UI_DashboardCircuitHandler>();
var app = builder.Build();

app.UseMiddleware<CircuitUserTrackingMiddleware>();  // Register custom middleware
app.UseMiddleware<LoggerMiddleware>();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<CBT_UI_Dashboard.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
