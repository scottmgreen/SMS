//-----------------------------------------------------------------------
// <copyright file="TestPipelineCommand.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Test command to verify pipeline execution is working correctly.
//                  This command can be used to test the audit pipeline functionality.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Test command to verify pipeline execution
/// Implements ICreateCommand to test audit field setting
/// </summary>
public class TestPipelineCommand : BaseCommandBundle, IRequest<Result<string>>, ICreateCommand
{
    public string TestMessage { get; set; }
    public string CreatedByTest { get; set; } = string.Empty;
    public DateTime CreatedDateTest { get; set; }

    public TestPipelineCommand(string testMessage)
    {
        TestMessage = testMessage ?? throw new ArgumentNullException(nameof(testMessage));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        CreatedByTest = userId;
        CreatedDateTest = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

/// <summary>
/// Test command handler to verify pipeline execution
/// </summary>
public class TestPipelineCommandHandler : BaseCommandBundle, IRequestHandler<TestPipelineCommand, Result<string>>
{
    private readonly ILogger<TestPipelineCommandHandler> _logger;

    public TestPipelineCommandHandler(ILogger<TestPipelineCommandHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<string>> HandleAsync(TestPipelineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("? Clean Architecture: TestPipelineCommandHandler executing with message: {Message}", request.TestMessage);
            _logger.LogInformation("? Clean Architecture: Audit fields set - CreatedBy: {CreatedBy}, CreatedDate: {CreatedDate}", 
                request.CreatedByTest, request.CreatedDateTest);

            // Simulate some work
            await Task.Delay(100, cancellationToken);

            var result = $"Pipeline test successful! Message: {request.TestMessage}, CreatedBy: {request.CreatedByTest}, CreatedDate: {request.CreatedDateTest:yyyy-MM-dd HH:mm:ss}";
            
            _logger.LogInformation("? Clean Architecture: TestPipelineCommandHandler completed successfully");
            
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error in TestPipelineCommandHandler");
            return Result<string>.Failure<string>(new Error("TEST_ERROR", ex.Message));
        }
    }
}