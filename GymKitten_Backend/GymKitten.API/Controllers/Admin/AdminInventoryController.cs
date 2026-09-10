using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Admin.Inventory.Commands.AdjustStock;
using GymKitten.Application.Features.Admin.Inventory.Commands.Restock;
using GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;
using GymKitten.Application.Features.Admin.Inventory.Queries.GetInventoryTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,admin")]
[Route("api/admin/inventory")]
public class AdminInventoryController : ControllerBase
{
    private readonly ISender _sender;

    public AdminInventoryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetInventory(
        [FromQuery] GetInventoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("restock")]
    public async Task<IResult> Restock(
        [FromBody] RestockRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RestockCommand(request.VariantId, request.Quantity, request.ReferenceId ?? string.Empty);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("adjust")]
    public async Task<IResult> AdjustStock(
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AdjustStockCommand(request.VariantId, request.NewQuantity, request.Note ?? string.Empty);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("transactions")]
    public async Task<IResult> GetTransactions(
        [FromQuery] GetInventoryTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
