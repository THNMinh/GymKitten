using GymKitten.Application.Features.Catalog.Categories.Commands.CreateCategory;
using GymKitten.Application.Features.Catalog.Categories.Commands.DeleteCategory;
using GymKitten.Application.Features.Catalog.Categories.Commands.UpdateCategory;
using GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;
using GymKitten.Application.Features.Catalog.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
    {
        var query = new GetAllCategoriesQuery();
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Category Not Found",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Create Category Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return CreatedAtAction(
            actionName: nameof(GetCategoryById),
            routeValues: new { id = result.Value.CategoryId },
            value: result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.CategoryId)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid CategoryId",
                Detail = "Route CategoryId does not match command CategoryId."
            });
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Update Category Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Delete Category Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return NoContent();
    }
}
