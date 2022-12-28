using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace PDFWebEdit.Pages
{
    /// <summary>
    /// A data Model for the error.
    /// </summary>
    /// <seealso cref="PageModel"/>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<ErrorModel> _logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFWebEdit.Pages.ErrorModel"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets the identifier of the request.
        /// </summary>
        /// <value>
        /// The identifier of the request.
        /// </value>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the request identifier is shown.
        /// </summary>
        /// <value>
        /// True if show request identifier, false if not.
        /// </value>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Executes the get action.
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}