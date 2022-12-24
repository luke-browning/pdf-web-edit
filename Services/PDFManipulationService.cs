using iText.Kernel.Pdf;

namespace PDFEdit.Services
{
    /// <summary>
    /// A PDF manipulation service.
    /// </summary>
    public class PDFManipulationService
    {
        /// <summary>
        /// The directory service.
        /// </summary>
        private readonly DirectoryService _directoryService;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFEdit.Services.PDFManipulationService"/>
        /// class.
        /// </summary>
        /// <param name="directoryService">The directory service.</param>
        public PDFManipulationService(DirectoryService directoryService)
        {
            _directoryService = directoryService;
        }

        /// <summary>
        /// Rotates pages clockwise.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers to rotate.</param>
        public void RotateClockwise(string document, List<int> pageNumbers)
        {
            var path = _directoryService.GetDocumentPath(document);
            var outPath = _directoryService.GetEditingDocumentPath(document);

            var ms = new MemoryStream();

            // Open the document
            var pdfDocument = new PdfDocument(new PdfReader(path), new PdfWriter(ms));

            // Rotate the requested pages
            foreach (var pageNumber in pageNumbers)
            {
                var page = pdfDocument.GetPage(pageNumber);
                var rotation = page.GetRotation();
                page.SetRotation((rotation + 90) % 360);
            }

            // Close the doc before getting the bytes from the memory stream
            // otherwise the output file is corrupt
            pdfDocument.Close();

            var bytes = ms.ToArray();

            // Update the file
            using (FileStream file = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                file.Write(bytes, 0, bytes.Length);
                ms.Close();
            }
        }

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers.</param>
        public void DeletePage(string document, List<int> pageNumbers)
        {
            var path = _directoryService.GetDocumentPath(document);
            var outPath = _directoryService.GetEditingDocumentPath(document);

            var ms = new MemoryStream();

            // Open the document
            var pdfDocument = new PdfDocument(new PdfReader(path), new PdfWriter(ms));

            // Sort the page numbers so we go start to finish
            pageNumbers.Sort();

            // Keep track of how many pages we remove so we can update the index
            // when multiple pages are being removed
            var pagesRemoved = 0;

            // Remove the pages
            foreach (var pageNumber in pageNumbers)
            {
                var pageToRemove = pageNumber - pagesRemoved;
                pdfDocument.RemovePage(pageToRemove);

                pagesRemoved++;
            }

            // Close the doc before getting the bytes from the memory stream
            // otherwise the output file is corrupt
            pdfDocument.Close();

            var bytes = ms.ToArray();

            // Update the file
            using (FileStream file = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                file.Write(bytes, 0, bytes.Length);
                ms.Close();
            }
        }

        /// <summary>
        /// Reorder pages.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="newPageOrder">The new page order.</param>
        public void ReorderPages(string document, List<int> newPageOrder)
        {
            var path = _directoryService.GetDocumentPath(document);
            var outPath = _directoryService.GetEditingDocumentPath(document);

            var ms = new MemoryStream();

            // Open the document
            var sourceDocument = new PdfDocument(new PdfReader(path));
            var targetDocument = new PdfDocument(new PdfWriter(ms));

            // Copy the pages into the new document
            sourceDocument.CopyPagesTo(newPageOrder, targetDocument);

            // Close the doc before getting the bytes from the memory stream
            // otherwise the output file is corrupt
            targetDocument.Close();
            sourceDocument.Close();

            var bytes = ms.ToArray();

            // Update the file
            using (FileStream file = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                file.Write(bytes, 0, bytes.Length);
                ms.Close();
            }
        }

        /// <summary>
        /// Revert changes.
        /// </summary>
        /// <param name="document">The document.</param>
        public void RevertChanges(string document)
        {
            var outPath = _directoryService.GetEditingDocumentPath(document);

            if (File.Exists(outPath))
            {
                File.Delete(outPath);
            }
        }
    }
}
