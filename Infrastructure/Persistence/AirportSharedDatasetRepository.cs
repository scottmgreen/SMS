using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Persistence;

public sealed class AirportSharedDatasetRepository : BaseRepository<AirportSharedDatasetRepository, AirportSharedDataset>
{
    private readonly ILogger<AirportSharedDatasetRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public AirportSharedDatasetRepository(ILogger<AirportSharedDatasetRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} AirportSharedDataset Repository Initialized");
    }

    public async Task<Result<AirportSharedDataset>> CreateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default)
    {
        try
        {
            if (airportSharedDataset is null)
            {
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            // Validate REQUIRED ReportID
            if (string.IsNullOrWhiteSpace(airportSharedDataset.ReportID))
            {
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.ReportIDRequired);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_AirportSharedDataset_Insert} Code:{airportSharedDataset.Code}, ReportID:{airportSharedDataset.ReportID}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_AirportSharedDataset_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add all parameters - REQUIRED ReportID first
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirportSharedDatasetCode, airportSharedDataset.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirportSharedDatasetReportCode, airportSharedDataset.ReportID));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirportSharedDatasetHazardCode, airportSharedDataset.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPrivateNarrative, airportSharedDataset.PrivateNarrative));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSharedNarrative, airportSharedDataset.SharedNarrative));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLocationArea, airportSharedDataset.LocationArea));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLocationSubArea, airportSharedDataset.LocationSubArea));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLocationOther, airportSharedDataset.LocationOther));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmWeather, airportSharedDataset.Weather));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTriggeringEvent, airportSharedDataset.TriggeringEvent));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAircraftInvolved, airportSharedDataset.AircraftInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPoweredEquipmentInvolved, airportSharedDataset.PoweredEquipmentInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmNonPoweredEquipmentInvolved, airportSharedDataset.NonPoweredEquipmentInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPedestrianInvolved, airportSharedDataset.PedestrianInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOtherInvolved, airportSharedDataset.OtherInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOtherDescription, airportSharedDataset.OtherDescription));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPropertyDamage, airportSharedDataset.PropertyDamage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPropertyDamageComments, airportSharedDataset.PropertyDamageComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPersonalInjury, airportSharedDataset.PersonalInjury));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPersonalInjuryComments, airportSharedDataset.PersonalInjuryComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFatality, airportSharedDataset.Fatality));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFatalityComments, airportSharedDataset.FatalityComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOtherIssues, airportSharedDataset.OtherIssues));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOtherIssuesDescription, airportSharedDataset.OtherIssuesDescription));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirlineCompanyOperator, airportSharedDataset.AirlineCompanyOperator));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOperatorsAuthorized, airportSharedDataset.OperatorsAuthorized));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFlightDelay, airportSharedDataset.FlightDelay));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFlightDelayDetails, airportSharedDataset.FlightDelayDetails));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEquipmentRemovedFromService, airportSharedDataset.EquipmentRemovedFromService));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEquipmentRemovalDetails, airportSharedDataset.EquipmentRemovalDetails));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPoliceReport, airportSharedDataset.PoliceReport));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPoliceReportDetails, airportSharedDataset.PoliceReportDetails));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmContributingFactors, airportSharedDataset.ContributingFactors));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFactorsOtherDescription, airportSharedDataset.FactorsOtherDescription));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewAirportSharedDatasetCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            AirportSharedDatasetID datasetId = new (newCodeValue);

            return await GetAirportSharedDatasetByIdAsync(datasetId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.CreateFailed);
        }
    }

    public async Task<Result<AirportSharedDataset>> GetAirportSharedDatasetByIdAsync(AirportSharedDatasetID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_AirportSharedDataset_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_AirportSharedDataset_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirportSharedDatasetId, id.Value.ToString()));

            AirportSharedDataset? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToAirportSharedDataset(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<AirportSharedDataset>.Success(response);
            }
            else
            {
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<AirportSharedDataset>>> GetAllAirportSharedDatasetsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_AirportSharedDataset_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_AirportSharedDataset_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<AirportSharedDataset> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var dataset = Mappers.MapToAirportSharedDataset(reader);
                    response.Add(dataset);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<AirportSharedDataset>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
        }
    }

    public async Task<Result<AirportSharedDataset>> UpdateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default)
    {
        try
        {
            if (airportSharedDataset is null)
            {
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            // Validate REQUIRED ReportID
            if (string.IsNullOrWhiteSpace(airportSharedDataset.ReportID))
            {
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.ReportIDRequired);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_AirportSharedDataset_Update} ID:{airportSharedDataset.Id}, ReportID:{airportSharedDataset.ReportID}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_AirportSharedDataset_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add all parameters for update
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, airportSharedDataset.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirportSharedDatasetCode, airportSharedDataset.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirportSharedDatasetReportCode, airportSharedDataset.ReportID));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirportSharedDatasetHazardCode, airportSharedDataset.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPrivateNarrative, airportSharedDataset.PrivateNarrative));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSharedNarrative, airportSharedDataset.SharedNarrative));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLocationArea, airportSharedDataset.LocationArea));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLocationSubArea, airportSharedDataset.LocationSubArea));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLocationOther, airportSharedDataset.LocationOther));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmWeather, airportSharedDataset.Weather));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTriggeringEvent, airportSharedDataset.TriggeringEvent));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAircraftInvolved, airportSharedDataset.AircraftInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPoweredEquipmentInvolved, airportSharedDataset.PoweredEquipmentInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmNonPoweredEquipmentInvolved, airportSharedDataset.NonPoweredEquipmentInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPedestrianInvolved, airportSharedDataset.PedestrianInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOtherInvolved, airportSharedDataset.OtherInvolved));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOtherDescription, airportSharedDataset.OtherDescription));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPropertyDamage, airportSharedDataset.PropertyDamage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPropertyDamageComments, airportSharedDataset.PropertyDamageComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPersonalInjury, airportSharedDataset.PersonalInjury));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPersonalInjuryComments, airportSharedDataset.PersonalInjuryComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFatality, airportSharedDataset.Fatality));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFatalityComments, airportSharedDataset.FatalityComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOtherIssues, airportSharedDataset.OtherIssues));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOtherIssuesDescription, airportSharedDataset.OtherIssuesDescription));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAirlineCompanyOperator, airportSharedDataset.AirlineCompanyOperator));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmOperatorsAuthorized, airportSharedDataset.OperatorsAuthorized));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFlightDelay, airportSharedDataset.FlightDelay));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFlightDelayDetails, airportSharedDataset.FlightDelayDetails));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEquipmentRemovedFromService, airportSharedDataset.EquipmentRemovedFromService));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEquipmentRemovalDetails, airportSharedDataset.EquipmentRemovalDetails));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPoliceReport, airportSharedDataset.PoliceReport));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPoliceReportDetails, airportSharedDataset.PoliceReportDetails));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmContributingFactors, airportSharedDataset.ContributingFactors));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFactorsOtherDescription, airportSharedDataset.FactorsOtherDescription));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetAirportSharedDatasetByIdAsync((AirportSharedDatasetID)airportSharedDataset.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteAirportSharedDatasetAsync(AirportSharedDatasetID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_AirportSharedDataset_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_AirportSharedDataset_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.AirportSharedDatasetError.DeleteFailed);
        }
    }
}