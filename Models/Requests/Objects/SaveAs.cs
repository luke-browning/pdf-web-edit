namespace PDFWebEdit.Models.Requests.Objects
{
    /// <summary>
    /// A save as.
    /// </summary>
    public class SaveAs
    {
        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public string Document { get; set; }

        /// <summary>
        /// Gets or sets the pathname of the source sub directory.
        /// </summary>
        /// <value>
        /// The pathname of the source sub directory.
        /// </value>
        public string? SourceSubDirectory { get; set; } = null;

        /// <summary>
        /// Gets or sets the pathname of the target sub directory.
        /// </summary>
        /// <value>
        /// The pathname of the target sub directory.
        /// </value>
        public string? TargetSubDirectory { get; set; } = null;

        /// <summary>
        /// Gets or sets the name of the new.
        /// </summary>
        /// <value>
        /// The name of the new.
        /// </value>
        public string? NewName { get; set; } = null;

    }
}
