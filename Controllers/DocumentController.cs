using Microsoft.AspNetCore.Mvc;
using PDFWebEdit.Enumerations;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models;
using PDFWebEdit.Services;

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
        /// Gets the directories in this collection.
        /// </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the directories in this collection.
        /// </returns>
        [HttpGet]
        [Route("directories")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Folder>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult GetDirectories()
        {
            try
            {
                return Ok(_directoryService.GetFolders());
            }
            catch (Exception x)
            {
                return ExceptionHelpers.GetErrorObjectResult("Directories", HttpContext, x);
            }
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Document>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult GetDocuments(TargetDirectory targetDirectory)
        {
            try
            {
                return Ok(_directoryService.GetDocumentList(targetDirectory, true));
            }
            catch (Exception x)
            {
                return ExceptionHelpers.GetErrorObjectResult("Documents", HttpContext, x);
            }
        }

        /// <summary>
        /// Gets a document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// The document.
        /// </returns>
        [HttpGet]
        [Route("list/{targetDirectory}/{document}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Document))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult GetDocument(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            try
            { 
                return Ok(_directoryService.GetDocument(targetDirectory, subDirectory, document));
            }
            catch (Exception x)
            {
                return ExceptionHelpers.GetErrorObjectResult("Document", HttpContext, x);
            }
        }

        /// <summary>
        /// Gets a document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// The document.
        /// </returns>
        [HttpGet]
        [Route("{targetDirectory}/{document}/download")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult DownloadDocument(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            // Get the path to the document
            var downloadFilePath = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);
            var originalFilePath = _directoryService.GetUnmodifiedDocumentPath(targetDirectory, subDirectory, document);

            if (downloadFilePath != null)
            {
                try
                {
                    var bytes = _directoryService.GetDocumentBytes(targetDirectory, subDirectory, document);

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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// The page count.
        /// </returns>
        [HttpGet]
        [Route("{targetDirectory}/{document}/page-count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult GetPageCount(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                { 
                    var pageCount = _pdfService.GetPageCount(path);

                    return Ok(pageCount);
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Count", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// The page preview.
        /// </returns>
        [HttpGet]
        [Route("{targetDirectory}/{document}/preview/{pageNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult GetPagePreview(TargetDirectory targetDirectory, string document, int pageNumber, int width, int height, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{targetDirectory}/{document}/rename/{newDocumentName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult Rename(TargetDirectory targetDirectory, string document, string newDocumentName, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                { 
                    _directoryService.Rename(targetDirectory, subDirectory, document, newDocumentName);

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
        /// Rotates pages in the specified document clockwise.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers (1-based).</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{targetDirectory}/{document}/rotate-pages-clockwise")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult RotatePagesClockwise(TargetDirectory targetDirectory, string document, [FromBody] List<int> pageNumbers, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                { 
                    _pdfManipulationService.RotateClockwise(targetDirectory, document, pageNumbers);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("RotateClockwise", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Rotate pages in the specified document anti clockwise.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pageNumbers">The page numbers (1-based).</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{targetDirectory}/{document}/rotate-pages-anti-clockwise")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult RotatePagesAntiClockwise(TargetDirectory targetDirectory, string document, [FromBody] List<int> pageNumbers, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                {
                    _pdfManipulationService.RotateAntiClockwise(targetDirectory, document, pageNumbers);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("RotateAntiClockwise", HttpContext, x);
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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{targetDirectory}/{document}/delete-pages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult DeletePages(TargetDirectory targetDirectory, string document, [FromBody] List<int> pageNumbers, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{targetDirectory}/{document}/reorder-pages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult ReorderPages(TargetDirectory targetDirectory, string document, [FromBody] List<int> newPageOrder, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{targetDirectory}/{document}/revert")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult RevertChanges(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{targetDirectory}/{document}/delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult Delete(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                { 
                    _directoryService.Delete(targetDirectory, subDirectory, document);

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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{targetDirectory}/{document}/unlock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult Unlock(TargetDirectory targetDirectory, string document, string password, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                { 
                    _pdfManipulationService.Unlock(targetDirectory, document, password);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Unlock", HttpContext, x);
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
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult Restore(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                {
                    _directoryService.Restore(targetDirectory, subDirectory, document);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Restore", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Saves the specified document in the input directory.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="sourceSubDirectory">The subDirectory to move frp,.</param>
        /// <param name="targetSubDirectory">The subDirectory to save to.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/saveto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult SaveTo(string document, string? sourceSubDirectory = null, string? targetSubDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(TargetDirectory.Input, sourceSubDirectory, document);

            if (path != null)
            {
                try
                {
                    _directoryService.Save(document, targetSubDirectory);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("SaveTo", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Saves the specified document in the input directory.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("{document}/save")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult Save(string document)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(TargetDirectory.Input, null, document);

            if (path != null)
            {
                try
                {
                    _directoryService.Save(document, null);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Save", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
}
