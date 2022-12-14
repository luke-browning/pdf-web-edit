using Microsoft.AspNetCore.Mvc;
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
        public IEnumerable<string> GetDocuments()
        {
            return _directoryService.GetDocumentList();
        }

        /// <summary>
        /// Gets page count of the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The page count.
        /// </returns>
        [HttpGet]
        [Route("{document}")]
        public IActionResult GetPageCount(string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                var info = _pdfService.GetPageCount(path);

                return Ok(info);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets page preview in the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="pageNumber">The page number (starting at 1).</param>
        /// <returns>
        /// The page preview.
        /// </returns>
        [HttpGet]
        [Route("{document}/{pageNumber}")]
        public IActionResult GetPagePreview(string document, int pageNumber)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            // Make sure pages are 1 based
            pageNumber = pageNumber - 1;

            // Make sure the page number is in the correct range 
            if ((pageNumber < 0) || (pageNumber > (_pdfService.GetPageCount(path) - 1)))
            {
                return NotFound();
            }

            if (path != null)
            {
                var img = _pdfService.GetPagePreview(path, pageNumber, ImageFormat.Png);

                return File(img, "image/png");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Rotate page in the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="pageNumber">The page number (starting at 1).</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/{pageNumber}/rotate")]
        public IActionResult RotatePage(string document, int pageNumber)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _pdfManipulationService.RotateClockwise(document, pageNumber);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes the page in the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="pageNumber">The page number (starting at 1).</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/{pageNumber}/delete")]
        public IActionResult DeletePage(string document, int pageNumber)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(document);

            if (path != null)
            {
                _pdfManipulationService.DeletePage(document, pageNumber);

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
        [Route("{document}/reorder")]
        public IActionResult ReorderPages(string document, List<int> newPageOrder)
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
