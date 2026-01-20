using FluentResults;

namespace Blog.Application.Errors
{
    public class UnauthorizedError : Error
    {
        public UnauthorizedError(string message) : base(message) { }
    }
}
