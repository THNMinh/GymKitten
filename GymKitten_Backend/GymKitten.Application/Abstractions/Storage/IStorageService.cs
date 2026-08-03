using Microsoft.AspNetCore.Http;

namespace GymKitten.Application.Abstractions.Storage;

public interface IStorageService
{
    Task<UploadResult> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task<List<UploadResult>> UploadMultipleAsync(List<IFormFile> files, CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectName, CancellationToken cancellationToken = default);
}
