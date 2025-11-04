using System.Threading;

using SMS_Domain.Errors;

namespace SMS_Infrastructure.Services
{
    public class FileService
    {
        public Task<Result<bool>> IsFileExistsAsync(string filePath, CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                // Validate filePath for illegal characters
                if (filePath.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return Task.FromResult(Result<bool>.Failure<bool>(DomainErrors.SystemError.FileExistsError));
                }
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string fullPath = Path.Combine(wwwRootPath, filePath.TrimStart('/'));
                bool result = File.Exists(fullPath);

            
                return Task.FromResult(Result<bool>.Success(result));
            }
            catch (Exception)
            {
                return Task.FromResult(Result<bool>.Failure<bool>(DomainErrors.SystemError.FileExistsError));
            }
        }
    }

}
