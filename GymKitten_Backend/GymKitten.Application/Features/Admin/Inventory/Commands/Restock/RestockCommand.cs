using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Inventory.Commands.Restock;

public sealed record RestockCommand(
    Guid VariantId,
    int QuantityAdded,
    string Note) : ICommand<Result<RestockCommandResponse>>;
