using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Application.Models.Momo;
using Microsoft.Extensions.Options;

namespace GymKitten.Infrastructure.Payment.MoMo;

public class MomoService : IMomoService
{
    private readonly HttpClient _httpClient;
    private readonly MomoOptionModel _momoOption;

    public MomoService(HttpClient httpClient, IOptions<MomoOptionModel> momoOption)
    {
        _httpClient = httpClient;
        _momoOption = momoOption.Value;
    }

    public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(
        Guid orderId,
        decimal amount,
        string orderInfo,
        CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        var orderIdStr = orderId.ToString();
        var amountLong = (long)amount;
        var extraData = string.Empty;

        // MoMo v2 raw hash format for create payment
        var rawHash = $"accessKey={_momoOption.AccessKey}&amount={amountLong}&extraData={extraData}&ipnUrl={_momoOption.NotifyUrl}&orderId={orderIdStr}&orderInfo={orderInfo}&partnerCode={_momoOption.PartnerCode}&redirectUrl={_momoOption.ReturnUrl}&requestId={requestId}&requestType={_momoOption.RequestType}";

        var signature = ComputeHmacSha256(rawHash, _momoOption.SecretKey);

        var requestBody = new
        {
            partnerCode = _momoOption.PartnerCode,
            partnerName = "GymKitten Store",
            storeId = "GymKitten",
            requestId = requestId,
            amount = amountLong,
            orderId = orderIdStr,
            orderInfo = orderInfo,
            redirectUrl = _momoOption.ReturnUrl,
            ipnUrl = _momoOption.NotifyUrl,
            lang = "vi",
            extraData = extraData,
            requestType = _momoOption.RequestType,
            signature = signature
        };

        var response = await _httpClient.PostAsJsonAsync(_momoOption.MomoApiUrl, requestBody, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var result = JsonSerializer.Deserialize<MomoCreatePaymentResponseModel>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null)
            {
                return result;
            }
        }
        catch
        {
        }

        return new MomoCreatePaymentResponseModel
        {
            ResultCode = (int)response.StatusCode,
            Message = string.IsNullOrWhiteSpace(content) ? (response.ReasonPhrase ?? "MoMo API Request Failed") : content
        };
    }

    public bool VerifyIpnSignature(
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
        long transId)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        // MoMo IPN raw hash string concatenated in strict alphabetical order
        var rawHash = $"accessKey={_momoOption.AccessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";

        var computedSignature = ComputeHmacSha256(rawHash, _momoOption.SecretKey);
        return string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
