using PDFWebEdit.Enumerations;
using PDFWebEdit.Models;

namespace PDFWebEdit.Services
{
    /// <summary>
    /// Interface for ipdf manipulation service.
    /// </summary>
    public interface IPDFManipulationService
    {
        /// <summary>
        /// Rotate clockwise.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void RotateClockwise(TargetDirectory targetDirectory, string document, List<int> pageNumbers, string? subDirectory = null);

        /// <summary>
        /// Rotate anti clockwise.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void RotateAntiClockwise(TargetDirectory targetDirectory, string document, List<int> pageNumbers, string? subDirectory = null);

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void DeletePage(TargetDirectory targetDirectory, string document, List<int> pageNumbers, string? subDirectory = null);

        /// <summary>
        /// Reorder pages.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="newPageOrder">The new page order.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void ReorderPages(TargetDirectory targetDirectory, string document, List<int> newPageOrder, string? subDirectory = null);

        /// <summary>
        /// Splits the pages into a new document.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pages">The pages.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <returns>The name of the new document.</returns>
        public string SplitPages(TargetDirectory targetDirectory, string document, List<int> pages, string? subDirectory = null);

        /// <summary>
        /// Merge a document into another document.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The source document.</param>
        /// <param name="mergeDocument">The document to append.</param>
        /// <param name="subDirectory">Subdirectory storing the source document.</param>
        /// <param name="mergeDocumentSubDirectory">Subdirectory storing the merge document.</param>
        public void MergeDocument(TargetDirectory targetDirectory, string document, string mergeDocument, string? subDirectory = null, string? mergeDocumentSubDirectory = null);

        /// <summary>
        /// Revert changes.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void RevertChanges(TargetDirectory targetDirectory, string document, string? subDirectory = null);

        /// <summary>
        /// Unlocks an encrypted file.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="password">The password.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void Unlock(TargetDirectory targetDirectory, string document, string password, string? subDirectory = null);
    }
}
