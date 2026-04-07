# Application Layer Unit Tests

## Overview

This directory contains comprehensive unit tests for the SMS Application layer, covering all aspects of the CQRS (Command Query Responsibility Segregation) architecture, services, and business logic.

## Test Architecture

### Structure
```
UnitTests/Application/
??? Common/
?   ??? ApplicationTestBase.cs          # Base class with DI setup and test utilities
??? CQRS/
?   ??? CommandHandlers/                # Tests for all command handlers
?   ??? QueryHandlers/                  # Tests for all query handlers
?   ??? Pipelines/                      # Tests for pipeline functionality
??? Services/                           # Tests for application services
```

### Test Categories

#### 1. Command Handler Tests
- **Purpose**: Test all CRUD operations via command handlers
- **Entities Covered**: Hazard, Report, Investigation, Interview, Mitigation, RiskAnalysis, etc.
- **Test Types**:
  - Valid operation tests
  - Validation and error handling
  - Performance and concurrency tests
  - Edge cases and boundary conditions

#### 2. Query Handler Tests  
- **Purpose**: Test all data retrieval operations via query handlers
- **Query Types**: Single entity retrieval, collection queries, filtered queries
- **Test Types**:
  - Successful data retrieval
  - Non-existent data handling
  - Performance optimization
  - Concurrent access patterns

#### 3. Service Tests
- **Purpose**: Test application services and business logic
- **Services Covered**: MediatorService, AuthorizationService, etc.
- **Test Types**:
  - Service functionality
  - Integration with dependencies
  - Security and permission handling
  - Error handling and recovery

#### 4. Pipeline Tests
- **Purpose**: Test CQRS pipeline functionality
- **Pipeline Types**: Validation, Audit, Logging, Performance
- **Test Types**:
  - Pipeline execution order
  - Cross-cutting concerns implementation
  - Pipeline error handling
  - Performance impact analysis

## Test Patterns and Conventions

### 1. Test Base Class
All tests inherit from `ApplicationTestBase` which provides:
- **Dependency Injection Setup**: Properly configured service container
- **Mock Services**: Consistent mocking for external dependencies
- **Test Data Factories**: Methods to create test entities with realistic data
- **Utility Methods**: Performance measurement, cancellation testing, etc.

### 2. Test Entity Creation
```csharp
// Example: Creating test entities with unique, realistic data
var hazard = CreateTestHazard(); // Uses factory method
var report = CreateTestReport("RP-CUSTOM-001"); // With specific code
var entities = CreateTestEntities(5, CreateTestHazard, "HZ-BATCH"); // Bulk creation
```

### 3. Mock Service Setup
```csharp
protected override void RegisterMockedServices(IServiceCollection services)
{
    var mockService = new Mock<IHazardService>();
    mockService.Setup(x => x.CreateAsync(It.IsAny<Hazard>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Hazard h, CancellationToken _) => Result<Hazard>.Success(h));
    services.AddSingleton(mockService.Object);
}
```

### 4. Test Organization
Each test class follows this structure:
- **Setup Methods**: Service registration and mock configuration
- **Happy Path Tests**: Valid operations and expected behaviors
- **Error Handling Tests**: Invalid inputs, exceptions, and edge cases
- **Performance Tests**: Timing, concurrency, and scalability
- **Integration Tests**: End-to-end scenarios and workflows

## Key Features

### 1. Comprehensive Coverage
- ? All command handlers tested
- ? All query handlers tested
- ? Core application services tested
- ? Pipeline functionality tested
- ? Error handling and edge cases covered

### 2. Realistic Test Data
- Uses factory methods for consistent test data
- Generates unique identifiers to avoid conflicts
- Includes realistic business scenarios and workflows
- Tests both valid and invalid data scenarios

### 3. Performance Testing
- Measures operation execution time
- Tests concurrent operation handling
- Validates bulk operation performance
- Includes performance benchmarks and thresholds

### 4. Mock-Based Testing
- Isolated unit testing with mocked dependencies
- Consistent mock setup across test classes
- Predictable test data and scenarios
- Fast test execution without external dependencies

### 5. Error Handling
- Tests validation failures
- Tests exception scenarios
- Tests cancellation handling
- Tests resource not found scenarios

## Test Execution

### Running Tests
```bash
# Run all Application tests
dotnet test --filter "FullyQualifiedName~Application"

# Run specific test class
dotnet test --filter "ClassName=HazardCommandHandlerTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~Application"
```

### Test Performance
- **Target**: Each test should complete in < 100ms
- **Bulk Operations**: Should handle 10+ entities in < 2 seconds
- **Concurrent Tests**: Should handle multiple simultaneous operations
- **Memory Usage**: Should not leak memory between tests

## Best Practices

### 1. Test Naming
- Use descriptive test method names
- Follow pattern: `MethodName_Scenario_ExpectedBehavior`
- Example: `CreateHazardCommandHandler_WithValidHazard_ShouldReturnSuccess`

### 2. Test Structure
- **Arrange**: Setup test data and dependencies
- **Act**: Execute the operation being tested
- **Assert**: Verify the expected outcomes

### 3. Test Data
- Use factory methods from `ApplicationTestBase`
- Generate unique identifiers for each test
- Create realistic business scenarios
- Test with both valid and invalid data

### 4. Error Assertions
- Test both success and failure scenarios
- Verify specific error messages when applicable
- Test exception handling appropriately
- Use FluentAssertions for readable assertions

### 5. Performance Considerations
- Keep tests fast (< 100ms each)
- Use mocks to avoid external dependencies
- Measure performance for bulk operations
- Test concurrent access patterns

## Dependencies

### Testing Frameworks
- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Readable assertion library

### Application Dependencies
- **Application Project**: The code being tested
- **Domain Project**: Entity definitions and business logic
- **Infrastructure Project**: For data access interfaces
- **Shared Project**: Common utilities and extensions

## Maintenance

### Adding New Tests
1. Inherit from `ApplicationTestBase`
2. Register required services and mocks
3. Use existing factory methods for test data
4. Follow established naming conventions
5. Include performance and error handling tests

### Updating Tests
1. Update mock setups when interfaces change
2. Add new test scenarios for new features
3. Maintain performance benchmarks
4. Update documentation as needed

### Test Quality Guidelines
- Each test should have a single responsibility
- Tests should be independent and isolated
- Use meaningful assertions with clear error messages
- Include both positive and negative test scenarios
- Maintain good test coverage (aim for > 90%)

## Troubleshooting

### Common Issues
1. **DI Registration Errors**: Ensure all required services are registered in test setup
2. **Mock Setup Issues**: Verify mock configurations match actual interface usage
3. **Test Data Conflicts**: Use unique identifiers for test entities
4. **Performance Issues**: Check for leaked resources or inefficient test patterns

### Debug Tips
- Use the logger in tests to trace execution flow
- Set breakpoints in test base class setup methods
- Verify mock service registrations
- Check test execution order for dependent tests