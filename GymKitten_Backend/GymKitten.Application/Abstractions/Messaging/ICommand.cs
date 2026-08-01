using MediatR;

namespace GymKitten.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
