using System.Net;
using System.Text.Json;

namespace PDFEdit.Extensions
{
    /// <summary>
    /// A global error handling middleware.
    /// </summary>
    public class GlobalErrorHandlingMiddleware
    {
        /// <summary>
        /// The next.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="PDFEdit.Extensions.GlobalErrorHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        public GlobalErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Executes the given operation on a different thread, and waits for the result.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A Task.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles the exception asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// A Task.
        /// </returns>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            var stackTrace = string.Empty;
            string message;

            var exceptionType = exception.GetType();

            //if (exceptionType == typeof(BadRequestException))
            //{
            //    message = exception.Message;
            //    status = HttpStatusCode.BadRequest;
            //    stackTrace = exception.StackTrace;
            //}
            //else if (exceptionType == typeof(NotFoundException))
            //{
            //    message = exception.Message;
            //    status = HttpStatusCode.NotFound;
            //    stackTrace = exception.StackTrace;
            //}
            
            if (exceptionType == typeof(NotImplementedException))
            {
                status = HttpStatusCode.NotImplemented;
                message = exception.Message;
                stackTrace = exception.StackTrace;
            }
            else if (exceptionType == typeof(UnauthorizedAccessException))
            {
                status = HttpStatusCode.Unauthorized;
                message = exception.Message;
                stackTrace = exception.StackTrace;
            }
            else if (exceptionType == typeof(KeyNotFoundException))
            {
                status = HttpStatusCode.Unauthorized;
                message = exception.Message;
                stackTrace = exception.StackTrace;
            }
            else
            {
                status = HttpStatusCode.InternalServerError;
                message = exception.Message;
                stackTrace = exception.StackTrace;
            }

            var exceptionResult = JsonSerializer.Serialize(new { error = message, stackTrace });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(exceptionResult);
        }
    }
}
