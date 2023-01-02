namespace PDFWebEdit.Models
{
    /// <summary>
    /// A configuration.
    /// </summary>
    public class Config
    {
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
