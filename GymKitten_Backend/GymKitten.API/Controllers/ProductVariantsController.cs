using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Catalog.ProductVariants.Commands.CreateProductVariant;
using GymKitten.Application.Features.Catalog.ProductVariants.Commands.DeleteProductVariant;
using GymKitten.Application.Features.Catalog.ProductVariants.Commands.UpdateProductVariant;
using GymKitten.Application.Features.Catalog.ProductVariants.Queries.GetVariantsByProductId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IResult> GetVariantsByProductId(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetVariantsByProductIdQuery(productId), cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{productId:guid}/variants/by-color")]
    public async Task<IResult> GetVariantsGroupedByColor(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GymKitten.Application.Features.Catalog.ProductVariants.Queries.GetVariantsGroupedByColor.GetVariantsGroupedByColorQuery(productId),
            cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("variants")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> CreateProductVariant(
        [FromBody] CreateProductVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateProductVariantCommand(
            request.ProductId,
            request.Sku,
            request.ColorName,
            request.ColorHex,
            request.Size,
            request.Price,
            request.OriginalPrice,
            request.WeightGrams);

        var result = await _sender.Send(command, cancellationToken);
        // return result.MatchCreated(val => $"/api/products/variants/{val.VariantId}");
        return result.MatchOk();
    }

    [HttpPut("variants/{variantId:guid}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> UpdateProductVariant(
        Guid variantId,
        [FromBody] UpdateProductVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProductVariantCommand(
            variantId,
            request.Sku,
            request.ColorName,
            request.ColorHex,
            request.Size,
            request.Price,
            request.OriginalPrice,
            request.WeightGrams);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("variants/{variantId:guid}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> DeleteProductVariant(
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new DeleteProductVariantCommand(variantId), cancellationToken);
        return result.MatchOk();
    }
}
