namespace PDFWebEdit.Models.Requests.Objects
{
    /// <summary>
    /// A delete.
    /// </summary>
    public class Delete
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
        public string? SubDirectory { get; set; } = null;
    }
}
