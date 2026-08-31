using GymKitten.API.Extensions;
using GymKitten.Application.Features.Catalog.ProductImages.Commands.DeleteProductImage;
using GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;
using GymKitten.Application.Features.Catalog.ProductImages.Queries.GetProductImagesByProduct;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductImagesController : ControllerBase
{
    private readonly ISender _sender;

    public ProductImagesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{productId:guid}/images")]
    public async Task<IResult> GetProductImagesByProduct(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetProductImagesByProductQuery(productId), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{productId:guid}/images")]
    [Authorize(Roles = "Admin,admin")]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadProductImages(
        Guid productId,
        [FromForm] List<IFormFile> photos,
        [FromForm] Guid? variantId,
        CancellationToken cancellationToken = default)
    {
        var command = new UploadProductImagesCommand(productId, variantId, photos);
        var result = await _sender.Send(command, cancellationToken);
        // return result.MatchCreated(val => $"/api/products/{val.ProductId}/images");
        return result.MatchOk();
    }

    [HttpDelete("images/{imageId:guid}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> DeleteProductImage(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new DeleteProductImageCommand(imageId), cancellationToken);
        return result.MatchOk();
    }
}
