using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Services;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Database Integration Tests for ScoringPanel CRUD Operations
/// Tests both Repository and DataService layers against the actual PDXSMS_V2 database
/// </summary>
[Collection("Database Integration Tests")]
public class ScoringPanelDatabaseIntegrationTests : DatabaseTestBase
{
    private readonly ScoringPanelRepository _scoringPanelRepository;
    private readonly ScoringPanelDataService _scoringPanelDataService;

    public ScoringPanelDatabaseIntegrationTests()
    {
        _scoringPanelRepository = GetScoringPanelRepository();
        _scoringPanelDataService = GetScoringPanelDataService();
    }

    #region Setup and Connection Tests

    [Fact]
    public async Task DatabaseConnection_ShouldBeValid()
    {
        // Act
        var isValid = await ValidateDatabaseConnectionAsync();

        // Assert
        isValid.Should().BeTrue("Database connection should be valid");
        _logger.LogInformation("Database connection validation passed for ScoringPanel tests");
    }

    [Fact]
    public void DependencyInjection_ShouldResolveServices()
    {
        // Act & Assert
        _scoringPanelRepository.Should().NotBeNull("ScoringPanelRepository should be resolved");
        _scoringPanelDataService.Should().NotBeNull("ScoringPanelDataService should be resolved");
        
        _logger.LogInformation("Dependency injection validation passed for ScoringPanel services");
    }

    #endregion

    #region Repository Layer Tests

    [Fact]
    public async Task Repository_CreateScoringPanelAsync_WithValidScoringPanel_ShouldCreateSuccessfully()
    {
        // Arrange
        var testScoringPanel = CreateTestScoringPanel();
        _logger.LogInformation("Creating test scoring panel via Repository with code: {Code}", testScoringPanel.Code);

        // Act
        var result = await _scoringPanelRepository.CreateScoringPanelAsync(testScoringPanel);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Repository creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be(testScoringPanel.Code);
        result.Value.HazardCode.Should().Be(testScoringPanel.HazardCode);
        result.Value.Likelihood.Should().Be(testScoringPanel.Likelihood);

        _logger.LogInformation("Repository successfully created scoring panel with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupScoringPanelAsync(new ScoringPanelID(result.Value.Code));
    }

    [Fact]
    public async Task Repository_GetScoringPanelByIdAsync_WithExistingScoringPanel_ShouldReturnScoringPanel()
    {
        // Arrange - Create a scoring panel first
        var testScoringPanel = CreateTestScoringPanel();
        var createResult = await _scoringPanelRepository.CreateScoringPanelAsync(testScoringPanel);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdScoringPanelId = new ScoringPanelID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _scoringPanelRepository.GetScoringPanelByIdAsync(createdScoringPanelId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should find existing scoring panel");
            result.Value.Should().NotBeNull();
            result.Value!.Code.Should().Be(createdScoringPanelId.Value);
            result.Value.Code.Should().Be(testScoringPanel.Code);
        }
        finally
        {
            // Cleanup
            await CleanupScoringPanelAsync(createdScoringPanelId);
        }
    }

    [Fact]
    public async Task Repository_GetAllScoringPanelsAsync_ShouldReturnScoringPanelsList()
    {
        // Arrange - Create test scoring panel
        var testScoringPanel = CreateTestScoringPanel();
        var createResult = await _scoringPanelRepository.CreateScoringPanelAsync(testScoringPanel);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new ScoringPanelID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _scoringPanelRepository.GetAllScoringPanelsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should successfully get all scoring panels");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test scoring panel");
            
            // Verify our test scoring panel is in the results
            var scoringPanelCodes = result.Value.Select(sp => sp.Code).ToList();
            scoringPanelCodes.Should().Contain(createdId.Value, "Should contain our test scoring panel");
        }
        finally
        {
            // Cleanup
            await CleanupScoringPanelAsync(createdId);
        }
    }

    [Fact]
    public async Task Repository_UpdateScoringPanelAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a scoring panel first
        var testScoringPanel = CreateTestScoringPanel();
        var createResult = await _scoringPanelRepository.CreateScoringPanelAsync(testScoringPanel);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdScoringPanel = createResult.Value!;
        var scoringPanelId = new ScoringPanelID(createdScoringPanel.Code);

        try
        {
            // Modify the scoring panel
            createdScoringPanel.Likelihood = "REPO_UPDATED - " + createdScoringPanel.Likelihood;
            createdScoringPanel.Severity = "REPO_UPDATED - " + createdScoringPanel.Severity;
            createdScoringPanel.UpdatedBy = "INTEGRATION_TEST_REPO_UPDATE";
            createdScoringPanel.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _scoringPanelRepository.UpdateScoringPanelAsync(createdScoringPanel);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("Repository update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.Likelihood.Should().StartWith("REPO_UPDATED - ", "Likelihood should be updated");
            updateResult.Value.Severity.Should().StartWith("REPO_UPDATED - ", "Severity should be updated");
        }
        finally
        {
            // Cleanup
            await CleanupScoringPanelAsync(scoringPanelId);
        }
    }

    [Fact]
    public async Task Repository_DeleteScoringPanelAsync_WithExistingScoringPanel_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a scoring panel first
        var testScoringPanel = CreateTestScoringPanel();
        var createResult = await _scoringPanelRepository.CreateScoringPanelAsync(testScoringPanel);
        createResult.IsSuccess.Should().BeTrue();
        
        var scoringPanelId = new ScoringPanelID(createResult.Value!.Code);

        // Act
        var deleteResult = await _scoringPanelRepository.DeleteScoringPanelAsync(scoringPanelId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("Repository delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify scoring panel is actually deleted
        var getResult = await _scoringPanelRepository.GetScoringPanelByIdAsync(scoringPanelId);
        getResult.IsSuccess.Should().BeFalse("ScoringPanel should no longer exist after deletion");
    }

    #endregion

    #region DataService Layer Tests

    [Fact]
    public async Task DataService_CreateScoringPanelAsync_WithValidScoringPanel_ShouldCreateSuccessfully()
    {
        // Arrange
        var testScoringPanel = CreateTestScoringPanel();
        _logger.LogInformation("Creating test scoring panel via DataService with code: {Code}", testScoringPanel.Code);

        // Act
        var result = await _scoringPanelDataService.CreateScoringPanelAsync(testScoringPanel);
        var createdId = new ScoringPanelID(result.Value!.Code);
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("DataService creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be(testScoringPanel.Code);
        //result.Value.Name.Should().Be(testScoringPanel.Name);

        _logger.LogInformation("DataService successfully created scoring panel with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupScoringPanelAsync(createdId);
    }

    [Fact]
    public async Task DataService_GetScoringPanelByIdAsync_WithExistingScoringPanel_ShouldReturnScoringPanel()
    {
        // Arrange - Create scoring panel via DataService
        var testScoringPanel = CreateTestScoringPanel();
        var createResult = await _scoringPanelDataService.CreateScoringPanelAsync(testScoringPanel);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdScoringPanelId = new ScoringPanelID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _scoringPanelDataService.GetScoringPanelByIdAsync(createdScoringPanelId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should find existing scoring panel");
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(createdScoringPanelId);
        }
        finally
        {
            // Cleanup
            await CleanupScoringPanelAsync(createdScoringPanelId);
        }
    }

    [Fact]
    public async Task DataService_GetAllScoringPanelsAsync_ShouldReturnScoringPanelsList()
    {
        // Arrange - Create test scoring panel via DataService
        var testScoringPanel = CreateTestScoringPanel();
        var createResult = await _scoringPanelDataService.CreateScoringPanelAsync(testScoringPanel);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new ScoringPanelID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _scoringPanelDataService.GetAllScoringPanelsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should successfully get all scoring panels");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test scoring panel");
            
            var scoringPanelIds = result.Value.Select(sp => sp.Id.Value).ToList();
            scoringPanelIds.Should().Contain(createdId.Value, "Should contain our test scoring panel");
        }
        finally
        {
            // Cleanup
            await CleanupScoringPanelAsync(createdId);
        }
    }

    [Fact]
    public async Task DataService_UpdateScoringPanelAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create scoring panel via DataService
        var testScoringPanel = CreateTestScoringPanel();
        var createResult = await _scoringPanelDataService.CreateScoringPanelAsync(testScoringPanel);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdScoringPanel = createResult.Value!;
        var scoringPanelId = new ScoringPanelID (createdScoringPanel.Code);

        try
        {
            // Modify the scoring panel
            //createdScoringPanel.Name = "DS_UPDATED - " + createdScoringPanel.Name;
            //createdScoringPanel.Description = "DS_UPDATED - " + createdScoringPanel.Description;
            createdScoringPanel.UpdatedBy = "INTEGRATION_TEST_DS_UPDATE";
            createdScoringPanel.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _scoringPanelDataService.UpdateScoringPanelAsync(createdScoringPanel);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("DataService update should succeed");
            updateResult.Value.Should().NotBeNull();
            //updateResult.Value!.Name.Should().StartWith("DS_UPDATED - ", "Name should be updated via DataService");
        }
        finally
        {
            // Cleanup
            await CleanupScoringPanelAsync(scoringPanelId);
        }
    }

    [Fact]
    public async Task DataService_DeleteScoringPanelAsync_WithExistingScoringPanel_ShouldDeleteSuccessfully()
    {
        // Arrange - Create scoring panel via DataService
        var testScoringPanel = CreateTestScoringPanel();
        var createResult = await _scoringPanelDataService.CreateScoringPanelAsync(testScoringPanel);
        createResult.IsSuccess.Should().BeTrue();
        
        var scoringPanelId = new ScoringPanelID(createResult.Value!.Code);

        // Act
        var deleteResult = await _scoringPanelDataService.DeleteScoringPanelAsync(scoringPanelId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("DataService delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify deletion via repository
        var getResult = await _scoringPanelRepository.GetScoringPanelByIdAsync(scoringPanelId);
        getResult.IsSuccess.Should().BeFalse("ScoringPanel should no longer exist after DataService deletion");
    }

    #endregion

    #region Cross-Layer Integration Tests

    [Fact]
    public async Task CrossLayer_CreateViaDataService_ReadViaRepository_ShouldBeConsistent()
    {
        // Arrange
        var testScoringPanel = CreateTestScoringPanel();
        var testId = GenerateTestId();
        //testScoringPanel.Name = $"CrossLayer Test {testId}";

        try
        {
            // Act - Create via DataService
            var createResult = await _scoringPanelDataService.CreateScoringPanelAsync(testScoringPanel);
            createResult.IsSuccess.Should().BeTrue();
            
            var createdId = new ScoringPanelID(createResult.Value!.Code);

            // Read via Repository
            var readResult = await _scoringPanelRepository.GetScoringPanelByIdAsync(createdId);

            // Assert
            readResult.IsSuccess.Should().BeTrue("Repository should read DataService-created scoring panel");
            readResult.Value!.Id.Should().Be(createResult.Value.Id);
            readResult.Value.Code.Should().Be(createResult.Value.Code);
            //readResult.Value.Name.Should().Be(createResult.Value.Name);

            // Cleanup
            await CleanupScoringPanelAsync(createdId);
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
    public async Task Repository_CreateScoringPanelAsync_WithNullScoringPanel_ShouldReturnFailure()
    {
        // Act
        var result = await _scoringPanelRepository.CreateScoringPanelAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("Repository creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task DataService_CreateScoringPanelAsync_WithNullScoringPanel_ShouldReturnFailure()
    {
        // Act
        var result = await _scoringPanelDataService.CreateScoringPanelAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("DataService creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Performance_CreateMultipleScoringPanels_ShouldCompleteInReasonableTime()
    {
        const int scoringPanelCount = 3;
        var createdIds = new List<ScoringPanelID>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create multiple scoring panels via DataService
            for (int i = 0; i < scoringPanelCount; i++)
            {
                var testScoringPanel = CreateTestScoringPanel();
                //testScoringPanel.Name = $"Perf Test ScoringPanel {i + 1}";
                
                var result = await _scoringPanelDataService.CreateScoringPanelAsync(testScoringPanel);
                result.IsSuccess.Should().BeTrue($"ScoringPanel {i + 1} should be created successfully");
                
                createdIds.Add(new ScoringPanelID(result.Value!.Code ));
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert performance (adjust threshold as needed)
            elapsedMs.Should().BeLessThan(15000, $"Creating {scoringPanelCount} scoring panels should complete within 15 seconds");
            
            _logger.LogInformation("Created {Count} scoring panels in {ElapsedMs}ms (avg: {AvgMs}ms per scoring panel)", 
                scoringPanelCount, elapsedMs, elapsedMs / scoringPanelCount);
        }
        finally
        {
            // Cleanup all created scoring panels
            foreach (var id in createdIds)
            {
                await CleanupScoringPanelAsync(id);
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Cleanup helper to delete test scoring panels
    /// </summary>
    private async Task CleanupScoringPanelAsync(ScoringPanelID scoringPanelId)
    {
        try
        {
            var deleteResult = await _scoringPanelRepository.DeleteScoringPanelAsync(scoringPanelId);
            if (deleteResult.IsSuccess)
            {
                _logger.LogDebug("Cleaned up test scoring panel: {Id}", scoringPanelId.Value);
            }
            else
            {
                _logger.LogWarning("Failed to cleanup test scoring panel: {Id}", scoringPanelId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup of test scoring panel: {Id}", scoringPanelId.Value);
        }
    }

    #endregion
}