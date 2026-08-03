namespace GymKitten.Application.Abstractions.Storage;

public record UploadResult(
    bool Success,
    string PublicUrl,
    string ErrorMessage);
