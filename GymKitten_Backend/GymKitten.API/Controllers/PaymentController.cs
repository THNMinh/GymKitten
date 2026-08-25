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
    public async Task<IActionResult> VnPayIpn(CancellationToken cancellationToken = default)
    {
        try
        {
            var callbackData = _vnPayService.ProcessCallback(Request.Query);
            if (callbackData is null)
            {
                return Ok(new { RspCode = "97", Message = "Invalid data" });
            }

            var command = new ProcessVnPayIpnCommand(callbackData);
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return Ok(new { RspCode = "99", Message = result.Error.Message });
            }

            return Ok(new
            {
                RspCode = result.Value.RspCode,
                Message = result.Value.Message
            });
        }
        catch
        {
            return Ok(new { RspCode = "99", Message = "Server error" });
        }
    }

    [HttpGet("vnpay-callback")]
    public async Task<IActionResult> VnPayCallback(CancellationToken cancellationToken = default)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:8081";

        var callbackData = _vnPayService.ProcessCallback(Request.Query);
        if (callbackData is null)
        {
            return Redirect($"{frontendUrl}/orders/success?isSuccess=false&message=InvalidCallbackData");
        }

        var command = new ProcessVnPayIpnCommand(callbackData);
        await _sender.Send(command, cancellationToken);

        var isSuccess = callbackData.IsSuccess;
        var redirectUrl = $"{frontendUrl}/orders/success?orderCode={callbackData.TxnRef}&total={callbackData.Amount}&isSuccess={isSuccess.ToString().ToLower()}";

        return Redirect(redirectUrl);
    }

    [HttpPost("momo-ipn")]
    public async Task<IActionResult> ProcessMomoIpn(
        [FromBody] ProcessMomoIpnCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error.Message });
        }

        return NoContent(); // Fast 204 response as required by MoMo IPN specification
    }

    [HttpGet("momo-return")]
    public IActionResult ProcessMomoReturn(
        [FromQuery] string orderId,
        [FromQuery] int resultCode,
        [FromQuery] string message,
        [FromQuery] long? amount)
    {
        var isSuccess = resultCode == 0;
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:8081";

        var totalAmount = amount ?? 0;
        var redirectUrl = $"{frontendUrl}/orders/success?orderCode={orderId}&total={totalAmount}&isSuccess={isSuccess.ToString().ToLower()}&message={Uri.EscapeDataString(message ?? "")}";

        return Redirect(redirectUrl);
    }
}
