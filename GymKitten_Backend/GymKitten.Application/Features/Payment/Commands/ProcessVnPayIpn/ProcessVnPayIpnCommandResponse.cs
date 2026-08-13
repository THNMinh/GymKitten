namespace GymKitten.Application.Features.Payment.Commands.ProcessVnPayIpn;

public sealed record ProcessVnPayIpnCommandResponse(
    bool Success,
    string RspCode,
    string Message);
