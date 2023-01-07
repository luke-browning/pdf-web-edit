using Docnet.Core.Exceptions;
using iText.Kernel.Pdf;
using PDFWebEdit.Enumerations;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models;

namespace PDFWebEdit.Services
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
        /// The document net.
        /// </summary>
        private readonly DocNetSingleton _docNet;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFWebEdit.Services.PDFManipulationService"/>
        /// class.
        /// </summary>
        /// <param name="directoryService">The directory service.</param>
        /// <param name="docNet">The DocNet service.</param>
        public PDFManipulationService(DirectoryService directoryService, DocNetSingleton docNet)
        {
            _directoryService = directoryService;
            _docNet = docNet;
        }

        /// <summary>
        /// Rotates pages clockwise.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers to rotate.</param>
        public void RotateClockwise(TargetDirectory targetDirectory, string document, List<int> pageNumbers)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, document);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, document);

            using (var ms = new MemoryStream())
            { 

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

                // Write the file
                FileHelpers.WriteFile(outPath, bytes);
            }
        }

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers.</param>
        public void DeletePage(TargetDirectory targetDirectory, string document, List<int> pageNumbers)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, document);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, document);

            using (var ms = new MemoryStream())
            { 
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

                // Write the file
                FileHelpers.WriteFile(outPath, bytes);
            }
        }

        /// <summary>
        /// Reorder pages.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="newPageOrder">The new page order.</param>
        public void ReorderPages(TargetDirectory targetDirectory, string document, List<int> newPageOrder)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, document);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, document);

            using (var ms = new MemoryStream())
            {
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

                // Write the file
                FileHelpers.WriteFile(outPath, bytes);
            }
        }

        /// <summary>
        /// Revert changes.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        public void RevertChanges(TargetDirectory targetDirectory, string document)
        {
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, document);

            if (File.Exists(outPath))
            {
                File.Delete(outPath);
            }
        }

        /// <summary>
        /// Unlocks an encrypted file.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="password">The password.</param>
        public Status Unlock(TargetDirectory targetDirectory, string document, string password)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, document);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, document);

            // Get the file 
            var file = FileHelpers.LoadFile(path);

            var result = new Status();

            try
            {
                var decryptedFile = _docNet.Instance.Unlock(file, password);

                // Write the file
                FileHelpers.WriteFile(outPath, decryptedFile);

                result.Success = true;
            }
            catch (DocnetLoadDocumentException ex)
            {
                switch (ex.ErrorCode)
                {
                    case 4:
                        result.Message = "Incorrect password";
                        result.Success = false;
                        break;
                }
            }
            catch (Exception x)
            {
                result.Message = "An unknown error occurred";
                result.Success = false;
            }

            return result;
        }
    }
}
