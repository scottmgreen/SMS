using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Persistence;

public sealed class ReportRepository : BaseRepository<ReportRepository, Report>
{
    private readonly ILogger<ReportRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public ReportRepository(ILogger<ReportRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} Report Repository Initialized");
    }

    public async Task<Result<Report>> CreateReportAsync(Report report, CancellationToken ct = default)
    {
        try
        {
            if (report is null)
            {
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_Report_Insert} Code:{report.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Report_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportCode, report.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportName, report.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportedBy, report.ReportedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportedOn, report.ReportedOn));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportedByDepartment, report.Department));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportDescription, report.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportStatus, report.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportStage, report.Stage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, report.CreatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewReportCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            ReportID reportId = new (newCodeValue);

            return await GetReportByCodeAsync(reportId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.CreateFailed);
        }
    }

    public async Task<Result<Report>> GetReportByCodeAsync(ReportID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Report_GetByCode} {code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Report_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            Report? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToReport(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<Report>.Success(response);
            }
            else
            {
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<Report>.Failure<Report>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<Report>>> GetAllReportsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Report_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Report_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<Report> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var report = Mappers.MapToReport(reader);
                    response.Add(report);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<Report>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<Report>>.Failure<List<Report>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }

    public async Task<Result<Report>> UpdateReportAsync(Report report, CancellationToken ct = default)
    {
        try
        {
            if (report is null)
            {
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Report_Update} Code:{report.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Report_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, report.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportName, report.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportDescription, report.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportStatus, report.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportStage, report.Stage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, report.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetReportByCodeAsync((ReportID)report.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteReportAsync(ReportID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_Report_Delete} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Report_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }
}