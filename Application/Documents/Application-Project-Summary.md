# SMS Application Layer - Project Summary

## Overview

The **Application** project serves as the core business logic layer of the SMS (Safety Management System) application, implementing a Clean Architecture pattern with CQRS (Command Query Responsibility Segregation) and Mediator patterns. This layer orchestrates domain operations, manages business workflows, and provides a clean separation between the presentation layer and infrastructure concerns.

## Project Information

- **Target Framework**: .NET 8.0
- **Architecture Pattern**: Clean Architecture with CQRS
- **Design Patterns**: Mediator, Pipeline, Service Layer, Repository
- **Dependencies**: Domain, Infrastructure, Shared projects

## Architecture Overview

### Core Components

```
Application Layer Structure:
??? Common/           # Shared application components
??? Configuration/    # Dependency injection and service registration
??? Interfaces/       # Application service interfaces
??? Messaging/        # CQRS implementation (Commands, Queries, Handlers)
??? Services/         # Business logic and application services
??? States/           # Workflow state management
??? Documents/        # Project documentation
```

## Key Features

### 1. CQRS Implementation
The application implements Command Query Responsibility Segregation with:
- **Commands**: Handle write operations and business actions
- **Queries**: Handle read operations and data retrieval
- **Handlers**: Process commands and queries with business logic
- **Pipelines**: Cross-cutting concerns (logging, auditing, validation)

### 2. Mediator Pattern
- Decouples presentation layer from business logic
- Centralized request/response handling
- Pipeline-based request processing
- Automatic handler discovery and registration

### 3. Business Domain Coverage

#### User Management
- SMS Application Users
- SMS Organizational Users  
- SMS Stakeholder Users
- User Roles and Permissions
- User Groups (Application, Organizational, Stakeholder)

#### Safety Management
- Hazard Management and Tracking
- Risk Analysis and Assessment
- Investigation Workflows
- Interview Management
- Mitigation Planning and Assignment
- Report Processing and Validation

#### Audit and Compliance
- Audit Plan Management
- Audit Execution and Findings
- Safety Performance Indicators (SPI)
- Evidence Collection and Management

#### Data Management
- Airport Shared Dataset
- Hazard File Management
- Scoring Panel Configuration
- Dashboard Statistics

## Project Structure Details

### Messaging Layer (`/Messaging`)

#### Commands (`/Commands`)
Write operations with business validation:
- User management commands (Create, Update, Delete users)
- Safety workflow commands (Hazards, Investigations, Mitigations)
- Audit management commands (Plans, Audits, Findings)
- System configuration commands

#### Queries (`/Queries`)
Read operations optimized for specific use cases:
- User lookup and search queries
- Dashboard and reporting queries
- Workflow status and tracking queries
- Configuration and reference data queries

#### Command Handlers (`/CommandHandlers`)
Business logic processors for write operations:
- Input validation and business rule enforcement
- Domain entity creation and modification
- Cross-entity coordination and workflows
- Result validation and error handling

#### Query Handlers (`/QueryHandlers`)
Optimized data retrieval processors:
- Performance-optimized data access
- Result transformation and mapping
- Filtering and pagination support
- Aggregation and statistical calculations

#### Pipelines (`/Pipelines`)
Cross-cutting concern processors:
- **AuditFieldsPipeline**: Automatic audit field population
- **LoggingPipeline**: Request/response logging
- **AuditLogPipeline**: Business action audit trails

### Services Layer (`/Services`)

#### Core Services
- **MediatorService**: Central request routing and pipeline management
- **MessengerService**: Internal messaging and notifications
- **CurrentUserService**: User context and session management
- **SMSSessionService**: Session state and authentication management

#### Domain Services
- **HazardService**: Hazard lifecycle and workflow management
- **InvestigationService**: Investigation process coordination
- **RiskAssessmentService**: Risk analysis and scoring
- **MitigationService**: Mitigation planning and tracking
- **ReportService**: Report generation and processing

#### User Management Services
- **SMSApplicationUserService**: Administrative user management
- **SMSOrganizationalUserService**: Organizational user operations
- **SMSStakeholderUserService**: External stakeholder management
- **SMSUserRoleService**: Role and permission management

#### Specialized Services
- **SMSAuditService**: Audit execution and management
- **SafetyPerformanceIndicatorService**: SPI calculation and tracking
- **HazardFileService**: File attachment and document management
- **SMSInvestigationWorkflowService**: Investigation state management

### Configuration Layer (`/Configuration`)

#### Dependency Injection Setup
- **DependencyInjection.cs**: Main service registration
- **ServiceCollectionExtensions.cs**: Mediator and pipeline registration
- Automatic handler discovery and registration
- Scoped service lifetime management

### Common Components (`/Common`)

#### Base Classes
- **BaseCommandBundle**: Command base with audit support
- **BaseQueryBundle**: Query base with logging support  
- **BaseEventBundle**: Event handling foundation
- **BaseState**: Workflow state management base

#### Utilities
- **ApplicationEventIds**: Standardized logging event IDs
- **ApplicationLogMessages**: Consistent log message formatting
- **ModelMappers**: DTO to domain entity mapping
- **SPIConstants**: Safety Performance Indicator constants

## Technical Implementation

### Pipeline Processing Order
1. **AuditFieldsPipeline**: Sets CreatedBy, UpdatedBy, timestamps
2. **LoggingPipeline**: Logs request/response for debugging
3. **AuditLogPipeline**: Records business actions for compliance

### Error Handling Strategy
- Domain-driven error types and messages
- Graceful failure handling with detailed logging
- Business rule validation before data persistence
- Consistent error response format across operations

### Performance Considerations
- Scoped service registration for optimal memory usage
- Optimized query handlers for read operations
- Minimal data transfer objects (DTOs) for API responses
- Efficient entity mapping and transformation

## Integration Points

### Domain Layer Integration
- Direct use of domain entities and value objects
- Domain service coordination for complex operations
- Business rule enforcement through domain methods
- Event handling for domain state changes

### Infrastructure Layer Integration
- Repository pattern for data access abstraction
- Data service layer for optimized operations
- External service integration (email, file storage)
- Database transaction coordination

### Presentation Layer Integration
- Clean API surface through mediator pattern
- Standardized request/response models
- Session and authentication state management
- Real-time updates through SignalR circuit handlers

## Configuration and Setup

### Service Registration
```csharp
// In Program.cs or Startup.cs
services.AddApplicationServices();
```

### Key Dependencies
- **Microsoft.Extensions.Logging**: Comprehensive logging support
- **Microsoft.AspNetCore.Http**: HTTP context access for user sessions

## Business Workflow Examples

### User Management Workflow
1. **Create User Command** ? Validation ? Domain Entity Creation ? Audit Logging
2. **Assign Role Command** ? Permission Validation ? Role Assignment ? Notification
3. **Get User Query** ? Authorization Check ? Data Retrieval ? Response Formatting

### Safety Management Workflow
1. **Report Hazard** ? Hazard Entity Creation ? Investigation Assignment ? Stakeholder Notification
2. **Conduct Investigation** ? Evidence Collection ? Risk Assessment ? Mitigation Planning
3. **Track Mitigation** ? Progress Monitoring ? Effectiveness Evaluation ? Closure Documentation

### Audit Management Workflow
1. **Plan Audit** ? Scope Definition ? Resource Assignment ? Schedule Creation
2. **Execute Audit** ? Evidence Collection ? Finding Documentation ? Report Generation
3. **Follow-up Actions** ? Corrective Action Assignment ? Progress Tracking ? Verification

## Quality Assurance

### Code Quality Features
- Comprehensive nullable reference type handling
- Consistent error handling and logging patterns
- Separation of concerns with clear layer boundaries
- Testable design with dependency injection

### Monitoring and Diagnostics
- Structured logging with correlation IDs
- Performance metrics collection
- Business action audit trails
- Exception tracking and reporting

## Future Enhancements

### Planned Features
- Advanced workflow state machines
- Real-time collaboration features
- Enhanced reporting and analytics
- Mobile application API support
- Integration with external safety systems

### Technical Improvements
- GraphQL query support for flexible data access
- Event sourcing for complete audit trails
- Distributed caching for performance optimization
- Microservice decomposition for scalability

## Conclusion

The Application layer provides a robust, scalable foundation for the SMS system with clear separation of concerns, comprehensive business logic implementation, and flexible integration capabilities. The CQRS and Mediator patterns ensure maintainable code that can evolve with changing business requirements while maintaining system reliability and performance.

---

**Document Version**: 1.0  
**Last Updated**: January 2025  
**Author**: SMS Development Team  
**Review Date**: June 2025