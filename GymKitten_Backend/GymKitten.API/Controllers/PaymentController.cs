using GymKitten.API.Extensions;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Application.Features.Payment.Commands.ProcessMomoIpn;
using GymKitten.Application.Features.Payment.Commands.ProcessVnPayIpn;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/payment")]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IVnPayService _vnPayService;
    private readonly IConfiguration _configuration;

    public PaymentController(
        ISender sender,
        IVnPayService vnPayService,
        IConfiguration configuration)
    {
        _sender = sender;
        _vnPayService = vnPayService;
        _configuration = configuration;
    }

    [HttpGet("vnpay-ipn")]
    public async Task<IResult> VnPayIpn(CancellationToken cancellationToken = default)
    {
        var callbackData = _vnPayService.ProcessCallback(Request.Query);
        if (callbackData is null)
        {
            return Results.Ok(new { RspCode = "97", Message = "Invalid data" });
        }

        var command = new ProcessVnPayIpnCommand(callbackData);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("vnpay-callback")]
    public async Task<IResult> VnPayCallback(CancellationToken cancellationToken = default)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:8081";

        var callbackData = _vnPayService.ProcessCallback(Request.Query);
        if (callbackData is null)
        {
            return Results.Redirect($"{frontendUrl}/orders/success?isSuccess=false&message=InvalidCallbackData");
        }

        var command = new ProcessVnPayIpnCommand(callbackData);
        await _sender.Send(command, cancellationToken);

        var isSuccess = callbackData.IsSuccess;
        var redirectUrl = $"{frontendUrl}/orders/success?orderCode={callbackData.TxnRef}&total={callbackData.Amount}&isSuccess={isSuccess.ToString().ToLower()}";

        return Results.Redirect(redirectUrl);
    }

    [HttpPost("momo-ipn")]
    public async Task<IResult> ProcessMomoIpn(
        [FromBody] ProcessMomoIpnCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("momo-return")]
    public IResult ProcessMomoReturn(
        [FromQuery] string orderId,
        [FromQuery] int resultCode,
        [FromQuery] string message,
        [FromQuery] long? amount)
    {
        var isSuccess = resultCode == 0;
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:8081";

        var totalAmount = amount ?? 0;
        var redirectUrl = $"{frontendUrl}/orders/success?orderCode={orderId}&total={totalAmount}&isSuccess={isSuccess.ToString().ToLower()}&message={Uri.EscapeDataString(message ?? "")}";

        return Results.Redirect(redirectUrl);
    }
}
