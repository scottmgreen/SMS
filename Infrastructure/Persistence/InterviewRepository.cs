using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Enhanced Interview Repository with comprehensive interview management capabilities
/// </summary>
public sealed class InterviewRepository : BaseRepository<InterviewRepository, Interview>, IInterviewRepository
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
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} Enhanced Interview Repository Initialized");
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

            // Core properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCode, interview.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigationCode, interview.InvestigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewSMSInvestigatorCode, interview.SMSInvestigatorCode));
            
            // Interview details
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewed, interview.PersonInterviewed));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonRole, interview.PersonInterviewedRole ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonDepartment, interview.PersonInterviewedDepartment ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewedNotes, interview.PersonInterviewedNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigatorNotes, interview.InvestigatorNotes ?? (object)DBNull.Value));
            
            // Status and scheduling
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewStatus, interview.Status.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewDate, interview.InterviewDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewDurationMinutes, interview.DurationMinutes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewLocation, interview.InterviewLocation ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewType, interview.Type.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewIsConfidential, interview.IsConfidential));
            
            // Preparation properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPreparationNotes, interview.PreparationNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewQuestionsToAsk, interview.QuestionsToAsk ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewBackgroundInformation, interview.BackgroundInformation ?? (object)DBNull.Value));
            
            // Results properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewKeyFindings, interview.KeyFindings ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewFollowUpRequired, interview.FollowUpRequired ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewAdditionalWitnesses, interview.AdditionalWitnesses ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCompletedDate, interview.CompletedDate ?? (object)DBNull.Value));
            
            // Audit properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, interview.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, interview.CreatedDate));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewInterviewCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            InterviewID interviewId = new(newCodeValue);

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

    public async Task<Result<Interview>> GetInterviewByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.CodeRequired);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Interview_GetByCode} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCode, code));

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

    public async Task<Result<IEnumerable<Interview>>> GetByInvestigationAsync(string investigationCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(investigationCode))
            {
                return Result<IEnumerable<Interview>>.Failure<IEnumerable<Interview>>(DomainErrors.InterviewError.InvestigationCodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Interview_GetByInvestigation} InvestigationCode:{investigationCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_GetByInvestigation, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigationCode, investigationCode));

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

            return Result<IEnumerable<Interview>>.Success(response.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<Interview>>.Failure<IEnumerable<Interview>>(DomainErrors.InterviewError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<Interview>>> GetByInvestigatorAsync(string investigatorCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(investigatorCode))
            {
                return Result<IEnumerable<Interview>>.Failure<IEnumerable<Interview>>(DomainErrors.InterviewError.InvestigatorRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Interview_GetByInvestigator} InvestigatorCode:{investigatorCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_GetByInvestigator, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewSMSInvestigatorCode, investigatorCode));

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

            return Result<IEnumerable<Interview>>.Success(response.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<Interview>>.Failure<IEnumerable<Interview>>(DomainErrors.InterviewError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<Interview>>> GetByStatusAsync(InterviewStatus status, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Interview_GetByStatus} Status:{status}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_GetByStatus, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewStatus, status.ToString()));

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

            return Result<IEnumerable<Interview>>.Success(response.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<Interview>>.Failure<IEnumerable<Interview>>(DomainErrors.InterviewError.NotFound);
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

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Interview_Update} Code:{interview.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // All enhanced properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCode, interview.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigationCode, interview.InvestigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewSMSInvestigatorCode, interview.SMSInvestigatorCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewed, interview.PersonInterviewed));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonRole, interview.PersonInterviewedRole ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonDepartment, interview.PersonInterviewedDepartment ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewedNotes, interview.PersonInterviewedNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigatorNotes, interview.InvestigatorNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewStatus, interview.Status.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewDate, interview.InterviewDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewDurationMinutes, interview.DurationMinutes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewLocation, interview.InterviewLocation ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewType, interview.Type.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewIsConfidential, interview.IsConfidential));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPreparationNotes, interview.PreparationNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewQuestionsToAsk, interview.QuestionsToAsk ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewBackgroundInformation, interview.BackgroundInformation ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewKeyFindings, interview.KeyFindings ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewFollowUpRequired, interview.FollowUpRequired ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewAdditionalWitnesses, interview.AdditionalWitnesses ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCompletedDate, interview.CompletedDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, interview.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, interview.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetInterviewByCodeAsync(interview.Code, ct).ConfigureAwait(false);
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

    // Additional specialized methods for interview workflow
    public async Task<Result<bool>> UpdateStatusAsync(string interviewCode, InterviewStatus status, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(interviewCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CodeRequired);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Interview_UpdateStatus} Code:{interviewCode}, Status:{status}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_UpdateStatus, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCode, interviewCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewStatus, status.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> ScheduleInterviewAsync(string interviewCode, DateTime interviewDate, string location, 
        int? durationMinutes = null, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(interviewCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CodeRequired);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Interview_Schedule} Code:{interviewCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_Schedule, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCode, interviewCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewDate, interviewDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewLocation, location));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewDurationMinutes, durationMinutes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewStatus, InterviewStatus.Scheduled.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> CompleteInterviewAsync(string interviewCode, string? personNotes, string? investigatorNotes, 
        string? keyFindings = null, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(interviewCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CodeRequired);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Interview_Complete} Code:{interviewCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Interview_Complete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCode, interviewCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewPersonInterviewedNotes, personNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewInvestigatorNotes, investigatorNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewKeyFindings, keyFindings ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewStatus, InterviewStatus.Completed.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInterviewCompletedDate, DateTime.UtcNow));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.UpdateFailed);
        }
    }
}