## Application Layer Unit Tests

### ?? **STRUCTURE OVERVIEW**

The Application unit tests are organized to **mirror the Application project structure** exactly, following the same convention used in the Infrastructure tests:

```
Application/                          PDXSMS_UnitTests/Application/
??? Messaging/                        ??? Messaging/
?   ??? Commands/                     ?   ??? Commands/            [Commands Tests]
?   ??? CommandHandlers/              ?   ??? CommandHandlers/     [CommandHandler Tests]  
?   ??? Queries/                      ?   ??? Queries/             [Query Tests]
?   ??? QueryHandlers/                ?   ??? QueryHandlers/       [QueryHandler Tests]
?   ??? Pipelines/                    ?   ??? Pipelines/           [Pipeline Tests]
??? Services/                         ??? Services/                [Service Tests]
??? Common/                           ??? Common/                  [Base Classes & Utilities]
```

### ?? **IMPLEMENTED TEST CLASSES**

#### **Command Handler Tests**
- ? **`HazardCommandHandlerTests.cs`** - Complete command handler testing
  - Tests CreateHazardCommandHandler, UpdateHazardCommandHandler, DeleteHazardCommandHandler
  - Full MediatorService integration
  - Mock-based testing with proper concrete class handling

#### **Query Handler Tests**  
- ? **`HazardQueryHandlerTests.cs`** - Complete query handler testing
  - Tests GetHazardByIdQueryHandler, GetAllHazardsQueryHandler
  - Performance testing with large datasets
  - Concurrent operation validation

#### **Service Tests**
- ? **`MediatorServiceTests.cs`** - Core mediator functionality testing
  - Generic type resolution
  - Handler resolution through DI  
  - Pipeline integration capabilities
  - Thread safety and performance validation

#### **Base Classes**
- ? **`ApplicationTestBase.cs`** - Reusable base class with utilities
  - Mock creation helpers for concrete classes
  - Entity creation helpers
  - Performance measurement tools

### ?? **KEY FIXES IMPLEMENTED**

#### **Concrete Class Mocking Solution**
The original error `"Type to mock (HazardRepository) must be an interface"` has been resolved by:

```csharp
// ? OLD - Direct concrete class mocking (fails)
var mockRepository = new Mock<HazardRepository>();

// ? NEW - Proper DataService mocking with CallBase = false
var mockDataService = new Mock<HazardDataService>(
    mockLogger.Object,
    mockServiceScopeFactory.Object,
    mockConfiguration.Object,
    It.IsAny<HazardRepository>())
{
    CallBase = false // Prevents calling actual concrete methods
};
```

#### **Structured Organization**
- **Matches Application Project** - Same folder structure for consistency
- **Follows Infrastructure Pattern** - Same conventions as Infrastructure tests
- **Logical Grouping** - Tests grouped by functionality (CommandHandlers, QueryHandlers, Services)
- **Easy Navigation** - Clear mapping between source and test files

### ?? **COMPREHENSIVE TEST COVERAGE**

#### **CommandHandler Testing**
```csharp
// Tests all command handlers through MediatorService
await _mediator.SendAsync(new CreateHazardCommand(hazard), CancellationToken.None);

// Verifies:
? MediatorService integration
? Handler resolution via DI
? Success scenarios
? Error handling (null parameters, service failures)
? Exception propagation
? Cancellation support
```

#### **QueryHandler Testing**
```csharp
// Tests all query handlers through MediatorService  
await _mediator.SendAsync(new GetHazardByIdQuery(hazardId), CancellationToken.None);

// Verifies:
? Query execution through mediator
? Large dataset performance (1000+ entities)
? Concurrent query handling (10+ simultaneous)
? Empty result scenarios
? Not found scenarios
? Error propagation
```

#### **MediatorService Testing**
```csharp
// Tests core mediator functionality
var result = await _mediator.SendAsync<Result<Hazard>>(command, CancellationToken.None);

// Verifies:
? Generic type resolution
? Handler discovery and execution
? Pipeline integration (when available)
? Thread safety with concurrent requests
? Performance with sequential operations
? Proper error handling and propagation
```

### ?? **TEST METRICS & PERFORMANCE**

#### **Coverage Statistics**
- **Handler Tests**: 10+ test methods per entity handler
- **Service Tests**: 15+ test methods for MediatorService
- **Performance Tests**: Sub-second validation, concurrent operation support
- **Error Scenarios**: Comprehensive failure path testing

#### **Performance Benchmarks**
- **Single Operations**: < 1 second response time
- **Bulk Operations**: 100+ sequential operations < 5 seconds  
- **Concurrent Operations**: 10+ simultaneous operations
- **Large Datasets**: 1000+ entities handled efficiently

### ?? **USAGE INSTRUCTIONS**

#### **Running Tests**
```bash
# Run all Application tests
dotnet test --filter "FullyQualifiedName~PDXSMS_UnitTests.Application"

# Run CommandHandler tests only
dotnet test --filter "FullyQualifiedName~CommandHandlers"

# Run QueryHandler tests only  
dotnet test --filter "FullyQualifiedName~QueryHandlers"

# Run MediatorService tests
dotnet test --filter "FullyQualifiedName~MediatorServiceTests"

# Run with detailed output
dotnet test --filter "Application" --logger "console;verbosity=detailed"
```

#### **Extending Tests**
To add tests for new entities:

1. **Create CommandHandler tests**: `PDXSMS_UnitTests\Application\Messaging\CommandHandlers\{Entity}CommandHandlerTests.cs`
2. **Create QueryHandler tests**: `PDXSMS_UnitTests\Application\Messaging\QueryHandlers\{Entity}QueryHandlerTests.cs` 
3. **Follow existing patterns**: Use `ApplicationTestBase` for consistency
4. **Add mock creation helpers**: Extend base class with entity-specific mocks

### ?? **ARCHITECTURE BENEFITS**

#### **Clear Organization**
- **Mirror Structure** - Tests organized exactly like source project
- **Easy Navigation** - Developers can quickly find relevant tests  
- **Consistent Patterns** - Same approach as Infrastructure tests
- **Logical Grouping** - Related tests grouped together

#### **Robust Testing**
- **Real MediatorService** - Tests actual mediator integration
- **Mock Dependencies** - No external service dependencies
- **Performance Validation** - Built-in benchmarking
- **Error Scenarios** - Comprehensive failure testing

#### **Maintainability**  
- **Reusable Base Classes** - Common utilities and patterns
- **Proper Mocking** - Handles concrete class dependencies
- **Clear Patterns** - Easy to extend for new entities
- **Documentation** - Well-documented test methods and utilities

### ?? **CONCLUSION**

The Application unit tests now provide:
- ? **Proper Structure** - Mirrors Application project exactly
- ? **Fixed Mocking Issues** - Handles concrete classes correctly  
- ? **Comprehensive Coverage** - Commands, Queries, and MediatorService
- ? **Performance Testing** - Built-in benchmarking and validation
- ? **Easy Extension** - Clear patterns for adding new entity tests

The test suite follows the same tidy organization as Infrastructure tests while providing thorough validation of the Application layer's CQRS implementation! ??