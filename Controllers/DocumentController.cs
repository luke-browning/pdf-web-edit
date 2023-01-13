using Microsoft.AspNetCore.Mvc;
using PDFWebEdit.Enumerations;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models;
using PDFWebEdit.Services;
using System.Drawing.Imaging;
using System.Net;

namespace PDFWebEdit.Controllers
{
    /// <summary>
    /// A controller for handling documents.
    /// </summary>
    /// <seealso cref="ControllerBase"/>
    [Route("api/documents")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        /// <summary>
        /// The directory service.
        /// </summary>
        private readonly DirectoryService _directoryService;

        /// <summary>
        /// The PDF service.
        /// </summary>
        private readonly IPDFService _pdfService;

        /// <summary>
        /// The PDF manipulation service.
        /// </summary>
        private readonly IPDFManipulationService _pdfManipulationService;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFWebEdit.Controllers.DocumentController"/>
        /// class.
        /// </summary>
        /// <param name="directoryService">The directory service.</param>
        /// <param name="pdfService">The PDF service.</param>
        /// <param name="pdfManipulationService">The PDF manipulation service.</param>
        public DocumentController(DirectoryService directoryService, IPDFService pdfService, IPDFManipulationService pdfManipulationService)
        {
            _directoryService = directoryService;
            _pdfService = pdfService;
            _pdfManipulationService = pdfManipulationService;
        }

        /// <summary>
        /// Gets the documents in the input folder.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the documents in this collection.
        /// </returns>
        [HttpGet]
        [Route("list/{targetDirectory}")]
        public IEnumerable<Document> GetDocuments(TargetDirectory targetDirectory)
        {
            return _directoryService.GetDocumentList(targetDirectory);
        }

        /// <summary>
        /// Gets a document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The document.
        /// </returns>
        [HttpGet]
        [Route("list/{targetDirectory}/{document}")]
        public Document GetDocument(TargetDirectory targetDirectory, string document)
        {
            return _directoryService.GetDocument(targetDirectory, document);
        }

        /// <summary>
        /// Gets a document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The document.
        /// </returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [Route("{targetDirectory}/{document}/download")]
        public IActionResult DownloadDocument(TargetDirectory targetDirectory, string document)
        {
            // Get the path to the document
            var downloadFilePath = _directoryService.GetDocumentPath(targetDirectory, document);
            var originalFilePath = _directoryService.GetUnmodifiedDocumentPath(targetDirectory, document);

            if (downloadFilePath != null)
            {
                try
                {
                    var bytes = _directoryService.GetDocumentBytes(targetDirectory, document);

                    return File(bytes, "application/pdf", Path.GetFileName(originalFilePath));
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Download", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets page count of the specified document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The page count.
        /// </returns>
        [HttpGet]
        [Route("{targetDirectory}/{document}/page-count")]
        public int GetPageCount(TargetDirectory targetDirectory, string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

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
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumber">The page number (starting at 1).</param>
        /// <param name="width">The page width.</param>
        /// <param name="height">The page height.</param>
        /// <returns>
        /// The page preview.
        /// </returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [Route("{targetDirectory}/{document}/preview/{pageNumber}")]
        public IActionResult GetPagePreview(TargetDirectory targetDirectory, string document, int pageNumber, int width, int height)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

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
                    try
                    {
                        var img = _pdfService.GetPagePreview(path, pageNumber, width, height);

                        return File(img, "image/png");
                    }
                    catch (Exception x)
                    {
                        return ExceptionHelpers.GetErrorObjectResult("Preview", HttpContext, x);
                    }
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
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="newDocumentName">New name of the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [Route("{targetDirectory}/{document}/rename/{newDocumentName}")]
        public IActionResult Rename(TargetDirectory targetDirectory, string document, string newDocumentName)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

            if (path != null)
            {
                try
                { 
                    _directoryService.Rename(targetDirectory, document, newDocumentName);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Rename", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Rotates pages in the specified document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers (1-based).</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [Route("{targetDirectory}/{document}/rotate-pages")]
        public IActionResult RotatePages(TargetDirectory targetDirectory, string document, [FromBody] List<int> pageNumbers)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

            if (path != null)
            {
                try
                { 
                    _pdfManipulationService.RotateClockwise(targetDirectory, document, pageNumbers);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Rotate", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes the pages in the specified document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers (1-based).</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [Route("{targetDirectory}/{document}/delete-pages")]
        public IActionResult DeletePages(TargetDirectory targetDirectory, string document, [FromBody] List<int> pageNumbers)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

            if (path != null)
            {
                try
                { 
                    _pdfManipulationService.DeletePage(targetDirectory, document, pageNumbers);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Remove", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Reorder pages in the specified document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="newPageOrder">The new page order.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [Route("{targetDirectory}/{document}/reorder-pages")]
        public IActionResult ReorderPages(TargetDirectory targetDirectory, string document, [FromBody] List<int> newPageOrder)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

            if (path != null)
            {
                try
                { 
                    _pdfManipulationService.ReorderPages(targetDirectory, document, newPageOrder);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Reorder", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Revert changes to the specified document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [Route("{targetDirectory}/{document}/revert")]
        public IActionResult RevertChanges(TargetDirectory targetDirectory, string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

            if (path != null)
            {
                try
                { 
                    _pdfManipulationService.RevertChanges(targetDirectory, document);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Revert", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes the specified document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [Route("{targetDirectory}/{document}/delete")]
        public IActionResult Delete(TargetDirectory targetDirectory, string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

            if (path != null)
            {
                try
                { 
                    _directoryService.Delete(targetDirectory, document);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Delete", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Unlocks the document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [Route("{targetDirectory}/{document}/unlock")]
        public IActionResult Unlock(TargetDirectory targetDirectory, string document, string password)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

            if (path != null)
            {
                try
                { 
                    _pdfManipulationService.Unlock(targetDirectory, document, password);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Unlock", HttpContext, x, statusCode: HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Restores a document from trash.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/restore")]
        public IActionResult Restore(TargetDirectory targetDirectory, string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, document);

            if (path != null)
            {
                _directoryService.Restore(targetDirectory, document);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Saves the specified document in the input directory to the output directory.
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
            var path = _directoryService.GetDocumentPath(TargetDirectory.Input, document);

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
