using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Payment.Commands.ProcessMomoIpn;

public sealed class ProcessMomoIpnCommandHandler
    : ICommandHandler<ProcessMomoIpnCommand, Result<ProcessMomoIpnCommandResponse>>
{
    private readonly IMomoService _momoService;
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessMomoIpnCommandHandler(
        IMomoService momoService,
        IPaymentTransactionRepository paymentTransactionRepository,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork)
    {
        _momoService = momoService;
        _paymentTransactionRepository = paymentTransactionRepository;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProcessMomoIpnCommandResponse>> Handle(
        ProcessMomoIpnCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Security Check: Reconstruct raw hash string & verify signature via IMomoService
        var isSignatureValid = _momoService.VerifyIpnSignature(
            request.Signature,
            request.Amount,
            request.ExtraData ?? string.Empty,
            request.Message ?? string.Empty,
            request.OrderId,
            request.OrderInfo ?? string.Empty,
            request.OrderType ?? string.Empty,
            request.PartnerCode,
            request.PayType ?? string.Empty,
            request.RequestId,
            request.ResponseTime,
            request.ResultCode,
            request.TransId);

        if (!isSignatureValid)
        {
            return Result.Failure<ProcessMomoIpnCommandResponse>(PaymentErrors.InvalidSignature);
        }

        // 2. Parse OrderId & Fetch Transaction
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            return Result.Failure<ProcessMomoIpnCommandResponse>(OrderErrors.NotFound);
        }

        var transaction = await _paymentTransactionRepository.GetByOrderIdAndGatewayAsync(orderId, "MoMo", cancellationToken);
        if (transaction is null)
        {
            return Result.Failure<ProcessMomoIpnCommandResponse>(PaymentErrors.TransactionNotFound);
        }

        // 3. Idempotency Check
        if (transaction.Status != "Pending")
        {
            return Result.Success(new ProcessMomoIpnCommandResponse(true, "Already processed"));
        }

        var order = await _orderRepository.GetByIdWithItemsAsync(orderId, cancellationToken);

        // 4. Handle Result Code
        if (request.ResultCode == 0)
        {
            // Payment Success
            transaction.Status = "Success";
            transaction.Gatewaytransactionid = request.TransId.ToString();
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
        }
        else
        {
            // Payment Failed
            transaction.Status = "Failed";
            transaction.Updatedat = DateTime.UtcNow;
            _paymentTransactionRepository.Update(transaction);

            if (order != null)
            {
                order.Currentstatus = "Cancelled";
                order.Updatedat = DateTime.UtcNow;
                _orderRepository.Update(order);

                // Release inventory lock
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
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProcessMomoIpnCommandResponse(true, request.ResultCode == 0 ? "Payment Success" : "Payment Failed"));
    }
}
