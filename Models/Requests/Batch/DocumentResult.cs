namespace PDFWebEdit.Models.Requests.Batch
{
    /// <summary>
    /// Encapsulates the result of a document.
    /// </summary>
    public class DocumentResult : IBatchResult
    {
        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public string Document { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets information describing the additional.
        /// </summary>
        /// <value>
        /// Information describing the additional.
        /// </value>
        public string? AdditionalInformation { get; set; }
    }
}
