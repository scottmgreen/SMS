using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Services;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Database Integration Tests for Report CRUD Operations
/// Tests both Repository and DataService layers against the actual PDXSMS_V2 database
/// </summary>
[Collection("Database Integration Tests")]
public class ReportDatabaseIntegrationTests : DatabaseTestBase
{
    private readonly ReportRepository _reportRepository;
    private readonly ReportDataService _reportDataService;

    public ReportDatabaseIntegrationTests()
    {
        _reportRepository = GetReportRepository();
        _reportDataService = GetReportDataService();
    }

    #region Setup and Connection Tests

    [Fact]
    public async Task DatabaseConnection_ShouldBeValid()
    {
        // Act
        var isValid = await ValidateDatabaseConnectionAsync();

        // Assert
        isValid.Should().BeTrue("Database connection should be valid");
        _logger.LogInformation("Database connection validation passed for Report tests");
    }

    [Fact]
    public void DependencyInjection_ShouldResolveServices()
    {
        // Act & Assert
        _reportRepository.Should().NotBeNull("ReportRepository should be resolved");
        _reportDataService.Should().NotBeNull("ReportDataService should be resolved");
        
        _logger.LogInformation("Dependency injection validation passed for Report services");
    }

    #endregion

    #region Repository Layer Tests

    [Fact]
    public async Task Repository_CreateReportAsync_WithValidReport_ShouldCreateSuccessfully()
    {
        // Arrange
        var testReport = CreateTestReport();
        _logger.LogInformation("Creating test report via Repository with code: {Code}", testReport.Code);

        // Act
        var result = await _reportRepository.CreateReportAsync(testReport);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("Repository creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().NotBe(testReport.Code);
        result.Value.Name.Should().Be(testReport.Name);
        result.Value.Description.Should().Be(testReport.Description);

        _logger.LogInformation("Repository successfully created report with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupReportAsync(new ReportID(result.Value.Code));
    }

    [Fact]
    public async Task Repository_GetReportByIdAsync_WithExistingReport_ShouldReturnReport()
    {
        // Arrange - Create a report first
        var testReport = CreateTestReport();
        var createResult = await _reportRepository.CreateReportAsync(testReport);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdReportId = new ReportID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _reportRepository.GetReportByIdAsync(createdReportId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should find existing report");
            result.Value.Should().NotBeNull();
            result.Value!.Code.Should().Be(createdReportId.Value);
            result.Value.Code.Should().NotBe(testReport.Code);
        }
        finally
        {
            // Cleanup
            await CleanupReportAsync(createdReportId);
        }
    }

    [Fact]
    public async Task Repository_GetAllReportsAsync_ShouldReturnReportsList()
    {
        // Arrange - Create test report
        var testReport = CreateTestReport();
        var createResult = await _reportRepository.CreateReportAsync(testReport);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new ReportID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _reportRepository.GetAllReportsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("Repository should successfully get all reports");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test report");
            
            // Verify our test report is in the results
            var reportCodes = result.Value.Select(r => r.Code).ToList();
            reportCodes.Should().Contain(createdId.Value, "Should contain our test report");
        }
        finally
        {
            // Cleanup
            await CleanupReportAsync(createdId);
        }
    }

    [Fact]
    public async Task Repository_UpdateReportAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a report first
        var testReport = CreateTestReport();
        var createResult = await _reportRepository.CreateReportAsync(testReport);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdReport = createResult.Value!;
        var reportId = new ReportID(createdReport.Code);

        try
        {
            // Modify the report
            createdReport.Name = "REPO_UPDATED - " + createdReport.Name;
            createdReport.Description = "REPO_UPDATED - " + createdReport.Description;
            createdReport.UpdatedBy = "INTEGRATION_TEST_REPO_UPDATE";
            createdReport.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _reportRepository.UpdateReportAsync(createdReport);

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
            await CleanupReportAsync(reportId);
        }
    }

    [Fact]
    public async Task Repository_DeleteReportAsync_WithExistingReport_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a report first
        var testReport = CreateTestReport();
        var createResult = await _reportRepository.CreateReportAsync(testReport);
        createResult.IsSuccess.Should().BeTrue();
        
        var reportId = new ReportID(createResult.Value!.Code);

        // Act
        var deleteResult = await _reportRepository.DeleteReportAsync(reportId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("Repository delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify report is actually deleted
        var getResult = await _reportRepository.GetReportByIdAsync(reportId);
        getResult.IsSuccess.Should().BeFalse("Report should no longer exist after deletion");
    }

    #endregion

    #region DataService Layer Tests

    [Fact]
    public async Task DataService_CreateReportAsync_WithValidReport_ShouldCreateSuccessfully()
    {
        // Arrange
        var testReport = CreateTestReport();
        _logger.LogInformation("Creating test report via DataService with code: {Code}", testReport.Code);

        // Act
        var result = await _reportDataService.CreateReportAsync(testReport);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue("DataService creation should succeed");
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().NotBe(testReport.Code);
        result.Value.Name.Should().Be(testReport.Name);

        _logger.LogInformation("DataService successfully created report with ID: {Id}", result.Value.Id);

        // Cleanup
        await CleanupReportAsync(new ReportID(result.Value.Code));
    }

    [Fact]
    public async Task DataService_GetReportByIdAsync_WithExistingReport_ShouldReturnReport()
    {
        // Arrange - Create report via DataService
        var testReport = CreateTestReport();
        var createResult = await _reportDataService.CreateReportAsync(testReport);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdReportId = new ReportID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _reportDataService.GetReportByIdAsync(createdReportId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should find existing report");
            result.Value.Should().NotBeNull();
            result.Value!.Code.Should().Be(createdReportId.Value);
        }
        finally
        {
            // Cleanup
            await CleanupReportAsync(createdReportId);
        }
    }

    [Fact]
    public async Task DataService_GetAllReportsAsync_ShouldReturnReportsList()
    {
        // Arrange - Create test report via DataService
        var testReport = CreateTestReport();
        var createResult = await _reportDataService.CreateReportAsync(testReport);
        createResult.IsSuccess.Should().BeTrue();

        var createdId = new ReportID(createResult.Value!.Code);

        try
        {
            // Act
            var result = await _reportDataService.GetAllReportsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("DataService should successfully get all reports");
            result.Value.Should().NotBeNull();
            result.Value!.Should().NotBeEmpty("Should contain at least our test report");
            
            var reportCodes = result.Value.Select(r => r.Code).ToList();
            reportCodes.Should().Contain(createdId.Value, "Should contain our test report");
        }
        finally
        {
            // Cleanup
            await CleanupReportAsync(createdId);
        }
    }

    [Fact]
    public async Task DataService_UpdateReportAsync_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange - Create report via DataService
        var testReport = CreateTestReport();
        var createResult = await _reportDataService.CreateReportAsync(testReport);
        createResult.IsSuccess.Should().BeTrue();
        
        var createdReport = createResult.Value!;
        var reportId = new ReportID(createdReport.Code);

        try
        {
            // Modify the report
            createdReport.Name = "DS_UPDATED" + createdReport.Name;
            createdReport.Description = "DS_UPDATED" + createdReport.Description;
            createdReport.UpdatedBy = "INTEGRATION_TEST_DS_UPDATE";
            createdReport.UpdatedDate = DateTime.UtcNow;

            // Act
            var updateResult = await _reportDataService.UpdateReportAsync(createdReport);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue("DataService update should succeed");
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.Name.Should().StartWith("DS_UPDATED", "Name should be updated via DataService");
        }
        finally
        {
            // Cleanup
            await CleanupReportAsync(reportId);
        }
    }

    [Fact]
    public async Task DataService_DeleteReportAsync_WithExistingReport_ShouldDeleteSuccessfully()
    {
        // Arrange - Create report via DataService
        var testReport = CreateTestReport();
        var createResult = await _reportDataService.CreateReportAsync(testReport);
        createResult.IsSuccess.Should().BeTrue();
        
        var reportId = new ReportID(createResult.Value!.Code);

        // Act
        var deleteResult = await _reportDataService.DeleteReportAsync(reportId);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue("DataService delete should succeed");
        deleteResult.Value.Should().BeTrue();

        // Verify deletion via repository
        var getResult = await _reportRepository.GetReportByIdAsync(reportId);
        getResult.IsSuccess.Should().BeFalse("Report should no longer exist after DataService deletion");
    }

    #endregion

    #region Cross-Layer Integration Tests

    [Fact]
    public async Task CrossLayer_CreateViaDataService_ReadViaRepository_ShouldBeConsistent()
    {
        // Arrange
        var testReport = CreateTestReport();
        var testId = GenerateTestId();
        testReport.Name = $"CrossLayer Test {testId}";

        try
        {
            // Act - Create via DataService
            var createResult = await _reportDataService.CreateReportAsync(testReport);
            createResult.IsSuccess.Should().BeTrue();
            
            var createdId = new ReportID(createResult.Value!.Code);

            // Read via Repository
            var readResult = await _reportRepository.GetReportByIdAsync(createdId);

            // Assert
            readResult.IsSuccess.Should().BeTrue("Repository should read DataService-created report");
            readResult.Value!.Code.Should().Be(createResult.Value.Code);
            readResult.Value.Name.Should().Be(createResult.Value.Name);

            // Cleanup
            await CleanupReportAsync(createdId);
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
    public async Task Repository_CreateReportAsync_WithNullReport_ShouldReturnFailure()
    {
        // Act
        var result = await _reportRepository.CreateReportAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("Repository creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task DataService_CreateReportAsync_WithNullReport_ShouldReturnFailure()
    {
        // Act
        var result = await _reportDataService.CreateReportAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse("DataService creation with null should fail");
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Performance_CreateMultipleReports_ShouldCompleteInReasonableTime()
    {
        const int reportCount = 3;
        var createdIds = new List<ReportID>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create multiple reports via DataService
            for (int i = 0; i < reportCount; i++)
            {
                var testReport = CreateTestReport();
                testReport.Name = $"Perf Test Report {i + 1}";
                
                var result = await _reportDataService.CreateReportAsync(testReport);
                result.IsSuccess.Should().BeTrue($"Report {i + 1} should be created successfully");
                
                createdIds.Add(new ReportID(result.Value!.Code));
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert performance (adjust threshold as needed)
            elapsedMs.Should().BeLessThan(15000, $"Creating {reportCount} reports should complete within 15 seconds");
            
            _logger.LogInformation("Created {Count} reports in {ElapsedMs}ms (avg: {AvgMs}ms per report)", 
                reportCount, elapsedMs, elapsedMs / reportCount);
        }
        finally
        {
            // Cleanup all created reports
            foreach (var id in createdIds)
            {
                await CleanupReportAsync(id);
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Cleanup helper to delete test reports
    /// </summary>
    private async Task CleanupReportAsync(ReportID reportId)
    {
        try
        {
            var deleteResult = await _reportRepository.DeleteReportAsync(reportId);
            if (deleteResult.IsSuccess)
            {
                _logger.LogDebug("Cleaned up test report: {Id}", reportId.Value);
            }
            else
            {
                _logger.LogWarning("Failed to cleanup test report: {Id}", reportId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup of test report: {Id}", reportId.Value);
        }
    }

    #endregion
}