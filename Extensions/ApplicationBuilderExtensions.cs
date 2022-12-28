namespace PDFWebEdit.Extensions
{
    /// <summary>
    /// An application builder extensions.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// An IApplicationBuilder extension method that adds a global error handler.
        /// </summary>
        /// <param name="applicationBuilder">The applicationBuilder to act on.</param>
        /// <returns>
        /// An IApplicationBuilder.
        /// </returns>
        public static IApplicationBuilder AddGlobalErrorHandler(this IApplicationBuilder applicationBuilder)
            => applicationBuilder.UseMiddleware<GlobalErrorHandlingMiddleware>();
    }
}
