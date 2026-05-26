//-----------------------------------------------------------------------
// <copyright file="EventQueueRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing persisted EventQueue operations.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.ValueObjects;
using SMS_Domain.Errors;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

public sealed class EventQueueRepository : BaseRepository<EventQueueRepository, QueuedEvent>, IEventQueueRepository
{
    private readonly ILogger<EventQueueRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public EventQueueRepository(ILogger<EventQueueRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} EventQueue Repository Initialized");
    }

    public async Task<Result<QueuedEvent>> CreateAsync(QueuedEvent queuedEvent, CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueCode, queuedEvent.Id.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueGuid, queuedEvent.Id.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventCategory, (int)queuedEvent.EventCategory));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventType, queuedEvent.EventType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventData, queuedEvent.EventData));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueReportCode, string.IsNullOrWhiteSpace(queuedEvent.ReportId) ? DBNull.Value : queuedEvent.ReportId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTargetSystem, queuedEvent.TargetSystem ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventPriority, (int)queuedEvent.Priority));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMaxAttempts, 5));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmQueuedBy, queuedEvent.QueuedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueCreatedBy, queuedEvent.QueuedBy ?? "SYSTEM"));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetByQueueGuidAsync(queuedEvent.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<QueuedEvent>.Failure<QueuedEvent>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<QueuedEvent>> LeaseByQueueGuidAsync(Guid queueGuid, string worker, int lockSeconds = 60, CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            await sql.OpenAsync(ct).ConfigureAwait(false);

            using (SqlCommand updateCmd = new(
                @"UPDATE dbo.tbld_EventQueue
                     SET fldi_Status = 1,
                         fldd_ProcessingStartedDate = GETUTCDATE(),
                         fldv_LockedBy = @pWorker,
                         fldd_LockExpiresDate = DATEADD(SECOND, @pLockSeconds, GETUTCDATE()),
                         fldv_UpdatedBy = @pWorker,
                         fldd_UpdatedDate = GETUTCDATE()
                   WHERE fldv_QueueGuid = @pQueueGuid
                     AND fldi_Status = 0
                     AND (fldd_NextAttemptDate IS NULL OR fldd_NextAttemptDate <= GETUTCDATE());", sql))
            {
                updateCmd.CommandType = CommandType.Text;
                updateCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueGuid, queueGuid.ToString()));
                updateCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmWorker, worker));
                updateCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLockSeconds, lockSeconds));

                var rowsUpdated = await updateCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                if (rowsUpdated == 0)
                {
                    await sql.CloseAsync().ConfigureAwait(false);
                    return Result<QueuedEvent>.Failure<QueuedEvent>(DomainErrors.GeneralError.UnProcessableRequest);
                }
            }

            using SqlCommand selectCmd = new(StoredProcs.pr_EventQueue_GetByQueueGuid, sql)
            {
                CommandType = CommandType.StoredProcedure
            };
            selectCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueGuid, queueGuid.ToString()));

            QueuedEvent? response = null;
            using (SqlDataReader reader = await selectCmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = MapQueuedEvent(reader);
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return response is not null
                ? Result<QueuedEvent>.Success(response)
                : Result<QueuedEvent>.Failure<QueuedEvent>(DomainErrors.GeneralError.UnProcessableRequest);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<QueuedEvent>.Failure<QueuedEvent>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<QueuedEvent>> GetByQueueGuidAsync(Guid queueGuid, CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_GetByQueueGuid, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueGuid, queueGuid.ToString()));

            QueuedEvent? response = null;
            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = MapQueuedEvent(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return response is not null
                ? Result<QueuedEvent>.Success(response)
                : Result<QueuedEvent>.Failure<QueuedEvent>(DomainErrors.HazardReportTrackingError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<QueuedEvent>.Failure<QueuedEvent>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<QueuedEvent>>> GetPendingAsync(int maxResults = 100, EventCategory? eventCategory = null, CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_GetPending, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMaxResults, maxResults));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventCategory, eventCategory.HasValue ? (object)(int)eventCategory.Value : DBNull.Value));

            List<QueuedEvent> response = new();
            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response.Add(MapQueuedEvent(reader));
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<QueuedEvent>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<QueuedEvent>>.Failure<List<QueuedEvent>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<QueuedEvent>>> GetByStatusAsync(QueuedEventStatus status, int maxResults = 100, EventCategory? eventCategory = null, CancellationToken ct = default)
    {
        if (status == QueuedEventStatus.Pending)
        {
            return await GetPendingAsync(maxResults, eventCategory, ct).ConfigureAwait(false);
        }

        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_GetByStatus, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventStatus, (int)status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMaxResults, maxResults));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventCategory, eventCategory.HasValue ? (object)(int)eventCategory.Value : DBNull.Value));

            List<QueuedEvent> response = new();
            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response.Add(MapQueuedEvent(reader));
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<QueuedEvent>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<QueuedEvent>>.Failure<List<QueuedEvent>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<QueuedEvent>>> LeaseBatchAsync(int batchSize, string worker, int lockSeconds = 60, CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_LeaseBatch, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmBatchSize, batchSize));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmWorker, worker));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLockSeconds, lockSeconds));

            List<QueuedEvent> response = new();
            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response.Add(MapQueuedEvent(reader));
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<QueuedEvent>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<QueuedEvent>>.Failure<List<QueuedEvent>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> MarkProcessedAsync(Guid queueGuid, string worker, CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_MarkProcessed, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueGuid, queueGuid.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmWorker, worker));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            await sql.CloseAsync().ConfigureAwait(false);

            if (rows > 0)
            {
                return Result<bool>.Success(true);
            }

            var verifyResult = await GetByQueueGuidAsync(queueGuid, ct).ConfigureAwait(false);
            var markedProcessed = verifyResult.IsSuccess && verifyResult.Value.Status == QueuedEventStatus.Processed;
            return Result<bool>.Success(markedProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> MarkFailedAsync(Guid queueGuid, string worker, string lastError, int backoffSeconds = 30, CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_MarkFailed, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueGuid, queueGuid.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmWorker, worker));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLastError, lastError));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmBackoffSeconds, backoffSeconds));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            await sql.CloseAsync().ConfigureAwait(false);

            if (rows > 0)
            {
                return Result<bool>.Success(true);
            }

            var verifyResult = await GetByQueueGuidAsync(queueGuid, ct).ConfigureAwait(false);
            var markedFailed = verifyResult.IsSuccess
                && (verifyResult.Value.Status == QueuedEventStatus.Failed || verifyResult.Value.Status == QueuedEventStatus.Pending)
                && !string.IsNullOrWhiteSpace(verifyResult.Value.LastError);

            return Result<bool>.Success(markedFailed);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> CancelAsync(Guid queueGuid, string cancelledBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_Cancel, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEventQueueGuid, queueGuid.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCancelledBy, cancelledBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            var rowsAffected = 0;
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                rowsAffected = reader.GetValue<int>(FieldNames.fRowsAffected);
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(rowsAffected > 0);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<int>> ClearCompletedAsync(string clearedBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_ClearCompleted, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmClearedBy, clearedBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            var removedCount = 0;
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                removedCount = reader.GetValue<int>(FieldNames.fRemovedCount);
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<int>.Success(removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<int>.Failure<int>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<int>> ClearAllAsync(CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_ClearAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            var removedCount = 0;
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                removedCount = reader.GetValue<int>(FieldNames.fRemovedCount);
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<int>.Success(removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<int>.Failure<int>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<int>> RecoverExpiredLocksAsync(int recoveryDelaySeconds = 5, string recoveredBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_RecoverExpiredLocks, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRecoveryDelaySeconds, recoveryDelaySeconds));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRecoveredBy, recoveredBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            var recoveredCount = 0;
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                recoveredCount = reader.GetValue<int>(FieldNames.fRecoveredCount);
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<int>.Success(recoveredCount);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<int>.Failure<int>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<QueueStatistics>> GetStatsAsync(CancellationToken ct = default)
    {
        try
        {
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_EventQueue_GetStats, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            QueueStatistics stats = new();
            var byEventType = new Dictionary<EventCategory, EventTypeStatistics>();
            var byPriority = new Dictionary<EventPriority, int>();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            static int ReadIntOrDefault(SqlDataReader r, string columnName, int defaultValue = 0)
            {
                return r.HasColumn(columnName) ? r.GetValue<int>(columnName) : defaultValue;
            }

            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                stats = new QueueStatistics
                {
                    PendingCount = ReadIntOrDefault(reader, FieldNames.fEventQueuePendingCount),
                    ProcessedCount = ReadIntOrDefault(reader, FieldNames.fEventQueueProcessedCount),
                    FailedCount = ReadIntOrDefault(reader, FieldNames.fEventQueueFailedCount),
                    CancelledCount = ReadIntOrDefault(reader, FieldNames.fEventQueueCancelledCount)
                };
            }

            if (await reader.NextResultAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    EventCategory category;
                    if (reader.HasColumn(FieldNames.fEventQueueEventCategory))
                    {
                        category = (EventCategory)reader.GetValue<int>(FieldNames.fEventQueueEventCategory);
                    }
                    else if (reader.HasColumn(FieldNames.fEventQueueEventType)
                             && Enum.TryParse<EventCategory>(reader.GetValue<string>(FieldNames.fEventQueueEventType), true, out var parsedCategory))
                    {
                        category = parsedCategory;
                    }
                    else
                    {
                        continue;
                    }

                    byEventType[category] = new EventTypeStatistics
                    {
                        Pending = ReadIntOrDefault(reader, FieldNames.fEventQueuePendingCount, ReadIntOrDefault(reader, FieldNames.fEventQueuePending)),
                        Processed = ReadIntOrDefault(reader, FieldNames.fEventQueueProcessedCount, ReadIntOrDefault(reader, FieldNames.fEventQueueProcessed)),
                        Failed = ReadIntOrDefault(reader, FieldNames.fEventQueueFailedCount, ReadIntOrDefault(reader, FieldNames.fEventQueueFailed)),
                        Cancelled = ReadIntOrDefault(reader, FieldNames.fEventQueueCancelledCount, ReadIntOrDefault(reader, FieldNames.fEventQueueCancelled))
                    };
                }
            }

            if (await reader.NextResultAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    if (!reader.HasColumn(FieldNames.fEventQueuePriority))
                    {
                        continue;
                    }

                    var priority = (EventPriority)reader.GetValue<int>(FieldNames.fEventQueuePriority);
                    byPriority[priority] = ReadIntOrDefault(reader, FieldNames.fEventQueueCount);
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<QueueStatistics>.Success(stats with
            {
                ByEventType = byEventType,
                ByPriority = byPriority
            });
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<QueueStatistics>.Failure<QueueStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    private static QueuedEvent MapQueuedEvent(SqlDataReader reader)
    {
        return new QueuedEvent
        {
            Id = Guid.TryParse(reader.GetValue<string>(FieldNames.fEventQueueGuid), out var queueGuid)
                ? queueGuid
                : Guid.NewGuid(),
            EventCategory = (EventCategory)reader.GetValue<int>(FieldNames.fEventQueueEventCategory),
            EventType = reader.GetValue<string>(FieldNames.fEventQueueEventType) ?? string.Empty,
            EventData = reader.GetValue<string>(FieldNames.fEventQueueEventData) ?? string.Empty,
            ReportId = reader.GetValue<string>(FieldNames.fEventQueueReportCode) ?? string.Empty,
            Status = (QueuedEventStatus)reader.GetValue<int>(FieldNames.fEventQueueStatus),
            QueuedAt = reader.GetValue<DateTime>(FieldNames.fEventQueueQueuedDate),
            ProcessedAt = reader.GetValue<DateTime?>(FieldNames.fEventQueueProcessedDate),
            AttemptCount = reader.GetValue<int>(FieldNames.fEventQueueAttemptCount),
            LastError = reader.GetValue<string>(FieldNames.fEventQueueLastError),
            Priority = (EventPriority)reader.GetValue<int>(FieldNames.fEventQueuePriority),
            TargetSystem = reader.GetValue<string>(FieldNames.fEventQueueTargetSystem),
            QueuedBy = reader.GetValue<string>(FieldNames.fEventQueueQueuedBy)
        };
    }
}
