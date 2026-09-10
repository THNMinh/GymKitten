using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Jobs;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.Jobs;

public class OrderAutoCancelService : IOrderAutoCancelService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderAutoCancelService> _logger;

    public OrderAutoCancelService(
        IBackgroundJobClient backgroundJobClient,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OrderAutoCancelService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public void ScheduleAutoCancel(Guid orderId, TimeSpan delay)
    {
        _backgroundJobClient.Schedule(
            () => CancelUnpaidOrderAsync(orderId),
            delay);
        _logger.LogInformation("Scheduled auto-cancel job for Order {OrderId} in {DelayMinutes} minutes", orderId, delay.TotalMinutes);
    }

    public async Task CancelUnpaidOrderAsync(Guid orderId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var inventoryRepository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
        var inventoryTransactionRepository = scope.ServiceProvider.GetRequiredService<IInventoryTransactionRepository>();
        var systemLogService = scope.ServiceProvider.GetService<ISystemLogService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var order = await orderRepository.GetByIdWithItemsAsync(orderId);
        if (order is null)
        {
            _logger.LogWarning("Auto-cancel job executing: Order {OrderId} not found.", orderId);
            return;
        }

        if (order.Paymentstatus == "Unpaid" && order.Currentstatus == "Pending")
        {
            _logger.LogInformation("Auto-cancelling unpaid order {OrderId}.", orderId);
            order.Currentstatus = "Cancelled";
            order.Updatedat = DateTime.UtcNow;
            orderRepository.Update(order);

            // Release stock
            var variantIds = order.Orderitems.Select(i => i.Variantid).ToList();
            var inventoryItems = await inventoryRepository.GetByVariantIdsAsync(variantIds);

            foreach (var item in order.Orderitems)
            {
                var inventory = inventoryItems.FirstOrDefault(i => i.Variantid == item.Variantid);
                if (inventory != null)
                {
                    inventory.Quantityreserved = Math.Max(0, inventory.Quantityreserved - item.Quantity);
                    inventory.Updatedat = DateTime.UtcNow;
                    inventoryRepository.Update(inventory);

                    var refText = $"AutoCancel #{order.Ordercode} | System";
                    if (refText.Length > 100) refText = refText[..100];

                    var cancelTxn = new GymKitten.Domain.Entities.Inventorytransaction
                    {
                        Transactionid = Guid.NewGuid(),
                        Variantid = item.Variantid,
                        Quantitychange = -item.Quantity,
                        Type = "Reserve",
                        Referenceid = refText,
                        Createdat = DateTime.UtcNow,
                        Updatedat = DateTime.UtcNow
                    };
                    await inventoryTransactionRepository.AddAsync(cancelTxn);
                }
            }

            await unitOfWork.SaveChangesAsync();

            if (systemLogService != null)
            {
                await systemLogService.LogAsync(
                    "AutoCancelOrder",
                    $"Order #{order.Ordercode} auto-cancelled due to payment timeout. Reserved inventory released.",
                    "Information",
                    order.Userid);
            }

            _logger.LogInformation("Order {OrderId} auto-cancelled and reserved inventory released successfully.", orderId);
        }
        else
        {
            _logger.LogInformation("Order {OrderId} is not in Pending/Unpaid status (Current: {Status}, Payment: {PaymentStatus}). Skipping auto-cancel.",
                orderId, order.Currentstatus, order.Paymentstatus);
        }
    }
}
