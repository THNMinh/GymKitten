using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AppUploadResult = GymKitten.Application.Abstractions.Storage.UploadResult;

namespace GymKitten.Infrastructure.Storage;

public sealed class CloudinaryStorageService : IStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryStorageService> _logger;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public CloudinaryStorageService(
        IOptions<CloudinarySettings> settings,
        ILogger<CloudinaryStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret);

        _cloudinary = new Cloudinary(account)
        {
            Api = { Secure = true }
        };
    }

    public async Task<AppUploadResult> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return new AppUploadResult(false, string.Empty, "File is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return new AppUploadResult(false, string.Empty, "File size exceeds 10MB limit.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return new AppUploadResult(false, string.Empty, $"Content-Type '{file.ContentType}' is not supported. Allowed: JPEG, PNG, WEBP.");
        }

        try
        {
            using var stream = file.OpenReadStream();

            var folderName = string.IsNullOrWhiteSpace(_settings.Folder) ? "gymkitten" : _settings.Folder.Trim();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName,
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (uploadResult.Error is not null)
            {
                _logger.LogError("Cloudinary upload failed for file {FileName}: {ErrorMessage}", file.FileName, uploadResult.Error.Message);
                return new AppUploadResult(false, string.Empty, uploadResult.Error.Message);
            }

            var secureUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty;

            _logger.LogInformation("Successfully uploaded file {FileName} to Cloudinary as {PublicId}", file.FileName, uploadResult.PublicId);

            return new AppUploadResult(true, secureUrl, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while uploading file {FileName} to Cloudinary", file.FileName);
            return new AppUploadResult(false, string.Empty, $"Upload failed: {ex.Message}");
        }
    }

    public async Task<List<AppUploadResult>> UploadMultipleAsync(List<IFormFile> files, CancellationToken cancellationToken = default)
    {
        var results = new List<AppUploadResult>();

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
            var publicId = ExtractPublicId(objectName);
            if (string.IsNullOrWhiteSpace(publicId))
            {
                _logger.LogWarning("Could not extract publicId from objectName: {ObjectName}", objectName);
                return;
            }

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            _logger.LogInformation("Cloudinary deletion for {PublicId} completed with result: {Result}", publicId, result.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image {ObjectName} from Cloudinary", objectName);
        }
    }

    private static string ExtractPublicId(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var cleanInput = input.Trim();

        // 1. If it's a full Cloudinary URL:
        // Format: .../image/upload/(v[0-9]+/)?(folder/.../filename).(jpg|png|webp|...)
        var uploadMatch = Regex.Match(cleanInput, @"/upload/(?:v\d+/)?(.+?)(?:\.[a-zA-Z0-9]+)?$", RegexOptions.IgnoreCase);
        if (uploadMatch.Success)
        {
            return uploadMatch.Groups[1].Value;
        }

        // 2. If it's a relative path or object name with extension:
        // Strip leading slash
        cleanInput = cleanInput.TrimStart('/');

        // Strip file extension if present
        var extIndex = cleanInput.LastIndexOf('.');
        if (extIndex > 0)
        {
            cleanInput = cleanInput[..extIndex];
        }

        return cleanInput;
    }
}
