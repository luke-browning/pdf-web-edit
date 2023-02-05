namespace PDFWebEdit.Models
{
    /// <summary>
    /// A folder.
    /// </summary>
    public class Folder
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sub folders.
        /// </summary>
        /// <value>
        /// The sub folders.
        /// </value>
        public IEnumerable<Folder> SubFolders { get; set; }
    }
}
