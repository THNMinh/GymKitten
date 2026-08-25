using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class InventoryErrors
{
    public static readonly Error NotFound = new(
        "Inventory.NotFound",
        "The inventory item for the specified variant was not found.");

    public static readonly Error InvalidQuantity = new(
        "Inventory.InvalidQuantity",
        "Quantity must be greater than 0.");
}
