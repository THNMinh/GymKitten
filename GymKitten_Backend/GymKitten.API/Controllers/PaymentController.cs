using GymKitten.Application.Abstractions.Services;
using GymKitten.Application.Features.Payment.Commands.ProcessVnPayIpn;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IVnPayService _vnPayService;

    public PaymentController(ISender sender, IVnPayService vnPayService)
    {
        _sender = sender;
        _vnPayService = vnPayService;
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
        var callbackData = _vnPayService.ProcessCallback(Request.Query);
        if (callbackData is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Payment Callback Data"
            });
        }

        // Also process IPN logic as fallback if IPN didn't reach yet
        var command = new ProcessVnPayIpnCommand(callbackData);
        await _sender.Send(command, cancellationToken);

        return Ok(new
        {
            TxnRef = callbackData.TxnRef,
            Amount = callbackData.Amount,
            IsSuccess = callbackData.IsSuccess,
            ResponseCode = callbackData.ResponseCode,
            BankCode = callbackData.BankCode,
            TransactionNo = callbackData.TransactionNo
        });
    }
}
