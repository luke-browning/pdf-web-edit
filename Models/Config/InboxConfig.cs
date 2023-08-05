namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A inbox configuration.
    /// </summary>
    public class InboxConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether the save as is shown.
        /// </summary>
        /// <value>
        /// True if show save as, false if not.
        /// </value>
        public bool ShowSaveAs { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the save is shown.
        /// </summary>
        /// <value>
        /// True if show save, false if not.
        /// </value>
        public bool ShowSave { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the revert is shown.
        /// </summary>
        /// <value>
        /// True if show revert, false if not.
        /// </value>
        public bool ShowRevert { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the download is shown.
        /// </summary>
        /// <value>
        /// True if show download, false if not.
        /// </value>
        public bool ShowDownload { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the archive is shown.
        /// </summary>
        /// <value>
        /// True if show archive, false if not.
        /// </value>
        public bool ShowArchive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the rename is shown.
        /// </summary>
        /// <value>
        /// True if show rename, false if not.
        /// </value>
        public bool ShowRename { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the reverse page order is shown.
        /// </summary>
        /// <value>
        /// True if show reverse page order, false if not.
        /// </value>
        public bool ShowReversePageOrder { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the merge is shown.
        /// </summary>
        /// <value>
        /// True if show merge, false if not.
        /// </value>
        public bool ShowMerge { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the split is shown.
        /// </summary>
        /// <value>
        /// True if show split, false if not.
        /// </value>
        public bool ShowSplit { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the remove is shown.
        /// </summary>
        /// <value>
        /// True if show remove, false if not.
        /// </value>
        public bool ShowRemove { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the rotate clockwise is shown.
        /// </summary>
        /// <value>
        /// True if show rotate clockwise, false if not.
        /// </value>
        public bool ShowRotateClockwise { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the rotate anti clockwise is shown.
        /// </summary>
        /// <value>
        /// True if show rotate anti clockwise, false if not.
        /// </value>
        public bool ShowRotateAntiClockwise { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the select all is shown.
        /// </summary>
        /// <value>
        /// True if show select all, false if not.
        /// </value>
        public bool ShowSelectAll { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the unselect is shown.
        /// </summary>
        /// <value>
        /// True if show unselect, false if not.
        /// </value>
        public bool ShowUnselect { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the batch show archive.
        /// </summary>
        /// <value>
        /// True if batch show archive, false if not.
        /// </value>
        public bool BatchShowArchive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the batch show download.
        /// </summary>
        /// <value>
        /// True if batch show download, false if not.
        /// </value>
        public bool BatchShowDownload { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the batch show save as.
        /// </summary>
        /// <value>
        /// True if batch show save as, false if not.
        /// </value>
        public bool BatchShowSaveAs { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the batch show save.
        /// </summary>
        /// <value>
        /// True if batch show save, false if not.
        /// </value>
        public bool BatchShowSave { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to archive the original file on save .
        /// </summary>
        /// <value>
        /// True if archive original file on save, false if not.
        /// </value>
        public bool ArchiveOriginalFileOnSave { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the document on archive wil be deleted.
        /// </summary>
        /// <value>
        /// True if delete document on archive, false if not.
        /// </value>
        public bool DeleteDocumentOnArchive { get; set; } = false;
    }
}
