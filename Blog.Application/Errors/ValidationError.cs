using FluentResults;

namespace Blog.Application.Errors
{
    public class ValidationError : Error
    {
        /// <summary>
        /// The key that this validation error is associated with.
        /// The view model property name in MVC, the json key in API requests.
        /// </summary>
        public string? Key { get; init; } = null;

        public ValidationError(string message) : base(message) { }

        public ValidationError(string message, string key) : base(message)
        {
            Key = key;
        }
    }
}
