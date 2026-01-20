using FluentResults;

namespace Blog.Application.Errors
{
    public class BadRequestError : Error
    {
        public BadRequestError(string message) : base(message) { }
    }
}
