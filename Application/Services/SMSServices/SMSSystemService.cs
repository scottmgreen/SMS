//-----------------------------------------------------------------------
// <copyright file="SMSSystemService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------


using SMS_Domain.Entities;
using Microsoft.FeatureManagement;

namespace SMS_Application.Services;

/// <summary>
/// System Service providing system-wide operations and configuration management
/// </summary>
public class SMSSystemService
{
    private readonly IFeatureManager _featureManager;
    private readonly SystemDataService _dataService;
    private readonly FileService _fileService;

    public SMSSystemService(IFeatureManager featureManager, SystemDataService dataService, FileService fileservice)
    {
        _featureManager = featureManager;
        _dataService = dataService;
        _fileService = fileservice;
    }

    public async Task<Result<bool>> AddAuditLogEntryAsync(AuditLogEntry auditlogentry, CancellationToken ct = default)
    {

        var result = await _dataService.AddAuditLogEntryAsync(auditlogentry, ct).ConfigureAwait(false);
        if (result.IsSuccess)
            return Result<bool>.Success(true);
        else
            return Result<bool>.Failure<bool>(DomainErrors.SystemError.AuditLogEntryError);


    }

    public IAsyncEnumerable<string> GetFeatureNamesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsEnabledAsync(string feature)
    {
        return _featureManager.IsEnabledAsync(feature);
    }

    public Task<bool> IsEnabledAsync<TContext>(string feature, TContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> IsFileExists(string filePath)
    {
        try
        {
            CancellationToken ct = new();
            return _fileService.IsFileExistsAsync(filePath, ct);
        }
        catch (Exception)
        {
            return Task.FromResult(Result<bool>.Failure<bool>(DomainErrors.SystemError.FileExistsError));
        }


    }

    public Task<Result<int>> GetAdminPasscodeAsync()
    {
        return _dataService.GetAdminPasscodeAsync();
    }

    public Task<Result<bool>> GetFeatureEnabledAsync(string featurename)
    {
        return _dataService.GetFeatureEnabledAsync(featurename);
    }


}

