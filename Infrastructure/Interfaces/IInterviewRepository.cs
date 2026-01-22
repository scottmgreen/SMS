namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Enhanced Interview Repository Interface
/// Defines comprehensive contract for interview data operations
/// </summary>
public interface IInterviewRepository
{
    // Core CRUD Operations
    Task<Result<Interview>> CreateInterviewAsync(Interview interview, CancellationToken ct = default);
    Task<Result<Interview>> GetInterviewByCodeAsync(InterviewID code, CancellationToken ct = default);
    Task<Result<List<Interview>>> GetAllInterviewsAsync(CancellationToken ct = default);
    Task<Result<Interview>> UpdateInterviewAsync(Interview interview, CancellationToken ct = default);
    Task<Result<bool>> DeleteInterviewAsync(InterviewID id, CancellationToken ct = default);

    // Query Operations
    Task<Result<IEnumerable<Interview>>> GetByInvestigationAsync(string investigationCode, CancellationToken ct = default);
    Task<Result<IEnumerable<Interview>>> GetByInvestigatorAsync(string investigatorCode, CancellationToken ct = default);
    Task<Result<IEnumerable<Interview>>> GetByStatusAsync(InterviewStatus status, CancellationToken ct = default);

    // Interview Workflow Operations
    Task<Result<bool>> UpdateStatusAsync(string interviewCode, InterviewStatus status, CancellationToken ct = default);
    Task<Result<bool>> ScheduleInterviewAsync(string interviewCode, DateTime interviewDate, string location,
        int? durationMinutes = null, CancellationToken ct = default);
    Task<Result<bool>> CompleteInterviewAsync(string interviewCode, string? personNotes, string? investigatorNotes,
        string? keyFindings = null, CancellationToken ct = default);
}