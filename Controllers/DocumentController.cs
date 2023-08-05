using Microsoft.AspNetCore.Mvc;
using PDFWebEdit.Enumerations;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models;
using PDFWebEdit.Models.Api;
using PDFWebEdit.Models.Requests.Batch;
using PDFWebEdit.Models.Requests.Objects;
using PDFWebEdit.Services;
using System.IO.Compression;

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
        /// Gets directory documents.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// The directory documents.
        /// </returns>
        [HttpGet]
        [Route("directories/documents")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Document>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult GetDirectoryDocuments(TargetDirectory targetDirectory, string? subDirectory = null)
        {
            try
            {
                return Ok(_directoryService.GetDocumentList(targetDirectory, subDirectory: subDirectory, includeSubdirectories: false));
            }
            catch (Exception x)
            {
                return ExceptionHelpers.GetErrorObjectResult("Directories", HttpContext, x);
            }
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="name">The name.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// The new directory.
        /// </returns>
        [HttpPost]
        [Route("directories")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Folder))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult CreateDirectory(TargetDirectory targetDirectory, string name, string? subDirectory = null)
        {
            try
            {
                return Ok(_directoryService.CreateFolder(targetDirectory, name, subDirectory: subDirectory));
            }
            catch (Exception x)
            {
                return ExceptionHelpers.GetErrorObjectResult("CreateDirectory", HttpContext, x);
            }
        }

        /// <summary>
        /// Gets the documents in the selected folder.
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
                return Ok(_directoryService.GetDocumentList(targetDirectory, includeSubdirectories: true));
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
        [Route("download/{targetDirectory}/{document}")]
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
        [Route("page-count/{targetDirectory}/{document}")]
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
        [Route("preview/{targetDirectory}/{document}/{pageNumber}")]
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
        [Route("rename/{targetDirectory}/{document}/{newDocumentName}")]
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
        [Route("rotate-pages-clockwise/{targetDirectory}/{document}")]
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
                    _pdfManipulationService.RotateClockwise(targetDirectory, document, pageNumbers, subDirectory);

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
        [Route("rotate-pages-anti-clockwise{targetDirectory}/{document}")]
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
                    _pdfManipulationService.RotateAntiClockwise(targetDirectory, document, pageNumbers, subDirectory);

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
        [Route("delete-pages/{targetDirectory}/{document}")]
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
                    _pdfManipulationService.DeletePage(targetDirectory, document, pageNumbers, subDirectory);

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
        [Route("reorder-pages/{targetDirectory}/{document}")]
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
                    _pdfManipulationService.ReorderPages(targetDirectory, document, newPageOrder, subDirectory);

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
        /// Reverse pages order.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("reverse-pages-order/{targetDirectory}/{document}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult ReversePagesOrder(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                {
                    _pdfManipulationService.ReversePagesOrder(targetDirectory, document, subDirectory);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Reverse", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Splits the pages into a new document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="pages">The pages.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("split-pages/{targetDirectory}/{document}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Document))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult SplitPages(TargetDirectory targetDirectory, string document, [FromBody] List<int> pages, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                {
                    var newDocumentName = _pdfManipulationService.SplitPages(targetDirectory, document, pages, subDirectory);

                    return Ok(_directoryService.GetDocument(targetDirectory, subDirectory, newDocumentName));
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Split", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Merges another document into the defined document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="mergeDocument">The merge document.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <param name="mergeDocumentSubDirectory">Pathname of the merge document sub directory.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("merge/{targetDirectory}/{document}/{mergeDocument}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Document))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult Merge(TargetDirectory targetDirectory, string document, string mergeDocument, string? subDirectory = null, string? mergeDocumentSubDirectory = null)
        {
            // Get the path to the document
            var sourcePath = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);
            var targetPath = _directoryService.GetDocumentPath(targetDirectory, mergeDocumentSubDirectory, mergeDocument);

            if ((sourcePath != null) && (targetPath != null))
            {
                try
                {
                    _pdfManipulationService.MergeDocument(targetDirectory, document, mergeDocument, subDirectory, mergeDocumentSubDirectory);

                    return Ok(_directoryService.GetDocument(targetDirectory, subDirectory, document));
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Merge", HttpContext, x);
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
        [Route("revert/{targetDirectory}/{document}")]
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
                    _pdfManipulationService.RevertChanges(targetDirectory, document, subDirectory);

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
        /// Archives the specified document.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("archive/{targetDirectory}/{document}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult Archive(TargetDirectory targetDirectory, string document, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(targetDirectory, subDirectory, document);

            if (path != null)
            {
                try
                {
                    _directoryService.Archive(targetDirectory, subDirectory, document);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("Archive", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Permentently deletes the specified document from the archive.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("delete/{document}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult DeleteFromArchive(string document, string? subDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(TargetDirectory.Archive, subDirectory, document);

            if (path != null)
            {
                try
                {
                    _directoryService.PermenentlyDeleteFromArchive(document, subDirectory);

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
        [Route("unlock/{targetDirectory}/{document}")]
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
                    _pdfManipulationService.Unlock(targetDirectory, document, password, subDirectory);

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
        /// Restores a document from the target directory to the inbox.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="document">The document.</param>
        /// <param name="subDirectory">The sub directory containing the document.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("restore/{document}")]
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
        /// Saves the specified document from the inbox directory.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="sourceSubDirectory">The subDirectory to move from.</param>
        /// <param name="targetSubDirectory">The subDirectory to save to.</param>
        /// <param name="newName">New name of the file</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("save-as/{document}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult SaveAs(string document, string? sourceSubDirectory = null, string? targetSubDirectory = null, string newName = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(TargetDirectory.Inbox, sourceSubDirectory, document);

            if (path != null)
            {
                try
                {
                    _directoryService.Save(document, sourceSubDirectory, targetSubDirectory, newName);

                    return Ok();
                }
                catch (Exception x)
                {
                    return ExceptionHelpers.GetErrorObjectResult("SaveAs", HttpContext, x);
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Saves the specified document from the inbox directory.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="sourceSubDirectory">The subDirectory to move from.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("save/{document}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult Save(string document, string? sourceSubDirectory = null)
        {
            // Get the path to the document
            var path = _directoryService.GetDocumentPath(TargetDirectory.Inbox, sourceSubDirectory, document);
            
            if (path != null)
            {
                try
                {
                    _directoryService.Save(document, sourceSubDirectory, null);

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

        #region Batch

        /// <summary>
        /// Saves a batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("batch/save")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status207MultiStatus, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult SaveBatch([FromBody] List<Save> batch)
        {
            var batchResult = new BatchResponse<DocumentResult>();

            // Call each batch item
            foreach (var item in batch)
            {
                // Call the existing API
                var result = Save(item.Document, item.SourceSubDirectory);

                // Get the status code
                var statusCode = result.GetStatusCode();

                // Get error
                string? error = (statusCode == StatusCodes.Status500InternalServerError) ? result.GetErrorMessage() : null;

                // And add the result to the batch
                batchResult.Add(new DocumentResult
                {
                    Document = item.Document,
                    StatusCode = statusCode,
                    AdditionalInformation = error
                });
            }

            // Send the result of all batch items
            return StatusCode(batchResult.GetStatusCode(), batchResult);
        }

        /// <summary>
        /// Saves a batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("batch/save-as")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status207MultiStatus, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult SaveAsBatch([FromBody] List<SaveAs> batch)
        {
            var batchResult = new BatchResponse<DocumentResult>();

            // Call each batch item
            foreach (var item in batch)
            {
                // Call the existing API
                var result = SaveAs(item.Document, item.SourceSubDirectory, item.TargetSubDirectory, item.NewName);

                // Get the status code
                var statusCode = result.GetStatusCode();

                // Get error
                string? error = (statusCode == StatusCodes.Status500InternalServerError) ? result.GetErrorMessage() : null;

                // And add the result to the batch
                batchResult.Add(new DocumentResult
                {
                    Document = item.Document,
                    StatusCode = statusCode,
                    AdditionalInformation = error
                });
            }

            // Send the result of all batch items
            return StatusCode(batchResult.GetStatusCode(), batchResult);
        }

        /// <summary>
        /// Archive batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("batch/archive")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status207MultiStatus, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult ArchiveBatch([FromBody] List<Archive> batch)
        {
            var batchResult = new BatchResponse<DocumentResult>();

            // Call each batch item
            foreach (var item in batch)
            {
                // Call the existing API
                var result = Archive(item.TargetDirectory, item.Document, item.SubDirectory);

                // Get the status code
                var statusCode = result.GetStatusCode();

                // Get error
                string? error = (statusCode == StatusCodes.Status500InternalServerError) ? result.GetErrorMessage() : null;

                // And add the result to the batch
                batchResult.Add(new DocumentResult
                {
                    Document = item.Document,
                    StatusCode = statusCode,
                    AdditionalInformation = error
                });
            }

            // Send the result of all batch items
            return StatusCode(batchResult.GetStatusCode(), batchResult);
        }

        /// <summary>
        /// Deletes from archive by batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("batch/delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status207MultiStatus, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult DeleteFromArchiveBatch([FromBody] List<Delete> batch)
        {
            var batchResult = new BatchResponse<DocumentResult>();

            // Call each batch item
            foreach (var item in batch)
            {
                // Call the existing API
                var result = DeleteFromArchive(item.Document, item.SubDirectory);

                // Get the status code
                var statusCode = result.GetStatusCode();

                // Get error
                string? error = (statusCode == StatusCodes.Status500InternalServerError) ? result.GetErrorMessage() : null;

                // And add the result to the batch
                batchResult.Add(new DocumentResult
                {
                    Document = item.Document,
                    StatusCode = statusCode,
                    AdditionalInformation = error
                });
            }

            // Send the result of all batch items
            return StatusCode(batchResult.GetStatusCode(), batchResult);
        }

        /// <summary>
        /// Downloads the document batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpGet]
        [Route("batch/download")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status207MultiStatus, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult DownloadDocumentBatch([FromQuery] List<Download> batch)
        {
            var batchResult = new BatchResponse<DocumentResult>();

            using (var ms = new MemoryStream())
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                // Call each batch item
                foreach (var item in batch)
                {
                    // Call the existing API
                    var result = DownloadDocument(item.TargetDirectory, item.Document, item.SubDirectory);

                    // Get error
                    string? error = result.GetErrorMessage();

                    // And add the file to the archive
                    if (result is FileContentResult)
                    {
                        var fileResult = (FileContentResult)result;

                        var zipArchiveEntry = archive.CreateEntry(fileResult.FileDownloadName, CompressionLevel.Fastest);
                        using (var zipStream = zipArchiveEntry.Open())
                        {
                            zipStream.Write(fileResult.FileContents, 0, fileResult.FileContents.Length);
                        }

                        // And add the result to the batch
                        batchResult.Add(new DocumentResult
                        {
                            Document = item.Document,
                            StatusCode = StatusCodes.Status200OK,
                            AdditionalInformation = error
                        });
                    }
                    else if (result is IActionResult)
                    {
                        // Get the status code
                        var statusCode = result.GetStatusCode();

                        // And add the result to the batch
                        batchResult.Add(new DocumentResult
                        {
                            Document = item.Document,
                            StatusCode = statusCode,
                            AdditionalInformation = error
                        });
                    }
                    else
                    {
                        throw new Exception("Unknown error");
                    }
                }

                // Dispose of the archive to flush the the stream to the memory stream
                archive.Dispose();

                if (batchResult.GetStatusCode() == StatusCodes.Status200OK)
                {
                    // Send the file back
                    return File(ms.ToArray(), "application/zip", "documents.zip");
                }
                else
                {
                    // Send the result of all batch items
                    return StatusCode(batchResult.GetStatusCode(), batchResult);
                }
            }
        }

        /// <summary>
        /// Restore batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns>
        /// An IActionResult.
        /// </returns>
        [HttpPost]
        [Route("batch/restore")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status207MultiStatus, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BatchResponse<DocumentResult>))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult RestoreBatch([FromBody] List<Restore> batch)
        {
            var batchResult = new BatchResponse<DocumentResult>();

            // Call each batch item
            foreach (var item in batch)
            {
                // Call the existing API
                var result = Restore(item.TargetDirectory, item.Document, item.SubDirectory);

                // Get the status code
                var statusCode = result.GetStatusCode();

                // Get error
                string? error = (statusCode == StatusCodes.Status500InternalServerError) ? result.GetErrorMessage() : null;

                // And add the result to the batch
                batchResult.Add(new DocumentResult
                {
                    Document = item.Document,
                    StatusCode = statusCode,
                    AdditionalInformation = error
                });
            }

            // Send the result of all batch items
            return StatusCode(batchResult.GetStatusCode(), batchResult);
        }

        #endregion
    }
}
