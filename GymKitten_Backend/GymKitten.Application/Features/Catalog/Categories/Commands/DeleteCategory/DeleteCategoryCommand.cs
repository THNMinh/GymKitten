using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId) : ICommand<Result>;
