namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A outbox configuration.
    /// </summary>
    public class OutboxConfig
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

        /// <summary>
        /// Gets or sets a value indicating whether the batch show restore.
        /// </summary>
        /// <value>
        /// True if batch show restore, false if not.
        /// </value>
        public bool BatchShowRestore { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the batch show download.
        /// </summary>
        /// <value>
        /// True if batch show download, false if not.
        /// </value>
        public bool BatchShowDownload { get; set; } = true;
    }
}
