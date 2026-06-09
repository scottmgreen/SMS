# Copilot Instructions

## Project Guidelines
- The app must run strictly on HTTPS only; HTTP is not allowed in any environment.
- The app must support strict protocol switching between HTTP and HTTPS via configuration, with HTTP used until server certificates are deployed and HTTPS validated beforehand.
- The solution must remain on .NET 10; do not propose or apply any downgrade to .NET 8 while fixing Swagger or related issues. The .NET 10 migration is complete and should be treated as the baseline target framework.
- Swagger configuration must present both API endpoint definitions (v1 and v2) in .NET 10.
- Do not recommend running `dotnet run`; validate manually from the IDE.
- Implement configuration tasks as a single page based on appsettings; do not change any other pages.

## Service Organization
- All EventBus services must stay together in `Application/Services/EventBusServices`; files have been moved accordingly.

## Database Naming Conventions
- Field names should use typed prefixes like `fldi_`, `fldv_`, `fldd_` (and similar) consistently in SQL/stored procedures.

## Logging and Error Handling
- Use `Application/Common/ApplicationLogMessages.cs` consistently for logging.
- For all new or modified Application-layer code, use `LogApplication*` methods with `ApplicationEventIds`.
- Avoid direct `ILogger.LogInformation/LogWarning/LogError/LogDebug/LogTrace/LogCritical` calls in Application code.
- When migrating or touching existing files, convert any nearby direct `ILogger` calls to the `ApplicationLogMessages` pattern.
- Use `Infrastructure/Common/InfrastructureLogMessages.cs` consistently for Infrastructure logging.
- For all new or modified Infrastructure-layer code, use `LogInfrastructure*` methods with `InfrastructureEventIds`.
- Avoid direct `ILogger.LogInformation/LogWarning/LogError/LogDebug/LogTrace/LogCritical` calls in Infrastructure code; when migrating or touching Infrastructure files, convert nearby direct `ILogger` calls to the `InfrastructureLogMessages` pattern.
- Use `DomainErrors` definitions consistently across Application and Domain projects, adding new `DomainErrors` as needed.

## Risk Assessment
- In this codebase, RiskAssessment should be fetched by hazard code (GetRiskAssessmentByHazardCode query), not via a Hazard.RiskAssessmentCode property.
- Resetting/revalidating reports must preserve existing investigations, interviews, assessments, mitigations, and hazard scoring sessions across validation decisions (including NOT_SMS_RISK and NEEDS_INVESTIGATION).

## UI Rendering
- When rendering HazardDescription in modals or static display areas, treat it as HTML markup (e.g., via MarkupString) so RadzenHtmlEditor formatting is preserved.
- Use Radzen DialogService for confirmation prompts; do not use IJSRuntime/JavaScript confirm dialogs in this codebase.