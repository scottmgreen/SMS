using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Repositories;

public sealed class InterviewRepository : BaseRepository<InterviewRepository, Interview>
{
    private readonly ILogger<InterviewRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public InterviewRepository(ILogger<InterviewRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} Interview Repository Initialized");
    }

    public async Task<Result<Interview>> CreateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        try
        {
            if (interview is null)
            {
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_Interview_Insert} Code:{interview.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCode, interview.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigationCode, interview.InvestigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewSMSInvestigatorCode, interview.SMSInvestigatorCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewed, interview.PersonInterviewed));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewedNotes, interview.PersonInterviewedNotes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigatorNotes, interview.InvestigatorNotes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewInterviewCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            InterviewID interviewId = new (newCodeValue);

            return await GetInterviewByIdAsync(interviewId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.CreateFailed);
        }
    }

    public async Task<Result<Interview>> GetInterviewByIdAsync(InterviewID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Interview_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

            Interview? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToInterview(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<Interview>.Success(response);
            }
            else
            {
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<Interview>.Failure<Interview>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<Interview>>> GetAllInterviewsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Interview_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<Interview> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var interview = Mappers.MapToInterview(reader);
                    response.Add(interview);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<Interview>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<Interview>>.Failure<List<Interview>>(DomainErrors.InterviewError.NullOrEmpty);
        }
    }

    public async Task<Result<Interview>> UpdateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        try
        {
            if (interview is null)
            {
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Interview_Update} ID:{interview.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, interview.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCode, interview.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigationCode, interview.InvestigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewSMSInvestigatorCode, interview.SMSInvestigatorCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewed, interview.PersonInterviewed));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewedNotes, interview.PersonInterviewedNotes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigatorNotes, interview.InvestigatorNotes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetInterviewByIdAsync((InterviewID)interview.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteInterviewAsync(InterviewID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_Interview_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_Delete, sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.DeleteFailed);
        }
    }
}