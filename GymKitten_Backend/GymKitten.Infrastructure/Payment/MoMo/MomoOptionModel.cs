namespace GymKitten.Infrastructure.Payment.MoMo;

public class MomoOptionModel
{
    public const string SectionName = "MomoAPI";

    public string MomoApiUrl { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    public string NotifyUrl { get; set; } = string.Empty;

    public string PartnerCode { get; set; } = string.Empty;

    public string RequestType { get; set; } = string.Empty;
}
