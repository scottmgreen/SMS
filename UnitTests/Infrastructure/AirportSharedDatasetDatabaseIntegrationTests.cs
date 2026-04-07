using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SMS_Domain.Entities;

using SMS_Infrastructure.Persistence;
using SMS_Infrastructure.Services;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Database Integration Tests for AirportSharedDataset CRUD Operations
/// Tests both Repository and DataService layers against the actual PDXSMS_V2 database
/// </summary>
[Collection("Database Integration Tests")]
public class AirportSharedDatasetDatabaseIntegrationTests : DatabaseTestBase
{
    private readonly AirportSharedDatasetRepository _airportSharedDatasetRepository;
    private readonly AirportSharedDatasetDataService _airportSharedDatasetDataService;

    public AirportSharedDatasetDatabaseIntegrationTests()
    {
        _airportSharedDatasetRepository = GetAirportSharedDatasetRepository();
        _airportSharedDatasetDataService = GetAirportSharedDatasetDataService();
    }

    #region Setup and Connection Tests

    [Fact]
    public async Task DatabaseConnection_ShouldBeValid()
    {
        // Act
        var isValid = await ValidateDatabaseConnectionAsync();

        // Assert
        isValid.Should().BeTrue("Database connection should be valid");
        _logger.LogInformation("Database connection validation passed for AirportSharedDataset tests");
    }

    [Fact]
    public void DependencyInjection_ShouldResolveServices()
    {
        // Act & Assert
        _airportSharedDatasetRepository.Should().NotBeNull("AirportSharedDatasetRepository should be resolved");
        _airportSharedDatasetDataService.Should().NotBeNull("AirportSharedDatasetDataService should be resolved");
        
        _logger.LogInformation("Dependency injection validation passed for AirportSharedDataset services");
    }

    #endregion

    #region Repository Layer Tests

    [Fact]
    public async Task Repository_CreateAirportSharedDatasetAsync_WithValidDataset_ShouldCreateSuccessfully()
    {
        // Arrange
        var testDataset = CreateTestAirportSharedDataset();
        _logger.LogInformation("Creating test airport shared dataset via Repository with code: {Code}", testDataset.Code);

        // Act
        var result = await _airportSharedDatasetRepository.CreateAirportSharedDatasetAsync(testDataset);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Repository creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().NotBe(testDataset.Code);
        result.Value.ReportCode.Should().Be(testDataset.ReportCode);
        result.Value.PrivateNarrative.Should().Be(testDataset.PrivateNarrative);

        _logger.LogInformation("Repository successfully created airport shared dataset with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupAirportSharedDatasetAsync(new AirportSharedDatasetID(result.Value.Code));
    }

    [Fact]
    public async Task Repository_GetAirportSharedDatasetByIdAsync_WithExistingDataset_ShouldReturnDataset()
    {
        // Arrange - Create a dataset first
        var testDataset = CreateTestAirportSharedDataset();
        var createResult = await _airportSharedDatasetRepository.CreateAirportSharedDatasetAsync(testDataset);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdDatasetId = new AirportSharedDatasetID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _airportSharedDatasetRepository.GetAirportSharedDatasetByCodeAsync(createdDatasetId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should find existing dataset");
            result.Value.Should().NotBeNull();
            result.Value!.Id.Value.Should().Be(createdDatasetId.Value);
            result.Value.Code.Should().NotBe(testDataset.Code);
        }
        finally
        {
            // Cleanup
            await CleanupAirportSharedDatasetAsync(createdDatasetId);
        }
    }

    [Fact]
    public async Task Repository_GetAllAirportSharedDatasetsAsync_ShouldReturnDatasetsList()
    {
        // Arrange - Create test dataset
        var testDataset = CreateTestAirportSharedDataset();
        var createResult = await _airportSharedDatasetRepository.CreateAirportSharedDatasetAsync(testDataset);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new AirportSharedDatasetID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _airportSharedDatasetRepository.GetAllAirportSharedDatasetsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should successfully get all datasets");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test dataset");
            
            // Verify our test dataset is in the results
            var datasetIds = result.Value.Select(d => d.Code).ToList();
            datasetIds.Should().Contain(createdId.Value, "Should contain our test dataset");
        }
        finally
        {
            // Cleanup
            await CleanupAirportSharedDatasetAsync(createdId);
        }
    }

    [Fact]
    public async Task Repository_UpdateAirportSharedDatasetAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a dataset first
        var testDataset = CreateTestAirportSharedDataset();
        var createResult = await _airportSharedDatasetRepository.CreateAirportSharedDatasetAsync(testDataset);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdDataset = createResult.Value!;
        var datasetId = new AirportSharedDatasetID(createdDataset.Code);

        try
        {
            // Modify the dataset
            createdDataset.PrivateNarrative = "UPDATED - " + createdDataset.PrivateNarrative;
            createdDataset.SharedNarrative = "UPDATED - " + createdDataset.SharedNarrative;
            createdDataset.UpdatedBy = "INTEGRATION_TEST_REPO_UPDATE";
            createdDataset.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _airportSharedDatasetRepository.UpdateAirportSharedDatasetAsync(createdDataset);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("Repository update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.PrivateNarrative.Should().StartWith("UPDATED", "PrivateNarrative should be updated");
            updateResult.Value.SharedNarrative.Should().StartWith("UPDATED", "SharedNarrative should be updated");
        }
        finally
        {
            // Cleanup
            await CleanupAirportSharedDatasetAsync(datasetId);
        }
    }

    [Fact]
    public async Task Repository_DeleteAirportSharedDatasetAsync_WithExistingDataset_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a dataset first
        var testDataset = CreateTestAirportSharedDataset();
        var createResult = await _airportSharedDatasetRepository.CreateAirportSharedDatasetAsync(testDataset);
        createResult.IsSuccess.Should().BeTrue();
        
        var datasetId = new AirportSharedDatasetID(createResult.Value!.Code);

        // Act
        var deleteResult = await _airportSharedDatasetRepository.DeleteAirportSharedDatasetAsync(datasetId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("Repository delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify dataset is actually deleted
        var getResult = await _airportSharedDatasetRepository.GetAirportSharedDatasetByCodeAsync(datasetId);
        getResult.IsSuccess.Should().BeFalse("Dataset should no longer exist after deletion");
    }

    #endregion

    #region DataService Layer Tests

    [Fact]
    public async Task DataService_CreateAirportSharedDatasetAsync_WithValidDataset_ShouldCreateSuccessfully()
    {
        // Arrange
        var testDataset = CreateTestAirportSharedDataset();
        _logger.LogInformation("Creating test airport shared dataset via DataService with code: {Code}", testDataset.Code);

        // Act
        var result = await _airportSharedDatasetDataService.CreateAirportSharedDatasetAsync(testDataset);
        var datasetId = new AirportSharedDatasetID(result.Value!.Code);
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("DataService creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().NotBe(testDataset.Code);
        result.Value.ReportCode.Should().Be(testDataset.ReportCode);
        //result.Value.Name.Should().Be(testDataset.Name);

        _logger.LogInformation("DataService successfully created airport shared dataset with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupAirportSharedDatasetAsync(datasetId);
    }

    [Fact]
    public async Task DataService_GetAirportSharedDatasetByIdAsync_WithExistingDataset_ShouldReturnDataset()
    {
        // Arrange - Create dataset via DataService
        var testDataset = CreateTestAirportSharedDataset();
        var createResult = await _airportSharedDatasetDataService.CreateAirportSharedDatasetAsync(testDataset);
        createResult.IsSuccess.Should().BeTrue();

        var createdDatasetId = new AirportSharedDatasetID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _airportSharedDatasetDataService.GetAirportSharedDatasetByCodeAsync(createdDatasetId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should find existing dataset");
            result.Value.Should().NotBeNull();
            result.Value!.Id.Value.Should().Be(createdDatasetId.Value);
        }
        finally
        {
            // Cleanup
            await CleanupAirportSharedDatasetAsync(createdDatasetId);
        }
    }

    [Fact]
    public async Task DataService_GetAllAirportSharedDatasetsAsync_ShouldReturnDatasetsList()
    {
        // Arrange - Create test dataset via DataService
        var testDataset = CreateTestAirportSharedDataset();
        var createResult = await _airportSharedDatasetDataService.CreateAirportSharedDatasetAsync(testDataset);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new AirportSharedDatasetID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _airportSharedDatasetDataService.GetAllAirportSharedDatasetsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should successfully get all datasets");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test dataset");
            
            var datasetIds = result.Value.Select(d => d.Code).ToList();
            datasetIds.Should().Contain(createdId.Value, "Should contain our test dataset");
        }
        finally
        {
            // Cleanup
            await CleanupAirportSharedDatasetAsync(createdId);
        }
    }

    [Fact]
    public async Task DataService_UpdateAirportSharedDatasetAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create dataset via DataService
        var testDataset = CreateTestAirportSharedDataset();
        var createResult = await _airportSharedDatasetDataService.CreateAirportSharedDatasetAsync(testDataset);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdDataset = createResult.Value!;
        var datasetId = new AirportSharedDatasetID(createdDataset.Code);

        try
        {
            // Modify the dataset
            createdDataset.PrivateNarrative = "UPDATED" + createdDataset.PrivateNarrative;
            createdDataset.SharedNarrative = "UPDATED" + createdDataset.SharedNarrative;
            createdDataset.UpdatedBy = "INTEGRATION_TEST_DS_UPDATE";
            createdDataset.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _airportSharedDatasetDataService.UpdateAirportSharedDatasetAsync(createdDataset);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("DataService update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.PrivateNarrative.Should().StartWith("UPDATED", "PrivateNarrative should be updated via DataService");
        }
        finally
        {
            // Cleanup
            await CleanupAirportSharedDatasetAsync(datasetId);
        }
    }

    [Fact]
    public async Task DataService_DeleteAirportSharedDatasetAsync_WithExistingDataset_ShouldDeleteSuccessfully()
    {
        // Arrange - Create dataset via DataService
        var testDataset = CreateTestAirportSharedDataset();
        var createResult = await _airportSharedDatasetDataService.CreateAirportSharedDatasetAsync(testDataset);
        createResult.IsSuccess.Should().BeTrue();
        
        var datasetId = new AirportSharedDatasetID(createResult.Value!.Code);

        // Act
        var deleteResult = await _airportSharedDatasetDataService.DeleteAirportSharedDatasetAsync(datasetId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("DataService delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify deletion via repository
        var getResult = await _airportSharedDatasetRepository.GetAirportSharedDatasetByCodeAsync(datasetId);
        getResult.IsSuccess.Should().BeFalse("Dataset should no longer exist after DataService deletion");
    }

    #endregion

    #region Cross-Layer Integration Tests

    [Fact]
    public async Task CrossLayer_CreateViaDataService_ReadViaRepository_ShouldBeConsistent()
    {
        // Arrange
        var testDataset = CreateTestAirportSharedDataset();
        var testId = GenerateTestId();
        testDataset.PrivateNarrative = $"CrossLayer Test {testId}";

        try
        {
            // Act - Create via DataService
            var createResult = await _airportSharedDatasetDataService.CreateAirportSharedDatasetAsync(testDataset);
            createResult.IsSuccess.Should().BeTrue();
            
            var createdId = new AirportSharedDatasetID(createResult.Value!.Code);

            // Read via Repository
            var readResult = await _airportSharedDatasetRepository.GetAirportSharedDatasetByCodeAsync(createdId);

            // Assert
            readResult.IsSuccess.Should().BeTrue("Repository should read DataService-created dataset");
            readResult.Value!.Code.Should().Be(createResult.Value.Code);
            readResult.Value.PrivateNarrative.Should().Be(createResult.Value.PrivateNarrative);

            // Cleanup
            await CleanupAirportSharedDatasetAsync(createdId);
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
    public async Task Repository_CreateAirportSharedDatasetAsync_WithNullDataset_ShouldReturnFailure()
    {
        // Act
        var result = await _airportSharedDatasetRepository.CreateAirportSharedDatasetAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("Repository creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task DataService_CreateAirportSharedDatasetAsync_WithNullDataset_ShouldReturnFailure()
    {
        // Act
        var result = await _airportSharedDatasetDataService.CreateAirportSharedDatasetAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("DataService creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Performance_CreateMultipleDatasets_ShouldCompleteInReasonableTime()
    {
        const int datasetCount = 3;
        var createdIds = new List<AirportSharedDatasetID>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create multiple datasets via DataService
            for (int i = 0; i < datasetCount; i++)
            {
                var testDataset = CreateTestAirportSharedDataset();
                testDataset.PrivateNarrative = $"Perf Test Dataset {i + 1}";
                
                var result = await _airportSharedDatasetDataService.CreateAirportSharedDatasetAsync(testDataset);
                result.IsSuccess.Should().BeTrue($"Dataset {i + 1} should be created successfully");
                
                createdIds.Add(new AirportSharedDatasetID(result.Value!.Code));
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert performance (adjust threshold as needed)
            elapsedMs.Should().BeLessThan(15000, $"Creating {datasetCount} datasets should complete within 15 seconds");
            
            _logger.LogInformation("Created {Count} datasets in {ElapsedMs}ms (avg: {AvgMs}ms per dataset)", 
                datasetCount, elapsedMs, elapsedMs / datasetCount);
        }
        finally
        {
            // Cleanup all created datasets
            foreach (var id in createdIds)
            {
                await CleanupAirportSharedDatasetAsync(id);
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Cleanup helper to delete test datasets
    /// </summary>
    private async Task CleanupAirportSharedDatasetAsync(AirportSharedDatasetID datasetId)
    {
        try
        {
            var deleteResult = await _airportSharedDatasetRepository.DeleteAirportSharedDatasetAsync(datasetId);
            if (deleteResult.IsSuccess)
            {
                _logger.LogDebug("Cleaned up test airport shared dataset: {Id}", datasetId.Value);
            }
            else
            {
                _logger.LogWarning("Failed to cleanup test airport shared dataset: {Id}", datasetId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup of test airport shared dataset: {Id}", datasetId.Value);
        }
    }

    #endregion
}