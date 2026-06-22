# Event-Driven Architecture Guide
## Domain\Events, EventBus, EventHandlers, SPI, and SMS Assurance

## 1) Purpose

This document explains how the current event-driven architecture works in this solution, how it connects to SPI automation, and how the SMS Assurance module should use it.

It covers:
- `Domain\Events` (Domain, UI, Integration events)
- EventBus contracts and runtime services
- Event handlers and subscription model
- How SPI and SMS Assurance currently use events
- Recommended target usage pattern

---

## 2) Core Building Blocks

## 2.1 Event contracts

### Domain events
- Contract: `Domain/Interfaces/IBaseDomainEvent.cs`
- Base type: `Domain/Common/BaseDomainEvent.cs`
- Typical purpose: represent business state changes (hazard lifecycle, mitigation, risk assessment, SPI compliance)

### Integration events
- Contract: `Domain/Interfaces/IBaseIntegrationEvent.cs`
- Typical purpose: communication with external systems (example: email)

### UI events
- Contract: `Domain/Interfaces/IBaseUIEvent.cs`
- Typical purpose: notifications and UI refresh actions (Blazor-facing behavior)

### SPI data-source events
- Contract: `Domain/Interfaces/IEventSource.cs`
- Purpose: events that should be discoverable as SPI data sources (metadata for dropdowns/configuration)

---

## 2.2 Event categories under `Domain/Events`

- Domain events: `Domain/Events/DomainEvents/*.cs`
  - Examples: `HazardCreatedEvent`, `HazardStatusChangedEvent`, `RiskAssessmentCompletedEvent`, `SPIComplianceChangedEvent`, `SPIThresholdExceededEvent`
- Integration events: `Domain/Events/IntegrationEvents/*.cs`
  - Example: `EmailNotificationEvent`
- UI events: `Domain/Events/UIEvents/*.cs`
  - Examples: `UINotificationEvent`, `SPIDashboardRefreshEvent`

---

## 2.3 EventBus abstraction and implementations

### Main EventBus contract
- Interface: `Application/Interfaces/EventBusInterfaces/IBaseEventBus.cs`
- Supports publishing by category:
  - `PublishDomainEventAsync(...)`
  - `PublishIntegrationEventAsync(...)`
  - `PublishUIEventAsync(...)`
- Supports handler subscriptions:
  - `Subscribe<TEvent,THandler>()` (domain)
  - `SubscribeIntegration<TEvent,THandler>()`
  - `SubscribeUI<TEvent,THandler>()`

### Event execution modes
Defined in `IBaseEventBus.cs`:
- `Immediate` (execute handlers now)
- `Queued` (store and process later)
- `Manual` (store for explicit/manual execution)

### Dispatch service
- `Application/Services/EventBusServices/EventDispatchService.cs`
- Maintains runtime map: `event type -> handlers`
- Uses DI scopes to resolve handler instances per execution

### Queue service
- `Application/Services/EventBusServices/EventQueueService.cs`
- Stores and executes queued events (manual or batch)
- Rehydrates event payloads and republishes using EventBus
- Important: has explicit type maps for replay paths

---

## 2.4 Handler model

### Generic handler contract
- `Application/Interfaces/EventBusInterfaces/IBaseEventHandler.cs`

### Specialized marker contracts
- `IDomainEventHandler<T>`
- `IIntegrationEventHandler<T>`
- `IUIEventHandler<T>`

### Base handler classes
- `BaseDomainEventHandler<T>`
- `BaseIntegrationEventHandler<T>`
- `BaseUIEventHandler<T>`

### Startup subscription
- `Application/Configuration/EventBusExtensions.cs`
  - `InitializeEventBus()` auto-discovers handlers implementing `IBaseEventHandler<>`
  - Subscribes by event interface type
- `Presentation/SMS3/Program.cs`
  - Calls `app.InitializeEventBus()`
  - Explicitly subscribes Blazor `UIEventHandler` for `UINotificationEvent`

---

## 3) SPI Integration (Current State)

## 3.1 SPI data source discovery from events

- Implemented in `Application/Common/SPIConstants.cs`
- `SPIDataSources.GetEventDrivenSources()` uses reflection to find event types implementing `IEventSource`
- This powers dynamic event-source discovery for SPI configuration/test pages

### Current behavior
- Events implementing `IEventSource` can appear as dynamic SPI source options
- Metadata comes from event class properties:
  - display name
  - category
  - description
  - automatic flag
  - display priority

---

## 3.2 SPI automation services

- `Application/Services/SMSServices/SPIAutomationService.cs`
  - Uses mediator (`IBaseMediator`) and CQRS queries to update SPI metrics
  - Example: hazard rate and validated risk counts

- `Application/Services/SMSServices/SPIEventCoordinator.cs`
  - Currently marked as **temporarily simplified**
  - Logs events but does not fully orchestrate event-driven SPI workflows yet

---

## 3.3 Domain handler integration with SPI

- `Application/EventHandlers/DomainEventHandlers/DomainEventHandlers.cs`
  - Contains domain handlers (e.g., hazard-created flow)
  - Example pattern:
	1. process domain event
	2. call SPI automation service
	3. publish integration/UI events

This indicates SPI updates are partly event-driven, but the end-to-end pattern is still transitional.

---

## 4) SMS Assurance Integration (Current State)

## 4.1 SPI dashboard and configuration pages

- `Presentation/SMS3/Components/Pages/SMSAssurance/SPIDashboard.razor.cs`
  - Loads data via `IBaseMediator` query (`GetSPIDashboardDataQuery`)
  - No direct EventBus subscription in this component

- `Presentation/SMS3/Components/Pages/SMSAssurance/SPIConfiguration.razor.cs`
  - Manages SPI definitions via mediator commands/queries
  - Event-source discovery is available through SPI constants/reflection infrastructure

---

## 4.2 UI notification/event bridge

- `Presentation/SMS3/EventHandlers/UIEventHandler.cs`
  - Handles `UINotificationEvent`
  - Uses `EventBusDispatcher` for Blazor-safe UI thread dispatch

- `Presentation/SMS3/Components/Shared/Common/EventBusDispatcher.cs`
  - Static receiver registry for UI components
  - Allows handlers running outside component context to push notifications

---

## 4.3 Queue management UI

- `Presentation/SMS3/Components/Pages/SMSSystem/EventBus/EventBusQueueManager.razor.cs`
  - Allows manual execution/cancellation/inspection of queued events
  - Useful for validating event workflows and replay behavior

---

## 5) What Is Working Well

1. Clear separation of event categories (Domain/UI/Integration)
2. Central EventBus contract with explicit execution modes
3. Reflection-based auto-subscription of handlers
4. SPI data-source discovery via `IEventSource`
5. Queue + manual replay tooling for operational visibility

---

## 6) Gaps and Risks (Current Implementation)

1. **SPI orchestration is partial**
   - `SPIEventCoordinator` is intentionally simplified
   - Not yet the central orchestrator implied by design

2. **Dashboard refresh event path is not fully wired**
   - `SPIDashboardRefreshEvent` exists, but Assurance dashboard flow is still mediator-refresh centered

3. **Queue replay type maps require maintenance**
   - New event types must be added to maps in queue replay logic or replay fails for unknown types

4. **Event model consistency is mixed**
   - Some events are highly metadata-rich (`IEventSource`), some are basic transport notifications

5. **Potential drift between “event intended” vs “event actually consumed”**
   - Some events are defined and published conceptually but not always consumed by targeted Assurance UI components

---

## 7) Recommended Target Pattern for SPI + SMS Assurance

## 7.1 Canonical event flow

1. **Domain state change occurs**
   - e.g., hazard created/updated/closed, risk assessment completed
2. **Publish domain event**
   - via `IBaseEventBus.PublishDomainEventAsync(...)`
3. **Domain handler updates SPI metrics**
   - call `ISPIAutomationService` and write updated data points
4. **If threshold/compliance condition changes**
   - publish `SPIThresholdExceededEvent` and/or `SPIComplianceChangedEvent`
5. **Publish UI refresh + notification events**
   - `SPIDashboardRefreshEvent` for targeted dashboard refresh
   - `UINotificationEvent` for user-visible toast/alerts
6. **Publish integration events if needed**
   - `EmailNotificationEvent` (queued/manual/immediate as policy requires)

---

## 7.2 Execution mode guidance for SPI/SMS Assurance

- Domain events: generally `Immediate`
- UI events:
  - `Immediate` for user experience-critical updates
  - `Manual` only when intentionally testing flows
- Integration events:
  - default `Queued` for reliability and retry control

---

## 7.3 SMS Assurance module expectations

For Assurance pages (`SPIDashboard`, `SPIConfiguration`, audit/risk views):
- Use mediator for data reads/writes (already in place)
- Use EventBus-driven signals to trigger re-query/refresh when critical metrics change
- Keep notifications event-driven through `UINotificationEvent` + `EventBusDispatcher`

---

## 8) Practical “Should Use” Checklist

1. Every SPI-relevant domain transition should emit one canonical domain event
2. Every SPI-affecting handler should:
   - update SPI data through service/CQRS
   - publish threshold/compliance events when state crosses boundaries
3. Assurance dashboard should respond to `SPIDashboardRefreshEvent` (targeted refresh)
4. Keep all new replayable events registered in queue replay maps
5. Keep `IEventSource` metadata complete for any event intended as SPI configuration source

---

## 9) Suggested Next Technical Steps

1. **Finish SPIEventCoordinator orchestration**
   - Move from logging-only to concrete publish/update orchestration

2. **Wire SPIDashboardRefreshEvent consumption in Assurance UI**
   - Ensure dashboard receives refresh signal and re-runs query pipeline

3. **Harden replay maps and discovery**
   - Add tests ensuring all known events are replayable when queued

4. **Standardize event contracts**
   - Align EventType values, ReportId propagation, and metadata richness across event classes

5. **Add architecture tests**
   - Verify: published event -> handler executes -> SPI update -> UI/integration follow-up events emitted

---

## 10) File Reference Index

### Event contracts and base types
- `Domain/Interfaces/IBaseDomainEvent.cs`
- `Domain/Interfaces/IBaseIntegrationEvent.cs`
- `Domain/Interfaces/IBaseUIEvent.cs`
- `Domain/Interfaces/IEventSource.cs`
- `Domain/Common/BaseDomainEvent.cs`

### Event definitions
- `Domain/Events/DomainEvents/*.cs`
- `Domain/Events/IntegrationEvents/*.cs`
- `Domain/Events/UIEvents/*.cs`

### EventBus and handlers
- `Application/Interfaces/EventBusInterfaces/IBaseEventBus.cs`
- `Application/Services/EventBusServices/EventDispatchService.cs`
- `Application/Services/EventBusServices/EventQueueService.cs`
- `Application/Configuration/EventBusExtensions.cs`
- `Application/Interfaces/EventBusInterfaces/BaseDomainEventHandler.cs`
- `Application/Interfaces/EventBusInterfaces/BaseIntegrationEventHandler.cs`
- `Application/Interfaces/EventBusInterfaces/BaseUIEventHandler.cs`

### SPI + Assurance integration
- `Application/Common/SPIConstants.cs`
- `Application/Services/SMSServices/SPIAutomationService.cs`
- `Application/Services/SMSServices/SPIEventCoordinator.cs`
- `Application/EventHandlers/DomainEventHandlers/DomainEventHandlers.cs`
- `Presentation/SMS3/Components/Pages/SMSAssurance/SPIDashboard.razor.cs`
- `Presentation/SMS3/Components/Pages/SMSAssurance/SPIConfiguration.razor.cs`
- `Presentation/SMS3/EventHandlers/UIEventHandler.cs`
- `Presentation/SMS3/Components/Shared/Common/EventBusDispatcher.cs`
- `Presentation/SMS3/Components/Pages/SMSSystem/EventBus/EventBusQueueManager.razor.cs`
- `Presentation/SMS3/Program.cs`

---

## 11) Bottom Line

The architecture already has the right primitives (event contracts, categorized EventBus, handler templates, queue, and Blazor notification bridge). The main work remaining is **closing the loop** for SPI/SMS Assurance so domain transitions consistently drive SPI recalculation, threshold/compliance events, and dashboard refresh/notifications in a deterministic, testable flow.
