# SMS Assurance SPIConfiguration and SPI Internals Design Document

## Document Metadata
- **Workspace**: `C:\Projects\PDXSMS_V2`
- **Target Framework Baseline**: `.NET 10`
- **UI Stack**: `Blazor Server` (Radzen components)
- **Prepared For**: SPI architecture review and implementation hardening

---

## Purpose and Scope
This document provides a comprehensive technical analysis of:
1. `SMSAssurance/SPIConfiguration` behavior and architecture.
2. SPI internals across Presentation, Application, Domain, and Infrastructure layers.
3. All identified command creation points relevant to SPI and SMSAssurance flows.
4. All identified SPI-related event publication and handler trigger paths.
5. EventBus and UI refresh mechanics for SPI dashboard updates.

This includes all details previously summarized as sections **1–7**, plus consolidated command/event inventories.

---

## 1) `SMSAssurance/SPIConfiguration` Detailed Analysis

### 1.1 UI Entry Points
- `Presentation/SMS3/Components/Pages/SMSAssurance/SPIConfiguration.razor`
- `Presentation/SMS3/Components/Pages/SMSAssurance/SPIConfiguration.razor.cs`

### 1.2 Responsibilities
`SPIConfiguration` is the primary configuration screen for Safety Performance Indicators (SPIs):
- Loads SPI metadata (types/statuses/frequencies/departments).
- Loads full SPI set via mediator query.
- Applies client-side filtering/search over loaded dataset.
- Supports Create / Edit / Duplicate / Delete.
- Opens edit dialogs and persists changes via CQRS commands.
- Navigates users into dashboard context for datapoint visibility.

### 1.3 Read path (query)
- `GetAllSafetyPerformanceIndicatorsQuery` is created and sent in `LoadSPIs()`.
- This feeds `_allSpis`, then local filtering builds `_filteredSpis`.

### 1.4 Write path (commands created here)
Command objects are created in this component and sent through mediator:
- `DeleteSafetyPerformanceIndicatorCommand`
  - `SPIConfiguration.razor.cs:253`
- `CreateSafetyPerformanceIndicatorCommand`
  - `SPIConfiguration.razor.cs:318`
- `UpdateSafetyPerformanceIndicatorCommand`
  - `SPIConfiguration.razor.cs:350`

### 1.5 Notes
- Uses Radzen `DialogService.Confirm` (aligned with codebase UI prompt pattern).
- Edit mode clones entity fields before mutation.
- Duplicate flow copies configuration but resets identity/code semantics for creation.

---

## 2) Command Creation Inventory (SPI and SMSAssurance)

## 2.1 SPI-specific command creation points
### `SPIConfiguration`
- `DeleteSafetyPerformanceIndicatorCommand` (`line 253`)
- `CreateSafetyPerformanceIndicatorCommand` (`line 318`)
- `UpdateSafetyPerformanceIndicatorCommand` (`line 350`)

### `SPIDetail`
- `DeleteSPIDataPointCommand` (`line 225`)
- `UpdateSPIDataPointCommand` (`line 263`)
- `AddSPIDataPointCommand` (`line 280`)

## 2.2 Additional command creation under SMSAssurance area
(Found by scan of `Presentation/SMS3/Components/Pages/SMSAssurance/**/*.cs`)
- `CreateSMSAuditCommand` (`Components/AuditDialog.razor.cs:183`)
- `UpdateSMSAuditCommand` (`Components/AuditDialog.razor.cs:247`)
- `CreateSMSAuditEvidenceCommand` (`Components/AuditEvidenceDialog.razor.cs:177`)
- `UpdateSMSAuditEvidenceCommand` (`Components/AuditEvidenceDialog.razor.cs:219`)
- `CreateSMSAuditFindingCommand` (`Components/AuditFindingDialog.razor.cs:110`)
- `UpdateSMSAuditFindingCommand` (`Components/AuditFindingDialog.razor.cs:147`)
- `CreateSMSAuditPlanCommand` (`Components/AuditPlanDialog.razor.cs:282`)
- `UpdateSMSAuditPlanCommand` (`Components/AuditPlanDialog.razor.cs:299`)
- `UpdateSMSAuditPlanCommand` (`AuditCalendar.razor.cs:366`)
- `StartSMSAuditCommand` (`AuditDetail.razor.cs:262`)
- `CompleteSMSAuditCommand` (`AuditDetail.razor.cs:315`)
- `DeleteSMSAuditFindingCommand` (`AuditDetail.razor.cs:418`)
- `AssignSMSAuditCorrectiveActionCommand` (`AuditDetail.razor.cs:457`)
- `CompleteSMSAuditCorrectiveActionCommand` (`AuditDetail.razor.cs:500`)
- `VerifySMSAuditFindingCommand` (`AuditDetail.razor.cs:541`)
- `UpdateSMSAuditPlanCommand` (`AuditManagement.razor.cs:381`)
- `CreateSMSAuditCommand` (`AuditManagement.razor.cs:391`)
- `DeleteSMSAuditPlanCommand` (`AuditManagement.razor.cs:435`)
- `StartSMSAuditCommand` (`AuditManagement.razor.cs:466`)

---

## 3) SPI Command Definitions vs Runtime Handler Coverage

## 3.1 SPI commands defined
File: `Application/CQRS/Commands/SafetyPerformanceIndicatorCommands.cs`

Defined command families include:
- SPI CRUD commands
- SPI configuration/status/target commands
- SPI datapoint commands
- SPI review commands
- SPI dashboard/alert/archive commands

## 3.2 Implemented handler classes found
File: `Application/CQRS/CommandHandlers/SafetyPerformanceIndicatorCommandHandlers.cs`
- `CreateSafetyPerformanceIndicatorCommandHandler`
- `UpdateSafetyPerformanceIndicatorCommandHandler`
- `DeleteSafetyPerformanceIndicatorCommandHandler`
- `UpdateSPIDataPointCommandHandler`
- `AddSPIDataPointCommandHandler`
- `DeleteSPIDataPointCommandHandler`
- `UpdateSPIConfigurationCommandHandler`
- `SetSPITargetsCommandHandler`
- `UpdateSPIStatusCommandHandler`
- `ScheduleSPIReviewCommandHandler`
- `CompleteSPIReviewCommandHandler`
- `RecalculateSPIDashboardCommandHandler`
- `GenerateSPIAlertsCommandHandler`

## 3.3 Coverage gaps (defined command type, no matching handler class found in workspace scan)
- `ArchiveOldSPIDataCommand`

---

## 4) EventBus Internals and How Event Handlers Are Triggered

## 4.1 Core EventBus contracts and implementation
- Contract: `Application/Interfaces/EventBusInterfaces/IBaseEventBus.cs`
- Implementation: `Application/Services/EventBusServices/EventDispatchService.cs`

Supports:
- Domain events (`PublishDomainEventAsync`)
- UI events (`PublishUIEventAsync`)
- Integration events (`PublishIntegrationEventAsync`)
- Execution modes (`Immediate`, `Queued`, `Manual`)

## 4.2 Registration and subscription bootstrap
- DI registration scan:
  - `Application/Configuration/ServiceCollectionExtensions.cs`
  - `AddEventBusHandlers()` uses reflection to register `IBaseEventHandler<T>` implementations.
- Runtime auto-subscription:
  - `Application/Configuration/EventBusExtensions.cs`
  - `InitializeEventBus()` and `AddSubscribeEventHandlers()`
- Startup call:
  - `Presentation/SMS3/Program.cs:240` -> `app.InitializeEventBus();`

## 4.3 Explicit UI subscriptions in startup
- `Presentation/SMS3/Program.cs:247-248`
  - `UINotificationEvent -> UIEventHandler`
  - `SPIDashboardRefreshEvent -> SPIDashboardRefreshEventHandler`

## 4.4 Base domain handler orchestration
- `Application/EventHandlers/DomainEventHandlers/BaseDomainEventHandler.cs`
- Execution order per event:
  1. `HandleDomainEventAsync`
  2. `HandleIntegrationEventAsync`
  3. `HandleUIEventAsync`

---

## 5) SPI-Related Event Trigger Matrix (Publisher -> Handler -> Effect)

| Event Type | Event Creation / Publish Site | Triggered Handler | Primary SPI Effect |
|---|---|---|---|
| `HazardCreatedEvent` | `Application/CQRS/CommandHandlers/HazardCommandHandlers.cs` (`HazardEventPublisher`) | `HazardCreatedEventHandler` | `UpdateHazardReportRateAsync` |
| `HazardStatusChangedEvent` | `HazardCommandHandlers.cs:225` (transition publish) | `HazardStatusChangedEventHandler` | `UpdateHazardClosureTimeAsync` |
| `RiskAssessmentCompletedEvent` | `RiskAssessmentCommandHandlers.cs:186` | `RiskAssessmentCompletedEventHandler` | `UpdateRiskAssessmentCompletionAsync` |
| `HighRiskIdentifiedEvent` | `RiskAssessmentCommandHandlers.cs:215` | `HighRiskIdentifiedEventHandler` | `UpdateHighRiskExposureAsync` |
| `ValidationDecisionMadeEvent` | `ReportValidationCommandHandlers.cs:105,175` | `ValidationDecisionMadeEventHandler` | `UpdateRiskIdentificationEffectivenessAsync` |
| `MitigationCompletedEvent` | `MitigationCommandHandlers.cs:181` | `MitigationCompletedEventHandler` | `UpdateMitigationImplementationRateAsync` |
| `MitigationOverdueEvent` | Publisher not identified in runtime scan | `MitigationOverdueEventHandler` | `UpdateCorrectiveActionClosureAsync` |
| `MitigationStatusChangedEvent` | `MitigationCommandHandlers.cs:156` | `MitigationStatusChangedEventHandler` | Overdue status path updates corrective action closure SPI |
| `SPIComplianceChangedEvent` | `SafetyPerformanceIndicatorCommandHandlers.cs` datapoint add/update flows | `SPIComplianceChangedEventHandler` | Logs compliance transition + publishes UI notification |
| `SPIThresholdExceededEvent` | `SafetyPerformanceIndicatorCommandHandlers.cs` datapoint add/update flows | `SPIThresholdEventHandler` | Logs threshold-exceeded processing |

### 5.1 SPI compliance handler details
- File: `Application/EventHandlers/DomainEventHandlers/DomainEventHandlers.cs` around `line 1302`
- Class: `SPIComplianceChangedEventHandler : BaseDomainEventHandler<SPIComplianceChangedEvent>`
- Performs:
  - Application logging of status transition
  - Elevated warning logging for `NonCompliant` / `AtRisk`
  - Publishes `UINotificationEvent` via EventBus

### 5.2 SPI threshold handler details
- Consolidated class in `DomainEventHandlers.cs` (`SPIThresholdEventHandler`)
- Processes `SPIThresholdExceededEvent`
- Records threshold processing results to logs

---

## 6) SPI Dashboard Refresh Internals

## 6.1 Event definition
- `Domain/Events/UIEvents/SPIDashboardRefreshEvent.cs`
- Encapsulates dashboard refresh context:
  - affected SPI codes
  - reason
  - section
  - metadata
  - target component (`SPIDashboard`)

## 6.2 Event UI handler
- `Presentation/SMS3/EventHandlers/SPIDashboardRefreshEventHandler.cs`
- On event, dispatches to Blazor-side dispatcher:
  - `EventBusDispatcher.DispatchSPIDashboardRefreshAsync(refreshEvent)`

## 6.3 Dispatcher and component receiver path
- Dispatcher: `Presentation/SMS3/Components/Shared/Common/EventBusDispatcher.cs`
- Receiver interface: `ISPIDashboardRefreshReceiver`
- Component implementation:
  - `Presentation/SMS3/Components/Pages/SMSAssurance/SPIDashboard.razor.cs`
  - `RegisterSPIDashboardRefresh(this)` in `OnInitializedAsync`
  - Unregisters in `Dispose`

## 6.4 Current-state finding
- Runtime publication now exists from SPI datapoint command handlers and dashboard recalculation command handler.

---

## 7) SPI Automation Engine Internals

## 7.1 Service
- `Application/Services/SMSServices/SPIAutomationService.cs`
- Contract: `Application/Interfaces/SMSInterfaces/ISPIAutomationService.cs`

## 7.2 Automated update methods
- `UpdateHazardReportRateAsync`
- `UpdateHazardClosureTimeAsync`
- `UpdateRiskAssessmentCompletionAsync`
- `UpdateHighRiskExposureAsync`
- `UpdateMitigationImplementationRateAsync`
- `UpdateCorrectiveActionClosureAsync`
- `UpdateRiskIdentificationEffectivenessAsync`

## 7.3 Data flow pattern
1. Resolve target SPI (name/indicator-type based lookup using mediator query for all SPIs).
2. Build `SPIDataPoint` payload.
3. Write datapoint through `SafetyPerformanceIndicatorService.AddSPIDataPointAsync(...)`.
4. Log success/failure outcomes with application log methods.

## 7.4 Implementation observations
- Placeholder counter logic for hazards, high-risk assessments, and mitigation overdue/active counts has been replaced with query-backed calculations.
- Dynamic SPI lookup is still mixed with some hardcoded SPI codes, which remains a Phase 2 cleanup opportunity.

---

## Appendix A: Key SPI Query/Read Pipeline
- Query object definitions: `Application/CQRS/Queries/SafetyPerformanceIndicatorQueries.cs`
- Query handlers: `Application/CQRS/QueryHandlers/SafetyPerformanceIndicatorQueryHandlers.cs`
- Dashboard query handler: `Application/CQRS/QueryHandlers/GetSPIDashboardDataQueryHandler.cs`

`SPIDashboard` page issues `GetSPIDashboardDataQuery` and renders summary/cards/trends/alerts from the computed aggregate.

---

## Appendix B: Startup Wiring Summary
- App service registration: `Presentation/SMS3/Program.cs` -> `builder.Services.AddApplicationServices(builder.Configuration)`
- EventBus initialization: `app.InitializeEventBus()`
- Explicit UI subscriptions in startup scope:
  - `UINotificationEvent`
  - `SPIDashboardRefreshEvent`

---

## Appendix C: Open Gaps / Hardening Targets
1. `ArchiveOldSPIDataCommand` still has no implementation.
2. Alert/trend generation service methods (`GenerateAlertsAsync`, `GenerateTrendAnalysisAsync`) remain stubbed and should be implemented for full operational coverage.
3. Consolidated `DomainEventHandlers.cs` mixes many domains; traceability is good but maintainability can degrade as event count grows.

---

## Appendix D: Phase 2 Readiness Checklist (Short)

1. Keep SPI routes and components behind a feature flag (not just hidden nav links).
2. Implement `ArchiveOldSPIDataCommandHandler` or remove/defer command contract explicitly.
3. Implement `GenerateAlertsAsync` and `GenerateTrendAnalysisAsync` in `SafetyPerformanceIndicatorService`.
4. Add focused tests for datapoint command handlers to validate publication of:
   - `SPIComplianceChangedEvent`
   - `SPIThresholdExceededEvent`
   - `SPIDashboardRefreshEvent`
5. Add a smoke test for `RecalculateSPIDashboardCommand` to ensure query + UI refresh publication remains intact.
6. Validate Power BI coexistence strategy (ownership of KPI definitions and source-of-truth semantics) before feature re-enable.

---

## Conclusion
The SPI architecture is operational for core hazard/risk/mitigation-driven updates and dashboard reads, with event-driven hooks in place. The principal near-term design concerns are handler coverage gaps for defined commands and incomplete runtime publication paths for SPI compliance/threshold/dashboard-refresh event types.