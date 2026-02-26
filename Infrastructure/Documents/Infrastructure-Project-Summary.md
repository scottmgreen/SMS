# SMS Infrastructure Layer - Project Summary

## Overview

The **Infrastructure** project serves as the data access and external concerns layer of the SMS (Safety Management System) application, implementing the Infrastructure layer in Clean Architecture. This layer handles database operations, external integrations, security, logging, and all technical concerns that support the business logic defined in the Application and Domain layers.

## Project Information

- **Target Framework**: .NET 8.0
- **Architecture Pattern**: Clean Architecture Infrastructure Layer
- **Data Access**: Repository Pattern with Stored Procedures
- **Dependencies**: Domain, Shared projects, Microsoft.Data.SqlClient

## Architecture Overview

### Core Components

```
Infrastructure Layer Structure:
??? Common/           # Shared infrastructure utilities and base classes
??? Configuration/    # DI setup, middleware, and service registration
??? Interfaces/       # Infrastructure service contracts
??? Persistence/      # Data access repositories
??? Services/         # Data services and external integrations
??? Security/         # Authentication and authorization components
??? Documents/        # Project documentation
```

## Key Features

### 1. Comprehensive Data Access
- **Repository Pattern**: Clean separation of data access logic
- **Stored Procedure Based**: Performance-optimized database operations
- **Entity Mapping**: Domain entity to database model conversion
- **Connection Management**: Centralized database connection handling

### 2. Service Layer Architecture
- **Data Services**: Business-focused data operations
- **Repository Abstraction**: Clean interface between domain and data
- **Transaction Management**: Consistent data integrity
- **Error Handling**: Comprehensive exception management

### 3. Cross-Cutting Concerns
- **Logging Infrastructure**: Structured logging with correlation IDs
- **Configuration Management**: Centralized settings and connection strings
- **Security Services**: API key authentication and user tracking
- **Middleware Pipeline**: Request processing and user context

## Project Structure Details

### Configuration Layer (`/Configuration`)

#### Service Registration
- **DependencyInjection.cs**: Main infrastructure service registration
- **ServiceCollectionExtensions.cs**: Repository and data service registration
- **Comprehensive Registration**: 50+ repositories and data services

#### Middleware Components
- **ConnectionInfoMiddleware.cs**: Database connection tracking
- **LoggerMiddleware.cs**: Request/response logging
- **CircuitUserTrackingMiddleware.cs**: Blazor circuit user tracking
- **UserDetailsFactory.cs**: User context creation and management

### Repository Layer (`/Persistence`)

#### User Management Repositories
- **SMSApplicationUserRepository**: Administrative user data access
- **SMSOrganizationalUserRepository**: Internal user management
- **SMSStakeholderUserRepository**: External stakeholder access
- **SMSUserRoleRepository**: Role and permission management
- **User Group Repositories**: Application, Organizational, Stakeholder groups

#### Safety Management Repositories
- **HazardRepository**: Core hazard data operations
- **RiskAssessmentRepository**: Risk evaluation data management
- **RiskAnalysisRepository**: Detailed risk analysis storage
- **InvestigationRepository**: Investigation workflow data
- **InterviewRepository**: Interview management and tracking
- **MitigationRepository**: Risk mitigation data operations
- **ReportRepository**: Safety report data management

#### Audit and Compliance Repositories
- **SMSAuditPlanRepository**: Audit planning data operations
- **SMSAuditRepository**: Audit execution data management
- **SMSAuditFindingRepository**: Non-conformance tracking
- **SMSAuditEvidenceRepository**: Evidence collection and storage
- **SafetyPerformanceIndicatorRepository**: SPI data management

#### Supporting Repositories
- **AirportSharedDatasetRepository**: Shared aviation data
- **HazardFileRepository**: Document and attachment management
- **HazardLocationRepository**: Geographic hazard tracking
- **ScoringPanelRepository**: Risk assessment panel management
- **ReportValidationRepository**: Multi-stage validation workflows
- **HazardReportTrackingRepository**: Report processing tracking
- **SystemRepository**: System-wide operations and utilities

### Data Services Layer (`/Services`)

#### Core Data Services
- **ConnectionService**: Database connection management
- **FileService**: File operations and document management
- **SystemDataService**: System-wide data operations

#### User Management Data Services
- **SMSApplicationUserDataService**: Admin user business operations
- **SMSOrganizationalUserDataService**: Internal user data operations
- **SMSStakeholderUserDataService**: External user data management
- **SMSApplicationGroupDataService**: Application group operations
- **SMSOrganizationalGroupDataService**: Organizational group management
- **SMSStakeholderGroupDataService**: Stakeholder group operations
- **SMSUserRoleDataService**: Role assignment and validation

#### Safety Management Data Services
- **HazardDataService**: Hazard lifecycle data operations
- **HazardLocationDataService**: Geographic hazard data management
- **HazardFileDataService**: Document attachment operations
- **RiskAssessmentDataService**: Risk evaluation data processing
- **RiskAnalysisDataService**: Risk analysis data operations
- **InvestigationDataService**: Investigation workflow data
- **InterviewDataService**: Interview management data operations
- **MitigationDataService**: Mitigation strategy data management
- **MitigationAssignmentDataService**: Assignment tracking operations
- **ReportDataService**: Report lifecycle data management
- **ReportValidationDataService**: Validation workflow data
- **HazardReportTrackingDataService**: Report processing tracking

#### Audit Data Services
- **SMSAuditPlanDataService**: Audit planning data operations
- **SMSAuditDataService**: Audit execution data management
- **SMSAuditFindingDataService**: Finding tracking and management
- **SMSAuditEvidenceDataService**: Evidence collection operations

#### Specialized Data Services
- **AirportSharedDatasetDataService**: Shared aviation data operations
- **ScoringPanelDataService**: Assessment panel data management
- **SafetyPerformanceIndicatorDataService**: SPI calculation and tracking

### Common Components (`/Common`)

#### Base Infrastructure
- **BaseRepository<TEntity, TModel>**: Generic repository foundation
- **DataAccess**: Core database operation utilities
- **Mappers**: Entity-to-model conversion logic
- **SqlDataReaderExtensions**: Enhanced data reader functionality

#### Constants and Configuration
- **StoredProcs**: Comprehensive stored procedure catalog (200+ procedures)
- **ParameterNames**: Standardized parameter naming
- **FieldNames**: Database field name constants
- **InfrastructureEventIds**: Structured logging event identifiers
- **InfrastructureLogMessages**: Consistent log message formatting

### Security Layer (`/Security`)

#### Authentication and Authorization
- **ApiKeyAuthenticationFilter**: API key validation and authentication
- **User Context Management**: Session and identity tracking
- **Security Logging**: Authentication and authorization audit trails

### Interface Contracts (`/Interfaces`)

#### Repository Interfaces
- **IBaseRepository<TEntity, TModel>**: Generic repository contract
- **ISMSApplicationUserRepository**: Admin user repository interface
- **ISMSOrganizationalUserRepository**: Internal user repository interface
- **ISMSStakeholderUserRepository**: External user repository interface
- **IHazardRepository**: Hazard data access interface
- **IRiskAssessmentRepository**: Risk assessment data interface
- **Additional Specialized Interfaces**: 15+ domain-specific contracts

#### Service Interfaces
- **IConnectionService**: Database connection management interface
- **ILogSupport**: Infrastructure logging interface
- **IUserDetailsFactory**: User context creation interface
- **IHazardDataService**: Hazard business data operations interface
- **IInterviewDataService**: Interview data operations interface
- **IInvestigationDataService**: Investigation data operations interface
- **IReportDataService**: Report data operations interface
- **Additional Data Service Interfaces**: 10+ business operation contracts

## Technical Implementation

### Repository Pattern Implementation
```csharp
public abstract class BaseRepository<TEntity, TModel> : IBaseRepository<TEntity, TModel>
    where TEntity : class
    where TModel : class
{
    private readonly ILogger<TEntity> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly ILogSupport _logsupport;

    protected string ConnectionString => _connectionString;
    protected ILogger<TEntity> Logger => _logger;
    
    // Common database operations with logging and error handling
}
```

### Stored Procedure Integration
All data operations utilize stored procedures for:
- **Performance Optimization**: Pre-compiled execution plans
- **Security**: SQL injection prevention
- **Maintainability**: Database logic centralization
- **Audit Support**: Comprehensive operation logging

### Entity Mapping Strategy
- **Domain Entity Preservation**: No ORM dependencies in domain layer
- **Conversion Layer**: Infrastructure handles entity-to-model mapping
- **Performance Optimization**: Minimal object allocation
- **Type Safety**: Strongly-typed conversions with validation

## Database Integration

### Stored Procedure Catalog
The infrastructure manages 200+ stored procedures organized by functional area:

#### User Management (40+ procedures)
- SMS Application User CRUD operations
- SMS Organizational User management
- SMS Stakeholder User operations
- User Role assignment and validation
- User Group management and membership

#### Safety Management (80+ procedures)
- Hazard lifecycle management
- Risk assessment and analysis operations
- Investigation and interview workflows
- Mitigation strategy and assignment tracking
- Report processing and validation

#### Audit Management (50+ procedures)
- Audit plan creation and approval
- Audit execution and team management
- Finding identification and tracking
- Evidence collection and retention
- Performance indicator calculations

#### System Operations (30+ procedures)
- Audit logging and system tracking
- Code generation and validation
- Statistics and reporting operations
- Data cleanup and maintenance utilities

### Connection Management
- **Centralized Configuration**: Single connection string management
- **Connection Pooling**: Optimized resource utilization
- **Error Handling**: Comprehensive database exception management
- **Logging Integration**: Database operation audit trails

## Security Features

### Authentication Infrastructure
- **API Key Authentication**: Secure service-to-service communication
- **User Context Tracking**: Session and identity management
- **Authorization Integration**: Role-based access control support
- **Security Logging**: Comprehensive access audit trails

### Data Protection
- **Parameterized Queries**: SQL injection prevention
- **Connection Security**: Encrypted database connections
- **Audit Trails**: Complete operation logging
- **Error Sanitization**: Secure error message handling

## Performance Optimization

### Data Access Performance
- **Stored Procedure Execution**: Pre-compiled query plans
- **Connection Pooling**: Efficient resource management
- **Lazy Loading**: On-demand data retrieval
- **Bulk Operations**: Optimized multi-record processing

### Caching Strategy
- **Service Lifetime Management**: Scoped service registration
- **Memory Optimization**: Minimal object allocation
- **Resource Cleanup**: Proper disposal patterns
- **Connection Management**: Efficient database resource usage

## Configuration Management

### Service Registration Pattern
```csharp
public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
{
    // Repository registration with interface bindings
    services.AddScoped<ISMSApplicationUserRepository, SMSApplicationUserRepository>();
    
    // Data service registration
    services.AddScoped<SMSApplicationUserDataService>();
    
    // Infrastructure services
    services.AddScoped<IConnectionService, ConnectionService>();
    
    return services;
}
```

### Dependency Injection Integration
- **Clean Architecture Compliance**: Proper dependency direction
- **Interface Segregation**: Focused service contracts
- **Lifetime Management**: Appropriate service scopes
- **Configuration Binding**: Settings integration

## Integration Points

### Application Layer Integration
- **Repository Interfaces**: Clean data access abstraction
- **Data Services**: Business-focused operations
- **Entity Mapping**: Domain model preservation
- **Error Handling**: Consistent exception management

### Domain Layer Integration
- **Entity Preservation**: No infrastructure dependencies in domain
- **Value Object Support**: Proper conversion handling
- **Business Rule Validation**: Domain integrity maintenance
- **Aggregate Consistency**: Transaction boundary management

### Presentation Layer Integration
- **Service Registration**: DI container configuration
- **Middleware Pipeline**: Request processing support
- **User Context**: Authentication and authorization data
- **Logging Integration**: Request tracking and audit trails

## Quality Assurance

### Error Handling Strategy
- **Comprehensive Exception Management**: Proper error categorization
- **Logging Integration**: Structured error reporting
- **Recovery Patterns**: Graceful failure handling
- **User-Friendly Messages**: Sanitized error responses

### Testing Support
- **Interface-Based Design**: Easy mocking and testing
- **Repository Pattern**: Isolated data access testing
- **Configuration Abstraction**: Environment-independent testing
- **Logging Verification**: Audit trail testing support

### Performance Monitoring
- **Operation Logging**: Database operation tracking
- **Performance Metrics**: Query execution monitoring
- **Resource Usage**: Connection and memory tracking
- **Error Rate Monitoring**: System health indicators

## Development Patterns

### Repository Implementation Example
```csharp
public class SMSApplicationUserRepository : BaseRepository<SMSApplicationUser, SMSApplicationUser>
{
    public async Task<SMSApplicationUser> GetByCodeAsync(string code)
    {
        // Stored procedure execution with logging
        // Entity mapping and validation
        // Error handling and audit trail
    }
}
```

### Data Service Pattern
```csharp
public class SMSApplicationUserDataService
{
    private readonly SMSApplicationUserRepository _repository;
    
    public async Task<Result<List<SMSApplicationUser>>> GetAllSMSApplicationUsersAsync()
    {
        // Business logic coordination
        // Repository orchestration
        // Result pattern implementation
    }
}
```

## Future Enhancements

### Planned Features
- **Distributed Caching**: Redis integration for performance
- **Event Sourcing**: Complete audit trail implementation
- **Database Sharding**: Scalability improvements
- **Read/Write Splitting**: Performance optimization

### Technical Improvements
- **GraphQL Integration**: Flexible data access patterns
- **Message Queue Integration**: Asynchronous processing
- **Health Check Implementation**: System monitoring
- **Metrics Collection**: Performance analytics

## Dependencies

### Core Dependencies
- **Microsoft.Data.SqlClient**: SQL Server connectivity
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Logging**: Structured logging
- **Microsoft.Extensions.Http**: HTTP client integration

### Project Dependencies
- **Domain Project**: Entity and value object definitions
- **Shared Project**: Common utilities and base classes

## Conclusion

The Infrastructure layer provides a robust, scalable foundation for data access and external integrations in the SMS system. With comprehensive repository coverage, performance-optimized stored procedures, and clean architectural boundaries, it effectively supports the business logic while maintaining security, performance, and maintainability standards.

The layer's design facilitates easy testing, monitoring, and future enhancements while providing the reliability required for aviation safety management operations.

---

**Document Version**: 1.0  
**Last Updated**: January 2025  
**Author**: SMS Development Team  
**Review Date**: June 2025