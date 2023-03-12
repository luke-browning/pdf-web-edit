namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A preview configuration.
    /// </summary>
    public class PreviewConfig
    {
        /// <summary>
        /// Gets or sets the default size.
        /// </summary>
        /// <value>
        /// The default size.
        /// </value>
        public string DefaultSize { get; set; } = "medium";

        /// <summary>
        /// Gets or sets a value indicating whether the page number is shown.
        /// </summary>
        /// <value>
        /// True if show page number, false if not.
        /// </value>
        public bool ShowPageNumber { get; set; } = true;
    }
}
