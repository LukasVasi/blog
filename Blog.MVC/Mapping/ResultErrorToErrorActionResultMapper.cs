using Blog.Application.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Mapping
{
    internal static class ResultErrorToErrorActionResultMapper
    {
        /// <summary>
        /// Maps an error to the corresponding <see cref="ActionResult"/>.
        /// </summary>
        /// <param name="error">
        /// The <see cref="IError"/> object that should be mapped.
        /// </param>
        /// <returns>
        /// An action result that corresponds to the type of the error.
        /// </returns>
        public static ActionResult ToErrorActionResult(this IError? error)
        {
            return error switch
            {
                UnauthorizedError => new ChallengeResult(),
                ForbiddenError => new ForbidResult(),
                NotFoundError => new NotFoundResult(),
                //BadRequestError => new BadRequestResult(),
                _ => new ViewResult { ViewName = "Error" }
            };
        }
    }
}
