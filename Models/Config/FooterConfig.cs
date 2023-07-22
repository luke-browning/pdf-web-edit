namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A footer configuration.
    /// </summary>
    public class FooterConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether the always is shown.
        /// </summary>
        /// <value>
        /// True if show always, false if not.
        /// </value>
        public bool ShowAlways { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the select all is shown.
        /// </summary>
        /// <value>
        /// True if show select all, false if not.
        /// </value>
        public bool ShowSelectAll { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the deselect all is shown.
        /// </summary>
        /// <value>
        /// True if show deselect all, false if not.
        /// </value>
        public bool ShowDeselectAll { get; set; } = true;
    }
}
