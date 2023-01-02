using PDFWebEdit.Enumerations;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models;

namespace PDFWebEdit.Services
{
    /// <summary>
    /// A directory service.
    /// </summary>
    public class DirectoryService
    {
        /// <summary>
        /// The lock.
        /// </summary>
        private object __lock = new object();

        /// <summary>
        /// The PDF extension.
        /// </summary>
        private const string PDF_EXTENSION = ".pdf";

        /// <summary>
        /// The editing PDF extension.
        /// </summary>
        private const string EDITING_PDF_EXTENSION = ".edit.pdf";

        /// <summary>
        /// Pathname of the input directory.
        /// </summary>
        private readonly string _inputDirectory;

        /// <summary>
        /// Pathname of the trash directory.
        /// </summary>
        private readonly string _trashDirectory;

        /// <summary>
        /// Pathname of the output directory.
        /// </summary>
        private readonly string _outputDirectory;

        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly ConfigService _configService;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFWebEdit.Services.DirectoryService"/> class.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="configuration">The configuration.</param>
        /// <param name="configService"></param>
        public DirectoryService(IConfiguration configuration, ConfigService configService)
        {
            // Get directory paths
            _inputDirectory = configuration["Directories:Input"];
            _trashDirectory = configuration["Directories:Trash"];
            _outputDirectory = configuration["Directories:Output"];

            // Check the directories are writable
            DirectoryHelpers.CheckDirectory(_inputDirectory);
            DirectoryHelpers.CheckDirectory(_trashDirectory);
            DirectoryHelpers.CheckDirectory(_outputDirectory);

            // The config service
            _configService = configService;
        }

        /// <summary>
        /// Gets the document lists in this collection.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the document lists in this collection.
        /// </returns>
        public IEnumerable<Document> GetDocumentList(TargetDirectory targetDirectory = TargetDirectory.Input)
        {
            string directory;

            switch (targetDirectory)
            {
                default:
                case TargetDirectory.Input:
                    directory = _inputDirectory;
                    break;

                case TargetDirectory.Output:
                    directory = _outputDirectory;
                    break;

                case TargetDirectory.Trash:
                    directory = _trashDirectory;
                    break;
            }

            // Load docs
            var documents = Directory.EnumerateFiles(directory, $"*{PDF_EXTENSION}")
                .Where(x => !x.EndsWith(EDITING_PDF_EXTENSION, StringComparison.OrdinalIgnoreCase))
                .Select(x => new Document
                {
                    Name = Path.GetFileNameWithoutExtension(x),
                    Created = File.GetCreationTime(x),
                    LastModified = File.GetLastWriteTime(x)
                });

            return documents;
        }

        /// <summary>
        /// Gets document path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The document path.
        /// </returns>
        public string? GetDocumentPath(string name)
        {
            var editedPdfPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
            var pdfPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

            if (File.Exists(editedPdfPath))
            {
                return editedPdfPath;
            }
            else if (File.Exists(pdfPath))
            {
                return pdfPath;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets unmodified document path.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The unmodified document path.
        /// </returns>
        public string GetUnmodifiedDocumentPath(string name)
        {
            var pdfPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

            if (File.Exists(pdfPath))
            {
                return pdfPath;
            }
            else
            {
                throw new Exception($"Source file does not exist: {pdfPath}");
            }
        }

        /// <summary>
        /// Gets editing document path.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The editing document path.
        /// </returns>
        public string GetEditingDocumentPath(string name)
        {
            var editedPdfPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
            var pdfPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

            // Check the original file exists before returning an editing path
            if (File.Exists(pdfPath))
            {
                return editedPdfPath;
            }
            else
            {
                throw new Exception($"Source file does not exist: {pdfPath}");
            }
        }

        /// <summary>
        /// Renames a file.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="name">The name.</param>
        /// <param name="newName">Name of the new.</param>
        public void Rename(string name, string newName)
        {
            lock (__lock)
            {
                var editedPDFPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

                if (newName.Any(x => Path.GetInvalidFileNameChars().Contains(x)))
                {
                    throw new Exception($"New file name contains invalid characters");
                }

                if (File.Exists(editedPDFPath))
                {
                    var newEditedPDFPath = Path.Combine(_inputDirectory, newName) + EDITING_PDF_EXTENSION;

                    if (File.Exists(newEditedPDFPath))
                    {
                        throw new Exception($"New file name already exists: {newEditedPDFPath}");
                    }

                    File.Move(editedPDFPath, newEditedPDFPath);
                }

                if (File.Exists(originalPDFPath))
                {
                    var newOriginalPDFPath = Path.Combine(_inputDirectory, newName) + PDF_EXTENSION;

                    if (File.Exists(newOriginalPDFPath))
                    {
                        throw new Exception($"New file name already exists: {newOriginalPDFPath}");
                    }

                    File.Move(originalPDFPath, newOriginalPDFPath);
                }
            }
        }

        /// <summary>
        /// Gets document bytes.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// An array of byte.
        /// </returns>
        public byte[] GetDocumentBytes(string name)
        {
            byte[] result;

            var editedPDFPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
            var originalPDFPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

            if (File.Exists(editedPDFPath))
            {
                result = File.ReadAllBytes(editedPDFPath);
            }
            else  if (File.Exists(originalPDFPath))
            {
                result = File.ReadAllBytes(originalPDFPath);
            }
            else
            {
                throw new Exception($"Source file does not exist: {originalPDFPath}");
            }

            return result;
        }

        /// <summary>
        /// Deletes the document and any modifications.
        /// </summary>
        /// <param name="name">The name.</param>
        public void Delete(string name)
        {
            lock (__lock)
            {
                var editedPDFPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

                var editedPDFTrashPath = Path.Combine(_trashDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFTrashPath = Path.Combine(_trashDirectory, name) + PDF_EXTENSION;

                if (File.Exists(editedPDFPath))
                {
                    if (_configService.Settings.MoveFilesToTrashOnDelete)
                    {
                        File.Move(editedPDFPath, editedPDFTrashPath);
                    }
                    else
                    {
                        File.Delete(editedPDFPath);
                    }
                }

                if (File.Exists(originalPDFPath))
                {
                    if (_configService.Settings.MoveFilesToTrashOnDelete)
                    {
                        File.Move(originalPDFPath, originalPDFTrashPath);
                    }
                    else
                    {
                        File.Delete(originalPDFPath);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the document to the output directory.
        /// </summary>
        /// <param name="name">The name.</param>
        public void Save(string name)
        {
            lock (__lock)
            {
                var editedPDFPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

                var outputPath = Path.Combine(_outputDirectory, name) + PDF_EXTENSION;
                var trashOutputPath = Path.Combine(_trashDirectory, name) + PDF_EXTENSION;

                if (File.Exists(editedPDFPath))
                {
                    File.Move(editedPDFPath, outputPath);

                    if (_configService.Settings.DeleteOriginalFileOnSave)
                    {
                        File.Move(originalPDFPath, trashOutputPath);
                    }
                }
                else if (File.Exists(originalPDFPath))
                {
                    File.Move(originalPDFPath, outputPath);
                }
            }
        }
    }
}
