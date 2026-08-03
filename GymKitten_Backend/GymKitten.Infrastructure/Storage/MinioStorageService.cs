using System.Security.Cryptography;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace GymKitten.Infrastructure.Storage;

public sealed class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioStorageService> _logger;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public MinioStorageService(
        IMinioClient minioClient,
        IOptions<MinioSettings> settings,
        ILogger<MinioStorageService> logger)
    {
        _minioClient = minioClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<UploadResult> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return new UploadResult(false, string.Empty, "File is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return new UploadResult(false, string.Empty, "File size exceeds 10MB limit.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return new UploadResult(false, string.Empty, $"Content-Type '{file.ContentType}' is not supported. Allowed: JPEG, PNG, WEBP.");
        }

        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            var objectName = GenerateObjectName(file);

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName)
                .WithStreamData(memoryStream)
                .WithObjectSize(memoryStream.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            var baseUrl = !string.IsNullOrWhiteSpace(_settings.PublicEndpoint)
                ? _settings.PublicEndpoint.TrimEnd('/')
                : $"http://{_settings.Endpoint.TrimEnd('/')}";

            var publicUrl = $"{baseUrl}/{_settings.BucketName}/{objectName}";

            _logger.LogInformation("Successfully uploaded file {FileName} to MinIO as {ObjectName}", file.FileName, objectName);

            return new UploadResult(true, publicUrl, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName} to MinIO bucket {BucketName}", file.FileName, _settings.BucketName);
            return new UploadResult(false, string.Empty, $"Upload failed: {ex.Message}");
        }
    }

    public async Task<List<UploadResult>> UploadMultipleAsync(List<IFormFile> files, CancellationToken cancellationToken = default)
    {
        var results = new List<UploadResult>();

        if (files is null || files.Count == 0)
        {
            return results;
        }

        foreach (var file in files)
        {
            var result = await UploadAsync(file, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return;
        }

        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);

            _logger.LogInformation("Successfully deleted object {ObjectName} from MinIO bucket {BucketName}", objectName, _settings.BucketName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete object {ObjectName} from MinIO bucket {BucketName}", objectName, _settings.BucketName);
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);
            var exists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

            if (!exists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_settings.BucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);

                _logger.LogInformation("Created MinIO bucket {BucketName}", _settings.BucketName);

                // Apply public read policy
                var policyJson = $$"""
                {
                    "Version": "2012-10-17",
                    "Statement": [
                        {
                            "Effect": "Allow",
                            "Principal": {"AWS": ["*"]},
                            "Action": ["s3:GetObject"],
                            "Resource": ["arn:aws:s3:::{{_settings.BucketName}}/*"]
                        }
                    ]
                }
                """;

                var setPolicyArgs = new SetPolicyArgs()
                    .WithBucket(_settings.BucketName)
                    .WithPolicy(policyJson);

                await _minioClient.SetPolicyAsync(setPolicyArgs, cancellationToken);
                _logger.LogInformation("Applied public-read policy to MinIO bucket {BucketName}", _settings.BucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure MinIO bucket {BucketName} exists", _settings.BucketName);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static string GenerateObjectName(IFormFile file, string folder = "images")
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
        {
            extension = file.ContentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        var dateFolder = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var uniqueId = Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToLowerInvariant(); // 12 chars

        return $"{folder}/{dateFolder}/{uniqueId}{extension}";
    }
}
