using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Common;

namespace SMS_Infrastructure.Services;

public sealed class HazardFileExternalStorageService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HazardFileExternalStorageService> _logger;

    public HazardFileExternalStorageService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<HazardFileExternalStorageService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Result<HazardFile>> PrepareHazardFileForStorageAsync(HazardFile hazardFile, byte[] fileData, CancellationToken ct = default)
    {
        if (hazardFile is null)
        {
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
        }

        if (fileData is null || fileData.Length == 0)
        {
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
        }

        var useMockCloudStorage = _configuration.GetValue<bool>("HazardFileCloudStorage:EnableMockCloudStorage", true);
        var uploadEndpoint = _configuration.GetValue<string>("HazardFileCloudStorage:UploadEndpoint")?.Trim();

        if (useMockCloudStorage)
        {
            ApplyMockCloudStorage(hazardFile);
            return Result<HazardFile>.Success(hazardFile);
        }

        if (string.IsNullOrWhiteSpace(uploadEndpoint))
        {
            _logger.LogInfrastructureError("Cloud upload endpoint is not configured for {FileName}", null);
            return Result<HazardFile>.Failure<HazardFile>(
                new Error("HazardFile.CloudStorageNotConfigured", "Cloud upload endpoint is not configured."));
        }

        try
        {
            var cloudResult = await TryUploadToCloudAsync(hazardFile, fileData, uploadEndpoint, ct).ConfigureAwait(false);
            if (cloudResult.IsSuccess)
            {
                return cloudResult;
            }

            _logger.LogInfrastructureError("Cloud upload failed for {FileName}; file will not be stored.", null);
            return cloudResult;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError("Cloud upload exception for {FileName}: {Error}", null, ex);
            return Result<HazardFile>.Failure<HazardFile>(
                new Error("HazardFile.CloudUploadException", $"Cloud upload failed: {ex.Message}"));
        }
    }

    private void ApplyMockCloudStorage(HazardFile hazardFile)
    {
        var configuredBaseUri = _configuration.GetValue<string>("HazardFileCloudStorage:BaseUri")?.Trim();
        if (string.IsNullOrWhiteSpace(configuredBaseUri))
        {
            throw new InvalidOperationException("HazardFileCloudStorage:BaseUri is required when mock cloud storage is enabled.");
        }

        var originalName = Path.GetFileName(hazardFile.FileName);
        var safeOriginalName = string.Concat(originalName.Where(ch => !Path.GetInvalidFileNameChars().Contains(ch))).Trim();
        if (string.IsNullOrWhiteSpace(safeOriginalName))
        {
            safeOriginalName = "hazard-file.bin";
        }

        var extension = Path.GetExtension(safeOriginalName);
        var guidFileName = $"{Guid.NewGuid():D}{extension}";
        var uploadSessionFolder = Guid.NewGuid().ToString("D");

        hazardFile.FileName = guidFileName;
        hazardFile.FilePath = $"{configuredBaseUri.TrimEnd('/')}/{DateTime.UtcNow:yyyy/MM/dd}/{uploadSessionFolder}/{guidFileName}";
        hazardFile.StorageType = "Cloud";
        hazardFile.FileData = null;
    }

    private async Task<Result<HazardFile>> TryUploadToCloudAsync(HazardFile hazardFile, byte[] fileData, string uploadEndpoint, CancellationToken ct)
    {
        var permanentToken = _configuration.GetValue<string>("HazardFileCloudStorage:PermanentToken")?.Trim();
        var clientId = _configuration.GetValue<string>("HazardFileCloudStorage:OAuthClientId")?.Trim();

        var originalName = Path.GetFileName(hazardFile.FileName);
        var safeOriginalName = string.Concat(originalName.Where(ch => !Path.GetInvalidFileNameChars().Contains(ch))).Trim();
        if (string.IsNullOrWhiteSpace(safeOriginalName))
        {
            safeOriginalName = "hazard-file.bin";
        }

        var extension = Path.GetExtension(safeOriginalName);
        var uploadUid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var guidFileName = $"{uploadUid}{extension}";
        var endpointWithUid = QueryHelpers.AddQueryString(uploadEndpoint, "uid", uploadUid);

        using var httpClient = _httpClientFactory.CreateClient();
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileData);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue(hazardFile.ContentType ?? "application/octet-stream");
        form.Add(fileContent, "file", guidFileName);

        string? tokenValue = null;
        if (!string.IsNullOrWhiteSpace(permanentToken))
        {
            tokenValue = permanentToken.StartsWith("perm:", StringComparison.OrdinalIgnoreCase)
                ? permanentToken
                : $"perm:{permanentToken}";
        }

        var request = new HttpRequestMessage(HttpMethod.Post, endpointWithUid)
        {
            Content = form
        };

        request.Version = HttpVersion.Version11;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

        request.Headers.Accept.ParseAdd("*/*");
        request.Headers.UserAgent.ParseAdd("curl/8.19.0");
        request.Headers.ExpectContinue = false;

        if (!string.IsNullOrWhiteSpace(clientId))
        {
            request.Headers.TryAddWithoutValidation("ClientId", clientId);
        }

        if (!string.IsNullOrWhiteSpace(tokenValue))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
        }

        var requestHeaders = string.Join(" | ", request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"));
        var contentHeaders = string.Join(" | ", request.Content.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"));

        _logger.LogInfrastructureWarning(
            "Cloud upload PRE-SEND request details: Method={Method}; RequestUri={RequestUri}; RequestHeaders=[{RequestHeaders}]; ContentHeaders=[{ContentHeaders}]; MultipartField=file; MultipartFileName={MultipartFileName}; MultipartContentType={MultipartContentType}; MultipartSizeBytes={MultipartSizeBytes}; Query.uid={Uid}",
            request.Method,
            request.RequestUri,
            string.IsNullOrWhiteSpace(requestHeaders) ? "(none)" : requestHeaders,
            string.IsNullOrWhiteSpace(contentHeaders) ? "(none)" : contentHeaders,
            guidFileName,
            hazardFile.ContentType ?? "application/octet-stream",
            fileData.Length,
            uploadUid);

        var response = await httpClient.SendAsync(request, ct).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        _logger.LogInfrastructureWarning(
            "Cloud upload response details: StatusCode={StatusCode}; ReasonPhrase={ReasonPhrase}; Body={Body}",
            (int)response.StatusCode,
            response.ReasonPhrase,
            responseBody);

        if (!response.IsSuccessStatusCode)
        {
            var reason = string.IsNullOrWhiteSpace(responseBody)
                ? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"
                : $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {responseBody}";

            return Result<HazardFile>.Failure<HazardFile>(
                new Error("HazardFile.CloudUploadFailed", $"Cloud upload failed ({reason})."));
        }

        var returnedFileName = TryExtractFileNameFromUploadResponse(responseBody);
        var returnedFileUri = TryExtractUriFromUploadResponse(responseBody);

        hazardFile.FileName = !string.IsNullOrWhiteSpace(returnedFileName)
            ? Path.GetFileName(returnedFileName)
            : guidFileName;

        hazardFile.FilePath = !string.IsNullOrWhiteSpace(returnedFileUri)
            ? returnedFileUri
            : $"{(_configuration.GetValue<string>("HazardFileCloudStorage:BaseUri") ?? uploadEndpoint).TrimEnd('/')}/{uploadUid}/{guidFileName}";

        hazardFile.StorageType = "Cloud";
        hazardFile.FileData = null;

        return Result<HazardFile>.Success(hazardFile);
    }

    private static string? TryExtractUriFromUploadResponse(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var candidates = new[] { "fileUri", "fileUrl", "url", "uri", "location", "path" };
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                if (candidates.Any(c => string.Equals(c, property.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    var text = property.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static string? TryExtractFileNameFromUploadResponse(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var candidates = new[] { "fileName", "filename", "name" };
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                if (candidates.Any(c => string.Equals(c, property.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    var text = property.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
