# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: .NET 10.0 (LTS)

## Source Control
- **Source Branch**: main
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task

## User Preferences
### Technical Preferences
- **Authentication Strategy (Blazor sign-in)**: Prefer `SessionBased` with `CircuitBased` fallback; Session reads should remain available after response start, with Circuit as fallback.
- **Protocol Validation Workflow**: Temporarily switch config to HTTPS for validation, then revert to strict HTTP mode until server certificates are deployed.
