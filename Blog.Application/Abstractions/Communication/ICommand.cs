namespace Blog.Application.Abstractions.Communication
{
    public interface ICommand<TResponse> : IRequest<TResponse>;
}
