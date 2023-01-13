using PDFWebEdit.Enumerations;

namespace PDFWebEdit.Services
{
    /// <summary>
    /// Interface for PDF service.
    /// </summary>
    public interface IPDFService
    {
        /// <summary>
        /// Gets document status.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <returns>
        /// The document status.
        /// </returns>
        public DocumentStatus GetDocumentStatus(string path);

        /// <summary>
        /// Gets page count.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <returns>
        /// The page count.
        /// </returns>
        public int GetPageCount(string path);

        /// <summary>
        /// Gets page preview.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <returns>
        /// An array of byte.
        /// </returns>
        public byte[] GetPagePreview(string path, int pageNumber, int width, int height);
    }
}
