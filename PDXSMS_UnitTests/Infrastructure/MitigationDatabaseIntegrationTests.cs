using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Services;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Database Integration Tests for Mitigation CRUD Operations
/// Tests both Repository and DataService layers against the actual PDXSMS_V2 database
/// </summary>
[Collection("Database Integration Tests")]
public class MitigationDatabaseIntegrationTests : DatabaseTestBase
{
    private readonly MitigationRepository _mitigationRepository;
    private readonly MitigationDataService _mitigationDataService;

    public MitigationDatabaseIntegrationTests()
    {
        _mitigationRepository = GetMitigationRepository();
        _mitigationDataService = GetMitigationDataService();
    }

    #region Setup and Connection Tests

    [Fact]
    public async Task DatabaseConnection_ShouldBeValid()
    {
        // Act
        var isValid = await ValidateDatabaseConnectionAsync();

        // Assert
        isValid.Should().BeTrue("Database connection should be valid");
        _logger.LogInformation("Database connection validation passed for Mitigation tests");
    }

    [Fact]
    public void DependencyInjection_ShouldResolveServices()
    {
        // Act & Assert
        _mitigationRepository.Should().NotBeNull("MitigationRepository should be resolved");
        _mitigationDataService.Should().NotBeNull("MitigationDataService should be resolved");
        
        _logger.LogInformation("Dependency injection validation passed for Mitigation services");
    }

    #endregion

    #region Repository Layer Tests

    [Fact]
    public async Task Repository_CreateMitigationAsync_WithValidMitigation_ShouldCreateSuccessfully()
    {
        // Arrange
        var testMitigation = CreateTestMitigation();
        _logger.LogInformation("Creating test mitigation via Repository with code: {Code}", testMitigation.Code);

        // Act
        var result = await _mitigationRepository.CreateMitigationAsync(testMitigation);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Repository creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be(testMitigation.Code);
        result.Value.HazardCode.Should().Be(testMitigation.HazardCode);

        _logger.LogInformation("Repository successfully created mitigation with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupMitigationAsync(new MitigationID(result.Value.Code));
    }

    [Fact]
    public async Task Repository_GetMitigationByIdAsync_WithExistingMitigation_ShouldReturnMitigation()
    {
        // Arrange - Create a mitigation first
        var testMitigation = CreateTestMitigation();
        var createResult = await _mitigationRepository.CreateMitigationAsync(testMitigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdMitigationId = new MitigationID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _mitigationRepository.GetMitigationByIdAsync(createdMitigationId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should find existing mitigation");
            result.Value.Should().NotBeNull();
            result.Value!.Code.Should().Be(createdMitigationId.Value);
            result.Value.Code.Should().Be(testMitigation.Code);
        }
        finally
        {
            // Cleanup
            await CleanupMitigationAsync(createdMitigationId);
        }
    }

    [Fact]
    public async Task Repository_GetAllMitigationsAsync_ShouldReturnMitigationsList()
    {
        // Arrange - Create test mitigation
        var testMitigation = CreateTestMitigation();
        var createResult = await _mitigationRepository.CreateMitigationAsync(testMitigation);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new MitigationID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _mitigationRepository.GetAllMitigationsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should successfully get all mitigations");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test mitigation");
            
            // Verify our test mitigation is in the results
            var mitigationCodes = result.Value.Select(m => m.Code).ToList();
            mitigationCodes.Should().Contain(createdId.Value, "Should contain our test mitigation");
        }
        finally
        {
            // Cleanup
            await CleanupMitigationAsync(createdId);
        }
    }

    [Fact]
    public async Task Repository_UpdateMitigationAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a mitigation first
        var testMitigation = CreateTestMitigation();
        var createResult = await _mitigationRepository.CreateMitigationAsync(testMitigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdMitigation = createResult.Value!;
        var mitigationId = new MitigationID(createdMitigation.Code);

        try
        {
            // Modify the mitigation
            createdMitigation.HazardCode = "REPO_UPDATED - " + createdMitigation.HazardCode;
            createdMitigation.UpdatedBy = "INTEGRATION_TEST_REPO_UPDATE";
            createdMitigation.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _mitigationRepository.UpdateMitigationAsync(createdMitigation);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("Repository update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.HazardCode.Should().StartWith("REPO_UPDATED - ", "HazardCode should be updated");
        }
        finally
        {
            // Cleanup
            await CleanupMitigationAsync(mitigationId);
        }
    }

    [Fact]
    public async Task Repository_DeleteMitigationAsync_WithExistingMitigation_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a mitigation first
        var testMitigation = CreateTestMitigation();
        var createResult = await _mitigationRepository.CreateMitigationAsync(testMitigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var mitigationId = new MitigationID(createResult.Value!.Code);

        // Act
        var deleteResult = await _mitigationRepository.DeleteMitigationAsync(mitigationId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("Repository delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify mitigation is actually deleted
        var getResult = await _mitigationRepository.GetMitigationByIdAsync(mitigationId);
        getResult.IsSuccess.Should().BeFalse("Mitigation should no longer exist after deletion");
    }

    #endregion

    #region DataService Layer Tests

    [Fact]
    public async Task DataService_CreateMitigationAsync_WithValidMitigation_ShouldCreateSuccessfully()
    {
        // Arrange
        var testMitigation = CreateTestMitigation();
        _logger.LogInformation("Creating test mitigation via DataService with code: {Code}", testMitigation.Code);

        // Act
        var result = await _mitigationDataService.CreateMitigationAsync(testMitigation);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("DataService creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be(testMitigation.Code);
        result.Value.HazardCode.Should().Be(testMitigation.HazardCode);

        _logger.LogInformation("DataService successfully created mitigation with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupMitigationAsync(new MitigationID(result.Value.Code));
    }

    [Fact]
    public async Task DataService_GetMitigationByIdAsync_WithExistingMitigation_ShouldReturnMitigation()
    {
        // Arrange - Create mitigation via DataService
        var testMitigation = CreateTestMitigation();
        var createResult = await _mitigationDataService.CreateMitigationAsync(testMitigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdMitigationId = new MitigationID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _mitigationDataService.GetMitigationByIdAsync(createdMitigationId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should find existing mitigation");
            result.Value.Should().NotBeNull();
            result.Value!.Code.Should().Be(createdMitigationId.Value);
        }
        finally
        {
            // Cleanup
            await CleanupMitigationAsync(createdMitigationId);
        }
    }

    [Fact]
    public async Task DataService_GetAllMitigationsAsync_ShouldReturnMitigationsList()
    {
        // Arrange - Create test mitigation via DataService
        var testMitigation = CreateTestMitigation();
        var createResult = await _mitigationDataService.CreateMitigationAsync(testMitigation);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new MitigationID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _mitigationDataService.GetAllMitigationsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should successfully get all mitigations");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test mitigation");
            
            var mitigationCodes = result.Value.Select(m => m.Code).ToList();
            mitigationCodes.Should().Contain(createdId.Value, "Should contain our test mitigation");
        }
        finally
        {
            // Cleanup
            await CleanupMitigationAsync(createdId);
        }
    }

    [Fact]
    public async Task DataService_UpdateMitigationAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create mitigation via DataService
        var testMitigation = CreateTestMitigation();
        var createResult = await _mitigationDataService.CreateMitigationAsync(testMitigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdMitigation = createResult.Value!;
        var mitigationId = new MitigationID(createdMitigation.Code);

        try
        {
            // Modify the mitigation
            createdMitigation.HazardCode = "DS_UPDATED - " + createdMitigation.HazardCode;
            createdMitigation.UpdatedBy = "INTEGRATION_TEST_DS_UPDATE";
            createdMitigation.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _mitigationDataService.UpdateMitigationAsync(createdMitigation);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("DataService update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.HazardCode.Should().StartWith("DS_UPDATED - ", "HazardCode should be updated via DataService");
        }
        finally
        {
            // Cleanup
            await CleanupMitigationAsync(mitigationId);
        }
    }

    [Fact]
    public async Task DataService_DeleteMitigationAsync_WithExistingMitigation_ShouldDeleteSuccessfully()
    {
        // Arrange - Create mitigation via DataService
        var testMitigation = CreateTestMitigation();
        var createResult = await _mitigationDataService.CreateMitigationAsync(testMitigation);
        createResult.IsSuccess.Should().BeTrue();
        
        var mitigationId = new MitigationID(createResult.Value!.Code);

        // Act
        var deleteResult = await _mitigationDataService.DeleteMitigationAsync(mitigationId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("DataService delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify deletion via repository
        var getResult = await _mitigationRepository.GetMitigationByIdAsync(mitigationId);
        getResult.IsSuccess.Should().BeFalse("Mitigation should no longer exist after DataService deletion");
    }

    #endregion

    #region Cross-Layer Integration Tests

    [Fact]
    public async Task CrossLayer_CreateViaDataService_ReadViaRepository_ShouldBeConsistent()
    {
        // Arrange
        var testMitigation = CreateTestMitigation();
        var testId = GenerateTestId();
        testMitigation.HazardCode = $"CrossLayer Test {testId}";

        try
        {
            // Act - Create via DataService
            var createResult = await _mitigationDataService.CreateMitigationAsync(testMitigation);
            createResult.IsSuccess.Should().BeTrue();
            
            var createdId = new MitigationID(createResult.Value!.Code);

            // Read via Repository
            var readResult = await _mitigationRepository.GetMitigationByIdAsync(createdId);

            // Assert
            readResult.IsSuccess.Should().BeTrue("Repository should read DataService-created mitigation");
            readResult.Value!.Code.Should().Be(createResult.Value.Code);
            readResult.Value.HazardCode.Should().Be(createResult.Value.HazardCode);

            // Cleanup
            await CleanupMitigationAsync(createdId);
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
    public async Task Repository_CreateMitigationAsync_WithNullMitigation_ShouldReturnFailure()
    {
        // Act
        var result = await _mitigationRepository.CreateMitigationAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("Repository creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task DataService_CreateMitigationAsync_WithNullMitigation_ShouldReturnFailure()
    {
        // Act
        var result = await _mitigationDataService.CreateMitigationAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("DataService creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Performance_CreateMultipleMitigations_ShouldCompleteInReasonableTime()
    {
        const int mitigationCount = 3;
        var createdIds = new List<MitigationID>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create multiple mitigations via DataService
            for (int i = 0; i < mitigationCount; i++)
            {
                var testMitigation = CreateTestMitigation();
                testMitigation.HazardCode = $"Perf Test Hazard {i + 1}";
                
                var result = await _mitigationDataService.CreateMitigationAsync(testMitigation);
                result.IsSuccess.Should().BeTrue($"Mitigation {i + 1} should be created successfully");
                
                createdIds.Add(new MitigationID(result.Value!.Code));
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert performance (adjust threshold as needed)
            elapsedMs.Should().BeLessThan(15000, $"Creating {mitigationCount} mitigations should complete within 15 seconds");
            
            _logger.LogInformation("Created {Count} mitigations in {ElapsedMs}ms (avg: {AvgMs}ms per mitigation)", 
                mitigationCount, elapsedMs, elapsedMs / mitigationCount);
        }
        finally
        {
            // Cleanup all created mitigations
            foreach (var id in createdIds)
            {
                await CleanupMitigationAsync(id);
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Cleanup helper to delete test mitigations
    /// </summary>
    private async Task CleanupMitigationAsync(MitigationID mitigationId)
    {
        try
        {
            var deleteResult = await _mitigationRepository.DeleteMitigationAsync(mitigationId);
            if (deleteResult.IsSuccess)
            {
                _logger.LogDebug("Cleaned up test mitigation: {Id}", mitigationId.Value);
            }
            else
            {
                _logger.LogWarning("Failed to cleanup test mitigation: {Id}", mitigationId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup of test mitigation: {Id}", mitigationId.Value);
        }
    }

    #endregion
}