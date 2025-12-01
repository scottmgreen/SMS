using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;

namespace SMS_Application.Services;

/// <summary>
/// High-level application service for Airport Shared Dataset business operations
/// Provides business logic orchestration and cross-cutting concerns for SMS compliance data
/// </summary>
public sealed class AirportSharedDatasetService
{
    private readonly AirportSharedDatasetDataService _dataService;
    private readonly ILogger<AirportSharedDatasetService> _logger;

    public AirportSharedDatasetService(AirportSharedDatasetDataService dataService, ILogger<AirportSharedDatasetService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new Airport Shared Dataset with business validation
    /// </summary>
    public async Task<Result<AirportSharedDataset>> CreateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating Airport Shared Dataset with code: {Code}", airportSharedDataset?.Code);

            // Business validation - ensure dataset is not null
            if (airportSharedDataset is null)
            {
                _logger.LogError("CreateAirportSharedDatasetAsync received null dataset");
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            // Business validation - ensure ReportID is provided (critical SMS requirement)
            if (string.IsNullOrWhiteSpace(airportSharedDataset.ReportCode))
            {
                _logger.LogError("CreateAirportSharedDatasetAsync received dataset without ReportID");
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.ReportIDRequired);
            }

            // Business validation - ensure HazardCode is provided (critical SMS requirement)
            if (string.IsNullOrWhiteSpace(airportSharedDataset.HazardCode))
            {
                _logger.LogError("CreateAirportSharedDatasetAsync received dataset without HazardCode");
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.HazardCodeRequired);
            }

            var result = await _dataService.CreateAirportSharedDatasetAsync(airportSharedDataset, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created Airport Shared Dataset with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create Airport Shared Dataset. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Airport Shared Dataset");
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets Airport Shared Dataset by ID
    /// </summary>
    public async Task<Result<AirportSharedDataset>> GetAirportSharedDatasetByIdAsync(AirportSharedDatasetID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving Airport Shared Dataset with ID: {Id}", id);
            return await _dataService.GetAirportSharedDatasetByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving Airport Shared Dataset with ID: {Id}", id);
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NotFound);
        }
    }

    /// <summary>
    /// Gets all Airport Shared Datasets
    /// </summary>
    public async Task<Result<List<AirportSharedDataset>>> GetAllAirportSharedDatasetsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all Airport Shared Datasets");
            return await _dataService.GetAllAirportSharedDatasetsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all Airport Shared Datasets");
            return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
        }
    }

    /// <summary>
    /// Updates an existing Airport Shared Dataset with business validation
    /// </summary>
    public async Task<Result<AirportSharedDataset>> UpdateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating Airport Shared Dataset with Code: {Code}", airportSharedDataset?.Code);

            if (airportSharedDataset is null)
            {
                _logger.LogError("UpdateAirportSharedDatasetAsync received null dataset");
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            // Business validation - check if dataset exists
            var datasetId = new AirportSharedDatasetID(airportSharedDataset.Code);
            var existingDatasetResult = await _dataService.GetAirportSharedDatasetByIdAsync(datasetId, ct).ConfigureAwait(false);
            if (existingDatasetResult.IsFailure)
            {
                _logger.LogWarning("Cannot update non-existent Airport Shared Dataset with Code: {Code}", airportSharedDataset.Code);
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NotFound);
            }

            // Business validation - ensure critical fields are not being cleared
            if (string.IsNullOrWhiteSpace(airportSharedDataset.ReportCode))
            {
                _logger.LogError("UpdateAirportSharedDatasetAsync attempt to clear ReportCode - not allowed");
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.ReportIDRequired);
            }

            if (string.IsNullOrWhiteSpace(airportSharedDataset.HazardCode))
            {
                _logger.LogError("UpdateAirportSharedDatasetAsync attempt to clear HazardCode - not allowed");
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.HazardCodeRequired);
            }

            var result = await _dataService.UpdateAirportSharedDatasetAsync(airportSharedDataset, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Airport Shared Dataset with Code: {Code}", airportSharedDataset.Code);
            }
            else
            {
                _logger.LogError("Failed to update Airport Shared Dataset. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating Airport Shared Dataset with Code: {Code}", airportSharedDataset?.Code);
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an Airport Shared Dataset with business validation
    /// </summary>
    public async Task<Result<bool>> DeleteAirportSharedDatasetAsync(AirportSharedDatasetID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting Airport Shared Dataset with ID: {Id}", id);

            // Business validation - check if dataset exists
            var existingDatasetResult = await _dataService.GetAirportSharedDatasetByIdAsync(id, ct).ConfigureAwait(false);
            if (existingDatasetResult.IsFailure)
            {
                _logger.LogWarning("Cannot delete non-existent Airport Shared Dataset with ID: {Id}", id);
                return Result<bool>.Failure<bool>(DomainErrors.AirportSharedDatasetError.NotFound);
            }

            var result = await _dataService.DeleteAirportSharedDatasetAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Airport Shared Dataset with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete Airport Shared Dataset. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting Airport Shared Dataset with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.AirportSharedDatasetError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets Airport Shared Datasets by Report ID with business logic
    /// </summary>
    public async Task<Result<List<AirportSharedDataset>>> GetAirportSharedDatasetsByReportIdAsync(string reportId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving Airport Shared Datasets for Report ID: {ReportId}", reportId);

            if (string.IsNullOrWhiteSpace(reportId))
            {
                _logger.LogWarning("GetAirportSharedDatasetsByReportIdAsync called with empty ReportId");
                return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(DomainErrors.AirportSharedDatasetError.ReportIDRequired);
            }

            // Get all datasets and filter by ReportID
            var allDatasetsResult = await _dataService.GetAllAirportSharedDatasetsAsync(ct).ConfigureAwait(false);
            if (allDatasetsResult.IsFailure)
            {
                return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(allDatasetsResult.Error);
            }

            var filteredDatasets = allDatasetsResult.Value
                .Where(d => d.ReportCode.Equals(reportId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation("Found {Count} Airport Shared Datasets for Report ID: {ReportId}", 
                filteredDatasets.Count, reportId);

            return Result<List<AirportSharedDataset>>.Success(filteredDatasets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving Airport Shared Datasets for Report ID: {ReportId}", reportId);
            return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(DomainErrors.AirportSharedDatasetError.NotFound);
        }
    }

    /// <summary>
    /// Gets Airport Shared Datasets by Hazard Code with business logic
    /// </summary>
    public async Task<Result<List<AirportSharedDataset>>> GetAirportSharedDatasetsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving Airport Shared Datasets for Hazard Code: {HazardCode}", hazardCode);

            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                _logger.LogWarning("GetAirportSharedDatasetsByHazardCodeAsync called with empty HazardCode");
                return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(DomainErrors.AirportSharedDatasetError.HazardCodeRequired);
            }

            // Get all datasets and filter by HazardCode
            var allDatasetsResult = await _dataService.GetAllAirportSharedDatasetsAsync(ct).ConfigureAwait(false);
            if (allDatasetsResult.IsFailure)
            {
                return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(allDatasetsResult.Error);
            }

            var filteredDatasets = allDatasetsResult.Value
                .Where(d => d.HazardCode.Equals(hazardCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation("Found {Count} Airport Shared Datasets for Hazard Code: {HazardCode}", 
                filteredDatasets.Count, hazardCode);

            return Result<List<AirportSharedDataset>>.Success(filteredDatasets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving Airport Shared Datasets for Hazard Code: {HazardCode}", hazardCode);
            return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(DomainErrors.AirportSharedDatasetError.NotFound);
        }
    }

    /// <summary>
    /// Validates the Airport Shared Dataset for SMS compliance
    /// </summary>
    public async Task<Result<bool>> ValidateDatasetForComplianceAsync(AirportSharedDataset dataset, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Validating Airport Shared Dataset for SMS compliance: {Code}", dataset?.Code);

            if (dataset is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            var validationErrors = new List<string>();

            // Critical compliance checks
            if (string.IsNullOrWhiteSpace(dataset.ReportCode))
                validationErrors.Add("ReportCode is required for SMS compliance");

            if (string.IsNullOrWhiteSpace(dataset.HazardCode))
                validationErrors.Add("HazardCode is required for SMS compliance");

            if (string.IsNullOrWhiteSpace(dataset.SharedNarrative))
                validationErrors.Add("SharedNarrative is required for SMS compliance reporting");

            // Business rule validations
            if (!string.IsNullOrWhiteSpace(dataset.PrivateNarrative) && 
                dataset.PrivateNarrative.Length > 4000)
                validationErrors.Add("PrivateNarrative exceeds maximum length (4000 characters)");

            if (!string.IsNullOrWhiteSpace(dataset.SharedNarrative) && 
                dataset.SharedNarrative.Length > 4000)
                validationErrors.Add("SharedNarrative exceeds maximum length (4000 characters)");

            if (validationErrors.Any())
            {
                _logger.LogWarning("Dataset validation failed for Code: {Code}. Errors: {Errors}", 
                    dataset.Code, string.Join(", ", validationErrors));
                return Result<bool>.Failure<bool>(DomainErrors.AirportSharedDatasetError.InvalidNarrative);
            }

            _logger.LogInformation("Dataset validation passed for Code: {Code}", dataset.Code);
            return Result<bool>.Success(true);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during dataset validation");
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}