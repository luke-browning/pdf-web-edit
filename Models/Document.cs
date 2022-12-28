namespace PDFWebEdit.Models
{
    /// <summary>
    /// A document.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Date/Time of the created.
        /// </summary>
        /// <value>
        /// The created.
        /// </value>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the Date/Time of the last modified.
        /// </summary>
        /// <value>
        /// The last modified.
        /// </value>
        public DateTime LastModified { get; set; }
    }
}
