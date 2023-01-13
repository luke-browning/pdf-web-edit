using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PDFWebEdit.Helpers
{
    /// <summary>
    /// An exception helpers.
    /// </summary>
    public static class ExceptionHelpers
    {
        /// <summary>
        /// Gets error object result.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="context">The context.</param>
        /// <param name="x">The Exception to process (optional).</param>
        /// <param name="message">The message (optional).</param>
        /// <param name="statusCode">The status code (optional).</param>
        /// <returns>
        /// The error object result.
        /// </returns>
        public static ObjectResult GetErrorObjectResult(string method, HttpContext context, Exception? x = null, string? message = null, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Type = "",
                Title = method,
                Detail = x?.Message ?? message ?? string.Empty,
                Instance = context?.Request?.Path
            };

            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = (int)statusCode,
            };
        }
    }
}
