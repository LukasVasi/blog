using Blog.Application.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.MVC.Utility
{
    internal static class ResultErrorToModelErrorMapper
    {
        /// <summary>
        /// Adds validation errors from the provided result to the provided model state dictionary.
        /// </summary>
        /// <param name="modelState">
        /// The model state dictionary where the errors should be added.
        /// </param>
        /// <param name="result">
        /// The failed result object that contains the errors.
        /// </param>
        public static void AddResultErrorsToModelState(ModelStateDictionary modelState, ResultBase result)
        {
            if (result.IsFailed)
            {
                foreach (var error in result.Errors)
                {
                    if (error is ValidationError)
                    {
                        var validationError = error as ValidationError;
                        modelState.AddModelError(validationError?.Key ?? string.Empty , error.Message);
                    }
                }
            }
        }
    }
}
