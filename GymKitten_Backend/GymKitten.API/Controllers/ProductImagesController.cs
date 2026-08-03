using GymKitten.Application.Features.Catalog.ProductImages.Commands.DeleteProductImage;
using GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;
using GymKitten.Application.Features.Catalog.ProductImages.Queries.GetProductImagesByProduct;
using MediatR;
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
    public async Task<IActionResult> GetProductImagesByProduct(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductImagesByProductQuery(productId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Product Images Not Found",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("{productId:guid}/images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProductImages(
        Guid productId,
        [FromForm] List<IFormFile> photos,
        [FromForm] Guid? variantId,
        CancellationToken cancellationToken = default)
    {
        var command = new UploadProductImagesCommand(productId, variantId, photos);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Upload Product Images Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return CreatedAtAction(
            actionName: nameof(GetProductImagesByProduct),
            routeValues: new { productId = result.Value.ProductId },
            value: result.Value);
    }

    [HttpDelete("images/{imageId:guid}")]
    public async Task<IActionResult> DeleteProductImage(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductImageCommand(imageId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Delete Product Image Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return NoContent();
    }
}
