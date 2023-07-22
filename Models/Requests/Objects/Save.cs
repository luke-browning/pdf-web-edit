namespace PDFWebEdit.Models.Requests.Objects
{
    /// <summary>
    /// A save.
    /// </summary>
    public class Save
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
    }
}
