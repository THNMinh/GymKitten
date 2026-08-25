using GymKitten.Application.Models.Momo;

namespace GymKitten.Application.Abstractions.Services;

public interface IMomoService
{
    Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(
        Guid orderId,
        decimal amount,
        string orderInfo,
        CancellationToken cancellationToken = default);

    bool VerifyIpnSignature(
        string signature,
        long amount,
        string extraData,
        string message,
        string orderId,
        string orderInfo,
        string orderType,
        string partnerCode,
        string payType,
        string requestId,
        long responseTime,
        int resultCode,
        long transId);
}
