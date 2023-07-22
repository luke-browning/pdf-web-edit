using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PDFWebEdit.Helpers
{
    /// <summary>
    /// An action result helpers.
    /// </summary>
    public static class ActionResultHelpers
    {
        /// <summary>
        /// An IActionResult extension method that tries to get the HTTP status code from the result.
        /// </summary>
        /// <param name="actionResult">The actionResult to act on.</param>
        /// <returns>
        /// The status code.
        /// </returns>
        public static int GetStatusCode(this IActionResult actionResult)
        {
            var statusCode = ((IStatusCodeActionResult)actionResult).StatusCode.Value;
            return statusCode;
        }

        /// <summary>
        /// An IActionResult extension method that tries to read an error message from the result.
        /// </summary>
        /// <param name="actionResult">The actionResult to act on.</param>
        /// <returns>
        /// The error message.
        /// </returns>
        public static string? GetErrorMessage(this IActionResult actionResult)
        {
            string? error = null;

            try
            {
                var errorResponse = (ObjectResult)actionResult;
                var problem = (ProblemDetails)errorResponse.Value;
                error = problem.Detail;
            }
            catch
            {
                // No need to handle failed parsing
            }

            return error;
        }
    }
}
