using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Inventory.Commands.AdjustStock;

public sealed record AdjustStockCommand(
    Guid VariantId,
    int NewQuantityOnHand,
    string Reason) : ICommand<Result<AdjustStockCommandResponse>>;
