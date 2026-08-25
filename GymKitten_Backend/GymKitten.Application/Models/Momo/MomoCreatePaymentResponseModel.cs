namespace GymKitten.Application.Models.Momo;

public class MomoCreatePaymentResponseModel
{
    public string PartnerCode { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;

    public long Amount { get; set; }

    public long ResponseTime { get; set; }

    public string Message { get; set; } = string.Empty;

    public int ResultCode { get; set; }

    public string PayUrl { get; set; } = string.Empty;

    public string Deeplink { get; set; } = string.Empty;

    public string QrCodeUrl { get; set; } = string.Empty;
}
