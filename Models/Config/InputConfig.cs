namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A input configuration.
    /// </summary>
    public class InputConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether the save to is shown.
        /// </summary>
        /// <value>
        /// True if show save to, false if not.
        /// </value>
        public bool ShowSaveTo { get; set; } = true;

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
        /// Gets or sets a value indicating whether the delete is shown.
        /// </summary>
        /// <value>
        /// True if show delete, false if not.
        /// </value>
        public bool ShowDelete { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the rename is shown.
        /// </summary>
        /// <value>
        /// True if show rename, false if not.
        /// </value>
        public bool ShowRename { get; set; } = true;

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
        /// Gets or sets a value indicating whether the original file on save wil be deleted.
        /// </summary>
        /// <value>
        /// True if delete original file on save, false if not.
        /// </value>
        public bool DeleteOriginalFileOnSave { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the move files to trash on delete.
        /// </summary>
        /// <value>
        /// True if move files to trash on delete, false if not.
        /// </value>
        public bool MoveFilesToTrashOnDelete { get; set; } = true;
    }
}
