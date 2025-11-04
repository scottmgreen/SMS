using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Services;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Database Integration Tests for Hazard CRUD Operations
/// Tests both Repository and DataService layers against the actual PDXSMS_V2 database
/// </summary>
[Collection("Database Integration Tests")]
public class HazardDatabaseIntegrationTests : DatabaseTestBase
{
    private readonly HazardRepository _hazardRepository;
    private readonly HazardDataService _hazardDataService;

    public HazardDatabaseIntegrationTests()
    {
        _hazardRepository = GetHazardRepository();
        _hazardDataService = GetHazardDataService();
    }

    #region Setup and Connection Tests

    [Fact]
    public async Task DatabaseConnection_ShouldBeValid()
    {
        // Act
        var isValid = await ValidateDatabaseConnectionAsync();

        // Assert
        isValid.Should().BeTrue("Database connection should be valid");
        _logger.LogInformation("Database connection validation passed");
    }

    [Fact]
    public void DependencyInjection_ShouldResolveServices()
    {
        // Act & Assert
        _hazardRepository.Should().NotBeNull("HazardRepository should be resolved");
        _hazardDataService.Should().NotBeNull("HazardDataService should be resolved");
        
        _logger.LogInformation("Dependency injection validation passed");
    }

    #endregion

    #region Repository Layer Tests

    [Fact]
    public async Task Repository_CreateHazardAsync_WithValidHazard_ShouldCreateSuccessfully()
    {
        // Arrange
        var testHazard = CreateTestHazard();
        _logger.LogInformation("Creating test hazard via Repository with name: {Name}", testHazard.Name);

        // Act
        var result = await _hazardRepository.CreateHazardAsync(testHazard);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Repository creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().StartWith("HZ-", "Generated code should have HZ prefix");
        result.Value.Name.Should().Be(testHazard.Name);
        result.Value.Description.Should().Be(testHazard.Description);

        _logger.LogInformation("Repository successfully created hazard with code: {Code}", result.Value.Code);

        // Cleanup
        await CleanupHazardAsync(new HazardID(result.Value.Code));
    }

    [Fact]
    public async Task Repository_GetHazardByIdAsync_WithExistingHazard_ShouldReturnHazard()
    {
        // Arrange - Create a hazard first
        var testHazard = CreateTestHazard();
        var createResult = await _hazardRepository.CreateHazardAsync(testHazard);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdHazardId = new HazardID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _hazardRepository.GetHazardByIdAsync(createdHazardId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should find existing hazard");
            result.Value.Should().NotBeNull();
            result.Value!.Code.Should().Be(createdHazardId.Value);
            result.Value.Name.Should().Be(testHazard.Name);
        }
        finally
        {
            // Cleanup
            await CleanupHazardAsync(createdHazardId);
        }
    }

    [Fact]
    public async Task Repository_GetAllHazardsAsync_ShouldReturnHazardsList()
    {
        // Arrange - Create test hazard
        var testHazard = CreateTestHazard();
        var createResult = await _hazardRepository.CreateHazardAsync(testHazard);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new HazardID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _hazardRepository.GetAllHazardsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should successfully get all hazards");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test hazard");
            
            // Verify our test hazard is in the results
            var hazardCodes = result.Value.Select(h => h.Code).ToList();
            hazardCodes.Should().Contain(createdId.Value, "Should contain our test hazard");
        }
        finally
        {
            // Cleanup
            await CleanupHazardAsync(createdId);
        }
    }

    [Fact]
    public async Task Repository_UpdateHazardAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a hazard first
        var testHazard = CreateTestHazard();
        var createResult = await _hazardRepository.CreateHazardAsync(testHazard);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdHazard = createResult.Value!;
        var hazardId = new HazardID(createdHazard.Code);

        try
        {
            // Modify the hazard
            createdHazard.Name = "REPO_UPDATED - " + createdHazard.Name;
            createdHazard.Description = "REPO_UPDATED - " + createdHazard.Description;
            createdHazard.UpdatedBy = "INTEGRATION_TEST_REPO_UPDATE";
            createdHazard.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _hazardRepository.UpdateHazardAsync(createdHazard);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("Repository update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.Name.Should().StartWith("REPO_UPDATED - ", "Name should be updated");
            updateResult.Value.Description.Should().StartWith("REPO_UPDATED - ", "Description should be updated");
        }
        finally
        {
            // Cleanup
            await CleanupHazardAsync(hazardId);
        }
    }

    [Fact]
    public async Task Repository_DeleteHazardAsync_WithExistingHazard_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a hazard first
        var testHazard = CreateTestHazard();
        var createResult = await _hazardRepository.CreateHazardAsync(testHazard);
        createResult.IsSuccess.Should().BeTrue();
        
        var hazardId = new HazardID(createResult.Value!.Code);

        // Act
        var deleteResult = await _hazardRepository.DeleteHazardAsync(hazardId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("Repository delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify hazard is actually deleted
        var getResult = await _hazardRepository.GetHazardByIdAsync(hazardId);
        getResult.IsSuccess.Should().BeFalse("Hazard should no longer exist after deletion");
    }

    #endregion

    #region DataService Layer Tests

    [Fact]
    public async Task DataService_CreateHazardAsync_WithValidHazard_ShouldCreateSuccessfully()
    {
        // Arrange
        var testHazard = CreateTestHazard();
        _logger.LogInformation("Creating test hazard via DataService with name: {Name}", testHazard.Name);

        // Act
        var result = await _hazardDataService.CreateHazardAsync(testHazard);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("DataService creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().StartWith("HZ-", "Generated code should have HZ prefix");
        result.Value.Name.Should().Be(testHazard.Name);

        _logger.LogInformation("DataService successfully created hazard with code: {Code}", result.Value.Code);

        // Cleanup
        await CleanupHazardAsync(new HazardID(result.Value.Code));
    }

    [Fact]
    public async Task DataService_GetHazardByIdAsync_WithExistingHazard_ShouldReturnHazard()
    {
        // Arrange - Create hazard via DataService
        var testHazard = CreateTestHazard();
        var createResult = await _hazardDataService.CreateHazardAsync(testHazard);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdHazardId = new HazardID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _hazardDataService.GetHazardByIdAsync(createdHazardId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should find existing hazard");
            result.Value.Should().NotBeNull();
            result.Value!.Code.Should().Be(createdHazardId.Value);
        }
        finally
        {
            // Cleanup
            await CleanupHazardAsync(createdHazardId);
        }
    }

    [Fact]
    public async Task DataService_GetAllHazardsAsync_ShouldReturnHazardsList()
    {
        // Arrange - Create test hazard via DataService
        var testHazard = CreateTestHazard();
        var createResult = await _hazardDataService.CreateHazardAsync(testHazard);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new HazardID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _hazardDataService.GetAllHazardsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should successfully get all hazards");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test hazard");
            
            var hazardCodes = result.Value.Select(h => h.Code).ToList();
            hazardCodes.Should().Contain(createdId.Value, "Should contain our test hazard");
        }
        finally
        {
            // Cleanup
            await CleanupHazardAsync(createdId);
        }
    }

    [Fact]
    public async Task DataService_UpdateHazardAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create hazard via DataService
        var testHazard = CreateTestHazard();
        var createResult = await _hazardDataService.CreateHazardAsync(testHazard);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdHazard = createResult.Value!;
        var hazardId = new HazardID(createdHazard.Code);

        try
        {
            // Modify the hazard
            createdHazard.Name = "DS_UPDATED - " + createdHazard.Name;
            createdHazard.Description = "DS_UPDATED - " + createdHazard.Description;
            createdHazard.UpdatedBy = "INTEGRATION_TEST_DS_UPDATE";
            createdHazard.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _hazardDataService.UpdateHazardAsync(createdHazard);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("DataService update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.Name.Should().StartWith("DS_UPDATED - ", "Name should be updated via DataService");
        }
        finally
        {
            // Cleanup
            await CleanupHazardAsync(hazardId);
        }
    }

    [Fact]
    public async Task DataService_DeleteHazardAsync_WithExistingHazard_ShouldDeleteSuccessfully()
    {
        // Arrange - Create hazard via DataService
        var testHazard = CreateTestHazard();
        var createResult = await _hazardDataService.CreateHazardAsync(testHazard);
        createResult.IsSuccess.Should().BeTrue();
        
        var hazardId = new HazardID(createResult.Value!.Code);

        // Act
        var deleteResult = await _hazardDataService.DeleteHazardAsync(hazardId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("DataService delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify deletion via repository
        var getResult = await _hazardRepository.GetHazardByIdAsync(hazardId);
        getResult.IsSuccess.Should().BeFalse("Hazard should no longer exist after DataService deletion");
    }

    #endregion

    #region Cross-Layer Integration Tests

    [Fact]
    public async Task CrossLayer_CreateViaDataService_ReadViaRepository_ShouldBeConsistent()
    {
        // Arrange
        var testHazard = CreateTestHazard();
        var testId = GenerateTestId();
        testHazard.Name = $"CrossLayer Test {testId}";

        try
        {
            // Act - Create via DataService
            var createResult = await _hazardDataService.CreateHazardAsync(testHazard);
            createResult.IsSuccess.Should().BeTrue();
            
            var createdId = new HazardID(createResult.Value!.Code);

            // Read via Repository
            var readResult = await _hazardRepository.GetHazardByIdAsync(createdId);

            // Assert
            readResult.IsSuccess.Should().BeTrue("Repository should read DataService-created hazard");
            readResult.Value!.Code.Should().Be(createResult.Value.Code);
            readResult.Value.Name.Should().Be(createResult.Value.Name);

            // Cleanup
            await CleanupHazardAsync(createdId);
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
    public async Task Repository_CreateHazardAsync_WithNullHazard_ShouldReturnFailure()
    {
        // Act
        var result = await _hazardRepository.CreateHazardAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("Repository creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task DataService_CreateHazardAsync_WithNullHazard_ShouldReturnFailure()
    {
        // Act
        var result = await _hazardDataService.CreateHazardAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("DataService creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Performance_CreateMultipleHazards_ShouldCompleteInReasonableTime()
    {
        const int hazardCount = 3;
        var createdIds = new List<HazardID>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create multiple hazards via DataService
            for (int i = 0; i < hazardCount; i++)
            {
                var testHazard = CreateTestHazard();
                testHazard.Name = $"Perf Test Hazard {i + 1}";
                
                var result = await _hazardDataService.CreateHazardAsync(testHazard);
                result.IsSuccess.Should().BeTrue($"Hazard {i + 1} should be created successfully");
                
                createdIds.Add(new HazardID(result.Value!.Code));
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert performance (adjust threshold as needed)
            elapsedMs.Should().BeLessThan(15000, $"Creating {hazardCount} hazards should complete within 15 seconds");
            
            _logger.LogInformation("Created {Count} hazards in {ElapsedMs}ms (avg: {AvgMs}ms per hazard)", 
                hazardCount, elapsedMs, elapsedMs / hazardCount);
        }
        finally
        {
            // Cleanup all created hazards
            foreach (var id in createdIds)
            {
                await CleanupHazardAsync(id);
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Cleanup helper to delete test hazards
    /// </summary>
    private async Task CleanupHazardAsync(HazardID hazardId)
    {
        try
        {
            var deleteResult = await _hazardRepository.DeleteHazardAsync(hazardId);
            if (deleteResult.IsSuccess)
            {
                _logger.LogDebug("Cleaned up test hazard: {Code}", hazardId.Value);
            }
            else
            {
                _logger.LogWarning("Failed to cleanup test hazard: {Code}", hazardId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup of test hazard: {Code}", hazardId.Value);
        }
    }

    #endregion
}