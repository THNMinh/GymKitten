using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Payment.Commands.ProcessMomoReturn;

public sealed record ProcessMomoReturnCommand(
    string OrderId,
    int ResultCode,
    string? Message = null,
    long? Amount = null,
    long? TransId = null) : ICommand<Result>;
