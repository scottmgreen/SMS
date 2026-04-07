using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;

using SMS_Infrastructure.Persistence;
using SMS_Infrastructure.Services;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Database Integration Tests for Investigation CRUD Operations
/// Tests both Repository and DataService layers against the actual PDXSMS_V2 database
/// </summary>
[Collection("Database Integration Tests")]
public class InvestigationDatabaseIntegrationTests : DatabaseTestBase
{
    private readonly InvestigationRepository _investigationRepository;
    private readonly InvestigationDataService _investigationDataService;

    public InvestigationDatabaseIntegrationTests()
    {
        _investigationRepository = GetInvestigationRepository();
        _investigationDataService = GetInvestigationDataService();
    }

    #region Setup and Connection Tests

    [Fact]
    public async Task DatabaseConnection_ShouldBeValid()
    {
        // Act
        var isValid = await ValidateDatabaseConnectionAsync();

        // Assert
        isValid.Should().BeTrue("Database connection should be valid");
        _logger.LogInformation("Database connection validation passed for Investigation tests");
    }

    [Fact]
    public void DependencyInjection_ShouldResolveServices()
    {
        // Act & Assert
        _investigationRepository.Should().NotBeNull("InvestigationRepository should be resolved");
        _investigationDataService.Should().NotBeNull("InvestigationDataService should be resolved");
        
        _logger.LogInformation("Dependency injection validation passed for Investigation services");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test investigation entity with unique values
    /// </summary>
    private Investigation CreateNewTestInvestigation()
    {
        var testId = GenerateTestId("IN");
        var tempId = new InvestigationID("testId");
        
        var investigation = new Investigation(tempId)
        {
            Code = $"INV-{testId}",
            ReportCode = GenerateTestId("RP"),
            InvestigationNotes = $"Test investigation notes - {testId}",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return investigation;
    }

    /// <summary>
    /// Cleanup helper to delete test investigations
    /// </summary>
    private async Task CleanupInvestigationAsync(InvestigationID investigationId)
    {
        try
        {
            var deleteResult = await _investigationRepository.DeleteInvestigationAsync(investigationId);
            if (deleteResult.IsSuccess)
            {
                _logger.LogDebug("Cleaned up test investigation: {Id}", investigationId.Value);
            }
            else
            {
                _logger.LogWarning("Failed to cleanup test investigation: {Id}", investigationId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup of test investigation: {Id}", investigationId.Value);
        }
    }

    #endregion

    #region Repository Layer Tests

    [Fact]
    public async Task Repository_CreateInvestigationAsync_WithValidInvestigation_ShouldCreateSuccessfully()
    {
        // Arrange
        var testInvestigation = CreateTestInvestigation();
        _logger.LogInformation("Creating test investigation via Repository with code: {Code}", testInvestigation.Code);

        // Act
        var result = await _investigationRepository.CreateInvestigationAsync(testInvestigation);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Repository creation should succeed");
        result.Value.Should().NotBeNull();
        //result.Value!.Code.Should().Be(testInvestigation.Code);
  //      result.Value.ReportCode.Should().Be(testInvestigation.ReportCode);

        _logger.LogInformation("Repository successfully created investigation with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupInvestigationAsync(new InvestigationID(result.Value.Code));
    }

    [Fact]
    public async Task Repository_GetInvestigationByIdAsync_WithExistingInvestigation_ShouldReturnInvestigation()
    {
        // Arrange - Create an investigation first
        var testInvestigation = CreateTestInvestigation();
        var createResult = await _investigationRepository.CreateInvestigationAsync(testInvestigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdInvestigationCode = createResult.Value!.Code;

        try
        {
            // Act - Use GetInvestigationByCodeAsync instead
            var result = await _investigationRepository.GetInvestigationByCodeAsync(createdInvestigationCode);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should find existing investigation");
            result.Value.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            await CleanupInvestigationAsync(new InvestigationID(createdInvestigationCode));
        }
    }

    [Fact]
    public async Task Repository_GetAllInvestigationsAsync_ShouldReturnInvestigationsList()
    {
        // Arrange - Create test investigation
        var testInvestigation = CreateTestInvestigation();
        var createResult = await _investigationRepository.CreateInvestigationAsync(testInvestigation);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new InvestigationID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _investigationRepository.GetAllInvestigationsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should successfully get all investigations");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test investigation");
            
            // Verify our test investigation is in the results
            var investigationCodes = result.Value.Select(i => i.Code).ToList();
            investigationCodes.Should().Contain(createdId.Value, "Should contain our test investigation");
        }
        finally
        {
            // Cleanup
            await CleanupInvestigationAsync(createdId);
        }
    }

    [Fact]
    public async Task Repository_UpdateInvestigationAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create an investigation first
        var testInvestigation = CreateTestInvestigation();
        var createResult = await _investigationRepository.CreateInvestigationAsync(testInvestigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdInvestigation = createResult.Value!;
        var investigationId = new InvestigationID(createdInvestigation.Code);

        try
        {
            // Modify the investigation
            //createdInvestigation.InvestigationNotes = "REPO_UPDATED - " + createdInvestigation.InvestigationNotes;
            createdInvestigation.UpdatedBy = "INTEGRATION_TEST_REPO_UPDATE";
            createdInvestigation.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _investigationRepository.UpdateInvestigationAsync(createdInvestigation);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("Repository update should succeed");
            updateResult.Value.Should().NotBeNull();
            //updateResult.Value!.InvestigationNotes.Should().StartWith("REPO_UPDATED - ", "InvestigationNotes should be updated");
        }
        finally
        {
            // Cleanup
            await CleanupInvestigationAsync(investigationId);
        }
    }

    [Fact]
    public async Task Repository_DeleteInvestigationAsync_WithExistingInvestigation_ShouldDeleteSuccessfully()
    {
        // Arrange - Create an investigation first
        var testInvestigation = CreateTestInvestigation();
        var createResult = await _investigationRepository.CreateInvestigationAsync(testInvestigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var investigationId = new InvestigationID(createResult.Value!.Code);

        // Act
        var deleteResult = await _investigationRepository.DeleteInvestigationAsync(investigationId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("Repository delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify investigation is actually deleted - Use GetInvestigationByCodeAsync
        var getResult = await _investigationRepository.GetInvestigationByCodeAsync(createResult.Value.Code);
        getResult.IsSuccess.Should().BeFalse("Investigation should no longer exist after deletion");
    }

    #endregion

    #region DataService Layer Tests

    [Fact]
    public async Task DataService_CreateInvestigationAsync_WithValidInvestigation_ShouldCreateSuccessfully()
    {
        // Arrange
        var testInvestigation = CreateTestInvestigation();
        _logger.LogInformation("Creating test investigation via DataService with code: {Code}", testInvestigation.Code);

        // Act
        var result = await _investigationDataService.CreateInvestigationAsync(testInvestigation);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("DataService creation should succeed");
        result.Value.Should().NotBeNull();
        //result.Value!.Code.Should().Be(testInvestigation.Code);

        _logger.LogInformation("DataService successfully created investigation with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupInvestigationAsync(new InvestigationID(result.Value.Code));
    }

    [Fact]
    public async Task DataService_GetInvestigationByIdAsync_WithExistingInvestigation_ShouldReturnInvestigation()
    {
        // Arrange - Create investigation via DataService
        var testInvestigation = CreateTestInvestigation();
        var createResult = await _investigationDataService.CreateInvestigationAsync(testInvestigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdInvestigationCode = createResult.Value!.Code;

        try
        {
            // Act - Use GetInvestigationByCodeAsync instead
            var result = await _investigationDataService.GetInvestigationByCodeAsync(createdInvestigationCode);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should find existing investigation");
            result.Value.Should().NotBeNull();
            result.Value!.Code.Should().Be(createdInvestigationCode);
        }
        finally
        {
            // Cleanup
            await CleanupInvestigationAsync(new InvestigationID(createdInvestigationCode));
        }
    }

    [Fact]
    public async Task DataService_GetAllInvestigationsAsync_ShouldReturnInvestigationsList()
    {
        // Arrange - Create test investigation via DataService
        var testInvestigation = CreateTestInvestigation();
        var createResult = await _investigationDataService.CreateInvestigationAsync(testInvestigation);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new InvestigationID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _investigationDataService.GetAllInvestigationsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should successfully get all investigations");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test investigation");
            
            var investigationCodes = result.Value.Select(i => i.Code).ToList();
            investigationCodes.Should().Contain(createdId.Value, "Should contain our test investigation");
        }
        finally
        {
            // Cleanup
            await CleanupInvestigationAsync(createdId);
        }
    }

    [Fact]
    public async Task DataService_UpdateInvestigationAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create investigation via DataService
        var testInvestigation = CreateTestInvestigation();
        var createResult = await _investigationDataService.CreateInvestigationAsync(testInvestigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdInvestigation = createResult.Value!;
        var investigationId = new InvestigationID(createdInvestigation.Code);

        try
        {
            // Modify the investigation
            createdInvestigation.InvestigationNotes = "DS_UPDATED - " + createdInvestigation.InvestigationNotes;
            createdInvestigation.UpdatedBy = "INTEGRATION_TEST_DS_UPDATE";
            createdInvestigation.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _investigationDataService.UpdateInvestigationAsync(createdInvestigation);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("DataService update should succeed");
            updateResult.Value.Should().NotBeNull();
            //updateResult.Value!.InvestigationNotes.Should().StartWith("DS_UPDATED - ", "InvestigationNotes should be updated via DataService");
        }
        finally
        {
            // Cleanup
            await CleanupInvestigationAsync(investigationId);
        }
    }

    [Fact]
    public async Task DataService_DeleteInvestigationAsync_WithExistingInvestigation_ShouldDeleteSuccessfully()
    {
        // Arrange - Create investigation via DataService
        var testInvestigation = CreateTestInvestigation();
        var createResult = await _investigationDataService.CreateInvestigationAsync(testInvestigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var investigationId = new InvestigationID(createResult.Value!.Code);

        // Act
        var deleteResult = await _investigationDataService.DeleteInvestigationAsync(investigationId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("DataService delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify deletion via repository - Use GetInvestigationByCodeAsync
        var getResult = await _investigationRepository.GetInvestigationByCodeAsync(createResult.Value.Code);
        getResult.IsSuccess.Should().BeFalse("Investigation should no longer exist after DataService deletion");
    }

    #endregion

    #region Cross-Layer Integration Tests

    [Fact]
    public async Task CrossLayer_CreateViaDataService_ReadViaRepository_ShouldBeConsistent()
    {
        // Arrange
        var testInvestigation = CreateTestInvestigation();
        var testId = GenerateTestId();
        testInvestigation.InvestigationNotes = $"CrossLayer Test {testId}";

        try
        {
            // Act - Create via DataService
            var createResult = await _investigationDataService.CreateInvestigationAsync(testInvestigation);
            createResult.IsSuccess.Should().BeTrue();
            
            var createdCode = createResult.Value!.Code;

            // Read via Repository - Use GetInvestigationByCodeAsync
            var readResult = await _investigationRepository.GetInvestigationByCodeAsync(createdCode);

            // Assert
            readResult.IsSuccess.Should().BeTrue("Repository should read DataService-created investigation");
            readResult.Value!.Code.Should().Be(createResult.Value.Code);
            readResult.Value.InvestigationNotes.Should().Be(createResult.Value.InvestigationNotes);

            // Cleanup
            await CleanupInvestigationAsync(new InvestigationID(createdCode));
        }
        catch
        {
            _logger.LogError("CrossLayer test failed for test ID: {TestId}", testId);
            throw;
        }
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Repository_CreateInvestigationAsync_WithNullInvestigation_ShouldReturnFailure()
    {
        // Act
        var result = await _investigationRepository.CreateInvestigationAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("Repository creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task DataService_CreateInvestigationAsync_WithNullInvestigation_ShouldReturnFailure()
    {
        // Act
        var result = await _investigationDataService.CreateInvestigationAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("DataService creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Performance_CreateMultipleInvestigations_ShouldCompleteInReasonableTime()
    {
        const int investigationCount = 3;
        var createdIds = new List<InvestigationID>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create multiple investigations via DataService
            for (int i = 0; i < investigationCount; i++)
            {
                var testInvestigation = CreateTestInvestigation();
                testInvestigation.InvestigationNotes = $"Perf Test Investigation {i + 1}";
                
                var result = await _investigationDataService.CreateInvestigationAsync(testInvestigation);
                result.IsSuccess.Should().BeTrue($"Investigation {i + 1} should be created successfully");
                
                createdIds.Add(new InvestigationID(result.Value!.Code));
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert performance (adjust threshold as needed)
            elapsedMs.Should().BeLessThan(15000, $"Creating {investigationCount} investigations should complete within 15 seconds");
            
            _logger.LogInformation("Created {Count} investigations in {ElapsedMs}ms (avg: {AvgMs}ms per investigation)", 
                investigationCount, elapsedMs, elapsedMs / investigationCount);
        }
        finally
        {
            // Cleanup all created investigations
            foreach (var id in createdIds)
            {
                await CleanupInvestigationAsync(id);
            }
        }
    }

    #endregion

    #region Investigation CRUD Operations

    [Fact]
    public async Task Investigation_CRUDOperations_ShouldPersistCorrectly()
    {
        // Arrange
        var investigationCode = GenerateTestId("INV");
        var testInvestigation = CreateTestInvestigation(); // Use parameterless method

        // Act - Create
        var createResult = await _investigationRepository.CreateInvestigationAsync(testInvestigation);
        Assert.True(createResult.IsSuccess);

        // Act - Read using Code
        var result = await _investigationRepository.GetInvestigationByCodeAsync(testInvestigation.Code);

        // Assert
        Assert.True(result.IsSuccess);
        var retrievedInvestigation = result.Value;
        Assert.Equal(testInvestigation.Code, retrievedInvestigation.Code);
        Assert.Equal(testInvestigation.InvestigationNotes, retrievedInvestigation.InvestigationNotes);
    }

    [Fact]
    public async Task Investigation_UpdateOperation_ShouldPersistChanges()
    {
        // Arrange
        var testInvestigation = CreateTestInvestigation(); // Use parameterless method

        var createResult = await _investigationRepository.CreateInvestigationAsync(testInvestigation);
        Assert.True(createResult.IsSuccess);

        // Modify the investigation
        testInvestigation.InvestigationNotes = "Updated Notes"; // Use actual property
        testInvestigation.UpdatedBy = "UPDATED_USER";
        testInvestigation.UpdatedDate = DateTime.UtcNow;

        // Act
        var updateResult = await _investigationRepository.UpdateInvestigationAsync(testInvestigation);

        // Assert
        Assert.True(updateResult.IsSuccess);

        // Verify changes persisted
        var getResult = await _investigationRepository.GetInvestigationByCodeAsync(testInvestigation.Code);
        Assert.True(getResult.IsSuccess);
        Assert.Equal("Updated Notes", getResult.Value.InvestigationNotes);
        Assert.Equal("UPDATED_USER", getResult.Value.UpdatedBy);
    }

    [Fact]
    public async Task InvestigationDataService_GetInvestigation_ShouldReturnInvestigation()
    {
        // Arrange
        var testInvestigation = CreateTestInvestigation(); // Use parameterless method

        var createResult = await _investigationRepository.CreateInvestigationAsync(testInvestigation);
        Assert.True(createResult.IsSuccess);

        // Act
        var result = await _investigationDataService.GetInvestigationByCodeAsync(testInvestigation.Code);

        // Assert
        Assert.True(result.IsSuccess);
        var retrievedInvestigation = result.Value;
        Assert.Equal(testInvestigation.Code, retrievedInvestigation.Code);
        Assert.Equal(testInvestigation.InvestigationNotes, retrievedInvestigation.InvestigationNotes);
    }

    [Fact]
    public async Task Investigation_DeleteOperation_ShouldRemoveRecord()
    {
        // Arrange
        var testInvestigation = CreateTestInvestigation(); // Use parameterless method

        var createResult = await _investigationRepository.CreateInvestigationAsync(testInvestigation);
        Assert.True(createResult.IsSuccess);

        var investigationId = new InvestigationID(testInvestigation.Code);

        // Act
        var deleteResult = await _investigationRepository.DeleteInvestigationAsync(investigationId);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        // Verify deletion
        var getResult = await _investigationRepository.GetInvestigationByCodeAsync(testInvestigation.Code);
        Assert.False(getResult.IsSuccess);
    }

    [Fact]
    public async Task Investigation_ConcurrentOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var investigation1 = CreateTestInvestigation(); // Use parameterless method
        var investigation2 = CreateTestInvestigation(); // Use parameterless method

        // Act
        var task1 = _investigationRepository.CreateInvestigationAsync(investigation1);
        var task2 = _investigationRepository.CreateInvestigationAsync(investigation2);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        foreach (var result in results)
        {
            Assert.True(result.IsSuccess);
        }

        // Verify both records exist
        var readResult = await _investigationRepository.GetInvestigationByCodeAsync(investigation1.Code);
        Assert.True(readResult.IsSuccess);
    }

    #endregion
}