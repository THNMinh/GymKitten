using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Payment.Commands.ProcessVnPayIpn;

public sealed class ProcessVnPayIpnCommandHandler
    : ICommandHandler<ProcessVnPayIpnCommand, Result<ProcessVnPayIpnCommandResponse>>
{
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessVnPayIpnCommandHandler(
        IPaymentTransactionRepository paymentTransactionRepository,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentTransactionRepository = paymentTransactionRepository;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProcessVnPayIpnCommandResponse>> Handle(
        ProcessVnPayIpnCommand request,
        CancellationToken cancellationToken)
    {
        var callback = request.CallbackData;

        if (string.IsNullOrWhiteSpace(callback.TxnRef))
        {
            return Result.Success(new ProcessVnPayIpnCommandResponse(false, "01", "Order not found"));
        }

        var transaction = await _paymentTransactionRepository.GetByTxnRefAsync(callback.TxnRef, cancellationToken);
        if (transaction is null)
        {
            return Result.Success(new ProcessVnPayIpnCommandResponse(false, "01", "Order not found"));
        }

        // Idempotency check: If transaction is already processed, return success to VNPay
        if (transaction.Status != "Pending")
        {
            return Result.Success(new ProcessVnPayIpnCommandResponse(true, "00", "Order already confirmed"));
        }

        // Check amount match
        if (transaction.Amount != callback.Amount)
        {
            return Result.Success(new ProcessVnPayIpnCommandResponse(false, "04", "Invalid amount"));
        }

        var order = await _orderRepository.GetByIdWithItemsAsync(transaction.Orderid, cancellationToken);

        if (!callback.IsSuccess)
        {
            transaction.Status = "Failed";
            transaction.Updatedat = DateTime.UtcNow;
            _paymentTransactionRepository.Update(transaction);

            if (order != null)
            {
                order.Currentstatus = "Cancelled";
                order.Updatedat = DateTime.UtcNow;
                _orderRepository.Update(order);

                // Release stock
                var variantIds = order.Orderitems.Select(i => i.Variantid).ToList();
                var inventoryItems = await _inventoryRepository.GetByVariantIdsAsync(variantIds, cancellationToken);

                foreach (var item in order.Orderitems)
                {
                    var inventory = inventoryItems.FirstOrDefault(i => i.Variantid == item.Variantid);
                    if (inventory != null)
                    {
                        inventory.Quantityreserved = Math.Max(0, inventory.Quantityreserved - item.Quantity);
                        inventory.Updatedat = DateTime.UtcNow;
                        _inventoryRepository.Update(inventory);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(new ProcessVnPayIpnCommandResponse(false, "00", "Payment failed"));
        }

        // Payment Success
        transaction.Status = "Success";
        transaction.Gatewaytransactionid = string.IsNullOrEmpty(callback.TransactionNo) ? callback.TxnRef : callback.TransactionNo;
        transaction.Paymentdate = DateTime.UtcNow;
        transaction.Updatedat = DateTime.UtcNow;
        _paymentTransactionRepository.Update(transaction);

        if (order != null)
        {
            order.Paymentstatus = "Paid";
            order.Currentstatus = "Processing";
            order.Updatedat = DateTime.UtcNow;
            _orderRepository.Update(order);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProcessVnPayIpnCommandResponse(true, "00", "Confirm Success"));
    }
}
