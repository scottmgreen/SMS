# Copilot Instructions

## Project Guidelines
- The app must support strict protocol switching between HTTP and HTTPS via configuration, with HTTP used until server certificates are deployed and HTTPS validated beforehand.
- The solution must remain on .NET 10; do not propose or apply any downgrade to .NET 8 while fixing Swagger or related issues.
- Swagger configuration must present both API endpoint definitions (v1 and v2) in .NET 10.
- Do not recommend running `dotnet run`; validate manually from the IDE.
- Implement configuration tasks as a single page based on appsettings; do not change any other pages.

## Service Organization
- All EventBus services must stay together in `Application/Services/EventBusServices`; files have been moved accordingly.

## Database Naming Conventions
- Field names should use typed prefixes like `fldi_`, `fldv_`, `fldd_` (and similar) consistently in SQL/stored procedures.

## Risk Assessment
- In this codebase, RiskAssessment should be fetched by hazard code (GetRiskAssessmentByHazardCode query), not via a Hazard.RiskAssessmentCode property.