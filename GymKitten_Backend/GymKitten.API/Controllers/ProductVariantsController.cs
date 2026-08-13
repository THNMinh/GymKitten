using GymKitten.Application.Features.Catalog.ProductVariants.Commands.CreateProductVariant;
using GymKitten.Application.Features.Catalog.ProductVariants.Commands.DeleteProductVariant;
using GymKitten.Application.Features.Catalog.ProductVariants.Commands.UpdateProductVariant;
using GymKitten.Application.Features.Catalog.ProductVariants.Queries.GetVariantsByProductId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductVariantsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductVariantsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{productId:guid}/variants")]
    public async Task<IActionResult> GetVariantsByProductId(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVariantsByProductIdQuery(productId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Get Product Variants Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("variants")]
    public async Task<IActionResult> CreateProductVariant(
        [FromBody] CreateProductVariantCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Create Product Variant Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return CreatedAtAction(
            actionName: nameof(GetVariantsByProductId),
            routeValues: new { productId = command.ProductId },
            value: result.Value);
    }

    [HttpPut("variants/{variantId:guid}")]
    public async Task<IActionResult> UpdateProductVariant(
        Guid variantId,
        [FromBody] UpdateProductVariantCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.VariantId != Guid.Empty && command.VariantId != variantId)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid VariantId",
                Detail = "Route VariantId does not match command VariantId."
            });
        }

        var commandToExecute = command.VariantId == Guid.Empty
            ? command with { VariantId = variantId }
            : command;

        var result = await _sender.Send(commandToExecute, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Update Product Variant Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }

    [HttpDelete("variants/{variantId:guid}")]
    public async Task<IActionResult> DeleteProductVariant(
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductVariantCommand(variantId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Delete Product Variant Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return NoContent();
    }
}
