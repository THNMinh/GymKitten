namespace GymKitten.Infrastructure.Settings;

public sealed class MinioSettings
{
    public const string SectionName = "MinioSettings";

    public string Endpoint { get; init; } = "localhost:9000";

    public string AccessKey { get; init; } = "kickify_admin";

    public string SecretKey { get; init; } = "miniolocal";

    public string BucketName { get; init; } = "gymkitten-media";

    public bool UseSSL { get; init; } = false;

    public string PublicEndpoint { get; init; } = "http://localhost:9000";
}
