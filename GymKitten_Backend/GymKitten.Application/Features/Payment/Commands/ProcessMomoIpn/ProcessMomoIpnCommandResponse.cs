namespace GymKitten.Application.Features.Payment.Commands.ProcessMomoIpn;

public sealed record ProcessMomoIpnCommandResponse(
    bool Success,
    string Message);
