using PDFWebEdit.Enumerations;

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
        /// Gets or sets the pathname of the directory.
        /// </summary>
        /// <value>
        /// The pathname of the directory.
        /// </value>
        public string Directory { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating whether this object has changes.
        /// </summary>
        /// <value>
        /// True if this object has changes, false if not.
        /// </value>
        public bool HasChanges { get; set; }

        /// <summary>
        /// Gets or sets the document status.
        /// </summary>
        /// <value>
        /// The document status.
        /// </value>
        public DocumentStatus Status { get; set; }
    }
}
