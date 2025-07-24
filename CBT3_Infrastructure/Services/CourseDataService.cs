using CBT3_Domain.Entities;
using CBT3_Domain.Interfaces;

using CBT3_Infrastructure.Repositories;

namespace CBT3_Infrastructure.Services;



public class CourseDataService : BaseDataService<CourseDataService>
{
    private readonly ILogger<CourseDataService> _logger;
    private readonly string _logheader;
    private CourseRepository _repo;

    public CourseDataService(ILogger<CourseDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, CourseRepository repo) : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader}  {repo.GetType().Name} ");
        _repo = repo;
    }

    
    public Task<Result<List<Course>>> GetCoursesAsync(TraineeID traineeId, CancellationToken ct = default)
    {
        return _repo.GetCoursesAsync(traineeId, ct);
        
    }

    public Task<Result<Course>> GetCourseByIdAsync(CourseID id, CancellationToken ct = default)
    {
        return _repo.GetCourseByIdAsync(id, ct);
    }
    
    


}
