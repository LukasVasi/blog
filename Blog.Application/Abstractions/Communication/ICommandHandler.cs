using FluentResults;

namespace Blog.Application.Abstractions.Communication
{
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>;
}
