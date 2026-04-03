# SMS Application Layer - Project Summary

## AP-1.0.0 Overview

The **Application** project serves as the core business logic layer of the SMS (Safety Management System) application, implementing a Clean Architecture pattern with CQRS (Command Query Responsibility Segregation) and Mediator patterns. This layer orchestrates domain operations, manages business workflows, and provides a clean separation between the presentation layer and infrastructure concerns.

## AP-1.1.0 Project Information

- **Target Framework**: .NET 8.0
- **Namespace**: SMS_Application
- **Architecture Pattern**: Clean Architecture with CQRS
- **Design Patterns**: Mediator, Pipeline, Service Layer, Repository
- **Dependencies**: Domain, Infrastructure, Shared projects

## AP-1.2.0 Architecture Overview

### AP-1.2.1 Core Components

```
Application Layer Structure:
├── Attributes/       # Custom attributes for application layer
├── Common/           # Shared application components
├── Configuration/    # Dependency injection and service registration
├── CQRS/            # CQRS implementation (Commands, Queries, Handlers, Pipelines)
├── Interfaces/       # Application service interfaces
├── Scripts/          # Database and utility scripts
├── Services/         # Business logic and application services
└── Documents/        # Project documentation
```

## AP-1.3.0 Key Features

### AP-1.3.1 CQRS Implementation
The application implements Command Query Responsibility Segregation with:
- **AP-1.3.1.1 Commands**: Handle write operations and business actions
- **AP-1.3.1.2 Queries**: Handle read operations and data retrieval
- **AP-1.3.1.3 Command Handlers**: Process commands with business logic
- **AP-1.3.1.4 Query Handlers**: Process queries with optimized data access
- **AP-1.3.1.5 Pipelines**: Cross-cutting concerns (logging, auditing, validation)
- **AP-1.3.1.6 Circuit Handlers**: Blazor SignalR circuit management

### AP-1.3.2 Mediator Pattern
- **AP-1.3.2.1** Decouples presentation layer from business logic
- **AP-1.3.2.2** Centralized request/response handling
- **AP-1.3.2.3** Pipeline-based request processing
- **AP-1.3.2.4** Automatic handler discovery and registration

### AP-1.3.3 Business Domain Coverage

#### AP-1.3.3.1 User Management
- **AP-1.3.3.1.1** SMS Application Users
- **AP-1.3.3.1.2** SMS Organizational Users  
- **AP-1.3.3.1.3** SMS Stakeholder Users
- **AP-1.3.3.1.4** User Roles and Permissions
- **AP-1.3.3.1.5** User Groups (Application, Organizational, Stakeholder)

#### AP-1.3.3.2 Safety Management
- **AP-1.3.3.2.1** Hazard Management and Tracking
- **AP-1.3.3.2.2** Risk Analysis and Assessment
- **AP-1.3.3.2.3** Investigation Workflows
- **AP-1.3.3.2.4** Interview Management
- **AP-1.3.3.2.5** Mitigation Planning and Assignment
- **AP-1.3.3.2.6** Report Processing and Validation

#### AP-1.3.3.3 Audit and Compliance
- **AP-1.3.3.3.1** Audit Plan Management
- **AP-1.3.3.3.2** Audit Execution and Findings
- **AP-1.3.3.3.3** Safety Performance Indicators (SPI)
- **AP-1.3.3.3.4** Evidence Collection and Management

#### AP-1.3.3.4 Data Management
- **AP-1.3.3.4.1** Airport Shared Dataset
- **AP-1.3.3.4.2** Hazard File Management
- **AP-1.3.3.4.3** Scoring Panel Configuration
- **AP-1.3.3.4.4** Dashboard Statistics

## AP-2.0.0 Project Structure Details

### AP-2.1.0 CQRS Layer (`/CQRS`)

#### AP-2.1.1 Commands (`/Commands`)
Write operations with business validation:
- **AP-2.1.1.1** User management commands (Create, Update, Delete users)
- **AP-2.1.1.2** Safety workflow commands (Hazards, Investigations, Mitigations)
- **AP-2.1.1.3** Audit management commands (Plans, Audits, Findings)
- **AP-2.1.1.4** System configuration commands
- **AP-2.1.1.5** Authentication and authorization commands

#### AP-2.1.2 Queries (`/Queries`)
Read operations optimized for specific use cases:
- **AP-2.1.2.1** User lookup and search queries
- **AP-2.1.2.2** Dashboard and reporting queries
- **AP-2.1.2.3** Workflow status and tracking queries
- **AP-2.1.2.4** Configuration and reference data queries
- **AP-2.1.2.5** Risk analysis and assessment queries

#### AP-2.1.3 Command Handlers (`/CommandHandlers`)
Business logic processors for write operations:
- **AP-2.1.3.1** Input validation and business rule enforcement
- **AP-2.1.3.2** Domain entity creation and modification
- **AP-2.1.3.3** Cross-entity coordination and workflows
- **AP-2.1.3.4** Result validation and error handling
- **AP-2.1.3.5** Audit log command handlers

#### AP-2.1.4 Query Handlers (`/QueryHandlers`)
Optimized data retrieval processors:
- **AP-2.1.4.1** Performance-optimized data access
- **AP-2.1.4.2** Result transformation and mapping
- **AP-2.1.4.3** Filtering and pagination support
- **AP-2.1.4.4** Aggregation and statistical calculations
- **AP-2.1.4.5** Dashboard statistics handlers

#### AP-2.1.5 Pipelines (`/Pipelines`)
Cross-cutting concern processors:
- **AP-2.1.5.1 AuditFieldsPipeline**: Automatic audit field population
- **AP-2.1.5.2 LoggingPipeline**: Request/response logging
- **AP-2.1.5.3 AuditLogPipeline**: Business action audit trails
- **AP-2.1.5.4 CommandAuditPipeline**: Command execution auditing
- **AP-2.1.5.5 QueryAuditPipeline**: Query access auditing
- **AP-2.1.5.6 ValidationPipeline**: Request validation

#### AP-2.1.6 Circuit Handlers (`/CircuitHandlers`)
Blazor-specific handlers:
- **AP-2.1.6.1 BaseCircuitHandler**: Base functionality for circuit management
- **AP-2.1.6.2 SMS_CircuitHandler**: SMS-specific circuit handling

### AP-2.2.0 Services Layer (`/Services`)

#### AP-2.2.1 Core Services
- **AP-2.2.1.1 MediatorService**: Central request routing and pipeline management
- **AP-2.2.1.2 MessengerService**: Internal messaging and notifications
- **AP-2.2.1.3 SystemService**: System-level operations

#### AP-2.2.2 Authentication Services
- **AP-2.2.2.1 AuthenticationService**: Core authentication logic
- **AP-2.2.2.2 BlazorAuthenticationService**: Blazor-specific authentication
- **AP-2.2.2.3 AuthenticationStateCache**: Authentication state management
- **AP-2.2.2.4 AuthenticationStrategyManager**: Authentication strategy coordination
- **AP-2.2.2.5 TwoFactorAuthService**: Two-factor authentication
- **AP-2.2.2.6 SessionBasedCurrentUserService**: Session-based user context
- **AP-2.2.2.7 StaticCurrentUserService**: Static user context for testing
- **AP-2.2.2.8 StrategyBasedCurrentUserService**: Strategy-based user context

#### AP-2.2.3 Authorization Services
- **AP-2.2.3.1 AuthorizationService**: Core authorization logic
- **AP-2.2.3.2 SMSAuthorizationService**: SMS-specific authorization

#### AP-2.2.4 Session Management Services
- **AP-2.2.4.1 SMSSessionService**: SMS session management
- **AP-2.2.4.2 SessionTimerService**: Session timeout management
- **AP-2.2.4.3 TwoFactorSessionTimerService**: Two-factor session timing
- **AP-2.2.4.4 BlazorCircuitAuthStorage**: Blazor circuit authentication storage

#### AP-2.2.5 Security Services
- **AP-2.2.5.1 SecurityFeatureService**: Security feature management
- **AP-2.2.5.2 SecurityMonitoringService**: Security monitoring and alerts

#### AP-2.2.6 User Management Services
- **AP-2.2.6.1 SMSApplicationUserService**: Administrative user management
- **AP-2.2.6.2 SMSOrganizationalUserService**: Organizational user operations
- **AP-2.2.6.3 SMSStakeholderUserService**: External stakeholder management
- **AP-2.2.6.4 SMSUserRoleService**: Role and permission management
- **AP-2.2.6.5 UserCompletenessValidator**: User data validation
- **AP-2.2.6.6 UserInstantiationService**: User creation and setup

#### AP-2.2.7 Group Management Services
- **AP-2.2.7.1 SMSApplicationGroupService**: Application group management
- **AP-2.2.7.2 SMSOrganizationalGroupService**: Organizational group operations
- **AP-2.2.7.3 SMSStakeholderGroupService**: Stakeholder group management

#### AP-2.2.8 Safety Management Services
- **AP-2.2.8.1 HazardService**: Hazard lifecycle and workflow management
- **AP-2.2.8.2 HazardLocationService**: Hazard location management
- **AP-2.2.8.3 HazardReportTrackingService**: Hazard report tracking
- **AP-2.2.8.4 HazardFileService**: File attachment and document management
- **AP-2.2.8.5 InvestigationService**: Investigation process coordination
- **AP-2.2.8.6 InterviewService**: Interview management
- **AP-2.2.8.7 RiskAnalysisService**: Risk analysis processing
- **AP-2.2.8.8 RiskAssessmentService**: Risk assessment coordination
- **AP-2.2.8.9 ReportService**: Report generation and processing
- **AP-2.2.8.10 ReportValidationService**: Report validation logic

#### AP-2.2.9 Mitigation Services
- **AP-2.2.9.1 MitigationService**: Mitigation planning and tracking
- **AP-2.2.9.2 MitigationAssignmentService**: Mitigation assignment management

#### AP-2.2.10 Audit Services
- **AP-2.2.10.1 SMSAuditService**: Audit execution and management
- **AP-2.2.10.2 SMSAuditPlanService**: Audit planning and scheduling
- **AP-2.2.10.3 CommandAccessAuditService**: Command access auditing
- **AP-2.2.10.4 QueryAccessAuditService**: Query access auditing

#### AP-2.2.11 Performance and Configuration Services
- **AP-2.2.11.1 SafetyPerformanceIndicatorService**: SPI calculation and tracking
- **AP-2.2.11.2 ScoringPanelService**: Scoring panel configuration
- **AP-2.2.11.3 AirportSharedDatasetService**: Airport data management

#### AP-2.2.12 Workflow Services
- **AP-2.2.12.1 SMSInvestigationWorkflowService**: Investigation state management
- **AP-2.2.12.2 SMSRiskAssessmentWorkflowService**: Risk assessment workflow
- **AP-2.2.12.3 SMSWorkflowService**: General workflow management

#### AP-2.2.13 Protocol and Detection Services
- **AP-2.2.13.1 ProtocolDetectionService**: Protocol detection and analysis

### AP-2.3.0 Configuration Layer (`/Configuration`)

#### AP-2.3.1 Core Configuration
- **AP-2.3.1.1 DependencyInjection.cs**: Main service registration
- **AP-2.3.1.2 ServiceCollectionExtensions.cs**: Mediator and pipeline registration

#### AP-2.3.2 Specialized Configuration
- **AP-2.3.2.1 AuthenticationConfiguration.cs**: Authentication setup
- **AP-2.3.2.2 SessionConfiguration.cs**: Session management configuration
- **AP-2.3.2.3 TwoFactorAuthConfiguration.cs**: Two-factor authentication setup
- **AP-2.3.2.4 RequestValidationConfiguration.cs**: Request validation setup

#### AP-2.3.3 Registration Features
- **AP-2.3.3.1** Automatic handler discovery and registration
- **AP-2.3.3.2** Scoped service lifetime management
- **AP-2.3.3.3** Pipeline configuration and ordering

### AP-2.4.0 Common Components (`/Common`)

#### AP-2.4.1 Base Classes
- **AP-2.4.1.1 BaseCommandBundle**: Command base with audit support
- **AP-2.4.1.2 BaseQueryBundle**: Query base with logging support  
- **AP-2.4.1.3 BaseEventBundle**: Event handling foundation
- **AP-2.4.1.4 BaseState**: Workflow state management base

#### AP-2.4.2 Utilities and Constants
- **AP-2.4.2.1 ApplicationEventIds**: Standardized logging event IDs
- **AP-2.4.2.2 ApplicationLogMessages**: Consistent log message formatting
- **AP-2.4.2.3 ModelMappers**: DTO to domain entity mapping
- **AP-2.4.2.4 SPIConstants**: Safety Performance Indicator constants

#### AP-2.4.3 Behaviors
- **AP-2.4.3.1 AuditFieldsPipelineBehavior**: Pipeline behavior for audit field management

### AP-2.5.0 Interfaces Layer (`/Interfaces`)

#### AP-2.5.1 Core Interfaces
- **AP-2.5.1.1 IMediator**: Mediator pattern interface
- **AP-2.5.1.2 IRequest**: Request interface
- **AP-2.5.1.3 IRequestHandler**: Request handler interface
- **AP-2.5.1.4 IPipeline**: Pipeline interface
- **AP-2.5.1.5 IMessenger**: Messaging interface

#### AP-2.5.2 State and Strategy Interfaces
- **AP-2.5.2.1 IBaseState**: Base state interface
- **AP-2.5.2.2 IBaseStrategy**: Base strategy interface
- **AP-2.5.2.3 IBaseMachine**: Base machine interface

#### AP-2.5.3 Authentication and Authorization Interfaces
- **AP-2.5.3.1 IAuthenticationService**: Authentication service interface
- **AP-2.5.3.2 IAuthenticationStrategy**: Authentication strategy interface
- **AP-2.5.3.3 IAuthenticationStrategyManager**: Authentication strategy manager
- **AP-2.5.3.4 IAuthorizationService**: Authorization service interface
- **AP-2.5.3.5 ICurrentUserService**: Current user service interface
- **AP-2.5.3.6 ISMSSessionService**: SMS session service interface

#### AP-2.5.4 Business Service Interfaces
- **AP-2.5.4.1 IHazardService**: Hazard management interface
- **AP-2.5.4.2 IHazardFileService**: Hazard file management interface
- **AP-2.5.4.3 IRiskAssessmentService**: Risk assessment interface
- **AP-2.5.4.4 IRiskAnalysisService**: Risk analysis interface
- **AP-2.5.4.5 IInterviewService**: Interview management interface
- **AP-2.5.4.6 IMitigationService**: Mitigation service interface
- **AP-2.5.4.7 IReportService**: Report service interface
- **AP-2.5.4.8 IReportValidationService**: Report validation interface
- **AP-2.5.4.9 IScoringPanelService**: Scoring panel interface
- **AP-2.5.4.10 ISafetyPerformanceIndicatorService**: SPI service interface

#### AP-2.5.5 User Management Interfaces
- **AP-2.5.5.1 ISMSApplicationUserService**: Application user service
- **AP-2.5.5.2 ISMSOrganizationalUserService**: Organizational user service
- **AP-2.5.5.3 ISMSStakeholderUserService**: Stakeholder user service
- **AP-2.5.5.4 ISMSApplicationGroupService**: Application group service
- **AP-2.5.5.5 ISMSOrganizationalGroupService**: Organizational group service
- **AP-2.5.5.6 IUserCompletenessValidator**: User validation interface
- **AP-2.5.5.7 IUserInstantiationService**: User instantiation interface

#### AP-2.5.6 Workflow Interfaces
- **AP-2.5.6.1 ISMSInvestigationWorkflowService**: Investigation workflow interface

#### AP-2.5.7 Audit Interfaces
- **AP-2.5.7.1 IAuditableCommand**: Auditable command interface

#### AP-2.5.8 Protocol Detection Interfaces
- **AP-2.5.8.1 IProtocolDetectionService**: Protocol detection interface
- **AP-2.5.8.2 IMasterProtocolService**: Master protocol service interface

## AP-3.0.0 Technical Implementation

### AP-3.1.0 Pipeline Processing Order
1. **AP-3.1.1 ValidationPipeline**: Request validation and data integrity
2. **AP-3.1.2 AuditFieldsPipeline**: Sets CreatedBy, UpdatedBy, timestamps
3. **AP-3.1.3 LoggingPipeline**: Logs request/response for debugging
4. **AP-3.1.4 CommandAuditPipeline**: Command execution auditing
5. **AP-3.1.5 QueryAuditPipeline**: Query access auditing

### AP-3.2.0 Error Handling Strategy
- **AP-3.2.1** Domain-driven error types and messages
- **AP-3.2.2** Graceful failure handling with detailed logging
- **AP-3.2.3** Business rule validation before data persistence
- **AP-3.2.4** Consistent error response format across operations

### AP-3.3.0 Performance Considerations
- **AP-3.3.1** Scoped service registration for optimal memory usage
- **AP-3.3.2** Optimized query handlers for read operations
- **AP-3.3.3** Minimal data transfer objects (DTOs) for API responses
- **AP-3.3.4** Efficient entity mapping and transformation

## AP-4.0.0 Integration Points

### AP-4.1.0 Domain Layer Integration
- **AP-4.1.1** Direct use of domain entities and value objects
- **AP-4.1.2** Domain service coordination for complex operations
- **AP-4.1.3** Business rule enforcement through domain methods
- **AP-4.1.4** Event handling for domain state changes

### AP-4.2.0 Infrastructure Layer Integration
- **AP-4.2.1** Repository pattern for data access abstraction
- **AP-4.2.2** Data service layer for optimized operations
- **AP-4.2.3** External service integration (email, file storage)
- **AP-4.2.4** Database transaction coordination

### AP-4.3.0 Presentation Layer Integration
- **AP-4.3.1** Clean API surface through mediator pattern
- **AP-4.3.2** Standardized request/response models
- **AP-4.3.3** Session and authentication state management
- **AP-4.3.4** Real-time updates through SignalR circuit handlers

## AP-5.0.0 Configuration and Setup

### AP-5.1.0 Service Registration
```csharp
// In Program.cs or Startup.cs
services.AddApplicationServices();
```

### AP-5.2.0 Key Dependencies
- **AP-5.2.1 Microsoft.Extensions.Logging**: Comprehensive logging support
- **AP-5.2.2 Microsoft.AspNetCore.Http**: HTTP context access for user sessions

## AP-6.0.0 Business Workflow Examples

### AP-6.1.0 User Management Workflow
1. **AP-6.1.1 Create User Command** → Validation → Domain Entity Creation → Audit Logging
2. **AP-6.1.2 Assign Role Command** → Permission Validation → Role Assignment → Notification
3. **AP-6.1.3 Get User Query** → Authorization Check → Data Retrieval → Response Formatting

### AP-6.2.0 Safety Management Workflow
1. **AP-6.2.1 Report Hazard** → Hazard Entity Creation → Investigation Assignment → Stakeholder Notification
2. **AP-6.2.2 Conduct Investigation** → Evidence Collection → Risk Assessment → Mitigation Planning
3. **AP-6.2.3 Track Mitigation** → Progress Monitoring → Effectiveness Evaluation → Closure Documentation

### AP-6.3.0 Audit Management Workflow
1. **AP-6.3.1 Plan Audit** → Scope Definition → Resource Assignment → Schedule Creation
2. **AP-6.3.2 Execute Audit** → Evidence Collection → Finding Documentation → Report Generation
3. **AP-6.3.3 Follow-up Actions** → Corrective Action Assignment → Progress Tracking → Verification

## AP-7.0.0 Quality Assurance

### AP-7.1.0 Code Quality Features
- **AP-7.1.1** Comprehensive nullable reference type handling
- **AP-7.1.2** Consistent error handling and logging patterns
- **AP-7.1.3** Separation of concerns with clear layer boundaries
- **AP-7.1.4** Testable design with dependency injection

### AP-7.2.0 Monitoring and Diagnostics
- **AP-7.2.1** Structured logging with correlation IDs
- **AP-7.2.2** Performance metrics collection
- **AP-7.2.3** Business action audit trails
- **AP-7.2.4** Exception tracking and reporting

## AP-8.0.0 Future Enhancements

### AP-8.1.0 Planned Features
- **AP-8.1.1** Advanced workflow state machines
- **AP-8.1.2** Real-time collaboration features
- **AP-8.1.3** Enhanced reporting and analytics
- **AP-8.1.4** Mobile application API support
- **AP-8.1.5** Integration with external safety systems

### AP-8.2.0 Technical Improvements
- **AP-8.2.1** GraphQL query support for flexible data access
- **AP-8.2.2** Event sourcing for complete audit trails
- **AP-8.2.3** Distributed caching for performance optimization
- **AP-8.2.4** Microservice decomposition for scalability

## AP-9.0.0 Conclusion

The Application layer provides a robust, scalable foundation for the SMS system with clear separation of concerns, comprehensive business logic implementation, and flexible integration capabilities. The CQRS and Mediator patterns ensure maintainable code that can evolve with changing business requirements while maintaining system reliability and performance.

---

**Document Version**: 2.0  
**Last Updated**: January 2025  
**Author**: SMS Development Team  
**Review Date**: June 2025