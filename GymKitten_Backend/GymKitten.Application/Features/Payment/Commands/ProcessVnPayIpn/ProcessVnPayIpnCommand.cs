using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Payment.Commands.ProcessVnPayIpn;

public sealed record ProcessVnPayIpnCommand(
    VnPayCallbackData CallbackData) : ICommand<Result<ProcessVnPayIpnCommandResponse>>;
