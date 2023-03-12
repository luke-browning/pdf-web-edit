namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A save configuration.
    /// </summary>
    public class OutputConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether the restore is shown.
        /// </summary>
        /// <value>
        /// True if show restore, false if not.
        /// </value>
        public bool ShowRestore { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the download is shown.
        /// </summary>
        /// <value>
        /// True if show download, false if not.
        /// </value>
        public bool ShowDownload { get; set; } = true;
    }
}
