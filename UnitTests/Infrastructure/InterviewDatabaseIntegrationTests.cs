using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SMS_Domain.Common;
using SMS_Domain.Entities;

using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Services;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Database Integration Tests for Interview CRUD Operations
/// Tests both Repository and DataService layers against the actual PDXSMS_V2 database
/// </summary>
[Collection("Database Integration Tests")]
public class InterviewDatabaseIntegrationTests : DatabaseTestBase
{
    private readonly InterviewRepository _interviewRepository;
    private readonly InterviewDataService _interviewDataService;

    public InterviewDatabaseIntegrationTests()
    {
        _interviewRepository = GetInterviewRepository();
        _interviewDataService = GetInterviewDataService();
    }

    #region Setup and Connection Tests

    [Fact]
    public async Task DatabaseConnection_ShouldBeValid()
    {
        // Act
        var isValid = await ValidateDatabaseConnectionAsync();

        // Assert
        isValid.Should().BeTrue("Database connection should be valid");
        _logger.LogInformation("Database connection validation passed for Interview tests");
    }

    [Fact]
    public void DependencyInjection_ShouldResolveServices()
    {
        // Act & Assert
        _interviewRepository.Should().NotBeNull("InterviewRepository should be resolved");
        _interviewDataService.Should().NotBeNull("InterviewDataService should be resolved");
        
        _logger.LogInformation("Dependency injection validation passed for Interview services");
    }

    #endregion

    #region Repository Layer Tests

    [Fact]
    public async Task Repository_CreateInterviewAsync_WithValidInterview_ShouldCreateSuccessfully()
    {
        // Arrange
        var testInterview = CreateTestInterview();
        _logger.LogInformation("Creating test interview via Repository with code: {Code}", testInterview.Code);

        // Act
        var result = await _interviewRepository.CreateInterviewAsync(testInterview);
        var createdId = new InterviewID (result.Value?.Code);
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Repository creation should succeed");
        result.Value.Should().NotBeNull();
        //result.Value!.Code.Should().Be(testInterview.Code);
        //result.Value.PersonInterviewed.Should().Be(testInterview.PersonInterviewed);
        //result.Value.InvestigatorNotes.Should().Be(testInterview.InvestigatorNotes);

        _logger.LogInformation("Repository successfully created interview with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupInterviewAsync(createdId);
    }

    [Fact]
    public async Task Repository_GetInterviewByIdAsync_WithExistingInterview_ShouldReturnInterview()
    {
        // Arrange - Create an interview first
        var testInterview = CreateTestInterview();
        var createResult = await _interviewRepository.CreateInterviewAsync(testInterview);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new InterviewID(createResult.Value?.Code);

        var result = await _interviewRepository.GetInterviewByIdAsync(createdId);
        var createdresultId = new InterviewID(createResult.Value?.Code);

        try
        {
            // Act
            
            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should find existing interview");
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(createdId);
            //result.Value.Code.Should().Be(testInterview.Code);
        }
        finally
        {
            // Cleanup
            await CleanupInterviewAsync(createdresultId);
        }
    }

    [Fact]
    public async Task Repository_GetAllInterviewsAsync_ShouldReturnInterviewsList()
    {
        // Arrange - Create test interview
        var testInterview = CreateTestInterview();
        var createResult = await _interviewRepository.CreateInterviewAsync(testInterview);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new InterviewID(createResult.Value?.Code);

        try
        {
            // Act
            var result = await _interviewRepository.GetAllInterviewsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should successfully get all interviews");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test interview");
            
            // Verify our test interview is in the results
            var interviewIds = result.Value.Select(i => i.Id.Value).ToList();
            interviewIds.Should().Contain(createdId.Value, "Should contain our test interview");
        }
        finally
        {
            // Cleanup
            await CleanupInterviewAsync(createdId);
        }
    }

    [Fact]
    public async Task Repository_UpdateInterviewAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create an interview first
        var testInterview = CreateTestInterview();
        var createResult = await _interviewRepository.CreateInterviewAsync(testInterview);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdInterview = createResult.Value!;
        var interviewId = createdInterview.Id;
        var updatedinterviewId = new InterviewID(createResult.Value?.Code);

        try
        {
            // Modify the interview
            //createdInterview.PersonInterviewed = "REPO_UPDATED - " + createdInterview.PersonInterviewed;
            //createdInterview.InvestigatorNotes = "REPO_UPDATED - " + createdInterview.InvestigatorNotes;
            createdInterview.UpdatedBy = "INTEGRATION_TEST_REPO_UPDATE";
            createdInterview.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _interviewRepository.UpdateInterviewAsync(createdInterview);
            
            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("Repository update should succeed");
            updateResult.Value.Should().NotBeNull();
            //updateResult.Value!.PersonInterviewed.Should().StartWith("REPO_UPDATED - ", "PersonInterviewed should be updated");
            //updateResult.Value.InvestigatorNotes.Should().StartWith("REPO_UPDATED - ", "InvestigatorNotes should be updated");
        }
        finally
        {
            // Cleanup
            await CleanupInterviewAsync(updatedinterviewId);
        }
    }

    [Fact]
    public async Task Repository_DeleteInterviewAsync_WithExistingInterview_ShouldDeleteSuccessfully()
    {
        // Arrange - Create an interview first
        var testInterview = CreateTestInterview();
        var createResult = await _interviewRepository.CreateInterviewAsync(testInterview);
        createResult.IsSuccess.Should().BeTrue();

        var interviewId = new InterviewID(createResult.Value?.Code);

        // Act
        var deleteResult = await _interviewRepository.DeleteInterviewAsync(interviewId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("Repository delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify interview is actually deleted
        var getResult = await _interviewRepository.GetInterviewByIdAsync(interviewId);
        getResult.IsSuccess.Should().BeFalse("Interview should no longer exist after deletion");
    }

    #endregion

    #region DataService Layer Tests

    [Fact]
    public async Task DataService_CreateInterviewAsync_WithValidInterview_ShouldCreateSuccessfully()
    {
        // Arrange
        var testInterview = CreateTestInterview();
        _logger.LogInformation("Creating test interview via DataService with code: {Code}", testInterview.Code);

        // Act
        var result = await _interviewDataService.CreateInterviewAsync(testInterview);
        var interviewId = new InterviewID(result.Value?.Code);
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("DataService creation should succeed");
        result.Value.Should().NotBeNull();
  //      result.Value!.Code.Should().Be(testInterview.Code);
  //      result.Value.PersonInterviewed.Should().Be(testInterview.PersonInterviewed);

        _logger.LogInformation("DataService successfully created interview with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupInterviewAsync(interviewId);
    }

    [Fact]
    public async Task DataService_GetInterviewByIdAsync_WithExistingInterview_ShouldReturnInterview()
    {
        // Arrange - Create interview via DataService
        var testInterview = CreateTestInterview();
        var createResult = await _interviewDataService.CreateInterviewAsync(testInterview);
        createResult.IsSuccess.Should().BeTrue();

        var interviewId = new InterviewID(createResult.Value?.Code);

        try
        {
            // Act
            var result = await _interviewDataService.GetInterviewByIdAsync(interviewId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should find existing interview");
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(interviewId);
        }
        finally
        {
            // Cleanup
            await CleanupInterviewAsync(interviewId);
        }
    }

    [Fact]
    public async Task DataService_GetAllInterviewsAsync_ShouldReturnInterviewsList()
    {
        // Arrange - Create test interview via DataService
        var testInterview = CreateTestInterview();
        var createResult = await _interviewDataService.CreateInterviewAsync(testInterview);
        createResult.IsSuccess.Should().BeTrue();

        var interviewId = new InterviewID(createResult.Value?.Code);

        try
        {
            // Act
            var result = await _interviewDataService.GetAllInterviewsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should successfully get all interviews");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test interview");
            
            var interviewIds = result.Value.Select(i => i.Id.Value).ToList();
            interviewIds.Should().Contain(interviewId.Value, "Should contain our test interview");
        }
        finally
        {
            // Cleanup
            await CleanupInterviewAsync(interviewId);
        }
    }

    [Fact]
    public async Task DataService_UpdateInterviewAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create interview via DataService
        var testInterview = CreateTestInterview();
        var createResult = await _interviewDataService.CreateInterviewAsync(testInterview);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdInterview = createResult.Value!;
        var interviewId = new InterviewID(createResult.Value?.Code);

        try
        {
            // Modify the interview
            createdInterview.PersonInterviewed = "DS_UPDATED - " + createdInterview.PersonInterviewed;
            createdInterview.InvestigatorNotes = "DS_UPDATED - " + createdInterview.InvestigatorNotes;
            createdInterview.UpdatedBy = "INTEGRATION_TEST_DS_UPDATE";
            createdInterview.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _interviewDataService.UpdateInterviewAsync(createdInterview);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("DataService update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.PersonInterviewed.Should().StartWith("DS_UPDATED", "PersonInterviewed should be updated via DataService");
        }
        finally
        {
            // Cleanup
            await CleanupInterviewAsync(interviewId);
        }
    }

    [Fact]
    public async Task DataService_DeleteInterviewAsync_WithExistingInterview_ShouldDeleteSuccessfully()
    {
        // Arrange - Create interview via DataService
        var testInterview = CreateTestInterview();
        var createResult = await _interviewDataService.CreateInterviewAsync(testInterview);
        createResult.IsSuccess.Should().BeTrue();

        var interviewId = new InterviewID(createResult.Value?.Code);

        // Act
        var deleteResult = await _interviewDataService.DeleteInterviewAsync(interviewId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("DataService delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify deletion via repository
        var getResult = await _interviewRepository.GetInterviewByIdAsync(interviewId);
        getResult.IsSuccess.Should().BeFalse("Interview should no longer exist after DataService deletion");
    }

    #endregion

    #region Cross-Layer Integration Tests

    [Fact]
    public async Task CrossLayer_CreateViaDataService_ReadViaRepository_ShouldBeConsistent()
    {
        // Arrange
        var testInterview = CreateTestInterview();
        var testId = GenerateTestId();
        testInterview.PersonInterviewed = $"CrossLayer Test {testId}";

        try
        {
            // Act - Create via DataService
            var createResult = await _interviewDataService.CreateInterviewAsync(testInterview);
            createResult.IsSuccess.Should().BeTrue();

            var createdId = new InterviewID(createResult.Value?.Code);

            // Read via Repository
            var readResult = await _interviewRepository.GetInterviewByIdAsync(createdId);

            // Assert
            readResult.IsSuccess.Should().BeTrue("Repository should read DataService-created interview");
            readResult.Value!.Id.Should().Be(createResult.Value.Id);
            readResult.Value.Code.Should().Be(createResult.Value.Code);
            readResult.Value.PersonInterviewed.Should().Be(createResult.Value.PersonInterviewed);

            // Cleanup
            await CleanupInterviewAsync(createdId);
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
    public async Task Repository_CreateInterviewAsync_WithNullInterview_ShouldReturnFailure()
    {
        // Act
        var result = await _interviewRepository.CreateInterviewAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("Repository creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task DataService_CreateInterviewAsync_WithNullInterview_ShouldReturnFailure()
    {
        // Act
        var result = await _interviewDataService.CreateInterviewAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("DataService creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Performance_CreateMultipleInterviews_ShouldCompleteInReasonableTime()
    {
        const int interviewCount = 3;
        var createdIds = new List<InterviewID>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create multiple interviews via DataService
            for (int i = 0; i < interviewCount; i++)
            {
                var testInterview = CreateTestInterview();
                testInterview.PersonInterviewed = $"Perf Test Person {i + 1}";
                
                var result = await _interviewDataService.CreateInterviewAsync(testInterview);
                result.IsSuccess.Should().BeTrue($"Interview {i + 1} should be created successfully");
                var createdId = new InterviewID(result.Value?.Code);

                createdIds.Add(createdId);
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert performance (adjust threshold as needed)
            elapsedMs.Should().BeLessThan(15000, $"Creating {interviewCount} interviews should complete within 15 seconds");
            
            _logger.LogInformation("Created {Count} interviews in {ElapsedMs}ms (avg: {AvgMs}ms per interview)", 
                interviewCount, elapsedMs, elapsedMs / interviewCount);
        }
        finally
        {
            // Cleanup all created interviews
            foreach (var id in createdIds)
            {
                await CleanupInterviewAsync(id);
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Cleanup helper to delete test interviews
    /// </summary>
    private async Task CleanupInterviewAsync(InterviewID interviewId)
    {
        try
        {
            var deleteResult = await _interviewRepository.DeleteInterviewAsync(interviewId);
            if (deleteResult.IsSuccess)
            {
                _logger.LogDebug("Cleaned up test interview: {Id}", interviewId.Value);
            }
            else
            {
                _logger.LogWarning("Failed to cleanup test interview: {Id}", interviewId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup of test interview: {Id}", interviewId.Value);
        }
    }

    #endregion
}