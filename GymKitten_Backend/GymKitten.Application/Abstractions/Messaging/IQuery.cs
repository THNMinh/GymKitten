using MediatR;

namespace GymKitten.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}
