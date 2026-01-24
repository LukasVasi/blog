using FluentResults;

namespace Blog.Application.Abstractions.Communication
{
    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public Task<Result<TResponse>> HandleAsync(TRequest request);
    }
}
