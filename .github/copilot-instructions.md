# Copilot Instructions

## Project Guidelines
- The app must run strictly on HTTPS only; HTTP is not allowed in any environment.
- The app must support strict protocol switching between HTTP and HTTPS via configuration, with HTTP used until server certificates are deployed and HTTPS validated beforehand.
- The solution must remain on .NET 10; do not propose or apply any downgrade to .NET 8 while fixing Swagger or related issues. The .NET 10 migration is complete and should be treated as the baseline target framework.
- Swagger configuration must present both API endpoint definitions (v1 and v2) in .NET 10.
- Do not recommend running `dotnet run`; validate manually from the IDE.
- Implement configuration tasks as a single page based on appsettings; do not change any other pages.
- Use a single shared appsettings approach across dev, test, and production instead of environment-specific appsettings overrides.
- Large refactors must be done in batches with a QA build run between batches.
- Batch cleanup must proceed directly without asking for confirmation before running scripts/commands.

## Service Organization
- All EventBus services must stay together in `Application/Services/EventBusServices`; files have been moved accordingly.
- IMediator is a custom interface in the Application Services layer, not a NuGet/MediatR dependency.
- Actively work through a concrete hardening plan for EventBus/SPI/SMS Assurance: complete SPIEventCoordinator orchestration, wire SPIDashboardRefreshEvent consumption, reduce replay-map maintenance, standardize event contracts, and add architecture tests.

## Database Naming Conventions
- Field names should use typed prefixes like `fldi_`, `fldv_`, `fldd_` (and similar) consistently in SQL/stored procedures.
- Truncate hazard file DB data; no need to retain or report DB-stored hazard files during cloud-only storage cutover.

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
- Use `VALIDATION_DECISION_MADE` as a datasource and trigger metric updates when validation decision is `SMS_RISK` (hazard reported, evaluated, processing decision made).

## UI Rendering
- When rendering HazardDescription in modals or static display areas, treat it as HTML markup (e.g., via MarkupString) so RadzenHtmlEditor formatting is preserved.
- Use Radzen DialogService for confirmation prompts; do not use IJSRuntime/JavaScript confirm dialogs in this codebase.

## Code Style
- Private variables in SMS3 code-behind files must consistently follow the _variableName naming convention (e.g., _memberName). 
- Cleanup should continue in targeted batches with build validation.
- Use enums only for display styles; do not use hard-coded strings for event source display naming.
- Standardize data-reader string mapping to `GetValue<string>` with trimming handled in the extension method rather than direct `GetString` calls.
- Use Domain Entities rather than introducing DTOs when realigning code to clean architecture patterns in this codebase.

## Paging
- When applying paging changes, update all SMSListings pages in SMS3 so every listing uses 15 rows per page and do not skip any listing page.

## Cloud Uploads
- For FlyPDX hazard file cloud uploads, the request must include User-Agent 'curl/8.19.0'; removing it causes upload failure.
- Hazard file storage policy is cloud-only; remove database failover. On cloud upload failure, publish an error instead of storing in the database.
- Hazard file cloud storage must only use appsettings values; no hardcoded fallback URIs like contoso-sms.blob.core.windows.net are allowed. Hazard file paths must use configured Port of Portland endpoints only; never use contoso-sms.blob.core.windows.net for hazard file paths under any circumstances.

## Investigation Evidence File Viewing
- For investigation evidence file viewing, use cloud returned FilePath when storage is cloud; viewer behavior must automatically handle cloud storage.