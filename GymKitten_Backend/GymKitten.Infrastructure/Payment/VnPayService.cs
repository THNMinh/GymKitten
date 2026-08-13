using GymKitten.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;

namespace GymKitten.Infrastructure.Payment;

public class VnPayService : IVnPayService
{
    private readonly IVnpayClient _vnpayClient;

    public VnPayService(IVnpayClient vnpayClient)
    {
        _vnpayClient = vnpayClient;
    }

    public (string Url, string TxnRef) CreatePaymentUrl(decimal amount, string description, Guid? customTransactionId = null)
    {
        var request = new VnpayPaymentRequest
        {
            Money = (double)amount,
            Description = description,
            BankCode = BankCode.ANY,
            Language = DisplayLanguage.Vietnamese
        };

        var paymentUrlInfo = _vnpayClient.CreatePaymentUrl(request);
        var txnRef = customTransactionId?.ToString() ?? request.PaymentId.ToString();

        var url = paymentUrlInfo.Url;
        if (customTransactionId.HasValue)
        {
            url = ReplaceQueryParam(url, "vnp_TxnRef", customTransactionId.Value.ToString());
        }

        return (url, txnRef);
    }

    public VnPayCallbackData? ProcessCallback(IQueryCollection query)
    {
        try
        {
            var txnRef = query["vnp_TxnRef"].ToString();
            var vnpAmount = query["vnp_Amount"].ToString();
            var vnpResponseCode = query["vnp_ResponseCode"].ToString();
            var vnpTransactionNo = query["vnp_TransactionNo"].ToString();
            var vnpBankCode = query["vnp_BankCode"].ToString();
            var vnpTransactionStatus = query["vnp_TransactionStatus"].ToString();

            if (string.IsNullOrEmpty(txnRef) || string.IsNullOrEmpty(vnpResponseCode))
            {
                return null;
            }

            decimal amount = 0;
            if (long.TryParse(vnpAmount, out var rawAmount))
            {
                amount = rawAmount / 100m;
            }

            long.TryParse(vnpTransactionNo, out var transactionId);

            return new VnPayCallbackData
            {
                TxnRef = txnRef,
                Amount = amount,
                ResponseCode = vnpResponseCode,
                TransactionStatus = string.IsNullOrEmpty(vnpTransactionStatus) ? vnpResponseCode : vnpTransactionStatus,
                TransactionNo = vnpTransactionNo ?? "",
                BankCode = vnpBankCode ?? "",
                VnpayTransactionId = transactionId,
                IsVerified = true
            };
        }
        catch
        {
            return null;
        }
    }

    private static string ReplaceQueryParam(string url, string key, string value)
    {
        var uri = new Uri(url);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        var dictionary = query.ToDictionary(k => k.Key, v => v.Value.ToString());
        dictionary[key] = value;

        var newQueryString = string.Join("&", dictionary.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}?{newQueryString}";
    }
}
