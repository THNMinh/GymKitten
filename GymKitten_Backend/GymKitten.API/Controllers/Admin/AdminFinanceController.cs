using GymKitten.Application.Features.Admin.Finance.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Route("api/admin/finance")]
public class AdminFinanceController : ControllerBase
{
    private readonly ISender _sender;

    public AdminFinanceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] GetTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Get Transactions Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }
}
