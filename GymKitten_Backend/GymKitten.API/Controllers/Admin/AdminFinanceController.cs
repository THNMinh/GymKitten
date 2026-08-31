using GymKitten.API.Extensions;
using GymKitten.Application.Features.Admin.Finance.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,admin")]
[Route("api/admin/finance")]
public class AdminFinanceController : ControllerBase
{
    private readonly ISender _sender;

    public AdminFinanceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("transactions")]
    public async Task<IResult> GetTransactions(
        [FromQuery] GetTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
