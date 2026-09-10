using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class InventoryTransactionErrors
{
    public static readonly Error NotFound = new(
        "InventoryTransaction.NotFound",
        "The requested inventory transaction was not found.");

    public static readonly Error Forbidden = new(
        "InventoryTransaction.Forbidden",
        "You do not have permission to access inventory transactions.");
}
