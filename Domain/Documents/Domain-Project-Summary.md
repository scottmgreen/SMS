# SMS Domain Layer - Project Summary

## Overview

The **Domain** project serves as the core business domain layer of the SMS (Safety Management System) application, implementing Domain-Driven Design (DDD) principles. This layer contains the business entities, value objects, domain services, and business logic that represent the real-world concepts and rules governing aviation safety management operations.

## Project Information

- **Target Framework**: .NET 8.0
- **Architecture Pattern**: Domain-Driven Design (DDD)
- **Design Patterns**: Entity, Value Object, Aggregate Root, Repository Interface
- **Dependencies**: Shared project, BCrypt.Net-Next for password hashing

## Architecture Overview

### Core Components

```
Domain Layer Structure:
??? Common/           # Base classes and shared domain infrastructure
??? Entities/         # Domain entities and aggregate roots
??? ValueObjects/     # Immutable value objects with business logic
??? Enums/           # Domain-specific enumerations
??? Interfaces/       # Domain service and repository contracts
??? Errors/          # Comprehensive domain error definitions
??? Models/          # Domain models for complex data structures
??? Exceptions/      # Custom domain exceptions
??? Documents/       # Project documentation
```

## Key Features

### 1. Rich Domain Model
- **Entities**: Business objects with identity and lifecycle
- **Value Objects**: Immutable objects representing domain concepts
- **Aggregate Roots**: Consistency boundaries for related entities
- **Domain Services**: Complex business logic coordination

### 2. Type-Safe Design
- **Strongly-Typed IDs**: Preventing primitive obsession
- **Value Object Validation**: Business rules enforced at creation
- **Enum Safety**: Domain-specific enumerations with validation
- **Result Pattern**: Explicit error handling without exceptions

### 3. Business Domain Coverage

#### User Management Domain
- **BaseUser**: Abstract base for all user types with authentication
- **SMSApplicationUser**: Administrative system users
- **SMSOrganizationalUser**: Internal organization personnel
- **SMSStakeholderUser**: External stakeholders and partners
- **User Groups**: Application, Organizational, and Stakeholder groupings
- **User Roles**: Comprehensive role-based access control

#### Safety Management Domain
- **Hazard**: Core safety hazard entities with lifecycle management
- **Risk Assessment**: Systematic risk evaluation and scoring
- **Risk Analysis**: Detailed risk analysis with root cause identification
- **Investigation**: Formal safety investigations with workflow
- **Interview**: Investigation interview management and tracking
- **Mitigation**: Risk mitigation strategies and implementation tracking
- **Report**: Safety reporting with validation and processing

#### Audit and Compliance Domain
- **SMS Audit Plan**: Audit planning and scheduling
- **SMS Audit**: Audit execution and management
- **SMS Audit Finding**: Non-conformance identification and tracking
- **SMS Audit Evidence**: Evidence collection and management
- **SMS Audit Checklist**: Structured audit checklist management
- **Safety Performance Indicators (SPI)**: Performance measurement and tracking

#### Data and Configuration Domain
- **Airport Shared Dataset**: Shared aviation data management
- **Hazard Files**: Document and attachment management
- **Scoring Panel**: Risk scoring committee management
- **Hazard Location**: Geographic hazard tracking
- **Report Validation**: Multi-stage validation workflows

## Project Structure Details

### Base Classes (`/Common`)

#### Core Infrastructure
- **BaseEntity**: Identity and equality semantics for all entities
- **BaseAuditableEntity**: Audit trail support with creation/modification tracking
- **BaseValueObject**: Immutable value objects with structural equality
- **BaseAggregateRoot**: Aggregate root pattern for consistency boundaries
- **BaseID<T>**: Strongly-typed identifier base class
- **BaseResult**: Result pattern for explicit error handling

#### Domain Foundation
- **Error**: Structured error representation with codes and messages
- **BaseDomainEvent**: Domain event base for event-driven architecture
- **BaseEnum**: Enhanced enumeration with business logic support

### Entity Layer (`/Entities`)

#### User Management Entities
- **BaseUser**: Abstract user with authentication and session management
- **SMSApplicationUser**: System administrators with full access
- **SMSOrganizationalUser**: Department-based internal users
- **SMSStakeholderUser**: External partners with limited access
- **SMSUserRole**: Role definition with module-based permissions
- **SMSUserRolePermission**: Granular permission management (CRUD operations)

#### Safety Management Entities
- **Hazard**: Central hazard entity with status, category, and lifecycle
- **RiskAssessment**: Comprehensive risk evaluation with scoring panels
- **RiskAnalysis**: Detailed analysis with worst-case scenarios and root causes
- **Investigation**: Formal investigation process with assignments and decisions
- **Interview**: Investigation interviews with scheduling and documentation
- **Mitigation**: Risk mitigation with progress tracking and effectiveness measurement
- **MitigationAssignment**: Assignment and responsibility tracking
- **Report**: Safety reports with validation workflows

#### Audit Entities
- **SMSAuditPlan**: Audit planning with scope and resource allocation
- **SMSAudit**: Audit execution with phases and team management
- **SMSAuditFinding**: Non-conformance identification with severity and corrective actions
- **SMSAuditEvidence**: Evidence collection with retention and confidentiality
- **SMSAuditChecklistItem**: Structured audit items with completion tracking
- **SafetyPerformanceIndicator**: KPI definition and measurement

#### Supporting Entities
- **AirportSharedDataset**: Shared aviation operational data
- **HazardFile**: Document attachments with metadata and security
- **HazardLocation**: Geographic coordinate tracking
- **ScoringPanel**: Risk assessment committee management
- **ReportValidation**: Multi-level report validation workflow
- **HazardReportTracking**: Report processing and status tracking

### Value Objects (`/ValueObjects`)

#### Identity and Authentication
- **UserName**: Email validation with business rules
- **Password**: BCrypt hashing with complexity requirements
- **FirstName/LastName**: Name validation with character restrictions

#### Business Logic Objects
- **ApplicationPermissions**: Module-based permission matrices
- **WorkflowPermissions**: Workflow-specific authorization rules
- **StakeholderPermissions**: External user access limitations
- **UserRoleAssignment**: Role assignment with expiration and validation
- **RiskAssessmentValueObjects**: Risk scoring and assessment data structures
- **URL**: Validated URL handling with security checks

### Enumerations (`/Enums`)

#### User and Role Management
- **SMSUserType**: Application, Organizational, Stakeholder
- **SMSDepartment**: Organizational department classifications
- **SMSOrganizationalLevel**: Management hierarchy levels
- **SMSStakeholderType**: External stakeholder categorization

#### Safety Management
- **HazardCategory**: Safety hazard classifications
- **HazardType**: Incident, Near-miss, Observation, etc.
- **HazardStatus**: Draft, Reported, Under Investigation, etc.
- **RiskLevel**: Low, Medium, High, Critical
- **RiskAssessmentStatus**: Planned, In Progress, Completed, Reviewed
- **RiskAssessmentStage**: Initial, Detailed, Residual, Final

#### Investigation and Analysis
- **InvestigationStatus**: Assigned, In Progress, Completed, Closed
- **InterviewStatus**: Scheduled, Conducted, Cancelled, Rescheduled
- **InterviewType**: Initial, Follow-up, Expert, Witness
- **MitigationStatus**: Proposed, Approved, Implemented, Validated

#### Reporting and Validation
- **ReportStatus**: Draft, Submitted, Validated, Published
- **ReportValidationStatus**: Pending, In Review, Approved, Rejected
- **ValidationDecision**: Approve, Reject, Request Information, Defer

#### System Configuration
- **ScoringPanelType**: Technical, Operational, Management
- **HazardFileCategory**: Evidence, Procedure, Photo, Document
- **HazardFileStorageType**: Database, FileSystem, Cloud
- **SPIEnums**: Various SPI-related classifications

### Error Management (`/Errors`)

#### Comprehensive Error Catalog
The domain provides over 300 specific error types organized by business area:

- **User Management Errors**: Authentication, authorization, profile management
- **Safety Management Errors**: Hazard reporting, risk assessment, investigation
- **Audit Errors**: Planning, execution, findings, evidence
- **Validation Errors**: Data integrity, business rule violations
- **Workflow Errors**: Process violations, approval requirements
- **System Errors**: Infrastructure, configuration, integration

#### Error Design Pattern
```csharp
public static Error UserNotFound => new Error("BaseUser.UserNotFound", "The user was not found.");
```

All errors follow consistent naming conventions and provide actionable messages for both developers and end users.

### Domain Interfaces (`/Interfaces`)

#### Repository Contracts
- **IBaseUserRepository**: Generic user data access patterns
- **ISMSOrganizationalGroupRepository**: Group management operations

#### Domain Services
- **ISMSRoleService**: Role assignment and validation logic
- **ISMSAuthorizationService**: Permission checking and enforcement

#### Entity Contracts
- **IReport**: Report entity contract with validation
- **IHazard**: Hazard entity behavior definition
- **IInvestigation**: Investigation workflow contract
- **IRiskAssessment**: Risk assessment process definition
- **IMitigation**: Mitigation implementation contract

### Domain Models (`/Models`)

#### Statistical and Reporting Models
- **UserRoleStatistics**: Role assignment and usage metrics
- **HazardFileStatistics**: File attachment analytics
- **SMSAuditFindingStatistics**: Audit finding trends and patterns
- **SMSAuditEvidenceStatistics**: Evidence collection metrics
- **SMSAuditCalendarData**: Audit scheduling and timeline data
- **SMSAuditExecutionDashboard**: Real-time audit progress tracking

## Technical Implementation

### Value Object Pattern
All value objects implement structural equality and immutability:
```csharp
public sealed class Password : BaseValueObject
{
    public string HashedValue { get; }
    public DateTime CreatedDate { get; }
    public bool RequiresChange { get; }

    public bool Verify(string plainTextPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainTextPassword, HashedValue);
    }
}
```

### Entity Identity Pattern
Strongly-typed identifiers prevent primitive obsession:
```csharp
public class HazardID : BaseID<string>
{
    public HazardID(string value) : base(value) { }
}

public class Hazard : BaseAuditableEntity
{
    public HazardID Id { get; private set; }
    // ... other properties
}
```

### Result Pattern Implementation
Explicit error handling without exceptions:
```csharp
public static Result<Password> Create(string plainTextPassword, bool requiresChange = false) =>
    Result.Create(plainTextPassword, DomainErrors.PasswordError.NullOrEmpty)
        .Ensure(p => !string.IsNullOrWhiteSpace(p), DomainErrors.PasswordError.NullOrEmpty)
        .Ensure(p => p.Length >= MinLength, DomainErrors.PasswordError.TooShort)
        .Map(p => new Password(HashPassword(p), DateTime.UtcNow, requiresChange));
```

### Business Rule Enforcement
Domain entities enforce business rules:
```csharp
public bool Authenticate(string plainTextPassword)
{
    if (!IsActive)
        return false;

    return Password.Verify(plainTextPassword);
}
```

## Security Features

### Authentication and Authorization
- **BCrypt Password Hashing**: Industry-standard password security
- **Role-Based Access Control**: Granular permission management
- **Session Management**: Login tracking and stale account detection
- **Password Policy**: Complexity requirements and expiration

### Data Protection
- **Audit Trails**: Complete change tracking for all entities
- **Confidentiality Levels**: Classified data handling
- **Retention Policies**: Evidence and document lifecycle management
- **Access Logging**: Comprehensive access audit trails

## Integration Points

### Application Layer Integration
- **Rich Domain Models**: Complex business logic encapsulation
- **Domain Services**: Coordination of multi-entity operations
- **Domain Events**: Event-driven architecture support
- **Repository Interfaces**: Data access abstraction

### Infrastructure Layer Integration
- **Entity Mapping**: ORM configuration and database schema
- **Value Object Conversion**: Database type conversion
- **Identity Generation**: Primary key and business key strategies
- **Migration Support**: Schema evolution and data migration

## Quality Assurance

### Design Patterns
- **Domain-Driven Design**: Business logic encapsulation
- **Value Object Pattern**: Immutable data with behavior
- **Aggregate Root Pattern**: Consistency boundaries
- **Repository Pattern**: Data access abstraction
- **Result Pattern**: Explicit error handling

### Validation Strategy
- **Creation Validation**: Business rules enforced at object creation
- **State Transitions**: Valid state change enforcement
- **Cross-Entity Validation**: Aggregate boundary integrity
- **Business Rule Enforcement**: Domain invariants protection

### Testing Support
- **Deterministic Behavior**: Predictable entity operations
- **Isolated Testing**: No external dependencies
- **Validation Testing**: Business rule verification
- **Error Scenario Testing**: Complete error coverage

## Business Rules Examples

### User Management Rules
- Users must have unique usernames within their type
- Passwords must meet complexity requirements
- Inactive users cannot authenticate
- User roles determine module access permissions

### Safety Management Rules
- Hazards must be assigned to investigations within 48 hours
- High-risk assessments require management approval
- Mitigations must have assigned responsible parties
- Critical findings require immediate escalation

### Audit Rules
- Audit plans must be approved before scheduling
- Audit findings require evidence for validation
- Corrective actions must have target completion dates
- Evidence must be retained per regulatory requirements

## Future Enhancements

### Planned Features
- **Domain Events**: Full event-driven architecture implementation
- **Aggregate Optimization**: Performance tuning for large aggregates
- **Multi-Tenancy**: Support for multiple organizational units
- **Advanced Workflows**: Configurable business process flows

### Technical Improvements
- **Specification Pattern**: Complex query composition
- **Policy Pattern**: Configurable business rules
- **State Machine**: Formal state transition management
- **Event Sourcing**: Complete audit trail implementation

## Dependencies

### Core Dependencies
- **.NET 8.0**: Latest framework features and performance
- **BCrypt.Net-Next**: Secure password hashing
- **Microsoft.Extensions.Logging**: Structured logging support
- **Microsoft.Extensions.Configuration**: Configuration binding

### Design Dependencies
- **Shared Project**: Common utilities and base classes
- **Clean Architecture**: Layer separation and dependency inversion
- **SOLID Principles**: Maintainable and extensible design

## Conclusion

The Domain layer provides a robust, secure foundation for the SMS system with comprehensive business logic implementation, strong type safety, and extensive error handling. The rich domain model accurately represents aviation safety management concepts while providing the flexibility to evolve with changing business requirements and regulatory compliance needs.

---

**Document Version**: 1.0  
**Last Updated**: January 2025  
**Author**: SMS Development Team  
**Review Date**: June 2025