using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Catalog.Categories.Commands.CreateCategory;
using GymKitten.Application.Features.Catalog.Categories.Commands.DeleteCategory;
using GymKitten.Application.Features.Catalog.Categories.Commands.UpdateCategory;
using GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;
using GymKitten.Application.Features.Catalog.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetAllCategories(
        [FromQuery] GetAllCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetCategoryById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetCategoryByIdQuery(id), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCategoryCommand(
            request.ParentCategoryId,
            request.Name,
            request.Slug,
            request.Description,
            request.DisplayOrder);

        var result = await _sender.Send(command, cancellationToken);
        // return result.MatchCreated(val => $"/api/categories/{val.CategoryId}");
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.ParentCategoryId,
            request.Name,
            request.Slug,
            request.Description,
            request.DisplayOrder);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<IResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        return result.MatchOk();
    }
}
