using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Catalog.Products.Commands.CreateProduct;
using GymKitten.Application.Features.Catalog.Products.Commands.DeleteProduct;
using GymKitten.Application.Features.Catalog.Products.Commands.UpdateProduct;
using GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;
using GymKitten.Application.Features.Catalog.Products.Queries.GetProductById;
using GymKitten.Application.Features.Catalog.Products.Queries.GetProductBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetAllProducts(
        [FromQuery] string? q,
        [FromQuery] string? searchName,
        [FromQuery] string? gender,
        [FromQuery] string? fitType,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? categorySlug,
        [FromQuery] bool? isActive,
        [FromQuery] string? activeState,
        [FromQuery] string? colors,
        [FromQuery] string? sizes,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] decimal? minDiscountPercent,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var finalSearch = !string.IsNullOrWhiteSpace(q) ? q : searchName;

        bool? finalIsActive = isActive;
        if (!string.IsNullOrWhiteSpace(activeState) && activeState.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            finalIsActive = null;
        }

        var colorList = !string.IsNullOrWhiteSpace(colors)
            ? colors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : null;

        var sizeList = !string.IsNullOrWhiteSpace(sizes)
            ? sizes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : null;

        var query = new GetAllProductsQuery(
            finalSearch,
            gender,
            fitType,
            categoryId,
            categorySlug,
            finalIsActive,
            colorList,
            sizeList,
            minPrice,
            maxPrice,
            minDiscountPercent,
            sort,
            page,
            pageSize);

        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetProductById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("slug/{slug}")]
    public async Task<IResult> GetProductBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetProductBySlugQuery(slug), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateProductCommand(
            request.CategoryId,
            request.Name,
            request.Slug,
            request.Description,
            request.FitType,
            request.Gender);

        var result = await _sender.Send(command, cancellationToken);
        // return result.MatchCreated(val => $"/api/products/{val.ProductId}");
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProductCommand(
            id,
            request.CategoryId,
            request.Name,
            request.Slug,
            request.Description,
            request.FitType,
            request.Gender,
            request.IsActive);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new DeleteProductCommand(id), cancellationToken);
        return result.MatchOk();
    }
}
