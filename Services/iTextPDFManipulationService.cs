using Docnet.Core.Exceptions;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using PDFWebEdit.Enumerations;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models;

namespace PDFWebEdit.Services
{
    /// <summary>
    /// A PDF manipulation service using iText.
    /// </summary>
    public class iTextPDFManipulationService : IPDFManipulationService
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
        /// Initialises a new instance of the <see cref="PDFWebEdit.Services.iTextPDFManipulationService"/>
        /// class.
        /// </summary>
        /// <param name="directoryService">The directory service.</param>
        /// <param name="docNet">The DocNet service.</param>
        public iTextPDFManipulationService(DirectoryService directoryService, DocNetSingleton docNet)
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
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void RotateClockwise(TargetDirectory targetDirectory, string document, List<int> pageNumbers, string? subDirectory = null)
        {
            Rotate(targetDirectory, document, pageNumbers, 90, subDirectory);
        }

        /// <summary>
        /// Rotates pages anti clockwise.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers to rotate.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void RotateAntiClockwise(TargetDirectory targetDirectory, string document, List<int> pageNumbers, string? subDirectory = null)
        {
            Rotate(targetDirectory, document, pageNumbers, -90, subDirectory);
        }

        /// <summary>
        /// Rotates pages.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers to rotate.</param>
        /// <param name="degree">The degree.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void Rotate(TargetDirectory targetDirectory, string document, List<int> pageNumbers, int degree, string? subDirectory = null)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, subDirectory, document);

            // Check if the doc exists
            if (path == null)
            {
                throw new Exception($"The document does not exist: {path}");
            }

            byte[]? bytes = null;

            using (var open = File.OpenRead(path))
            using (var save = new MemoryStream())
            {
                // Open the document
                using (var pdfDocument = new PdfDocument(new PdfReader(open), new PdfWriter(save)))
                {
                    // Rotate the requested pages
                    foreach (var pageNumber in pageNumbers)
                    {
                        var page = pdfDocument.GetPage(pageNumber);
                        var rotation = page.GetRotation();
                        page.SetRotation((rotation + degree) % 360);
                    }

                    // Close the doc before getting the bytes from the memory stream
                    // otherwise the output file is corrupt
                    pdfDocument.Close();

                    // Store the doc 
                    bytes = save.ToArray();
                }
            }

            // Write the file
            FileHelpers.WriteFile(outPath, bytes);
        }

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void DeletePage(TargetDirectory targetDirectory, string document, List<int> pageNumbers, string? subDirectory = null)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, subDirectory, document);

            // Check if the doc exists
            if (path == null)
            {
                throw new Exception($"The document does not exist: {path}");
            }

            byte[]? bytes = null;

            using (var open = File.OpenRead(path))
            using (var save = new MemoryStream())
            {
                // Open the document
                using (var pdfDocument = new PdfDocument(new PdfReader(open), new PdfWriter(save)))
                {
                    // Sort the page numbers so we go start to finish
                    pageNumbers.Sort();

                    // Check we will leave the document with at least 1 page
                    if (pdfDocument.GetNumberOfPages() == 1)
                    {
                        throw new Exception("Unable to remove page. A document must contain at least 1 page.");
                    }

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

                    // Store the doc 
                    bytes = save.ToArray();
                }
            }

            // Write the file
            FileHelpers.WriteFile(outPath, bytes);
        }

        /// <summary>
        /// Reorder pages.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="newPageOrder">The new page order.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void ReorderPages(TargetDirectory targetDirectory, string document, List<int> newPageOrder, string? subDirectory = null)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, subDirectory, document);

            // Check if the doc exists
            if (path == null)
            {
                throw new Exception($"The document does not exist: {path}");
            }

            byte[]? bytes = null;

            using (var open = File.OpenRead(path))
            using (var save = new MemoryStream())
            {
                // Open the document
                using (var sourceDocument = new PdfDocument(new PdfReader(open)))
                using (var targetDocument = new PdfDocument(new PdfWriter(save)))
                {
                    // Copy the pages into the new document
                    sourceDocument.CopyPagesTo(newPageOrder, targetDocument);

                    // Close the doc before getting the bytes from the memory stream
                    // otherwise the output file is corrupt
                    targetDocument.Close();
                    sourceDocument.Close();

                    // Store the doc 
                    bytes = save.ToArray();
                }
            }

            // Write the file
            FileHelpers.WriteFile(outPath, bytes);
        }

        /// <summary>
        /// Splits the pages into a new document.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pages">The pages.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <returns>The name of the new document.</returns>
        public string SplitPages(TargetDirectory targetDirectory, string document, List<int> pages, string? subDirectory = null)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);
            var outPath = _directoryService.GetNextDocumentVersionNumberPath(targetDirectory, subDirectory, document);

            // Check if the doc exists
            if (path == null)
            {
                throw new Exception($"The document does not exist: {document}");
            }

            byte[]? bytes = null;

            using (var open = File.OpenRead(path))
            using (var save = new MemoryStream())
            {
                // Open the document
                using (var sourceDocument = new PdfDocument(new PdfReader(open)))
                using (var targetDocument = new PdfDocument(new PdfWriter(save)))
                {
                    // Copy the pages into the new document
                    sourceDocument.CopyPagesTo(pages, targetDocument);

                    // Close the doc before getting the bytes from the memory stream
                    // otherwise the output file is corrupt
                    targetDocument.Close();
                    sourceDocument.Close();

                    // Store the doc 
                    bytes = save.ToArray();
                }
            }

            // Write the file
            FileHelpers.WriteFile(outPath, bytes);

            // Return the name of the new document
            return Path.GetFileNameWithoutExtension(outPath);
        }

        /// <summary>
        /// Merge a document into another document.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The source document.</param>
        /// <param name="mergeDocument">The document to append.</param>
        /// <param name="subDirectory">Subdirectory storing the source document.</param>
        /// <param name="mergeDocumentSubDirectory">Subdirectory storing the merge document.</param>
        public void MergeDocument(TargetDirectory targetDirectory, string document, string mergeDocument, string? subDirectory = null, string? mergeDocumentSubDirectory = null)
        {
            var sourcePath = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);
            var targetPath = _directoryService.GetDocumentPath(targetDirectory, mergeDocumentSubDirectory, mergeDocument);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, subDirectory, document);

            // Check if the doc exists
            if (sourcePath == null)
            {
                throw new Exception($"The document does not exist: {sourcePath}");
            }

            if (targetPath == null)
            {
                throw new Exception($"The document does not exist: {targetPath}");
            }

            byte[]? bytes = null;

            using (var open = File.OpenRead(sourcePath))
            using (var target = File.OpenRead(targetPath))
            using (var save = new MemoryStream())
            {
                // Open the document
                using (var sourceDocument = new PdfDocument(new PdfReader(open)))
                using (var targetDocument = new PdfDocument(new PdfReader(target)))
                using (var outputDocument = new PdfDocument(new PdfWriter(save)))
                {
                    var merger = new PdfMerger(outputDocument);

                    // Copy the pages into the new document
                    merger.Merge(sourceDocument, 1, sourceDocument.GetNumberOfPages());
                    merger.Merge(targetDocument, 1, targetDocument.GetNumberOfPages());

                    // Close the doc before getting the bytes from the memory stream
                    // otherwise the output file is corrupt
                    outputDocument.Close();
                    sourceDocument.Close();
                    targetDocument.Close();

                    merger.Close();

                    // Store the doc 
                    bytes = save.ToArray();
                }
            }

            // Write the file
            FileHelpers.WriteFile(outPath, bytes);
        }

        /// <summary>
        /// Revert changes.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void RevertChanges(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, subDirectory, document);

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
        /// <param name="subDirectory">Subdirectory storing document.</param>
        public void Unlock(TargetDirectory targetDirectory, string document, string password, string? subDirectory = null)
        {
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);
            var outPath = _directoryService.GetEditingDocumentPath(targetDirectory, subDirectory, document);

            // Check if the doc exists
            if (path == null)
            {
                throw new Exception($"The document does not exist: {path}");
            }

            // Get the file contents 
            var file = FileHelpers.LoadFile(path);

            try
            {
                // Decrypt the file
                var decryptedFile = _docNet.Instance.Unlock(file, password);

                // Write the file
                FileHelpers.WriteFile(outPath, decryptedFile);
            }
            catch (DocnetLoadDocumentException x)
            {
                switch (x.ErrorCode)
                {
                    case 4:
                        throw new Exception("Incorrect password.");
                        break;
                }
            }
            catch (Exception x)
            {
                throw new Exception("An unknown error occurred.");
            }
        }
    }
}
