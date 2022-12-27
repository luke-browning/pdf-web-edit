using Microsoft.AspNetCore.Mvc;
using PDFEdit.Models;
using PDFEdit.Services;
using System.Drawing.Imaging;

namespace PDFEdit.Controllers
{
    /// <summary>
    /// A controller for handling documents.
    /// </summary>
    /// <seealso cref="ControllerBase"/>
    [Route("api/documents")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly DirectoryService _directoryService;
        private readonly PDFService _pdfService;
        private readonly PDFManipulationService _pdfManipulationService;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFEdit.Controllers.DocumentController"/>
        /// class.
        /// </summary>
        /// <param name="directoryService">The directory service.</param>
        /// <param name="pdfService">The PDF service.</param>
        /// <param name="pdfManipulationService">The PDF manipulation service.</param>
        public DocumentController(DirectoryService directoryService, PDFService pdfService, PDFManipulationService pdfManipulationService)
        {
            _directoryService = directoryService;
            _pdfService = pdfService;
            _pdfManipulationService = pdfManipulationService;
        }

        /// <summary>
        /// Gets the documents in the input folder.
        /// </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the documents in this collection.
        /// </returns>
        [HttpGet]
        [Route("list")]
        public IEnumerable<Document> GetDocuments()
        {
            return _directoryService.GetDocumentList();
        }

        /// <summary>
        /// Gets a document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The document.
        /// </returns>
        [HttpGet]
        [Route("{document}/download")]
        public IActionResult GetDocument(string document)
        {
            // Get the path to the document
            var downloadFilePath = _directoryService.GetDocumentPath(document);
            var originalFilePath = _directoryService.GetUnmodifiedDocumentPath(document);

            if (downloadFilePath != null)
            {
                var bytes = _directoryService.GetDocumentBytes(document);

                return File(bytes, "application/pdf", Path.GetFileName(originalFilePath));
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets page count of the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The page count.
        /// </returns>
        [HttpGet]
        [Route("{document}/page-count")]
        public int GetPageCount(string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                var pageCount = _pdfService.GetPageCount(path);

                return pageCount;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets page preview in the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="pageNumber">The page number (starting at 1).</param>
        /// <param name="width">The page width.</param>
        /// <param name="height">The page height.</param>
        /// <returns>
        /// The page preview.
        /// </returns>
        [HttpGet]
        [Route("{document}/preview/{pageNumber}")]
        public IActionResult GetPagePreview(string document, int pageNumber, int width, int height)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            // Make sure pages are 1 based
            pageNumber = pageNumber - 1;

            if (path != null)
            {
                // Make sure the page number is in the correct range 
                if ((pageNumber < 0) || (pageNumber > (_pdfService.GetPageCount(path) - 1)))
                {
                    return NotFound();
                }
                else
                {
                    var img = _pdfService.GetPagePreview(path, pageNumber, width, height);

                    return File(img, "image/png");
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Renames a document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="newDocumentName">New name of the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/rename/{newDocumentName}")]
        public IActionResult Rename(string document, string newDocumentName)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _directoryService.Rename(document, newDocumentName);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Rotates pages in the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers (1-based).</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/rotate-pages")]
        public IActionResult RotatePages(string document, [FromBody] List<int> pageNumbers)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _pdfManipulationService.RotateClockwise(document, pageNumbers);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes the pages in the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers (1-based).</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/delete-pages")]
        public IActionResult DeletePages(string document, [FromBody] List<int> pageNumbers)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _pdfManipulationService.DeletePage(document, pageNumbers);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Reorder pages in the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="newPageOrder">The new page order.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/reorder-pages")]
        public IActionResult ReorderPages(string document, [FromBody] List<int> newPageOrder)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _pdfManipulationService.ReorderPages(document, newPageOrder);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Revert changes to the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/revert")]
        public IActionResult RevertChanges(string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _pdfManipulationService.RevertChanges(document);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/delete")]
        public IActionResult Delete(string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _directoryService.Delete(document);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Saves the specified document to the output directory.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/save")]
        public IActionResult Save(string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _directoryService.Save(document);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
