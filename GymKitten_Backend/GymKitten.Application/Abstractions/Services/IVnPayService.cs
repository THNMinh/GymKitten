using Microsoft.AspNetCore.Http;

namespace GymKitten.Application.Abstractions.Services;

public interface IVnPayService
{
    (string Url, string TxnRef) CreatePaymentUrl(decimal amount, string description, Guid? customTransactionId = null);

    VnPayCallbackData? ProcessCallback(IQueryCollection query);
}

public class VnPayCallbackData
{
    public string TxnRef { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string ResponseCode { get; set; } = string.Empty;

    public string TransactionStatus { get; set; } = string.Empty;

    public string TransactionNo { get; set; } = string.Empty;

    public string BankCode { get; set; } = string.Empty;

    public long VnpayTransactionId { get; set; }

    public bool IsVerified { get; set; }

    public bool IsSuccess => ResponseCode == "00" && TransactionStatus == "00";
}
