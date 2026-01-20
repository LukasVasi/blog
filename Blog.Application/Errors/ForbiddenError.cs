using FluentResults;

namespace Blog.Application.Errors
{
    public class ForbiddenError : Error
    {
        public ForbiddenError(string message) : base(message) { }
    }
}
