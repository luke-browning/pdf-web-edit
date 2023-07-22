namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A archive configuration.
    /// </summary>
    public class ArchiveConfig
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
        /// Gets or sets a value indicating whether the delete is shown.
        /// </summary>
        /// <value>
        /// True if show delete, false if not.
        /// </value>
        public bool ShowDelete { get; set; } = true;

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

        /// <summary>
        /// Gets or sets a value indicating whether the batch show delete.
        /// </summary>
        /// <value>
        /// True if batch show delete, false if not.
        /// </value>
        public bool BatchShowDelete { get; set; } = false;
    }
}
