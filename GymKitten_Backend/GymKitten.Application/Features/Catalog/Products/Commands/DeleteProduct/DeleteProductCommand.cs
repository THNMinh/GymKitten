using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId) : ICommand<Result>;
